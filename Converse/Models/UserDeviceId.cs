﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Converse.Models
{
	[Table("userdeviceids")]
	public class UserDeviceId
	{
		public int Id { get; set; }

		public int UserId { get; set; }
		public User User { get; set; }

		public string DeviceId { get; set; }

		public DateTime UpdatedAt { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
