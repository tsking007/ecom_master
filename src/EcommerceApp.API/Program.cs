using EcommerceApp.API.Filters;
using EcommerceApp.API.Middleware;
using EcommerceApp.API.Services;
using EcommerceApp.Application;
using EcommerceApp.Application.Common;
using EcommerceApp.Domain.Interfaces;
using EcommerceApp.Infrastructure;
using EcommerceApp.Infrastructure.Notifications;
using EcommerceApp.Infrastructure.Persistence;
using EcommerceApp.Infrastructure.Persistence.Seeders;
using EcommerceApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stripe;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"]
    ?? throw new InvalidOperationException("Stripe:SecretKey is not configured.");

// ── Application layer ─────────────────────────────────────────────────────
builder.Services.AddApplication();

// ── Infrastructure layer ──────────────────────────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ── HttpContext / CurrentUser ─────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// ── JWT Authentication ─────────────────────────────────────────────────────
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is not configured.");


builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        // Return JSON 401 instead of redirect — API clients expect JSON
        options.Events = new JwtBearerEvents
        {
            OnChallenge = ctx =>
            {
                ctx.HandleResponse();
                ctx.Response.StatusCode = 401;
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsync(
                    """{"statusCode":401,"message":"Unauthorized"}""");
            },
            OnForbidden = ctx =>
            {
                ctx.Response.StatusCode = 403;
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsync(
                    """{"statusCode":403,"message":"Forbidden"}""");
            }
        };
    });

builder.Services.AddAuthorization();

// ── Controllers with ValidateModelFilter ──────────────────────────────────
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidateModelFilter>();
});

builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IRateLimitService, RateLimitService>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Writes a consistent JSON body when the in-memory limiter rejects a request
    options.OnRejected = async (ctx, ct) =>
    {
        ctx.HttpContext.Response.ContentType = "application/json";

        if (ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            ctx.HttpContext.Response.Headers["Retry-After"] =
                ((int)retryAfter.TotalSeconds).ToString();

        await ctx.HttpContext.Response.WriteAsync(
            """{"statusCode":429,"message":"Too many requests. Please slow down."}""",
            ct);
    };

    // ── "products" policy: 60 req / min per IP ────────────────────────────────
    options.AddPolicy("products", httpContext =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 4,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // ── "search" policy: 30 req / min per IP ─────────────────────────────────
    options.AddPolicy("search", httpContext =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 4,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // ── "general" policy: 100 req / min per IP ────────────────────────────────
    // Applied to cart, wishlist, orders, banners
    options.AddPolicy("general", httpContext =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 4,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

// ── Swagger with JWT bearer ────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EcommerceApp API",
        Version = "v1"
    });

    // JWT bearer definition — enables the Authorize button in Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. " +
                       "Enter your token below (without the 'Bearer ' prefix).",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    // Apply globally — every endpoint shows the lock icon in Swagger UI
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


//frontend cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",   // Vite dev server
                "http://localhost:3000"    // fallback / production preview
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ── Build ──────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Middleware pipeline (order matters) ────────────────────────────────────

// 1. Correlation ID first — every subsequent log entry gets the ID
app.UseMiddleware<CorrelationIdMiddleware>();

// 2. Global exception handler — catches anything thrown below this point
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EcommerceApp v1");
        c.DisplayRequestDuration();
    });
}

app.UseCors("FrontendPolicy");

//app.UseHttpsRedirection();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// 3. Authentication — parses and validates the JWT
app.UseAuthentication();

// 4. Session validation — confirms session is not revoked in DB
app.UseMiddleware<SessionValidationMiddleware>();

// 5. Authorization — enforces [Authorize] attributes
app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        // Apply any pending EF migrations automatically
        await context.Database.MigrateAsync();

        // Seed admin user, banners, and sample products (idempotent)
        await DbSeeder.SeedAsync(context, logger);

        logger.LogInformation("Database migration and seeding completed.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database migration or seeding.");
        throw;
    }
}

app.Run();