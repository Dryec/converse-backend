﻿using System;
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

			if (_nodeConfiguration.BlockSyncCount <= 0)
			{
				// @ToDo: Warning log - No sync because syncCount is zero
				return;
			}

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
					// @ToDo: Error log - Token not found
					Environment.Exit(0);
					return;
				}

				var syncCount = _nodeConfiguration.BlockSyncCount;

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

								var senderAddress = Client.WalletAddress.Encode58Check(transferAssetContract.OwnerAddress.ToByteArray());
								var receiverAddress = Client.WalletAddress.Encode58Check(transferAssetContract.ToAddress.ToByteArray());

								databaseContext.CreateUserWhenNotExist(senderAddress);
								databaseContext.CreateUserWhenNotExist(receiverAddress);

								var message = transaction.Transaction.RawData.Data.ToStringUtf8();
								var transactionHash = Common.Utils
									.ToHexString(Crypto.Sha256.Hash(transaction.Transaction.RawData.ToByteArray()))
									.ToLower();

								// ToDO: Parse new data
								//var chat = databaseContext.GetChat(senderAddress, receiverAddress);
								//if (chat == null)
								//{
								//	chat = new Chat
								//	{
								//		CreatedAt = DateTime.Now
								//	};

								//	databaseContext.Chats.Add(chat);
								//}
								//var chatMessage = new ChatMessage()
								//{
								//	Chat = chat,

								//	Address = senderAddress,
								//	Message = message,

								//	BlockId = block.BlockHeader.RawData.Number,
								//	BlockCreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(block.BlockHeader.RawData.Timestamp).DateTime,

								//	TransactionHash = transactionHash,
								//	TransactionCreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(transaction.Transaction.RawData.Timestamp).DateTime,

								//	CreatedAt = DateTime.Now,
								//};
								//databaseContext.ChatMessages.Add(chatMessage);
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
						catch (Exception)
						{
							// @ToDo: Log Error - Couldn't save the changes
							dbLastSyncedBlock.Value = lastSavedSyncedBlock.ToString();
							databaseContext = scope.ServiceProvider.GetService<DatabaseContext>();
						}
					}

					if ((blocks.Block.Count == 0 || blocks.Block.Count < syncCount) && syncCount > 3)	// try at least to sync 3 blocks
					{
						// Decrease when: ResourcesExhausted, Already up2date sync
						syncCount--;
					}

					Thread.Sleep(_nodeConfiguration.BlockSyncSleepTime);
				}
			}
		}
	}
}