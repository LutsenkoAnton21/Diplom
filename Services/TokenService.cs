using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Diplom.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace Diplom.Services
{
    public interface ITokenService
    {
        string BuildToken(User user);
        void AttachUserToContext(HttpContext context, string token);
    }
    public class TokenService : ITokenService
    {
        private const double EXPIRY_DURATION_MINUTES = 30;
        private readonly IConfiguration _config;
        private readonly UserService _userService;

        public TokenService(IConfiguration config, UserService userService)
        {
            _config = config;
            _userService = userService;
        }

        public void AttachUserToContext(HttpContext context, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"].ToString());
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                string userName = jwtToken.Claims.First(x => x.Type == ClaimTypes.Name).Value;

                // attach user to context on successful jwt validation
                context.Items["User"] = _userService.GetUserByEmail(userName);
            }
            catch
            {
                // do nothing if jwt validation fails
                // user is not attached to context so request won't have access to secure routes
            }
        }

        public string BuildToken(User user)
        {
            Claim[] claims = new[] {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier,
                Guid.NewGuid().ToString())
            };

            string key = _config["Jwt:Key"].ToString();
            string issuer = _config["Jwt:Issuer"].ToString();

            TimeSpan expiryDuration = TimeSpan.FromHours(1);

            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
            //JwtSecurityToken tokenDescriptor = new JwtSecurityToken(issuer, issuer, claims,
            //    expires: DateTime.Now.AddMinutes(EXPIRY_DURATION_MINUTES), signingCredentials: credentials);
            JwtSecurityToken tokenDescriptor = new JwtSecurityToken(issuer, issuer, claims,
                expires: DateTime.Now.Add(expiryDuration), signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}
