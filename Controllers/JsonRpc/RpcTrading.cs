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
    public class RpcTrading : RpcBaseController
    {
        private IHttpContextAccessor accessor;
        private Notifications notifications;
        private Persistence db;
        public RpcTrading(IHttpContextAccessor accessor, Notifications notifications, Persistence db)
        {
            this.accessor = accessor;
            this.notifications = notifications;
            this.db = db;
        }

        /// <summary>
        /// 取得balance (snapshot)
        /// </summary>
        /// <returns></returns>
        public IRpcMethodResult getBalances()
        {
            var userId = this.accessor.HttpContext.User.FindFirst(c => c.Type == KEYSTR.CLAIM_USERID).Value;

            var balance = db.getBalance(userId);
            return Ok(balance.balances);
        }

        /// <summary>
        /// 存錢
        /// todo:此api僅供測試,待移除
        /// </summary>
        /// <returns></returns>
        public IRpcMethodResult deposit(CURRENCY_NAME currency, string amount)
        {
            return Ok(true);
        }


    }
}

