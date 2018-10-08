using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using UW.Shared.MQueue.MQException;
using UW.Shared.MQueue.Utils;

namespace UW.Shared.MQueue.Handlers
{
    public abstract class MQUtils
    {
        public async static Task<object> SendAndWaitReply(List<AzureSBus> senders, string msgLabel, dynamic data, int timeout = 1000)
        {
            return null;
        }

        public async static Task<object> SendAndWaitReply(AzureSBus sender, string msgLabel, dynamic data, int timeout = 1000)
        {
            var watch = new Stopwatch();
            watch.Start();

            // new a `result waiter` of MQReplyCenter to wait the result async 
            var replyTo = F.NewGuid();
            var waiter = MQReplyCenter.NewWaiter(replyTo);

            /*await*/ sender.Send(msgLabel, data, replyToSessionId: MQReplyCenter.INSTANCE_ID, replyTo: replyTo);

            // var res = await waiter.wait();

            var task = waiter.wait();

            if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
            {
                Console.WriteLine("Queue Communication Elapsed : " + watch.Elapsed.TotalSeconds);
                return task.Result;
            }
            else
            {
                MQReplyCenter.CancelWaiter(replyTo);
            }

            var e = new MQReplyTimeoutException(sender.queueName, msgLabel, timeout);

            Console.ForegroundColor = System.ConsoleColor.DarkRed;
            Console.WriteLine(e);
            Console.ResetColor();

            throw e;
        }
    }
}