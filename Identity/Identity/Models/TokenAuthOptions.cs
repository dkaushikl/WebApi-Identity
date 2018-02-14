using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Models
{
    public static class TokenAuthOptions
    {
        public static string GenerateToken(string userName)
        {
            //var claims = new Claim[]
            //{
            //    new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            //    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            //    new Claim(JwtRegisteredClaimNames.Email, user.Email)

            //    new Claim(ClaimTypes.Name, username),
            //    new Claim(ClaimTypes.Name, username),
            //    new Claim(ClaimTypes.Role, roles),
            //    new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()),
            //    new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.Now.AddDays(1)).ToUnixTimeSeconds().ToString()),
            //};

            //var token = new JwtSecurityToken(new JwtHeader(new SigningCredentials(
            //        new SymmetricSecurityKey(Encoding.UTF8.GetBytes("DiscoDisco")),
            //        SecurityAlgorithms.HmacSha256)),
            //    new JwtPayload(claims));

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, userName),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("DiscoDiscoDiscoDiscoDiscoDisco"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken("demo.com", "demo.com", claims, DateTime.Now.AddMinutes(30), signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }


}
