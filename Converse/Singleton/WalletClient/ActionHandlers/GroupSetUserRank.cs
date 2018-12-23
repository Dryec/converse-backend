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
	public static class GroupSetUserRank
	{
		public static void Handle(Action.Context context)
		{
			// Deserialize message
			var setUserRankMessage = JsonConvert.DeserializeObject<Action.Group.SetUserRank>(context.Message);

			// Decrypt rank address
			var setRankAddress = setUserRankMessage.Address.DecryptByTransaction(context.Transaction)
				?.ToUtf8String();
			if (setRankAddress == null || Client.WalletAddress.Decode58Check(setRankAddress) == null || setRankAddress == context.Receiver)
			{
				context.Logger.Log.LogDebug(Logger.InvalidBase64Format, "Invalid Base64 Format!");
				return;
			}

			// Check if rank is valid and not owner
			if (Enum.IsDefined(typeof(ChatUserRank), setUserRankMessage.Rank) || setUserRankMessage.Rank == ChatUserRank.Owner)
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupSetUserRank, "Rank invalid/Owner not settable!");
				return;
			}

			context.Logger.Log.LogDebug(Logger.HandleGroupSetUserRank,
				"SetUserRankGroup: Sender '{Address}' GroupOwnerAddress '{OwnerAddress}' UserSetRank '{UserSetRank}' Rank '{Rank}'!",
				context.Sender, context.Receiver, setRankAddress, setUserRankMessage.Rank);

			// Get chat
			var chat = context.DatabaseContext
				.GetChatAsync(context.Receiver, chats => chats.Include(c => c.Setting).Include(c => c.Users))
				.GetAwaiter().GetResult();
			if (chat == null)
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupSetUserRank, "Chat not found!");
				return;
			}

			// Get sender and set-rank-user
			var senderChatUser = chat.GetUser(context.Sender);
			var setRankChatUser = chat.GetUser(setRankAddress);
			if (senderChatUser == null || setRankChatUser == null)
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupSetUserRank,
					"User not in chat.");
				return;
			}

			// Only owner can set ranks
			if (senderChatUser.Rank != ChatUserRank.Owner)
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupSetUserRank,
					"User rank not high enough.");
				return;
			}

			setRankChatUser.Rank = setUserRankMessage.Rank;
			context.DatabaseContext.SaveChanges();

			// Notify all members in group
			context.ServiceProvider.GetService<FCMClient>()?.SetUserGroupRank(chat, setRankChatUser);
		}
	}
}
