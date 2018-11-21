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
using Node = Converse.Configuration.Node;

namespace Converse.Service
{
	public class WalletClient
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly Client.WalletClient _walletClient;
		private readonly Node _nodeConfiguration;

		public WalletClient(IServiceProvider serviceProvider, IOptions<Node> nodeOptions)
		{
			var syncBlocksThread = new Thread(SyncBlocksToDatabase);

			_serviceProvider = serviceProvider;
			_nodeConfiguration = nodeOptions.Value;
			_walletClient = new Client.WalletClient(_nodeConfiguration.Ip + ":" + _nodeConfiguration.Port);

			syncBlocksThread.Start();
		}


		async void SyncBlocksToDatabase()
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var databaseContext = scope.ServiceProvider.GetService<DatabaseContext>();
				var dbLastSyncedBlock = databaseContext.Settings.First(s => s.Key == "LastSyncedBlockId");

				if (Convert.ToUInt64(dbLastSyncedBlock.Value) < _nodeConfiguration.StartBlockId)
				{
					dbLastSyncedBlock.Value = (_nodeConfiguration.StartBlockId - 1).ToString();
				}

				var token = await _walletClient.GetAssetIssueByNameAsync(new BytesMessage()
				{
					Value = ByteString.CopyFrom(_nodeConfiguration.TokenName, Encoding.ASCII)
				});

				if (token.OwnerAddress.Length == 0)
				{
					// @ToDo: Error log - token not found
					Environment.Exit(0);
					return;
				}

				while (true) {
					var lastSavedSyncedBlock = Convert.ToInt64(dbLastSyncedBlock.Value);

					var blocks = await _walletClient.GetBlockByLimitNextAsync(new BlockLimit()
					{
						StartNum = lastSavedSyncedBlock + 1,
						EndNum = lastSavedSyncedBlock + 3,
					});

					if (blocks.Block.Count > 0)
					{
						var chatMessages = new List<ChatMessage>();
						var lastSyncedId = lastSavedSyncedBlock;

						foreach (var block in blocks.Block)
						{
							foreach (var transaction in block.Transactions)
							{
								if (transaction.Transaction.RawData.Contract.Count <= 0)
								{
									continue;
								}

								var contract = transaction.Transaction.RawData.Contract[0];
								if (contract.Type != Transaction.Types.Contract.Types.ContractType.TransferAssetContract)
								{
									continue;
								}

								var transferAssetContract = contract.Parameter.Unpack<TransferAssetContract>();
								if (transferAssetContract.AssetName != token.Name)
								{
									continue;
								}

								var x1 = transferAssetContract.OwnerAddress.ToByteArray();
								var x2 = transferAssetContract.ToAddress.ToByteArray();

								var message = transaction.Transaction.RawData.Data.ToStringUtf8();
								var transactionHash = Common.Utils
									.ToHexString(Crypto.Sha256.Hash(transaction.Transaction.RawData.ToByteArray()))
									.ToLower();
								var chatMessage = new ChatMessage()
								{
									ChatId = 0,

									Address = "",
									Message = message,

									BlockId = block.BlockHeader.RawData.Number,
									BlockCreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(block.BlockHeader.RawData.Timestamp).DateTime,

									TransactionHash = transactionHash,
									TransactionCreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(block.BlockHeader.RawData.Timestamp).DateTime,

									CreatedAt = DateTime.Now,
								};
								chatMessages.Add(chatMessage);
							}

							if (lastSyncedId < block.BlockHeader.RawData.Number) { 
								lastSyncedId = block.BlockHeader.RawData.Number;
							}
						}

						try
						{
							dbLastSyncedBlock.Value = lastSyncedId.ToString();
							databaseContext.SaveChanges();
							lastSavedSyncedBlock = lastSyncedId;
						}
						catch (Exception e)
						{
							// @ToDo: Log Error - Couldn't save the changes
							Console.WriteLine(e.Message);
							Console.WriteLine(e.StackTrace);
							dbLastSyncedBlock.Value = lastSavedSyncedBlock.ToString();
							databaseContext = scope.ServiceProvider.GetService<DatabaseContext>();
						}
					}

					Thread.Sleep(_nodeConfiguration.BlockSyncTime);
				}
			}
		}
	}
}