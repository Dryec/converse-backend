using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Converse.Service.WalletClient.ActionHandlers
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
			context.DatabaseContext.SaveChanges();
		}
	}
}
