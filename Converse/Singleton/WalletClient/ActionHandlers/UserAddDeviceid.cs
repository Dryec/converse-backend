using System;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Common;
using Converse.Utils;

namespace Converse.Singleton.WalletClient.ActionHandlers
{
	public static class UserAddDeviceId
	{
		public static void Handle(Action.Context context)
		{
			// Get public key from transaction
			var publicKey = context.Transaction.GetPublicKey();
			if (publicKey == null)
			{
				context.Logger.Log.LogError(Logger.InvalidPublicKey, "Couldn't get public key.");
				return;
			}
			
			// Deserialize message from transaction
			var userAddDeviceId = JsonConvert.DeserializeObject<Action.User.AddDeviceId>(context.Message);

			// Decrypt deviceId by PropertyAddress key
			var deviceIdHexString = WalletClient.PropertyAddress.DecryptData(userAddDeviceId.DeviceId.FromHexToByteArray(), publicKey);

			// Decode the device id
			var deviceId = Encoding.UTF8.GetString(deviceIdHexString);

			// ToDo: Check deviceId

			context.Logger.Log.LogDebug(Logger.HandleUserSendMessage,
				"AddDeviceId: Sender '{Sender} DeviceId {DeviceId}'!",
				context.Sender, deviceId);

			// Get user id to search if device id is already registered
			var senderUser = context.DatabaseContext.GetUser(context.Sender, users => users.Include(u => u.DeviceIds));
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
