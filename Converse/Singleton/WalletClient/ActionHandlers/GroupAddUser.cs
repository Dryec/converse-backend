using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Converse.Database;
using Converse.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Converse.Utils;

namespace Converse.Singleton.WalletClient.ActionHandlers
{
	public static class GroupAddUser
	{
		public static void Handle(Action.Context context)
		{
			var addUserToGroup = JsonConvert.DeserializeObject<Action.Group.AddUser>(context.Message);

			var invitedUserAddress = addUserToGroup.Address.DecryptByTransaction(context.Transaction)
				?.ToUtf8String();
			if (invitedUserAddress == null || Client.WalletAddress.Decode58Check(invitedUserAddress) == null || invitedUserAddress == context.Receiver)
			{
				context.Logger.Log.LogDebug(Logger.InvalidBase64Format, "Invalid Base64 Format!");
				return;
			}

			context.Logger.Log.LogDebug(Logger.HandleGroupAddUser,
				"AddUserToGroup: Sender '{Address}' GroupOwnerAddress '{OwnerAddress}' InvitedUser '{InvitedUser}'!",
				context.Sender, context.Receiver, invitedUserAddress);

			var chat = context.DatabaseContext
				.GetChatAsync(context.Receiver, chats => chats.Include(c => c.Setting).Include(c => c.Users))
				.GetAwaiter().GetResult();
			if (chat == null)
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupAddUser, "Chat not found!");
				return;
			}

			if (!chat.Setting.IsPublic && !chat.IsUserAdminOrHigher(context.Sender))
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupAddUser,
					"User in Chat not found or doesn't have the permissions.");
				return;
			}

			if (chat.HasUser(invitedUserAddress))
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupAddUser,
					"User already in chat.");
				return;
			}

			Models.User user = context.DatabaseContext.CreateUserWhenNotExist(invitedUserAddress);
			Models.ChatUser chatUser = context.DatabaseContext.CreateChatUser(chat,
				new DatabaseContext.ChatUserSetting()
				{
					User = user,
					PrivateKey = addUserToGroup.PrivateKey,
					Rank = ChatUserRank.User
				},
				context.Transaction.RawData.Timestamp);

			context.DatabaseContext.SaveChanges();

			context.ServiceProvider.GetService<FCMClient>()?.AddUserToGroup(chat, chatUser);
		}
	}
}
