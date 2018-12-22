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
			var changeGroupName = JsonConvert.DeserializeObject<Action.Group.ChangeName>(context.Message);

			var groupName = changeGroupName.Name.DecryptByTransaction(context.Transaction)?.ToUtf8String();
			if (groupName == null)
			{
				context.Logger.Log.LogDebug(Logger.InvalidBase64Format, "Invalid Base64 Format!");
				return;
			}

			context.Logger.Log.LogDebug(Logger.HandleGroupChangeName, "ChangeGroupName: Sender '{Address}' GroupOwnerAddress '{OwnerAddress}' Name '{GroupName}'!",
				context.Sender, context.Receiver, groupName);

			var chat = context.DatabaseContext.GetChatAsync(context.Receiver, chats => chats.Include(c => c.Setting).Include(c => c.Users)).GetAwaiter().GetResult();
			if (chat?.Setting == null)
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupChangeName, "Chat not found!");
				return;
			}

			if (!chat.IsUserAdminOrHigher(context.Sender))
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupChangeName,
					"User in Chat not found or doesn't have the permissions.");
				return;
			}

			if (chat.Setting.Name != groupName)
			{
				chat.Setting.Name = groupName;
				context.DatabaseContext.SaveChanges();
			}
		}
	}
}
