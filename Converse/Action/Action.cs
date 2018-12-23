using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Protocol;

namespace Converse.Action
{
	public enum Type : uint
	{
		// User Actions
		UserChangeNickname = 1,
		UserChangeStatus,
		UserChangeProfilePicture,
		UserBlockUser,
		UserSendMessage,
		UserAddDeviceId,

		// Group Actions
		GroupCreate = 50,
		GroupChangeName,
		GroupChangeDescription,
		GroupChangePicture,
		GroupAddUser,
		GroupKickUser,
		GroupSetUserRank,
		GroupJoin,
		GroupLeave,
		GroupMessage,
		//GroupSetPublic,
	}

	public static class Constants
	{
		public static readonly Type[] PropertyAddressTypes = {
			Type.UserChangeNickname,
			Type.UserChangeStatus,
			Type.UserChangeProfilePicture,
			Type.UserBlockUser,
			Type.UserAddDeviceId,

			Type.GroupCreate,
		};
	}

	public class Context
	{
		public string Sender { get; set; }
		public string Receiver { get; set; }

		public string Message { get; set; }

		public string TransactionHash { get; set; }
		public Transaction Transaction { get; set; }
		public BlockExtention Block { get; set; }

		public IServiceProvider ServiceProvider { get; set; }
		public Database.DatabaseContext DatabaseContext { get; set; }
		public Singleton.WalletClient.Logger Logger { get; set; }
	}

	public class Action
	{
		[JsonProperty(Required = Required.Always)]
		public Type Type { get; set; }
	}
}
