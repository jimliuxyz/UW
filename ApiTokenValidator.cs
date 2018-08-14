using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

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

        //parse user token
        public ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            ClaimsPrincipal principal;
            try
            {
                validatedToken = null;
                var token = new JwtSecurityToken(securityToken);
                var payload = token.Payload;
                var role = (from t in payload where t.Key == ClaimTypes.Role select t.Value).FirstOrDefault();
                var name = (from t in payload where t.Key == ClaimTypes.Name select t.Value).FirstOrDefault();
                var userid = (from t in payload where t.Key == "userid" select t.Value).FirstOrDefault();
                var issuer = token.Issuer;
                var key = token.SecurityKey;
                var audience = token.Audiences;
                var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.Name, name.ToString()));
                identity.AddClaim(new Claim(ClaimsIdentity.DefaultRoleClaimType, role.ToString()));
                identity.AddClaim(new Claim("userid", userid.ToString()));
                principal = new ClaimsPrincipal(identity);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());

                validatedToken = null;
                principal = null;
            }

            return principal;
        }
    }
}