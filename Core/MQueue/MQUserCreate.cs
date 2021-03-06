using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using UW.Core.Misc;
using UW.Core.MQueue.Utils;
using UW.Core.Persis;
using UW.Core.Persis.Helper;

namespace UW.Core.MQueue.Handlers
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

        public async static Task<object> CreateUser(string userId, string phoneno, string name = null, string avatar = null)
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
                    .SetPrefetchCount(5)
                    .AddMessageHandlerChain(LABEL_USER_CREATE, async (pack) =>
                    {
                        // Console.WriteLine("start...");
                        // Console.WriteLine(pack.data);
                    }, Unpack, Flow1, CompleteAsync)
                    .build();
            }
        }

        public static int cnt = 0;
        private static async Task Unpack(HandlerPack pack)
        {
            pack.param["msg"] = JsonConvert.DeserializeObject<MsgUserCreate>(Encoding.UTF8.GetString(pack.message.Body));
        }
        private static async Task Flow1(HandlerPack pack)
        {
            var msg = pack.param["msg"] as MsgUserCreate;

            var helper = new UserHelper();

            var pkgid = UserHelper.IdGen.Parse(msg.userId);

            await helper.Create(pkgid, "", msg.phoneno);
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
                    echo = pack.param,
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

