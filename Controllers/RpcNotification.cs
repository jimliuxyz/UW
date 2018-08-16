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

namespace UW.JsonRpc
{
    [Authorize]
    public class RpcNotification : RpcController
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

        public async Task<IRpcMethodResult> regPnsToken(PNS pns, string pnsToken)
        {
            pnsToken = pnsToken.Trim();
            
            var userId = this.accessor.HttpContext.User.FindFirst(c => c.Type == "userid").Value;
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
            return this.Error(JsonRpcErrCode.ACTION_FAILED, "action failed");
        }

        public async Task<IRpcMethodResult> sendMessage(string userId, string message)
        {
            var noinfo = db.getUserNoHubInfo(userId);
            if (noinfo != null)
            {
                notifications.sendMessage(userId, noinfo.pns, message);
                return Ok(true);
            }
            return this.Error(JsonRpcErrCode.ACTION_FAILED, "action failed");
        }

        public async Task<IRpcMethodResult> broadcast(string message)
        {
            notifications.broadcast(message);
            return Ok(true);
        }
    }
}

