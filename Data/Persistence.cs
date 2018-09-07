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
using System.Diagnostics;

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

        //receipts
        private static string COL_RECEIPT = typeof(TxReceipt).Name;
        private static Uri URI_TXRECEIPT = UriFactory.CreateDocumentCollectionUri(R.DB_NAME, COL_RECEIPT);


        private Notifications notifications;

        public readonly DocumentClient client;
        public Persistence(Notifications notifications)
        {
            Console.WriteLine("====init db====");
            this.notifications = notifications;

            //get database client as a connection
            client = new DocumentClient(new Uri(R.DB_URI), R.DB_KEY);

            InitDB();
            Mockup();
        }

        public void InitDB()
        {
            //create database
            client.CreateDatabaseIfNotExistsAsync(new Database { Id = R.DB_NAME }).Wait();

            //create collections
            var defReqOpts = new RequestOptions { OfferThroughput = 400 }; //todo:實際運作400RU可能太小
            client.CreateDocumentCollectionIfNotExistsAsync(URI_DB,
                                new DocumentCollection { Id = COL_USER }, defReqOpts).Wait();
            client.CreateDocumentCollectionIfNotExistsAsync(URI_DB,
                                new DocumentCollection { Id = COL_SMSPCODE, DefaultTimeToLive = 60 * 60 }, defReqOpts).Wait(); //60min for a round
            client.CreateDocumentCollectionIfNotExistsAsync(URI_DB,
                                new DocumentCollection { Id = COL_CONTACT }, defReqOpts).Wait();
            client.CreateDocumentCollectionIfNotExistsAsync(URI_DB,
                                new DocumentCollection { Id = COL_BALANCE }, defReqOpts).Wait();
            client.CreateDocumentCollectionIfNotExistsAsync(URI_DB,
                                new DocumentCollection { Id = COL_RECEIPT }, defReqOpts).Wait();
        }

        public void Mockup()
        {
            var user1 = new
            {
                phoneno = "886986123456",
                name = "buzz",
                avatar = "https://ionicframework.com/dist/preview-app/www/assets/img/avatar-ts-buzz.png"
            };
            var user2 = new
            {
                phoneno = "886986123457",
                name = "jessie",
                avatar = "https://ionicframework.com/dist/preview-app/www/assets/img/avatar-ts-jessie.png"
            };

            foreach (var u in new dynamic[] { user1, user2 })
            {
                var user = new Models.Collections.User()
                {
                    userId = "tempid-" + u.phoneno, //todo : 暫時以phoneno綁定id 便於識別 (日後移除)
                    phoneno = u.phoneno,
                    name = u.name,
                    avatar = u.avatar,
                    currencies = new List<CurrencySettings>{
                                new CurrencySettings{
                                    name = D.CNY,
                                    order = 0,
                                    isDefault = true,
                                    isVisible = false
                                },
                                new CurrencySettings{
                                    name = D.USD,
                                    order = 1,
                                    isDefault = false,
                                    isVisible = false
                                },
                                new CurrencySettings{
                                    name = D.BTC,
                                    order = 2,
                                    isDefault = false,
                                    isVisible = false
                                },
                                new CurrencySettings{
                                    name = D.ETH,
                                    order = 3,
                                    isDefault = false,
                                    isVisible = false
                                }
                            }
                };
                if (upsertUser(user))
                    user = getUserByPhone(u.phoneno);

                var friends = new List<Friend> { };
                addFriends(user.userId, friends);
                updateBalance(user.userId, new List<BalanceSlot>());
            }
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

        public void addFriends(string userId, List<string> friends)
        {
            var list = getUserByUserId(friends.ToArray()).Select(u =>
                new Friend
                {
                    name = u.name,
                    avatar = u.avatar
                }
            ).ToList();
            addFriends(userId, list);
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

        public void delFriends(string userId, List<string> rmlist)
        {
            var contact = getContact(userId);
            if (contact == null)
            {
                contact = new Contacts();
                contact.ownerId = userId;
                contact.friends = new List<Friend>();
            }

            // 移除
            contact.friends = contact.friends.Where((x, i) => rmlist.FindIndex(z => z == x.userId) < 0).ToList();

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
                    new BalanceSlot{name=D.CNY, balance="1000"},
                    new BalanceSlot{name=D.USD, balance="1000"},
                    new BalanceSlot{name=D.BTC, balance="1000"},
                    new BalanceSlot{name=D.ETH, balance="1000"}
                };
            }

            // 去重
            balance.balances.AddRange(newBalances);
            balance.balances = balance.balances.Where((x, i) => balance.balances.FindLastIndex(z => z.name == x.name) == i).ToList();

            var res = client.UpsertDocumentAsync(URI_BALANCE, balance).Result;
        }

        /// <summary>
        /// 更新或新增交易收據
        /// </summary>
        /// <param name="receipt"></param>
        /// <returns></returns>
        public bool upsertReceipt(TxReceipt receipt)
        {
            try
            {
                var res = client.UpsertDocumentAsync(URI_TXRECEIPT, receipt).Result;
                return res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.Created;
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return false;
        }

        /// <summary>
        /// 轉帳(付款)
        /// </summary>
        /// <param name="fromId"></param>
        /// <param name="toId"></param>
        /// <param name="currency"></param>
        /// <param name="amount"></param>
        /// <returns>receiptId</returns>
        public string doTransfer(string fromId, string toId, string currency, decimal amount, string message)
        {
            var ok = false;
            var receiptId = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            //get user
            var fromUser = getUserByUserId(fromId);
            var toUser = getUserByUserId(toId);

            //get user's balance
            var fromBSlot = getBalance(fromId)?.balances.Find(c => c.name.Equals(currency));
            var toBSlot = getBalance(toId)?.balances.Find(c => c.name.Equals(currency));

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
                message, //user message
                currency = currency,
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
        public string doExchange(string userId, string fromCurrency, string toCurrency, decimal fromAmount, decimal toAmount, string message)
        {
            var ok = false;
            var receiptId = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            //get user
            var user = getUserByUserId(userId);
            var fromBSlot = getBalance(userId)?.balances.Find(c => c.name.Equals(fromCurrency, StringComparison.OrdinalIgnoreCase));
            var toBSlot = getBalance(userId)?.balances.Find(c => c.name.Equals(toCurrency, StringComparison.OrdinalIgnoreCase));

            if (user == null || fromBSlot == null || fromBSlot == null)
                throw new Exception();

            if (fromCurrency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
                throw new Exception();

            //simulate transcation
            {
                var from_balance = Decimal.Parse(fromBSlot.balance);
                var to_balance = Decimal.Parse(toBSlot.balance);
                if (from_balance < fromAmount)
                {
                    ok = false;
                    // throw new Exception("Insufficient balance");
                }
                else
                {
                    fromBSlot.balance = (from_balance - fromAmount).ToString();
                    toBSlot.balance = (to_balance + toAmount).ToString();

                    updateBalance(userId, new List<BalanceSlot> { fromBSlot });
                    updateBalance(userId, new List<BalanceSlot> { toBSlot });
                    ok = true;
                }
            }

            //generate receipt
            var param = new ExchangeParams
            {
                fromCurrency = fromCurrency,
                toCurrency = toCurrency,
                fromAmount = fromAmount
            };
            var receipt = new TxReceipt
            {
                receiptId = receiptId,
                executorId = userId,
                ownerId = userId,
                currency = fromCurrency,
                message = message,
                isParent = true,

                txAction = (int)TxAction.EXCHANGE,
                txStatusCode = ok ? 0 : -1,
                txStatusMsg = "",
                txParams = param,
                txResult = new TxResult
                {
                    outflow = true,
                    amount = fromAmount,
                    // balance = 0,
                }
            };
            var receiptTo = new TxReceipt
            {
                receiptId = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                executorId = userId,
                ownerId = userId,
                currency = toCurrency,
                message = message,
                isParent = false,

                txAction = (int)TxAction.EXCHANGE,
                txStatusCode = ok ? 0 : -1,
                txStatusMsg = "",
                txParams = param,
                txResult = new TxResult
                {
                    outflow = false,
                    amount = toAmount,
                    // balance = 0,
                }
            };
            //暫時直接先將receipt寫入db
            //for from currency
            upsertReceipt(receipt);

            //for to currency
            upsertReceipt(receiptTo);

            //simulate receipt notification
            Task.Run(() =>
            {
                Task.Delay(200).Wait();
                //notify sender
                if (user.ntfInfo != null)
                    notifications.sendMessage(userId, user.ntfInfo.pns, $"exchange({(ok ? "okay" : "failure")})", "TX_RECEIPT", receipt);
            });

            return receiptId;
        }


    }


}
