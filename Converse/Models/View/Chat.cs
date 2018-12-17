using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Converse.Models.View
{
	public class Chat
	{
		[JsonProperty(PropertyName = "id")]
		public int Id { get; set; }

		[JsonProperty(PropertyName = "type")]
		public Constants.Chat.Type Type { get; set; }
		
		[JsonProperty(PropertyName = "messages_count")]
		public int MessageCount { get; set; }

		[JsonProperty(PropertyName = "chat_partner")]
		public ChatUser ChatUser { get; set; }

		[JsonProperty(PropertyName = "group_info")]
		public ChatSetting ChatSetting { get; set; }

		[JsonProperty(PropertyName = "last_message")]
		public ChatMessage ChatMessage { get; set; }

		public Chat(Models.Chat chat, string userAddress)
		{
			Id = chat.Id;
			Type = chat.GetType();
			 
			MessageCount = chat.Messages.Count;

			if (Type == Constants.Chat.Type.Normal)
			{
				var chatPartner = chat.GetPartner(userAddress);
				if (chatPartner != null)
				{
					ChatUser = new Models.View.ChatUser(chatPartner);
				}
			}
			else if (Type == Constants.Chat.Type.Group)
			{
				if (chat.Setting != null)
				{
					ChatSetting = new Models.View.ChatSetting(chat.Setting);
				}
			}

			if (chat.Messages.Count > 0)
			{
				ChatMessage = new Models.View.ChatMessage(chat.Messages.OrderBy(m => m.InternalId).Last());
			}
		}
	}
}
