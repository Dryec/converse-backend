using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using Common;
using System;
using System.Linq;
using Org.BouncyCastle.Crypto.Signers;
using System.IO;
using Org.BouncyCastle.Asn1;
using System.Collections.Generic;

namespace Crypto
{
    public class ECKey
    {
        private const string CurveName = "secp256k1";

        public static readonly BigInteger HALF_CURVE_ORDER;
        public static readonly BigInteger CURVE_ORDER;
        private static readonly ECDomainParameters CURVE;
        private static readonly SecureRandom SecureRandom;
        private static readonly X9ECParameters Params;

        public ECPoint Pub { get; }
        private readonly ECPrivateKeyParameters _privateKey;
        private readonly ECPublicKeyParameters _pubKey;


        static ECKey()
        {
            Params = SecNamedCurves.GetByName(CurveName);
            CURVE = new ECDomainParameters(Params.Curve, Params.G, Params.N, Params.H);
            HALF_CURVE_ORDER = Params.N.ShiftRight(1);
            CURVE_ORDER = Params.N;
            SecureRandom = new SecureRandom();
        }

        public ECKey()
        {
            var parameters = new ECDomainParameters(Params.Curve, Params.G, Params.N, Params.H);
            var generator = new ECKeyPairGenerator();
            generator.Init(new ECKeyGenerationParameters(parameters, SecureRandom));
            var pair = generator.GenerateKeyPair();
            Pub = ((ECPublicKeyParameters)pair.Public).Q;
            _privateKey = (ECPrivateKeyParameters)pair.Private;
            _pubKey = (ECPublicKeyParameters)pair.Public;
        }


        public ECKey(BigInteger privateKey, ECPoint publicPoint)
        {
            _privateKey = new ECPrivateKeyParameters(privateKey, CURVE);
            Pub = publicPoint;
        }

        public ECKey(BigInteger privateKey) : this(privateKey, CURVE.G.Multiply(privateKey))
        {
        }

        public ECKey(byte[] privateKey) : this(new BigInteger(1, privateKey))
        {
        }

        public ECKey(string privateKey) : this(new BigInteger(privateKey, 16))
        {
        }

        public BigInteger GetPrivateKey()
        {
            return _privateKey.D;
        }

        public byte[] Sign(byte[] hash)
        {
            var signer = new DeterministicECDSA();
            signer.setPrivateKey(_privateKey);
            var sig = ECDSASignature.FromDER(signer.signHash(hash));
            var signature = sig.MakeCanonical();
            var recId = CalculateRecId(signature, hash);
            signature.V = new[] { (byte)(recId + 27) };

            var fixedV = (signature.V.First()) >= 27 ? (byte)(signature.V.First() - 27) : signature.V.First();

            return signature.R.BigIntegerToBytes(32)
                            .Concat(signature.S.BigIntegerToBytes(32))
                            .Concat(new byte[] { fixedV }).ToArray();

            // TTTTTUxA6VACn9sBAUpbJFvHv8XM7JuHiV
        }

        public bool Verify(byte[] hash, ECDSASignature sig)
        {
            var signer = new ECDsaSigner();
            signer.Init(false, _pubKey);
            return signer.VerifySignature(hash, sig.R, sig.S);
        }

        internal int CalculateRecId(ECDSASignature signature, byte[] hash)
        {
            var recId = -1;
            var thisKey = Pub.GetEncoded();

            for (var i = 0; i < 4; i++)
            {
                var rec = ECKey.RecoverPubBytesFromSignature(i, signature, hash, false);
                if (rec != null && rec.SequenceEqual(thisKey))
                {
                    recId = i;
                    break;
                }
            }
            if (recId == -1)
                throw new Exception("Could not construct a recoverable key. This should never happen.");
            return recId;
        }

