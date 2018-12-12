using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Converse.Singleton.WalletClient.ActionHandlers
{
	public static class UserBlockUser
	{
		public static void Handle(Action.Context context)
		{
			var blockUserMessage = JsonConvert.DeserializeObject<Action.User.BlockUser>(context.Message);
			var blockedUser = context.DatabaseContext.GetBlockedUser(context.Sender, blockUserMessage.Address);

			context.Logger.Log.LogDebug(Logger.HandleUserBlockedUser, "BlockUser: Sender '{Address}' Blocked: '{BlockedAddress}' IsBlocked: {IsBlocked}!",
				context.Sender, blockUserMessage.Address, blockUserMessage.IsBlocked);

			var user = context.DatabaseContext.GetUser(context.Sender);
			if (user == null)
			{
				context.Logger.Log.LogDebug(Logger.HandleUserBlockedUser, "BlockUser: sender does not exist as user!");
				return;
			}

			if (blockUserMessage.IsBlocked)
			{
				if (blockedUser != null)
				{
					return;
				}

				blockedUser = new Models.BlockedUser()
				{
					User = user,
					Address = context.Sender,
					BlockedAddress = blockUserMessage.Address,
					CreatedAt = DateTime.Now,
				};

				context.DatabaseContext.BlockedUsers.Add(blockedUser);
				context.DatabaseContext.SaveChanges();
			}
			else if (blockedUser != null)
			{
				context.DatabaseContext.Remove(blockUserMessage);
				context.DatabaseContext.SaveChanges();
			}
		}
	}
}
