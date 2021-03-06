﻿using System;
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
			// Deserialize send message
			var userSendMessage = JsonConvert.DeserializeObject<Action.User.SendMessage>(context.Message);

			context.Logger.Log.LogDebug(Logger.HandleUserSendMessage,
				"SendMessage: Sender '{Sender}' Receiver '{Receiver}'!",
				context.Sender, context.Receiver);

			// Get user models from sender and receiver
			var senderUser = context.DatabaseContext.GetUserAsync(context.Sender, users => users.Include(u => u.DeviceIds))
				.GetAwaiter().GetResult();
			var receiverUser = context.DatabaseContext.GetUserAsync(context.Receiver, users => users.Include(u => u.DeviceIds))
				.GetAwaiter().GetResult();
			if (senderUser == null || receiverUser == null)
			{
				context.Logger.Log.LogDebug(Logger.HandleUserSendMessage,
					"SendMessage: Sender or receiver does not exist as user! Does exist: Sender({SenderUser}), Receiver({ReceiverUser}).",
					senderUser != null, receiverUser != null);
				return;
			}

			// Get chat or create if not exist
			var chat = context.DatabaseContext.GetChatAsync(context.Sender, context.Receiver).GetAwaiter().GetResult() ??
			           context.DatabaseContext.CreateChat(senderUser, receiverUser, context.Transaction.RawData.Timestamp);

			// Get last sent message id
			var chatMessages = context.DatabaseContext.ChatMessages.Where(cm => cm.Chat == chat);
			var lastMessageInternalId = (chatMessages.Any() ? chatMessages.Max(cm => cm.InternalId) : 0);

			// @ToDo: Combine with SendMessage from Groups
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
