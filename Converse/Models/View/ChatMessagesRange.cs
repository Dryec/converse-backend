using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Converse.Models.View
{
	public class ChatMessagesRange
	{
		public int Id { get; set; }

		public int Start { get; set; }
		public int End { get; set; }

		[JsonProperty(PropertyName = "messages")]
		public List<Models.View.ChatMessage> ChatMessages { get; set; }

		public ChatMessagesRange(int chatId, int start, int end, IEnumerable<Models.ChatMessage> chatMessages)
		{
			Id = chatId;

			Start = start;
			End = end;

			ChatMessages = new List<ChatMessage>();
			foreach (var chatMessage in chatMessages)
			{
				ChatMessages.Add(new Models.View.ChatMessage(chatMessage));
			}
		}
	}
}
