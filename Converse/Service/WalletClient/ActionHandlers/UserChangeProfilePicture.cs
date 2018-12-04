using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Converse.Service.WalletClient.ActionHandlers
{
	public static class UserChangeProfilePicture
	{
		public static void Handle(Action.Context context)
		{
			// @ToDo: Check if picture url is valid
			var changeProfilePictureMessage = JsonConvert.DeserializeObject<Action.User.ChangeProfilePicture>(context.Message);

			var user = context.DatabaseContext.GetUser(context.Sender);
			if (user == null)
			{
				return;
			}

			context.Logger.Log.LogDebug(Logger.HandleUserChangeProfilePicture,
				"UserChangeProfilePicture: Sender '{Address}' Image: '{Image}' Clear: {Clear}!",
				context.Sender, changeProfilePictureMessage.Image, changeProfilePictureMessage.Clear);

			user.ProfilePictureUrl = (changeProfilePictureMessage.Clear ? null : changeProfilePictureMessage.Image);
		}
	}
}
