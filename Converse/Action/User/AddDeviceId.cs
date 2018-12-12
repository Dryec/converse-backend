using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Converse.Action.User
{
	public class AddDeviceId
	{
		[JsonProperty(Required = Required.Always)]
		public Type Type { get; set; }

		[JsonProperty(PropertyName = "device_id", Required = Required.Always)]
		public string DeviceId { get; set; }
	}
}
