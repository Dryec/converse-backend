using System;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Converse.Utils;

namespace Converse.Singleton.WalletClient.ActionHandlers
{
	public static class UserAddDeviceId
	{
		public static void Handle(Action.Context context)
		{
			// Deserialize message from transaction
			var userAddDeviceId = JsonConvert.DeserializeObject<Action.User.AddDeviceId>(context.Message);

			string deviceId = userAddDeviceId.DeviceId.DecryptByTransaction(context.Transaction)?.ToUtf8String();
			if (deviceId == null)
			{
				context.Logger.Log.LogDebug(Logger.InvalidBase64Format, "Invalid Base64 Format!");
				return;
			}

			// ToDo: Check deviceId

			context.Logger.Log.LogDebug(Logger.HandleUserSendMessage,
				"AddDeviceId: Sender '{Sender} DeviceId {DeviceId}'!",
				context.Sender, deviceId);

			// Get user id to search if device id is already registered
			var senderUser =
				context.DatabaseContext.GetUser(context.Sender, users => users.Include(u => u.DeviceIds));
			var userDeviceId = senderUser.DeviceIds.Find(u => u.DeviceId == deviceId);

			if (userDeviceId == null)
			{
				userDeviceId = new Models.UserDeviceId()
				{
					User = senderUser,
					DeviceId = deviceId,
					UpdatedAt = DateTime.UtcNow,
					CreatedAt = DateTime.UtcNow,
				};

				context.DatabaseContext.UserDeviceIds.Add(userDeviceId);
			}
			else
			{
				userDeviceId.UpdatedAt = DateTime.UtcNow;
			}

			context.DatabaseContext.SaveChanges();
		}
	}
}
