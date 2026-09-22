using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TechChallenge.Oficina.Adapters.Features.Clientes;
using TechChallenge.Oficina.Adapters.Features.Indicadores;
using TechChallenge.Oficina.Adapters.Features.Insumos;
using TechChallenge.Oficina.Adapters.Features.OrdensServico;
using TechChallenge.Oficina.Adapters.Features.Servicos;
using TechChallenge.Oficina.Adapters.Features.Veiculos;
using TechChallenge.Oficina.Controllers.Features.Clientes;
using TechChallenge.Oficina.Controllers.Features.Indicadores;
using TechChallenge.Oficina.Controllers.Features.Insumos;
using TechChallenge.Oficina.Controllers.Features.OrdensServico;
using TechChallenge.Oficina.Controllers.Features.Servicos;
using TechChallenge.Oficina.Controllers.Features.Veiculos;
using TechChallenge.Oficina.DB.Data.Context;
using TechChallenge.Oficina.DB.Data.Features.Clientes;
using TechChallenge.Oficina.DB.Data.Features.Indicadores;
using TechChallenge.Oficina.DB.Data.Features.Insumos;
using TechChallenge.Oficina.DB.Data.Features.OrdensServico;
using TechChallenge.Oficina.DB.Data.Features.Servicos;
using TechChallenge.Oficina.DB.Data.Features.Veiculos;
using TechChallenge.Oficina.Entities.Features.OrdensServico;
using TechChallenge.Oficina.Entities.Features.OrdensServico.Enums;
using TechChallenge.Oficina.UseCases.Features.Clientes.Mappings;
using TechChallenge.Oficina.UseCases.Features.Clientes.UseCases;
using TechChallenge.Oficina.UseCases.Features.Indicadores.Services;
using TechChallenge.Oficina.UseCases.Features.Indicadores.UseCases;
using TechChallenge.Oficina.UseCases.Features.Insumos.UseCases;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.Mappings;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.UseCases;
using TechChallenge.Oficina.UseCases.Features.Servicos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Veiculos.UseCases;

namespace TechChallenge.Oficina.Integration.Tests.Infrastructure;

/// <summary>
/// Fixture de integração: DI real com EF Core InMemory e mock apenas de IOrcamentoEmailSender.
/// Cada instância possui banco de dados isolado (nome único por GUID).
/// </summary>
public sealed class IntegrationTestFixture : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IServiceScope _scope;

    public IntegrationTestFixture()
    {
        var services = new ServiceCollection();

        services.AddLogging();

        var dbName = Guid.NewGuid().ToString();
        services.AddDbContext<OficinaDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        services.AddScoped<IClienteGateway, ClienteGateway>();
        services.AddScoped<IInsumoGateway, InsumoGateway>();
        services.AddScoped<IIndicadorGateway, IndicadorGateway>();
        services.AddScoped<IOrdemServicoGateway, OrdemServicoGateway>();
        services.AddScoped<IServicoGateway, ServicoGateway>();
        services.AddScoped<IVeiculoGateway, VeiculoGateway>();

        var emailSenderMock = new Mock<IOrcamentoEmailSender>();
        emailSenderMock
            .Setup(s => s.EnviarOrcamentoAsync(
                It.IsAny<OrdemServico>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        services.AddSingleton(emailSenderMock.Object);

        var statusEmailSenderMock = new Mock<IOrdemServicoStatusEmailSender>();
        statusEmailSenderMock
            .Setup(s => s.EnviarStatusAlteradoAsync(
                It.IsAny<OrdemServico>(),
                It.IsAny<string>(),
                It.IsAny<Entities.Features.OrdensServico.Enums.StatusOrdemServico>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        services.AddSingleton(statusEmailSenderMock.Object);

        services.AddAutoMapper(_ => { }, typeof(ClienteProfile).Assembly, typeof(OrdemServicoProfile).Assembly);
        services.AddScoped<IClienteUseCases, ClienteUseCases>();
        services.AddScoped<IIndicadorUseCases, IndicadorUseCases>();
        services.AddScoped<IInsumoUseCases, InsumoUseCases>();
        services.AddScoped<IEstoqueUseCases, EstoqueUseCases>();
        services.AddScoped<IOrdemServicoUseCasesFacade, OrdemServicoUseCasesFacade>();
        services.AddScoped<IOrdemServicoUseCases, OrdemServicoUseCases>();
        services.AddScoped<IServicoUseCases, ServicoUseCases>();
        services.AddScoped<IVeiculoUseCases, VeiculoUseCases>();
        services.AddScoped<IClienteAdapter, ClienteAdapter>();
        services.AddScoped<ClienteController>();
        services.AddScoped<IIndicadoresAdapter, IndicadoresAdapter>();
        services.AddScoped<IIndicadoresController, IndicadoresController>();
        services.AddScoped<IServicoAdapter, ServicoAdapter>();
        services.AddScoped<ServicoController>();
        services.AddScoped<IOrdensServicoAdapter, OrdensServicoAdapter>();
        services.AddScoped<OrdensServicoController>();
        services.AddScoped<IVeiculoAdapter, VeiculoAdapter>();
        services.AddScoped<VeiculoController>();
        services.AddScoped<InsumoController>();
        services.AddScoped<IInsumoAdapter, InsumoAdapter>();

        _serviceProvider = services.BuildServiceProvider();
        _scope = _serviceProvider.CreateScope();
    }

    private T Obter<T>() where T : notnull
        => _scope.ServiceProvider.GetRequiredService<T>();

    public ClienteController CriarClientesEndpoints()
        => Obter<ClienteController>();

    public InsumoController CriarInsumosEndpoints()
        => Obter<InsumoController>();

    public VeiculoController CriarVeiculosEndpoints()
        => Obter<VeiculoController>();

    public ServicoController CriarServicosEndpoints()
        => Obter<ServicoController>();

    public OrdensServicoController CriarOrdensServicoEndpoints()
        => Obter<OrdensServicoController>();

    public IIndicadoresController CriarIndicadoresEndpoints()
        => Obter<IIndicadoresController>();

    public void Dispose()
    {
        _scope.Dispose();
        _serviceProvider.Dispose();
    }
}

