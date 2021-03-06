﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Converse.Action.User
{
	public class BlockUser : Action
	{
		[JsonProperty(Required = Required.Always)]
		public string Address { get; set; }

		[JsonProperty(PropertyName = "is_blocked", Required = Required.Always)]
		public bool IsBlocked { get; set; }
	}
}
