using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Converse.Models
{
	public class ChatSetting
	{
		public int Id { get; set; }
		public int ChatId { get; set; }
		public Chat Chat { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string PictureUrl { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
