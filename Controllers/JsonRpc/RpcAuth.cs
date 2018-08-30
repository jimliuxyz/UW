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
        private JwtSettings setting;
        private IHttpContextAccessor accessor;
        private Notifications notifications;
        private Persistence db;
        public RpcAuth(IOptions<JwtSettings> options, IHttpContextAccessor accessor, Notifications notifications, Persistence db)
        {
            setting = options.Value;
            this.accessor = accessor;
            this.notifications = notifications;
            this.db = db;
        }

        private static string JWT_SALT = "ChHNrj.Be&%w>(*";
        private static string Hash(string context)
        {
            SHA256 sha256 = new SHA256CryptoServiceProvider();
            byte[] source = Encoding.Default.GetBytes(context + JWT_SALT);
            byte[] crypto = sha256.ComputeHash(source);
            return Convert.ToBase64String(crypto);
            // return context;
        }

        /// <summary>
        /// 登入API service
        /// </summary>
        /// <param name="phoneno"></param>
        /// <param name="passcode"></param>
        /// <returns></returns>
        public async Task<IRpcMethodResult> login(string phoneno, string passcode, PNS pns, string pngToken)
        {
            try
            {
                // 驗證sms passcode
                // todo : remove 8888
                if (db.isSmsPasscodeMatched(phoneno, passcode) || passcode == "8888")
                {
                    // 建立或取得user
                    var user = db.getUserByPhone(phoneno);
                    if (user != null)
                    {
                        // 通知前裝置必須登出
                        var noinfo = db.getUserNoHubInfo(user.userId);
                        if (noinfo != null && (noinfo.pns != pns || noinfo.pnsRegId != pngToken))
                            notifications.sendMessage(user.userId, noinfo.pns, "someone logged into your account\\nyou've got to logout!", KEYSTR.NOTIFY_LOGOUT);
                        
                        // 更新裝置pns token
                        if (noinfo == null || noinfo.pns != pns || noinfo.pnsRegId != pngToken)
                        {
                            var nc = (RpcNotification)this.accessor.HttpContext.RequestServices.GetService(typeof(RpcNotification));
                            await nc.regPnsToken(pns, pngToken, user.userId);
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
                                    name = CURRENCY_NAME.cny,
                                    order = 0,
                                    isDefault = true,
                                    isVisible = false
                                },
                                new CurrencySettings{
                                    name = CURRENCY_NAME.usd,
                                    order = 1,
                                    isDefault = false,
                                    isVisible = false
                                },
                                new CurrencySettings{
                                    name = CURRENCY_NAME.bitcoin,
                                    order = 2,
                                    isDefault = false,
                                    isVisible = false
                                },
                                new CurrencySettings{
                                    name = CURRENCY_NAME.ether,
                                    order = 3,
                                    isDefault = false,
                                    isVisible = false
                                }
                            }
                        };
                        if (db.upsertUser(user))
                            user = db.getUserByPhone(phoneno);

                        var nc = (RpcNotification)this.accessor.HttpContext.RequestServices.GetService(typeof(RpcNotification));
                        await nc.regPnsToken(pns, pngToken, user.userId);

                        var friends = new List<Friend>{
                                new Friend{
                                    userId = "mock-id-1",
                                    name = "buzz",
                                    avatar = "https://ionicframework.com/dist/preview-app/www/assets/img/avatar-ts-buzz.png"
                                },
                                new Friend{
                                    userId = "mock-id-2",
                                    name = "jessie",
                                    avatar = "https://ionicframework.com/dist/preview-app/www/assets/img/avatar-ts-jessie.png"
                                }
                            };

                        //todo : 測試用friend list
                        db.addFriends(user.userId, friends);
                        db.updateBalance(user.userId, new List<BalanceSlot>());
                    }

                    // 發行token (JWT)
                    var tokenRnd = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                    var claims = new Claim[]{
                        new Claim(ClaimTypes.MobilePhone, phoneno),
                        new Claim(ClaimTypes.Name, user.name),
                        new Claim(ClaimTypes.Role, "User"),
                        new Claim(KEYSTR.CLAIM_USERID, user.userId),
                        new Claim(KEYSTR.CLAIM_TOKEN_RND, tokenRnd)
                    };
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(setting.SecretKey));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var token = new JwtSecurityToken(
                                setting.Issuer,
                                setting.Audience,
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
            var userId = this.accessor.HttpContext.User.FindFirst(c => c.Type == KEYSTR.CLAIM_USERID)?.Value;
            var tokenRnd = this.accessor.HttpContext.User.FindFirst(c => c.Type == KEYSTR.CLAIM_TOKEN_RND)?.Value;
            var user = db.getUserByUserId(userId);

            return Ok(tokenRnd != null && user != null && tokenRnd == user.tokenRnd);
        }

        public IRpcMethodResult adminLogin()
        {
            var claims = new Claim[]{
                        new Claim(ClaimTypes.MobilePhone, "phoneno"),
                        new Claim(ClaimTypes.Name, "user.name"),
                        new Claim(ClaimTypes.Role, "Admin"),
                        new Claim(KEYSTR.CLAIM_USERID, "user.userId"),
                        new Claim(KEYSTR.CLAIM_TOKEN_RND, "tokenRnd")
                    };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(setting.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                        setting.Issuer,
                        setting.Audience,
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

