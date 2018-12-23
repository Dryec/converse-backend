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
			// Deserialize message
			var changeNicknameMessage = JsonConvert.DeserializeObject<Action.User.ChangeNickname>(context.Message);

			// Decrypt nickname
			var nickname = changeNicknameMessage.Name.DecryptByTransaction(context.Transaction)?.ToUtf8String();
			if (nickname == null)
			{
				context.Logger.Log.LogDebug(Logger.InvalidBase64Format, "Invalid Base64 Format!");
				return;
			}

			// Get user
			var user = context.DatabaseContext.GetUser(context.Sender).GetAwaiter().GetResult();
			if (user == null)
			{
				return;
			}

			context.Logger.Log.LogDebug(Logger.HandleUserChangeNickname,
				"UserChangeNickname: Sender '{Address}' Name: '{Name}'!",
				context.Sender, nickname);

			user.Nickname = nickname;
			context.DatabaseContext.SaveChanges();

			// Notify everyone that knows this user
			context.ServiceProvider.GetService<FCMClient>()?.UpdateAddress(user);
		}
	}
}
