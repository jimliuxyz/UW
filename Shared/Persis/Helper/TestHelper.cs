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
    public class TestData
    {
        public string id;
        public string pk;
        public string payload;
    }
    public class TestHelper : BaseHelper
    {
        private static string DBNAME = R.DB_NAME;
        private static string COLNAME = "Testing";
        private string fixedPK = "fixedKey";
        private static List<String> guids = new List<String> { "pIrjwpB3cXAHXwFCsRGolR", "F7YFc5r2SKtlHH024FJGT1", "H9qtPn47BAGx1qtb7X6L1r", "btqt06faSQlfbe3awM3WS", "kzKt1pSir9t1T12PqpGzF7" };


        private readonly DocumentClient client;
        public static readonly PkuidGen IdGen = new PkuidGen("test").SetRandomRange(0, 9999999);
        public static readonly Uri uriCol = UriFactory.CreateDocumentCollectionUri(DBNAME, COLNAME);

        private static readonly string payload = "";
        static TestHelper()
        {
            var sbuilder = new StringBuilder("");
            while (sbuilder.Length < 10 * 1024)
            {
                sbuilder.Append(sbuilder.Length % 10);
            }
            payload = sbuilder.ToString();
            Console.WriteLine(Encoding.UTF8.GetBytes(payload).Length + "Bytes");
        }

        public static async Task QueryTest()
        {
            var helper = new TestHelper();

            // Console.WriteLine("---");
            // await helper.ReadAll();
            Console.WriteLine("---");
            await helper.ReadSqlAll();
            // Console.WriteLine("---");
            // await helper.ReadSqlId();
            Console.WriteLine("---");
            await helper.ReadSqlAllx5();
        }

        public static async Task CreateDoc()
        {
            var taskCount = 20;
            var list = new List<Task>();
            for (var i = 0; i < taskCount; i++)
            {
                list.Add(Task.Run(InsertDoc));
            }
            await Task.WhenAll(list.ToArray());
        }

        public static async Task InsertDoc()
        {
            var helper = new TestHelper();
            helper.GetPartition();

            var count = 50000;
            for (var i = 0; i < count; i++)
            {
                try
                {
                    await helper.Create();
                    Console.WriteLine($"{i}/{count}");
                }
                catch (DocumentClientException e)
                {
                    if (e.StatusCode == HttpStatusCode.Conflict)
                    {
                        i--;
                        continue;
                    }

                    throw;
                }
                if (i % 1000 == 0)
                    helper.GetPartition();
            }
            helper.GetPartition();
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
            Console.WriteLine("============");
        }

        public async Task Create()
        {
            var data = new
            {
                id = F.NewGuid(),
                pk = fixedPK,
                payload = payload,
            };

            // WriteToJsonFile<dynamic>("data.json", data);
            try
            {
                var opt = (fixedPK == null) ? new RequestOptions() : new RequestOptions { PartitionKey = new PartitionKey(fixedPK) };

                var res = await client.CreateDocumentAsync(uriCol, data, opt);
                Console.WriteLine(String.Format("Create data / RU: {0}", res.RequestCharge));
            }
            catch (DocumentClientException e)
            {
                Console.WriteLine(e.StatusCode);
                Console.WriteLine(e.Message);

                throw e;
            }
        }
        public async Task ReadAll()
        {
            // var client = GetNewClient();
            await client.ReadDocumentCollectionAsync(uriCol);

            var opt = (fixedPK == null) ? new RequestOptions() : new RequestOptions { PartitionKey = new PartitionKey(fixedPK) };

            foreach (var id in guids)
            {
                var docUri = UriFactory.CreateDocumentUri(DBNAME, COLNAME, id);

                var watch = new Stopwatch();
                watch.Start();

                var result = await client.ReadDocumentAsync<TestData>(docUri, opt);
                Console.WriteLine(String.Format("Read / RU: {0} / Elapsed: {1}", result.RequestCharge, watch.Elapsed.TotalSeconds));
            }
        }

        public async Task ReadSqlId()
        {
            // var client = GetNewClient();
            await client.ReadDocumentCollectionAsync(uriCol);

            var opt = (fixedPK == null) ? new FeedOptions() : new FeedOptions { PartitionKey = new PartitionKey(fixedPK) };

            foreach (var id in guids)
            {
                var watch = new Stopwatch();
                watch.Start();

                var result = await client.CreateDocumentQuery<TestData>(uriCol, opt)
                .Where(u => u.id == id)
                .Select(u => new
                {
                    id = u.id,
                    // payload = u.payload
                })
                .AsDocumentQuery()
                .ToListAsync();
                Console.WriteLine(String.Format("Elapsed: {0}", watch.Elapsed.TotalSeconds));
            }
        }


        public async Task ReadSqlAll()
        {
            // var client = GetNewClient();
            await client.ReadDocumentCollectionAsync(uriCol);

            var opt = (fixedPK == null) ? new FeedOptions() : new FeedOptions { PartitionKey = new PartitionKey(fixedPK) };

            foreach (var id in guids)
            {
                var watch = new Stopwatch();
                watch.Start();

                var result = await client.CreateDocumentQuery<TestData>(uriCol, opt)
                .Where(u => u.id == id)
                .Select(u => new
                {
                    id = u.id,
                    payload = u.payload
                })
                .AsDocumentQuery()
                .ToListAsync();
                Console.WriteLine(String.Format("Elapsed: {0}", watch.Elapsed.TotalSeconds));
            }
        }
        public async Task ReadSqlAllx5()
        {
            // var client = GetNewClient();
            await client.ReadDocumentCollectionAsync(uriCol);

            var opt = (fixedPK == null) ? new FeedOptions() : new FeedOptions { PartitionKey = new PartitionKey(fixedPK) };

            foreach (var id in guids)
            {
                var watch = new Stopwatch();
                watch.Start();

                var result = await client.CreateDocumentQuery<TestData>(uriCol, opt)
                .Where(u => guids.Contains(u.id))
                .Select(u => new
                {
                    id = u.id,
                    payload = u.payload
                })
                .AsDocumentQuery()
                .ToListAsync();
                Console.WriteLine(String.Format("Elapsed: {0}", watch.Elapsed.TotalSeconds));
            }
        }

    }
}