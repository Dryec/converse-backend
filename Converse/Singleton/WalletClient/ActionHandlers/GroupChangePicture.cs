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
	public static class GroupChangePicture
	{
		public static void Handle(Action.Context context)
		{
			var changeGroupPicture = JsonConvert.DeserializeObject<Action.Group.ChangePicture>(context.Message);

			var groupImage = (changeGroupPicture.Clear ? null : changeGroupPicture.Image.DecryptByTransaction(context.Transaction)?.ToUtf8String());
			if (!changeGroupPicture.Clear && groupImage == null)
			{
				context.Logger.Log.LogDebug(Logger.InvalidBase64Format, "Invalid Base64 Format!");
				return;
			}

			context.Logger.Log.LogDebug(Logger.HandleGroupChangeImage, "ChangeGroupImage: Sender '{Address}' GroupOwnerAddress '{OwnerAddress}' Image '{GroupImage}'!",
				context.Sender, context.Receiver, groupImage);

			var chat = context.DatabaseContext.GetChatAsync(context.Receiver, chats => chats.Include(c => c.Setting).Include(c => c.Users)).GetAwaiter().GetResult();
			if (chat?.Setting == null)
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupChangeImage, "Chat not found!");
				return;
			}

			if (!chat.IsUserAdminOrHigher(context.Sender))
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupChangeImage,
					"User in Chat not found or doesn't have the permissions.");
				return;
			}

			if (chat.Setting.PictureUrl != groupImage)
			{
				chat.Setting.PictureUrl = groupImage;
				context.DatabaseContext.SaveChanges();

				context.ServiceProvider.GetService<FCMClient>()?.UpdateGroupAddress(chat);
			}
		}
	}
}
