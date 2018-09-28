using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Newtonsoft.Json;
using UW.Models.Collections;
using System.Net;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UW.Services;
using System.Threading.Tasks;
using System.Diagnostics;
using UW.Models;

namespace UW.Persis
{
    /// <summary>
    /// 交易動作資料鎖定
    /// </summary>
    public class TxLockerHelper : Base
    {
        private readonly DocumentClient client;
        public TxLockerHelper()
        {
            client = new DocumentClient(new Uri(R.DB_URI), R.DB_KEY);
        }

        public async Task<bool> TryLock(MemberID memberId)
        {
            var obj = new TxLocker()
            {
                id = F.NewGuid(),
                memId = memberId.ToString(),
                pk = "Vol-" + memberId.volume
            };
            Console.WriteLine(obj.ToJson());

            await client.CreateDocumentAsync(URI_TXLOCKER, obj);


            return false;
        }
        public async Task<bool> UnLock(MemberID memberId)
        {
            return false;
        }

    }
}
