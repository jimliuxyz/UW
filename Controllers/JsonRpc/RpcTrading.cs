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
using UW.Data;
using UW.Shared.Persis.Collections;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Newtonsoft.Json;
using UW.Shared;
using UW.Shared.Services;

namespace UW.Controllers.JsonRpc
{
    [Authorize]
    public class RpcTrading : RpcBaseController
    {
        private IHttpContextAccessor accessor;
        private Ntfy notifications;
        private Persistence db;
        public RpcTrading(IHttpContextAccessor accessor, Ntfy notifications, Persistence db)
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
            var userId = this.accessor.HttpContext.User.FindFirst(c => c.Type == D.CLAIM.USERID).Value;

            var balance = db.getBalance(userId);
            return Ok(new
            {
                list = balance?.balances
            });
        }

        /// <summary>
        /// 存錢
        /// </summary>
        /// <returns></returns>
        public IRpcMethodResult deposit(string currency, decimal amount)
        {
            if (amount <= 0)
                return ERROR_ACT_FAILED;

            var userId = this.accessor.HttpContext.User.FindFirst(c => c.Type == D.CLAIM.USERID).Value;

            var balance = db.getBalance(userId);

            db.updateBalance(userId, balance.balances);
            return Ok();
        }

        /// <summary>
        /// 轉帳/付款
        /// todo:transation搬移到cosmos的stored procedure
        /// </summary>
        /// <param name="currency"></param>
        /// <param name="amount"></param>
        /// <param name="toUserId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public IRpcMethodResult transfer(string toUserId, string currency, decimal amount, string message)
        {
            var userId = this.accessor.HttpContext.User.FindFirst(c => c.Type == D.CLAIM.USERID).Value;

            var receiptId = db.doTransfer(userId, toUserId, currency, amount, message);

            return Ok(new
            {
                receiptId = receiptId
            });
        }


        public IRpcMethodResult getReceipts(DateTime fromDatetime)
        {
            var userId = this.accessor.HttpContext.User.FindFirst(c => c.Type == D.CLAIM.USERID).Value;

            return Ok(new
            {
                list = db.getReceipts(userId, fromDatetime).Select(rec => rec.ToApiResult())
            });
        }
    }
}

