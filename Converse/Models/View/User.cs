using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Converse.Models.View
{
	public class UserStatus
	{
		[JsonProperty(PropertyName = "message")]
		public string Message { get; set; }

		[JsonProperty(PropertyName = "timestamp")]
		public DateTime StatusUpdatedAt { get; set; }
	}

	public class User
	{
		[JsonProperty(PropertyName = "id")]
		public int Id { get; }

		[JsonProperty(PropertyName = "address")]
		public string Address { get; }

		[JsonProperty(PropertyName = "name")]
		public string Name { get; }

		[JsonProperty(PropertyName = "image")]
		public string Image { get; }

		[JsonProperty(PropertyName = "public_key")]
		public string PublicKey { get; }

		[JsonProperty(PropertyName = "status")]
		public UserStatus Status { get; }

		public User(Models.User user)
		{
			Id = user.Id;
			Address = user.Address;
			Name = user.Nickname;
			Image = user.ProfilePictureUrl;
			PublicKey = user.PublicKey;

			if (!string.IsNullOrEmpty(user.Status))
			{
				Status = new UserStatus()
				{
					Message = user.Status,
					StatusUpdatedAt = user.StatusUpdatedAt,
				};
			}
		}
	}
}
