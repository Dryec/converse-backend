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
			// @ToDo: Check if picture is valid
			// Deserialize message
			var changePictureMessage = JsonConvert.DeserializeObject<Action.Group.ChangePicture>(context.Message);

			// Decrypt group image
			var groupImage = (changePictureMessage.Clear ? null : changePictureMessage.Image.DecryptByTransaction(context.Transaction)?.ToUtf8String());
			if (!changePictureMessage.Clear && groupImage == null)
			{
				context.Logger.Log.LogDebug(Logger.InvalidBase64Format, "Invalid Base64 Format!");
				return;
			}

			context.Logger.Log.LogDebug(Logger.HandleGroupChangeImage, "ChangeGroupImage: Sender '{Address}' GroupOwnerAddress '{OwnerAddress}' Image '{GroupImage}'!",
				context.Sender, context.Receiver, groupImage);

			// Get chat
			var chat = context.DatabaseContext.GetChatAsync(context.Receiver, chats => chats.Include(c => c.Setting).Include(c => c.Users)).GetAwaiter().GetResult();
			if (chat?.Setting == null)
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupChangeImage, "Chat not found!");
				return;
			}

			// Check if user has permissions
			if (!chat.IsUserAdminOrHigher(context.Sender))
			{
				context.Logger.Log.LogDebug(Logger.HandleGroupChangeImage,
					"User in Chat not found or doesn't have the permissions.");
				return;
			}

			// Change image and notify all members
			if (chat.Setting.PictureUrl != groupImage)
			{
				chat.Setting.PictureUrl = groupImage;
				context.DatabaseContext.SaveChanges();

				context.ServiceProvider.GetService<FCMClient>()?.UpdateGroupAddress(chat);
			}
		}
	}
}
