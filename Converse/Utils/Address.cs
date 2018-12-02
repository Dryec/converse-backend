using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;

namespace Converse.Utils
{
	public class Address
	{
		public static string FromByteString(ByteString address)
		{
			return Client.WalletAddress.Encode58Check(address.ToByteArray());
		}
	}
}
