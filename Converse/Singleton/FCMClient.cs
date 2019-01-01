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
			if (string.IsNullOrEmpty(serverKey))
			{
				return;
			}

			_client = new FirebaseNet.Messaging.FCMClient(serverKey);
			_isInitialized = true;
		}

		private async Task<IFCMResponse> SendMessage<T>(string receiver, string id, string title, string type, T data, bool silent, MessagePriority priority)
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
							"title", title
						},
						{
							"type", type
						},
						{
							"data", JsonConvert.SerializeObject(data)
						},
						{
							"silent", silent.ToString().ToLower()
						},
						{
							"priority", priority.ToString()
						}
					}
					: null),
				Priority = priority,
			};

			return await _client.SendMessageAsync(message);
		}

		public void UpdateAddress(Models.User user)
		{
			SendMessage(
					"/topics/update_" + user.Address,
					user.Id.ToString(),
					user.Address,
					"update_user",
					new Models.View.User(user),
					true,
					MessagePriority.high
			).ConfigureAwait(false);
		}

		public void UpdateGroupAddress(Models.Chat chat)
		{
			if (!chat.IsGroup || chat.Setting == null)
			{
				return;
			}

			SendMessage(
				"/topics/group_" + chat.Setting.Address,
				chat.Id.ToString(),
				chat.Setting.Name,
				"update_info",
				new Models.View.ChatSetting(chat.Setting), 
				true,
				MessagePriority.high
			).ConfigureAwait(false);
		}
		
		public void NotifyGroupMessage(Models.Chat chat, Models.ChatMessage chatMessage)
		{
			if (!chat.IsGroup || chat.Setting == null)
			{
				return;
			}

			SendMessage(
				"/topics/group_" + chat.Setting.Address,
				chat.Id.ToString(),
				chat.Setting.Name,
				"grp_msg",
				new Models.View.ChatMessage(chatMessage), 
				false,
				MessagePriority.high
			).ConfigureAwait(false);
		}

		public void GroupCreated(List<Models.UserDeviceId> deviceIds, Models.Chat chat)
		{
			var chatView = new Models.View.Chat(chat, null);

			deviceIds.ForEach(deviceId => SendMessage(
				deviceId.DeviceId,
				chat.Id.ToString(),
				chat.Setting.Name,
				"group_created",
				chatView, 
				true,
				MessagePriority.high
			).ConfigureAwait(false));
		}

		public void AddUserToGroup(Models.Chat chat, Models.ChatUser chatUser)
		{
			if (!chat.IsGroup || chat.Setting == null)
			{
				return;
			}

			var chatUserData = new Models.View.ChatUser(chatUser);

			chatUser.User?.DeviceIds?.ForEach(deviceId =>
			{
				SendMessage(
					deviceId.DeviceId,
					chat.Id.ToString(),
					chat.Setting.Name,
					"user_added",
					chatUserData,
					true,
					MessagePriority.high
				).ConfigureAwait(false);
			});

			SendMessage(
				"/topics/group_" + chat.Setting.Address,
				chat.Id.ToString(),
				chat.Setting.Name,
				"user_added",
				chatUserData, 
				true,
				MessagePriority.high
			).ConfigureAwait(false);
		}

		public void RemoveUserFromGroup(Models.Chat chat, Models.ChatUser chatUser)
		{
			if (!chat.IsGroup || chat.Setting == null)
			{
				return;
			}

			SendMessage(
				"/topics/group_" + chat.Setting.Address,
				chat.Id.ToString(),
				chat.Setting.Name,
				"user_removed",
				new Models.View.ChatUser(chatUser), 
				true,
				MessagePriority.high
			).ConfigureAwait(false);
		}

		public void SetUserGroupRank(Models.Chat chat, Models.ChatUser chatUser)
		{
			if (!chat.IsGroup || chat.Setting == null)
			{
				return;
			}

			SendMessage(
				"/topics/group_" + chat.Setting.Address,
				chat.Id.ToString(),
				chat.Setting.Name,
				"user_rank",
				new Models.View.ChatUser(chatUser), 
				true,
				MessagePriority.high
			).ConfigureAwait(false);
		}

		public void NotifyUserMessage(Models.User sender, Models.User receiver, Models.ChatMessage chatMessage)
		{
			var viewChatMessage = new Models.View.ChatMessage(chatMessage);
			var chatId = chatMessage.ChatId.ToString();

			foreach (var receiverUserDeviceId in receiver.DeviceIds)
			{
				SendMessage(receiverUserDeviceId.DeviceId,
					chatId,
					sender.Nickname,
					"msg",
					viewChatMessage,
					false,
					MessagePriority.high
				).ConfigureAwait(false);
			}

			foreach (var senderUserDeviceId in sender.DeviceIds)
			{
				SendMessage(senderUserDeviceId.DeviceId,
					chatMessage.ChatId.ToString(),
					receiver.Nickname,
					"msg",
					viewChatMessage,
					true,
					MessagePriority.high
				).ConfigureAwait(false);
			}
		}
	}
}
