using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Converse.Action.User
{
	public class ChangeStatus : Action
	{
		[JsonProperty(Required = Required.Always)]
		public string Status { get; set; }
	}
}
