using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UdemyAuthServer.Core.Configuration;
using UdemyAuthServer.Core.Dtos;
using UdemyAuthServer.Core.Models;
using UdemyAuthServer.Core.Repositories;
using UdemyAuthServer.Core.Services;
using UdemyAuthServer.Core.UnitOfWork;

namespace UdemyAuthServer.Service.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly List<Client> clients;
        private readonly ITokenService tokenService;
        private readonly UserManager<UserApp> userManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly IGenericRepository<UserRefreshToken> userRefreshTokenService;

        public AuthenticationService(IOptions<List<Client>> options, ITokenService tokenService, UserManager<UserApp> userManager, IUnitOfWork unitOfWork, IGenericRepository<UserRefreshToken> genericRepository)
        {
            clients = options.Value;
            this.tokenService = tokenService;
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;
            this.userRefreshTokenService = genericRepository;
        }
        public async Task<Response<TokenDto>> CreateTokenAsync(LoginDto loginDto)
        {
            if (loginDto ==null)
            {
                throw new ArgumentNullException(nameof(loginDto));
            }

            var user= await userManager.FindByEmailAsync(loginDto.Email);
            if (user==null)
            {
                return Response<TokenDto>.Fail("Email or Password is wrong", 400, true);
            }

            if (!await userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return Response<TokenDto>.Fail("Email or Password is wrong", 400, true);
            }

            var token = tokenService.CreateToken(user);

            var userRefreshToken= await userRefreshTokenService.Where(x=>x.UserId == user.Id).SingleOrDefaultAsync();

            if (userRefreshToken == null)
            {
                await userRefreshTokenService.AddAsync(new UserRefreshToken { Code = token.RefreshToken, UserId = user.Id, Expiration = token.RefreshTokenExpiration });
            }
            else
            {
                userRefreshToken.Code = token.RefreshToken;
                userRefreshToken.Expiration = token.RefreshTokenExpiration;
            }

            await unitOfWork.CommitAsync();

            return Response<TokenDto>.Success(token, 200);
        }

        public Response<ClientTokenDto> CreateTokenByClient(ClientLoginDto clientLoginDto)
        {
            var client = clients.SingleOrDefault(x => x.Id == clientLoginDto.ClientId && x.Secret == clientLoginDto.ClientSecret);

            if (client == null)
            {
                return Response<ClientTokenDto>.Fail("ClientId or ClientSecret not found", 404, true);
            }

            var token = tokenService.CreateTokenByClient(client);
            return Response<ClientTokenDto>.Success(token, 200);
        }

        public async Task<Response<TokenDto>> CreateTokenByRefreshTokenAsync(string refreshToken)
        {
            var existRefreshToken = await userRefreshTokenService.Where(x => x.Code == refreshToken).SingleOrDefaultAsync();

            if (existRefreshToken == null)
            {
                return Response<TokenDto>.Fail("RefreshToekn not found", 404, true);
            }

            var user = await userManager.FindByIdAsync(existRefreshToken.UserId);

            if (user == null)
            {
                return Response<TokenDto>.Fail("User not found", 404, true);
            }

            var tokenDto = tokenService.CreateToken(user);
            existRefreshToken.Code = tokenDto.RefreshToken;
            existRefreshToken.Expiration = tokenDto.RefreshTokenExpiration;

            await unitOfWork.CommitAsync();
            return Response<TokenDto>.Success(tokenDto, 200);
        }

        public async Task<Response<NoDataDto>> RevokeRefreshTokenAsync(string refreshToken)
        {
            var existRefreshToken = await userRefreshTokenService.Where(x => x.Code == refreshToken).SingleOrDefaultAsync();

            if (existRefreshToken == null)
            {
                return Response<NoDataDto>.Fail("Refresh Token not found", 404, true);
            }

            userRefreshTokenService.Remove(existRefreshToken);

            await unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(200);
        }
    }
}
