using System;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UW.Shared.Persis.Collections;

namespace UW.Shared
{
    /// <summary>
    /// Shared Functions
    /// </summary>
    public static class F
    {
        /// <summary>
        /// Guid in base64 format
        /// </summary>
        /// <returns></returns>
        public static string NewGuid()
        {
            return Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("=", ""), "[/]", "-");
        }
    }

}

