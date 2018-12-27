using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Converse.Models.View
{
	public class ChatSetting
	{
		[JsonProperty(PropertyName = "id")]
		public int Id { get; set; }

		[JsonProperty(PropertyName = "address")]
		public string Address { get; set; }

		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "description")]
		public string Description { get; set; }

		[JsonProperty(PropertyName = "image")]
		public string Image { get; set; }

		[JsonProperty(PropertyName = "is_public")]
		public bool IsPublic { get; set; }

		[JsonProperty(PropertyName = "users")]
		public List<Models.View.User> Users;

		[JsonProperty(PropertyName = "public_key")]
		public string PublicKey { get; set; }

		[JsonProperty(PropertyName = "private_key")]
		public string PrivateKey { get; set; }

		public ChatSetting(Models.ChatSetting chatSetting, string requesterAddress = null, IEnumerable<Models.ChatUser> chatUsers = null, bool
			includeUsers = true)
		{
			Id = chatSetting.ChatId;
			Address = chatSetting.Address;

			Name = chatSetting.Name;
			Description = chatSetting.Description;
			Image = chatSetting.PictureUrl;

			PublicKey = chatSetting.PublicKey;
			PrivateKey = chatSetting.PrivateKey;

			IsPublic = chatSetting.IsPublic;

			if (chatUsers != null)
			{
				var chatUserArray = chatUsers as Models.ChatUser[] ?? chatUsers.ToArray();

				if (!IsPublic && requesterAddress != null)
				{
					PrivateKey = chatUserArray.FirstOrDefault(cu => cu.Address == requesterAddress)?.PrivateKey;
				}

				if (includeUsers)
				{
					Users = new List<User>();
					foreach (var chatUser in chatUserArray)
					{
						Users.Add(new Models.View.ChatUser(chatUser));
					}
				}
			}
		}
	}
}
