using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Converse.Action
{
	enum EType
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

	interface IAction
	{
		EType Type { get; set; }
	}
}
