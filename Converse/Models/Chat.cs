using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Converse.Models
{
	public class Chat
	{
		public int Id { get; set; }
		public bool IsGroup { get; set; }
		public DateTime CreatedAt { get; set; }

		public List<ChatMessage> Messages { get; set; }

		public Chat()
		{
			Messages = new List<ChatMessage>();
		}
	}
}
