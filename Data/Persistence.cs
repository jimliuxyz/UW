using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Newtonsoft.Json;
using UW.Models.Collections;
using System.Net;
using System.Collections.Generic;

namespace UW.Data
{
    class Settings
    {
        public string Uri { get; set; }
        public string SecretKey { get; set; }
    }

    /// <summary>
    /// 資料持久層
    /// </summary>
    public class Persistence
    {
        private static string SETTING_ROOT = "AzureCosmos";
        private static string DB_NAME = "UWallet";
        private static Uri URI_DB = UriFactory.CreateDatabaseUri(DB_NAME);

        //user
        private static string COL_USER = typeof(UW.Models.Collections.User).Name;
        private static Uri URI_USER = UriFactory.CreateDocumentCollectionUri(DB_NAME, COL_USER);

        //sms passcode
        private static string COL_SMSPCODE = typeof(SmsPasscode).Name;
        private static Uri URI_SMSPCODE = UriFactory.CreateDocumentCollectionUri(DB_NAME, COL_SMSPCODE);

        //sms passcode
        private static string COL_NOHUB = typeof(NoHubInfo).Name;
        private static Uri URI_NOHUB = UriFactory.CreateDocumentCollectionUri(DB_NAME, COL_NOHUB);

        private Settings setting;
        private IConfiguration configuration;

        public readonly DocumentClient client;
        public Persistence(IConfiguration configuration)
        {
            this.configuration = configuration;

            this.setting = new Settings();
            configuration.Bind(SETTING_ROOT, setting);

            //get database client as a connection
            client = new DocumentClient(new Uri(setting.Uri), setting.SecretKey);

            //create database
            client.CreateDatabaseIfNotExistsAsync(new Database { Id = DB_NAME }).Wait();

            //create collections
            var defReqOpts = new RequestOptions { OfferThroughput = 400 };
            client.CreateDocumentCollectionIfNotExistsAsync(URI_DB,
                                new DocumentCollection { Id = COL_USER }, defReqOpts);
            client.CreateDocumentCollectionIfNotExistsAsync(URI_DB,
                                new DocumentCollection { Id = COL_SMSPCODE, DefaultTimeToLive = 30 }, defReqOpts);
            client.CreateDocumentCollectionIfNotExistsAsync(URI_DB,
                                new DocumentCollection { Id = COL_NOHUB, DefaultTimeToLive = 30 }, defReqOpts);

            // test();
            // test_ttl();
        }

        /// <summary>
        /// 取得所有使用者
        /// </summary>
        /// <returns></returns>
        public List<Models.Collections.User> getUsers()
        {
            var q = client.CreateDocumentQuery<Models.Collections.User>(URI_USER);
            var result = from user in q select user;

            return result.ToList();
        }

        /// <summary>
        /// 以電話號碼取得使用者
        /// </summary>
        /// <param name="phoneno"></param>
        /// <returns></returns>
        public Models.Collections.User getUserByPhone(string phoneno)
        {
            var q = client.CreateDocumentQuery<Models.Collections.User>(URI_USER);
            var result = from user in q where user.phoneno == phoneno select user;

            return (result.Count() > 0) ? result.ToList().First() : null;
        }

        /// <summary>
        /// 以userId取得使用者
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Models.Collections.User getUserByUserId(string userId)
        {
            var q = client.CreateDocumentQuery<Models.Collections.User>(URI_USER);
            var result = from user in q where user.userId == userId select user;

            return (result.Count() > 0) ? result.ToList().First() : null;
        }

        /// <summary>
        /// 更新或新增使用者
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool upsertUser(Models.Collections.User user)
        {
            try
            {
                client.UpsertDocumentAsync(URI_USER, user).Wait();
                return true;
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return false;
        }

        /// <summary>
        /// 取得NoHubInfo
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public NoHubInfo getUserNoHubInfo(string userId)
        {
            var q = client.CreateDocumentQuery<NoHubInfo>(URI_NOHUB);
            var result = from user in q where user.ownerId == userId select user;

            return (result.Count() > 0) ? result.ToList().First() : null;
        }

        /// <summary>
        /// 更新或新增NoHubInfo
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool upsertNoHubInfo(NoHubInfo info)
        {
            try
            {
                var res = client.UpsertDocumentAsync(URI_NOHUB, info).Result;
                return res.StatusCode == HttpStatusCode.OK;
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return false;
        }

        private void test_ttl()
        {
            client.CreateDocumentAsync(URI_SMSPCODE,
                new SmsPasscode
                {
                    phoneno = "1234567890",
                    passcode = "3333"
                });
            Console.WriteLine("......");
        }

        private void test()
        {
            // client.CreateDocumentAsync(URI_USER,
            //     new Models.Collections.User
            //     {
            //         name = "Jimx"
            //     });

            var q = client.CreateDocumentQuery<Models.Collections.User>(URI_USER);
            var u = from f in q where f.name == "Jim" select f;
            // u = q.Where(f => f.Name == "Jim");

            Console.WriteLine("======");
            Console.WriteLine(u.ToString());
            Console.WriteLine(JsonConvert.SerializeObject(u.ToList(), Formatting.Indented));

            u = from f in q where f.name == "Jim2" select f;
            Console.WriteLine("======");
            Console.WriteLine(u.ToString());
            Console.WriteLine(JsonConvert.SerializeObject(u.ToList(), Formatting.Indented));

            foreach (var user in u)
            {
                Console.WriteLine(JsonConvert.SerializeObject(user, Formatting.Indented));

                user.phoneno = "123";

                client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(DB_NAME, COL_USER, user.userId), user);
            }
        }


    }


}
