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
	public static class UserSendMessage
	{
		public static void Handle(Action.Context context)
		{
			var userSendMessage = JsonConvert.DeserializeObject<Action.User.SendMessage>(context.Message);

			context.Logger.Log.LogDebug(Logger.HandleUserSendMessage,
				"SendMessage: Sender '{Sender}' Receiver '{Receiver}'!",
				context.Sender, context.Receiver);

			var senderUser = context.DatabaseContext.GetUser(context.Sender, users => users.Include(u => u.DeviceIds));
			var receiverUser = context.DatabaseContext.GetUser(context.Receiver, users => users.Include(u => u.DeviceIds));
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
					CreatedAt = DateTime.UtcNow
				};

				context.DatabaseContext.Chats.Add(chat);

				var chatUser1 = new Models.ChatUser()
				{
					Chat = chat,
					User = senderUser,
					Address = context.Sender,

					IsAdmin = false,
					JoinedAt = DateTimeOffset.FromUnixTimeMilliseconds(context.Transaction.RawData.Timestamp).DateTime,
					CreatedAt = DateTime.UtcNow,
				};
				var chatUser2 = new Models.ChatUser()
				{
					Chat = chat,
					User = receiverUser,
					Address = context.Receiver,

					IsAdmin = false,
					JoinedAt = DateTimeOffset.FromUnixTimeMilliseconds(context.Transaction.RawData.Timestamp).DateTime,
					CreatedAt = DateTime.UtcNow,
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
				TransactionCreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(context.Transaction.RawData.Timestamp).DateTime,

				CreatedAt = DateTime.UtcNow,
			};
			context.DatabaseContext.ChatMessages.Add(chatMessage);
			context.DatabaseContext.SaveChanges();

			var fcmClient = context.ServiceProvider.GetService<FCMClient>();
			if (fcmClient != null)
			{
				var androidNotification = new AndroidNotification()
				{
					Title = senderUser.Nickname ?? senderUser.Address,
					Body = chatMessage.Message,
				};
				var fcmData = new Models.View.ChatMessage(chatMessage);

				foreach (var receiverUserDeviceId in receiverUser.DeviceIds)
				{
					fcmClient.SendMessage(receiverUserDeviceId.DeviceId, chat.Id.ToString(), "msg", fcmData,
						androidNotification, MessagePriority.high).ConfigureAwait(false);
				}

				foreach (var senderUserDeviceId in senderUser.DeviceIds)
				{
					fcmClient.SendMessage(senderUserDeviceId.DeviceId, chat.Id.ToString(), "msg", fcmData, null,
						MessagePriority.high).ConfigureAwait(false);
				}
			}
		}
	}
}
