using System.Linq;
using Common;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Protocol;
using Converse.Utils;

namespace Converse.Singleton.WalletClient
{
	public class ActionHandler
	{
		public Service.DatabaseContext DatabaseContext { get; set; }

		private readonly Logger _logger;
		private readonly Token _token;

		public ActionHandler(Logger logger, Token token)
		{
			_logger = logger;
			_token = token;
		}

		public void Handle(TransactionExtention transaction, BlockExtention block, System.IServiceProvider serviceProvider)
		{
			// Parse transaction contract data
			if (transaction.Transaction.RawData.Contract.Count <= 0)
			{
				return;
			}

			var contract = transaction.Transaction.RawData.Contract[0];
			if (contract.Type != Protocol.Transaction.Types.Contract.Types.ContractType.TransferAssetContract)
			{
				return;
			}

			var transferAssetContract = contract.Parameter.Unpack<TransferAssetContract>();
			if (transferAssetContract.AssetName != _token.GetName())
			{
				return;
			}

			// Get the TRON-Public Address
			var senderAddress = Utils.Address.FromByteString(transferAssetContract.OwnerAddress);
			var receiverAddress = Utils.Address.FromByteString(transferAssetContract.ToAddress);

			var senderUser = DatabaseContext.CreateUsersWhenNotExist(new[] { senderAddress, receiverAddress }).Find(u => u.Address == senderAddress);
			if (string.IsNullOrEmpty(senderUser.PublicKey))
			{
				var publicKey = transaction.Transaction.GetPublicKey();
				if (publicKey != null)
				{
					senderUser.PublicKey = publicKey.ToHexString();
				}
			}
			DatabaseContext.SaveChanges();

			// Get message + transactionHash
			var transactionHash = Crypto.Sha256.Hash(transaction.Transaction.RawData.ToByteArray())
				.ToHexString()
				.ToLower();

			var message = transaction.Transaction.RawData.Data.ToStringUtf8();

			_logger.Log.LogDebug(Logger.NewTransaction, "Handle new Transaction with Hash '{TransactionHash}'!", transactionHash);

			try
			{
				var action = JsonConvert.DeserializeObject<Action.Action>(message);
				if (action == null)
				{
					return;
				}

				_logger.Log.LogDebug(Logger.NewTransaction, "Handle Action " + action.Type.ToString() + "!");

				// Actions only valid when sent to propertyAddress
				if (Action.Constants.PropertyAddressTypes.Contains(action.Type))
				{
					if (receiverAddress != Singleton.WalletClient.WalletClient.PropertyAddress?.Address)
					{
						_logger.Log.LogDebug(Logger.ActionPropertyAddressInvalid, "This Action needs PropertyAddress as receiver!");
						return;
					}
				}

				var context = new Action.Context()
				{
					Sender = senderAddress,
					Receiver = receiverAddress,
					Message = message,

					Transaction = transaction.Transaction,
					TransactionHash = transactionHash,
					Block = block,
					
					ServiceProvider = serviceProvider,
					DatabaseContext = DatabaseContext,
					Logger = _logger,
				};

				switch (action.Type)
				{
					case Action.Type.UserChangeNickname:
						Singleton.WalletClient.ActionHandlers.UserChangeNickname.Handle(context);
						break;
					case Action.Type.UserChangeStatus:
						Singleton.WalletClient.ActionHandlers.UserChangeStatus.Handle(context);
						break;
					case Action.Type.UserChangeProfilePicture:
						Singleton.WalletClient.ActionHandlers.UserChangeProfilePicture.Handle(context);
						break;
					case Action.Type.UserBlockUser:
						Singleton.WalletClient.ActionHandlers.UserBlockUser.Handle(context);
						break;
					case Action.Type.UserSendMessage:
						Singleton.WalletClient.ActionHandlers.UserSendMessage.Handle(context);
						break;
					case Action.Type.UserAddDeviceId:
						Singleton.WalletClient.ActionHandlers.UserAddDeviceId.Handle(context);
						break;
					case Action.Type.GroupCreate:
						break;
					case Action.Type.GroupChangeName:
						break;
					case Action.Type.GroupChangeDescription:
						break;
					case Action.Type.GroupChangePicture:
						break;
					case Action.Type.GroupAddUsers:
						break;
					case Action.Type.GroupKickUsers:
						break;
					case Action.Type.GroupSetUserRanks:
						break;
					case Action.Type.GroupJoin:
						break;
					case Action.Type.GroupLeave:
						break;
					case Action.Type.GroupMessage:
						break;
					case Action.Type.GroupSetPublic:
						break;
					default:
						_logger.Log.LogDebug(Logger.InvalidActionType, "Invalid ActionType({Type})!", action.Type);
						break;
				}
			}
			catch (Newtonsoft.Json.JsonException e)
			{
				_logger.Log.LogDebug(Logger.InvalidJsonFormat, "Could not parse JSON! Error: {Message}", e.Message);
			}
		}
	}
}