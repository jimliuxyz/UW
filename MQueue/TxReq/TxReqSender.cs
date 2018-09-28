using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;
using UW.MQueue;
using UW.MQueue.Messages;

namespace UW.MQueue.TxReq
{
    public class TxReqSender
    {
        private static readonly string QNAME = D.QN.TXREQ;
        public static async Task<TxReqSender> get()
        {
            await MQHelper.CreateQueue(QNAME);

            return new TxReqSender(new MessageSender(R.QUEUE_CSTR, QNAME));
        }

        private MessageSender client;
        private TxReqSender(MessageSender client)
        {
            this.client = client;
        }

        public async Task send(int type)
        {
            var data = new TxReqMsg(){
                type = type,
                uid = F.NewGuid()
            };

            var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)))
            {
                // SessionId = F.NewGuid(),
                SessionId = "test",
                ContentType = "application/json",
                Label = "RecipeStep",
                // MessageId = j.ToString(),
                // TimeToLive = TimeSpan.FromMinutes(2)
            };

            await client.SendAsync(message);
        }

    }
}