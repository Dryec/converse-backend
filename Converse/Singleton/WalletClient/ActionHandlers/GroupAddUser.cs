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
			// Deserialize message
			var addUserMessage = JsonConvert.DeserializeObject<Action.Group.AddUser>(context.Message);

			// Decrypt invite address
			var inviteAddress = addUserMessage.Address.DecryptByTransaction(context.Transaction)
				?.ToUtf8String();
			if (inviteAddress == null || Client.WalletAddress.Decode58Check(inviteAddress) == null || inviteAddress == context.Receiver)
			{
				context.Logger.Log.LogDebug(Logger.InvalidBase64Format, "Invalid Base64 Format!");
				return;
			}

			context.Logger.Log.LogDebug(Logger.HandleGroupAddUser,
				"AddUserToGroup: Sender '{Address}' GroupOwnerAddress '{OwnerAddress}' InvitedUser '{InvitedUser}'!",
				context.Sender, context.Receiver, inviteAddress);

			// Get chat
			var chat = context.DatabaseContext
				.GetChatAsync(context.Receiver, chats => chats.Include(c => c.Setting).Include(c => c.Users))
				.GetAwaiter().GetResult();
			if (chat == null)
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupAddUser, "Chat not found!");
				return;
			}

			// When chat is public everyone can invite, else inviter has to be admin or higher
			if (!chat.Setting.IsPublic && !chat.IsUserAdminOrHigher(context.Sender))
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupAddUser,
					"User in Chat not found or doesn't have the permissions.");
				return;
			}

			// Check if is already in group
			if (chat.HasUser(inviteAddress))
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupAddUser,
					"User already in chat.");
				return;
			}

			// Add User to Group
			var invitedUser = context.DatabaseContext.CreateUserWhenNotExist(inviteAddress);
			var invitedChatUser = context.DatabaseContext.CreateChatUser(chat,
				new DatabaseContext.ChatUserSetting()
				{
					User = invitedUser,
					PrivateKey = addUserMessage.PrivateKey,
					Rank = ChatUserRank.User
				},
				context.Transaction.RawData.Timestamp);

			context.DatabaseContext.SaveChanges();

			// Notify all members
			context.ServiceProvider.GetService<FCMClient>()?.AddUserToGroup(chat, invitedChatUser);
		}
	}
}
