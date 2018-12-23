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
	public static class GroupJoin
	{
		public static void Handle(Action.Context context)
		{
			// Deserialize message
			var joinMessage = JsonConvert.DeserializeObject<Action.Group.Join>(context.Message);

			context.Logger.Log.LogDebug(Logger.HandleGroupJoin,
				"GroupJoin: Sender '{Address}' GroupOwnerAddress '{OwnerAddress}'!",
				context.Sender, context.Receiver);

			// Get chat
			var chat = context.DatabaseContext
				.GetChatAsync(context.Receiver, chats => chats.Include(c => c.Setting).Include(c => c.Users))
				.GetAwaiter().GetResult();
			if (chat == null)
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupJoin, "Chat not found!");
				return;
			}

			// Can only join when chat is public
			if (!chat.Setting.IsPublic)
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupJoin, "Chat not public.");
				return;
			}

			// Check if is already in group
			if (chat.HasUser(context.Sender))
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupJoin, "User already in chat.");
				return;
			}

			// Add User to Group
			var joinedUser = context.DatabaseContext.GetUserAsync(context.Sender).GetAwaiter().GetResult();
			var joinedChatUser = context.DatabaseContext.CreateChatUser(chat,
				new DatabaseContext.ChatUserSetting()
				{
					User = joinedUser,
					PrivateKey = null,
					Rank = ChatUserRank.User
				},
				context.Transaction.RawData.Timestamp);

			context.DatabaseContext.SaveChanges();

			// Notify all members
			context.ServiceProvider.GetService<FCMClient>()?.AddUserToGroup(chat, joinedChatUser);
		}
	}
}
