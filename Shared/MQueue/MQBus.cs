using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;

namespace UW.Shared.MQueue
{
    public class MQBus
    {
        public delegate Task MQHandler(HandlerPack msgPack);
        public static MQBusBuilder Builder(string queueName, string busId)
        {
            return new MQBusBuilder(queueName, busId);
        }


        private Dictionary<string, List<MQHandler>> messageHandlers = new Dictionary<string, List<MQHandler>>();
        private string queueName;
        private string busId;
        private ClientEntity receiver;
        private ClientEntity sender;

        public MQBus(string queueName, string busId, ClientEntity receiver, ClientEntity sender, Dictionary<string, List<MQHandler>> messageHandlers)
        {
            this.queueName = queueName;
            this.busId = busId;
            this.receiver = receiver;
            this.sender = sender;
            this.messageHandlers = messageHandlers;

            if (messageHandlers.Count > 0)
            {
                var thread = new Thread(async () =>
                {
                    await StartReceiver();
                });
                thread.Start();
            }
        }

        private async Task StartReceiver()
        {
            Console.WriteLine("StartReceive start " + busId);
            if (receiver is MessageReceiver)
            {
                while (true)
                {
                    Message message = await (receiver as MessageReceiver).ReceiveAsync();
                    if (message != null)
                        await GotMessage(message);
                }
            }
            Console.WriteLine("StartReceive end " + busId);
        }

        public async Task Send(string label, dynamic data)
        {
            var client = sender as MessageSender;

            var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)))
            {
                // SessionId = F.NewGuid(),
                SessionId = "test",
                ContentType = "application/json",
                Label = label,
                // MessageId = j.ToString(),
                // TimeToLive = TimeSpan.FromMinutes(2)
            };
            await client.SendAsync(message);
        }

        private async Task GotMessage(Message message)
        {
            // Console.WriteLine("ID" + this.GetHashCode() + " : " + Encoding.UTF8.GetString(message.Body));

            var label = message.Label;

            var list = messageHandlers[label];

            var hId = -1;

            var pack = new HandlerPack();
            pack.receiver = receiver;
            pack.message = message;
            pack.payload = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(message.Body));
            pack.next = async () =>
            {
                hId++;
                if (hId < list.Count)
                {
                    var handler = list[hId];
                    await handler.Invoke(pack);
                }
            };

            while (hId < list.Count)
            {
                await pack.next();
            }
            Console.WriteLine("done!!");


        }



    }
}
