using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using UW.Shared.Misc;
using UW.Shared.MQueue.Utils;
using UW.Shared.Persis;

namespace UW.Shared.MQueue.Handlers
{
    public partial class MQUserCreate
    {
        private class MsgUserCreate
        {
            public string userId { get; set; }
            public string name { get; set; }
            public string phoneno { get; set; }
            public string avatar { get; set; }
        }
    }

    /// <summary>
    /// sender part
    /// </summary>
    public partial class MQUserCreate
    {
        public static readonly string QUEUE_NAME = "teseting";
        private static readonly string LABEL_USER_CREATE = "user_create";

        private static AzureSBus sender = AzureSBus.Builder(QUEUE_NAME).UseSender(1).build();

        public async static Task<Object> CreateUser(string userId, string name = null, string phoneno = null, string avatar = null)
        {
            var data = new MsgUserCreate
            {
                userId = userId,
                name = name,
                phoneno = phoneno,
                avatar = avatar,
            };

            return await MQUtils.SendAndWaitReply(sender, LABEL_USER_CREATE, data);
        }
    }

    /// <summary>
    /// receiver part
    /// </summary>
    public partial class MQUserCreate
    {
        public static void StartReceiving(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var mqbus = AzureSBus.Builder(QUEUE_NAME, QUEUE_NAME + "-Receiver-" + i)
                    .SetReceiveMode(ReceiveMode.PeekLock)
                    .AddMessageHandlerChain(LABEL_USER_CREATE, async (pack) =>
                    {
                        // Console.WriteLine("start...");
                        // Console.WriteLine(pack.data);
                    }, Flow1, CompleteAsync)
                    .SetPrefetchCount(5)
                    .build();
            }
        }

        public static int cnt = 0;
        private static async Task Flow1(HandlerPack pack)
        {
            Console.WriteLine("got " + pack.data);

            // var data = (MsgUserCreate)pack.data;
            // var data = pack.data as MsgUserCreate;
            var data = JsonConvert.DeserializeObject<MsgUserCreate>(Encoding.UTF8.GetString(pack.message.Body));

            var helper = new UserHelper();

            var pkgid = PkuidGen.Parse(data.userId);
            Console.WriteLine("got " + pkgid.ToJson());

        }

        private static async Task CompleteAsync(HandlerPack pack)
        {
            // reply to MQReplyCenter
            if (pack.message.ReplyToSessionId != null)
            {
                var replyData = new
                {
                    stationId = pack.stationId,
                    threadHashCode = Thread.CurrentThread.GetHashCode(),
                    echo = pack.data.msg,
                    error = (object)null
                };
                /*await*/
                MQReplyCenter.SendReply(pack.message.ReplyToSessionId, pack.message.ReplyTo, replyData);
            }

            /*await*/
            pack.receiver.CompleteAsync(pack.message.SystemProperties.LockToken);
        }
    }
}

