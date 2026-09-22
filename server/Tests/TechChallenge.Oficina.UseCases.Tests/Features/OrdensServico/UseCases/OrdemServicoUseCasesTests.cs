using AutoMapper;
using Moq;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.Commands;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.Queries;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.UseCases;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.Entities.Exceptions;
using TechChallenge.Oficina.Entities.Features.Clientes;
using TechChallenge.Oficina.Entities.Features.Clientes.VOs;
using TechChallenge.Oficina.Entities.Features.Insumos;
using TechChallenge.Oficina.Entities.Features.Orcamentos;
using TechChallenge.Oficina.Entities.Features.OrdensServico;
using TechChallenge.Oficina.Entities.Features.OrdensServico.Enums;
using TechChallenge.Oficina.Entities.Features.Servicos;
using TechChallenge.Oficina.Entities.Features.Veiculos;
using Xunit;
using TechChallenge.Oficina.UseCases.Features.Indicadores.UseCases;
using TechChallenge.Oficina.UseCases.Features.Servicos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Veiculos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Clientes.UseCases;
using TechChallenge.Oficina.UseCases.Features.Insumos.UseCases;

namespace TechChallenge.Oficina.UseCases.Tests.Features.OrdensServico.UseCases;

public sealed class OrdemServicoUseCasesTests
{
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IOrdemServicoGateway> _ordemServicoRepositoryMock = new();
    private readonly Mock<IClienteGateway> _clienteRepositoryMock = new();
    private readonly Mock<IVeiculoGateway> _veiculoRepositoryMock = new();
    private readonly Mock<IServicoGateway> _servicoRepositoryMock = new();
    private readonly Mock<IEstoqueUseCases> _estoqueServiceMock = new();
    private readonly Mock<IIndicadorUseCases> _indicadorServiceMock = new();
    private readonly Mock<IOrcamentoEmailSender> _orcamentoEmailSenderMock = new();
    private readonly Mock<IOrdemServicoUseCasesFacade> _ordemServicoServicesFacadeMock = new();

    private OrdemServicoUseCases CriarService()
    {
        _ordemServicoServicesFacadeMock.SetupGet(f => f.EstoqueService).Returns(_estoqueServiceMock.Object);
        _ordemServicoServicesFacadeMock.SetupGet(f => f.IndicadorService).Returns(_indicadorServiceMock.Object);
        _ordemServicoServicesFacadeMock.SetupGet(f => f.OrcamentoEmailSender).Returns(_orcamentoEmailSenderMock.Object);

        return new OrdemServicoUseCases(
            _mapperMock.Object,
            _ordemServicoRepositoryMock.Object,
            _clienteRepositoryMock.Object,
            _veiculoRepositoryMock.Object,
            _servicoRepositoryMock.Object,
            _ordemServicoServicesFacadeMock.Object);
    }

    private static Cliente CriarCliente(string identificacao = "52998224725", string? email = null) => Cliente.Criar("Cliente Teste", IdentificacaoCliente.Criar(identificacao), email);

    private static Veiculo CriarVeiculo(Guid clienteId) => Veiculo.Criar("ABC1D23", "Toyota", "Corolla", 2023, "12345678901", clienteId);

    private static Servico CriarServico(string nome = "Revisao") => Servico.Criar(nome, "Servico completo", []);

    private static OrdemServico CriarOrdemServico(Guid clienteId, Guid veiculoId, IReadOnlyCollection<Servico> servicos, DateTime? dataCadastro = null) => OrdemServico.Criar(clienteId, veiculoId, servicos, dataCadastro);

    private static OrdemServicoViewModel MapearViewModel(OrdemServico ordemServico) =>
        new()
        {
            Id = ordemServico.Id,
            ClienteId = ordemServico.ClienteId,
            VeiculoId = ordemServico.VeiculoId,
            Status = (int)ordemServico.Status,
            StatusDescricao = "Recebida",
            Servicos = ordemServico.Servicos.Select(servico => new ServicoResumoOrdemServicoViewModel { Id = servico.Id, Nome = servico.Nome }).ToArray(),
            Orcamento = ordemServico.Orcamento is null
                ? null
                : new OrcamentoViewModel
                {
                    OrdemServicoId = ordemServico.Orcamento.OrdemServicoId,
                    DataGeracao = ordemServico.Orcamento.DataGeracao,
                    ValorTotal = ordemServico.Orcamento.ValorTotal,
                    Servicos = ordemServico.Orcamento.Servicos.Select(item => new OrcamentoServicoViewModel
                    {
                        ServicoId = item.ServicoId,
                        NomeServico = item.NomeServico,
                        ValorTotal = item.ValorTotal
                    }).ToArray()
                }
        };

