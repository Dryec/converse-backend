﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebaseNet.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Converse.Singleton.WalletClient.ActionHandlers
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
			context.DatabaseContext.SaveChanges();

			context.ServiceProvider.GetService<FCMClient>()?
				.SendMessage("/topic/update/" + user.Address, user.Id.ToString(), "update_user",
					new Models.View.User(user), null, MessagePriority.high)
				.ConfigureAwait(false);
		}
	}
}
