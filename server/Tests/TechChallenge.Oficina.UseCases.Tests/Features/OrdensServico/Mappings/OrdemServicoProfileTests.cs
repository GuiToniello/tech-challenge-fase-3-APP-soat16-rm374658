using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.Mappings;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.Entities.Features.Insumos;
using TechChallenge.Oficina.Entities.Features.OrdensServico;
using TechChallenge.Oficina.Entities.Features.Servicos;
using Xunit;

namespace TechChallenge.Oficina.UseCases.Tests.Features.OrdensServico.Mappings;

public sealed class OrdemServicoProfileTests
{
    private readonly IMapper _mapper;

    public OrdemServicoProfileTests()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddAutoMapper(_ => { }, typeof(OrdemServicoProfile).Assembly);

        using var serviceProvider = services.BuildServiceProvider();
        _mapper = serviceProvider.GetRequiredService<IMapper>();
    }

    [Fact]
    public void Map_OrdemServicoParaOrdemServicoViewModel_DeveMapearStatusEServicos()
    {
        var servico = Servico.Criar("Revisao", "Servico completo", []);
        var ordemServico = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), [servico]);

        var viewModel = _mapper.Map<OrdemServicoViewModel>(ordemServico);

        Assert.Equal(ordemServico.Id, viewModel.Id);
        Assert.Equal(ordemServico.ClienteId, viewModel.ClienteId);
        Assert.Equal(ordemServico.VeiculoId, viewModel.VeiculoId);
        Assert.Equal(1, viewModel.Status);
        Assert.Equal("Recebida", viewModel.StatusDescricao);
        Assert.Single(viewModel.Servicos);
        Assert.Equal(servico.Id, viewModel.Servicos.Single().Id);
    }

    [Fact]
    public void Map_OrdemServicoParaAcompanhamentoOrdemServicoViewModel_DeveMapearStatusEHistorico()
    {
        var ordemServico = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), [Servico.Criar("Alinhamento", "Servico completo", [])]);

        var viewModel = _mapper.Map<AcompanhamentoOrdemServicoViewModel>(ordemServico);

        Assert.Equal(ordemServico.Id, viewModel.Id);
        Assert.Equal(1, viewModel.Status);
        Assert.Equal("Recebida", viewModel.StatusDescricao);
        Assert.Single(viewModel.HistoricoStatus);
        Assert.Equal(1, (int)viewModel.HistoricoStatus.Single().Status);
    }

    [Fact]
    public void Map_OrdemServicoComOrcamento_DeveMapearTotaisDoOrcamento()
    {
        var insumo = Insumo.Criar("Oleo", "Fabricante", 10, 19.90m);
        var servico = Servico.Criar("Troca de oleo", "Servico completo", [ItemServico.Criar(insumo, 2)]);
        var ordemServico = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), [servico]);
        ordemServico.GerarOrcamento(new DateTime(2024, 01, 01, 10, 00, 00, DateTimeKind.Utc));

        var viewModel = _mapper.Map<OrdemServicoViewModel>(ordemServico);

        Assert.NotNull(viewModel.Orcamento);
        Assert.Equal(ordemServico.Id, viewModel.Orcamento!.OrdemServicoId);
        Assert.Equal(39.80m, viewModel.Orcamento.ValorTotal);
        Assert.Single(viewModel.Orcamento.Servicos);
        Assert.Equal(servico.Id, viewModel.Orcamento.Servicos.Single().ServicoId);
    }
}
