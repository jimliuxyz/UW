using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using EdjCase.JsonRpc.Client;
using EdjCase.JsonRpc.Core;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UW.Shared;
using UW.Shared.Misc;
using UW.Shared.MQueue;
using UW.Shared.MQueue.Handlers;
using UW.Shared.Persis;
using UW.Shared.Persis.Helper;

namespace UW
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(150, 150);
            TestHelper.QueryTest().Wait();
        }
        // public static void Main(string[] args)
        // {
        //     ThreadPool.SetMinThreads(150, 150);
        //     TestCreateDoc().Wait();
        //     TestPkuid();

        //     StartQueueDaemon().Wait();

        //     // StartHttpTestClient();

        //     CreateWebHostBuilder(args).Build().Run();
        // }

        public static void StartHttpTestClient()
        {
            Task.Run(async () =>
            {
                await Task.Delay(3000);
                HttpClientTester.Start(1, 50);
            });
        }
        public static async Task StartQueueDaemon()
        {
            MQReplyCenter.Start();
            MQTesting1.StartReceiving(5);
            MQUserCreate.StartReceiving(1);
        }

        public static void TestPkuid()
        {
            var guid = UserHelper.IdGen.Generate(1000);
            Console.WriteLine(guid.ToJson());

            var guid2 = UserHelper.IdGen.Parse(guid.ToString());
            Console.WriteLine(guid2.ToJson());
        }

        public static async Task TestCreateDoc()
        {
            var taskCount = 20;
            var list = new List<Task>();
            for (var i = 0; i < taskCount; i++)
            {
                list.Add(Task.Run(InsertUser));
            }
            await Task.WhenAll(list.ToArray());
        }

        public static async Task InsertUser()
        {
            var helper = new UserHelper();
            helper.GetPartition();

            var count = 50000;
            for (var i = 0; i < count; i++)
            {
                try
                {
                    var guid = UserHelper.IdGen.Generate();
                    var phoneno = "XXX" + F.Random(100000000, 999999999);

                    await helper.Create(guid, "", phoneno);
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
            }
            helper.GetPartition();
        }
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

    }
}
