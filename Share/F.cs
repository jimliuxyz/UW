
using System;
using System.Security.Cryptography;
using System.Text;

namespace UW
{
    /// <summary>
    /// Shared Functions
    /// </summary>
    public static class F
    {
    }

    /// <summary>
    /// Extensions
    /// </summary>
    public static class Extensions
    {
        public static string toHash(this string context, string salt)
        {
            SHA256 sha256 = new SHA256CryptoServiceProvider();
            byte[] source = Encoding.Default.GetBytes(context + salt);
            byte[] crypto = sha256.ComputeHash(source);
            return Convert.ToBase64String(crypto);
        }
    }
}

