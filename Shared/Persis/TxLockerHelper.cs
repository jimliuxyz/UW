using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Newtonsoft.Json;
using UW.Shared.Persis.Collections;
using System.Net;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using UW.Shared.Misc;

namespace UW.Shared.Persis
{
    /// <summary>
    /// 交易動作資料鎖定
    /// </summary>
    public class TxLockerHelper : PersisBase
    {
        private readonly DocumentClient client;
        public TxLockerHelper()
        {
            client = GetClient();
        }

        public async Task<bool> TryLock(Pkuid memberId)
        {
            var obj = new TxLocker()
            {
                id = F.NewGuid(),
                memId = memberId.ToString()+F.NewGuid(),
                pk = "Vol-" + F.NewGuid()
            };
            Console.WriteLine(obj.ToJson());

            var res = await client.CreateDocumentAsync(URI_TXLOCKER, obj);
            Console.WriteLine(String.Format("RU: {0}", res.RequestCharge));


            Console.WriteLine(String.Format("GetDocsCount: {0}", client.GetDocsCount(TxLocker._URI_COL)));

            return false;
        }
        public async Task<bool> UnLock(Pkuid memberId)
        {
            return false;
        }

    }
}
