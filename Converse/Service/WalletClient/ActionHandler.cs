﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Converse.Action;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Google.Protobuf;
using Protocol;

namespace Converse.Service.WalletClient
{
	public class ActionHandler
	{
		public DatabaseContext DatabaseContext { get; set; }

		private readonly Logger _logger;
		private readonly Token _token;

		public ActionHandler(Logger logger, Token token)
		{
			_logger = logger;
			_token = token;
		}

		public void Handle(TransactionExtention transaction, BlockExtention block)
		{
			// Parse transaction contract data
			if (transaction.Transaction.RawData.Contract.Count <= 0)
			{
				return;
			}

			var contract = transaction.Transaction.RawData.Contract[0];
			if (contract.Type != Transaction.Types.Contract.Types.ContractType.TransferAssetContract)
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

			DatabaseContext.CreateUsersWhenNotExist(new[] { senderAddress, receiverAddress });
			DatabaseContext.SaveChanges();

			// Get message + transactionHash
			var transactionHash = Common.Utils
				.ToHexString(Crypto.Sha256.Hash(transaction.Transaction.RawData.ToByteArray()))
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
					if (receiverAddress != WalletClient.PropertyAddress?.Address)
					{
						_logger.Log.LogDebug(Logger.ActionPropertyAddressInvalid, "This Action needs PropertyAddress as receiver!");
						return;
					}
				}

				var context = new Context()
				{
					Sender = senderAddress,
					Receiver = receiverAddress,
					Message = message,

					Transaction = transaction,
					TransactionHash = transactionHash,
					Block = block,
					
					DatabaseContext = DatabaseContext,
					Logger = _logger,
				};

				switch (action.Type)
				{
					case Action.Type.UserChangeNickname:
						ActionHandlers.UserChangeNickname.Handle(context);
						break;
					case Action.Type.UserChangeStatus:
						ActionHandlers.UserChangeStatus.Handle(context);
						break;
					case Action.Type.UserChangeProfilePicture:
						ActionHandlers.UserChangeProfilePicture.Handle(context);
						break;
					case Action.Type.UserBlockUser:
						ActionHandlers.UserBlockUser.Handle(context);
						break;
					case Action.Type.UserSendMessage:
						ActionHandlers.UserSendMessage.Handle(context);
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