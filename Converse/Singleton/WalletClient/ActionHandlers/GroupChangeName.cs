using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Converse.Constants;
using Converse.Database;
using Converse.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Converse.Utils;

namespace Converse.Singleton.WalletClient.ActionHandlers
{
	public static class GroupChangeName
	{
		public static void Handle(Action.Context context)
		{
			// @ToDo: Check if name is valid
			// Deserialize message
			var changeNameMessage = JsonConvert.DeserializeObject<Action.Group.ChangeName>(context.Message);

			// Get encrypted groupName
			var groupName = changeNameMessage.Name.DecryptByTransaction(context.Transaction)?.ToUtf8String();
			if (groupName == null)
			{
				context.Logger.Log.LogDebug(Logger.InvalidBase64Format, "Invalid Base64 Format!");
				return;
			}

			context.Logger.Log.LogDebug(Logger.HandleGroupChangeName, "ChangeGroupName: Sender '{Address}' GroupOwnerAddress '{OwnerAddress}' Name '{GroupName}'!",
				context.Sender, context.Receiver, groupName);

			// Get chat
			var chat = context.DatabaseContext.GetChatAsync(context.Receiver, chats => chats.Include(c => c.Setting).Include(c => c.Users)).GetAwaiter().GetResult();
			if (chat?.Setting == null)
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupChangeName, "Chat not found!");
				return;
			}

			// Check user rank
			if (!chat.IsUserAdminOrHigher(context.Sender))
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupChangeName,
					"User in Chat not found or doesn't have the permissions.");
				return;
			}

			// Change name and notify all members
			if (chat.Setting.Name != groupName)
			{
				chat.Setting.Name = groupName;
				context.DatabaseContext.SaveChanges();

				context.ServiceProvider.GetService<FCMClient>()?.UpdateGroupAddress(chat);
			}
		}
	}
}
