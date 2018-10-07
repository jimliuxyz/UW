using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;
using static UW.Shared.MQueue.Utils.AzureSBus;

namespace UW.Shared.MQueue.Utils
{
    public class AzureSBusBuilder
    {
        public static async Task TestHandler(HandlerPack pack)
        {
            Console.WriteLine("test1...");
        }

        public async static Task Example()
        {
            Console.WriteLine("Example...");

            var label = "test";
            var data = new
            {
                type = 1,
                msg = "hello",
                uid = F.NewGuid()
            };

            var mqbus = AzureSBus.Builder(D.QN.TXREQ, "bus1")
                .SetReceiveMode(ReceiveMode.PeekLock)
                .UseSender()
                .AddMessageHandlerChain(label, TestHandler)
                .AddMessageHandlerChain(label, async (pack) =>
                {
                    Console.WriteLine("test2...");
                    Console.WriteLine(pack.data);
                })
                .build();

            // await mqbus.Send(label, data);
        }

        private Dictionary<string, List<MQHandler>> messageHandlers = new Dictionary<string, List<MQHandler>>();
        private string queueName;
        private string busId;
        private bool useSession;
        private List<string> sessionIds = new List<string>();
        private bool useSender;
        private int prefetchCount;
        private ReceiveMode receiveMode;
        private RetryPolicy receiveRetryPolicy = RetryPolicy.Default;
        private RetryPolicy senderRetryPolicy = RetryPolicy.Default;

        public AzureSBusBuilder(string queueName, string busId)
        {
            this.queueName = queueName;
            this.busId = busId;
        }

        public AzureSBusBuilder SetReceiveMode(ReceiveMode receiveMode)
        {
            this.receiveMode = receiveMode;
            return this;
        }
        public AzureSBusBuilder SetReceiveRetryPolicy(RetryPolicy receiveRetryPolicy)
        {
            this.receiveRetryPolicy = receiveRetryPolicy;
            return this;
        }
        public AzureSBusBuilder UseSession(params string[] sessionIds)
        {
            useSession = true;
            this.sessionIds.AddRange(sessionIds);
            return this;
        }
        public AzureSBusBuilder UseSender()
        {
            this.useSender = true;
            return this;
        }
        public AzureSBusBuilder SetPrefetchCount(int prefetchCount)
        {
            this.prefetchCount = prefetchCount;
            return this;
        }
        public AzureSBusBuilder AddMessageHandlerChain(string label, params MQHandler[] handlers)
        {
            if (!messageHandlers.ContainsKey(label))
                messageHandlers[label] = new List<MQHandler>();

            messageHandlers[label].AddRange(handlers);
            return this;
        }

        public AzureSBus build()
        {
            var qmbus = new AzureSBus(queueName, busId, useSession, receiveRetryPolicy, senderRetryPolicy, receiveMode, prefetchCount, sessionIds, messageHandlers);
            return qmbus;
        }



    }
}
