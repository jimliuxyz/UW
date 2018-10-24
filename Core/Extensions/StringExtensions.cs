using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace UW.Core
{
    public static class StringExtensions
    {
        public static decimal ToNumber(this string str)
        {
            return decimal.Parse(str);
        }
        public static long ToLong(this string str)
        {
            return long.Parse(str);
        }
        public static string ToHash(this string context, string salt = "")
        {
            SHA256 sha256 = new SHA256CryptoServiceProvider();
            byte[] source = Encoding.Default.GetBytes(context + salt);
            byte[] crypto = sha256.ComputeHash(source);
            return Convert.ToBase64String(crypto);
        }

        public static string Scramble(this string context, int seed = 0)
        {
            char[] chars = context.ToArray();
            Random r = new Random(seed);
            for (int i = 0; i < chars.Length; i++)
            {
                int randomIndex = r.Next(0, chars.Length);
                char temp = chars[randomIndex];
                chars[randomIndex] = chars[i];
                chars[i] = temp;
            }
            return new string(chars);
        }


        public static string Descramble(this string context, int seed = 0)
        {
            char[] chars = context.ToArray();
            Random r = new Random(seed);
            for (int i = 0; i < chars.Length; i++)
            {
                int randomIndex = r.Next(0, chars.Length);
                char temp = chars[randomIndex];
                chars[randomIndex] = chars[i];
                chars[i] = temp;
            }
            return new string(chars);
        }

        public static int GetSum(this string guid)
        {
            return guid.ToCharArray().Aggregate(0, (sum, next) =>
            {
                return sum + next;
            });
        }
    }
}