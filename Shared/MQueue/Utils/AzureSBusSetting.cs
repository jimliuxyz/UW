using System.Collections.Generic;
using Microsoft.Azure.ServiceBus;
using static UW.Shared.MQueue.Utils.AzureSBus;

namespace UW.Shared.MQueue.Utils
{
    public class AzureSBusSetting
    {
        public Dictionary<string, List<MQHandler>> messageHandlers = new Dictionary<string, List<MQHandler>>();
        public string queueName;
        public string stationId;
        public bool useSession;
        public List<string> sessionIds = new List<string>();
        public bool useSender;
        public int prefetchCount;
        public int ttl;    //time to live
        public ReceiveMode receiveMode;
        public RetryPolicy receiveRetryPolicy = RetryPolicy.Default;
        public RetryPolicy sendRetryPolicy = RetryPolicy.Default;
    }
}