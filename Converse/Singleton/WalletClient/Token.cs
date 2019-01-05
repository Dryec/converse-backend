using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Protocol;
using Google.Protobuf;
using Microsoft.Extensions.Logging;

namespace Converse.Singleton.WalletClient
{
	public class Token
	{
		private readonly Client.WalletClient _walletClient;
		private readonly Logger _logger;
		private readonly string _name;

		public Token(string name, Logger logger, Client.WalletClient walletClient)
		{
			_name = name.ToLower();
			_logger = logger;
			_walletClient = walletClient;
		}
	}
}