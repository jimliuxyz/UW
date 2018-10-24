using System;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UW.Core.Misc;
using UW.Core.Persis.Collections;

namespace UW.Core
{
    /// <summary>
    /// Shared Functions
    /// </summary>
    public static class F
    {
        private static readonly Base62Converter B62 = new Base62Converter(1454); //default base62
        public static readonly Random Rnd = new Random();   //default

        /// <summary>
        /// Guid in base64 format
        /// </summary>
        /// <returns></returns>
        public static string NewGuid()
        {
            // return Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("=", ""), "[/]", "-");
            return B62.Encode(Guid.NewGuid().ToByteArray());
        }

        public static string NewShortGuid()
        {
            // return Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("=", ""), "[/]", "-");
            return NewGuid().Substring(0, 8);
        }

        public static int Random(int minValue, int maxValue)
        {
            return Rnd.Next(minValue, maxValue);
        }
    }

}

