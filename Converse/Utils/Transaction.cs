using System;
using System.Linq;
using Crypto;
using Google.Protobuf;

namespace Converse.Utils
{
	public static class Transaction
	{
		public static byte[] GetPublicKey(this Protocol.Transaction transaction)
		{
			if (transaction.Signature.Count > 0)
			{
				try
				{
					return ECKey.RecoverPubBytesFromSignature(
						new ECDSASignature(transaction.Signature.ElementAt(0).ToByteArray()),
						Crypto.Sha256.Hash(transaction.RawData.ToByteArray()), false);
				}
				catch (Exception)
				{
					return null;
				}
			}

			return null;
		}
	}
}
