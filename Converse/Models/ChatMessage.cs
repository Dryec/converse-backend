using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Converse.Models
{
	public class ChatMessage
	{
		public int Id { get; set; }

		public int ChatId { get; set; }
		public virtual Chat Chat { get; set; }

		public string Address { get; set; }
		public string Message { get; set; }

		public string BlockId { get; set; }
		public string TransactionHash { get; set; }

		public DateTime BlockCreatedAt { get; set; }
		public DateTime TransactionCreatedAt { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
