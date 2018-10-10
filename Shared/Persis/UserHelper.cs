using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using UW.Shared.Misc;
using UW.Shared.Persis.Collections;

namespace UW.Shared.Persis
{
    public class UserHelper : PersisBase
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
        }

        public UserHelper()
        {
            client = GetClient();
        }

        public async Task<Pkuid> GenUid()
        {
            // var amount = client.GetDocsCount(User._URI_COL);
            return IdGen.Generate(0);
        }
        static int cc = 0;
        public async Task<User> Create(Pkuid guid, string phoneno, string name = "", string avatar = "")
        {
            await client.ClearCollectionAsync(User._URI_COL);

            var user = new User()
            {
                userId = guid.ToString(),
                pk = guid.PK,
                phoneno = phoneno,
                name = name + (++cc),
                avatar = avatar,
                alias = guid.PkIdx + "-" + F.NewShortGuid(),

                currencies = DefCurrencies,
            };


            // var res = await client.CreateDocumentAsync(User._URI_COL, user, disableAutomaticIdGeneration: true);
            var res = await client.CreateDocumentIfNotExists(User._DB_NAME, User._COLLECTION_NAME, user.userId, user, user.pk);
            Console.WriteLine(String.Format("Create User / RU: {0}", res.RequestCharge));

            return user;
        }
    }
}