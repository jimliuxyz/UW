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
using System.Security.Cryptography;
using UW.Core;
using UW.Core.Services;
using UW.Core.Persis;
using UW.Core.Misc;
using UW.Core.Persis.Helper;
using Microsoft.Extensions.Logging;

namespace UW.Controllers.JsonRpc2
{
    public class RpcAuth : RpcBaseController
    {
        private IHttpContextAccessor accessor;
        private Ntfy notifications;
        public RpcAuth(ILogger<RpcAuth> logger, IHttpContextAccessor accessor, Ntfy notifications)
        {
            this.accessor = accessor;
            this.notifications = notifications;
        }

        /// <summary>
        /// 登入API service
        /// </summary>
        /// <param name="phoneno"></param>
        /// <param name="passcode"></param>
        /// <returns></returns>
        public async Task<IRpcMethodResult> login(string phoneno, string passcode, PNS pns, string pnsToken)
        {
            var smsHelper = new SmsPasscodeHelper();
            var userHelper = new UserHelper();
            try
            {
                // todo : remove this
                Console.WriteLine("萬用passcode未移除!");
                if (passcode == "88888888")
                { }
                else
                {
                    // 驗證sms passcode
                    dynamic res = await smsHelper.IsSmsPasscodeMatched(phoneno, passcode);

                    // 驗證失敗
                    if (!res.passed)
                        return Bad(res.error);
                }

                // 取得user
                var user = await userHelper.GetByPhoneno(phoneno);
                var uid = (user != null) ? UserHelper.IdGen.Parse(user.userId) : userHelper.GenUid();

                var tokenRnd = F.NewGuid();
                var jwt = IssueToken(uid, tokenRnd);
                var jwtHash = tokenRnd.ToHash();  //todo : each user should have their own signature to verify

                //sign up
                if (user == null)
                {
                    // create new user
                    user = await userHelper.Create(uid, jwtHash, phoneno, phoneno);

                    // reg pns token
                    var nc = (RpcNotification)this.accessor.HttpContext.RequestServices.GetService(typeof(RpcNotification));
                    await nc.regPnsToken(pns, pnsToken, user);

                    // db.addFriends(user.userId, new List<string> { });
                    // db.updateBalance(user.userId, new Dictionary<string, decimal>());
                }
                //sign in
                else
                {
                    // 通知前裝置必須登出
                    var ntfInfo = user.ntfInfo;
                    if (ntfInfo != null && (ntfInfo.pns != pns || ntfInfo.pnsRegId != pnsToken))
                    {
                        await notifications.sendMessage(user.userId, ntfInfo.pns, "someone logged into your account\\nyou've got to logout!(t1)", D.NTFTYPE.LOGOUT);
                        // 避免rpc時間差可能造成regPnsToken在sendMessage之前
                        Task.Delay(3000).Wait();
                    }

                    // 更新裝置pns token
                    if (ntfInfo == null || ntfInfo.pns != pns || ntfInfo.pnsRegId != pnsToken)
                    {
                        var nc = (RpcNotification)this.accessor.HttpContext.RequestServices.GetService(typeof(RpcNotification));
                        await nc.regPnsToken(pns, pnsToken, user);
                    }

                    // update token hash
                    user.jwtHash = jwtHash;
                    await userHelper.Update(user);
                }

                return Ok(new { token = jwt });
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return ERROR_ACT_FAILED;
        }

        private static string IssueToken(Pkuid uid, string tokenRnd)
        {
            // 發行token (JWT)
            var claims = new Claim[]{
                        new Claim(ClaimTypes.Role, "User"),
                        new Claim(D.CLAIM.USERID, uid.Guid),
                        new Claim(D.CLAIM.TOKENRND, tokenRnd),
                    };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(R.JWT_SECRET));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                        R.JWT_ISSUER,
                        R.JWT_AUDIENCE,
                        claims,
                        null, //DateTime.UtcNow, //todo : 日後再決定是否每次token帶入時間加密
                        null,
                        creds);
            var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenStr;
        }


        [Authorize]
        public async Task<IRpcMethodResult> isTokenAvailable()
        {
            var userId = this.accessor.HttpContext.User.FindFirst(c => c.Type == D.CLAIM.USERID)?.Value;
            var tokenRnd = this.accessor.HttpContext.User.FindFirst(c => c.Type == D.CLAIM.TOKENRND)?.Value;

            var uid = UserHelper.IdGen.Parse(userId);

            var userHelper = new UserHelper();
            var available = await userHelper.IsTokenAvailable(uid, tokenRnd);
            return Ok(new
            {
                available = available
            });
        }

    }
}

