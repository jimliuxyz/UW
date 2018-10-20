using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using UW.Shared.Misc;
using UW.Shared.Persis.Collections;
using User = UW.Shared.Persis.Collections.User;

namespace UW.Shared.Persis.Helper
{
    public class BalanceHelper : PersisHelper
    {
        private readonly DocumentClient client;

        //todo : 要以user切pk
        public static readonly PkuidGen txIdGen = new PkuidGen("tx").SetRandomRange(0, 9999999);
        public static readonly PkuidGen receiptIdGen = new PkuidGen("receipt").SetRandomRange(0, 9999999);

        public BalanceHelper()
        {
            client = GetClient();
        }

        public void GetPartition()
        {
            client.GetPartitionInfo(BalanceV2._URI_COL);
        }

        private async Task<BalanceV2> Create(Pkuid uid)
        {
            var data = new BalanceV2
            {
                userId = uid.Guid,
                pk = uid.PK,
                balance = new Dictionary<string, decimal>{
                    {D.CNY, 0},
                    {D.USD, 0},
                    {D.BTC, 0},
                    {D.ETH, 0},
                }
            };
            try
            {
                var res = await client.CreateDocumentAsync(BalanceV2._URI_COL, data, new RequestOptions
                {
                    PartitionKey = new PartitionKey(uid.PK)
                });
                return data;
            }
            catch (DocumentClientException e)
            {
                Console.WriteLine(e.StatusCode);
                Console.WriteLine(e.Message);

                throw e;
            }
        }

        private async Task<BalanceV2> Get(Pkuid uid)
        {
            var result = await client.CreateDocumentQuery<BalanceV2>(BalanceV2._URI_COL,
                new FeedOptions { PartitionKey = new PartitionKey(uid.PK) })
                .Where(d => d.userId == uid.Guid)
                .AsDocumentQuery()
                .ExecuteNextAsync<BalanceV2>();

            if (result.Count == 0)
                return await Create(uid);

            return result.FirstOrDefault();
        }

        
    }
}