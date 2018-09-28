
using System;
using Microsoft.Azure.Documents.Client;

namespace UW.Shared
{
    /// <summary>
    /// Shared Resources
    /// </summary>
    public static class R
    {
        //Environment Variables
        public static readonly string JWT_ISSUER, JWT_AUDIENCE, JWT_SECRET;
        public static readonly string SALT_SMS;
        public static readonly string DB_URI, DB_KEY, DB_NAME;
        public static readonly string NTFHUB_NAME, NTFHUB_CSTR;

        public static readonly string DB_COL_SMSPASSCODE = "SmsPasscode";
        public static readonly string DB_COL_USER = "User";
        public static readonly string QUEUE_CSTR;

        static R()
        {
            Console.WriteLine("GetEnvironmentVariable....");
            JWT_ISSUER = Environment.GetEnvironmentVariable("JWT_ISSUER");
            JWT_AUDIENCE = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
            JWT_SECRET = Environment.GetEnvironmentVariable("JWT_SECRET");

            SALT_SMS = Environment.GetEnvironmentVariable("SALT_SMS");
            
            DB_URI = Environment.GetEnvironmentVariable("DB_URI");
            DB_KEY = Environment.GetEnvironmentVariable("DB_KEY");
            DB_NAME = Environment.GetEnvironmentVariable("DB_NAME");

            NTFHUB_NAME = Environment.GetEnvironmentVariable("NTFHUB_NAME");
            NTFHUB_CSTR = Environment.GetEnvironmentVariable("NTFHUB_CSTR");

            QUEUE_CSTR = Environment.GetEnvironmentVariable("QUEUE_CSTR");
        }
    }
}