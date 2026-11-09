
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StockFlow.Api.Data;
using StockFlow.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Controllers and OpenAPI
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("StockFlowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",
                "https://localhost:3000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// SQL Server
builder.Services.AddDbContext<StockFlowDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );
});

// ASP.NET Core Identity
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<StockFlowDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// JWT configuration
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "JWT signing key is missing from configuration."
    );

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "JWT issuer is missing from configuration."
    );

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "JWT audience is missing from configuration."
    );

var signingKey = new SymmetricSecurityKey(
    Convert.FromBase64String(jwtKey)
);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1),

                NameClaimType = JwtRegisteredClaimNames.Sub,
                RoleClaimType = "role"
            };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// OpenAPI
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Middleware
app.UseCors("StockFlowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// Controllers
app.MapControllers();

app.Run();
