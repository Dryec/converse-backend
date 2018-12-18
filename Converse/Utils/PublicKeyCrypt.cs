using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Converse.Singleton.WalletClient;

namespace Converse.Utils
{
	public static class PublicKeyCrypt
	{
		public static byte[] DecryptByTransaction(this string data, Protocol.Transaction transaction)
		{
			var publicKey = transaction.GetPublicKey();
			return (publicKey == null ? null : data.DecryptByPublicKey(publicKey));
		}

		public static byte[] DecryptByPublicKey(this string data, byte[] publicKey)
		{
			try
			{
				return WalletClient.PropertyAddress.DecryptData(data.DecodeBase64(), publicKey);
			}
			catch (FormatException)
			{
				return null;
			}
		}

		public static string EncodeBase64(this byte[] data)
		{
			return Convert.ToBase64String(data);
		}

		public static string EncodeBase64(this string data)
		{
			return System.Text.Encoding.UTF8.GetBytes(data).EncodeBase64();
		}

		public static byte[] DecodeBase64(this string data)
		{
			return Convert.FromBase64String(data);
		}

		public static string ToUtf8String(this byte[] data)
		{
			return System.Text.Encoding.UTF8.GetString(data);
		}
	}
}
