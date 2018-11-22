using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Converse.Models
{
	public class User
	{
		[Key]
		public int Id { get; set; }
		[MaxLength(150)]
		public string Address { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
