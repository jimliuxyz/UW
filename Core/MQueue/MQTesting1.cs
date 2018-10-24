using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using UW.Core.MQueue.Utils;

namespace UW.Core.MQueue.Handlers
{
    /// <summary>
    /// sender part
    /// </summary>
    public partial class MQTesting1
    {
        public static readonly string QUEUE_NAME = "teseting";
        private static readonly string MSG_LABEL = "default";

        private static AzureSBus sender = AzureSBus.Builder(QUEUE_NAME).UseSender(1).build();

        public async static Task Send()
        {
            var data = new
            {
                msg = "hello",
            };
            Console.WriteLine("cnt+1=" + Interlocked.Increment(ref cnt));
            await sender.Send(MSG_LABEL, data);
        }

        public async static Task<Object> SendAndWaitReply(int timeout = 1000)
        {
            var data = new
            {
                msg = "hello",
            };
            Console.WriteLine("cnt+1=" + Interlocked.Increment(ref cnt));
            return await MQUtils.SendAndWaitReply(sender, MSG_LABEL, data, timeout);
        }
    }

    /// <summary>
    /// receiver part
    /// </summary>
    public partial class MQTesting1
    {
        public static void StartReceiving(int count)
        {
            var tasks = new List<Task>();
            for (int i = 0; i < count; i++)
            {
                var task = Task.Run(() =>
                {
                    NewReceiver();
                });
                tasks.Add(task);
            }
            Task.WaitAll(tasks.ToArray());
        }

        private static int receiverCounter = 0;
        private static void NewReceiver()
        {
            var mqbus = AzureSBus.Builder(QUEUE_NAME, QUEUE_NAME + "-Receiver-" + Interlocked.Increment(ref receiverCounter))
                .SetReceiveMode(ReceiveMode.PeekLock)
                .SetPrefetchCount(5)
                .AddMessageHandlerChain(MSG_LABEL, async (pack) =>
                {
                    // Console.WriteLine("start...");
                    // Console.WriteLine(pack.data);
                }, Flow1, Flow2)
                .build();
        }

        public static int cnt = 0;
        private static async Task Flow1(HandlerPack pack)
        {
            Console.WriteLine("cnt-1=" + Interlocked.Decrement(ref cnt) + " : " + pack.stationId);
        }

        private static async Task Flow2(HandlerPack pack)
        {
            var data = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(pack.message.Body));

            // reply to MQReplyCenter
            if (pack.message.ReplyToSessionId != null)
            {
                var replyData = new
                {
                    stationId = pack.stationId,
                    threadHashCode = Thread.CurrentThread.GetHashCode(),
                    echo = data,
                    error = (object)null
                };
                /*await*/
                MQReplyCenter.SendReply(pack.message.ReplyToSessionId, pack.message.ReplyTo, replyData);
            }

            /*await*/
            pack.receiver.CompleteAsync(pack.message.SystemProperties.LockToken);
        }
    }
}

