using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;
using UW.MQueue;
using UW.MQueue.Messages;

namespace UW.MQueue.TxReq
{
    public class TxReqSessReceiver
    {
        private static readonly string QNAME = D.QN.TXREQ;
        public static async Task<TxReqSessReceiver> get()
        {
            await MQHelper.CreateQueue(QNAME);

            return new TxReqSessReceiver(new SessionClient(R.QUEUE_CSTR, QNAME, ReceiveMode.PeekLock, null, 1));
        }

        private SessionClient client;
        private TxReqSessReceiver(SessionClient client)
        {
            this.client = client;
            // StartReceive();
        }

        public async Task StartPeek()
        {
            Console.WriteLine("StartPeek 1");
            // while (true)
            // {
            //     Message message = await client.PeekAsync();
            //     if (message == null)
            //         break;

            //     Console.WriteLine(Encoding.UTF8.GetString(message.Body));

            //     // Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
            // }
            Console.WriteLine("StartPeek 2");
        }

        static int cnt = 0;
        public async Task StartReceive()
        {
            Console.WriteLine("StartReceive 1");
            var client_ = await client.AcceptMessageSessionAsync("test");

            var id = 0;
            lock (QNAME)
            {
                id = ++cnt;
            }

            while (true)
            {
                Message message = await client_.ReceiveAsync();
                if (message == null)
                    break;
                Console.WriteLine("ID" + this.GetHashCode() + " : " + Encoding.UTF8.GetString(message.Body));
                // await Task.Delay(5000);

                await client_.CompleteAsync(message.SystemProperties.LockToken);
                // await client_.CloseAsync();
                break;
            }
            Console.WriteLine("StartReceive 2");
        }

    }
}