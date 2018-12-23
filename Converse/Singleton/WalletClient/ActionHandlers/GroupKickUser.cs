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
	public static class GroupKickUser
	{
		public static void Handle(Action.Context context)
		{
			// Deserialize message
			var kickUserMessage = JsonConvert.DeserializeObject<Action.Group.KickUser>(context.Message);

			// Decrypt kick address
			var kickAddress = kickUserMessage.Address.DecryptByTransaction(context.Transaction)
				?.ToUtf8String();
			if (kickAddress == null || Client.WalletAddress.Decode58Check(kickAddress) == null || kickAddress == context.Receiver)
			{
				context.Logger.Log.LogDebug(Logger.InvalidBase64Format, "Invalid Base64 Format!");
				return;
			}

			context.Logger.Log.LogDebug(Logger.HandleGroupKickUser,
				"UserKickedFromGroup: Sender '{Address}' GroupOwnerAddress '{OwnerAddress}' KickedUser '{KickedUser}'!",
				context.Sender, context.Receiver, kickAddress);

			// Get chat
			var chat = context.DatabaseContext
				.GetChatAsync(context.Receiver, chats => chats.Include(c => c.Setting).Include(c => c.Users))
				.GetAwaiter().GetResult();
			if (chat == null)
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupKickUser, "Chat not found!");
				return;
			}

			// Get user that get kicked
			var kickChatUser = chat.GetUser(kickAddress);
			if (kickChatUser == null)
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupKickUser,
					"User not found in chat.");
				return;
			}

			// Get user that wants to kick
			var senderChatUser = chat.GetUser(context.Sender);
			if (kickChatUser.Rank >= senderChatUser.Rank)
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupKickUser,
					"User rank is too high");
				return;
			}

			context.DatabaseContext.ChatUsers.Remove(kickChatUser);
			context.DatabaseContext.SaveChanges();

			// Notify all members in group
			context.ServiceProvider.GetService<FCMClient>()?.RemoveUserFromGroup(chat, kickChatUser);
		}
	}
}
