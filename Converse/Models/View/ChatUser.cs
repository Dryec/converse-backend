using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Converse.Models.View
{
	public class ChatUser : Models.View.User
	{
		public ChatUser(Models.ChatUser chatUser)
			: base(chatUser.User)
		{ }
	}
}
