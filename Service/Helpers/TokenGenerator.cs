using Core.Entities;
using Microsoft.IdentityModel.Tokens;
using Service.Helpers;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Services.Helpers
{
    public static class TokenGenerator
    {
        public static (string, string) GetToken(ApplicationUser user, JwtSettings settings, string role)
        {
            //get app setting
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey));
            var min = Convert.ToDouble(settings.ExpiryTime);
            var expiry = DateTime.UtcNow.AddMinutes(min);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                        new(JwtRegisteredClaimNames.Sub, user.Id),
                        new(ClaimTypes.Role, role ),
                        new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new (ClaimTypes.NameIdentifier, user.Id),
                        new (ClaimTypes.Email, user.Email),
                        new (ClaimTypes.Name, user.UserName),
                }),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
                Issuer = settings.Site,
                Audience = settings.Audience,
                NotBefore = DateTime.UtcNow,
                Expires = expiry,
            };

            //generate token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return (tokenHandler.WriteToken(token), expiry.ToString());
        }
    }
}