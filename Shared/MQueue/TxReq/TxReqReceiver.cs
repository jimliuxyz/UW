using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;
using UW.Shared;

namespace UW.Shared.MQueue.TxReq
{
    public class TxReqReceiver
    {
        private static readonly string QNAME = D.QN.TXREQ;
        public static async Task<TxReqReceiver> get()
        {
            await MQHelper.CreateQueue(QNAME);

            return new TxReqReceiver(new MessageReceiver(R.QUEUE_CSTR, QNAME, ReceiveMode.PeekLock, null, 5));
        }

        private MessageReceiver client;
        private TxReqReceiver(MessageReceiver client)
        {
            this.client = client;
            // StartReceive();
        }

        public async Task StartPeek()
        {
            Console.WriteLine("StartPeek 1");
            while (true)
            {
                Message message = await client.PeekAsync();
                if (message == null)
                    break;

                Console.WriteLine(Encoding.UTF8.GetString(message.Body));
                // Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
            }
            Console.WriteLine("StartPeek 2");
        }
        public async Task StartReceive()
        {
            Console.WriteLine("StartReceive " + this.GetHashCode());
            while (true)
            {
                // Console.WriteLine("next...");

                Message message = await client.ReceiveAsync();

                Console.WriteLine("ID" + this.GetHashCode() + " : " + Encoding.UTF8.GetString(message.Body));

                await Task.Delay(new Random().Next(100, 1000));

                dynamic data = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(message.Body));

                // if (data.type == 5)
                {
                    await Task.Delay(1000);
                    Console.WriteLine("pass...");

                }
                try
                {
                    await client.CompleteAsync(message.SystemProperties.LockToken);

                }
                catch (System.Exception e)
                {

                    Console.WriteLine(e.Message);
                    // throw;
                }
            }
            Console.WriteLine("StartReceive 2");
        }

    }
}