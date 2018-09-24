using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BasicSendReceiveQuickStart;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UW.Services;

namespace UW
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // CreateWebHostBuilder(args).Build().Run();
            // var q = new QueueServ();
            // q.test().Wait();

            ProgramTest.Main2(new string[]{"-ConnectionString", "Endpoint=sb://uwqueue-sess.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=demykbfbZ98rIUI1wb5oU1SsPQIVYLwFp2Hr4GdhGD0=", "-QueueName", "myqueue"});

            // ProgramTest.MainAsync("Endpoint=sb://uwqueue.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=0WQP0IqqUV+h7qRsImJu3HFeFCTqmfVfQi+UPOMkq/0=", "myqueue2").Wait();

            Console.WriteLine("end...");
            Console.ReadKey();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
