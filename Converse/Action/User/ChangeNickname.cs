using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Converse.Action.User
{
	public class ChangeNickname
	{
		[JsonProperty(Required = Required.Always)]
		public Type Type { get; set; }

		[JsonProperty(Required = Required.Always)]
		public string Name { get; set; }
	}
}
