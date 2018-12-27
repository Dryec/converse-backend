using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Converse.Models
{
	[Table("subscriptions")]
	public class Subscriber
	{
		public int Id { get; set; }
		public string EMail { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
