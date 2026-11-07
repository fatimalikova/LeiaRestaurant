using FluentValidation;
using FluentValidation.AspNetCore;
using Leia.Api.BackgroundServices;
using Leia.Api.Hubs;
using Leia.Application.Dtos.UserDtos;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Leia.Application.Models;
using Leia.Application.Profiles;
using Leia.Application.Settings;
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
using System.Threading.RateLimiting;

namespace Leia.Api
{
    public static class ServicesRegistration
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment)
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

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                        context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();

                    await context.HttpContext.Response.WriteAsJsonAsync(new ResponseModel<object>
                    {
                        Success = false,
                        Errors = new List<string> { "Too many requests. Please try again later." },
                        StatusCode = StatusCodes.Status429TooManyRequests
                    }, cancellationToken);
                };

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(http =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        $"global:{http.Connection.RemoteIpAddress}",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 300,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }));

                options.AddPolicy("auth", http =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        $"auth:{http.Connection.RemoteIpAddress}",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }));
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
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            services.Configure<JwtSettings>(configuration.GetSection("JWT"));
            var jwt = configuration.GetSection("JWT").Get<JwtSettings>() ?? new JwtSettings();

            if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
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
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
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

            services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

            services.AddAutoMapper(typeof(MapperProfile).Assembly);

            var allowFakePayments = environment.IsDevelopment()
                || (bool.TryParse(configuration["Payments:AllowFake"], out var allow) && allow);

            if (!allowFakePayments)
                throw new InvalidOperationException("No payment gateway is configured. Register a real gateway or set Payments:AllowFake=true.");

            services.AddScoped<IPaymentGateway, FakePaymentGateway>();

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
            services.AddScoped<IMenuOptionService, MenuOptionService>();
            services.AddScoped<IAllergenService, AllergenService>();
            services.AddScoped<ICouponService, CouponService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IOrderTimeoutService, OrderTimeoutService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<ISliderService, SliderService>();
            services.AddScoped<ISystemSettingService, SystemSettingService>();
            services.AddScoped<ICuisineService, CuisineService>();
            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<IFavoriteService, FavoriteService>();
            services.AddScoped<ITableService, TableService>();
            services.AddScoped<IReservationService, ReservationService>();
            services.AddScoped<IDashboardService, DashboardService>();

            services.AddSingleton<IRealtimeNotifier, SignalRNotifier>();
            services.AddSignalR();
            services.AddHostedService<OrderTimeoutWorker>();

            return services;
        }
    }
}