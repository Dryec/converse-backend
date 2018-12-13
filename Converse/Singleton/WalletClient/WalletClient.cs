using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Converse.Service;
using Converse.Singleton.WalletClient;
using Google.Protobuf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Protocol;

namespace Converse.Singleton.WalletClient
{
	public class WalletClient
	{
		public static Client.Wallet PropertyAddress { get; set; }

		private readonly IServiceProvider _serviceProvider;
		private readonly IApplicationLifetime _appLifeTime;

		private readonly Configuration.Block _blockConfiguration;

		private readonly Client.WalletClient _walletClient;
		private readonly Logger _logger;
		private readonly Token _token;
		private readonly ActionHandler _actionHandler;

		private readonly Thread _synchronizeBlocksThread;
		private bool _isThreadRunning;

		public WalletClient(IServiceProvider serviceProvider, IOptions<Configuration.Node> nodeOptions, IOptions<Configuration.Token> tokenOptions, IOptions<Configuration.Block> blockOptions)
		{
			var nodeConfiguration = nodeOptions.Value;
			var tokenConfiguration = tokenOptions.Value;
			_blockConfiguration = blockOptions.Value;

			_serviceProvider = serviceProvider;
			_appLifeTime = serviceProvider.GetService<IApplicationLifetime>();
			
			_walletClient = new Client.WalletClient(nodeConfiguration.Ip + ":" + nodeConfiguration.Port);
			_logger = new Logger(serviceProvider.GetService<ILoggerFactory>().CreateLogger("WalletClient"));
			_token = new Token(tokenConfiguration.Name, _logger, _walletClient);

			_actionHandler = new ActionHandler(_logger, _token);

			if (_blockConfiguration.SyncCount <= 0)
			{
				_logger.Log.LogWarning(Logger.SynchronizationDisabled, "The synchronization is disabled, because 'syncCount' in 'blockchain.json' is zero!");
				return;
			}

			_synchronizeBlocksThread = new Thread(SynchronizeBlocks);
		}

		public void Start()
		{
			if (!_synchronizeBlocksThread.IsAlive)
			{
				_isThreadRunning = true;
				_synchronizeBlocksThread.Start();
			}
		}

		public void Stop()
		{
			_isThreadRunning = false;
			if (_synchronizeBlocksThread.IsAlive)
			{
				_synchronizeBlocksThread.Join(_blockConfiguration.SyncSleepTime + 5000);
			}
		}

		public async Task<Account> GetAddressInformation(string address)
		{
			return await _walletClient.GetAccountAsync(new Account()
			{
				Address = ByteString.CopyFrom(Client.WalletAddress.Decode58Check(address))
			});
		}

		public struct TransferTokenResult
		{
			public bool Result { get; set; }
			public Return.Types.response_code Code { get; set; }
			public string Message { get; set; }
			public Transaction Transaction { get; set; }
		}

		public async Task<TransferTokenResult> TransferTokenFromProperty(int amount, string receiver)
		{
			var transferTokenResult = new TransferTokenResult()
			{
				Result = false
			};

			if (receiver.Equals(PropertyAddress.Address, StringComparison.CurrentCultureIgnoreCase))
			{
				return transferTokenResult;
			}

			var contract = new Protocol.TransferAssetContract()
			{
				Amount = amount,
				AssetName = ByteString.CopyFromUtf8(_token.ToString()),
				OwnerAddress = ByteString.CopyFrom(Client.WalletAddress.Decode58Check(PropertyAddress.Address)),
				ToAddress = ByteString.CopyFrom(Client.WalletAddress.Decode58Check(receiver)),
			};
			var transaction = await _walletClient.TransferAssetAsync(contract);
			if (transaction != null)
			{
				PropertyAddress.SignTransaction(transaction.Transaction);
				var broadcastResult = await _walletClient.BroadcastTransactionAsync(transaction.Transaction);
				if (broadcastResult.Result)
				{
					transferTokenResult.Transaction = transaction.Transaction;
				}

				transferTokenResult.Result = broadcastResult.Result;
				transferTokenResult.Code = broadcastResult.Code;
				transferTokenResult.Message = broadcastResult.Message.ToString(Encoding.ASCII);
			}

			return transferTokenResult;
		}

