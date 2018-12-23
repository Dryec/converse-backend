using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
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
			// Deserialize message
			var createGroupMessage = JsonConvert.DeserializeObject<Action.Group.Create>(context.Message);

			context.Logger.Log.LogDebug(Logger.HandleGroupCreate, "CreateGroup: Sender '{Address}' GroupOwnerAddress '{OwnerAddress}'!",
				context.Sender, createGroupMessage.Address);

			// Get public key
			var publicKey = context.Transaction.GetPublicKey();
			if (publicKey == null)
			{
				context.Logger.Log.LogDebug("Invalid PublicKey!");
				return;
			}

			// Decrypt group address
			var groupAddress = createGroupMessage.Address.DecryptByPublicKey(publicKey)?.ToUtf8String();
			if (groupAddress == null)
			{
				context.Logger.Log.LogDebug(Logger.InvalidBase64Format, "Invalid Base64 Format!");
				return;
			}

			// Decrypt group public key
			var groupPublicKey = createGroupMessage.PublicKey.DecryptByPublicKey(publicKey)?.EncodeBase64();
			if (groupPublicKey == null)
			{
				context.Logger.Log.LogDebug(Logger.InvalidBase64Format, "Invalid Base64 Format!");
				return;
			}

			// Decrypt group data
			var groupName = createGroupMessage.Name?.DecryptByPublicKey(publicKey)?.ToUtf8String();
			var groupDescription = createGroupMessage.Description?.DecryptByPublicKey(publicKey)?.ToUtf8String();
			var groupImage = createGroupMessage.Image?.DecryptByPublicKey(publicKey)?.ToUtf8String();
			if (groupName == null)
			{
				groupName = groupAddress;
			}


			// Check if address is valid
			if (Client.WalletAddress.Decode58Check(groupAddress) == null)
			{
				context.Logger.Log.LogDebug("Invalid Address!");
				return;
			}

			// Get user
			var senderUser = context.DatabaseContext.GetUserAsync(context.Sender, users => users.Include(u => u.DeviceIds)).GetAwaiter().GetResult();
			if (senderUser == null)
			{
				context.Logger.Log.LogDebug("User not found!");
				return;
			}

			// Check if address already exists
			if (context.DatabaseContext.ChatSettings.SingleOrDefault(cs => cs.Address == groupAddress) != null)
			{
				context.Logger.Log.LogDebug("Address already in use as a group!");
				return;
			}

			// Transfer a token to the group address to register it in tron
			TransferToGroup(context.ServiceProvider, groupAddress);

			// Create chat
			var chat = context.DatabaseContext.CreateGroupChat(senderUser, createGroupMessage.PrivateKey,
				new DatabaseContext.ChatGroupInfo()
				{
					Address = groupAddress,
					Name = groupName,
					Description = groupDescription,
					Image = groupImage,
					PublicKey = groupPublicKey,
					IsPublic = createGroupMessage.IsPublic,
				}, context.Transaction.RawData.Timestamp);

			context.DatabaseContext.SaveChanges();

			// Notify user that chat is created
			context.ServiceProvider.GetService<FCMClient>()?.GroupCreated(senderUser.DeviceIds, chat);
		}
	}
}
