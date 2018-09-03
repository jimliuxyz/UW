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
        private decimal EXRATE_DIF = 5 / 1000m;

        //匯率
        //以台幣兌美元匯率為例 買30.295 賣30.985 (匯率買賣要從銀行的角度來看)
        //銀行賣美金 客戶30.985台幣可買到1美金
        //銀行買美金 客戶1美金可賣到30.295台幣
        private decimal BTC_USD = 1 / 6996.83m;
        private decimal ETH_USD = 1 / 281.53m;
        private decimal CNY_USD = 6.83m;
        private decimal TWD_USD = 30.71m;

        /// <summary>
        /// 取得匯率
        /// </summary>
        /// <returns></returns>
        public IRpcMethodResult getAll()
        {
            return Ok(new List<object>(){
                new{
                    name = CURRENCY_NAME.BTC.ToString(),
                    buy = BTC_USD*(1m-EXRATE_DIF),
                    sell = BTC_USD*(1m+EXRATE_DIF)
                },
                new{
                    name = CURRENCY_NAME.ETH.ToString(),
                    buy = ETH_USD*(1m-EXRATE_DIF),
                    sell = ETH_USD*(1m+EXRATE_DIF)
                },
                new{
                    name = CURRENCY_NAME.CNY.ToString(),
                    buy = CNY_USD*(1m-EXRATE_DIF),
                    sell = CNY_USD*(1m+EXRATE_DIF)
                }
            });
        }


        public IRpcMethodResult get(CURRENCY_NAME from, CURRENCY_NAME to)
        {
            // var exrate = 0;
            // if (from == CURRENCY_NAME.CNY)
            //     exrate = 0;

            // if (to == CURRENCY_NAME.USD)
            // {

            // }
            // else
            // {

            // }

            return Ok();
        }

    }
}

