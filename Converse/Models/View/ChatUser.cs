using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Converse.Models.View
{
	public class ChatUser : Models.View.User
	{
		[JsonProperty(PropertyName = "is_admin")]
		public bool IsAdmin { get; set; }

		[JsonProperty(PropertyName = "timestamp")]
		public DateTime JoinedAt { get; }

		public ChatUser(Models.ChatUser chatUser)
			: base(chatUser.User)
		{
			IsAdmin = chatUser.IsAdmin;
			JoinedAt = chatUser.JoinedAt;
		}
	}
}
