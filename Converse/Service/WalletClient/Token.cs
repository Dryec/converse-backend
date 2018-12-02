using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Protocol;
using Google.Protobuf;
using Microsoft.Extensions.Logging;

namespace Converse.Service.WalletClient
{
	public class Token
	{
		private readonly Client.WalletClient _walletClient;
		private readonly Logger _logger;
		private readonly string _name;

		private AssetIssueContract _data;

		public Token(string name, Logger logger, Client.WalletClient walletClient)
		{
			_name = name;
			_logger = logger;
			_walletClient = walletClient;
		}

		public async Task<bool> Update()
		{
			if (!IsValid())
			{
				_data = await _walletClient.GetAssetIssueByNameAsync(new BytesMessage()
				{
					Value = ByteString.CopyFrom(_name, Encoding.ASCII)
				});

				if (_data.OwnerAddress.Length == 0)
				{
					_logger.Log.LogCritical(Logger.TokenNotFound, "Token with the name '{TokenName} not found!", _name);
					return false;
				}
			}

			return true;
		}

		public ByteString GetName()
		{
			return (IsValid() ? _data.Name : null);
		}

		public bool IsValid()
		{
			return _data != null;
		}
	}
}