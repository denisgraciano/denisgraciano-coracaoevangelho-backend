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

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((ctx, services, config) =>
        config.ReadFrom.Configuration(ctx.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext());

    // DbContext MySQL
    var connStr = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string nao configurada.");

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(connStr, ServerVersion.AutoDetect(connStr))
    );

    // AutoMapper
    builder.Services.AddAutoMapper(typeof(Program).Assembly);

    // FluentValidation
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

    // Repositories
    builder.Services.AddScoped<ILivroRepository, LivroRepository>();
    builder.Services.AddScoped<ICapituloRepository, CapituloRepository>();
    builder.Services.AddScoped<IVersiculoRepository, VersiculoRepository>();
    builder.Services.AddScoped<IDevocionalRepository, DevocionalRepository>();
    builder.Services.AddScoped<IFavoritoRepository, FavoritoRepository>();
    builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

    // Services
    builder.Services.AddScoped<ILivroService, LivroService>();
    builder.Services.AddScoped<IVersiculoService, VersiculoService>();
    builder.Services.AddScoped<IDevocionalService, DevocionalService>();
    builder.Services.AddScoped<IFavoritoService, FavoritoService>();
    builder.Services.AddScoped<IConfiguracaoService, ConfiguracaoService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ISyncService, SyncService>();

    // JWT
    var jwtKey = builder.Configuration["Jwt:Key"]
        ?? throw new InvalidOperationException("JWT Key nao configurada.");

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorization();

    // CORS
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? Array.Empty<string>();

    builder.Services.AddCors(o =>
        o.AddPolicy("FrontendPolicy", p =>
            p.WithOrigins(allowedOrigins)
             .AllowAnyHeader()
             .AllowAnyMethod()
             .AllowCredentials()));

    // Controllers + JSON camelCase
    builder.Services.AddControllers()
        .AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.PropertyNamingPolicy =
                System.Text.Json.JsonNamingPolicy.CamelCase;
            o.JsonSerializerOptions.DefaultIgnoreCondition =
                System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        });

    // Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Coracao Evangelho API",
            Version = "v1",
            Description = "API da plataforma PWA de conteudo cristao/evangelico."
        });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Insira: Bearer {token}"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
        c.EnableAnnotations();
    });

    builder.Services.AddResponseCaching();

    // ── PIPELINE ──────────────────────────────────────────────────────────
    var app = builder.Build();

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Coracao Evangelho API v1"));
    }

    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();
    app.UseResponseCaching();
    app.UseCors("FrontendPolicy");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
       .WithTags("Health");

    Log.Information("Coracao Evangelho API iniciando...");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host encerrado inesperadamente.");
}
finally
{
    await Log.CloseAndFlushAsync();
}
