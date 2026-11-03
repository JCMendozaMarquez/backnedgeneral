
using System.Text;
using BackendGeneral.Api.Data;
using BackendGeneral.Api.Configuration;
using BackendGeneral.Api.Services;
using BackendGeneral.Api.Authorization;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// CONFIGURACIÓN DE POSTGRESQL
// ==========================================================

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No se encontró ConnectionStrings:DefaultConnection."
    );
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention()
);

// ==========================================================
// CONTROLLERS + OPENAPI
// ==========================================================

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
                Description = "Ingresa tu token JWT."
            };

        return Task.CompletedTask;
    });
});

// ==========================================================
// CONFIGURACIÓN JWT
// ==========================================================

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt")
);

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key es obligatorio y debe tener al menos 32 caracteres."
    );
}

if (string.IsNullOrWhiteSpace(jwtIssuer) ||
    string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException(
        "Jwt:Issuer y Jwt:Audience son obligatorios."
    );
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            ),

            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

// ==========================================================
// AUTORIZACIÓN DINÁMICA POR PERMISOS
// ==========================================================

builder.Services.AddSingleton<
    IAuthorizationHandler,
    PermissionHandler
>();

builder.Services.AddSingleton<
    IAuthorizationPolicyProvider,
    PermissionPolicyProvider
>();

builder.Services.AddAuthorization();

// ==========================================================
// SERVICIOS DE AUTENTICACIÓN
// ==========================================================

builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<AuthService>();

// ==========================================================
// SERVICIOS DE NEGOCIO
// ==========================================================

builder.Services.AddScoped<DbViewsService>();
builder.Services.AddScoped<CatalogoService>();
builder.Services.AddScoped<PedidosService>();
builder.Services.AddScoped<CobrosCajaService>();
builder.Services.AddScoped<InventarioService>();
builder.Services.AddScoped<IntegracionesService>();
builder.Services.AddScoped<ReportesService>();

// ==========================================================
// CORS
// ==========================================================

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

// ==========================================================
// CONSTRUCCIÓN DE LA APLICACIÓN
// ==========================================================

var app = builder.Build();

// ==========================================================
// OPENAPI + SCALAR
// ==========================================================

app.MapOpenApi();

app.MapScalarApiReference(options =>
{
    options.WithTitle("BackendGeneral API");
    options.WithTheme(ScalarTheme.Purple);

    options.AddPreferredSecuritySchemes("Bearer");

    options.AddHttpAuthentication("Bearer", scheme =>
    {
        scheme.Token = "";
    });
});

// ==========================================================
// HEALTH CHECK
// ==========================================================

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    application = "BackendGeneral.Api",
    environment = app.Environment.EnvironmentName
}));

// ==========================================================
// MIDDLEWARE
// ==========================================================

app.UseCors("VuesticPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ==========================================================
// HOSTLESS - PUERTO DINÁMICO
// ==========================================================

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";

app.Run($"http://0.0.0.0:{port}");
