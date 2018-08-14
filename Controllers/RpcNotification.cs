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
            var userId = this.accessor.HttpContext.User.FindFirst(c => c.Type == "userid").Value;

            var regId = await this.notifications.updateRegId(pns, pnsToken, userId);

            if (string.IsNullOrEmpty(regId))
                return this.Error(JsonRpcErrCode.ACTION_FAILED, "action failed");
            else{

                // db.

            }
            return Ok(true);
        }

        public async Task sendTo(string message, string toUserPhoneNo){
            var user = db.getUser(toUserPhoneNo);
            

        }
    }
}

