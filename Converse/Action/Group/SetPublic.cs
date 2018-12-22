using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Converse.Action.Group
{
	public class SetPublic : Action
	{
		[JsonProperty(Required = Required.Always)]
		public bool IsPublic { get; set; }
	}
}
