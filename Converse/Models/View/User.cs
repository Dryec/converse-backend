using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Converse.Models.View
{
	public class UserStatus
	{
		public string Message { get; set; }
		[JsonProperty(PropertyName = "timestamp")]
		public DateTime StatusUpdatedAt { get; set; }
	}

	public class User
	{
		public int Id { get; }
		public string Address { get; }
		public string Name { get; }
		public string Image { get; }
		public UserStatus Status { get; }

		public User(Models.User user)
		{
			Id = user.Id;
			Address = user.Address;
			Name = user.Nickname;
			Image = user.ProfilePictureUrl;

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
