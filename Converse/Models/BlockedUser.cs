using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Converse.Models
{
	public class BlockedUser
	{
		public int Id { get; set; }
		public string Address { get; set; }
		public string BlockedAddress { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
