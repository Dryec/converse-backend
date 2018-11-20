using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Converse.Models
{
	public class User
	{
		public int Id { get; set; }
		public string Address { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
