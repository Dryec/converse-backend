using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Converse.Models;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Schema;
using Protocol;

namespace Converse.Service
{
	public class WalletClient
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly Client.WalletClient _walletClient;

		private readonly Configuration.Node _nodeConfiguration;
		private readonly Configuration.Token _tokenConfiguration;
		private readonly Configuration.Block _blockConfiguration;

		private AssetIssueContract _token = null;

		public WalletClient(IServiceProvider serviceProvider, IOptions<Configuration.Node> nodeOptions, IOptions<Configuration.Token> tokenOptions, IOptions<Configuration.Block> blockOptions)
		{
			var syncBlocksThread = new Thread(SyncBlocksToDatabase);

			_serviceProvider = serviceProvider;

			_nodeConfiguration = nodeOptions.Value;
			_tokenConfiguration = tokenOptions.Value;
			_blockConfiguration = blockOptions.Value;

			_walletClient = new Client.WalletClient(_nodeConfiguration.Ip + ":" + _nodeConfiguration.Port);

			if (_blockConfiguration.SyncCount <= 0)
			{
				ErrorOutput("BlockSync is disabled because syncCount in 'blockchain.json' is zero!", false);
				return;
			}

			syncBlocksThread.Start();
		}

		void ErrorOutput(string message, bool exit)
		{
			var oldColor = Console.ForegroundColor;

			Console.ForegroundColor = ConsoleColor.DarkRed;
			Console.WriteLine(message);

			Console.ForegroundColor = oldColor;

			if (exit)
			{
				Console.Write("Press enter to exit...");
				Console.ReadLine();
				Environment.Exit(0);
			}
		}

		async Task UpdateTokenData()
		{
			if (_token != null)
			{
				return;
			}

			_token = await _walletClient.GetAssetIssueByNameAsync(new BytesMessage()
			{
				Value = ByteString.CopyFrom(_tokenConfiguration.Name, Encoding.ASCII)
			});

			if (_token.OwnerAddress.Length == 0)
			{
				// @ToDo: Error log - Token not found
				Environment.Exit(0);
			}
		}

		async void SyncBlocksToDatabase()
		{
			await UpdateTokenData();

			using (var scope = _serviceProvider.CreateScope())
			{
				var databaseContext = scope.ServiceProvider.GetService<DatabaseContext>();
				var dbLastSyncedBlock = databaseContext.Settings.First(s => s.Key == "LastSyncedBlockId");

				if (Convert.ToUInt64(dbLastSyncedBlock.Value) < _blockConfiguration.StartId)
				{
					dbLastSyncedBlock.Value = (_blockConfiguration.StartId - 1).ToString();
				}

				var syncCount = _blockConfiguration.SyncCount;

				while (true) {
					var lastSavedSyncedBlock = Convert.ToInt64(dbLastSyncedBlock.Value);
					var blocks = await _walletClient.GetBlockByLimitNextAsync(new BlockLimit()
					{
						StartNum = lastSavedSyncedBlock + 1,
						EndNum = lastSavedSyncedBlock + 1 + syncCount,
					});

					if (blocks.Block.Count > 0)
					{
						var lastSyncedId = lastSavedSyncedBlock;

						foreach (var block in blocks.Block)
						{
							foreach (var transaction in block.Transactions)
							{
								this.ParseTransaction(transaction, block, databaseContext);
							}

							if (lastSyncedId < block.BlockHeader.RawData.Number) { 
								lastSyncedId = block.BlockHeader.RawData.Number;
							}
						}

						try
						{
							dbLastSyncedBlock.Value = lastSyncedId.ToString();
							databaseContext.SaveChanges();
						}
						catch (Exception)
						{
							// @ToDo: Log Error - Couldn't save the changes
							dbLastSyncedBlock.Value = lastSavedSyncedBlock.ToString();
							databaseContext = scope.ServiceProvider.GetService<DatabaseContext>();
						}
					}

					// try at least to sync 3 blocks
					if ((blocks.Block.Count == 0 || blocks.Block.Count < syncCount) && syncCount > 3)
					{
						// Decrease when: ResourcesExhausted, Already up2date sync
						syncCount--;
					}

					Thread.Sleep(_blockConfiguration.SyncSleepTime);
				}
			}
		}

		void ParseTransaction(TransactionExtention transaction, BlockExtention block, DatabaseContext databaseContext)
		{
			if (transaction.Transaction.RawData.Contract.Count <= 0)
			{
				return;
			}

			var contract = transaction.Transaction.RawData.Contract[0];
			if (contract.Type != Transaction.Types.Contract.Types.ContractType.TransferAssetContract)
			{
				return;
			}

			var transferAssetContract = contract.Parameter.Unpack<TransferAssetContract>();
			if (transferAssetContract.AssetName != _token.Name)
			{
				return;
			}

			var senderAddress = Client.WalletAddress.Encode58Check(transferAssetContract.OwnerAddress.ToByteArray());
			var receiverAddress = Client.WalletAddress.Encode58Check(transferAssetContract.ToAddress.ToByteArray());

			databaseContext.CreateUserWhenNotExist(senderAddress);
			databaseContext.CreateUserWhenNotExist(receiverAddress);

			var message = transaction.Transaction.RawData.Data.ToStringUtf8();
			var transactionHash = Common.Utils
				.ToHexString(Crypto.Sha256.Hash(transaction.Transaction.RawData.ToByteArray()))
				.ToLower();
		}
	}
}