using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TechChallenge.Oficina.Controllers.Features.OrdensServico;
using TechChallenge.Oficina.DB.Data.Context;
using TechChallenge.Oficina.DB.Data.Features.Clientes;
using TechChallenge.Oficina.DB.Data.Features.Indicadores;
using TechChallenge.Oficina.DB.Data.Features.Insumos;
using TechChallenge.Oficina.DB.Data.Features.OrdensServico;
using TechChallenge.Oficina.DB.Data.Features.Servicos;
using TechChallenge.Oficina.DB.Data.Features.Veiculos;
using TechChallenge.Oficina.Entities.Features.OrdensServico;
using TechChallenge.Oficina.Email;
using TechChallenge.Oficina.GetOSService.API.Features.OrdensServico;
using TechChallenge.Oficina.UseCases.Features.Clientes.Mappings;
using TechChallenge.Oficina.UseCases.Features.Clientes.UseCases;
using TechChallenge.Oficina.UseCases.Features.Indicadores.Services;
using TechChallenge.Oficina.UseCases.Features.Indicadores.UseCases;
using TechChallenge.Oficina.UseCases.Features.Insumos.UseCases;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.Mappings;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.UseCases;
using TechChallenge.Oficina.UseCases.Features.Servicos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Veiculos.UseCases;

namespace TechChallenge.Oficina.GetOSService.API.Tests.Infrastructure;

/// <summary>
/// Fixture de integração para GetOSService: DI real com EF Core InMemory.
/// Registra apenas as dependências necessárias para o endpoint GetOrdenadas.
/// </summary>
public sealed class GetOSServiceIntegrationTestFixture : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IServiceScope _scope;

    public GetOSServiceIntegrationTestFixture()
    {
        var services = new ServiceCollection();

        services.AddLogging();

        var dbName = Guid.NewGuid().ToString();
        services.AddDbContext<OficinaDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        // Registra os gateways necessários para consultar ordens de serviço
        services.AddScoped<IClienteGateway, ClienteGateway>();
        services.AddScoped<IIndicadorGateway, IndicadorGateway>();
        services.AddScoped<IInsumoGateway, InsumoGateway>();
        services.AddScoped<IOrdemServicoGateway, OrdemServicoGateway>();
        services.AddScoped<IServicoGateway, ServicoGateway>();
        services.AddScoped<IVeiculoGateway, VeiculoGateway>();

        // Mock do email sender (não usado, mas registrado para consistência)
        var emailSenderMock = new Mock<IOrcamentoEmailSender>();
        emailSenderMock
            .Setup(s => s.EnviarOrcamentoAsync(
                It.IsAny<OrdemServico>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        services.AddSingleton(emailSenderMock.Object);

        // Mock do status email sender
        var statusEmailSenderMock = new Mock<IOrdemServicoStatusEmailSender>();
        statusEmailSenderMock
            .Setup(s => s.EnviarStatusAlteradoAsync(
                It.IsAny<OrdemServico>(),
                It.IsAny<string>(),
                It.IsAny<Entities.Features.OrdensServico.Enums.StatusOrdemServico>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        services.AddSingleton(statusEmailSenderMock.Object);

        // AutoMapper com o perfil de mapeamento
        services.AddAutoMapper(_ => { }, typeof(ClienteProfile).Assembly, typeof(OrdemServicoProfile).Assembly);

        // Registra os use cases
        services.AddScoped<IClienteUseCases, ClienteUseCases>();
        services.AddScoped<IIndicadorUseCases, IndicadorUseCases>();
        services.AddScoped<IInsumoUseCases, InsumoUseCases>();
        services.AddScoped<IEstoqueUseCases, EstoqueUseCases>();
        services.AddScoped<IOrdemServicoUseCasesFacade, OrdemServicoUseCasesFacade>();
        services.AddScoped<IOrdemServicoUseCases, OrdemServicoUseCases>();
        services.AddScoped<IServicoUseCases, ServicoUseCases>();
        services.AddScoped<IVeiculoUseCases, VeiculoUseCases>();

        // Registra o adaptador e o controller
        services.AddScoped<IOrdensServicoAdapter, OrdensServicoAdapter>();
        services.AddScoped<OrdensServicoController>();

        _serviceProvider = services.BuildServiceProvider();
        _scope = _serviceProvider.CreateScope();
    }

    private T Obter<T>() where T : notnull
        => _scope.ServiceProvider.GetRequiredService<T>();

    public OrdensServicoController CriarOrdensServicoEndpoints()
        => Obter<OrdensServicoController>();

    public void Dispose()
    {
        _scope?.Dispose();
        _serviceProvider?.Dispose();
    }
}
