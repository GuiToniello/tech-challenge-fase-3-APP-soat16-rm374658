using TechChallenge.Oficina.Monolith.API.Extensions;
using TechChallenge.Oficina.Monolith.API.Features.Clientes;
using TechChallenge.Oficina.Monolith.API.Features.Indicadores;
using TechChallenge.Oficina.Monolith.API.Features.Insumos;
using TechChallenge.Oficina.Monolith.API.Features.OrdensServico;
using TechChallenge.Oficina.Monolith.API.Features.Servicos;
using TechChallenge.Oficina.Monolith.API.Features.Veiculos;
using TechChallenge.Oficina.Adapters.Features.Clientes;
using TechChallenge.Oficina.Adapters.Features.Indicadores;
using TechChallenge.Oficina.Adapters.Features.Insumos;
using TechChallenge.Oficina.Adapters.Features.OrdensServico;
using TechChallenge.Oficina.Adapters.Features.Servicos;
using TechChallenge.Oficina.Adapters.Features.Veiculos;
using TechChallenge.Oficina.Monolith.API.Middleware;
using TechChallenge.Oficina.Monolith.API.Settings;
using TechChallenge.Oficina.Controllers.Features.Clientes;
using TechChallenge.Oficina.Controllers.Features.Indicadores;
using TechChallenge.Oficina.Controllers.Features.Insumos;
using TechChallenge.Oficina.Controllers.Features.OrdensServico;
using TechChallenge.Oficina.Controllers.Features.Servicos;
using TechChallenge.Oficina.Controllers.Features.Veiculos;
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

// Add services to the container.

builder.Services.AddSwaggerConfiguration();
builder.Services.AddAuthConfiguration(builder.Configuration);

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
builder.Services.AddInfraData(databaseSettings.ConnectionString);

var resendSettings = builder.Configuration
    .GetSection(ResendSettings.SectionName)
    .Get<ResendSettings>()
    ?? new ResendSettings();
builder.Services.AddInfraEmail(resendSettings);

builder.Services.AddScoped<IClienteAdapter, ClienteAdapter>();
builder.Services.AddScoped<IClienteController, ClienteController>();
builder.Services.AddScoped<IServicoController, ServicoController>();
builder.Services.AddScoped<IServicoAdapter, ServicoAdapter>();
builder.Services.AddScoped<IOrdensServicoController, OrdensServicoController>();
builder.Services.AddScoped<IOrdensServicoAdapter, OrdensServicoAdapter>();
builder.Services.AddScoped<IVeiculoController, VeiculoController>();
builder.Services.AddScoped<IVeiculoAdapter, VeiculoAdapter>();
builder.Services.AddScoped<IInsumoController, InsumoController>();
builder.Services.AddScoped<IInsumoAdapter, InsumoAdapter>();
builder.Services.AddScoped<IIndicadoresAdapter, IndicadoresAdapter>();
builder.Services.AddScoped<IIndicadoresController, IndicadoresController>();

var app = builder.Build();

app.Services.ApplyMigrations();


app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseSwaggerConfiguration();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok - monolith" })).AllowAnonymous();
app.MapClienteEndpoints();
app.MapIndicadoresEndpoints();
app.MapInsumoEndpoints();
app.MapVeiculoEndpoints();
app.MapServicoEndpoints();
app.MapOrdensServicoEndpoints();

await app.RunAsync();
