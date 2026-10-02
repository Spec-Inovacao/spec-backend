using Microsoft.EntityFrameworkCore;
using YourNamespace.Data;
using DotNetEnv;

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

builder.Services.AddDbContext<YourDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// CORS entra ANTES de Authorization e do MapControllers
app.UseCors(PoliticaCors);

app.UseAuthorization();
app.MapControllers();

app.Run();