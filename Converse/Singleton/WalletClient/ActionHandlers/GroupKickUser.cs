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
			var kickUserFromGroup = JsonConvert.DeserializeObject<Action.Group.KickUser>(context.Message);

			var kickedUserAddress = kickUserFromGroup.Address.DecryptByTransaction(context.Transaction)
				?.ToUtf8String();
			if (kickedUserAddress == null || Client.WalletAddress.Decode58Check(kickedUserAddress) == null || kickedUserAddress == context.Receiver)
			{
				context.Logger.Log.LogDebug(Logger.InvalidBase64Format, "Invalid Base64 Format!");
				return;
			}

			context.Logger.Log.LogDebug(Logger.HandleGroupKickUser,
				"UserKickedFromGroup: Sender '{Address}' GroupOwnerAddress '{OwnerAddress}' KickedUser '{KickedUser}'!",
				context.Sender, context.Receiver, kickedUserAddress);

			var chat = context.DatabaseContext
				.GetChatAsync(context.Receiver, chats => chats.Include(c => c.Users))
				.GetAwaiter().GetResult();
			if (chat == null)
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupKickUser, "Chat not found!");
				return;
			}

			var kickedUser = chat.GetUser(kickedUserAddress);
			if (kickedUser == null)
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupKickUser,
					"User not found in chat.");
				return;
			}

			var senderUser = chat.GetUser(context.Sender);
			if (kickedUser.Rank >= senderUser.Rank)
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupKickUser,
					"User rank is too high");
				return;
			}

			context.DatabaseContext.ChatUsers.Remove(kickedUser);
			context.DatabaseContext.SaveChanges();

			context.ServiceProvider.GetService<FCMClient>()?.KickUserFromGroup(chat, kickedUser);
		}
	}
}