        public static byte[] RecoverPubBytesFromSignature(int recId, ECDSASignature sig, byte[] message, bool compressed)
        {
            if (recId < 0)
                throw new ArgumentException("recId should be positive");
            if (sig.R.SignValue < 0)
                throw new ArgumentException("r should be positive");
            if (sig.S.SignValue < 0)
                throw new ArgumentException("s should be positive");
            if (message == null)
                throw new ArgumentNullException(nameof(message));


            var curve = CURVE;

            // 1.0 For j from 0 to h   (h == recId here and the loop is outside this function)
            //   1.1 Let x = r + jn

            var n = curve.N;
            var i = BigInteger.ValueOf((long)recId / 2);
            var x = sig.R.Add(i.Multiply(n));

            //   1.2. Convert the integer x to an octet string X of length mlen using the conversion routine
            //        specified in Section 2.3.7, where mlen = ⌈(log2 p)/8⌉ or mlen = ⌈m/8⌉.
            //   1.3. Convert the octet string (16 set binary digits)||X to an elliptic curve point R using the
            //        conversion routine specified in Section 2.3.4. If this conversion routine outputs “invalid”, then
            //        do another iteration of Step 1.
            //
            // More concisely, what these points mean is to use X as a compressed public key.

            //using bouncy and Q value of Point
            var prime = new BigInteger(1,
                Org.BouncyCastle.Utilities.Encoders.Hex.Decode(
                    "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFFC2F"));
            if (x.CompareTo(prime) >= 0)
                return null;

            // Compressed keys require you to know an extra bit of data about the y-coord as there are two possibilities.
            // So it's encoded in the recId.
            var R = DecompressKey(x, (recId & 1) == 1);
            //   1.4. If nR != point at infinity, then do another iteration of Step 1 (callers responsibility).

            if (!R.Multiply(n).IsInfinity)
                return null;

            //   1.5. Compute e from M using Steps 2 and 3 of ECDSA signature verification.
            var e = new BigInteger(1, message);
            //   1.6. For k from 1 to 2 do the following.   (loop is outside this function via iterating recId)
            //   1.6.1. Compute a candidate public key as:
            //               Q = mi(r) * (sR - eG)
            //
            // Where mi(x) is the modular multiplicative inverse. We transform this into the following:
            //               Q = (mi(r) * s ** R) + (mi(r) * -e ** G)
            // Where -e is the modular additive inverse of e, that is z such that z + e = 0 (mod n). In the above equation
            // ** is point multiplication and + is point addition (the EC group operator).
            //
            // We can find the additive inverse by subtracting e from zero then taking the mod. For example the additive
            // inverse of 3 modulo 11 is 8 because 3 + 8 mod 11 = 0, and -3 mod 11 = 8.

            var eInv = BigInteger.Zero.Subtract(e).Mod(n);
            var rInv = sig.R.ModInverse(n);
            var srInv = rInv.Multiply(sig.S).Mod(n);
            var eInvrInv = rInv.Multiply(eInv).Mod(n);
            var q = ECAlgorithms.SumOfTwoMultiplies(curve.G, eInvrInv, R, srInv);
            q = q.Normalize();
            if (compressed)
            {
                q = CURVE.Curve.CreatePoint(q.XCoord.ToBigInteger(), q.YCoord.ToBigInteger());
                return q.GetEncoded(true);
            }
            var xBytes = q.XCoord.ToBigInteger().ToByteArray();//.BigIntegerToBytes(32);
            var yBytes = q.YCoord.ToBigInteger().ToByteArray();//.BigIntegerToBytes(32);
            return q.GetEncoded(false);//xBytes.Concat(yBytes).ToArray();
        }

        private static ECPoint DecompressKey(BigInteger xBN, bool yBit)
        {
            var curve = CURVE.Curve;
            var compEnc = X9IntegerConverter.IntegerToBytes(xBN, 1 + X9IntegerConverter.GetByteLength(curve));
            compEnc[0] = (byte)(yBit ? 0x03 : 0x02);
            return curve.DecodePoint(compEnc);
        }

        public static ECKey FromPrivate(BigInteger privKey)
        {
            return new ECKey(privKey, new ECPublicKeyParameters(CURVE.G.Multiply(privKey), CURVE).Q);
        }

        public static ECKey FromPrivate(byte[] privKeyBytes)
        {
            return FromPrivate(new BigInteger(1, privKeyBytes));
        }

        public static ECKey FromPrivateHexString(string privateKeyStr)
        {
            var bytes = privateKeyStr.FromHexToByteArray();
            return FromPrivate(new BigInteger(1, bytes));
        }
    }
}