    private static OrdemServicoOrdenadasViewModel MapearViewModelOrdenadas(OrdemServico ordemServico) =>
        new()
        {
            Id = ordemServico.Id,
            ClienteId = ordemServico.ClienteId,
            VeiculoId = ordemServico.VeiculoId,
            Status = (int)ordemServico.Status,
            StatusDescricao = ordemServico.Status.ToString(),
            Servicos = ordemServico.Servicos.Select(servico => new ServicoResumoOrdemServicoViewModel { Id = servico.Id, Nome = servico.Nome }).ToArray(),
            Orcamento = null,
            DataAlteracao = ordemServico.HistoricoStatus.OrderBy(h => h.DataAlteracao).FirstOrDefault()?.DataAlteracao ?? default
        };

    private static AcompanhamentoOrdemServicoViewModel MapearAcompanhamento(OrdemServico ordemServico) =>
        new()
        {
            Id = ordemServico.Id,
            Status = (int)ordemServico.Status,
            StatusDescricao = "Recebida",
            HistoricoStatus = ordemServico.HistoricoStatus.Select(historico => new HistoricoStatusOrdemServicoViewModel { Status = historico.Status, DataAlteracao = historico.DataAlteracao }).ToArray()
        };

    [Fact]
    public async Task CriarAsync_DeveAdicionarOrdemServicoERetornarViewModel()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(cliente.Id);
        var servico = CriarServico();
        var command = new CriarOrdemServicoCommand { ClienteId = cliente.Id, VeiculoId = veiculo.Id, ServicoIds = [servico.Id] };

