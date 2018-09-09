
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UW.Models.Collections;

namespace UW
{
    /// <summary>
    /// Shared Functions
    /// </summary>
    public static class F
    {
        public static string NewGuid()
        {
            return Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("=", ""), "[/]", "-");
        }

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

        public static string toJson(this object context)
        {
            return JsonConvert.SerializeObject(context, Formatting.Indented);
        }

        public static T DeepClone<T>(this T source)
        {
            if (Object.ReferenceEquals(source, null))
                return default(T);

            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
        }
    }

    /// <summary>
    /// ModelHelper
    /// </summary>
    public static class ModelHelper
    {
        /// <summary>
        /// 將Collection Model轉型為api回傳的結果
        /// </summary>
        public static dynamic toApiResult(this TxReceipt receipt)
        {
            return new
            {
                id = receipt.receiptId,
                datetime = receipt.datetime,
                currency = receipt.currency,
                message = receipt.message,
                statusCode = receipt.statusCode,
                statusMsg = receipt.statusMsg,
                txType = receipt.txType,
                txParams = receipt.txParams,
                txResult = receipt.txResult
            };
        }

        public static TxReceipt derivative(this TxReceipt receipt, string currency, string ownerId, TxActResult txResult)
        {
            var rec = receipt.DeepClone();
            rec.receiptId = F.NewGuid();
            rec.isParent = false;
            rec.parentId = receipt.receiptId;

            rec.currency = currency;
            rec.ownerId = ownerId;
            rec.txResult = txResult;
            return rec;
        }
    }
}

