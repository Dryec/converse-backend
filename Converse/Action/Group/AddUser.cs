﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Converse.Action.Group
{
	public class AddUser : Action
	{
		[JsonProperty(Required = Required.Always)]
		public string Address { get; set; }

		public string Key { get; set; }
	}
}