        _clienteRepositoryMock.Setup(r => r.ObterPorIdAsync(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
        _veiculoRepositoryMock.Setup(r => r.ObterPorIdAsync(veiculo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(veiculo);
        _servicoRepositoryMock.Setup(r => r.ObterPorIdAsync(servico.Id, It.IsAny<CancellationToken>())).ReturnsAsync(servico);
        _mapperMock.Setup(m => m.Map<OrdemServicoViewModel>(It.IsAny<object>())).Returns((object source) => MapearViewModel((OrdemServico)source));

        var resultado = await service.CriarAsync(command);

        Assert.Equal(cliente.Id, resultado.ClienteId);
        Assert.Equal(veiculo.Id, resultado.VeiculoId);
        Assert.Equal((int)StatusOrdemServico.Recebida, resultado.Status);
        Assert.Single(resultado.Servicos);
        Assert.Single(((OrdemServico)_mapperMock.Invocations.Last().Arguments[0]).HistoricoStatus);
        _ordemServicoRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_DeveLancarDomainException_QuandoVeiculoNaoPertenceAoCliente()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var outroCliente = CriarCliente("04252011000110");
        var veiculo = CriarVeiculo(outroCliente.Id);
        var servico = CriarServico();
        var command = new CriarOrdemServicoCommand { ClienteId = cliente.Id, VeiculoId = veiculo.Id, ServicoIds = [servico.Id] };

        _clienteRepositoryMock.Setup(r => r.ObterPorIdAsync(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
        _veiculoRepositoryMock.Setup(r => r.ObterPorIdAsync(veiculo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(veiculo);

        var action = async () => await service.CriarAsync(command);

        var exception = await Assert.ThrowsAsync<DomainException>(action);
        Assert.Equal("O veiculo informado deve estar vinculado ao cliente da ordem de servico.", exception.Message);
    }

    [Fact]
    public async Task CriarAsync_DeveLancarDomainException_QuandoNaoInformarServicos()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(cliente.Id);
        var command = new CriarOrdemServicoCommand { ClienteId = cliente.Id, VeiculoId = veiculo.Id, ServicoIds = [] };

        _clienteRepositoryMock.Setup(r => r.ObterPorIdAsync(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
        _veiculoRepositoryMock.Setup(r => r.ObterPorIdAsync(veiculo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(veiculo);

        var action = async () => await service.CriarAsync(command);

        var exception = await Assert.ThrowsAsync<DomainException>(action);
        Assert.Equal("A ordem de servico deve possuir ao menos um servico.", exception.Message);
    }

    [Fact]
    public async Task AtualizarAsync_DeveAtualizarOrdemServico_SemAlterarStatus()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(cliente.Id);
        var servicoAtual = CriarServico();
        var servicoNovo = CriarServico("Alinhamento");
        var ordemServico = CriarOrdemServico(cliente.Id, veiculo.Id, [servicoAtual]);
        var command = new AtualizarOrdemServicoCommand { Id = ordemServico.Id, ClienteId = cliente.Id, VeiculoId = veiculo.Id, ServicoIds = [servicoNovo.Id] };

        _ordemServicoRepositoryMock.Setup(r => r.ObterPorIdAsync(ordemServico.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ordemServico);
        _clienteRepositoryMock.Setup(r => r.ObterPorIdAsync(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
        _veiculoRepositoryMock.Setup(r => r.ObterPorIdAsync(veiculo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(veiculo);
        _servicoRepositoryMock.Setup(r => r.ObterPorIdAsync(servicoNovo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(servicoNovo);
        _mapperMock.Setup(m => m.Map<OrdemServicoViewModel>(It.IsAny<object>())).Returns((object source) => MapearViewModel((OrdemServico)source));

        var resultado = await service.AtualizarAsync(command);

        Assert.Equal((int)StatusOrdemServico.Recebida, resultado.Status);
        Assert.Single(resultado.Servicos);
        Assert.Equal(servicoNovo.Id, resultado.Servicos.Single().Id);
        _ordemServicoRepositoryMock.Verify(r => r.AtualizarAsync(ordemServico, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarViewModel_QuandoOrdemServicoExiste()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(cliente.Id);
        var ordemServico = CriarOrdemServico(cliente.Id, veiculo.Id, [CriarServico()]);

        _ordemServicoRepositoryMock.Setup(r => r.ObterPorIdAsync(ordemServico.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ordemServico);
        _mapperMock.Setup(m => m.Map<OrdemServicoViewModel>(It.IsAny<object>())).Returns((object source) => MapearViewModel((OrdemServico)source));

        var resultado = await service.ObterPorIdAsync(new ObterOrdemServicoPorIdQuery { Id = ordemServico.Id });

        Assert.Equal(ordemServico.Id, resultado.Id);
    }

    [Fact]
    public async Task ObterAcompanhamentoAsync_DeveRetornarResumoDaOrdemServico()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(cliente.Id);
        var ordemServico = CriarOrdemServico(cliente.Id, veiculo.Id, [CriarServico()]);

        _ordemServicoRepositoryMock.Setup(r => r.ObterPorIdAsync(ordemServico.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ordemServico);
        _mapperMock.Setup(m => m.Map<AcompanhamentoOrdemServicoViewModel>(It.IsAny<object>())).Returns((object source) => MapearAcompanhamento((OrdemServico)source));

        var resultado = await service.ObterAcompanhamentoAsync(new ObterAcompanhamentoOrdemServicoPorIdQuery { Id = ordemServico.Id });

        Assert.Equal(ordemServico.Id, resultado.Id);
        Assert.Equal((int)StatusOrdemServico.Recebida, resultado.Status);
        Assert.Single(resultado.HistoricoStatus);
    }

    [Fact]
    public async Task ListarPorClienteAsync_DeveRetornarAcompanhamentosDoCliente()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(cliente.Id);
        var ordensServico = new[] { CriarOrdemServico(cliente.Id, veiculo.Id, [CriarServico()]) };

        _clienteRepositoryMock.Setup(r => r.ObterPorIdAsync(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
        _ordemServicoRepositoryMock.Setup(r => r.ListarPorClienteAsync(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ordensServico);
        _mapperMock.Setup(m => m.Map<IReadOnlyCollection<AcompanhamentoOrdemServicoViewModel>>(It.IsAny<object>())).Returns(ordensServico.Select(MapearAcompanhamento).ToArray());

        var resultado = await service.ListarPorClienteAsync(new ListarOrdensServicoPorClienteQuery { ClienteId = cliente.Id });

        Assert.Single(resultado);
    }

    [Fact]
    public async Task ExcluirAsync_DeveRemoverOrdemServico_QuandoExiste()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(cliente.Id);
        var ordemServico = CriarOrdemServico(cliente.Id, veiculo.Id, [CriarServico()]);

        _ordemServicoRepositoryMock.Setup(r => r.ObterPorIdAsync(ordemServico.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ordemServico);

        await service.ExcluirAsync(new ExcluirOrdemServicoCommand { Id = ordemServico.Id });

        _ordemServicoRepositoryMock.Verify(r => r.RemoverAsync(ordemServico, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_DeveLancarKeyNotFoundException_QuandoServicoNaoExiste()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(cliente.Id);
        var servicoId = Guid.NewGuid();
        var command = new CriarOrdemServicoCommand { ClienteId = cliente.Id, VeiculoId = veiculo.Id, ServicoIds = [servicoId] };

        _clienteRepositoryMock.Setup(r => r.ObterPorIdAsync(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
        _veiculoRepositoryMock.Setup(r => r.ObterPorIdAsync(veiculo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(veiculo);
        _servicoRepositoryMock.Setup(r => r.ObterPorIdAsync(servicoId, It.IsAny<CancellationToken>())).ReturnsAsync((Servico?)null);

        var action = async () => await service.CriarAsync(command);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(action);
        Assert.Equal("Servico nao encontrado.", exception.Message);
    }

    [Fact]
    public async Task AlterarStatusParaEmDiagnosticoAsync_DeveAlterarStatusEPersistir()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(cliente.Id);
        var servico = CriarServico();
        var ordemServico = CriarOrdemServico(cliente.Id, veiculo.Id, [servico]);
        var command = new AlterarStatusOrdemServicoCommand { Id = ordemServico.Id };

        _ordemServicoRepositoryMock.Setup(r => r.ObterPorIdAsync(ordemServico.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ordemServico);
        _mapperMock.Setup(m => m.Map<OrdemServicoViewModel>(It.IsAny<object>())).Returns((object source) => MapearViewModel((OrdemServico)source));

        var resultado = await service.AlterarStatusParaEmDiagnosticoAsync(command);

        Assert.Equal((int)StatusOrdemServico.EmDiagnostico, resultado.Status);
        _ordemServicoRepositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AlterarStatusParaEmDiagnosticoAsync_DeveLancarDomainException_QuandoTransicaoInvalida()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(cliente.Id);
        var servico = CriarServico();
        var ordemServico = CriarOrdemServico(cliente.Id, veiculo.Id, [servico]);
        ordemServico.AlterarParaEmDiagnostico();
        var command = new AlterarStatusOrdemServicoCommand { Id = ordemServico.Id };

        _ordemServicoRepositoryMock.Setup(r => r.ObterPorIdAsync(ordemServico.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ordemServico);

        var action = async () => await service.AlterarStatusParaEmDiagnosticoAsync(command);

        var exception = await Assert.ThrowsAsync<DomainException>(action);
        Assert.Equal("Somente ordem de servico recebida pode ser alterada para em diagnostico.", exception.Message);
    }

    [Fact]
    public async Task AlterarStatusParaEmExecucaoAsync_DeveAlterarStatusEPersistir()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(cliente.Id);
        var servico = CriarServico();
        var ordemServico = CriarOrdemServico(cliente.Id, veiculo.Id, [servico]);
        ordemServico.AlterarParaEmDiagnostico();
        var command = new AlterarStatusOrdemServicoCommand { Id = ordemServico.Id };

        _ordemServicoRepositoryMock.Setup(r => r.ObterPorIdAsync(ordemServico.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ordemServico);
        _mapperMock.Setup(m => m.Map<OrdemServicoViewModel>(It.IsAny<object>())).Returns((object source) => MapearViewModel((OrdemServico)source));

        var resultado = await service.AlterarStatusParaEmExecucaoAsync(command);

        Assert.Equal((int)StatusOrdemServico.EmExecucao, resultado.Status);
        _ordemServicoRepositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AlterarStatusParaFinalizadaAsync_DeveAlterarStatusEPersistir()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(cliente.Id);
        var servico = CriarServico();
        var ordemServico = CriarOrdemServico(cliente.Id, veiculo.Id, [servico]);
        ordemServico.AlterarParaEmDiagnostico();
        ordemServico.AlterarParaEmExecucao();
        var command = new AlterarStatusOrdemServicoCommand { Id = ordemServico.Id };

        _ordemServicoRepositoryMock.Setup(r => r.ObterPorIdAsync(ordemServico.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ordemServico);
        _mapperMock.Setup(m => m.Map<OrdemServicoViewModel>(It.IsAny<object>())).Returns((object source) => MapearViewModel((OrdemServico)source));

        var resultado = await service.AlterarStatusParaFinalizadaAsync(command);

        Assert.Equal((int)StatusOrdemServico.Finalizada, resultado.Status);
        _ordemServicoRepositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AlterarStatusParaEntregueAsync_DeveAlterarStatusEPersistirEAtualizarIndicadores()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(cliente.Id);
        var servico = CriarServico();
        var ordemServico = CriarOrdemServico(cliente.Id, veiculo.Id, [servico]);
        ordemServico.AlterarParaEmDiagnostico();
        ordemServico.AlterarParaEmExecucao();
        ordemServico.AlterarParaFinalizada();
        var command = new AlterarStatusOrdemServicoCommand { Id = ordemServico.Id };

        _ordemServicoRepositoryMock.Setup(r => r.ObterPorIdAsync(ordemServico.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ordemServico);
        _mapperMock.Setup(m => m.Map<OrdemServicoViewModel>(It.IsAny<object>())).Returns((object source) => MapearViewModel((OrdemServico)source));

        var resultado = await service.AlterarStatusParaEntregueAsync(command);

        Assert.Equal((int)StatusOrdemServico.Entregue, resultado.Status);
        _ordemServicoRepositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Once);
        _indicadorServiceMock.Verify(r => r.AtualizarAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AlterarStatusParaEntregueAsync_DeveLancarDomainException_QuandoTransicaoInvalida()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(cliente.Id);
        var servico = CriarServico();
        var ordemServico = CriarOrdemServico(cliente.Id, veiculo.Id, [servico]);
        var command = new AlterarStatusOrdemServicoCommand { Id = ordemServico.Id };

        _ordemServicoRepositoryMock.Setup(r => r.ObterPorIdAsync(ordemServico.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ordemServico);

        var action = async () => await service.AlterarStatusParaEntregueAsync(command);

        var exception = await Assert.ThrowsAsync<DomainException>(action);
        Assert.Equal("Somente ordem de servico finalizada pode ser alterada para entregue.", exception.Message);
    }

    [Fact]
    public async Task GerarOrcamentoAsync_DeveCalcularOrcamentoEPersistirUltimoResultado()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(cliente.Id);
        var insumo = Insumo.Criar("Oleo", "Fabricante", 10, 25m);
        var servico = Servico.Criar("Troca de oleo", "Servico completo", [ItemServico.Criar(insumo, 2)]);
        var ordemServico = CriarOrdemServico(cliente.Id, veiculo.Id, [servico]);
        var command = new AlterarStatusOrdemServicoCommand { Id = ordemServico.Id };

        _ordemServicoRepositoryMock.Setup(r => r.ObterPorIdAsync(ordemServico.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ordemServico);
        _estoqueServiceMock.Setup(s => s.VerificarDisponibilidadeParaOrcamentoAsync(ordemServico.Servicos, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mapperMock.Setup(m => m.Map<OrdemServicoViewModel>(It.IsAny<object>())).Returns((object source) => MapearViewModel((OrdemServico)source));

        var primeiroResultado = await service.GerarOrcamentoAsync(command);
        insumo.AtualizarValorUnitario(40m);
        var segundoResultado = await service.GerarOrcamentoAsync(command);

        Assert.NotNull(primeiroResultado.Orcamento);
        Assert.Equal(50m, primeiroResultado.Orcamento!.ValorTotal);
        Assert.NotNull(segundoResultado.Orcamento);
        Assert.Equal(80m, segundoResultado.Orcamento!.ValorTotal);
        Assert.Equal((int)StatusOrdemServico.AguardandoAprovacao, segundoResultado.Status);
        _estoqueServiceMock.Verify(s => s.VerificarDisponibilidadeParaOrcamentoAsync(ordemServico.Servicos, It.IsAny<CancellationToken>()), Times.Exactly(2));
        _ordemServicoRepositoryMock.Verify(r => r.AtualizarAsync(ordemServico, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GerarOrcamentoAsync_DeveLancarDomainException_QuandoEstoqueInsuficiente()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(cliente.Id);
        var ordemServico = CriarOrdemServico(cliente.Id, veiculo.Id, [CriarServico()]);

        _ordemServicoRepositoryMock.Setup(r => r.ObterPorIdAsync(ordemServico.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ordemServico);
        _estoqueServiceMock.Setup(s => s.VerificarDisponibilidadeParaOrcamentoAsync(ordemServico.Servicos, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainException("Estoque insuficiente para gerar orcamento."));

        var action = async () => await service.GerarOrcamentoAsync(new AlterarStatusOrdemServicoCommand { Id = ordemServico.Id });

        var exception = await Assert.ThrowsAsync<DomainException>(action);
        Assert.Equal("Estoque insuficiente para gerar orcamento.", exception.Message);
        _ordemServicoRepositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GerarOrcamentoAsync_DeveLancarKeyNotFoundException_QuandoOrdemNaoExiste()
    {
        var service = CriarService();
        var command = new AlterarStatusOrdemServicoCommand { Id = Guid.NewGuid() };

        _ordemServicoRepositoryMock.Setup(r => r.ObterPorIdAsync(command.Id, It.IsAny<CancellationToken>())).ReturnsAsync((OrdemServico?)null);

        var action = async () => await service.GerarOrcamentoAsync(command);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(action);
        Assert.Equal("Ordem de servico nao encontrada.", exception.Message);
    }

    [Fact]
    public async Task EnviarOrcamentoPorEmailAsync_DeveEnviarQuandoOrcamentoEEmailExistem()
    {
        var service = CriarService();
        var cliente = CriarCliente(email: "cliente@teste.com");
        var veiculo = CriarVeiculo(cliente.Id);
        var servico = CriarServico();
        var ordemServico = CriarOrdemServico(cliente.Id, veiculo.Id, [servico]);
        ordemServico.GerarOrcamento();
        var command = new AlterarStatusOrdemServicoCommand { Id = ordemServico.Id };

        _ordemServicoRepositoryMock.Setup(r => r.ObterPorIdAsync(ordemServico.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ordemServico);
        _clienteRepositoryMock.Setup(r => r.ObterPorIdAsync(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);

        await service.EnviarOrcamentoPorEmailAsync(command);

        _orcamentoEmailSenderMock.Verify(r => r.EnviarOrcamentoAsync(ordemServico, "cliente@teste.com", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnviarOrcamentoPorEmailAsync_DeveLancarDomainException_QuandoOrdemSemOrcamento()
    {
        var service = CriarService();
        var cliente = CriarCliente(email: "cliente@teste.com");
        var veiculo = CriarVeiculo(cliente.Id);
        var ordemServico = CriarOrdemServico(cliente.Id, veiculo.Id, [CriarServico()]);

        _ordemServicoRepositoryMock.Setup(r => r.ObterPorIdAsync(ordemServico.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ordemServico);

        var action = async () => await service.EnviarOrcamentoPorEmailAsync(new AlterarStatusOrdemServicoCommand { Id = ordemServico.Id });

        var exception = await Assert.ThrowsAsync<DomainException>(action);
        Assert.Equal("A ordem de servico informada nao possui orcamento gerado.", exception.Message);
    }

    [Fact]
    public async Task EnviarOrcamentoPorEmailAsync_DeveLancarDomainException_QuandoClienteSemEmail()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(cliente.Id);
        var ordemServico = CriarOrdemServico(cliente.Id, veiculo.Id, [CriarServico()]);
        ordemServico.GerarOrcamento();

        _ordemServicoRepositoryMock.Setup(r => r.ObterPorIdAsync(ordemServico.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ordemServico);
        _clienteRepositoryMock.Setup(r => r.ObterPorIdAsync(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);

        var action = async () => await service.EnviarOrcamentoPorEmailAsync(new AlterarStatusOrdemServicoCommand { Id = ordemServico.Id });

        var exception = await Assert.ThrowsAsync<DomainException>(action);
        Assert.Equal("O cliente da ordem de servico nao possui email cadastrado.", exception.Message);
    }

    [Fact]
    public async Task AprovarOrcamentoAsync_DeveAlterarStatusParaEmExecucao()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(cliente.Id);
        var ordemServico = CriarOrdemServico(cliente.Id, veiculo.Id, [CriarServico()]);
        ordemServico.GerarOrcamento();

        _ordemServicoRepositoryMock.Setup(r => r.ObterPorIdAsync(ordemServico.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ordemServico);
        _estoqueServiceMock.Setup(s => s.VerificarDisponibilidadeParaOrcamentoAsync(ordemServico.Servicos, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _estoqueServiceMock.Setup(s => s.DebitarEstoqueParaOrdemServicoAsync(ordemServico.Servicos, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mapperMock.Setup(m => m.Map<OrdemServicoViewModel>(It.IsAny<object>())).Returns((object source) => MapearViewModel((OrdemServico)source));

        var resultado = await service.AprovarOrcamentoAsync(new AlterarStatusOrdemServicoCommand { Id = ordemServico.Id });

        Assert.Equal((int)StatusOrdemServico.EmExecucao, resultado.Status);
        _estoqueServiceMock.Verify(s => s.VerificarDisponibilidadeParaOrcamentoAsync(ordemServico.Servicos, It.IsAny<CancellationToken>()), Times.Once);
        _estoqueServiceMock.Verify(s => s.DebitarEstoqueParaOrdemServicoAsync(ordemServico.Servicos, It.IsAny<CancellationToken>()), Times.Once);
        _ordemServicoRepositoryMock.Verify(r => r.AtualizarAsync(ordemServico, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AprovarOrcamentoAsync_DeveLancarDomainException_QuandoEstoqueInsuficienteNaAprovacao()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(cliente.Id);
        var ordemServico = CriarOrdemServico(cliente.Id, veiculo.Id, [CriarServico()]);
        ordemServico.GerarOrcamento();

        _ordemServicoRepositoryMock.Setup(r => r.ObterPorIdAsync(ordemServico.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ordemServico);
        _estoqueServiceMock.Setup(s => s.VerificarDisponibilidadeParaOrcamentoAsync(ordemServico.Servicos, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainException("Estoque insuficiente para aprovar orcamento."));

        var action = async () => await service.AprovarOrcamentoAsync(new AlterarStatusOrdemServicoCommand { Id = ordemServico.Id });

        var exception = await Assert.ThrowsAsync<DomainException>(action);
        Assert.Equal("Estoque insuficiente para aprovar orcamento.", exception.Message);
        _estoqueServiceMock.Verify(s => s.DebitarEstoqueParaOrdemServicoAsync(It.IsAny<IReadOnlyCollection<Servico>>(), It.IsAny<CancellationToken>()), Times.Never);
        _ordemServicoRepositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RecusarOrcamentoAsync_DeveAlterarStatusParaFinalizada()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(cliente.Id);
        var ordemServico = CriarOrdemServico(cliente.Id, veiculo.Id, [CriarServico()]);
        ordemServico.GerarOrcamento();

        _ordemServicoRepositoryMock.Setup(r => r.ObterPorIdAsync(ordemServico.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ordemServico);
        _mapperMock.Setup(m => m.Map<OrdemServicoViewModel>(It.IsAny<object>())).Returns((object source) => MapearViewModel((OrdemServico)source));

        var resultado = await service.RecusarOrcamentoAsync(new AlterarStatusOrdemServicoCommand { Id = ordemServico.Id });

        Assert.Equal((int)StatusOrdemServico.Finalizada, resultado.Status);
        _ordemServicoRepositoryMock.Verify(r => r.AtualizarAsync(ordemServico, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecusarOrcamentoAsync_DeveLancarDomainException_QuandoStatusInvalido()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(cliente.Id);
        var ordemServico = CriarOrdemServico(cliente.Id, veiculo.Id, [CriarServico()]);

        _ordemServicoRepositoryMock.Setup(r => r.ObterPorIdAsync(ordemServico.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ordemServico);

        var action = async () => await service.RecusarOrcamentoAsync(new AlterarStatusOrdemServicoCommand { Id = ordemServico.Id });

        var exception = await Assert.ThrowsAsync<DomainException>(action);
        Assert.Equal("Somente ordem de servico aguardando aprovacao pode ser recusada.", exception.Message);
        _ordemServicoRepositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ListarOrdenadasAsync_DeveRetornarOrdensOrdenadasPorStatusEData()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(cliente.Id);
        var servico = CriarServico();

        var ordemRecebida = CriarOrdemServico(cliente.Id, veiculo.Id, [servico]);

        var ordemDiagnostico = CriarOrdemServico(cliente.Id, veiculo.Id, [servico]);
        ordemDiagnostico.AlterarParaEmDiagnostico(DateTime.UtcNow.AddHours(-2));

        var ordemAguardandoAprovacao = CriarOrdemServico(cliente.Id, veiculo.Id, [servico]);
        ordemAguardandoAprovacao.AlterarParaEmDiagnostico(DateTime.UtcNow.AddHours(-1));
        ordemAguardandoAprovacao.GerarOrcamento();

        var ordemEmExecucao = CriarOrdemServico(cliente.Id, veiculo.Id, [servico]);
        ordemEmExecucao.AlterarParaEmDiagnostico(DateTime.UtcNow.AddHours(-3));
        ordemEmExecucao.GerarOrcamento();
        ordemEmExecucao.AlterarParaEmExecucao();

        var ordensServico = new[] { ordemRecebida, ordemDiagnostico, ordemAguardandoAprovacao, ordemEmExecucao };

        _ordemServicoRepositoryMock.Setup(r => r.ListarOrdenadasAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ordensServico.ToArray());
        _mapperMock.Setup(m => m.Map<IReadOnlyCollection<OrdemServicoOrdenadasViewModel>>(It.IsAny<object>()))
            .Returns((object source) => ((IEnumerable<OrdemServico>)source).Select(MapearViewModelOrdenadas).ToArray());

        var resultado = await service.ListarOrdenadasAsync(new ListarOrdensServicoOrdenadasQuery());

        Assert.NotNull(resultado);
        Assert.Equal(4, resultado.Count);

        var lista = resultado.ToList();
        // Verificar que todos os 4 status diferentes estão presentes
        var statusUnicos = lista.Select(o => o.Status).Distinct().Count();
        Assert.Equal(4, statusUnicos);
        // Verificar que DataAlteracao está preenchida
        Assert.All(lista, o => Assert.NotEqual(default, o.DataAlteracao));

        _ordemServicoRepositoryMock.Verify(r => r.ListarOrdenadasAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListarOrdenadasAsync_DeveExcluirOrdensFinalizadasEntreguesEEncerradas()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(cliente.Id);
        var servico = CriarServico();

        var ordemRecebida = CriarOrdemServico(cliente.Id, veiculo.Id, [servico]);

        var ordemFinalizada = CriarOrdemServico(cliente.Id, veiculo.Id, [servico]);
        ordemFinalizada.AlterarParaEmDiagnostico();
        ordemFinalizada.AlterarParaEmExecucao();
        ordemFinalizada.AlterarParaFinalizada();

        var ordemEntregue = CriarOrdemServico(cliente.Id, veiculo.Id, [servico]);
        ordemEntregue.AlterarParaEmDiagnostico();
        ordemEntregue.AlterarParaEmExecucao();
        ordemEntregue.AlterarParaFinalizada();
        ordemEntregue.AlterarParaEntregue();

        var ordensServico = new[] { ordemRecebida, ordemFinalizada, ordemEntregue };

        _ordemServicoRepositoryMock.Setup(r => r.ListarOrdenadasAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ordensServico.ToArray());
        _mapperMock.Setup(m => m.Map<IReadOnlyCollection<OrdemServicoOrdenadasViewModel>>(It.IsAny<object>()))
            .Returns((object source) => ((IEnumerable<OrdemServico>)source).Where(os => 
                os.Status != StatusOrdemServico.Finalizada && 
                os.Status != StatusOrdemServico.Entregue && 
                os.Status != StatusOrdemServico.Encerrada)
                .Select(MapearViewModelOrdenadas).ToArray());

        var resultado = await service.ListarOrdenadasAsync(new ListarOrdensServicoOrdenadasQuery());

        Assert.NotNull(resultado);
        Assert.Single(resultado);
        Assert.Equal(ordemRecebida.Id, resultado.First().Id);

        _ordemServicoRepositoryMock.Verify(r => r.ListarOrdenadasAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListarOrdenadasAsync_DeveOrdenarPorDataDentroDoMesmoStatus()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(cliente.Id);
        var servico = CriarServico();

        var ordem1Recebida = CriarOrdemServico(cliente.Id, veiculo.Id, [servico], DateTime.UtcNow.AddDays(-3));
        var ordem2Recebida = CriarOrdemServico(cliente.Id, veiculo.Id, [servico], DateTime.UtcNow.AddDays(-1));
        var ordem3Recebida = CriarOrdemServico(cliente.Id, veiculo.Id, [servico], DateTime.UtcNow.AddDays(-2));

        var ordensServico = new[] { ordem2Recebida, ordem1Recebida, ordem3Recebida };

        _ordemServicoRepositoryMock.Setup(r => r.ListarOrdenadasAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ordensServico.ToArray());
        _mapperMock.Setup(m => m.Map<IReadOnlyCollection<OrdemServicoOrdenadasViewModel>>(It.IsAny<object>()))
            .Returns((object source) => ((IEnumerable<OrdemServico>)source).Select(MapearViewModelOrdenadas).ToArray());

        var resultado = await service.ListarOrdenadasAsync(new ListarOrdensServicoOrdenadasQuery());

        Assert.NotNull(resultado);
        Assert.Equal(3, resultado.Count);

        var lista = resultado.ToList();
        // Verificar que DataAlteracao foi preenchida em todos
        Assert.All(lista, o => Assert.NotEqual(default, o.DataAlteracao));

        _ordemServicoRepositoryMock.Verify(r => r.ListarOrdenadasAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
