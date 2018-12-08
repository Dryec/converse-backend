using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Converse.Models
{
	[Table("chatsettings")]
	public class ChatSetting
	{
		public int Id { get; set; }

		public int ChatId { get; set; }
		public Chat Chat { get; set; }

		// ChatOwner
		public string Address { get; set; }

		public string Name { get; set; }
		public string Description { get; set; }
		public string PictureUrl { get; set; }

		public bool IsPublic { get; set; }

		public DateTime CreatedAt { get; set; }
	}
}
