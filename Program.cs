using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using YourNamespace.Data;
using YourNamespace.Models;
using YourNamespace.Services;

var builder = WebApplication.CreateBuilder(args);

// Carrega o .env em desenvolvimento (na nuvem não existe .env; as variáveis vêm do Render)
Env.TraversePath().Load();

var connectionString =
    Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "A variável DEFAULT_CONNECTION não foi encontrada (defina no .env local ou nas Environment Variables do Render).");
}

var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
if (string.IsNullOrWhiteSpace(jwtSecret) || Encoding.UTF8.GetByteCount(jwtSecret) < 32)
{
    throw new InvalidOperationException(
        "A variável JWT_SECRET não foi encontrada ou tem menos de 32 caracteres (defina no .env local ou nas Environment Variables do Render).");
}

builder.Services.AddDbContext<YourDbContext>(options =>
    options.UseNpgsql(connectionString));

// Autenticação: hash de senha + JWT
var tokenService = new TokenService(jwtSecret);
builder.Services.AddSingleton(tokenService);
builder.Services.AddSingleton<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = TokenService.Emissor,
            ValidateAudience = true,
            ValidAudience = TokenService.Emissor,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = tokenService.Chave,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Botão "Authorize" no Swagger para testar endpoints protegidos
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Cole apenas o token retornado no login (sem o prefixo 'Bearer')."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// CORS: libera o front para chamar a API
const string PoliticaCors = "PermitirFront";
builder.Services.AddCors(options =>
{
    options.AddPolicy(PoliticaCors, policy =>
    {
        policy
            .AllowAnyOrigin()   // para o trabalho acadêmico; em produção real, troque por .WithOrigins("https://seu-front.com")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Render fornece a porta em PORT. Localmente PORT não existe,
// então valem as URLs do launchSettings.json (5241 / 7169).
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var app = builder.Build();

// Swagger habilitado em todos os ambientes (inclusive no Render)
app.UseSwagger();
app.UseSwaggerUI();

// No Render o HTTPS é tratado pelo proxy; redireciona só localmente
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// CORS entra ANTES de Authentication/Authorization e do MapControllers
app.UseCors(PoliticaCors);

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();