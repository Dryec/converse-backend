using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Converse.Models.View
{
	public class ChatSetting
	{
		public int Id { get; set; }
		public string Address { get; set; }
		public string Name { get; set; }
		public string Image { get; set; }

		public List<Models.View.User> Users;

		public ChatSetting(Models.ChatSetting chatSetting, IEnumerable<Models.ChatUser> chatUsers = null)
		{
			Id = chatSetting.ChatId;
			Address = chatSetting.Address;
			Name = chatSetting.Name;
			Image = chatSetting.PictureUrl;

			Users = new List<User>();
	
			if (chatUsers != null)
			{
				foreach (var chatUser in chatUsers)
				{
					Users.Add(new Models.View.ChatUser(chatUser));
				}
			}
		}
	}
}
