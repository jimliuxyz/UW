
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;

namespace UW.MQueue
{
    public class MQHelper
    {



        // public static async Task<MessageSender> GetTxReqSender()
        // {
        //     await CreateQueue(QN_TXREQ);
        //     return new MessageSender(R.QUEUE_CSTR, QN_TXREQ);
        // }
        // public static async Task<MessageReceiver> GetTxReceiver()
        // {
        //     await CreateQueue(QN_TXREQ);
        //     return new MessageReceiver(R.QUEUE_CSTR, QN_TXREQ);
        // }

        public static async Task CreateQueue(string queueName, bool isRequiresSession = false)
        {
            var nm = new ManagementClient(R.QUEUE_CSTR);

            if (!await nm.QueueExistsAsync(queueName))
            {
                var desc = new QueueDescription(queueName)
                {
                    RequiresSession = isRequiresSession
                };
                await nm.CreateQueueAsync(desc);
            }
        }
    }
}

