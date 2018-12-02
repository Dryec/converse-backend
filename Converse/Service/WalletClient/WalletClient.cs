﻿using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Protocol;

namespace Converse.Service.WalletClient
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

		private async void SynchronizeBlocks()
		{
			var hasUpdatedToken = await _token.Update();
			if (!hasUpdatedToken)
			{
				_appLifeTime.StopApplication();
				return;
			}

			using (var scope = _serviceProvider.CreateScope())
			{
				DatabaseContext databaseContext;
				Models.Setting lastSyncedBlockModel;

				bool UpdateDatabaseContext()
				{
					databaseContext = scope.ServiceProvider.GetService<DatabaseContext>();
					lastSyncedBlockModel = databaseContext.GetLastSyncedBlock();
					if (lastSyncedBlockModel == null)
					{
						_appLifeTime.StopApplication();
						_logger.Log.LogCritical(Logger.LastSyncedBlockNotFound, "Could not find 'LastSyncedBlockId' in 'Settings' Table! Make sure to migrate the migrations!");
						return false;
					}

					_actionHandler.DatabaseContext = databaseContext;

					if (Convert.ToUInt64(lastSyncedBlockModel.Value) < _blockConfiguration.StartId)
					{
						lastSyncedBlockModel.Value = (_blockConfiguration.StartId - 1).ToString();
					}

					return true;
				}

				if (!UpdateDatabaseContext()) return;

				var synchronizationCount = _blockConfiguration.SyncCount;

				while (_isThreadRunning) {
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

						foreach (var block in blocks)
						{
							foreach (var transaction in block.Transactions)
							{
								_actionHandler.Handle(transaction, block);
							}

							if (lastSyncedBlock < block.BlockHeader.RawData.Number) {
								lastSyncedBlock = block.BlockHeader.RawData.Number;
							}
						}

						try
						{
							lastSyncedBlockModel.Value = lastSyncedBlock.ToString();
							databaseContext.SaveChanges();
						}
						catch (Exception e)
						{
							_logger.Log.LogCritical(Logger.CannotSaveChanges, "Could not save changes to database! Error: ");
							_logger.Log.LogCritical(Logger.CannotSaveChanges, e.Message);
							_logger.Log.LogCritical(Logger.CannotSaveChanges, e.StackTrace);

							if (!UpdateDatabaseContext()) return;
						}
					}

					// When could retrieve 0 blocks or less than tried to sync, decrease the counter, but synchronize at least 3 blocks
					// Reasons: ResourceExhausted or already Up2Date
					if ((unsortedBlocks.Block.Count == 0 || unsortedBlocks.Block.Count < synchronizationCount) && synchronizationCount > 3)
					{
						synchronizationCount--;
					}

					Thread.Sleep(_blockConfiguration.SyncSleepTime);
				}
			}
		}
	}
}