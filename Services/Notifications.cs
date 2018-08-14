using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.NotificationHubs;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UW.Models.Collections;
using Microsoft.Azure.NotificationHubs.Messaging;

namespace UW.Services
{
    public class Notifications
    {
        private NotificationHubClient hub;

        public Notifications(IConfiguration configuration)
        {
            HubSettings settings = new HubSettings();
            configuration.Bind("NotificationHub", settings);
            hub = NotificationHubClient.CreateClientFromConnectionString(settings.ConnectionString, settings.HubName);
        }

        public void broadcastMessage(string message)
        {
            Console.WriteLine("---- sendMulticast ----");
            var notif = "{ \"data\" : {\"message\":\"" + message + "\"}}";
            hub.SendGcmNativeNotificationAsync(notif);

            // var alert = "{\"aps\":{\"alert\":\"" + message + "\"}}";
            // hub.SendAppleNativeNotificationAsync(alert);
        }
        private string regId = "6999319467446834786-247359078146117058-1";

        public void sendMessage2(string message, string to)
        {
            var notif = "{ \"data\" : {\"message\":\"" + message + "\"}}";

            // get the RegistrationDescription by regId
            RegistrationDescription reg = hub.GetRegistrationAsync<RegistrationDescription>(regId).Result;
            Console.WriteLine(JsonConvert.SerializeObject(reg, Formatting.Indented));

            // create or update RegistrationDescription
            // reg.RegistrationId = regId;
            // reg.Tags = new HashSet<string>(new string[] { "gpk" });
            // reg.Tags.Add("username:" + "nate_");
            // await hub.CreateOrUpdateRegistrationAsync(reg);


            // send message
            // await hub.SendGcmNativeNotificationAsync(notif, new string[] { "username:" + "nate_"});
            // await hub.SendTemplateNotificationAsync(new Dictionary<string, string>{{"message","hello"}}, "username:" + "nate_");
        }
        public void sendNotification(string message, string tag, PNS pns)
        {
            var notif = "";
            switch (pns)
            {
                case PNS.apns:
                    notif = "{ \"aps\" : {\"alert\":\"" + message + "\"}}";
                    hub.SendAppleNativeNotificationAsync(notif, new string[] { tag });
                    break;
                case PNS.gcm:
                    notif = "{ \"data\" : {\"message\":\"" + message + "\"}}";
                    hub.SendGcmNativeNotificationAsync(notif, new string[] { tag });
                    break;
                default:
                    break;
            }
        }

        public static string getUserTag(string userId)
        {
            return "uid:" + userId;
        }

        // get a new/old regId and clear all
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

        public async Task<string> updateRegId(PNS pns, string pnsToken, string userId)
        {
            string tag = getUserTag(userId);

            string regId = getRegIdAsync(tag).Result;
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
                registration.RegistrationId = regId;
                registration.Tags = new HashSet<string>();
                registration.Tags.Add("everybody");
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

    class HubSettings
    {
        public string ConnectionString { get; set; }
        public string HubName { get; set; }
    }
}