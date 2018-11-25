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
		public virtual Chat Chat { get; set; }
		public string Name;
		public string Description;
		public string PictureUrl;
		public DateTime CreatedAt { get; set; }
	}
}
