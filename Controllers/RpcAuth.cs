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
    public class RpcAuth : RpcController
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

        //issue user token
        public IRpcMethodResult login(string phoneno, string passcode)
        {
            if (passcode == "3333")
            {
                //create user
                var user = db.getUser(phoneno);
                if (user == null)
                {
                    user = new User()
                    {
                        phoneno = phoneno,
                        name = phoneno
                    };
                    if (db.upsertUser(user))
                        user = db.getUser(phoneno);
                }
                Console.WriteLine("user : " + user.id);

                //make user token
                var claims = new Claim[]{
                    new Claim(ClaimTypes.MobilePhone, phoneno),
                    new Claim(ClaimTypes.Name, phoneno),
                    new Claim(ClaimTypes.Role, "user"),
                    new Claim("userid", user.id)
                };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(setting.SecretKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                            setting.Issuer,
                            setting.Audience,
                            claims,
                            null,
                            null,
                            creds);

                // return this.Ok("login okay");
                return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
            }

            return this.Error(JsonRpcErrCode.AUTHENTICATION_FAILED, "authentication failed");
        }

    }
}

