using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Azure.ServiceBus.Core;
using System.Threading;

namespace UW.Services
{
    public class QueueServ
    {
        string cs = "Endpoint=sb://uwqueue.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=0WQP0IqqUV+h7qRsImJu3HFeFCTqmfVfQi+UPOMkq/0=";
        string qname = "myqueue2";

        static QueueClient queueClient;
        public async Task test()
        {
            await CreateQueue(qname);

            queueClient = new QueueClient(cs, qname);


            var numberOfMessagesToSend = 0;
            try
            {
                for (var i = 0; i < numberOfMessagesToSend; i++)
                {
                    // Create a new message to send to the queue.
                    string messageBody = $"Message {i}";
                    var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                    // Write the body of the message to the console.
                    Console.WriteLine($"Sending message: {messageBody}");

                    // Send the message to the queue.
                    await queueClient.SendAsync(message);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }


            RegisterOnMessageHandlerAndReceiveMessages();
            Console.ReadKey();




            Console.WriteLine("done...");
        }

        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
        static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            Console.WriteLine("???????? " + message.SystemProperties.LockToken);
            // Process the message.
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

            // Complete the message so that it is not received again.
            // This can be done only if the queue Client is created in ReceiveMode.PeekLock mode (which is the default).
            await queueClient.CompleteAsync(message.SystemProperties.LockToken);
            // new MessageSender()
            // Note: Use the cancellationToken passed as necessary to determine if the queueClient has already been closed.
            // If queueClient has already been closed, you can choose to not call CompleteAsync() or AbandonAsync() etc.
            // to avoid unnecessary exceptions.
        }
        private async Task RegisterOnMessageHandlerAndReceiveMessages()
        {
            Console.WriteLine("a 1");
            var queueClient = new QueueClient(cs, qname);
            // var queueClient = new MessageReceiver(cs, qname, ReceiveMode.PeekLock);
            Console.WriteLine("a 2");

            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether the message pump should automatically complete the messages after returning from user callback.
                // False below indicates the complete operation is handled by the user callback as in ProcessMessagesAsync().
                AutoComplete = false
            };
                // await Task.Delay(20000);
            Console.WriteLine("a 3");


            // Register the function that processes messages.
            queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);

            // while (true)
            // {
            //     Message message = await queueClient.ReceiveAsync();
            //     Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
            //     await Task.Delay(1000);
            // }


            Console.WriteLine("a 4");

        }

        private async Task CreateQueue(string queueName, bool isRequiresSession = true)
        {
            var nm = new ManagementClient(cs);

            if (!await nm.QueueExistsAsync(queueName))
            {
                var desc = new QueueDescription(queueName)
                {
                    // RequiresSession = isRequiresSession
                };
                await nm.CreateQueueAsync(desc);
            }
        }
    }
}