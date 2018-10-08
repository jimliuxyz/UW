using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using UW.Shared.MQueue.Utils;

namespace UW.Shared.MQueue.Handlers
{
    /// <summary>
    /// sender part
    /// </summary>
    public partial class MQTesting1
    {
        private static readonly string QUEUE_NAME = "teseting";
        private static readonly string MSG_LABEL = "default";

        private static AzureSBus sender = AzureSBus.Builder(QUEUE_NAME).UseSender().build();

        static Dictionary<string, string> dict = new Dictionary<string, string>();
        public async static Task Send()
        {
            var data = new
            {
                msg = "hello",
            };
            lock (QUEUE_NAME)
            {
                ++cnt;
            }
            await sender.Send(MSG_LABEL, data);
        }

        public async static Task<Object> SendAndWaitReply(int timeout = 1000)
        {
            var data = new
            {
                msg = "hello",
            };
            lock (QUEUE_NAME)
            {
                ++cnt;
            }
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
            for (int i = 0; i < count; i++)
            {
                var mqbus = AzureSBus.Builder(QUEUE_NAME, QUEUE_NAME + "-Receiver-" + i)
                    .SetReceiveMode(ReceiveMode.PeekLock)
                    .AddMessageHandlerChain(MSG_LABEL, async (pack) =>
                    {
                        // Console.WriteLine("start...");
                        // Console.WriteLine(pack.data);
                    }, Flow1, Flow2)
                    .SetPrefetchCount(5)
                    .build();
            }
        }

        static int cnt = 0;
        private static async Task Flow1(HandlerPack pack)
        {
            lock (QUEUE_NAME)
            {
                Console.WriteLine("" + (--cnt) + " : " + pack.stationId);
            }

            // await Task.Delay(2000);
        }

        private static async Task Flow2(HandlerPack pack)
        {
            // reply to MQReplyCenter
            if (pack.message.ReplyToSessionId != null)
            {
                var replyData = new
                {
                    stationId = pack.stationId,
                    threadHashCode = Thread.CurrentThread.GetHashCode(),
                    echo = pack.data.msg,
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

