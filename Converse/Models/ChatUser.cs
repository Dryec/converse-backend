using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Converse.Models
{
	public enum ChatUserRank
	{
		User,
		Admin,
		Owner,
	}

	[Table("chatusers")]
	public class ChatUser
	{
		public int Id { get; set; }

		public int ChatId { get; set; }
		public Chat Chat { get; set; }

		public int UserId { get; set; }
		public User User { get; set; }

		public string Address { get; set; }
		public ChatUserRank Rank { get; set; }

		public string PrivateKey { get; set; }

		public DateTime JoinedAt { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
