﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Converse.Models
{
	public class ChatUser
	{
		public int Id { get; set; }

		public int ChatId { get; set; }
		public virtual Chat Chat { get; set; }

		public string Address { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
