using EcommerceApp.API.Filters;
using EcommerceApp.API.Middleware;
using EcommerceApp.API.Services;
using EcommerceApp.Application;
using EcommerceApp.Application.Common;
using EcommerceApp.Domain.Interfaces;
using EcommerceApp.Infrastructure;
using EcommerceApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stripe;
using System.Text;

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

app.UseHttpsRedirection();

// 3. Authentication — parses and validates the JWT
app.UseAuthentication();

// 4. Session validation — confirms session is not revoked in DB
app.UseMiddleware<SessionValidationMiddleware>();

// 5. Authorization — enforces [Authorize] attributes
app.UseAuthorization();

app.MapControllers();

app.Run();