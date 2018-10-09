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
            ThreadPool.SetMinThreads(1000, 1000);

            queueDaemon().Wait();

            Task.Run(async () =>
            {
                await Task.Delay(3000);
                HttpClientTester.Start(1);
            });

            CreateWebHostBuilder(args).Build().Run();

            // test().Wait();
            // testdb().Wait();
            // test2().Wait();

            // MQBusBuilder.Example().Wait();


            // Console.WriteLine("end...");
            // Console.ReadKey();
        }

        public static async Task queueDaemon()
        {
            Console.WriteLine(MQReplyCenter.INSTANCE_ID);
            MQTesting1.StartReceiving(20);
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

            // var user = new PkGuid(1, "qwer1234");

            // var locker = new TxLockerHelper();
            // await locker.TryLock(user);

            // var user2 = new PkGuid(1, "aaa");
            // await locker.TryLock(user2);


            var userHelper = new UserHelper();
            await userHelper.Create();
            // await userHelper.Create();
            // await userHelper.Create();

        }

    }
}
