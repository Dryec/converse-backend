using System;
using System.Collections.Generic;
using Client;
using Common;
using Crypto;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Protocol;

namespace Client
{
    public class Wallet
    {
        public ECKey ECKey { get; set; }

        private string _privateKey;
        public string PrivateKey
        {
            get
            {
                if (_privateKey != null)
                {
                    return _privateKey;
                }
                else if (ECKey != null)
                {
                    return ECKey.GetPrivateKey().ToHexString();
                }

                return string.Empty;
            }
            set => _privateKey = value;
        }

        private string _address;
        public string Address
        {
            get
            {
                if (_address != null)
                {
                    return _address;
                }
                else if (ECKey?.Pub != null)
                {
                    return new WalletAddress(ECKey).ToString();
                }
                else
                {
                    return string.Empty;
                }
            }

            set => _address = value;
        }

        public Wallet()
        {
            
        }

        public Wallet(bool generateEcKey)
        {
            if (generateEcKey)
            {
                ECKey = new ECKey();
            }
        }

        public Wallet(ECKey eCKey)
        {
            ECKey = eCKey;
        }

	    public byte[] DecryptData(byte[] data, byte[] publicKey)
		{
			return ECKey.Decrypt(data, publicKey);
		}


        public void SignTransaction(Transaction transaction, bool setTimestamp = true)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }
            
            if (ECKey != null)
            {
                transaction.SetTimestampToNow();

                var hash = Sha256.Hash(transaction.RawData.ToByteArray());

                var contracts = transaction.RawData.Contract;
                foreach (var contract in contracts)
                {
                    var signature = ECKey.Sign(hash);
                    transaction.Signature.Add(ByteString.CopyFrom(signature));
                }

            }
            else
            {
                throw new NullReferenceException("No valid ECKey");
            }
        }
    }
}
