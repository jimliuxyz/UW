using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using UW.Core.Misc;
using UW.Core.MQueue.Utils;

namespace UW.Core.MQueue.Handlers
{

    /**
    運作邏輯:
    reply queue啟動時監聽N組sessionId (INSTANCE_ID-0 ~ INSTANCE_ID-N)

    當任何訊息欲能回傳結果 (以下動作是運作在caller與handler兩個實體間)
    1. caller req送出前必須先透過`MQReplyCenter.GetReplySessionId()`取得一sessionId 設定到message.replyToSessionId
    2. 再透過`MQReplyCenter.NewWaiter()`取得一waiter 即可異步等待回傳結果
    3. handler處理完透過`MQReplyCenter.SendReply()`即可將訊息回傳到reply queue

     */

    public partial class MQReplyCenter
    {
        public static readonly string INSTANCE_ID = F.NewGuid(); //當下執行時隨機產生實體ID
        private static readonly string QUEUE_NAME = "reply";
        private static readonly string MSG_LABEL = "default";

        private static readonly int INSTANCE_COUNT = 10;
        private static readonly int PREFETCH_COUNT = 50;

        private static AzureSBus sender = AzureSBus.Builder(QUEUE_NAME).UseSession().UseSender(1).build();

        /// <summary>
        /// Send reply to a `session queue`
        /// </summary>
        /// <param name="toInstanceId">給哪個執行實體</param>
        /// <param name="toCallerId">給執行實體中的哪個呼叫者</param>
        /// <returns></returns>
        public async static Task SendReply(string toInstanceId, string toCallerId, dynamic data = null)
        {
            await sender.Send(MSG_LABEL, data, session: toInstanceId, replyTo: toCallerId);
        }
    }

    public partial class MQReplyCenter
    {
        private static List<string> insIds = new List<string>();

        /// <summary>
        /// Start Replay Receiver
        /// </summary>
        public static void Start()
        {
            for (int i = 0; i < INSTANCE_COUNT; i++)
            {
                insIds.Add(INSTANCE_ID + "-" + i);
            }
            StartReceiving();
        }

        /// <summary>
        /// get one of reply session as ID (random)
        /// </summary>
        /// <returns></returns>
        public static string GetReplySessionId()
        {
            return insIds[F.Random(0, insIds.Count)];
        }

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

        /// <summary>
        /// set data and then wake up waiter(caller)
        /// </summary>
        /// <param name="callerId"></param>
        /// <param name="data"></param>
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
        /// <summary>
        /// wake up waiter(caller) and do nothing
        /// </summary>
        /// <param name="callerId"></param>
        public static void CancelWaiter(string callerId)
        {
            WakeUpWaiter(callerId);
        }

        private static void StartReceiving()
        {
            var mqbus = AzureSBus.Builder(QUEUE_NAME, QUEUE_NAME + "-Receiver")
                .SetReceiveMode(ReceiveMode.PeekLock)
                .UseSession(insIds.ToArray())
                .AddMessageHandlerChain(MSG_LABEL, async (pack) =>
                {
                }, Flow1)
                .SetPrefetchCount(PREFETCH_COUNT)
                .build();
        }

        private static async Task Flow1(HandlerPack pack)
        {
            pack.param["return"] = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(pack.message.Body));

            // wake up the result waiter to end the query
            WakeUpWaiter(pack.message.ReplyTo, pack.param["return"]);

            // 'await' makes the reply queue slow!!
            /*await*/
            pack.receiver.CompleteAsync(pack.message.SystemProperties.LockToken);
        }

    }
}

