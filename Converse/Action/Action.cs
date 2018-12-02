using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Converse.Action
{
	public enum Type
	{
		// User Actions
		UserChangeNickname = 1,
		UserChangeStatus,
		UserChangeProfilePicture,
		UserBlockUser,
		UserSendMessage,

		// Group Actions
		GroupCreate = 50,
		GroupChangeName,
		GroupChangeDescription,
		GroupChangePicture,
		GroupAddUsers,
		GroupKickUsers,
		GroupSetUserRanks,
		GroupJoin,
		GroupLeave,
		GroupMessage,
	}

	public class Action
	{
		[JsonProperty(Required = Required.Always)]
		public Type Type { get; set; }
	}
}
