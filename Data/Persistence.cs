using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Newtonsoft.Json;
using UW.Models.Collections;
using System.Net;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UW.Services;
using System.Threading.Tasks;

namespace UW.Data
{


    class Settings
    {
        public string Uri { get; set; }
        public string SecretKey { get; set; }
    }

    /// <summary>
    /// 資料持久層
    /// </summary>
    public class Persistence
    {
        private static Uri URI_DB = UriFactory.CreateDatabaseUri(R.DB_NAME);

        //user
        private static string COL_USER = typeof(UW.Models.Collections.User).Name;
        private static Uri URI_USER = UriFactory.CreateDocumentCollectionUri(R.DB_NAME, COL_USER);

        //sms passcode
        private static string COL_SMSPCODE = typeof(SmsPasscode).Name;
        private static Uri URI_SMSPCODE = UriFactory.CreateDocumentCollectionUri(R.DB_NAME, COL_SMSPCODE);

        //contact
        private static string COL_CONTACT = typeof(Contacts).Name;
        private static Uri URI_CONTACT = UriFactory.CreateDocumentCollectionUri(R.DB_NAME, COL_CONTACT);

        //balance
        private static string COL_BALANCE = typeof(Balance).Name;
        private static Uri URI_BALANCE = UriFactory.CreateDocumentCollectionUri(R.DB_NAME, COL_BALANCE);
        private Notifications notifications;

        public readonly DocumentClient client;
        public Persistence(Notifications notifications)
        {
            Console.WriteLine("====init db====");
            this.notifications = notifications;

            //get database client as a connection
            client = new DocumentClient(new Uri(R.DB_URI), R.DB_KEY);

            InitDB();
        }

        public void InitDB()
        {
            //create database
            client.CreateDatabaseIfNotExistsAsync(new Database { Id = R.DB_NAME }).Wait();

            //create collections
            var defReqOpts = new RequestOptions { OfferThroughput = 100 }; //todo:實際運作400RU可能太小
            client.CreateDocumentCollectionIfNotExistsAsync(URI_DB,
                                new DocumentCollection { Id = COL_USER }, defReqOpts).Wait();
            client.CreateDocumentCollectionIfNotExistsAsync(URI_DB,
                                new DocumentCollection { Id = COL_SMSPCODE, DefaultTimeToLive = 60*60 }, defReqOpts).Wait(); //60min for a round
            client.CreateDocumentCollectionIfNotExistsAsync(URI_DB,
                                new DocumentCollection { Id = COL_CONTACT }, defReqOpts).Wait();
            client.CreateDocumentCollectionIfNotExistsAsync(URI_DB,
                                new DocumentCollection { Id = COL_BALANCE }, defReqOpts).Wait();
        }

        /// <summary>
        /// 取得所有使用者
        /// </summary>
        /// <returns></returns>
        public List<Models.Collections.User> getUsers()
        {
            var q = client.CreateDocumentQuery<Models.Collections.User>(URI_USER);
            var result = from user in q select user;

            return result.ToList();
        }

        /// <summary>
        /// 以電話號碼取得使用者列表
        /// </summary>
        /// <param name="phones"></param>
        /// <returns></returns>
        public List<Models.Collections.User> findUsersByPhone(List<string> phones)
        {
            var q = client.CreateDocumentQuery<Models.Collections.User>(URI_USER);
            var result = from user in q where phones.Contains(user.phoneno) select user;

            return result.ToList();
        }

        /// <summary>
        /// 以電話號碼取得使用者
        /// </summary>
        /// <param name="phoneno"></param>
        /// <returns></returns>
        public Models.Collections.User getUserByPhone(string phoneno)
        {
            var q = client.CreateDocumentQuery<Models.Collections.User>(URI_USER);
            var result = from user in q where user.phoneno == phoneno select user;

            return (result.Count() > 0) ? result.ToList().First() : null;
        }

        /// <summary>
        /// 以userId取得使用者
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Models.Collections.User getUserByUserId(string userId)
        {
            var q = client.CreateDocumentQuery<Models.Collections.User>(URI_USER);
            var result = from user in q where user.userId == userId select user;

            return (result.Count() > 0) ? result.ToList().First() : null;
        }

        public List<Models.Collections.User> getUserByUserId(string[] userIds)
        {
            var q = client.CreateDocumentQuery<Models.Collections.User>(URI_USER);
            var result = from user in q where userIds.Contains(user.userId) select user;

            return (result.Count() > 0) ? result.ToList() : null;
        }

