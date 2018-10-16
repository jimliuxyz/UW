using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class HttpClientTester
    {
        private static Uri endPoint = new Uri("http://localhost:5000/api/test");

        public static async Task Start(int clientCount, int requestCount)
        {
            var watch = new Stopwatch();
            watch.Start();

            Console.WriteLine("HttpClientTester start");
            var tasks = new List<Task>();
            for (int i = 0; i < clientCount; i++)
            {
                var task = Task1(requestCount);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
            Console.WriteLine($"HttpClientTester done ({watch.Elapsed.TotalSeconds}s)===================");
        }

        private static async Task Task1(int requestCount)
        {
            RpcClient client = new RpcClient(endPoint);
            RpcRequest req = new RpcRequest(0, "testQueue");

            var tasks = new List<Task>();
            for (int i = 0; i < requestCount; i++)
            {
                var task = Task.Run(() =>
                {
                    // client.SendRequestAsync<dynamic>(req);
                    var res = client.SendRequestAsync<dynamic>(req).Result;
                    // Console.WriteLine(res.HasError ? res.Error.Message : res.Result);
                });
                tasks.Add(task);
            }

            var all = Task.WhenAll(tasks);
            if (await Task.WhenAny(all, Task.Delay(60000)) == all)
            {
                Console.WriteLine("compelet!");
            }
            else
            {
                Console.WriteLine("timeout!");
            }

        }
    }
}
