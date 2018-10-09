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
        private List<ClientEntity> senders = new List<ClientEntity>();

        private AzureSBusSetting setting;

        public AzureSBus(AzureSBusSetting setting)
        {
            this.setting = setting;
            this.queueName = setting.queueName;
            this.stationId = setting.stationId;

            // create azure queue
            CreateAzureQueue(setting.queueName, setting.useSession).Wait();

            // create sender
            for (int i = 0; i < (setting.useSender); i++)
            {
                var sender = new MessageSender(R.QUEUE_CSTR, queueName, setting.sendRetryPolicy);
                senders.Add(sender);

                var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { })))
                {
                    SessionId = setting.useSession ? "_" : null,
                    ContentType = "application/json",
                    TimeToLive = TimeSpan.FromSeconds(1)
                };
                sender.SendAsync(message).Wait();
            }

            // create receiver
            if (setting.useSession)
                receiver = new SessionClient(R.QUEUE_CSTR, queueName, setting.receiveMode, setting.receiveRetryPolicy, setting.prefetchCount);
            else
                receiver = new MessageReceiver(R.QUEUE_CSTR, queueName, setting.receiveMode, setting.receiveRetryPolicy, setting.prefetchCount);

            // start receiving
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

        /// <summary>
        /// send a message
        /// </summary>
        /// <param name="session">訊息所處的session</param>
        /// <param name="label">用來分辨handler chain</param>
        /// <param name="data"></param>
        /// <param name="replyTo">盼回傳給某特定識別ID</param>
        /// <param name="replyToSessionId">盼回傳到某特定session</param>
        /// <param name="data">挾帶的內容</param>
        /// <returns></returns>
        public async Task Send(string label, dynamic data, string replyTo = null, string session = null, string replyToSessionId = null)
        {
            var client = (senders[F.Random(0, senders.Count)]) as MessageSender;

            var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)))
            {
                // MessageId = j.ToString(),
                SessionId = session,                   //the sessionId of this message
                Label = label,
                ReplyToSessionId = replyToSessionId,    //the sessionId for reply(send back)
                ReplyTo = replyTo,
                ContentType = "application/json",
            };
            if (setting.ttl > 0)
                message.TimeToLive = TimeSpan.FromMilliseconds(setting.ttl);

            await client.SendAsync(message);
        }

        private async Task StartReceiver()
        {
            Console.WriteLine($"MQ({queueName}) start receiving : {stationId}");
            if (receiver is MessageReceiver)
            {
                var client = receiver as MessageReceiver;

                while (true)
                {
                    Message message = await client.ReceiveAsync();
                    try
                    {
                        if (message != null)
                            await DispatchMessage(message, client);
                    }
                    catch (System.Exception e)
                    {
                        Task.Delay(1000).Wait();
                        Console.WriteLine(e.Message);
                    }
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
                                Console.WriteLine(e.Message);
                                break;
                            }
                        }
                    }, TaskCreationOptions.LongRunning);
                    tasks.Add(task);
                }
                Task.WaitAll(tasks.ToArray());
            }

            Console.WriteLine($"MQ({queueName}) stop receiving : {stationId}");
        }

        /// <summary>
        /// dispatch message by its label
        /// </summary>
        /// <param name="message"></param>
        /// <param name="receiver"></param>
        /// <returns></returns>
        private async Task DispatchMessage(Message message, IMessageReceiver receiver)
        {
            if (message.Label == null || !setting.messageHandlers.ContainsKey(message.Label))
            {
                // don't have to wait the call
                /*await*/
                receiver.AbandonAsync(message.SystemProperties.LockToken);
                return;
            }

            var handlers = setting.messageHandlers[message.Label];
            if (handlers.Count == 0)
            {
                // don't have to wait the call
                /*await*/
                receiver.CompleteAsync(message.SystemProperties.LockToken);
                return;
            }

            try
            {
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

                // start handler function chain
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
                return;
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            // for any uncatched exception
            try
            {
                receiver.AbandonAsync(message.SystemProperties.LockToken);
            }
            catch (System.Exception e)
            {
                throw;
            }

        }

    }
}
