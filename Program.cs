using System.Text;
using BackendGeneral.Api.Data;
using BackendGeneral.Api.Configuration;
using BackendGeneral.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Microsoft.OpenApi;
// backend dinamico par  asignar roles
using BackendGeneral.Api.Authorization;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════
// PostgreSQL
// ═══════════════════════════════════════════════════════════

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// ═══════════════════════════════════════════════════════════
// Controllers + OpenAPI
// ═══════════════════════════════════════════════════════════

builder.Services.AddControllers();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();

        document.Components.SecuritySchemes ??=
            new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["Bearer"] =
            new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description =
                    "Ingresa solamente el token JWT. Scalar agregará automáticamente 'Bearer '."
            };

        return Task.CompletedTask;
    });
});

// ═══════════════════════════════════════════════════════════
// Servicios de autenticación
// ═══════════════════════════════════════════════════════════

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt")
);

builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<AuthService>();

// ═══════════════════════════════════════════════════════════
// JWT
// ═══════════════════════════════════════════════════════════

var jwtKey = builder.Configuration["Jwt:Key"]!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],

                ValidAudience =
                    builder.Configuration["Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)
                    )
            };
    });

// Registra el handler (para saber qué significa cada permiso)
builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

// Registra el provider que crea policies dinámicamente
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

// Autorización por defecto (sin policies hardcodeadas)
builder.Services.AddAuthorization();
// ═══════════════════════════════════════════════════════════
// CORS
// ═══════════════════════════════════════════════════════════

builder.Services.AddCors(options =>
{
    options.AddPolicy("VuesticPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost:3000",
                "http://localhost:8080",
                "http://localhost:5253"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ═══════════════════════════════════════════════════════════
// APP
// ═══════════════════════════════════════════════════════════

var app = builder.Build();

// ═══════════════════════════════════════════════════════════
// Scalar
// ═══════════════════════════════════════════════════════════

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.WithTitle("BackendGeneral API");
        options.WithTheme(ScalarTheme.Purple);

        // ═══════════════════════════════════════════════
        // AUTENTICACIÓN GLOBAL
        // ═══════════════════════════════════════════════

        options.AddPreferredSecuritySchemes("Bearer");

        options.AddHttpAuthentication(
            "Bearer",
            scheme =>
            {
                scheme.Token = "";
            }
        );
    });
}

// ═══════════════════════════════════════════════════════════
// Middleware
// ═══════════════════════════════════════════════════════════

app.UseCors("VuesticPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();