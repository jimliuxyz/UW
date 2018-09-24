using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BasicSendReceiveQuickStart
{
    class ProgramTest
    {
        static IQueueClient queueClient;
        static IQueueClient queueClient2;
        static IQueueClient queueClient3;
        static String ServiceBusConnectionString;
        static String QueueName;

        public static void Main2(string[] args)
        {
            string ServiceBusConnectionString = "";
            string QueueName = "";

            for (int i = 0; i < args.Length; i++)
            {
                var p = new ProgramTest();
                if (args[i] == "-ConnectionString")
                {
                    Console.WriteLine($"ConnectionString: {args[i + 1]}");
                    ServiceBusConnectionString = args[i + 1]; // Alternatively enter your connection string here.
                }
                else if (args[i] == "-QueueName")
                {
                    Console.WriteLine($"QueueName: {args[i + 1]}");
                    QueueName = args[i + 1]; // Alternatively enter your queue name here.
                }
            }

            if (ServiceBusConnectionString != "" && QueueName != "")
            {
                ProgramTest.ServiceBusConnectionString = ServiceBusConnectionString;
                ProgramTest.QueueName = QueueName;
                MainAsync(ServiceBusConnectionString, QueueName).GetAwaiter().GetResult();
            }
            else
            {
                Console.WriteLine("Specify -Connectionstring and -QueueName to execute the example.");
                Console.ReadKey();
            }
        }

        static async Task MainAsync(string ServiceBusConnectionString, string QueueName)
        {
            const int numberOfMessages = 0;
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

            Console.WriteLine("======================================================");
            Console.WriteLine("Press any key to exit after receiving all the messages.");
            Console.WriteLine("======================================================");

            // Send Messages
            await SendMessagesAsync(numberOfMessages);

            // Register QueueClient's MessageHandler and receive messages in a loop
            RegisterOnMessageHandlerAndReceiveMessages();

            Console.ReadKey();

            await queueClient.CloseAsync();
        }

        static void RegisterOnMessageHandlerAndReceiveMessages()
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 100,

                AutoComplete = false
            };
            queueClient2 = new QueueClient(ServiceBusConnectionString, QueueName);
            // queueClient2.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);

            queueClient3 = new QueueClient(ServiceBusConnectionString, QueueName);
            // queueClient3.RegisterMessageHandler(ProcessMessagesAsync3, messageHandlerOptions);

            var sessionOptions = new SessionHandlerOptions(ExceptionReceivedHandler)
            {
                AutoComplete = false,
                // AutoRenewTimeout = TimeSpan.FromSeconds(30),
                MaxConcurrentSessions = 1,
                MessageWaitTimeout = TimeSpan.FromSeconds(10)
            };
            queueClient2.RegisterSessionHandler(SessProcessMessagesAsync, sessionOptions);
            queueClient3.RegisterSessionHandler(SessProcessMessagesAsync3, sessionOptions);

        }
        static async Task SessProcessMessagesAsync(IMessageSession sess, Message message, CancellationToken token)
        {
            Console.WriteLine("A");
            await ProcessMessagesAsync(message, token, sess);
        }
        static async Task SessProcessMessagesAsync3(IMessageSession sess, Message message, CancellationToken token)
        {
            Console.WriteLine("B");
            await ProcessMessagesAsync3(message, token, sess);
        }
        static async Task ProcessMessagesAsync(Message message, CancellationToken token, IMessageSession sess = null)
        {
            try
            {
                dynamic data = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(message.Body));
                // if ((int)data.i % 2 == 1)
                // {
                //     // await Task.Delay(20);
                //     if (sess != null)
                //         await sess.AbandonAsync(message.SystemProperties.LockToken);
                //     else
                //         await queueClient2.AbandonAsync(message.SystemProperties.LockToken);
                //     return;
                // }

                await Task.Delay(200);
                if (sess != null)
                    await sess.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await queueClient2.CompleteAsync(message.SystemProperties.LockToken);
                Console.WriteLine($"2 Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} data:{data.i}-{data.j}");
            }
            catch (System.Exception e)
            {
                if (sess != null)
                    await sess.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await queueClient2.AbandonAsync(message.SystemProperties.LockToken);
                Console.WriteLine("2 failue" + e);
            }
        }
        static async Task ProcessMessagesAsync3(Message message, CancellationToken token, IMessageSession sess = null)
        {
            try
            {
                dynamic data = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(message.Body));
                // if ((int)data.j == 1)
                // {
                //     // await Task.Delay(10);
                //     if (sess != null)
                //         await sess.AbandonAsync(message.SystemProperties.LockToken);
                //         // await sess.RenewSessionLockAsync();
                //     else
                //         await queueClient3.AbandonAsync(message.SystemProperties.LockToken);
                //     return;
                // }

                await Task.Delay(300);
                if (sess != null)
                    await sess.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await queueClient3.CompleteAsync(message.SystemProperties.LockToken);
                Console.WriteLine($"3 >>>>>>>>>>>>> Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} data:{data.i}-{data.j}");
            }
            catch (System.Exception e)
            {
                if (sess != null)
                    await sess.CompleteAsync(message.SystemProperties.LockToken);
                else
                    await queueClient3.AbandonAsync(message.SystemProperties.LockToken);
                Console.WriteLine("3 failue" + e.ToString());
            }
        }

        // Use this Handler to look at the exceptions received on the MessagePump
        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            throw new Exception();
            return Task.CompletedTask;
        }

        static async Task SendMessagesAsync(int numberOfMessagesToSend)
        {
            try
            {
                for (var i = 0; i < numberOfMessagesToSend; i++)
                {
                    for (var j = 0; j < 3; j++)
                    {
                        // Create a new message to send to the queue
                        string messageBody = $"Message {i}";
                        var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                        dynamic data = new
                        {
                            i = i,
                            j = j
                        };

                        var message2 = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)))
                        {
                            SessionId = $"SID_{j}",
                            ContentType = "application/json",
                            Label = "RecipeStep",
                            MessageId = j.ToString(),
                            TimeToLive = TimeSpan.FromMinutes(2)
                        };

                        // Write the body of the message to the console
                        Console.WriteLine($"Sending message: {i}-{j}");

                        // Send the message to the queue
                        await queueClient.SendAsync(message2);
                    }

                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }



            // var receiver = new MessageReceiver(ServiceBusConnectionString, QueueName);
            // var sessionClient = new SessionClient(ServiceBusConnectionString, QueueName);

        }
    }
}

