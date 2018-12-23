using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Converse.Models;
using Newtonsoft.Json;

namespace Converse.Action.Group
{
	public class SetUserRank : Action
	{
		[JsonProperty(Required = Required.Always)]
		public ChatUserRank Rank { get; set; }

		[JsonProperty(Required = Required.Always)]
		public string Address { get; set; }
	}
}
