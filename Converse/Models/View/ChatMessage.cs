using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Converse.Models.View
{
	public class ChatMessage
	{
		[JsonProperty(PropertyName = "id")]
		public int Id { get; set; }

		[JsonProperty(PropertyName = "trans_id")]
		public string TransactionId { get; set; }

		[JsonProperty(PropertyName = "chat_id")]
		public int ChatId { get; set; }

		[JsonProperty(PropertyName = "sender")]
		public Models.View.User User { get; set; }

		[JsonProperty(PropertyName = "message")]
		public string Message { get; set; }

		[JsonProperty(PropertyName = "timestamp")]
		public DateTime CreatedAt { get; set; }

		public ChatMessage(Models.ChatMessage chatMessage)
		{
			Id = chatMessage.InternalId;
			TransactionId = chatMessage.TransactionHash;
			ChatId = chatMessage.ChatId;

			User = new Models.View.User(chatMessage.User);
			Message = chatMessage.Message;
			CreatedAt = chatMessage.CreatedAt;
		}
	}
}
