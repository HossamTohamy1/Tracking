using Infrastructure.Data;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Helpers
{
    public class GenerateToken
    {
        public static string Generate(int userID, string Name, string Role)
        {
            var key = Encoding.ASCII.GetBytes(Constants.SecretKey);
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = "",
                Audience = "",
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim("ID", userID.ToString()),
                    new System.Security.Claims.Claim(ClaimTypes.Name, Name),
                    new System.Security.Claims.Claim(ClaimTypes.Role, Role)
                }),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}