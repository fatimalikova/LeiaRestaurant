using FluentValidation;
using FluentValidation.AspNetCore;
using Leia.Application.Dtos.AccountDtos;
using Leia.Application.Dtos.UserDtos;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Leia.Application.Profiles;
using Leia.Domain.Entities;
using Leia.Infrastructure.Data;
using Leia.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Leia.Api
{
    public static class ServicesRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();

            services.AddValidatorsFromAssemblyContaining<RegisterValidator>();
            services.AddFluentValidationAutoValidation();

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray();

                    return new BadRequestObjectResult(
                        ResponseModelHelper.BadRequestResult<object>(errors));
                };
            });

            // Front üçün CORS policy
            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                    policy.WithOrigins(
                        "http://localhost:3000",
                        "http://localhost:5173",
                        "http://127.0.0.1:5500",
                        "http://localhost:5500")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
            });

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter only the JWT token. 'Bearer' will be added automatically."
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        new string[] {}
                    }
                });
            });

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            var jwtKey = configuration["JWT:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
                throw new InvalidOperationException("JWT:Key tapılmadı və ya 32 simvoldan qısadır. User Secrets-ə yaz.");

            services.AddAuthentication(options =>
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
                    ValidIssuer = configuration["JWT:Issuer"],
                    ValidAudience = configuration["JWT:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                            context.Token = accessToken;

                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAutoMapper(typeof(MapperProfile).Assembly);

            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddHttpClient<IFileUploadService, FileUploadService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IAppUserService, AppUserService>();
            services.AddScoped<IRestaurantService, RestaurantService>();
            services.AddScoped<IWorkingHourService, WorkingHourService>();
            services.AddScoped<IUnavailableDayService, UnavailableDayService>();
            services.AddScoped<IMenuCategoryService, MenuCategoryService>();
            services.AddScoped<IMenuItemService, MenuItemService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<ISliderService, SliderService>();
            services.AddScoped<ISystemSettingService, SystemSettingService>();

            return services;
        }
    }
}