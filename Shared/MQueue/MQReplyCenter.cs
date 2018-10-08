using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using UW.Shared.Misc;
using UW.Shared.MQueue.Utils;

namespace UW.Shared.MQueue.Handlers
{
    public class MQReplyCenter
    {
        public static readonly string INSTANCE_ID = F.NewGuid(); //當下執行時隨機產生實體ID
        private static readonly string QUEUE_NAME = "reply";
        private static readonly string MSG_LABEL = "default";

        private static Dictionary<string, ResultWaiter> waiters = new Dictionary<string, ResultWaiter>();

        /// <summary>
        /// 取得建立一個`結果等待器`
        /// </summary>
        /// <param name="callerId">呼叫者</param>
        /// <returns></returns>
        public static ResultWaiter NewWaiter(string callerId)
        {
            lock (waiters)
            {
                var waiter = new ResultWaiter();
                waiters[callerId] = waiter;
                return waiter;
            }
        }

        public static void WakeUpWaiter(string callerId, dynamic data = null)
        {
            lock (waiters)
            {
                if (waiters.ContainsKey(callerId))
                {
                    waiters[callerId]?.wakeup(data);
                    waiters.Remove(callerId);
                }
            }
        }
        public static void CancelWaiter(string callerId)
        {
            WakeUpWaiter(callerId);
        }

        private static AzureSBus sender = AzureSBus.Builder(QUEUE_NAME).UseSession().UseSender().build();

        /// <summary>
        /// Send message reply to a `session queue`
        /// </summary>
        /// <param name="toInstanceId">給哪個執行實體</param>
        /// <param name="toCallerId">給執行實體中的哪個呼叫者</param>
        /// <returns></returns>
        public async static Task SendReply(string toInstanceId, string toCallerId, dynamic data = null)
        {
            await sender.Send(MSG_LABEL, data, session: toInstanceId, replyTo: toCallerId);
        }

        public static void StartReceiving(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var mqbus = AzureSBus.Builder(QUEUE_NAME, QUEUE_NAME + "-Receiver-" + i)
                    .SetReceiveMode(ReceiveMode.PeekLock)
                    .UseSession(INSTANCE_ID)
                    .AddMessageHandlerChain(MSG_LABEL, async (pack) =>
                    {
                        // Console.WriteLine("Got Reply...");
                        // Console.WriteLine(pack.data);
                    }, Flow1)
                    .SetPrefetchCount(10)
                    .build();
            }
        }

        private static async Task Flow1(HandlerPack pack)
        {
            // wake up the result waiter to end the query
            WakeUpWaiter(pack.message.ReplyTo, pack.data);

            // 'await' makes the reply queue slow!!
            /*await*/ pack.receiver.CompleteAsync(pack.message.SystemProperties.LockToken);
        }

    }
}

