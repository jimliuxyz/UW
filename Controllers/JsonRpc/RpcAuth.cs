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

        /// <summary>
        /// 登入API service
        /// </summary>
        /// <param name="phoneno"></param>
        /// <param name="passcode"></param>
        /// <returns></returns>
        public IRpcMethodResult login(string phoneno, string passcode)
        {
            try
            {
                // 驗證sms passcode
                // todo : remove 8888
                if (db.isSmsPasscodeMatched(phoneno, passcode) || passcode == "8888")
                {
                    // 建立或取得user
                    var user = db.getUserByPhone(phoneno);
                    if (user == null)
                    {
                        //todo : 暫時的假資料供測試
                        user = new User()
                        {
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
                    var claims = new Claim[]{
                        new Claim(ClaimTypes.MobilePhone, phoneno),
                        new Claim(ClaimTypes.Name, user.name),
                        new Claim(ClaimTypes.Role, "User"),
                        new Claim(KEYSTR.CLAIM_USERID, user.userId)
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

                    return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return ERROR_ACT_FAILED;
        }

    }
}

