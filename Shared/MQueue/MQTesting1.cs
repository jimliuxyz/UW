using System;
using System.Collections;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using UW.Shared.MQueue.Utils;

namespace UW.Shared.MQueue.Handlers
{
    public class MQTesting1 : MQBase
    {
        private static readonly string QUEUE_NAME = "teseting";
        private static readonly string MSG_LABEL = "default";

        private static AzureSBus sender = AzureSBus.Builder(QUEUE_NAME).UseSender().build();

        public MQTesting1()
        {
        }

        public async static Task Send()
        {
            var data = new
            {
                msg = "hello",
            };

            await sender.Send(MSG_LABEL, data);
        }

        public async static Task<Object> SendAndWaitReply()
        {
            var data = new
            {
                msg = "hello",
            };

            // new a `result waiter` of MQReplyCenter to wait the result async 
            var replyTo = F.NewGuid();
            var waiter = MQReplyCenter.NewWaiter(replyTo);

            await sender.Send(MSG_LABEL, data, replyToSessionId: MQReplyCenter.INSTANCE_ID, replyTo: replyTo);

            var res = await waiter.wait();

            return res;
        }

        public async static Task CreateMessageHandler()
        {
            for (int i = 0; i < 2; i++)
            {
                var mqbus = AzureSBus.Builder(QUEUE_NAME, QUEUE_NAME+"-Receiver-" + i)
                    .SetReceiveMode(ReceiveMode.PeekLock)
                    .AddMessageHandlerChain(MSG_LABEL, async (pack) =>
                    {
                        // Console.WriteLine("start...");
                        // Console.WriteLine(pack.data);
                    }, Flow1, Flow2)
                    .build();
            }
        }

        private static async Task Flow1(HandlerPack pack)
        {
        }

        private static async Task Flow2(HandlerPack pack)
        {
            // reply to MQReplyCenter
            if (pack.message.ReplyToSessionId != null)
            {
                var replyData = new
                {
                    stationId = pack.stationId,
                    error = 123
                };
                await MQReplyCenter.SendReply(pack.message.ReplyToSessionId, pack.message.ReplyTo, replyData);
            }

            await pack.receiver.CompleteAsync(pack.message.SystemProperties.LockToken);
        }
    }
}

