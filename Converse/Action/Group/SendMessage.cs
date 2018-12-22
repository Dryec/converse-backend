﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Converse.Action.Group
{
	public class SendMessage : Action
	{
		[JsonProperty(Required = Required.Always)]
		public string Message { get; set; }
	}
}
