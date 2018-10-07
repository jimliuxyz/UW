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
    public class HttpClientTester
    {
        public static async Task Start()
        {
            Console.WriteLine("HttpClientTester start");
            var list = new List<Thread>();
            for (int i = 0; i < 11; i++)
            {
                var th = new Thread(() =>
                {
                    RpcClient client = new RpcClient(new Uri("http://localhost:5000/api/test"));
                    RpcRequest req = new RpcRequest(0, "test");
                    var response = client.SendRequestAsync<dynamic>(req).Result;

                    Console.WriteLine(response.Result);
                });

                th.Start();
                list.Add(th);
            }

            foreach (var th in list)
            {
                th.Join();
            }
            Console.WriteLine("HttpClientTester done");
        }

    }
}
