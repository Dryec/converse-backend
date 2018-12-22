using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Converse.Action.Group
{
	public class ChangePicture : Action
	{
		[JsonProperty(Required = Required.Always)]
		public string Image { get; set; }

		[JsonProperty(Required = Required.Always)]
		public bool Clear { get; set; }
	}
}
