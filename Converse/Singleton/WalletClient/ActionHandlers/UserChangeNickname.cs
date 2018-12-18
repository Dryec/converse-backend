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
	public static class UserChangeNickname
	{
		public static void Handle(Action.Context context)
		{
			// @ToDo: Check if nickname is valid
			var changeNicknameMessage = JsonConvert.DeserializeObject<Action.User.ChangeNickname>(context.Message);

			var nickname = changeNicknameMessage.Name.DecryptByTransaction(context.Transaction)?.ToUtf8String();
			if (nickname == null)
			{
				context.Logger.Log.LogDebug(Logger.InvalidBase64Format, "Invalid Base64 Format!");
				return;
			}

			var user = context.DatabaseContext.GetUser(context.Sender);
			if (user == null)
			{
				return;
			}

			context.Logger.Log.LogDebug(Logger.HandleUserChangeNickname,
				"UserChangeNickname: Sender '{Address}' Name: '{Name}'!",
				context.Sender, nickname);

			user.Nickname = nickname;
			context.DatabaseContext.SaveChanges();

			context.ServiceProvider.GetService<FCMClient>()?
				.SendMessage("/topic/update/" + user.Address, user.Id.ToString(), "update_user",
					new Models.View.User(user), null, MessagePriority.high)
				.ConfigureAwait(false);
		}
	}
}
