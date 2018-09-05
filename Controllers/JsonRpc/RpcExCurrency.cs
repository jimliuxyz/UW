using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using EdjCase.JsonRpc.Router.Abstractions;
using EdjCase.JsonRpc.Router;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System;
using UW.Services;
using UW.Data;
using UW.Models.Collections;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Newtonsoft.Json;

namespace UW.Controllers.JsonRpc
{
    public static class Extensions
    {
        public static decimal AsSell(this decimal rate, decimal dif)
        {
            return rate * (1m + dif);
        }
        public static decimal AsBuy(this decimal rate, decimal dif)
        {
            return rate * (1m - dif);
        }
    }

    [Authorize]
    public class RpcExCurrency : RpcBaseController
    {
        private IHttpContextAccessor accessor;
        private Notifications notifications;
        private Persistence db;
        public RpcExCurrency(IHttpContextAccessor accessor, Notifications notifications, Persistence db)
        {
            this.accessor = accessor;
            this.notifications = notifications;
            this.db = db;
        }

        //匯差(exchange rate difference)
        private static decimal EXRATE_DIF = 5m / 1000m;

        //匯率
        //以台幣兌美元匯率為例 買30.295 賣30.985 (匯率買賣要從銀行的角度來看)
        //銀行賣美金 客戶30.985台幣可買到1美金
        //銀行買美金 客戶1美金可賣到30.295台幣
        //todo : 用1/n的方式求反向匯率應該不是準確的做法 應該用買/賣角度
        private static decimal USD_CNY = 6.83m;
        private static decimal BTC_USD = 6996.83m;
        private static decimal ETH_USD = 281.53m;

        private static Dictionary<string, decimal> rates = new Dictionary<string, decimal>();
        static RpcExCurrency()
        {
            rates.Add(STR.USD + STR.CNY + STR._SELL, USD_CNY.AsSell(EXRATE_DIF));
            rates.Add(STR.USD + STR.CNY + STR._BUY, USD_CNY.AsBuy(EXRATE_DIF));

            rates.Add(STR.BTC + STR.USD + STR._SELL, BTC_USD.AsSell(EXRATE_DIF));
            rates.Add(STR.BTC + STR.USD + STR._BUY, BTC_USD.AsBuy(EXRATE_DIF));

            rates.Add(STR.ETH + STR.USD + STR._SELL, ETH_USD.AsSell(EXRATE_DIF));
            rates.Add(STR.ETH + STR.USD + STR._BUY, ETH_USD.AsBuy(EXRATE_DIF));
        }


        /// <summary>
        /// 取得匯率
        /// </summary>
        /// <returns></returns>
        public IRpcMethodResult getExRate()
        {
            return Ok(rates);
        }

        public IRpcMethodResult doExFrom(string fromCurrency, string toCurrency, decimal fromAmount, string message)
        {
            var userId = this.accessor.HttpContext.User.FindFirst(c => c.Type == STR.CLAIM_USERID).Value;

            try
            {
                var toAmount = _estimateExFrom(fromCurrency, toCurrency, fromAmount);
                var receiptId = db.doExchange(userId, fromCurrency, toCurrency, fromAmount, toAmount, message);

                return Ok(new
                {
                    receiptId
                });
            }
            catch (System.Exception)
            {
                return ERROR_ACT_FAILED;
            }
        }

        public IRpcMethodResult estimateExFrom(string fromCurrency, string toCurrency, decimal fromAmount)
        {
            try
            {
                var amount = _estimateExFrom(fromCurrency, toCurrency, fromAmount);
                return Ok(new
                {
                    amount
                });
            }
            catch (System.Exception)
            {
                return ERROR_ACT_FAILED;
            }
        }

        /// <summary>
        /// 匯率換算預估
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="from_amount"></param>
        /// <returns></returns>
        public decimal _estimateExFrom(string from, string to, decimal from_amount)
        {
            from = from.ToUpper();
            to = to.ToUpper();

            var amount = from_amount;

            if (rates.ContainsKey(to + from + STR._SELL))
            {
                amount /= rates[to + from + STR._SELL];
            }
            else if (rates.ContainsKey(from + to + STR._BUY))
            {
                amount *= rates[from + to + STR._BUY];
            }
            else
            {
                //1st exchange : change to USD
                if (rates.ContainsKey(STR.USD + from + STR._SELL))
                    amount /= rates[STR.USD + from + STR._SELL];
                else if (rates.ContainsKey(from + STR.USD + STR._BUY))
                    amount *= rates[from + STR.USD + STR._BUY];
                else
                    throw new Exception($"can't exchange from {from} to {to}");

                //2nd exchange : change to `to` currency from USD
                if (rates.ContainsKey(to + STR.USD + STR._SELL))
                    amount /= rates[to + STR.USD + STR._SELL];
                else if (rates.ContainsKey(STR.USD + to + STR._BUY))
                    amount *= rates[STR.USD + to + STR._BUY];
                else
                    throw new Exception($"can't exchange from {from} to {to}");
            }

            return amount;
        }

    }
}

