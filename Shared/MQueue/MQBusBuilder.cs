using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;
using static UW.Shared.MQueue.MQBus;

namespace UW.Shared.MQueue
{
    public class MQBusBuilder
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

            var mqbus = MQBus.Builder(D.QN.TXREQ, "bus1")
                .SetReceiveMode(ReceiveMode.PeekLock)
                .UseSender()
                .AddMessageHandlerChain(label, TestHandler)
                .AddMessageHandlerChain(label, async (pack) =>
                {
                    Console.WriteLine("test2...");
                    Console.WriteLine(pack.payload);
                })
                .build();

            // await mqbus.Send(label, data);
        }

        private Dictionary<string, List<MQHandler>> messageHandlers = new Dictionary<string, List<MQHandler>>();
        private string queueName;
        private string busId;
        private bool useSession;
        private bool useSender;
        private int prefetchCount;
        private ReceiveMode receiveMode;
        private RetryPolicy receiveRetryPolicy;
        private RetryPolicy senderRetryPolicy;

        public MQBusBuilder(string queueName, string busId)
        {
            this.queueName = queueName;
            this.busId = busId;
        }


        public MQBusBuilder SetReceiveMode(ReceiveMode receiveMode)
        {
            this.receiveMode = receiveMode;
            return this;
        }
        public MQBusBuilder SetReceiveRetryPolicy(RetryPolicy receiveRetryPolicy)
        {
            this.receiveRetryPolicy = receiveRetryPolicy;
            return this;
        }
        public MQBusBuilder UseSession()
        {
            this.useSession = true;
            return this;
        }
        public MQBusBuilder UseSender()
        {
            this.useSender = true;
            return this;
        }
        public MQBusBuilder SetPrefetchCount(int prefetchCount)
        {
            this.prefetchCount = prefetchCount;
            return this;
        }
        public MQBusBuilder AddMessageHandlerChain(string msgType, MQHandler handler)
        {
            if (!messageHandlers.ContainsKey(msgType))
                messageHandlers[msgType] = new List<MQHandler>();

            messageHandlers[msgType].Add(handler);
            return this;
        }


        public MQBus build()
        {
            ClientEntity receiver, sender = null;

            if (useSession)
                receiver = new SessionClient(R.QUEUE_CSTR, queueName, receiveMode, receiveRetryPolicy, prefetchCount);
            else
                receiver = new MessageReceiver(R.QUEUE_CSTR, queueName, receiveMode, receiveRetryPolicy, prefetchCount);

            if (useSender)
                sender = new MessageSender(R.QUEUE_CSTR, queueName);


            var qmbus = new MQBus(queueName, busId, receiver, sender, messageHandlers);
            return qmbus;
        }



    }
}
