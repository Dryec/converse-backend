using System;
using System.Text;
using System.Text.RegularExpressions;
using Org.BouncyCastle.Math;
using Protocol;

namespace Common
{
    public static class Utils
    {
        public enum PasswordScore
        {
            Blank = 0,
            VeryWeak = 1,
            Weak = 2,
            Medium = 3,
            Strong = 4,
            VeryStrong = 5
        }

        public static void SetTimestamp(this Transaction transaction, long millis)
        {
            transaction.RawData.Timestamp = millis;
        }

        public static void SetTimestampToNow(this Transaction transaction)
        {
            SetTimestamp(transaction, ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds());
        }

        public static string ToHexString(this byte[] bytes)
        {
            var hex = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString().ToUpper();
        }

        public static String ToHexString(this BigInteger privateKey)
        {
            return privateKey.ToByteArrayUnsigned().ToHexString();
        }

        public static byte[] FromHexToByteArray(this string input)
        {
            var numberChars = input.Length;
            var bytes = new byte[numberChars / 2];
            for (var i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(input.Substring(i, 2), 16);
            }
            return bytes;
        }

        /*
       * The regular BigInteger.ToByteArray method isn't quite what we often need:
       * it appends a leading zero to indicate that the number is positive and may need padding.
       *
       * @param bigInteger the integer to format into a byte array
       * @param numBytes the desired size of the resulting byte array
       * @return numBytes byte long array.
       */
        public static byte[] BigIntegerToBytes(this BigInteger bigInteger, int numBytes)
        {
            if(bigInteger == null)
            {
                return null;
            }

            var bytes = new byte[numBytes];
            var bigIntegerBytes = bigInteger.ToByteArray();
            var start = (bigIntegerBytes.Length == numBytes + 1) ? 1 : 0;
            var length = Math.Min(bigIntegerBytes.Length, numBytes);
            System.Buffer.BlockCopy(bigIntegerBytes, start, bytes, numBytes - length, length);
            return bytes;
        }



        /*public static String ToHexString(this BigInteger privateKey)
        {
            return privateKey.ToByteArrayUnsigned().ToHexString();
        }*/

       
        public static PasswordScore CheckPasswordStrength(string password)
        {
            var score = 0;

            if (password.Length > 0)
                score++;
            if (password.Length >= 4)
                score++;

            if (password.Length >= 6)
            {
                score++;
                if (password.Length >= 12)
                    score++;
                if (Regex.Match(password, @"/\d+/", RegexOptions.ECMAScript).Success)
                    score++;
                if (Regex.Match(password, @"/[a-z]/", RegexOptions.ECMAScript).Success &&
                  Regex.Match(password, @"/[A-Z]/", RegexOptions.ECMAScript).Success)
                    score++;
                if (Regex.Match(password, @"/.[!,@,#,$,%,^,&,*,?,_,~,-,£,(,)]/", RegexOptions.ECMAScript).Success)
                    score++;
            }

            return (PasswordScore)score;
        }
    }
}
