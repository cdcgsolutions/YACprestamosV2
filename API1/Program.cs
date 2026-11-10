using API1.Configuration;
using API1.Filters;
using API1.Helpers;
using API1.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// 1. Configuración de Logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

// 2. Controladores con Filtro Global de Excepciones
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});

// 2.1 Configuración de CORS (Permite localhost y cualquier subdominio en monsterasp.net)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
            {
                return uri.Host == "localhost" ||
                       uri.Host.EndsWith(".monsterasp.net", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// 3. Base de Datos con Entity Framework Core (SQL Server nativo)
/*
var connectionString = configuration.GetConnectionString("Default");
builder.Services.AddDbContext<[TuDbContext]>(options =>
{
    if (!string.IsNullOrEmpty(connectionString))
    {
        options.UseSqlServer(connectionString);
    }
});
*/

// 4. Autenticación y Autorización JWT Estándar
var jwtKey = configuration["Jwt:Key"] ?? "TuSuperClaveSecretaDeDesarrolloParaProyectosPersonales123456!";
var jwtIssuer = configuration["Jwt:Issuer"] ?? "WEB_API_N10";
var jwtAudience = configuration["Jwt:Audience"] ?? "WEB_API_N10";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 5. Monitoreo y Health Checks
builder.Services.AddHealthChecks();

// 6. Application Insights Opcional
var appInsightsConnectionString = configuration["ApplicationInsights:ConnectionString"];
bool enableInsights = !string.IsNullOrWhiteSpace(appInsightsConnectionString)
                      && appInsightsConnectionString != "YOUR_CONNECTION_STRING_HERE";

if (enableInsights)
{
    builder.Services.AddApplicationInsightsTelemetry();
    builder.Services.AddTransient<TelemetryDelegatingHandler>();
    builder.Services.ConfigureHttpClientDefaults(b => b.AddHttpMessageHandler<TelemetryDelegatingHandler>());
}

// 7. Registro Automático de Capa de Reglas de Negocio (RN)
builder.Services.AddConfRN();

// 8. AutoMapper
builder.Services.AddAutoMapper(cfg => { }, typeof(Program));

// 9. Documentación OpenAPI y Scalar (con soporte para Bearer JWT en la interfaz)
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        var bearerScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Ingrese su TK"
        };

        document.Components.SecuritySchemes["Bearer"] = bearerScheme;

        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
        });

        return Task.CompletedTask;
    });
});

var app = builder.Build();

// Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Layout = ScalarLayout.Classic;
        options.Title = "WEB_API_N10";
        options.PersistentAuthentication = true;
        options.Theme = ScalarTheme.Kepler;
        options.HideModels = true;
        options.DefaultHttpClient = new KeyValuePair<ScalarTarget, ScalarClient>(
            ScalarTarget.CSharp,
            ScalarClient.HttpClient
        );
        options.DarkMode = false;
        options.ShowSidebar = true;
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

if (enableInsights)
{
    app.UseMiddleware<TelemetryMiddleware>();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
