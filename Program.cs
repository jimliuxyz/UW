using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using EdjCase.JsonRpc.Client;
using EdjCase.JsonRpc.Core;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UW.Shared;
using UW.Shared.Misc;
using UW.Shared.MQueue;
using UW.Shared.MQueue.Handlers;
using UW.Shared.Persis;

namespace UW
{
    public class Program
    {
        public static void Main(string[] args)
        {
            TestPkuid();
            ThreadPool.SetMinThreads(1000, 1000);

            StartQueueDaemon().Wait();

            CreateWebHostBuilder(args).Build().Run();
        }

        public static void StartHttpTestClient()
        {
            Task.Run(async () =>
            {
                await Task.Delay(3000);
                HttpClientTester.Start(1);
            });
        }
        public static async Task StartQueueDaemon()
        {
            MQReplyCenter.Start();
            MQUserCreate.StartReceiving(1);
        }

        public static void TestPkuid()
        {
            var guid = UserHelper.IdGen.Generate(1000);
            Console.WriteLine(guid.ToJson());

            var guid2 = UserHelper.IdGen.Parse(guid.ToString());
            Console.WriteLine(guid2.ToJson());
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

    }
}
