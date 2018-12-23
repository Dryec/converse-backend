using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebaseNet.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Converse.Utils;

namespace Converse.Singleton.WalletClient.ActionHandlers
{
	public static class UserChangeStatus
	{
		public static void Handle(Action.Context context)
		{
			// @ToDo: Check if status is valid
			// Deserialize message
			var changeStatusMessage = JsonConvert.DeserializeObject<Action.User.ChangeStatus>(context.Message);

			// Decrypt status
			var status = changeStatusMessage.Status.DecryptByTransaction(context.Transaction)?.ToUtf8String();
			if (status == null)
			{
				context.Logger.Log.LogDebug(Logger.InvalidBase64Format, "Invalid Base64 Format!");
				return;
			}

			// Get chat
			var user = context.DatabaseContext.GetUser(context.Sender).GetAwaiter().GetResult();
			if (user == null)
			{
				return;
			}

			context.Logger.Log.LogDebug(Logger.HandleUserChangeStatus,
				"UserChangeStatus: Sender '{Address}' Status: '{Status}'!",
				context.Sender, status);

			user.Status = status;
			user.StatusUpdatedAt = DateTimeOffset.FromUnixTimeMilliseconds(context.Transaction.RawData.Timestamp).DateTime;
			context.DatabaseContext.SaveChanges();

			// Notify everyone that knows this user
			context.ServiceProvider.GetService<FCMClient>()?.UpdateAddress(user);
		}
	}
}
