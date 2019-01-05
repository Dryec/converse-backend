using System;
using Common;
using Crypto;

namespace Client
{
    public sealed class WalletAddress
    {
        private const byte AddressPrefixByte = 0x41;   //41 + address

        public static string Encode58Check(byte[] input)
        {
            var hash0 = Sha256.Hash(input);
            var hash1 = Sha256.Hash(hash0);
            var inputCheck = new byte[input.Length + 4];
            Array.Copy(input, 0, inputCheck, 0, input.Length);
            Array.Copy(hash1, 0, inputCheck, input.Length, 4);
            return Base58.Encode(inputCheck);
        }

        public static byte[] Decode58Check(string input)
        {
            byte[] decodeCheck;
            try
            {
                decodeCheck = Base58.Decode(input);
            }
            catch (Exception)
            {
                return null;
            }

            if (decodeCheck.Length <= 4)
            {
                return null;
            }

            var decodeData = new byte[decodeCheck.Length - 4];
            Array.Copy(decodeCheck, 0, decodeData, 0, decodeData.Length);

            var hash0 = Sha256.Hash(decodeData);
            var hash1 = Sha256.Hash(hash0);
            if (hash1[0] == decodeCheck[decodeData.Length] &&
                hash1[1] == decodeCheck[decodeData.Length + 1] &&
                hash1[2] == decodeCheck[decodeData.Length + 2] &&
                hash1[3] == decodeCheck[decodeData.Length + 3])
            {
                return decodeData;
            }

            return null;
        }

        public WalletAddress(ECKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            PublicBytes = key.Pub.GetEncoded();
        }

        public WalletAddress(string privateKey)
        {
            if (string.IsNullOrEmpty(privateKey) || privateKey.Length != 64)
            {
                throw new ArgumentException("message", nameof(privateKey));
            }

            PublicBytes = new ECKey(privateKey).Pub.GetEncoded();
        }

        public override string ToString()
        {
            var pubBytes64 = new byte[PublicBytes.Length-1];
            Array.Copy(PublicBytes, 1, pubBytes64, 0, pubBytes64.Length);

            var sha3Hash = Crypto.Sha3Keccack.CalculateHash(pubBytes64);
            var sha3HashBytes = new byte[21];

            sha3HashBytes[0] = AddressPrefixByte;
            Array.Copy(sha3Hash, 12, sha3HashBytes, 1, 20);

            var hash = Sha256.HashTwice(sha3HashBytes);
            var checkSum = new byte[4];
            Array.Copy(hash, checkSum, 4);

            var address = new byte[25];
            Array.Copy(sha3HashBytes, address, sha3HashBytes.Length);
            Array.Copy(checkSum, 0, address, sha3HashBytes.Length, checkSum.Length);

            return Base58.Encode(address);
        }

        public byte[] PublicBytes { get; }
    }
}
