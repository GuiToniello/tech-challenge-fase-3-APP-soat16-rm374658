using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using TechChallenge.Oficina.CreateOSService.API.Extensions;
using TechChallenge.Oficina.CreateOSService.API.Features.OrdensServico;
using TechChallenge.Oficina.Controllers.Features.OrdensServico;
using TechChallenge.Oficina.Controllers.Features.Clientes;
using TechChallenge.Oficina.Controllers.Features.Insumos;
using TechChallenge.Oficina.Controllers.Features.Servicos;
using TechChallenge.Oficina.Controllers.Features.Veiculos;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.Mappings;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.UseCases;
using TechChallenge.Oficina.UseCases.Features.Clientes.Mappings;
using TechChallenge.Oficina.UseCases.Features.Clientes.UseCases;
using TechChallenge.Oficina.UseCases.Features.Indicadores.Services;
using TechChallenge.Oficina.UseCases.Features.Indicadores.UseCases;
using TechChallenge.Oficina.UseCases.Features.Insumos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Servicos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Veiculos.UseCases;
using TechChallenge.Oficina.DB.Data;
using TechChallenge.Oficina.Adapters.Features.Clientes;
using TechChallenge.Oficina.Adapters.Features.Insumos;
using TechChallenge.Oficina.Adapters.Features.OrdensServico;
using TechChallenge.Oficina.Adapters.Features.Servicos;
using TechChallenge.Oficina.Adapters.Features.Veiculos;
using TechChallenge.Oficina.Email;
using TechChallenge.Oficina.Email.Configuration;
using TechChallenge.Oficina.CreateOSService.API.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLoggingConfiguration(builder.Configuration);
builder.Services.AddSwaggerConfiguration();
builder.Services.AddAuthConfiguration(builder.Configuration);
builder.Services.AddHealthChecks();

var databaseSettings = builder.Configuration
    .GetSection(DatabaseSettings.SectionName)
    .Get<DatabaseSettings>()
    ?? throw new InvalidOperationException("A configuração de banco de dados é obrigatória.");

if (string.IsNullOrWhiteSpace(databaseSettings.ConnectionString))
{
    throw new InvalidOperationException("A connection string do banco de dados é obrigatória.");
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
builder.Services.AddScoped<IOrdensServicoController, OrdensServicoController>();
builder.Services.AddScoped<IOrdensServicoAdapter, OrdensServicoAdapter>();
builder.Services.AddInfraData(databaseSettings.ConnectionString);

var resendSettings = builder.Configuration
    .GetSection(ResendSettings.SectionName)
    .Get<ResendSettings>()
    ?? new ResendSettings();
builder.Services.AddInfraEmail(resendSettings);

builder.Services.AddScoped<IClienteAdapter, ClienteAdapter>();
builder.Services.AddScoped<IVeiculoAdapter, VeiculoAdapter>();
builder.Services.AddScoped<IInsumoAdapter, InsumoAdapter>();
builder.Services.AddScoped<IServicoAdapter, ServicoAdapter>();

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
app.UseGlobalExceptionHandlerMiddleware();

app.UseSwaggerConfiguration();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") }).AllowAnonymous();
app.MapCriarOSCompletaEndpoints();

await app.RunAsync();
