using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebaseNet.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Converse.Singleton.WalletClient.ActionHandlers
{
	public static class UserChangeStatus
	{
		public static void Handle(Action.Context context)
		{
			// @ToDo: Check if status is valid
			var changeStatusMessage = JsonConvert.DeserializeObject<Action.User.ChangeStatus>(context.Message);

			var user = context.DatabaseContext.GetUser(context.Sender);
			if (user == null)
			{
				return;
			}

			context.Logger.Log.LogDebug(Logger.HandleUserChangeStatus,
				"UserChangeStatus: Sender '{Address}' Status: '{Status}'!",
				context.Sender, changeStatusMessage.Status);

			user.Status = changeStatusMessage.Status;
			user.StatusUpdatedAt = DateTimeOffset.FromUnixTimeMilliseconds(context.Transaction.RawData.Timestamp).DateTime;
			context.DatabaseContext.SaveChanges();

			context.ServiceProvider.GetService<FCMClient>()?
				.SendMessage("/topic/update/" + user.Address, user.Id.ToString(), "update_user", new Models.View.User(user), null, MessagePriority.high)
				.ConfigureAwait(false);
		}
	}
}
