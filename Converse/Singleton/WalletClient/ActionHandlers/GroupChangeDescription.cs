using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Converse.Constants;
using Converse.Database;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Converse.Utils;

namespace Converse.Singleton.WalletClient.ActionHandlers
{
	public static class GroupChangeDescription
	{
		public static void Handle(Action.Context context)
		{
			var changeGroupDescription = JsonConvert.DeserializeObject<Action.Group.ChangeDescription>(context.Message);

			var groupDescription = changeGroupDescription.Description.DecryptByTransaction(context.Transaction)?.ToUtf8String();
			if (groupDescription == null)
			{
				context.Logger.Log.LogDebug(Logger.InvalidBase64Format, "Invalid Base64 Format!");
				return;
			}

			context.Logger.Log.LogDebug(Logger.HandleGroupChangeDescription, "ChangeGroupDescription: Sender '{Address}' GroupOwnerAddress '{OwnerAddress}' Description '{GroupDescription}'!",
				context.Sender, context.Receiver, groupDescription);

			var chat = context.DatabaseContext.GetChatAsync(context.Receiver, chats => chats.Include(c => c.Setting).Include(c => c.Users)).GetAwaiter().GetResult();
			if (chat?.Setting == null)
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupChangeDescription, "Chat not found!");
				return;
			}

			if (!chat.IsUserAdminOrHigher(context.Sender))
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupChangeDescription,
					"User in Chat not found or doesn't have the permissions.");
				return;
			}

			if (chat.Setting.Description != groupDescription)
			{
				chat.Setting.Description = groupDescription;
				context.DatabaseContext.SaveChanges();

				context.ServiceProvider.GetService<FCMClient>()?.UpdateGroupAddress(chat);
			}
		}
	}
}
