using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;
using static UW.Shared.MQueue.Utils.AzureSBus;

namespace UW.Shared.MQueue.Utils
{
    public class AzureSBusBuilder
    {
        private AzureSBusSetting setting = new AzureSBusSetting();

        public AzureSBusBuilder(string queueName, string stationId)
        {
            setting.queueName = queueName;
            setting.stationId = stationId;
        }

        public AzureSBusBuilder SetReceiveMode(ReceiveMode receiveMode)
        {
            setting.receiveMode = receiveMode;
            return this;
        }
        public AzureSBusBuilder SetReceiveRetryPolicy(RetryPolicy receiveRetryPolicy)
        {
            setting.receiveRetryPolicy = receiveRetryPolicy;
            return this;
        }
        public AzureSBusBuilder SetSendRetryPolicy(RetryPolicy sendRetryPolicy)
        {
            setting.sendRetryPolicy = sendRetryPolicy;
            return this;
        }
        public AzureSBusBuilder UseSession(params string[] sessionIds)
        {
            setting.useSession = true;
            setting.sessionIds.AddRange(sessionIds);
            return this;
        }

        /// <summary>
        /// set number of sender.
        /// </summary>
        /// <param name="count">(設定>1對效能並沒有提升)</param>
        /// <returns></returns>
        public AzureSBusBuilder UseSender(int count = 1)
        {
            setting.useSender = count;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefetchCount">設定值須考慮任務執行時間長短</param>
        /// <returns></returns>
        public AzureSBusBuilder SetPrefetchCount(int prefetchCount)
        {
            setting.prefetchCount = prefetchCount;
            return this;
        }
        public AzureSBusBuilder SetTTL(int ttl)
        {
            setting.ttl = ttl;
            return this;
        }
        public AzureSBusBuilder AddMessageHandlerChain(string label, params MQHandler[] handlers)
        {
            if (!setting.messageHandlers.ContainsKey(label))
                setting.messageHandlers[label] = new List<MQHandler>();

            setting.messageHandlers[label].AddRange(handlers);
            return this;
        }

        public AzureSBus build()
        {
            return new AzureSBus(setting);
        }



    }
}
