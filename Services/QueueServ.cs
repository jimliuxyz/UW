using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;

namespace UW.Services
{
    public class QueueServ
    {
        string cs = "Endpoint=sb://uwqueue.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=0WQP0IqqUV+h7qRsImJu3HFeFCTqmfVfQi+UPOMkq/0=";
        public async Task test()
        {
            // var namespaceClient = NamespaceManager.Create();
            // var q = new QueueClient()

            var queueClient = new QueueClient(cs, "uwqueue");
            var namespaceManager = new ManagementClient(cs);
            
            
            var numberOfMessagesToSend = 1;
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
            // Console.WriteLine("queueClient " + queueClient.);
            Console.WriteLine("done...");
        }
    }
}