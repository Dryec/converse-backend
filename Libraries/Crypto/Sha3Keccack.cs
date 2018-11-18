using System.Text;
using Common;
using Org.BouncyCastle.Crypto.Digests;

namespace Crypto
{
    public static class Sha3Keccack
    {
        public static string CalculateHash(string value)
        {
            var input = Encoding.UTF8.GetBytes(value);
            var output = CalculateHash(input);
            return output.ToHexString();
        }

        public static byte[] CalculateHash(byte[] value)
        {
            var digest = new KeccakDigest(256);
            var output = new byte[digest.GetDigestSize()];
            digest.BlockUpdate(value, 0, value.Length);
            digest.DoFinal(output, 0);
            return output;
        }
    }
}