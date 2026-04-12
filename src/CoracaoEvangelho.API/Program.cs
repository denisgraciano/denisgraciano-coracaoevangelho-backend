using System.Text;
using CoracaoEvangelho.API.Data;
using CoracaoEvangelho.API.Middlewares;
using CoracaoEvangelho.API.Repositories;
using CoracaoEvangelho.API.Repositories.Interfaces;
using CoracaoEvangelho.API.Services;
using CoracaoEvangelho.API.Services.Interfaces;
using CoracaoEvangelho.API.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ───────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// ── MySQL / EF Core ───────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        mysqlOptions => mysqlOptions.EnableRetryOnFailure(
            maxRetryCount:        3,
            maxRetryDelay:        TimeSpan.FromSeconds(5),
            errorNumbersToAdd:    null)
    )
);

// ── JWT Authentication ────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key não configurado. Defina em appsettings ou variável de ambiente.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew                = TimeSpan.Zero  // expira exatamente em 1h
        };
    });

builder.Services.AddAuthorization();

// ── CORS ──────────────────────────────────────────────────────
var frontendUrl = builder.Configuration["Frontend:Url"]
    ?? "http://localhost:4200";

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy
            .WithOrigins(frontendUrl)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

// ── Repositórios (Scoped — vida por request) ──────────────────
builder.Services.AddScoped<IUsuarioRepository,       UsuarioRepository>();
builder.Services.AddScoped<ICursoRepository,         CursoRepository>();
builder.Services.AddScoped<IMatriculaRepository,     MatriculaRepository>();
builder.Services.AddScoped<IProgressoRepository,     ProgressoRepository>();
builder.Services.AddScoped<ICertificadoRepository,   CertificadoRepository>();
builder.Services.AddScoped<IPedidoVibracaoRepository, PedidoVibracaoRepository>();
builder.Services.AddScoped<ICategoriaRepository,     CategoriaRepository>();
builder.Services.AddScoped<IAulaRepository,          AulaRepository>();

// ── Services (Scoped) ─────────────────────────────────────────
builder.Services.AddScoped<IAuthService,            AuthService>();
builder.Services.AddScoped<IUsuarioService,         UsuarioService>();
builder.Services.AddScoped<ICursoService,           CursoService>();
builder.Services.AddScoped<IMatriculaService,       MatriculaService>();
builder.Services.AddScoped<IProgressoService,       ProgressoService>();
builder.Services.AddScoped<ICertificadoService,     CertificadoService>();
builder.Services.AddScoped<IPedidoVibracaoService,  PedidoVibracaoService>();
builder.Services.AddScoped<IAdminService,           AdminService>();

// ── FluentValidation ──────────────────────────────────────────
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

// ── Controllers + JSON camelCase ──────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase);

// ── Swagger / OpenAPI ─────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Coração Evangelho API",
        Version     = "v1",
        Description = "Plataforma de cursos espíritas — cursos, matrículas, " +
                      "progresso, certificados e pedidos de vibrações."
    });

    c.EnableAnnotations();

    // Botão Authorize no Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name        = "Authorization",
        Type        = SecuritySchemeType.Http,
        Scheme      = "bearer",
        BearerFormat = "JWT",
        In          = ParameterLocation.Header,
        Description = "Informe o token JWT: Bearer {token}"
    });

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

// ── Health Check ──────────────────────────────────────────────
// AddDbContextCheck requer pacote separado (AspNetCore.HealthChecks.EntityFramework)
// Health check básico — já verifica se a aplicação está respondendo
builder.Services.AddHealthChecks();

var app = builder.Build();

// ── Pipeline HTTP ─────────────────────────────────────────────

// 1. Tratamento global de exceções — deve ser o primeiro
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 2. HTTPS redirect
app.UseHttpsRedirection();

// 3. CORS — antes de Auth e Controllers
app.UseCors("Frontend");

// 4. Swagger apenas em Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Coração Evangelho API v1");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
    });
}

// 5. Auth
app.UseAuthentication();
app.UseAuthorization();

// 6. Endpoints
app.MapControllers();
app.MapHealthChecks("/health");

// ── Migrations pendentes + Seed de desenvolvimento ───────────
using (var scope = app.Services.CreateScope())
{
    var db           = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var seederLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // Aplica migrations pendentes automaticamente ao iniciar
    await db.Database.MigrateAsync();

    if (app.Environment.IsDevelopment())
        await DbSeeder.SeedAsync(db, seederLogger);
}

// ── Logs de inicialização ─────────────────────────────────────
Log.Information("Coração Evangelho API iniciada. Ambiente: {Env}", app.Environment.EnvironmentName);
Log.Information("Frontend configurado: {Url}", frontendUrl);

app.Run();
