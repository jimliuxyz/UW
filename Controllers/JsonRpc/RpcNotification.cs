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
    public class RpcNotification : RpcBaseController
    {
        private IHttpContextAccessor accessor;
        private Ntfy notifications;
        private Persistence db;
        public RpcNotification(IHttpContextAccessor accessor, Ntfy notifications, Persistence db)
        {
            this.accessor = accessor;
            this.notifications = notifications;
            this.db = db;
        }

        /// <summary>
        /// 向azure notification hub註冊user device的pns與其token
        /// </summary>
        /// <param name="pns"></param>
        /// <param name="pnsToken"></param>
        /// <returns></returns>
        public async Task<IRpcMethodResult> regPnsToken(PNS pns, string pnsToken, User user = null)
        {
            pnsToken = pnsToken.Trim();

            if (user == null)
            {
                var userId = this.accessor.HttpContext.User.FindFirst(c => c.Type == D.CLAIM.USERID).Value;
                user = db.getUserByUserId(userId);
            }

            if (user != null)
            {
                var regId = await this.notifications.updateRegId(user.userId, pns, pnsToken);

                user.ntfInfo = user.ntfInfo ?? new NtfInfo();
                user.ntfInfo.pns = pns;
                user.ntfInfo.pnsRegId = pnsToken;
                user.ntfInfo.azureRegId = regId;

                if (db.upsertUser(user))
                    return Ok(null);
            }
            return ERROR_ACT_FAILED;
        }

        /// <summary>
        /// 發送通知給特定user
        /// todo : 僅用於測試,待移除或權限控管
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public IRpcMethodResult sendMessage(string userId, string message)
        {
            var user = db.getUserByUserId(userId);

            notifications.sendMessage(userId, user.ntfInfo.pns, message);
            return Ok();
        }

        /// <summary>
        /// 發送通知給所有user
        /// todo : 僅用於測試,待移除或權限控管
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public IRpcMethodResult broadcast(string message)
        {
            notifications.broadcast(message);
            return Ok();
        }
    }
}

