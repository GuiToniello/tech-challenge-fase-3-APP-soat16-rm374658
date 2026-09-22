using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechChallenge.Oficina.Entities.Features.Clientes;
using TechChallenge.Oficina.Entities.Features.Insumos;
using TechChallenge.Oficina.Entities.Features.Indicadores;
using TechChallenge.Oficina.Entities.Features.OrdensServico;
using TechChallenge.Oficina.Entities.Features.Servicos;
using TechChallenge.Oficina.Entities.Features.Veiculos;
using TechChallenge.Oficina.DB.Data.Context;
using TechChallenge.Oficina.DB.Data.Features.Clientes;
using TechChallenge.Oficina.DB.Data.Features.Insumos;
using TechChallenge.Oficina.DB.Data.Features.Indicadores;
using TechChallenge.Oficina.DB.Data.Features.OrdensServico;
using TechChallenge.Oficina.DB.Data.Features.Servicos;
using TechChallenge.Oficina.DB.Data.Features.Veiculos;
using TechChallenge.Oficina.UseCases.Features.Clientes.UseCases;
using TechChallenge.Oficina.UseCases.Features.Insumos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Indicadores.UseCases;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.UseCases;
using TechChallenge.Oficina.UseCases.Features.Servicos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Veiculos.UseCases;

namespace TechChallenge.Oficina.DB.Data;

public static class Extensions
{
    public static IServiceCollection AddInfraData(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<OficinaDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IClienteGateway, ClienteGateway>();
        services.AddScoped<IInsumoGateway, InsumoGateway>();
        services.AddScoped<IIndicadorGateway, IndicadorGateway>();
        services.AddScoped<IOrdemServicoGateway, OrdemServicoGateway>();
        services.AddScoped<IServicoGateway, ServicoGateway>();
        services.AddScoped<IVeiculoGateway, VeiculoGateway>();
        return services;
    }

    public static void ApplyMigrations(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OficinaDbContext>();
        dbContext.Database.Migrate();
    }
}
