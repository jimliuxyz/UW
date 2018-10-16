using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using UW.Shared.Misc;
using UW.Shared.Persis.Collections;
using User = UW.Shared.Persis.Collections.User;

namespace UW.Shared.Persis.Helper
{
    public class UserHelper : BaseHelper
    {
        private readonly DocumentClient client;
        // public static readonly PkuidGen IdGen = new PkuidGen("user").SetPkVolume(5);
        public static readonly PkuidGen IdGen = new PkuidGen("user").SetRandomRange(0, 9999999);
        private static readonly List<CurrencySettings> DefCurrencies = new List<CurrencySettings>();

        static UserHelper()
        {
            foreach (var item in new string[] { D.CNY, D.USD, D.BTC, D.ETH })
            {
                DefCurrencies.Add(new CurrencySettings
                {
                    name = item,
                    order = DefCurrencies.Count,
                    isDefault = true,
                    isVisible = false
                });
            }
            // Console.WriteLine("user count : " + GetClient().GetDocsCount(User._URI_COL));
        }

        public UserHelper()
        {
            client = GetClient();
        }

        public Pkuid GenUid()
        {
            // var amount = client.GetDocsCount(User._URI_COL);
            return IdGen.Generate(0);
        }

        /// <summary>
        /// Create user
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="jwtHash"></param>
        /// <param name="phoneno"></param>
        /// <param name="name"></param>
        /// <param name="avatar"></param>
        /// <returns></returns>
        public async Task<User> Create(Pkuid uid, string jwtHash, string phoneno, string name = "", string avatar = "")
        {
            // await client.ClearCollectionAsync(User._URI_COL);

            var user = new User()
            {
                userId = uid.ToString(),
                pk = uid.PK,
                jwtHash = jwtHash,
                phoneno = phoneno,
                name = name,
                avatar = avatar,
                alias = uid.PkIdx + "-" + F.NewShortGuid(),

                currencies = DefCurrencies,
            };

            try
            {
                var res = await client.CreateDocumentAsync(User._URI_COL, user, new RequestOptions
                {
                    PartitionKey = new PartitionKey(uid.PK)
                });
                Console.WriteLine(String.Format("Create User / RU: {0}", res.RequestCharge));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.Conflict)
                    Console.WriteLine("Conflict!! data:{0}", user.ToJson());
                else
                {
                    Console.WriteLine("================");
                    Console.WriteLine(e.Message);
                    await Task.Delay(10 * 60 * 1000);
                }

                throw e;
            }

            return user;
        }

        public async Task Update(User user)
        {
            var res = await client.ReplaceDocumentAsync(GetDocumentUri(user.userId), user);
            // return res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.Created;
        }

        private Uri GetDocumentUri(Pkuid uid)
        {
            return UriFactory.CreateDocumentUri(User._DB_NAME, User._COLLECTION_NAME, uid.Guid);
        }
        private Uri GetDocumentUri(string userId)
        {
            return UriFactory.CreateDocumentUri(User._DB_NAME, User._COLLECTION_NAME, userId);
        }

        public void GetPartition()
        {
            client.GetPartitionInfo(User._URI_COL);
        }
        public async Task<User> GetById(Pkuid uid)
        {
            try
            {
                var watch = new Stopwatch();
                watch.Start();
                var result = await client.ReadDocumentAsync<User>(GetDocumentUri(uid), new RequestOptions()
                {
                    PartitionKey = new PartitionKey(uid.PK)
                });
                Console.WriteLine(String.Format("GetById / RU: {0} / Elapsed: {1}", result.RequestCharge, watch.Elapsed.TotalSeconds));
                return result;
            }
            catch (DocumentClientException e)
            {
                //return null if not found
                if (e.StatusCode == HttpStatusCode.NotFound)
                    return null;
                throw;
            }
        }

        public async Task<List<User>> GetByIds(string[] userIds)
        {
            var pklist = new List<string>();
            foreach (var id in userIds)
            {
                var uid = IdGen.Parse(id);
                pklist.Add(uid.PK);
            }

            var result = await client.CreateDocumentQuery<User>(User._URI_COL,
                new FeedOptions { EnableCrossPartitionQuery = true })
                .Where(u => pklist.Contains(u.pk))
                .Where(u => userIds.Contains(u.userId))
                .AsDocumentQuery()
                .ToListAsync();

            return result;
        }

        // todo : improve low performance
        public async Task<User> GetByPhoneno(string phoneno)
        {
            var watch = new Stopwatch();
            watch.Start();

            var result = await client.CreateDocumentQuery<User>(User._URI_COL,
                new FeedOptions { MaxDegreeOfParallelism = -1, EnableCrossPartitionQuery = true })
                .Where(u => u.phoneno == phoneno)
                .AsDocumentQuery()
                .ExecuteNextAsync<User>();

            Console.WriteLine(String.Format("GetByPhoneno / RU: {0} / Elapsed: {1}", result.RequestCharge, watch.Elapsed.TotalSeconds));

            return result.FirstOrDefault();
        }
        // todo : improve low performance (考慮反正歸))
        public async Task<List<User>> GetByPhonenos(string[] phonenos)
        {
            var result = await client.CreateDocumentQuery<User>(User._URI_COL,
                new FeedOptions { MaxDegreeOfParallelism = -1, EnableCrossPartitionQuery = true })
                .Where(u => phonenos.Contains(u.phoneno))
                .AsDocumentQuery()
                .ToListAsync();

            return result;
        }

        // todo : deprecate this function
        public async Task<List<User>> GetFirst100()
        {
            var result = await client.CreateDocumentQuery<User>(User._URI_COL,
                new FeedOptions { EnableCrossPartitionQuery = true })
                // .OrderBy(u => u.createdTime)
                .Take(100)
                .AsDocumentQuery()
                .ToListAsync();

            return result;
        }

        // todo : improve low performance
        public async Task<bool> IsTokenAvailable(Pkuid uid, string tokenRnd)
        {
            var user = await GetById(uid);

            if (user == null || user.jwtHash == null)
                return false;

            var jwtHash = tokenRnd.ToHash();  //todo : each user should have their own signature(random seed) to verify

            return (jwtHash == user.jwtHash);
        }

        public async Task UpdateNtfInfo(Pkuid uid, PNS pns, string pnsRegId, string azureRegId)
        {
            var user = await GetById(uid);

            user.ntfInfo = new NtfInfo
            {
                pns = pns,
                pnsRegId = pnsRegId,
                azureRegId = azureRegId,
            };
            await Update(user);
        }

    }
}