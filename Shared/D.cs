namespace UW.Shared
{
    /// <summary>
    /// Defines
    /// </summary>
    public class D
    {
        /// <summary>
        /// Notification tag for sigle user or group
        /// </summary>
        public static class NTFTAG
        {
            public static readonly string USER_PREFIX = "uid:";
            public static readonly string EVERYBODY = "everybody";
        }

        /// <summary>
        /// Notification type
        /// </summary>
        public static class NTFTYPE
        {
            public static readonly string LOGOUT = "LOGOUT";
            public static readonly string TXRECEIPT = "TX_RECEIPT";
        }

        public static class CLAIM
        {
            public static readonly string USERID = "userid";
            public static readonly string TOKENRND = "tokenrnd";
        }

        /// <summary>
        /// Message Queue Name
        /// </summary>
        public static class QN
        {
            public static readonly string TXREQ = "TX-REQ";
        }

        public static readonly string DOC_USER_NAME = "name";
        public static readonly string DOC_USER_CURRENCIES = "currencies";


        // currency
        public static readonly string CNY = "CNY";
        public static readonly string USD = "USD";
        public static readonly string BTC = "BTC";
        public static readonly string ETH = "ETH";
        public static readonly string _SELL = "-sell";
        public static readonly string _BUY = "-buy";

    }
}