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
		public string Id { get; }
		public string Name { get; }

		public Token(string id, string name, Logger logger, Client.WalletClient walletClient)
		{
			Id = id;
			Name = name;
			_logger = logger;
			_walletClient = walletClient;
		}
	}
}