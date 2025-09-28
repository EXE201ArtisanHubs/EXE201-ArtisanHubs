
using ArtisanHubs.Bussiness.Services.Accounts.Implements;
using ArtisanHubs.Bussiness.Services.Accounts.Interfaces;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Repositories.Accounts.Implements;
using ArtisanHubs.Data.Repositories.Accounts.Interfaces;
using Microsoft.EntityFrameworkCore;
using ArtisanHubs.Bussiness.Mapping;
using ArtisanHubs.Data.Repositories.ArtistProfiles.Interfaces;
using ArtisanHubs.Data.Repositories.ArtistProfiles.Implements;
using ArtisanHubs.Bussiness.Services.ArtistProfiles.Interfaces;
using ArtisanHubs.Bussiness.Services.ArtistProfiles.Implements;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ArtisanHubs.Bussiness.Services.Tokens;
using ArtisanHubs.Data.Repositories.WorkshopPackages.Interfaces;
using ArtisanHubs.Data.Repositories.WorkshopPackages.Implements;
using ArtisanHubs.Bussiness.Services.WorkshopPackages.Interfaces;
using ArtisanHubs.Bussiness.Services.WorkshopPackages.Implements;
using ArtisanHubs.Bussiness.Services.Categories.Implements;
using ArtisanHubs.Bussiness.Services.Categories.Interfaces;
using ArtisanHubs.Data.Repositories.Categories.Implements;
using ArtisanHubs.Data.Repositories.Categories.Interfaces;
using ArtisanHubs.Data.Repositories.Products.Interfaces;
using ArtisanHubs.Data.Repositories.Products.Implements;
using ArtisanHubs.Bussiness.Services.Products.Interfaces;
using ArtisanHubs.Bussiness.Services.Products.Implements;
namespace ArtisanHubs.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;
            builder.Services.AddDbContext<ArtisanHubsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

            builder.Services.AddScoped<IAccountRepository,AccountRepository>();
            builder.Services.AddScoped<IAccountService,AccountService>();

            builder.Services.AddScoped<IArtistProfileRepository,ArtistProfileRepository>();
            builder.Services.AddScoped<IArtistProfileService, ArtistProfileService>();

            builder.Services.AddScoped<IWorkshopPackageRepository,WorkshopPackageRepository>();
            builder.Services.AddScoped<IWorkshopPackageService, WorkshopPackageService>();

            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();

            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IProductService, ProductService>();

            builder.Services.AddScoped<ITokenService, TokenService>();

            builder.Services.AddControllers();
            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // 1. Th�m c?u h�nh Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            // 2. Th�m c?u h�nh JWT Bearer
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // T? c?p token
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true, // Y�u c?u token kh�ng h?t h?n
                    ValidateIssuerSigningKey = true, // Y�u c?u x�c th?c k� hi?u c?a ng??i ph�t h�nh

                    ValidIssuer = configuration["Jwt:Issuer"]!,
                    ValidAudience = configuration["Jwt:Audience"]!,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
                };
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
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
        }
    }
}
