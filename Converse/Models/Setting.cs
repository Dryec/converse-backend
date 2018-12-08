using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Converse.Models
{
	[Table("settings")]
	public class Setting
	{
		public int Id { get; set; }
		public string Key { get; set; }
		public string Value { get; set; }
	}
}
