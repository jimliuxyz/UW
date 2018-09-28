using System;
using System.Security.Cryptography;
using System.Text;

namespace UW.Shared
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
        public static string ToHash(this string context, string salt)
        {
            SHA256 sha256 = new SHA256CryptoServiceProvider();
            byte[] source = Encoding.Default.GetBytes(context + salt);
            byte[] crypto = sha256.ComputeHash(source);
            return Convert.ToBase64String(crypto);
        }
    }
}