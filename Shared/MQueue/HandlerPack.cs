using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;

namespace UW.Shared.MQueue
{
    public class HandlerPack
    {
        public ClientEntity receiver;
        public Message message;

        public dynamic payload;
        public dynamic custom;
        public Func<Task> next; // invoke next function of handler chain
        public Func<Task> msgComplete;
        public Func<Task> msgSkip;
    }
}