        /// <summary>
        /// 更新或新增使用者
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool upsertUser(Models.Collections.User user)
        {
            try
            {
                var res = client.UpsertDocumentAsync(URI_USER, user).Result;
                return res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.Created;
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return false;
        }


        /// <summary>
        /// 檢查簡訊驗證碼是否相符
        /// </summary>
        /// <param name="phoneno"></param>
        /// <param name="passcode"></param>
        /// <returns></returns>
        public bool isSmsPasscodeMatched(string phoneno, string passcode)
        {
            phoneno = phoneno.toHash(R.SALT_SMS);
            passcode = passcode.toHash(R.SALT_SMS);

            var q = client.CreateDocumentQuery<SmsPasscode>(URI_SMSPCODE);
            var result = from c in q where (c.phoneno == phoneno && c.passcode == passcode) select c;

            return (result.Count() > 0);
        }

        public Contacts getContact(string userId)
        {
            var q = client.CreateDocumentQuery<Contacts>(URI_CONTACT);
            var result = from contact in q where contact.ownerId == userId select contact;

            return (result.Count() > 0) ? result.ToList().First() : null;
        }

        public void addFriends(string userId, List<Friend> friends)
        {
            var contact = getContact(userId);
            if (contact == null)
            {
                contact = new Contacts();
                contact.ownerId = userId;
                contact.friends = new List<Friend>();
            }

            // 去重
            contact.friends.AddRange(friends);
            contact.friends = contact.friends.Where((x, i) => contact.friends.FindLastIndex(z => z.userId == x.userId) == i).ToList();

            var res = client.UpsertDocumentAsync(URI_CONTACT, contact).Result;
        }

        public void delFriends(string userId, List<Friend> rmlist)
        {
            var contact = getContact(userId);
            if (contact == null)
            {
                contact = new Contacts();
                contact.ownerId = userId;
                contact.friends = new List<Friend>();
            }

            // 移除
            contact.friends = contact.friends.Where((x, i) => rmlist.FindIndex(z => z.userId == x.userId) < 0).ToList();

            var res = client.UpsertDocumentAsync(URI_CONTACT, contact).Result;
        }
        public Balance getBalance(string userId)
        {
            var q = client.CreateDocumentQuery<Balance>(URI_BALANCE);
            var result = from contact in q where contact.ownerId == userId select contact;

            return (result.Count() > 0) ? result.ToList().First() : null;
        }
        public void updateBalance(string userId, List<BalanceSlot> newBalances)
        {
            var balance = getBalance(userId);
            if (balance == null)
            {
                balance = new Balance();
                balance.ownerId = userId;
                balance.balances = new List<BalanceSlot>(){
                    new BalanceSlot{name=CURRENCY_NAME.CNY, balance="1000"},
                    new BalanceSlot{name=CURRENCY_NAME.USD, balance="1000"},
                    new BalanceSlot{name=CURRENCY_NAME.BTC, balance="1000"},
                    new BalanceSlot{name=CURRENCY_NAME.ETH, balance="1000"}
                };
            }

            // 去重
            balance.balances.AddRange(newBalances);
            balance.balances = balance.balances.Where((x, i) => balance.balances.FindLastIndex(z => z.name == x.name) == i).ToList();

            var res = client.UpsertDocumentAsync(URI_BALANCE, balance).Result;
        }

        /// <summary>
        /// 轉帳(付款)
        /// </summary>
        /// <param name="fromId"></param>
        /// <param name="toId"></param>
        /// <param name="ctype"></param>
        /// <param name="amount"></param>
        /// <returns>receiptId</returns>
        public string transfer(string fromId, string toId, CURRENCY_NAME ctype, decimal amount)
        {
            var ok = false;
            var receiptId = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            //get user
            var fromUser = getUserByUserId(fromId);
            var toUser = getUserByUserId(toId);

            //get user's balance
            var fromBSlot = getBalance(fromId)?.balances.Find(c => c.name.Equals(ctype));
            var toBSlot = getBalance(toId)?.balances.Find(c => c.name.Equals(ctype));

            //simulate transcation
            if (fromBSlot != null && toBSlot != null && !fromId.Equals(toId))
            {
                var from_balance = Decimal.Parse(fromBSlot.balance);
                var to_balance = Decimal.Parse(toBSlot.balance);
                if (from_balance >= amount && amount > 0)
                {
                    fromBSlot.balance = (from_balance - amount).ToString();
                    toBSlot.balance = (to_balance + amount).ToString();

                    updateBalance(fromId, new List<BalanceSlot> { fromBSlot });
                    updateBalance(toId, new List<BalanceSlot> { toBSlot });
                    ok = true;
                }
            }

            //generate receipt
            var receipt = new
            {
                receiptId = receiptId,
                action = "transfer",
                status = ok ? 0 : -1,   //0 means done, <0 means failed(error code), other means processing
                message = "", //user message
                currency = ctype,
                amount = amount,
                fromUserId = fromUser.userId,
                fromUserName = fromUser.name,
                toUserId = toUser.userId,
                toUserName = toUser.name
            };

            //simulate receipt notification
            Task.Run(() =>
            {
                Task.Delay(200).Wait();
                //notify sender
                if (fromUser.ntfInfo != null)
                    notifications.sendMessage(fromId, fromUser.ntfInfo.pns, $"transfer out({(ok ? "okay" : "failure")})", "TX_RECEIPT", receipt);

                //notify receiver
                if (ok)
                {
                    if (toUser.ntfInfo != null)
                        notifications.sendMessage(toId, toUser.ntfInfo.pns, "transfer in", "TX_RECEIPT", receipt);
                }
            });

            return receiptId;
        }

        private void test()
        {
            // client.CreateDocumentAsync(URI_USER,
            //     new Models.Collections.User
            //     {
            //         name = "Jimx"
            //     });

            var q = client.CreateDocumentQuery<Models.Collections.User>(URI_USER);
            var u = from f in q where f.name == "Jim" select f;
            // u = q.Where(f => f.Name == "Jim");

            Console.WriteLine("======");
            Console.WriteLine(u.ToString());
            Console.WriteLine(JsonConvert.SerializeObject(u.ToList(), Formatting.Indented));

            u = from f in q where f.name == "Jim2" select f;
            Console.WriteLine("======");
            Console.WriteLine(u.ToString());
            Console.WriteLine(JsonConvert.SerializeObject(u.ToList(), Formatting.Indented));

            foreach (var user in u)
            {
                Console.WriteLine(JsonConvert.SerializeObject(user, Formatting.Indented));

                user.phoneno = "123";

                client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(R.DB_NAME, COL_USER, user.userId), user);
            }
        }


    }


}
