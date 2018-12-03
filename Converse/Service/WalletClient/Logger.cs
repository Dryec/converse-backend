using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Converse.Service.WalletClient
{
	public class Logger
	{
		public const int SynchronizationDisabled = 1000;

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
		public const int HandleChangeNickname = 1500;
		public const int HandleChangeStatus = 1501;
		public const int HandleChangeProfilePicture = 1502;
		public const int HandleBlockedUser = 1503;
		public const int HandleSendMessage = 1504;

		public ILogger Log { get; }

		public Logger(ILogger logger)
		{
			Log = logger;
		}
	}
}
