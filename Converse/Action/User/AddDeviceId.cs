using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Converse.Action.User
{
	public class AddDeviceId : Action
	{
		[JsonProperty(PropertyName = "device_id", Required = Required.Always)]
		public string DeviceId { get; set; }
	}
}
