using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Converse.Models;
using Chat = Converse.Constants.Chat;

namespace Converse.Utils
{
	public static class ChatExtension
	{
		public static bool IsGroup(this Models.Chat chat)
		{
			return (chat != null && chat.GetType() == Chat.Type.Group);
		}

		public static bool IsNormal(this Models.Chat chat)
		{
			return (chat != null && chat.GetType() == Chat.Type.Normal);
		}

		public static Models.ChatUser GetUser(this Models.Chat chat, string address)
		{
			return chat?.Users?.SingleOrDefault(u => u.Address == address);
		}

		public static bool HasUser(this Models.Chat chat, string address)
		{
			return (chat.GetUser(address) != null);
		}

		public static bool IsUserAdminOrHigher(this Models.Chat chat, string address)
		{
			var user = chat.GetUser(address);
			return (user != null && user.Rank >= ChatUserRank.Admin);
		}
	}
}
