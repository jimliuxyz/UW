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
    public class TestHelper : BaseHelper
    {
        private readonly DocumentClient client;
        public static readonly PkuidGen IdGen = new PkuidGen("test").SetRandomRange(0, 9999999);
        public static readonly Uri uriCol = UriFactory.CreateDocumentCollectionUri(R.DB_NAME, "Testing");

        public static string payload = "";
        static TestHelper()
        {
            var myStringBuilder = new StringBuilder("");
            // while (myStringBuilder.Length < 2 * 1024 * 1024)
            // {
            //     myStringBuilder.Append(F.NewGuid());
            // }
            while (myStringBuilder.Length < 10 * 1024)
            {
                myStringBuilder.Append(myStringBuilder.Length%10);
            }
            payload = myStringBuilder.ToString();
            Console.WriteLine(Encoding.UTF8.GetBytes(payload).Length + "Bytes");
        }
        public static void WriteToJsonFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
        {
            TextWriter writer = null;
            try
            {
                var contentsToWriteToFile = JsonConvert.SerializeObject(objectToWrite);
                writer = new StreamWriter(filePath, append);
                writer.Write(contentsToWriteToFile);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        public TestHelper()
        {
            client = GetClient();
        }

        public void GetPartition()
        {
            client.GetPartitionInfo(uriCol);
        }

        public async Task Create()
        {
            var pk = "fixedKey";

            var data = new
            {
                id = F.NewGuid(),
                pk = pk,
                payload = payload,
            };

            // WriteToJsonFile<dynamic>("data.json", data);

            try
            {
                var res = await client.CreateDocumentAsync(uriCol, data, new RequestOptions
                {
                    PartitionKey = new PartitionKey(pk)
                });
                Console.WriteLine(String.Format("Create data / RU: {0}", res.RequestCharge));
            }
            catch (DocumentClientException e)
            {
                Console.WriteLine(e.Message);

                throw e;
            }
        }

    }
}