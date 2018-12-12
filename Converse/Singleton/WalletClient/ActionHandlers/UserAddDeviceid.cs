using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Converse.Singleton.WalletClient.ActionHandlers
{
	public static class UserAddDeviceId
	{
		public static void Handle(Action.Context context)
		{
			var userAddDeviceId = JsonConvert.DeserializeObject<Action.User.AddDeviceId>(context.Message);

			context.Logger.Log.LogDebug(Logger.HandleUserSendMessage,
				"AddDeviceId: Sender '{Sender} DeviceId {DeviceId}'!",
				context.Sender, userAddDeviceId.DeviceId);

			var sender = context.DatabaseContext.GetUser(context.Sender);

			// @ToDo

			context.DatabaseContext.SaveChanges();
		}
	}
}
