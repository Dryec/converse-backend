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
	public static class GroupLeave
	{
		public static void Handle(Action.Context context)
		{
			// Deserialize message
			var leaveMessage = JsonConvert.DeserializeObject<Action.Group.Leave>(context.Message);

			context.Logger.Log.LogDebug(Logger.HandleGroupLeave,
				"GroupLeave: Sender '{Address}' GroupOwnerAddress '{OwnerAddress}'!",
				context.Sender, context.Receiver);

			// Get chat
			var chat = context.DatabaseContext
				.GetChatAsync(context.Receiver, chats => chats.Include(c => c.Setting).Include(c => c.Users))
				.GetAwaiter().GetResult();
			if (chat == null)
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupLeave, "Chat not found!");
				return;
			}

			// Check if is in group
			var leftChatUser = chat.GetUser(context.Sender);
			if (leftChatUser == null)
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupLeave, "User not found.");
				return; 
			}

			// Remove User from Group
			context.DatabaseContext.ChatUsers.Remove(leftChatUser);
			context.DatabaseContext.SaveChanges();

			// Notify all members
			context.ServiceProvider.GetService<FCMClient>()?.RemoveUserFromGroup(chat, leftChatUser);
		}
	}
}
