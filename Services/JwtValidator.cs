using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace UW
{
    public class JwtValidator : ISecurityTokenValidator
    {
        public bool CanValidateToken => true;

        public int MaximumTokenSizeInBytes { get; set; }

        public bool CanReadToken(string securityToken)
        {
            return _tokenHandler.CanReadToken(securityToken);
        }

        private JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();
        private static TokenValidationParameters _validationParameters = new TokenValidationParameters
        {
            ValidIssuer = R.JWT_ISSUER,
            ValidAudience = R.JWT_AUDIENCE,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(R.JWT_SECRET)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            RequireExpirationTime = false,
        };

        //parse user token
        public ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            try
            {
                ClaimsPrincipal principal = _tokenHandler.ValidateToken(securityToken, _validationParameters, out validatedToken);

                // var userId = principal.FindFirst(c => c.Type == D.CLAIM.USERID).Value;
                // var tokenRnd = principal.FindFirst(c => c.Type == D.CLAIM.TOKENRND).Value;

                return principal;
            }
            catch (Exception e)
            {
                throw e;
                // throw new UnauthorizedAccessException();
            }
        }
    }
}
