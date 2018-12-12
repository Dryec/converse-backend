using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Converse.Singleton.WalletClient.ActionHandlers
{
	public static class UserChangeNickname
	{
		public static void Handle(Action.Context context)
		{
			// @ToDo: Check if nickname is valid
			var changeNicknameMessage = JsonConvert.DeserializeObject<Action.User.ChangeNickname>(context.Message); 

			var user = context.DatabaseContext.GetUser(context.Sender);
			if (user == null)
			{
				return;
			}

			context.Logger.Log.LogDebug(Logger.HandleUserChangeNickname,
				"UserChangeNickname: Sender '{Address}' Name: '{Name}'!",
				context.Sender, changeNicknameMessage.Name);

			user.Nickname = changeNicknameMessage.Name;
			context.DatabaseContext.SaveChanges();
		}
	}
}
