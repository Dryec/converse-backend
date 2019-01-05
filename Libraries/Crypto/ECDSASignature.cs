using System;
using System.IO;
using System.Linq;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Math;

namespace Crypto
{
    public class ECDSASignature
    {
        private const string InvalidDERSignature = "Invalid DER signature";

        public ECDSASignature(BigInteger r, BigInteger s, byte v)
        {
            R = r;
            S = s;
            V = new byte[] { v };
        }

        public ECDSASignature(BigInteger r, BigInteger s)
        {
            R = r;
            S = s;
        }

        public ECDSASignature(BigInteger[] rs)
        {
            R = rs[0];
            S = rs[1];
        }

        public ECDSASignature(byte[] sig) : this(new BigInteger(1, sig.Take(32).ToArray()), new BigInteger(1, sig.Skip(32).Take(32).ToArray()), sig.Last())
        {

        }

        public BigInteger R { get; }

        public BigInteger S { get; }

        public byte[] V { get; set; }

        public bool IsLowS => S.CompareTo(ECKey.HALF_CURVE_ORDER) <= 0;

        public static ECDSASignature FromDER(byte[] sig)
        {
            try
            {
                var decoder = new Asn1InputStream(sig);
                var seq = decoder.ReadObject() as DerSequence;
                if (seq == null || seq.Count != 2)
                    throw new FormatException(InvalidDERSignature);
                return new ECDSASignature(((DerInteger)seq[0]).Value, ((DerInteger)seq[1]).Value);
            }
            catch (Exception ex)
            {
                throw new FormatException(InvalidDERSignature, ex);
            }
        }

        public static bool IsValidDER(byte[] bytes)
        {
            try
            {
                FromDER(bytes);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     Enforce LowS on the signature
        /// </summary>
        public ECDSASignature MakeCanonical()
        {
            if (!IsLowS)
                return new ECDSASignature(R, ECKey.CURVE_ORDER.Subtract(S));
            return this;
        }

        /**
        * What we get back from the signer are the two components of a signature, r and s. To get a flat byte stream
        * of the type used by Bitcoin we have to encode them using DER encoding, which is just a way to pack the two
        * components into a structure.
        */

        public byte[] ToDER()
        {
            // Usually 70-72 bytes.
            var bos = new MemoryStream(72);
            var seq = new DerSequenceGenerator(bos);
            seq.AddObject(new DerInteger(R));
            seq.AddObject(new DerInteger(S));
            seq.Close();
            return bos.ToArray();
        }
    }
}
