using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Converse.Action.Group
{
	public class Create : Action
	{
		[JsonProperty(Required = Required.Always)]
		public string Address { get; set; }

		[JsonProperty(PropertyName = "public_key", Required = Required.Always)]
		public string PublicKey { get; set; }

		[JsonProperty(PropertyName = "private_key", Required = Required.Always)]
		public string PrivateKey { get; set; }

		[JsonProperty(Required = Required.AllowNull)]
		public string Name { get; set; }

		[JsonProperty(Required = Required.AllowNull)]
		public string Description { get; set; }

		[JsonProperty(Required = Required.AllowNull)]
		public string Image { get; set; }

		[JsonProperty(PropertyName = "is_public", Required = Required.Always)]
		public bool IsPublic { get; set; }
	}
}
