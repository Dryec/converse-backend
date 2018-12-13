using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Crypto;
using Common;
using Google.Protobuf;

namespace Converse.Singleton.WalletClient.ActionHandlers
{
	public static class UserAddDeviceId
	{
		public static void Handle(Action.Context context)
		{
			var signature = new ECDSASignature(context.Transaction.Transaction.Signature.ElementAt(0).ToByteArray());
			var publicKey = ECKey.RecoverPubBytesFromSignature(signature,
				Crypto.Sha256.Hash(context.Transaction.Transaction.RawData.ToByteArray()), false);


			var userAddDeviceId = JsonConvert.DeserializeObject<Action.User.AddDeviceId>(context.Message);

			var xy = WalletClient.PropertyAddress.ECKey.Decrypt(userAddDeviceId.DeviceId.FromHexToByteArray(), publicKey);

			var z = System.Text.Encoding.Default.GetString(xy);

			context.Logger.Log.LogDebug(Logger.HandleUserSendMessage,
				"AddDeviceId: Sender '{Sender} DeviceId {DeviceId}'!",
				context.Sender, userAddDeviceId.DeviceId);

			var sender = context.DatabaseContext.GetUser(context.Sender);

			// @ToDo

			context.DatabaseContext.SaveChanges();
		}
	}
}
