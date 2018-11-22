using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Converse.Models
{
	public class Chat
	{
		public int Id { get; set; }
		[MaxLength(150)]
		public string FirstAddress { get; set; }
		[MaxLength(150)]
		public string SecondAddress { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
