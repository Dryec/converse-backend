using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Converse.Action.User
{
	public class SendMessage
	{
		[JsonProperty(Required = Required.Always)]
		public Type Type { get; set; }

		[JsonProperty(Required = Required.Always)]
		public string Message { get; set; }
	}
}
