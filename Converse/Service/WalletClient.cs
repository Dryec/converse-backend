using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
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
			Thread syncBlocksThread = new Thread(SyncBlocksToDatabase);

			_serviceProvider = serviceProvider;
			_nodeConfiguration = nodeOptions.Value;
			_walletClient = new Client.WalletClient(_nodeConfiguration.Ip + ":" + _nodeConfiguration.Port);

			syncBlocksThread.Start();
		}


		async void SyncBlocksToDatabase()
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				DatabaseContext databaseContext = scope.ServiceProvider.GetService<DatabaseContext>();
				Models.Setting lastSyncedBlockId = databaseContext.Settings.First(s => s.Key == "LastSyncedBlockId");

				if (Convert.ToUInt64(lastSyncedBlockId.Value) < _nodeConfiguration.StartBlockId)
				{
					lastSyncedBlockId.Value = (_nodeConfiguration.StartBlockId - 1).ToString();
				}

				while (true) {
					long lastSavedBlockId = Convert.ToInt64(lastSyncedBlockId.Value);

					var lastBlockId = (await _walletClient.GetNowBlockAsync()).BlockHeader.RawData.Number;
					if (lastBlockId != lastSavedBlockId)
					{
						BlockListExtention blocks = await _walletClient.GetBlockByLimitNextAsync(new BlockLimit()
						{
							StartNum = lastSavedBlockId + 1,
							EndNum = lastBlockId
						});

						lastSyncedBlockId.Value = lastBlockId.ToString();

						databaseContext.SaveChanges();
					}

					Thread.Sleep(_nodeConfiguration.BlockSyncTime);
				}
			}
		}
	}
}