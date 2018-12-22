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
	public static class UserChangeProfilePicture
	{
		public static void Handle(Action.Context context)
		{
			// @ToDo: Check if picture url is valid
			var changeProfilePictureMessage = JsonConvert.DeserializeObject<Action.User.ChangeProfilePicture>(context.Message);

			var profilePictureUrl = changeProfilePictureMessage.Image.DecryptByTransaction(context.Transaction)?.ToUtf8String();
			if (profilePictureUrl == null)
			{
				context.Logger.Log.LogDebug(Logger.InvalidBase64Format, "Invalid Base64 Format!");
				return;
			}

			var user = context.DatabaseContext.GetUser(context.Sender).GetAwaiter().GetResult();
			if (user == null)
			{
				return;
			}

			context.Logger.Log.LogDebug(Logger.HandleUserChangeProfilePicture,
				"UserChangeProfilePicture: Sender '{Address}' Image: '{Image}' Clear: {Clear}!",
				context.Sender, profilePictureUrl, changeProfilePictureMessage.Clear);

			user.ProfilePictureUrl = (changeProfilePictureMessage.Clear ? null : profilePictureUrl);
			context.DatabaseContext.SaveChanges();

			context.ServiceProvider.GetService<FCMClient>()?.UpdateAddress(user);
		}
	}
}
