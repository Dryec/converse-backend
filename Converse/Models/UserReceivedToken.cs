using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Converse.Models
{
	public class UserReceivedToken
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public virtual User User { get; set; }
		public string Ip { get; set; }
		public int ReceivedTokens { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
