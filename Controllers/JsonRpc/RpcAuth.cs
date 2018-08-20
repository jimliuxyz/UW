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
                        user = new User()
                        {
                            phoneno = phoneno,
                            name = phoneno
                        };
                        if (db.upsertUser(user))
                            user = db.getUserByPhone(phoneno);
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
                                DateTime.UtcNow,
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

