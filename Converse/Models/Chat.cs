using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Converse.Models
{
	[Table("chats")]
	public class Chat
	{
		public int Id { get; set; }
		public bool IsGroup { get; set; }
		public DateTime CreatedAt { get; set; }

		public ChatSetting Setting { get; set; }
		public List<ChatUser> Users { get; set; }
		public List<ChatMessage> Messages { get; set; }

		public Chat()
		{
			Messages = new List<ChatMessage>();
			Users = new List<ChatUser>();
		}

		public ChatUser GetPartner(string address)
		{
			if (GetType() == Constants.Chat.Type.Normal && Users.Count == 2)
			{
				return Users.FirstOrDefault(u => u.Address != address);
			}

			return null;
		}

		public new Constants.Chat.Type GetType()
		{
			return (IsGroup ? Constants.Chat.Type.Group : Constants.Chat.Type.Normal);
		}
	}
}
