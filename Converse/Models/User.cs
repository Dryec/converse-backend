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
		public int Id { get; set; }
		public string Address { get; set; }
		public string Nickname { get; set; }
		public string ProfilePictureUrl { get; set; }
		public string Status { get; set; }
		public DateTime StatusUpdatedAt { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
