using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UW.Shared.Models;
using UW.Shared.MQueue.TxReq;
using UW.Shared.Persis;

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
            // test2().Wait();


            Console.WriteLine("end...");
            Console.ReadKey();
        }

        public static async Task test2()
        {
            string input = "123456";
            char[] chars = input.ToArray();
            Random r = new Random(259);
            for (int i = 0; i < chars.Length; i++)
            {
                int randomIndex = r.Next(0, chars.Length);
                char temp = chars[randomIndex];
                chars[randomIndex] = chars[i];
                chars[i] = temp;
            }
            string scrambled = new string(chars);
            // string scrambled_base64 = Regex.Replace(Convert.ToBase64String(Encoding.Default.GetBytes(scrambled)).Replace("=", ""), "[/]", "-");
            string scrambled_base64 = Regex.Replace(Convert.ToBase64String(BitConverter.GetBytes(500009342)).Replace("=", "="), "[/]", "/");

            Console.WriteLine(scrambled);
            Console.WriteLine(scrambled_base64);
            // Console.WriteLine(Convert.FromBase64String(scrambled_base64).);

            r = new Random(259);
            char[] scramChars = scrambled.ToArray();
            List<int> swaps = new List<int>();
            for (int i = 0; i < scramChars.Length; i++)
            {
                swaps.Add(r.Next(0, scramChars.Length));
            }
            for (int i = scramChars.Length - 1; i >= 0; i--)
            {
                char temp = scramChars[swaps[i]];
                scramChars[swaps[i]] = scramChars[i];
                scramChars[i] = temp;
            }

            string unscrambled = new string(scramChars);

            Console.WriteLine(scramChars);
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        public static async Task testdb()
        {
            await PersisBase.BuildDB();

            var user = new MemberID(1, "qwer1234");

            var locker = new TxLockerHelper();
            await locker.TryLock(user);
        }

        public static async Task test()
        {
            var sender = await TxReqSender.get();

            Console.WriteLine("1");
            for (int i = 0; i < 2; i++)
            {
                await sender.send(i);
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
