using System.IdentityModel.Tokens.Jwt;
using System.Text;
using ArtisanHubs.Bussiness.Mapping;
using ArtisanHubs.Bussiness.Services;
using ArtisanHubs.Bussiness.Services.Accounts.Implements;
using ArtisanHubs.Bussiness.Services.Accounts.Interfaces;
using ArtisanHubs.Bussiness.Services.ArtistProfiles.Implements;
using ArtisanHubs.Bussiness.Services.ArtistProfiles.Interfaces;
using ArtisanHubs.Bussiness.Services.Carts.Implements;
using ArtisanHubs.Bussiness.Services.Carts.Interfaces;
using ArtisanHubs.Bussiness.Services.Categories.Implements;
using ArtisanHubs.Bussiness.Services.Categories.Interfaces;
using ArtisanHubs.Bussiness.Services.Forums.Implements;
using ArtisanHubs.Bussiness.Services.Forums.Interfaces;
using ArtisanHubs.Bussiness.Services.Payment;
using ArtisanHubs.Bussiness.Services.Products.Implements;
using ArtisanHubs.Bussiness.Services.Products.Interfaces;
using ArtisanHubs.Bussiness.Services.RefreshTokens;
using ArtisanHubs.Bussiness.Services.Shared;
using ArtisanHubs.Bussiness.Services.Tokens;
using ArtisanHubs.Bussiness.Services.WorkshopPackages.Implements;
using ArtisanHubs.Bussiness.Services.WorkshopPackages.Interfaces;
using ArtisanHubs.Bussiness.Settings;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Repositories.Accounts.Implements;
using ArtisanHubs.Data.Repositories.Accounts.Interfaces;
using ArtisanHubs.Data.Repositories.ArtistProfiles.Implements;
using ArtisanHubs.Data.Repositories.ArtistProfiles.Interfaces;
using ArtisanHubs.Data.Repositories.Carts.Implements;
using ArtisanHubs.Data.Repositories.Carts.Interfaces;
using ArtisanHubs.Data.Repositories.Categories.Implements;
using ArtisanHubs.Data.Repositories.Categories.Interfaces;
using ArtisanHubs.Data.Repositories.Forums.Implements;
using ArtisanHubs.Data.Repositories.Forums.Interfaces;
using ArtisanHubs.Data.Repositories.OrderDetails.Implements;
using ArtisanHubs.Data.Repositories.OrderDetails.Interfaces;
using ArtisanHubs.Data.Repositories.Orders.Implements;
using ArtisanHubs.Data.Repositories.Orders.Interfaces;
using ArtisanHubs.Data.Repositories.Products.Implements;
using ArtisanHubs.Data.Repositories.Products.Interfaces;
using ArtisanHubs.Data.Repositories.WorkshopPackages.Implements;
using ArtisanHubs.Data.Repositories.WorkshopPackages.Interfaces;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddDbContext<ArtisanHubsDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();

builder.Services.AddScoped<IArtistProfileRepository, ArtistProfileRepository>();
builder.Services.AddScoped<IArtistProfileService, ArtistProfileService>();

builder.Services.AddScoped<IWorkshopPackageRepository, WorkshopPackageRepository>();
builder.Services.AddScoped<IWorkshopPackageService, WorkshopPackageService>();

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartService, CartService>();

builder.Services.AddScoped<IForumTopicRepository, ForumTopicRepository>();
builder.Services.AddScoped<IForumTopicService, ForumTopicService>();

builder.Services.AddScoped<IForumThreadRepository, ForumThreadRepository>();
builder.Services.AddScoped<IForumThreadService, ForumThreadService>();

builder.Services.AddScoped<IForumPostRepository, ForumPostRepository>();
builder.Services.AddScoped<IForumPostService, ForumPostService>();

builder.Services.AddScoped<IFavoriteProductRepository, FavoriteProductRepository>();
builder.Services.AddScoped<IFavoriteProductService, FavoriteProductService>();

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddTransient<IEmailService, SendGridEmailService>();

builder.Services.AddScoped<IPasswordHasher<ArtisanHubs.Data.Entities.Account>, PasswordHasher<ArtisanHubs.Data.Entities.Account>>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<PhotoService>();
builder.Services.AddSingleton<PayOSService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<OrderPaymentService>();
builder.Services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
builder.Services.AddHttpClient<GHTKService>();
builder.Services.AddScoped<OrderDetailService>();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddAuthorization();

var cloudName = configuration["CloudinarySettings:CloudName"];
var apiKey = configuration["CloudinarySettings:ApiKey"];
var apiSecret = configuration["CloudinarySettings:ApiSecret"];

var account = new CloudinaryDotNet.Account(cloudName, apiKey, apiSecret);
var cloudinary = new Cloudinary(account);

builder.Services.AddSingleton(cloudinary);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:5173",
                    "https://localhost:5173",
                    "https://artisanhubs.azurewebsites.net"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

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
        ValidIssuer = configuration["Jwt:Issuer"],
        ValidAudience = configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
    };
});

// Swagger
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

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
