using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using TechChallenge.Oficina.StatusService.API.Extensions;
using TechChallenge.Oficina.StatusService.API.Features.OrdensServico;
using TechChallenge.Oficina.StatusService.API.Middleware;
using TechChallenge.Oficina.StatusService.API.Settings;
using TechChallenge.Oficina.Controllers.Features.OrdensServico;
using TechChallenge.Oficina.DB.Data;
using TechChallenge.Oficina.Email;
using TechChallenge.Oficina.Email.Configuration;
using TechChallenge.Oficina.UseCases.Features.Clientes.Mappings;
using TechChallenge.Oficina.UseCases.Features.Clientes.UseCases;
using TechChallenge.Oficina.UseCases.Features.Indicadores.Services;
using TechChallenge.Oficina.UseCases.Features.Indicadores.UseCases;
using TechChallenge.Oficina.UseCases.Features.Insumos.UseCases;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.Mappings;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.UseCases;
using TechChallenge.Oficina.UseCases.Features.Servicos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Veiculos.UseCases;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLoggingConfiguration(builder.Configuration);
builder.Services.AddSwaggerConfiguration();
builder.Services.AddAuthConfiguration(builder.Configuration);

var databaseSettings = builder.Configuration
    .GetSection(DatabaseSettings.SectionName)
    .Get<DatabaseSettings>()
    ?? throw new InvalidOperationException("A configuracao de banco de dados e obrigatoria.");

if (string.IsNullOrWhiteSpace(databaseSettings.ConnectionString))
{
    throw new InvalidOperationException("A connection string do banco de dados e obrigatoria.");
}

builder.Services.AddAutoMapper(_ => { }, typeof(ClienteProfile).Assembly, typeof(OrdemServicoProfile).Assembly);
builder.Services.AddScoped<IClienteUseCases, ClienteUseCases>();
builder.Services.AddScoped<IIndicadorUseCases, IndicadorUseCases>();
builder.Services.AddScoped<IInsumoUseCases, InsumoUseCases>();
builder.Services.AddScoped<IEstoqueUseCases, EstoqueUseCases>();
builder.Services.AddScoped<IOrdemServicoUseCasesFacade, OrdemServicoUseCasesFacade>();
builder.Services.AddScoped<IOrdemServicoUseCases, OrdemServicoUseCases>();
builder.Services.AddScoped<IServicoUseCases, ServicoUseCases>();
builder.Services.AddScoped<IVeiculoUseCases, VeiculoUseCases>();
builder.Services.AddInfraData(databaseSettings.ConnectionString);

var resendSettings = builder.Configuration
    .GetSection(ResendSettings.SectionName)
    .Get<ResendSettings>()
    ?? new ResendSettings();
builder.Services.AddInfraEmail(resendSettings);

builder.Services.AddScoped<IOrdensServicoController, OrdensServicoController>();
builder.Services.AddScoped<IOrdensServicoAdapter, OrdensServicoAdapter>();

var app = builder.Build();

try
{
    app.Services.ApplyMigrations();
}
catch (Exception ex)
{
    app.Logger.LogCritical(new EventId(9002, "app.migracao_falhou"), ex, "Falha ao aplicar as migrations do banco");
    Environment.ExitCode = 1;
    return;
}

app.UseLoggingConfiguration();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseSwaggerConfiguration();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok - status service" })).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") }).AllowAnonymous();
app.MapStatusOrdemServicoEndpoints();

await app.RunAsync();
