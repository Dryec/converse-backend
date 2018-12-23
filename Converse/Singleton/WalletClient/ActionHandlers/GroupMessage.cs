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
	public static class GroupMessage
	{
		public static void Handle(Action.Context context)
		{
			// Deserialize message
			var groupMessage = JsonConvert.DeserializeObject<Action.Group.SendMessage>(context.Message);

			context.Logger.Log.LogDebug(Logger.HandleGroupMessage,
				"GroupMessage: Sender '{Address}' GroupOwnerAddress '{OwnerAddress}' Message '{Message}'!",
				context.Sender, context.Receiver, groupMessage.Message);

			// Get chat
			var chat = context.DatabaseContext
				.GetChatAsync(context.Receiver, chats => chats.Include(c => c.Setting).Include(c => c.Users).Include(c => c.Messages))
				.GetAwaiter().GetResult();
			if (chat == null)
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupMessage, "Chat not found!");
				return;
			}

			// Check if is in group
			var senderUser = context.DatabaseContext.GetUserAsync(context.Sender).GetAwaiter().GetResult();
			if (!chat.HasUser(context.Sender))
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupMessage, "User is not in chat.");
				return;
			}

			// Get last sent message id
			var lastMessageInternalId = (chat.Messages.Any() ? chat.Messages.Max(cm => cm.InternalId) : 0);

			// @ToDo: Combine with SendMessage for PrivateChats
			// Create new chat message
			var chatMessage = new Models.ChatMessage()
			{
				InternalId = lastMessageInternalId + 1,
				Chat = chat,

				User = senderUser,
				Address = context.Sender,
				Message = groupMessage.Message,

				BlockId = context.Block.BlockHeader.RawData.Number,
				BlockCreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(context.Block.BlockHeader.RawData.Timestamp)
					.DateTime,

				TransactionHash = context.TransactionHash,
				TransactionCreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(context.Transaction.RawData.Timestamp)
					.DateTime,

				CreatedAt = DateTime.UtcNow,
			};
			context.DatabaseContext.ChatMessages.Add(chatMessage);
			context.DatabaseContext.SaveChanges();


			// Notify all members
			context.ServiceProvider.GetService<FCMClient>()?.NotifyGroupMessage(chat, chatMessage);
		}
	}
}
