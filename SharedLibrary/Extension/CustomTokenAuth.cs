using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Configurations;
using SharedLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Extension
{
    public static class CustomTokenAuth
    {
        public static void AddCustomTokenAuth(this IServiceCollection services, CustomTokenOption tokenOptions)
        {
            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;//birden fazla þemam olsaydý belirtecektik bir adet olduðu için default verdik. þema kavramý örneðin bayiler ve normal üyeler için ayrý auth durumum varsa
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;//2 þemayý birbiri ile konuaþbilsin diye ayarladýk (aþaðýdaki jwt ve yukardaki autj içindeki þemalar)

            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts =>
            {
                opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidIssuer = tokenOptions.Issuer,
                    ValidAudience = tokenOptions.Audience[0],
                    ValidateIssuerSigningKey = true,//imza doðrulanacak
                    ValidateAudience = true,//aud doðrulanacak
                    ValidateIssuer = true,
                    IssuerSigningKey = SignService.GetSymmetricSecurityKey(tokenOptions.SecurityKey),
                    ValidateLifetime = true, //token gecmiþ mi gecerli mi
                    ClockSkew = TimeSpan.Zero//token oluþturulkduðunda default 5 dk verilir serverlar arasýnda da fark olur o yuzden default pay ekler tolere etmek için
                };
            });//requestin headerýndaki tokený arayarak yani jwt ile doðrulama yapýyoruz o yuzden bunu yazdýk
        }
    }
}
