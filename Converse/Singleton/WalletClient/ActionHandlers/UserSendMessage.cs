using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Converse.Singleton.WalletClient.ActionHandlers
{
	public static class UserSendMessage
	{
		public static void Handle(Action.Context context)
		{
			var userSendMessage = JsonConvert.DeserializeObject<Action.User.SendMessage>(context.Message);

			context.Logger.Log.LogDebug(Logger.HandleUserSendMessage,
				"SendMessage: Sender '{Sender}' Receiver '{Receiver}'!",
				context.Sender, context.Receiver);

			var senderUser = context.DatabaseContext.GetUser(context.Sender);
			var receiverUser = context.DatabaseContext.GetUser(context.Receiver);
			if (senderUser == null || receiverUser == null)
			{
				context.Logger.Log.LogDebug(Logger.HandleUserSendMessage,
					"SendMessage: Sender or receiver does not exist as user! Does exist: Sender({SenderUser}), Receiver({ReceiverUser}).",
					senderUser != null, receiverUser != null);
				return;
			}

			var chat = context.DatabaseContext.GetChat(context.Sender, context.Receiver);
			if (chat == null)
			{
				chat = new Models.Chat()
				{
					IsGroup = false,
					CreatedAt = DateTime.Now
				};

				context.DatabaseContext.Chats.Add(chat);

				var chatUser1 = new Models.ChatUser()
				{
					Chat = chat,
					User = senderUser,
					Address = context.Sender,

					IsAdmin = false,
					JoinedAt = DateTimeOffset.FromUnixTimeMilliseconds(context.Transaction.Transaction.RawData.Timestamp).DateTime,
					CreatedAt = DateTime.Now,
				};
				var chatUser2 = new Models.ChatUser()
				{
					Chat = chat,
					User = receiverUser,
					Address = context.Receiver,

					IsAdmin = false,
					JoinedAt = DateTimeOffset.FromUnixTimeMilliseconds(context.Transaction.Transaction.RawData.Timestamp).DateTime,
					CreatedAt = DateTime.Now,
				};

				context.DatabaseContext.ChatUsers.Add(chatUser1);
				context.DatabaseContext.ChatUsers.Add(chatUser2);
			}

			var lastMessageInternalId = context.DatabaseContext.ChatMessages.LastOrDefault(cm => cm.Chat == chat)?.InternalId ?? 0;
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
				TransactionCreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(context.Transaction.Transaction.RawData.Timestamp).DateTime,

				CreatedAt = DateTime.Now,
			};
			context.DatabaseContext.ChatMessages.Add(chatMessage);

			context.DatabaseContext.SaveChanges();
		}
	}
}
