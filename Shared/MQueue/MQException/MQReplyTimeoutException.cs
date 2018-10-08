using System;

namespace UW.Shared.MQueue.MQException
{
    public class MQReplyTimeoutException : Exception
    {
        public MQReplyTimeoutException(string queueName, string label, int timeout) : base($"Queue({queueName}'{label}) Reply Timeout({timeout}ms).")
        {
        }
    }
}
