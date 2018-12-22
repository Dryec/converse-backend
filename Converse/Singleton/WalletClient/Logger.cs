using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Converse.Singleton.WalletClient
{
	public class Logger
	{
		public const int SynchronizationDisabled = 1000;
		public const int InvalidBase64Format = 1001;

		// ParseTransaction
		public const int NewTransaction = 1050;
		public const int InvalidActionType = 1051;
		public const int InvalidJsonFormat = 1052;
		public const int ActionPropertyAddressInvalid = 1053;

		// SynchronizeBlocks
		public const int CannotSaveChanges = 1100;
		public const int LastSyncedBlockNotFound = 1101;

		// UpdateTokenData
		public const int TokenNotFound = 1150;

		// Handlers
		public const int HandleUserChangeNickname = 1500;
		public const int HandleUserChangeStatus = 1501;
		public const int HandleUserChangeProfilePicture = 1502;
		public const int HandleUserBlockedUser = 1503;
		public const int HandleUserSendMessage = 1504;

		public const int HandleGroupCreate = 1505;

		public ILogger Log { get; }

		public Logger(ILogger logger)
		{
			Log = logger;
		}
	}
}
