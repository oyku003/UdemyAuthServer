using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Configurations;
using SharedLibrary.Services;
using UdemyAuthServer.Core.Configuration;
using UdemyAuthServer.Core.Models;
using UdemyAuthServer.Core.Repositories;
using UdemyAuthServer.Core.Services;
using UdemyAuthServer.Core.UnitOfWork;
using UdemyAuthServer.Data;
using UdemyAuthServer.Data.Repositories;
using UdemyAuthServer.Service.Services;
using FluentValidation;
using UdemyAuthServer.API.Validations;
using UdemyAuthServer.Core.Dtos;
using SharedLibrary.Extension;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IServiceGeneric<,>), typeof(ServiceGeneric<,>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


builder.Services.AddIdentity<UserApp, IdentityRole>(Opt =>//ýdentityroledan tureyne bi class yazýp orayý da set edecek alan olþutrabilrdik ama default verdik
{
    Opt.User.RequireUniqueEmail = true;
    Opt.Password.RequireNonAlphanumeric = false;//*,? = vs zorunlu olmasýn diye
}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();//þifre sýfýrlama gibi iþlemlerde default token üretmek için set ettik.
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"), sqlOptions =>
    {
        sqlOptions.MigrationsAssembly("UdemyAuthServer.Data");//assembly ismini verdik migrationlarýn bruada oluþmasý için
    });
});
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.Configure<CustomTokenOption>(builder.Configuration.GetSection("TokenOption"));
var tokenOptions = builder.Configuration.GetSection("TokenOption").Get<CustomTokenOption>();
builder.Services.Configure<List<Client>>(builder.Configuration.GetSection("Clients"));

builder.Services.AddAuthentication(opt =>
{
   opt.DefaultAuthenticateScheme=JwtBearerDefaults.AuthenticationScheme;//birden fazla þemam olsaydý belirtecektik bir adet olduðu için default verdik. þema kavramý örneðin bayiler ve normal üyeler için ayrý auth durumum varsa
    opt.DefaultChallengeScheme=JwtBearerDefaults.AuthenticationScheme;//2 þemayý birbiri ile konuaþbilsin diye ayarladýk (aþaðýdaki jwt ve yukardaki autj içindeki þemalar)

}).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts =>
{
    opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters {
        ValidIssuer = tokenOptions.Issuer,
        ValidAudience = tokenOptions.Audience[0],
        ValidateIssuerSigningKey = true,//imza doðrulanacak
        ValidateAudience=true,//aud doðrulanacak
        ValidateIssuer=true,
        IssuerSigningKey= SharedLibrary.Services.SignService.GetSymmetricSecurityKey(tokenOptions.SecurityKey),
        ValidateLifetime = true, //token gecmiþ mi gecerli mi
        ClockSkew=TimeSpan.Zero//token oluþturulkduðunda default 5 dk verilir serverlar arasýnda da fark olur o yuzden default pay ekler tolere etmek için
                                };
});//requestin headerýndaki tokený arayarak yani jwt ile doðrulama yapýyoruz o yuzden bunu yazdýk

builder.Services.AddControllers();
builder.Services.AddTransient<IValidator<CreateUserDto>, CreateUserDtoValidator>();
builder.Services.UseCustomValidationResponse();
    
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseCustomException();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
