using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;

namespace UW.Shared.MQueue.Utils
{
    public class HandlerPack
    {
        public string stationId;
        public IMessageReceiver receiver;
        public Message message;

        public dynamic data;
        public dynamic custom;
        public Func<Task> next; // invoke next function of handler chain
        public Action terminate; // terminate the handler chain

        public async Task completeAsync()
        {
            terminate.Invoke();
            await receiver.CompleteAsync(message.SystemProperties.LockToken);
        }
        public async Task abandonAsync()
        {
            terminate.Invoke();
            await receiver.AbandonAsync(message.SystemProperties.LockToken);
        }
    }
}
