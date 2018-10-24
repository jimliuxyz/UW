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
using UW.Core.Persis.Collections;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Newtonsoft.Json;
using UW.Core;
using UW.Core.Services;
using UW.Core.Persis;
using UW.Core.Persis.Helper;

namespace UW.Controllers.JsonRpc2
{
    [Authorize]
    public class RpcNotification : RpcBaseController
    {
        private IHttpContextAccessor accessor;
        private Ntfy notifications;
        public RpcNotification(IHttpContextAccessor accessor, Ntfy notifications)
        {
            this.accessor = accessor;
            this.notifications = notifications;
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

            var uid = (user != null) ? UserHelper.IdGen.Parse(user.userId) : UserHelper.IdGen.Parse(this.accessor.HttpContext.User.FindFirst(c => c.Type == D.CLAIM.USERID).Value);

            var azureRegId = await this.notifications.updateRegId(uid.Guid, pns, pnsToken);

            var userHelper = new UserHelper();
            await userHelper.UpdateNtfInfo(uid, pns, pnsToken, azureRegId);

            return Ok();
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
            var uid = UserHelper.IdGen.Parse(userId);

            var userHelper = new UserHelper();

            var user = await userHelper.GetById(uid);

            await notifications.sendMessage(user.userId, user.ntfInfo.pns, message);
            return Ok();
        }

        /// <summary>
        /// 發送通知給所有user
        /// todo : 僅用於測試,待移除或權限控管
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<IRpcMethodResult> broadcast(string message)
        {
            await notifications.broadcast(message);
            return Ok();
        }
    }
}

