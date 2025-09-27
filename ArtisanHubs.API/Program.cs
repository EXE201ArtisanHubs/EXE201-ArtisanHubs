using System.Text;
using ArtisanHubs.Bussiness.Mapping;
using ArtisanHubs.Bussiness.Services.Accounts.Implements;
using ArtisanHubs.Bussiness.Services.Accounts.Interfaces;
using ArtisanHubs.Bussiness.Services.ArtistProfiles.Implements;
using ArtisanHubs.Bussiness.Services.ArtistProfiles.Interfaces;
using ArtisanHubs.Bussiness.Services.Categories.Implements;
using ArtisanHubs.Bussiness.Services.Categories.Interfaces;
using ArtisanHubs.Bussiness.Services.Tokens;
using ArtisanHubs.Bussiness.Services.WorkshopPackages.Implements;
using ArtisanHubs.Bussiness.Services.WorkshopPackages.Interfaces;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Repositories.Accounts.Implements;
using ArtisanHubs.Data.Repositories.Accounts.Interfaces;
using ArtisanHubs.Data.Repositories.ArtistProfiles.Implements;
using ArtisanHubs.Data.Repositories.ArtistProfiles.Interfaces;
using ArtisanHubs.Data.Repositories.Categories.Implements;
using ArtisanHubs.Data.Repositories.Categories.Interfaces;
using ArtisanHubs.Data.Repositories.WorkshopPackages.Implements;
using ArtisanHubs.Data.Repositories.WorkshopPackages.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ArtisanHubsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IArtistProfileRepository, ArtistProfileRepository>();
builder.Services.AddScoped<IArtistProfileService, ArtistProfileService>();
builder.Services.AddScoped<IWorkshopPackageRepository, WorkshopPackageRepository>();
builder.Services.AddScoped<IWorkshopPackageService, WorkshopPackageService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPasswordHasher<Account>, PasswordHasher<Account>>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        RoleClaimType = "role",
        NameClaimType = "email"
    };
});

builder.Services.AddControllers();
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ArtisanHubs API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
