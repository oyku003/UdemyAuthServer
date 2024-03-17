using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharedLibrary.Configurations;
using SharedLibrary.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UdemyAuthServer.Core.Configuration;
using UdemyAuthServer.Core.Dtos;
using UdemyAuthServer.Core.Models;
using UdemyAuthServer.Core.Services;

namespace UdemyAuthServer.Service.Services
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<UserApp> userManager;
        private readonly CustomTokenOption customTokenOption;

        public TokenService(IOptions<CustomTokenOption> options, UserManager<UserApp> userManager)
        {
            customTokenOption = options.Value;
            this.userManager = userManager;
        }

        private string CreateRefreshToken()
        {
            var numberByte=new byte[32];
            using var rnd = RandomNumberGenerator.Create();
            rnd.GetBytes(numberByte);

            return Convert.ToBase64String(numberByte);
        }

        private async Task<IEnumerable<Claim>> GetClaim(UserApp user , List<string> audiences)//token payloadda olmasını istediklerimizi claim olarak ekledik.
        {
            var userRoles = await userManager.GetRolesAsync(user);
            var users =new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email), 
                new Claim(ClaimTypes.Name, user.UserName) ,
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),//jti-> kimliklendirme için id
                new Claim("city", user.City),//jti-> kimliklendirme için id
                new Claim("birth-date", user.BirthDate.ToString()),//jti-> kimliklendirme için id
            };

            users.AddRange(audiences.Select(x=> new Claim(JwtRegisteredClaimNames.Aud, x)));
            users.AddRange(userRoles.Select(x => new Claim(ClaimTypes.Role, x)));
            return users;
        }
        private IEnumerable<Claim> GetClaimsByClient(Client client)
        {
            var claims = new List<Claim>();
            claims.AddRange(client.Audiences.Select(x => new Claim(JwtRegisteredClaimNames.Aud, x)));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, client.Id.ToString()));//bu tokenı ne için oluşturdugumuz

            return claims;
        }
        public TokenDto CreateToken(UserApp userApp)
        {
            var accessTokenExpiration = DateTime.Now.AddMinutes(customTokenOption.AccessTokenExpiration);
            var refreshTokenExpiration = DateTime.Now.AddMinutes(customTokenOption.RefreshTokenExpiration);
            var securityKey = SignService.GetSymmetricSecurityKey(customTokenOption.SecurityKey);
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(issuer: customTokenOption.Issuer,
                expires:accessTokenExpiration,
                notBefore: DateTime.Now,
                claims: GetClaim(userApp, customTokenOption.Audience).Result,
                signingCredentials: signingCredentials
                );

            var handler = new JwtSecurityTokenHandler();
            var token = handler.WriteToken(jwtSecurityToken);
            var tokenDto = new TokenDto
            {
                AccessToken=token,
                RefreshToken=CreateRefreshToken(),
                AccessTokenExpiration=accessTokenExpiration,
                RefreshTokenExpiration = refreshTokenExpiration
            };

            return tokenDto;
        }

        public TokenDto CreateToken(Client client)
        {
            var accessTokenExpiration = DateTime.Now.AddMinutes(customTokenOption.AccessTokenExpiration);
            var securityKey = SignService.GetSymmetricSecurityKey(customTokenOption.SecurityKey);
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(issuer: customTokenOption.Issuer,
                expires: accessTokenExpiration,
                notBefore: DateTime.Now,
                claims:GetClaimsByClient(client),
                audience: customTokenOption.Audience.ToList().ToString()
                );

            var handler = new JwtSecurityTokenHandler();
            var token = handler.WriteToken(jwtSecurityToken);
            var tokenDto = new TokenDto
            {
                AccessToken = token,
                AccessTokenExpiration = accessTokenExpiration
            };

            return tokenDto;
        }
        public ClientTokenDto CreateTokenByClient(Client client)
        {
            var accessTokenExpiration = DateTime.Now.AddMinutes(customTokenOption.AccessTokenExpiration);
            var securityKey = SignService.GetSymmetricSecurityKey(customTokenOption.SecurityKey);
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(issuer: customTokenOption.Issuer,
                expires: accessTokenExpiration,
                notBefore: DateTime.Now,
                signingCredentials: signingCredentials,
               claims: GetClaimsByClient(client)
                );

            var handler = new JwtSecurityTokenHandler();
            var token = handler.WriteToken(jwtSecurityToken);
            var tokenDto = new ClientTokenDto
            {
                AccessToken = token,
                AccessTokenExpiration = accessTokenExpiration
            };

            return tokenDto;
        }
    }
}
