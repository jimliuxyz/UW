using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;
using Newtonsoft.Json;

namespace UW.Shared.MQueue.Utils
{
    public class AzureSBus
    {
        public delegate Task MQHandler(HandlerPack msgPack);
        public static AzureSBusBuilder Builder(string queueName, string stationId = "")
        {
            return new AzureSBusBuilder(queueName, stationId);
        }

        public readonly string queueName;
        public readonly string stationId;

        private ClientEntity receiver;
        private ClientEntity sender;
        private List<ClientEntity> senders = new List<ClientEntity>();

        private AzureSBusSetting setting;

        public AzureSBus(AzureSBusSetting setting)
        {
            this.setting = setting;
            this.queueName = setting.queueName;
            this.stationId = setting.stationId;

            CreateAzureQueue(setting.queueName, setting.useSession).Wait();

            if (setting.useSender)
            {
                for (int i = 0; i < 5; i++)
                {
                    var sender = new MessageSender(R.QUEUE_CSTR, queueName, setting.sendRetryPolicy);
                    senders.Add(sender);

                    var message = new Message()
                    {
                        SessionId = "_",
                        TimeToLive = TimeSpan.FromMinutes(1)
                    };
                    sender.SendAsync(message).Wait();
                }
            }


            if (setting.useSession)
                receiver = new SessionClient(R.QUEUE_CSTR, queueName, setting.receiveMode, setting.receiveRetryPolicy, setting.prefetchCount);
            else
                receiver = new MessageReceiver(R.QUEUE_CSTR, queueName, setting.receiveMode, setting.receiveRetryPolicy, setting.prefetchCount);

            if (setting.messageHandlers.Count > 0)
            {
                Task.Factory.StartNew(StartReceiver, TaskCreationOptions.LongRunning);
            }
        }

        private static async Task CreateAzureQueue(string queueName, bool isRequiresSession = false)
        {
            var nm = new ManagementClient(R.QUEUE_CSTR);

            if (!await nm.QueueExistsAsync(queueName))
            {
                var desc = new QueueDescription(queueName)
                {
                    RequiresSession = isRequiresSession,
                    DefaultMessageTimeToLive = TimeSpan.FromMinutes(5)
                };
                await nm.CreateQueueAsync(desc);
            }
        }

        public async Task Send(string label, dynamic data, string replyTo = null, string session = null, string replyToSessionId = null)
        {
            // if (sender == null)
            // {
            //     lock (this)
            //     {
            //         if (sender == null)
            //             sender = new MessageSender(R.QUEUE_CSTR, queueName, setting.sendRetryPolicy);
            //     }
            // }

            // var client = sender as MessageSender;
            var idx = new Random().Next(0, senders.Count);
            var client = (senders[idx]) as MessageSender;

            var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)))
            {
                SessionId = session,                   //the sessionId of this message
                Label = label,
                ReplyToSessionId = replyToSessionId,    //the sessionId for reply(send back)
                ReplyTo = replyTo,
                ContentType = "application/json",
                // MessageId = j.ToString(),
                // TimeToLive = TimeSpan.FromMinutes(2)
            };
            await client.SendAsync(message);
        }

        private async Task StartReceiver()
        {
            Console.WriteLine("StartReceive start " + stationId);
            if (receiver is MessageReceiver)
            {
                var client = receiver as MessageReceiver;

                while (true)
                {
                    Message message = await client.ReceiveAsync();
                    if (message != null)
                        await DispatchMessage(message, client);
                }
            }
            else if (receiver is SessionClient)
            {
                var _client = receiver as SessionClient;

                var tasks = new List<Task>();
                foreach (var sessionId in setting.sessionIds)
                {
                    var task = Task.Factory.StartNew(async () =>
                    {
                        var client = _client.AcceptMessageSessionAsync(sessionId).Result;
                        while (true)
                        {
                            try
                            {
                                Message message = client.ReceiveAsync().Result;
                                if (message != null)
                                    await DispatchMessage(message, client);
                            }
                            catch (System.AggregateException e)
                            {
                                Task.Delay(1000).Wait();
                                Console.WriteLine(e.Message);
                            }
                            catch (SessionLockLostException e)
                            {
                                Task.Delay(1000).Wait();
                                Console.WriteLine(e.GetType());
                                Console.WriteLine("RenewSessionLockAsync...");

                                client.RenewSessionLockAsync().Wait();
                            }
                            catch (System.Exception e)
                            {
                                Task.Delay(1000).Wait();
                                Console.WriteLine(e.GetType());
                                break;
                            }
                        }
                    }, TaskCreationOptions.LongRunning);
                    tasks.Add(task);
                }
                Task.WaitAll(tasks.ToArray());
            }

            Console.WriteLine("StartReceive end " + stationId);
        }

        ///dispatch by message label
        private async Task DispatchMessage(Message message, IMessageReceiver receiver)
        {
            var handlers = setting.messageHandlers[message.Label];
            if (handlers == null || handlers.Count == 0)
            {
                // don't have to wait the call
                /*await*/
                receiver.AbandonAsync(message.SystemProperties.LockToken);
                return;
            }
            var hId = -1;

            var pack = new HandlerPack();
            pack.stationId = stationId;
            pack.receiver = receiver;
            pack.message = message;
            pack.data = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(message.Body));
            pack.terminate = () =>
            {
                hId = handlers.Count;
            };
            pack.next = async () =>
            {
                hId++;
                if (hId < handlers.Count)
                {
                    var handler = handlers[hId];
                    await handler.Invoke(pack);
                }
            };

            while (hId < handlers.Count)
            {
                try
                {
                    await pack.next();
                }
                catch (System.Exception e)
                {
                    Console.WriteLine(e.ToString());
                    throw;
                }
            }


        }



    }
}
