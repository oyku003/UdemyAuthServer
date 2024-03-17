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


builder.Services.AddIdentity<UserApp, IdentityRole>(Opt =>//�dentityroledan tureyne bi class yaz�p oray� da set edecek alan ol�utrabilrdik ama default verdik
{
    Opt.User.RequireUniqueEmail = true;
    Opt.Password.RequireNonAlphanumeric = false;//*,? = vs zorunlu olmas�n diye
}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();//�ifre s�f�rlama gibi i�lemlerde default token �retmek i�in set ettik.
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"), sqlOptions =>
    {
        sqlOptions.MigrationsAssembly("UdemyAuthServer.Data");//assembly ismini verdik migrationlar�n bruada olu�mas� i�in
    });
});
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.Configure<CustomTokenOption>(builder.Configuration.GetSection("TokenOption"));
var tokenOptions = builder.Configuration.GetSection("TokenOption").Get<CustomTokenOption>();
builder.Services.Configure<List<Client>>(builder.Configuration.GetSection("Clients"));

builder.Services.AddAuthentication(opt =>
{
   opt.DefaultAuthenticateScheme=JwtBearerDefaults.AuthenticationScheme;//birden fazla �emam olsayd� belirtecektik bir adet oldu�u i�in default verdik. �ema kavram� �rne�in bayiler ve normal �yeler i�in ayr� auth durumum varsa
    opt.DefaultChallengeScheme=JwtBearerDefaults.AuthenticationScheme;//2 �emay� birbiri ile konua�bilsin diye ayarlad�k (a�a��daki jwt ve yukardaki autj i�indeki �emalar)

}).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts =>
{
    opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters {
        ValidIssuer = tokenOptions.Issuer,
        ValidAudience = tokenOptions.Audience[0],
        ValidateIssuerSigningKey = true,//imza do�rulanacak
        ValidateAudience=true,//aud do�rulanacak
        ValidateIssuer=true,
        IssuerSigningKey= SharedLibrary.Services.SignService.GetSymmetricSecurityKey(tokenOptions.SecurityKey),
        ValidateLifetime = true, //token gecmi� mi gecerli mi
        ClockSkew=TimeSpan.Zero//token olu�turulkdu�unda default 5 dk verilir serverlar aras�nda da fark olur o yuzden default pay ekler tolere etmek i�in
                                };
});//requestin header�ndaki token� arayarak yani jwt ile do�rulama yap�yoruz o yuzden bunu yazd�k

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
