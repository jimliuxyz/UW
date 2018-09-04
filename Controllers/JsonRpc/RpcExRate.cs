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
    public class RpcExRate : RpcBaseController
    {
        private IHttpContextAccessor accessor;
        private Notifications notifications;
        private Persistence db;
        public RpcExRate(IHttpContextAccessor accessor, Notifications notifications, Persistence db)
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
        private static decimal USD_BTC = 1m / 6996.83m;
        private static decimal USD_ETH = 1m / 281.53m;
        private static decimal USD_CNY = 6.83m;
        private static decimal USD_TWD = 30.71m;

        private static Dictionary<string, decimal> rates = new Dictionary<string, decimal>();
        static RpcExRate()
        {
            // rates.Add(CURRENCY_NAME.USD+CURRENCY_NAME.USD, 0m);
        }


        /// <summary>
        /// 取得匯率
        /// </summary>
        /// <returns></returns>
        public IRpcMethodResult getAll()
        {
            return Ok(new List<object>(){
                new{
                    name = KEYSTR.BTC.ToString(),
                    buy = USD_BTC*(1m-EXRATE_DIF),
                    sell = USD_BTC*(1m+EXRATE_DIF)
                },
                new{
                    name = KEYSTR.ETH.ToString(),
                    buy = USD_ETH*(1m-EXRATE_DIF),
                    sell = USD_ETH*(1m+EXRATE_DIF)
                },
                new{
                    name = KEYSTR.CNY.ToString(),
                    buy = USD_CNY*(1m-EXRATE_DIF),
                    sell = USD_CNY*(1m+EXRATE_DIF)
                }
            });
        }

        /// <summary>
        /// 取得特定匯率
        /// </summary>
        /// <returns></returns>
        public IRpcMethodResult get(string from, string to)
        {
            return Ok(new
            {
                rate = getRate(from, to)
            });
        }

        /// <summary>
        /// 取得特定匯率
        /// </summary>
        /// <returns></returns>
        public static decimal getRate(string from, string to)
        {
            decimal exrate = 1;

            // Console.WriteLine("100 USD = " + (100m * USD_CNY.AsBuy(EXRATE_DIF)) + " CNY");
            // Console.WriteLine("100 USD = " + (100m / (1m / USD_CNY).AsSell(EXRATE_DIF)) + " CNY");

            if (from == KEYSTR.USD)
            {
                if (to == KEYSTR.CNY)
                    return (1m / USD_CNY).AsSell(EXRATE_DIF);
                else if (to == KEYSTR.BTC)
                    return (1m / USD_BTC).AsSell(EXRATE_DIF);
                else if (to == KEYSTR.ETH)
                    return (1m / USD_ETH).AsSell(EXRATE_DIF);
                else
                    throw new Exception($"Unknown Currency [{to}]");
            }
            if (to == KEYSTR.USD)
            {
                if (from == KEYSTR.CNY)
                    return USD_CNY.AsSell(EXRATE_DIF);
                else if (from == KEYSTR.BTC)
                    return USD_BTC.AsSell(EXRATE_DIF);
                else if (from == KEYSTR.ETH)
                    return USD_ETH.AsSell(EXRATE_DIF);
                else
                    throw new Exception($"Unknown Currency [{from}]");
            }

            if (from == KEYSTR.CNY)
                exrate = USD_CNY.AsSell(EXRATE_DIF);
            else if (from == KEYSTR.BTC)
                exrate = USD_BTC.AsSell(EXRATE_DIF);
            else if (from == KEYSTR.ETH)
                exrate = USD_ETH.AsSell(EXRATE_DIF);
            else
                throw new Exception($"Unknown Currency [{from}]");

            if (to == KEYSTR.CNY)
                exrate *= (1m / USD_CNY).AsSell(EXRATE_DIF);
            else if (to == KEYSTR.BTC)
                exrate *= (1m / USD_BTC).AsSell(EXRATE_DIF);
            else if (to == KEYSTR.ETH)
                exrate *= (1m / USD_ETH).AsSell(EXRATE_DIF);
            else
                throw new Exception($"Unknown Currency [{to}]");

            return exrate;
        }

    }
}

