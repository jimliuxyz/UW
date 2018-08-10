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

namespace UW.JsonRpc
{
    public class RpcAuth : RpcController
    {
        private JwtSettings setting;
        public RpcAuth(IOptions<JwtSettings> options)
        {
            setting = options.Value;
        }

        //issue user token
        public IRpcMethodResult login(string phoneno, string passcode, string devicetoken)
        {
            if (passcode == "3333")
            {
                DataEx.user[phoneno] = devicetoken;

                //todo : verify with database
                var claims = new Claim[]{
                    new Claim(ClaimTypes.MobilePhone, phoneno),
                    new Claim(ClaimTypes.Sid, phoneno),
                    new Claim(ClaimTypes.Name, phoneno),
                    new Claim(ClaimTypes.Role, "user"),
                    new Claim("devicetoken", devicetoken)
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
