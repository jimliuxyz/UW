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
    public class RpcNotification : RpcBaseController
    {
        private IHttpContextAccessor accessor;
        private Notifications notifications;
        private Persistence db;
        public RpcNotification(IHttpContextAccessor accessor, Notifications notifications, Persistence db)
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
        public async Task<IRpcMethodResult> regPnsToken(PNS pns, string pnsToken, string _userId=null)
        {
            try
            {
                            pnsToken = pnsToken.Trim();
            
            var userId = _userId ?? this.accessor.HttpContext.User.FindFirst(c => c.Type == KEYSTR.CLAIM_USERID).Value;
            var regId = await this.notifications.updateRegId(userId, pns, pnsToken);

            if (!string.IsNullOrEmpty(regId))
            {
                var noinfo = new NoHubInfo()
                {
                    ownerId = userId,
                    pns = pns,
                    pnsRegId = pnsToken,
                    azureRegId = regId
                };

                if (db.upsertNoHubInfo(noinfo))
                    return Ok(true);
            }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
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
        public async Task<IRpcMethodResult> sendMessage(string userId, string message)
        {
            var noinfo = db.getUserNoHubInfo(userId);
            if (noinfo != null)
            {
                notifications.sendMessage(userId, noinfo.pns, message);
                return Ok(true);
            }
            return ERROR_ACT_FAILED;
        }

        /// <summary>
        /// 發送通知給所有user
        /// todo : 僅用於測試,待移除或權限控管
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<IRpcMethodResult> broadcast(string message)
        {
            notifications.broadcast(message);
            return Ok(true);
        }
    }
}

