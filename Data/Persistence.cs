using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Newtonsoft.Json;
using UW.Shared.Persis.Collections;
using System.Net;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using UW.Shared;
using UW.Shared.Services;
using System.Threading;

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
        private static string COL_USER = typeof(UW.Shared.Persis.Collections.User).Name;
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
        private static string COL_TXRECEIPT = typeof(TxReceipt).Name;
        private static Uri URI_TXRECEIPT = UriFactory.CreateDocumentCollectionUri(R.DB_NAME, COL_TXRECEIPT);


        private Ntfy notifications;

        public readonly DocumentClient client;
        public Persistence(Ntfy notifications)
        {
            Console.WriteLine("====start db====");
            this.notifications = notifications;

            //get database client as a connection
            client = new DocumentClient(new Uri(R.DB_URI), R.DB_KEY);

            Console.WriteLine("====init====");
            InitDB();

            Console.WriteLine("====mockup====");
            Mockup();

            // Console.WriteLine("====clear====");
            // ClearDB();

            Console.WriteLine("====end db====");
        }

        private void InitDB()
        {
            try
            {
                this.client.ReadDatabaseAsync(URI_DB).Wait();
                return;
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode != HttpStatusCode.NotFound)
                    return;
            }
            catch (AggregateException e)
            {
                foreach (var ee in e.Flatten().InnerExceptions)
                {
                    if (ee is DocumentClientException)
                    {
                        DocumentClientException de = ee as DocumentClientException;
                        if (de.StatusCode != HttpStatusCode.NotFound)
                            return;
                    }
                }
            }
            //create database
            client.CreateDatabaseIfNotExistsAsync(new Database
            {
                Id = R.DB_NAME
            }).Wait();

            // create collections
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
                                new DocumentCollection { Id = COL_TXRECEIPT }, defReqOpts).Wait();
        }
        private void ClearDB()
        {
            ClearCollection(URI_USER).Wait();
            ClearCollection(URI_BALANCE).Wait();
            ClearCollection(URI_CONTACT).Wait();
            ClearCollection(URI_TXRECEIPT).Wait();
        }

        private async Task ClearCollection(Uri uri)
        {
            var docs = client.CreateDocumentQuery(uri, "select c._self, c.id from c", new FeedOptions() { EnableCrossPartitionQuery = true }).ToList();

            foreach (var doc in docs)
            {
                // Console.WriteLine(doc.id);
                await client.DeleteDocumentAsync(doc._self);
            }
        }
        private void Mockup()
        {
            var userEM = new
            {
                phoneno = "BANK",
                name = "BANK",
                avatar = "https://images.vexels.com/media/users/3/135318/isolated/preview/45939241515a561751ed6222f2012003-bank-square-icon-by-vexels.png"
            };
            var userGPK = new
            {
                phoneno = "GPK",
                name = "GPK",
                avatar = "https://www.bankatunited.com/UnitedBank/media/Homepage-Promos/small-business-icon.png?width=200&height=200&ext=.png"
            };
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

            foreach (var u in new dynamic[] { userEM, user1, user2 })
            {
                var user = new UW.Shared.Persis.Collections.User()
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
                updateBalance(user.userId, new Dictionary<string, decimal>());
            }
        }

        /// <summary>
        /// 取得所有使用者
        /// </summary>
        /// <returns></returns>
        public List<UW.Shared.Persis.Collections.User> getUsers()
        {
            var q = client.CreateDocumentQuery<UW.Shared.Persis.Collections.User>(URI_USER);
            var result = from user in q select user;

            return result.ToList();
        }

        /// <summary>
        /// 以電話號碼取得使用者列表
        /// </summary>
        /// <param name="phones"></param>
        /// <returns></returns>
        public List<UW.Shared.Persis.Collections.User> findUsersByPhone(List<string> phones)
        {
            var q = client.CreateDocumentQuery<UW.Shared.Persis.Collections.User>(URI_USER);
            var result = from user in q where phones.Contains(user.phoneno) select user;

            return result.ToList();
        }

        /// <summary>
        /// 以電話號碼取得使用者
        /// </summary>
        /// <param name="phoneno"></param>
        /// <returns></returns>
        public UW.Shared.Persis.Collections.User getUserByPhone(string phoneno)
        {
            var q = client.CreateDocumentQuery<UW.Shared.Persis.Collections.User>(URI_USER);
            var result = from user in q where user.phoneno == phoneno select user;

            return (result.Count() > 0) ? result.ToList().First() : null;
        }

        /// <summary>
        /// 以userId取得使用者
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public UW.Shared.Persis.Collections.User getUserByUserId(string userId)
        {
            var q = client.CreateDocumentQuery<UW.Shared.Persis.Collections.User>(URI_USER);
            var result = from user in q where user.userId == userId select user;

            return (result.Count() > 0) ? result.ToList().First() : null;
        }

        public List<UW.Shared.Persis.Collections.User> getUserByUserId(string[] userIds)
        {
            var q = client.CreateDocumentQuery<UW.Shared.Persis.Collections.User>(URI_USER);
            var result = from user in q where userIds.Contains(user.userId) select user;

            return result.ToList();
        }

        /// <summary>
        /// 更新或新增使用者
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool upsertUser(UW.Shared.Persis.Collections.User user)
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
        /// todo : 加加動作須確保`事務性` 避免程式攻擊
        /// </summary>
        /// <param name="phoneno"></param>
        /// <param name="passcode"></param>
        /// <returns></returns>
        public dynamic isSmsPasscodeMatched(string phoneno, string passcode)
        {
            var passed = false;
            var error = RPCERR.PASSCODE_EXPIRED;

            var phonenoHash = phoneno.ToHash(R.SALT_SMS);
            var passcodeHash = (phoneno + passcode).ToHash(R.SALT_SMS);

            // 取得SmsPasscode
            var q = client.CreateDocumentQuery<SmsPasscode>(URI_SMSPCODE);
            var result = from c in q where (c.phoneno == phonenoHash) select c;

            if (result.Count() > 0)
            {
                var sms = result.ToList().First();

                //是否在有效期間內
                if (sms.verifyAvailTime.CompareTo(DateTime.UtcNow) > 0)
                {
                    //是否嘗試3次內
                    if (sms.verifyCount < 3)
                    {
                        //是否正確
                        if (passcodeHash == sms.passcode)
                        {
                            passed = true;

                            //刪除sms
                            client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(R.DB_NAME, COL_SMSPCODE, sms.id)).Wait();
                        }
                        else
                        {
                            error = RPCERR.PASSCODE_MISMATCH;

                            //更新sms
                            sms.verifyCount++;
                            client.UpsertDocumentAsync(URI_SMSPCODE, sms).Wait();
                        }
                    }
                    else
                        error = RPCERR.PASSCODE_VERIFY_EXCEEDED;
                }
                else
                    error = RPCERR.PASSCODE_EXPIRED;
            }

            return new
            {
                passed,
                error
            };
        }

        public Contacts getContact(string userId)
        {
            var q = client.CreateDocumentQuery<Contacts>(URI_CONTACT);
            var result = from contact in q where contact.ownerId == userId select contact;

            return (result.Count() > 0) ? result.ToList().First() : null;
        }

        public void addFriends(string userId, List<string> friends)
        {
            //todo : are users allow to be friend
            var list = getUserByUserId(friends.ToArray())?.Select(u =>
                new Friend
                {
                    userId = u.userId,
                    name = u.name,
                    avatar = u.avatar,
                    favorite = false
                }
            ).ToList();

            if (list != null)
                addFriends(userId, list);
        }
        private void addFriends(string userId, List<Friend> friends)
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

        public void setFriendFavorite(string userId, string friendId, bool favorite)
        {
            var contact = getContact(userId);
            if (contact == null)
                return;

            var friend = contact.friends.First(u => u.userId == friendId);
            friend.favorite = favorite;

            var res = client.UpsertDocumentAsync(URI_CONTACT, contact).Result;
        }

        public Balance getBalance(string userId)
        {
            var q = client.CreateDocumentQuery<Balance>(URI_BALANCE);
            var result = from contact in q where contact.ownerId == userId select contact;

            return (result.Count() > 0) ? result.ToList().First() : null;
        }
        public void updateBalance(string userId, Dictionary<string, decimal> newBalances)
        {
            var balance = getBalance(userId);
            if (balance == null)
            {
                var init_balance = new Dictionary<string, decimal>() {
                    {D.BTC, 1000},
                    {D.CNY, 1000},
                    {D.ETH, 1000},
                    {D.USD, 1000},
                };
                balance = new Balance();
                balance.ownerId = userId;
                balance.balances = init_balance;
            }
            foreach (var kp in newBalances)
            {
                balance.balances[kp.Key] = kp.Value;
            }

            var res = client.UpsertDocumentAsync(URI_BALANCE, balance).Result;
        }

        public List<TxReceipt> getReceipts(string userId, DateTime datetime)
        {
            var q = client.CreateDocumentQuery<TxReceipt>(URI_TXRECEIPT);
            var result = from rec in q where rec.ownerId == userId && rec.datetime >= datetime select rec;

            return result.ToList();
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
                // Console.WriteLine(URI_TXRECEIPT.ToJson());
                // Console.WriteLine(receipt.ToJson());

                var res = client.UpsertDocumentAsync(URI_TXRECEIPT, receipt).Result;
                return res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.Created;
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return false;
        }

        public async Task<bool> doWithdraw(string userId, string receiptId, string currency, decimal amount, string message)
        {
            var ok = false;
            var fromUser = getUserByUserId(userId);
            var toUser = getUserByPhone("BANK");

            string fromId = fromUser.userId;
            string toId = toUser.userId;

            return await doTransfer(fromId, toId, receiptId, currency, amount, message);

            // var from_balance = getBalance(fromUser.userId).balances[currency];
            // var to_balance = getBalance(toUser.userId).balances[currency];
            // if (amount > 0 && from_balance >= amount)
            // {
            //     from_balance -= amount;
            //     updateBalance(fromUser.userId, new Dictionary<string, decimal> { { currency, from_balance } });
            //     ok = true;
            // }

            // //generate sender receipt
            // var param = new TxParams
            // {
            //     sender = fromUser.userId,
            //     receiver = toUser.userId,
            //     currency = currency,
            //     amount = amount
            // };
            // var sender_rec = new TxReceipt
            // {
            //     receiptId = receiptId,
            //     executorId = fromUser.userId,
            //     ownerId = fromUser.userId,
            //     currency = currency,
            //     message = message,
            //     isParent = true,

            //     txType = TxType.WITHDRAW,
            //     statusCode = ok ? 0 : -1,
            //     statusMsg = "",
            //     txParams = param,
            //     txResult = !ok ? null : new TxActResult
            //     {
            //         outflow = true,
            //         amount = amount,
            //         balance = from_balance
            //     }
            // };
            // upsertReceipt(sender_rec);

            // var receiver_rec = sender_rec.Derivative(sender_rec.currency, toUser.userId, new TxActResult
            // {
            //     outflow = false,
            //     amount = amount,
            //     balance = to_balance
            // });
            // upsertReceipt(receiver_rec);

            // return sender_rec;
        }
        public async Task<bool> doDeposit(string userId, string receiptId, string currency, decimal amount, string message)
        {
            var ok = false;
            var fromUser = getUserByPhone("BANK");
            var toUser = getUserByUserId(userId);

            string fromId = fromUser.userId;
            string toId = toUser.userId;

            return await doTransfer(fromId, toId, receiptId, currency, amount, message);

            // var from_balance = getBalance(fromUser.userId).balances[currency];
            // var to_balance = getBalance(toUser.userId).balances[currency];
            // if (amount > 0)
            // {
            //     to_balance += amount;
            //     ok = true;
            //     updateBalance(toUser.userId, new Dictionary<string, decimal> { { currency, to_balance } });
            // }

            // //generate sender receipt
            // var param = new TxParams
            // {
            //     sender = fromUser.userId,
            //     receiver = toUser.userId,
            //     currency = currency,
            //     amount = amount
            // };
            // var sender_rec = new TxReceipt
            // {
            //     receiptId = receiptId,
            //     executorId = toUser.userId,
            //     ownerId = toUser.userId,
            //     currency = currency,
            //     message = message,
            //     isParent = true,

            //     txType = TxType.DEPOSIT,
            //     statusCode = ok ? 0 : -1,
            //     statusMsg = "",
            //     txParams = param,
            //     txResult = !ok ? null : new TxActResult
            //     {
            //         outflow = true,
            //         amount = amount,
            //         balance = from_balance
            //     }
            // };
            // upsertReceipt(sender_rec);

            // var receiver_rec = sender_rec.Derivative(sender_rec.currency, toUser.userId, new TxActResult
            // {
            //     outflow = false,
            //     amount = amount,
            //     balance = to_balance
            // });
            // upsertReceipt(receiver_rec);

            // return receiver_rec;
        }

        /// <summary>
        /// 轉帳(付款)
        /// </summary>
        /// <param name="fromId"></param>
        /// <param name="toId"></param>
        /// <param name="currency"></param>
        /// <param name="amount"></param>
        /// <returns>receiptId</returns>
        public async Task<bool> doTransfer(string fromId, string toId, string receiptId, string currency, decimal amount, string message)
        {
            var ok = false;

            //get user
            var fromUser = getUserByUserId(fromId);
            var toUser = getUserByUserId(toId);

            //get user's balance
            var from_balance = getBalance(fromId).balances[currency];
            var to_balance = getBalance(toId).balances[currency];

            //simulate transcation
            if (!fromId.Equals(toId))
            {
                if (amount > 0 && (from_balance >= amount))
                {
                    from_balance = from_balance - (fromUser.phoneno == "BANK" ? 0 : amount);
                    to_balance = to_balance + amount;

                    updateBalance(fromId, new Dictionary<string, decimal> { { currency, from_balance } });
                    updateBalance(toId, new Dictionary<string, decimal> { { currency, to_balance } });
                    ok = true;
                }
            }

            //generate sender receipt
            var param = new TxParams
            {
                sender = fromUser.userId,
                receiver = toUser.userId,
                currency = currency,
                amount = amount
            };
            var sender_rec = new TxReceipt
            {
                receiptId = receiptId,
                executorId = fromUser.userId,
                ownerId = fromUser.userId,
                currency = currency,
                message = message,
                isParent = true,

                txType = TxType.TRANSFER,
                statusCode = ok ? 0 : -1,
                statusMsg = "",
                txParams = param,
                txResult = !ok ? null : new TxActResult
                {
                    outflow = true,
                    amount = amount,
                    balance = from_balance
                }
            };
            upsertReceipt(sender_rec);

            //simulate receipt notification
            new Thread(() =>
            {
                Task.Delay(10).Wait();
        //notify sender
        // if (fromUser.ntfInfo != null)
        //     notifications.sendMessage(fromId, fromUser.ntfInfo.pns, $"transfer out({(ok ? "okay" : "failure")})", D.NTFTYPE.TXRECEIPT, new { list = new List<dynamic>() { sender_rec.ToApiResult() } });

        //notify receiver
        if (ok && toUser.ntfInfo != null)
                {
            //generate receiver receipt
            var receiver_rec = sender_rec.Derivative(sender_rec.currency, toUser.userId, new TxActResult
                    {
                        outflow = false,
                        amount = amount,
                        balance = to_balance
                    });
                    upsertReceipt(receiver_rec);

                    notifications.sendMessage(toId, toUser.ntfInfo.pns, "transfer in", D.NTFTYPE.TXRECEIPT, new { list = new List<dynamic>() { receiver_rec.ToApiResult() } });
                }
            }).Start();

            return ok;
        }
        public async Task<bool> doExchange(string userId, string receiptId, string fromCurrency, string toCurrency, decimal fromAmount, decimal toAmount, string message)
        {
            var ok = false;

            //get user
            var user = getUserByUserId(userId);
            var from_balance = getBalance(userId).balances[fromCurrency];
            var to_balance = getBalance(userId).balances[toCurrency];

            if (user == null)
                throw new Exception();

            if (fromCurrency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
                throw new Exception();

            //simulate transcation
            {
                if (from_balance < fromAmount)
                {
                    ok = false;
                    // throw new Exception("Insufficient balance");
                }
                else
                {
                    from_balance = from_balance - fromAmount;
                    to_balance = to_balance + toAmount;

                    updateBalance(userId, new Dictionary<string, decimal> { { fromCurrency, from_balance } });
                    updateBalance(userId, new Dictionary<string, decimal> { { toCurrency, to_balance } });
                    ok = true;
                }
            }

            //generate receipt
            var receipt = new TxReceipt
            {
                receiptId = receiptId,
                executorId = userId,
                ownerId = userId,
                currency = fromCurrency,
                message = message,
                isParent = true,

                txType = TxType.EXCHANGE,
                statusCode = ok ? 0 : -1,
                statusMsg = "",
                txParams = new ExchangeParams
                {
                    sender = userId,
                    receiver = userId,
                    currency = fromCurrency,
                    amount = fromAmount,
                    toCurrency = toCurrency
                },
                txResult = !ok ? null : new TxActResult
                {
                    outflow = true,
                    amount = fromAmount,
                    balance = from_balance,
                }
            };

            var receiptTo = receipt.Derivative(toCurrency, userId, new TxActResult
            {
                outflow = false,
                amount = toAmount,
                balance = to_balance
            });

            //暫時直接先將receipt寫入db
            //for from currency
            upsertReceipt(receipt);

            //for to currency
            if (ok)
                upsertReceipt(receiptTo);

            //simulate receipt notification
            // Task.Run(() =>
            // {
            //     Task.Delay(200).Wait();
            //     //notify sender
            //     if (user.ntfInfo != null)
            //     {
            //         var list = new List<dynamic>() { receipt.ToApiResult() };
            //         if (ok)
            //             list.Add(receiptTo);
            //         notifications.sendMessage(userId, user.ntfInfo.pns, $"exchange({(ok ? "okay" : "failure")})", D.NTFTYPE.TXRECEIPT, new { list });
            //     }
            // });

            return ok;
        }

    }

}
