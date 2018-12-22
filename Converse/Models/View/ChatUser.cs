using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Converse.Models.View
{
	public class ChatUser : Models.View.User
	{
		[JsonProperty(PropertyName = "rank")]
		public ChatUserRank Rank { get; set; }

		[JsonProperty(PropertyName = "timestamp")]
		public DateTime JoinedAt { get; }

		public ChatUser(Models.ChatUser chatUser)
			: base(chatUser.User)
		{
			Rank = chatUser.Rank;
			JoinedAt = chatUser.JoinedAt;
		}
	}
}
