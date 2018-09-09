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
using System.Security.Cryptography;

namespace UW.Controllers.JsonRpc
{
    public class RpcAuth : RpcBaseController
    {
        private IHttpContextAccessor accessor;
        private Notifications notifications;
        private Persistence db;
        public RpcAuth(IHttpContextAccessor accessor, Notifications notifications, Persistence db)
        {
            this.accessor = accessor;
            this.notifications = notifications;
            this.db = db;
        }

        /// <summary>
        /// 登入API service
        /// </summary>
        /// <param name="phoneno"></param>
        /// <param name="passcode"></param>
        /// <returns></returns>
        public async Task<IRpcMethodResult> login(string phoneno, string passcode, PNS pns, string pnsToken)
        {
            try
            {
                // 驗證sms passcode
                dynamic res = db.isSmsPasscodeMatched(phoneno, passcode);

                // todo : remove this
                Console.WriteLine("萬用passcode未移除!");
                if (passcode == "88888888")
                { }
                else

                // 驗證失敗
                if (!res.passed)
                    return Bad(res.error);

                // 建立或取得user
                var user = db.getUserByPhone(phoneno);
                if (user != null)
                {
                    // 通知前裝置必須登出
                    var ntfInfo = user.ntfInfo;
                    if (ntfInfo != null && (ntfInfo.pns != pns || ntfInfo.pnsRegId != pnsToken))
                    {
                        await Task.Run(() =>
                        {
                            notifications.sendMessage(user.userId, ntfInfo.pns, "someone logged into your account\\nyou've got to logout!(t1)", D.NTFTYPE.LOGOUT);
                            // 避免rpc時間差可能造成regPnsToken在sendMessage之前
                            Task.Delay(3000).Wait();
                        });
                    }

                    // 更新裝置pns token
                    if (ntfInfo == null || ntfInfo.pns != pns || ntfInfo.pnsRegId != pnsToken)
                    {
                        var nc = (RpcNotification)this.accessor.HttpContext.RequestServices.GetService(typeof(RpcNotification));
                        await nc.regPnsToken(pns, pnsToken, user);
                    }
                }
                else
                {
                    //todo : 暫時的假資料供測試
                    user = new User()
                    {
                        userId = "tempid-" + phoneno, //todo : 暫時以phoneno綁定id 便於識別 (日後移除)
                        phoneno = phoneno,
                        name = phoneno,
                        avatar = "https://ionicframework.com/dist/preview-app/www/assets/img/avatar-ts-woody.png",
                        currencies = new List<CurrencySettings>{
                                new CurrencySettings{
                                    name = D.CNY,
                                    order = 0,
                                    isDefault = true,
                                    isVisible = false
                                },
                                new CurrencySettings{
                                    name = D.USD,
                                    order = 1,
                                    isDefault = false,
                                    isVisible = false
                                },
                                new CurrencySettings{
                                    name = D.BTC,
                                    order = 2,
                                    isDefault = false,
                                    isVisible = false
                                },
                                new CurrencySettings{
                                    name = D.ETH,
                                    order = 3,
                                    isDefault = false,
                                    isVisible = false
                                }
                            }
                    };
                    if (db.upsertUser(user))
                        user = db.getUserByPhone(phoneno);

                    var nc = (RpcNotification)this.accessor.HttpContext.RequestServices.GetService(typeof(RpcNotification));
                    await nc.regPnsToken(pns, pnsToken, user);

                    db.addFriends(user.userId, new List<string> { });
                    db.updateBalance(user.userId, new Dictionary<string, decimal>());
                }

                // 發行token (JWT)
                var tokenRnd = F.NewGuid();
                var claims = new Claim[]{
                        new Claim(ClaimTypes.MobilePhone, phoneno),
                        new Claim(ClaimTypes.Name, user.name),
                        new Claim(ClaimTypes.Role, "User"),
                        new Claim(D.CLAIM.USERID, user.userId),
                        new Claim(D.CLAIM.TOKENRND, tokenRnd)
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

                // update token random
                user.tokenRnd = tokenRnd;
                db.upsertUser(user);

                return Ok(new { token = tokenStr });
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return ERROR_ACT_FAILED;
        }


        [Authorize]
        public IRpcMethodResult isTokenAvailable()
        {
            var userId = this.accessor.HttpContext.User.FindFirst(c => c.Type == D.CLAIM.USERID)?.Value;
            var tokenRnd = this.accessor.HttpContext.User.FindFirst(c => c.Type == D.CLAIM.TOKENRND)?.Value;
            var user = db.getUserByUserId(userId);

            return Ok(new
            {
                available = tokenRnd != null && user != null && tokenRnd == user.tokenRnd
            });
        }

        public IRpcMethodResult adminLogin()
        {
            var claims = new Claim[]{
                        new Claim(ClaimTypes.MobilePhone, "phoneno"),
                        new Claim(ClaimTypes.Name, "user.name"),
                        new Claim(ClaimTypes.Role, "Admin"),
                        new Claim(D.CLAIM.USERID, "user.userId"),
                        new Claim(D.CLAIM.TOKENRND, "tokenRnd")
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

            return Ok(new { token = tokenStr });
        }

        [Authorize(Roles = "Admin")]
        public IRpcMethodResult adminGetHello()
        {
            return Ok("hello world~~~~");
        }
    }
}

