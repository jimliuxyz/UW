using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using UW.Data;

namespace UW.JWT
{
    public class ApiTokenValidator : ISecurityTokenValidator
    {
        public bool CanValidateToken => true;

        public int MaximumTokenSizeInBytes { get; set; }

        public bool CanReadToken(string securityToken)
        {
            return true;
        }

        /// <summary>
        /// 驗證與解析token
        /// </summary>
        /// <param name="securityToken"></param>
        /// <param name="validationParameters"></param>
        /// <param name="validatedToken"></param>
        /// <returns></returns>
        public ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                //驗證token
                ClaimsPrincipal principal = tokenHandler.ValidateToken(securityToken, validationParameters, out validatedToken);
                return principal;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }

            // ClaimsPrincipal principal;
            // try
            // {
            //     validatedToken = null;
            //     var token = new JwtSecurityToken(securityToken);
            //     var payload = token.Payload;
            //     var role = (from t in payload where t.Key == ClaimTypes.Role select t.Value).FirstOrDefault();
            //     var name = (from t in payload where t.Key == ClaimTypes.Name select t.Value).FirstOrDefault();
            //     var userid = (from t in payload where t.Key == KEYSTR.CLAIM_USERID select t.Value).FirstOrDefault();
            //     var issuer = token.Issuer;
            //     var key = token.SecurityKey;
            //     var audience = token.Audiences;
            //     var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);
            //     identity.AddClaim(new Claim(ClaimTypes.Name, name.ToString()));
            //     identity.AddClaim(new Claim(ClaimsIdentity.DefaultRoleClaimType, role.ToString()));
            //     identity.AddClaim(new Claim(KEYSTR.CLAIM_USERID, userid.ToString()));
            //     principal = new ClaimsPrincipal(identity);
            // }
            // catch (Exception e)
            // {
            //     Console.WriteLine(e.ToString());

            //     validatedToken = null;
            //     principal = null;
            // }

            // return principal;
        }
    }
}
