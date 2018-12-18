using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using FirebaseNet.Messaging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Converse.Singleton
{
	public class FCMClient
	{
		public static string ServerKey { get; set; }

		private FirebaseNet.Messaging.FCMClient _client;
		private bool _isInitialized;

		public void Initialize(IConfigurationSection configuration)
		{
			if (_isInitialized)
			{
				return;
			}

			var serverKey = configuration.GetValue<string>("ServerKey");
			_client = new FirebaseNet.Messaging.FCMClient(serverKey);

			_isInitialized = true;
		}

		private async Task<IFCMResponse> SendMessage<T>(string receiver, string id, string type, T data, INotification notification, MessagePriority priority)
			where T : class
		{
			if (!_isInitialized)
			{
				return null;
			}

			var message = new Message()
			{
				To = receiver,
				Data = (data != null
					? new Dictionary<string, string>()
					{
						{
							"id", id
						},
						{
							"type", type
						},
						{
							"data", JsonConvert.SerializeObject(data)
						},
						{
							"silent", (notification == null).ToString().ToLower()
						},
						{
							"priority", priority.ToString()
						}
					}
					: null),
				Notification = notification,
				Priority = priority,
			};

			return await _client.SendMessageAsync(message);
		}

		public void UpdateAddress(Models.User user)
		{
			SendMessage(
					"/topic/update_" + user.Address,
					user.Id.ToString(),
					"update_user",
					new Models.View.User(user),
					null,
					MessagePriority.high
			).ConfigureAwait(false);
		}

		public void NotifyUserMessage(Models.User sender, Models.User receiver, Models.ChatMessage chatMessage)
		{
			var androidNotification = new AndroidNotification()
			{
				Title = sender.Nickname ?? sender.Address,
				Body = chatMessage.Message,
			};
			var viewChatMessage = new Models.View.ChatMessage(chatMessage);

			var chatId = chatMessage.ChatId.ToString();

			foreach (var receiverUserDeviceId in receiver.DeviceIds)
			{
				SendMessage(receiverUserDeviceId.DeviceId,
					chatId,
					"msg",
					viewChatMessage,
					androidNotification,
					MessagePriority.high
				).ConfigureAwait(false);
			}

			foreach (var senderUserDeviceId in sender.DeviceIds)
			{
				SendMessage(senderUserDeviceId.DeviceId,
					chatMessage.ChatId.ToString(),
					"msg",
					viewChatMessage, null,
					MessagePriority.high
				).ConfigureAwait(false);
			}
		}
	}
}
