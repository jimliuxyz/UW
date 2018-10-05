using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using UW.Shared.Misc;
using UW.Shared.Persis.Collections;

namespace UW.Shared.Persis
{
    public class UserHelper : PersisBase
    {
        private readonly DocumentClient client;
        private static readonly PkGuidGen guidGen = new PkGuidGen().SetPkVolume(5);
        public UserHelper()
        {
            client = GetClient();
        }

        public async Task<User> Create()
        {
            // var amount = client.GetDocsCount(User._URI_COL);
            // var guid = guidGen.Generate(amount);

            var user = new User();

            // user.userId = guid.ToString();
            // user.pk = ""+guid.PkIdx;
            // user.phoneno = F.NewGuid();
            // user.alias = guid.PkIdx + "-" + F.NewShortGuid();

            // Console.WriteLine("=======");
            // Console.WriteLine(user.ToJson());

            // var mem = guidGen.Parse(user.userId);
            // Console.WriteLine(mem.ToJson());


            // var res = await client.CreateDocumentAsync(User._URI_COL, user);
            // Console.WriteLine(String.Format("RU: {0}", res.RequestCharge));

            return user;
        }
    }
}