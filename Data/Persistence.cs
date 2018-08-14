using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Newtonsoft.Json;
using UW.Models.Collections;

namespace UW.Data
{
    class Settings
    {
        public string Uri { get; set; }
        public string SecretKey { get; set; }
    }
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

        private Settings setting;
        private IConfiguration configuration;

        public readonly DocumentClient client;
        public Persistence(IConfiguration configuration)
        {
            this.configuration = configuration;

            this.setting = new Settings();
            configuration.Bind(SETTING_ROOT, setting);

            client = new DocumentClient(new Uri(setting.Uri), setting.SecretKey);

            //create database
            client.CreateDatabaseIfNotExistsAsync(new Database { Id = DB_NAME }).Wait();

            //create collections
            client.CreateDocumentCollectionIfNotExistsAsync(URI_DB,
                                new DocumentCollection { Id = COL_USER });
            client.CreateDocumentCollectionIfNotExistsAsync(URI_DB,
                                new DocumentCollection { Id = COL_SMSPCODE, DefaultTimeToLive = 30 });

            // test();
            // test_ttl();
        }

        public Models.Collections.User getUser(string phoneno)
        {
            var q = client.CreateDocumentQuery<Models.Collections.User>(URI_USER);
            var result = from user in q where user.phoneno == phoneno select user;

            return (result.Count() > 0) ? result.ToList().First() : null;
        }

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

                client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(DB_NAME, COL_USER, user.id), user);
            }
        }


    }


}
