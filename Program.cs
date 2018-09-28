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
using UW.Models;
using UW.MQueue.TxReq;
using UW.Persis;
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

            // ProgramTest.Main2(new string[]{"-ConnectionString", "Endpoint=sb://uwqueue-sess.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=demykbfbZ98rIUI1wb5oU1SsPQIVYLwFp2Hr4GdhGD0=", "-QueueName", "myqueue"});

            // ProgramTest.MainAsync("Endpoint=sb://uwqueue.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=0WQP0IqqUV+h7qRsImJu3HFeFCTqmfVfQi+UPOMkq/0=", "myqueue2").Wait();
            // test().Wait();
            testdb().Wait();
            Console.WriteLine("end...");
            Console.ReadKey();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        public static async Task testdb()
        {
            await Base.BuildDB();

            var user = new MemberID(1, "qwer1234");

            var locker = new TxLockerHelper();
            await locker.TryLock(user);
        }

        public static async Task test()
        {
            var sender = await TxReqSender.get();

            Console.WriteLine("1");
            for (int i = 0; i < 10; i++)
            {
                // await sender.send(i);
            }
            Console.WriteLine("2");

            var receiver_ = await TxReqReceiver.get();
            await receiver_.StartPeek();

            // var receiver = await TxReqReceiver.get();
            Task.Run(async () =>
            {
                var receiver = await TxReqReceiver.get();
                receiver.StartReceive();
            });
            Task.Run(async () =>
            {
                var receiver = await TxReqReceiver.get();
                receiver.StartReceive();
            });

            // Task.Run(async () =>
            // {
            //     var receiver = await TxReqSessReceiver.get();
            //      receiver.StartReceive();
            // });
            // Task.Run(async () =>
            // {
            //     var receiver = await TxReqSessReceiver.get();
            //      receiver.StartReceive();
            // });
            await Task.Delay(1000000);

        }
    }
}
