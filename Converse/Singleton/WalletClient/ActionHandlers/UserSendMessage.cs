using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebaseNet.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Converse.Singleton.WalletClient.ActionHandlers
{
	public class ChatUserSetting
	{
		public Models.User User { get; set; }
		public bool IsAdmin { get; set; }
	}

	public static class UserSendMessage
	{
		public static void Handle(Action.Context context)
		{
			// Deserialize send message
			var userSendMessage = JsonConvert.DeserializeObject<Action.User.SendMessage>(context.Message);

			context.Logger.Log.LogDebug(Logger.HandleUserSendMessage,
				"SendMessage: Sender '{Sender}' Receiver '{Receiver}'!",
				context.Sender, context.Receiver);

			// Get user models from sender and receiver
			var senderUser = context.DatabaseContext.GetUser(context.Sender, users => users.Include(u => u.DeviceIds));
			var receiverUser = context.DatabaseContext.GetUser(context.Receiver, users => users.Include(u => u.DeviceIds));
			if (senderUser == null || receiverUser == null)
			{
				context.Logger.Log.LogDebug(Logger.HandleUserSendMessage,
					"SendMessage: Sender or receiver does not exist as user! Does exist: Sender({SenderUser}), Receiver({ReceiverUser}).",
					senderUser != null, receiverUser != null);
				return;
			}

			// Get chat or create if not exist
			var chat = context.DatabaseContext.GetChat(context.Sender, context.Receiver) ??
			           context.DatabaseContext.CreateChat(senderUser, receiverUser, context.Transaction.RawData.Timestamp).Item1;

			// Get last sent message id
			var lastMessageInternalId = context.DatabaseContext.ChatMessages.LastOrDefault(cm => cm.Chat == chat)?.InternalId ?? 0;

			// Create new chat message
			var chatMessage = new Models.ChatMessage()
			{
				InternalId = lastMessageInternalId + 1,
				Chat = chat,

				User = senderUser,
				Address = context.Sender,
				Message = userSendMessage.Message,

				BlockId = context.Block.BlockHeader.RawData.Number,
				BlockCreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(context.Block.BlockHeader.RawData.Timestamp).DateTime,

				TransactionHash = context.TransactionHash,
				TransactionCreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(context.Transaction.RawData.Timestamp).DateTime,

				CreatedAt = DateTime.UtcNow,
			};
			context.DatabaseContext.ChatMessages.Add(chatMessage);
			context.DatabaseContext.SaveChanges();

			context.ServiceProvider.GetService<FCMClient>()?.NotifyUserMessage(senderUser, receiverUser, chatMessage);
		}
	}
}
