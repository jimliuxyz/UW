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
        /// <summary>
        /// send and wait reply
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msgLabel"></param>
        /// <param name="data"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async static Task<object> SendAndWaitReply(AzureSBus sender, string msgLabel, dynamic data, int timeout = 60000)
        {
            var watch = new Stopwatch();
            watch.Start();

            // new a `result waiter` of MQReplyCenter to wait the result async 
            var replyTo = F.NewGuid();
            var waiter = MQReplyCenter.NewWaiter(replyTo);

            // sent the data to queue
            await sender.Send(msgLabel, data, replyToSessionId: MQReplyCenter.GetReplySessionId(), replyTo: replyTo);

            // get a waiter and waiting
            var task = waiter.wait();
            if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
            {
                Console.WriteLine("Queue Communication Elapsed : " + watch.Elapsed.TotalSeconds);
                return task.Result;
            }
            else
            {
                //timeout
                MQReplyCenter.CancelWaiter(replyTo);

                var e = new MQReplyTimeoutException(sender.queueName, msgLabel, timeout);

                Console.ForegroundColor = System.ConsoleColor.DarkRed;
                Console.WriteLine(e);
                Console.ResetColor();
                throw e;
            }
        }
    }
}