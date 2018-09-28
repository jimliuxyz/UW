using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.NotificationHubs.Messaging;
using UW.Shared;
using UW.Shared.Persis.Collections;

namespace UW.Shared.Services
{
    public class Ntfy
    {
        private NotificationHubClient hub;

        public Ntfy()
        {
            hub = NotificationHubClient.CreateClientFromConnectionString(R.NTFHUB_CSTR, R.NTFHUB_NAME);
        }

        /// <summary>
        /// 對某user發送訊息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="pns"></param>
        /// <param name="message"></param>
        /// <param name="type">自訂動作</param>
        /// <param name="payload">自訂資料</param>
        public void sendMessage(string userId, PNS pns, string message, string type=null, object payload=null){
            var userTag = getUserTag(userId);
            sendToTag(userTag, pns, message, type, payload);
        }

        /// <summary>
        /// 以廣播方式發送訊息
        /// </summary>
        /// <param name="message"></param>
        public void broadcast(string message){
            // 分別對每個PNS發送
            foreach(PNS pns in Enum.GetValues(typeof(PNS))){
                sendToTag(D.NTFTAG.EVERYBODY, pns, message);
            }
        }

        /// <summary>
        /// 發送訊息到指定tag
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="pns"></param>
        /// <param name="message"></param>
        /// <param name="type">自訂動作</param>
        /// <param name="payload">自訂資料</param>
        private void sendToTag(string tag, PNS pns, string message, string type=null, object payload=null)
        {
            var notif = "";
            var custom = new {
                type = type,
                payload = payload
            };
            var custom_json = "\"custom\" : " + Newtonsoft.Json.JsonConvert.SerializeObject(custom);
            switch (pns)
            {
                case PNS.apns:
                    notif = "{ \"aps\" : {\"alert\":\"" + message + "\"}, " + custom_json + "}";
                    hub.SendAppleNativeNotificationAsync(notif, new string[] { tag });
                    break;
                case PNS.gcm:
                    notif = "{ \"data\" : {\"message\":\"" + message + "\"}, " + custom_json + "}";
                    hub.SendGcmNativeNotificationAsync(notif, new string[] { tag });
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 以userId取得個別使用者的tag
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private static string getUserTag(string userId)
        {
            return D.NTFTAG.USER_PREFIX + userId;
        }

        /// <summary>
        /// 以tag取得或新建一個azure regId, 並刪除舊id的內容
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        private async Task<string> getRegIdAsync(string tag = null)
        {
            string newRegistrationId = null;

            if (tag != null)
            {
                var registrations = await hub.GetRegistrationsByTagAsync(tag, 100);

                foreach (RegistrationDescription registration in registrations)
                {
                    if (newRegistrationId == null)
                    {
                        newRegistrationId = registration.RegistrationId;
                    }
                    else
                    {
                        await hub.DeleteRegistrationAsync(registration);
                    }
                }
            }

            if (newRegistrationId == null)
                newRegistrationId = await hub.CreateRegistrationIdAsync();

            return newRegistrationId;
        }

        /// <summary>
        /// 以userId更新其裝置PNS的資訊
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="pns"></param>
        /// <param name="pnsToken"></param>
        /// <returns></returns>
        public async Task<string> updateRegId(string userId, PNS pns, string pnsToken)
        {
            string tag = getUserTag(userId);

            //取得或新建azure regId
            string regId = getRegIdAsync(tag).Result;

            //依pns類型建立註冊描述
            RegistrationDescription registration = null;
            switch (pns)
            {
                case PNS.apns:
                    registration = new AppleRegistrationDescription(pnsToken);
                    break;
                case PNS.gcm:
                    registration = new GcmRegistrationDescription(pnsToken);
                    break;
                default:
                    break;
            }

            try
            {
                //填入內容並更新到azure notification hub
                registration.RegistrationId = regId;
                registration.Tags = new HashSet<string>();
                registration.Tags.Add(D.NTFTAG.EVERYBODY);
                registration.Tags.Add(tag);

                await hub.CreateOrUpdateRegistrationAsync(registration);
            }
            catch (MessagingException e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }

            return registration.RegistrationId;
        }
    }

}