		private async void SynchronizeBlocks()
		{
			var hasUpdatedToken = await _token.Update();
			if (!hasUpdatedToken)
			{
				_appLifeTime.StopApplication();
				return;
			}

			var synchronizationCount = _blockConfiguration.SyncCount;

			DatabaseContext databaseContext;
			Models.Setting lastSyncedBlockModel;

			void UpdateDbContext()
			{
				// Get new database context
				var scope = _serviceProvider.CreateScope();
				databaseContext = scope.ServiceProvider.GetService<DatabaseContext>();
				_actionHandler.DatabaseContext = databaseContext;

				// Get last synced block
				lastSyncedBlockModel = databaseContext.GetLastSyncedBlock();
				if (lastSyncedBlockModel == null)
				{
					_isThreadRunning = false;
					_appLifeTime.StopApplication();
					_logger.Log.LogCritical(Logger.LastSyncedBlockNotFound,
						"Could not find 'LastSyncedBlockId' in 'Settings' Table! Make sure to migrate the migrations!");
					return;
				}

				if (Convert.ToUInt64(lastSyncedBlockModel.Value) < _blockConfiguration.StartId)
				{
					lastSyncedBlockModel.Value = (_blockConfiguration.StartId - 1).ToString();
				}
			}
			UpdateDbContext();

			_appLifeTime.ApplicationStopping.Register(Stop);

			while (_isThreadRunning) {
				try
				{

				

					// Convert last synced block to integer
					var lastSyncedBlock = Convert.ToInt64(lastSyncedBlockModel.Value);

					// Read nextBlock til (nextBlock + synchronizationCount)
					var unsortedBlocks = await _walletClient.GetBlockByLimitNextAsync(new BlockLimit()
					{
						StartNum = lastSyncedBlock + 1,
						EndNum = lastSyncedBlock + 1 + synchronizationCount,
					});

					// Retrieved any blocks?
					if (unsortedBlocks.Block.Count > 0)
					{
						// @ToDo: Look if better solution then OrderBy
						var blocks = (unsortedBlocks.Block.Count > 1
							? unsortedBlocks.Block.OrderBy(block => block.BlockHeader.RawData.Number).ToList()
							: unsortedBlocks.Block.ToList());

						using (var transactionScope = databaseContext.Database.BeginTransaction())
						{
							try
							{
								foreach (var block in blocks)
								{
									foreach (var transaction in block.Transactions)
									{
										_actionHandler.Handle(transaction, block);
									}

									lastSyncedBlock = block.BlockHeader.RawData.Number;
								}

								lastSyncedBlockModel.Value = lastSyncedBlock.ToString();

								databaseContext.SaveChanges();
								transactionScope.Commit();
							}
							catch (DbUpdateException e)
							{
								_logger.Log.LogCritical(Logger.CannotSaveChanges,
									"Could not save changes to database! Error: ");
								_logger.Log.LogCritical(Logger.CannotSaveChanges, e.Message);
								_logger.Log.LogCritical(Logger.CannotSaveChanges, e.StackTrace);

								transactionScope.Rollback();

								UpdateDbContext();
							}
						}
					}

					// When could retrieve 0 blocks or less than tried to sync, decrease the counter, but synchronize at least 3 blocks
					// Reasons: ResourceExhausted or already Up2Date
					if (unsortedBlocks.Block.Count == 0 && synchronizationCount > 3)
					{
						synchronizationCount--;
					}
				}
				catch (Grpc.Core.RpcException)
				{
					if (synchronizationCount > 3)
					{
						synchronizationCount--;
					}
				}

				Thread.Sleep(_blockConfiguration.SyncSleepTime);
			}
		}
	}
}