using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Converse.Service.WalletClient.ActionHandlers
{
	public static class UserSendMessage
	{
		public static void Handle(Action.Context context)
		{
			var userSendMessage = JsonConvert.DeserializeObject<Action.User.SendMessage>(context.Message);

			context.Logger.Log.LogDebug(Logger.HandleUserSendMessage,
				"SendMessage: Sender '{Sender}' Receiver '{Receiver}'!",
				context.Sender, context.Receiver);


		}
	}
}
