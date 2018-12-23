using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Converse.Database;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Converse.Utils;

namespace Converse.Singleton.WalletClient.ActionHandlers
{
	public static class GroupCreate
	{
		private static async void TransferToGroup(IServiceProvider serviceProvider, string address)
		{
			var walletClient = serviceProvider.GetService<WalletClient>();
			if (walletClient == null) return;

			var retryCounter = 0;

			while (retryCounter < 3)
			{
				var result = await walletClient.TransferTokenFromProperty(1, address);
				if (!result.Result)
				{
					retryCounter++;
				}
			}
		}

		public static void Handle(Action.Context context)
		{
			var createGroupMessage = JsonConvert.DeserializeObject<Action.Group.Create>(context.Message);

			context.Logger.Log.LogDebug(Logger.HandleGroupCreate, "CreateGroup: Sender '{Address}' GroupOwnerAddress '{OwnerAddress}' PrivateKey '{PrivateKey}'!",
				context.Sender, createGroupMessage.Address);

			var groupAddress = createGroupMessage.Address.DecryptByTransaction(context.Transaction)?.ToUtf8String();
			if (groupAddress == null)
			{
				context.Logger.Log.LogDebug("Invalid Base64 Format!");
				return;
			}

			if (Client.WalletAddress.Decode58Check(groupAddress) == null)
			{
				context.Logger.Log.LogDebug("Invalid Address!");
				return;
			}

			var senderUser = context.DatabaseContext.GetUser(context.Sender, users => users.Include(u => u.DeviceIds)).GetAwaiter().GetResult();
			if (senderUser == null)
			{
				context.Logger.Log.LogDebug("User not found!");
				return;
			}

			if (context.DatabaseContext.ChatSettings.SingleOrDefault(cs => cs.Address == groupAddress) != null)
			{
				context.Logger.Log.LogDebug("Address already in use as a group!");
				return;
			}

			TransferToGroup(context.ServiceProvider, groupAddress);

			var chat = context.DatabaseContext.CreateGroupChat(senderUser, createGroupMessage.PrivateKey,
				new DatabaseContext.ChatGroupInfo()
				{
					Address = groupAddress,
					Name = createGroupMessage.Name,
					Description = createGroupMessage.Description,
					Image = createGroupMessage.Image,
					IsPublic = createGroupMessage.IsPublic,
				}, context.Transaction.RawData.Timestamp);

			context.DatabaseContext.SaveChanges();

			context.ServiceProvider.GetService<FCMClient>()?.GroupCreated(senderUser.DeviceIds, chat);
		}
	}
}
