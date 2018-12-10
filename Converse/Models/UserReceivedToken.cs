using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Converse.Models
{
	[Table("userreceivedtokens")]
	public class UserReceivedToken
	{
		public int Id { get; set; }

		public string Address { get; set; }

		public string Ip { get; set; }
		public int ReceivedTokens { get; set; }

		public DateTime CreatedAt { get; set; }
	}
}
