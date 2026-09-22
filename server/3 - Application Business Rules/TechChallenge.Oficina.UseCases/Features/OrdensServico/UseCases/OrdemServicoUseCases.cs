using AutoMapper;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.Commands;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.Queries;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.Entities.Exceptions;
using TechChallenge.Oficina.Entities.Features.OrdensServico;
using TechChallenge.Oficina.Entities.Features.OrdensServico.Enums;
using TechChallenge.Oficina.Entities.Features.Servicos;
using TechChallenge.Oficina.Entities.Features.Veiculos;
using TechChallenge.Oficina.UseCases.Features.Clientes.UseCases;
using TechChallenge.Oficina.UseCases.Features.Servicos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Veiculos.UseCases;

namespace TechChallenge.Oficina.UseCases.Features.OrdensServico.UseCases;

public sealed class OrdemServicoUseCases : IOrdemServicoUseCases
{
    private readonly IMapper _mapper;
    private readonly IOrdemServicoGateway _ordemServicoGateway;
    private readonly IClienteGateway _clienteGateway;
    private readonly IVeiculoGateway _veiculoGateway;
    private readonly IServicoGateway _servicoGateway;
    private readonly IOrdemServicoUseCasesFacade _ordemServicoServicesFacade;

    public OrdemServicoUseCases(IMapper mapper, IOrdemServicoGateway ordemServicoGateway, IClienteGateway clienteGateway, IVeiculoGateway veiculoGateway, IServicoGateway servicoGateway, IOrdemServicoUseCasesFacade ordemServicoServicesFacade)
    {
        _mapper = mapper;
        _ordemServicoGateway = ordemServicoGateway;
        _clienteGateway = clienteGateway;
        _veiculoGateway = veiculoGateway;
        _servicoGateway = servicoGateway;
        _ordemServicoServicesFacade = ordemServicoServicesFacade;
    }

    public async Task<OrdemServicoViewModel> CriarAsync(CriarOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        await ValidarClienteExistenteAsync(command.ClienteId, cancellationToken);
        var veiculo = await ObterVeiculoExistenteAsync(command.VeiculoId, cancellationToken);
        ValidarVeiculoPertenceAoCliente(veiculo, command.ClienteId);
        var servicos = await ObterServicosAsync(command.ServicoIds, cancellationToken);

        var ordemServico = OrdemServico.Criar(command.ClienteId, command.VeiculoId, servicos);

        await _ordemServicoGateway.AdicionarAsync(ordemServico, cancellationToken);

        return _mapper.Map<OrdemServicoViewModel>(ordemServico);
    }

    public async Task<OrdemServicoViewModel> AtualizarAsync(AtualizarOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ObterOrdemServicoExistenteAsync(command.Id, cancellationToken);

        await ValidarClienteExistenteAsync(command.ClienteId, cancellationToken);
        var veiculo = await ObterVeiculoExistenteAsync(command.VeiculoId, cancellationToken);
        ValidarVeiculoPertenceAoCliente(veiculo, command.ClienteId);
        var servicos = await ObterServicosAsync(command.ServicoIds, cancellationToken);

        ordemServico.AtualizarClienteId(command.ClienteId);
        ordemServico.AtualizarVeiculoId(command.VeiculoId);
        ordemServico.DefinirServicos(servicos);

        await _ordemServicoGateway.AtualizarAsync(ordemServico, cancellationToken);

        return _mapper.Map<OrdemServicoViewModel>(ordemServico);
    }

    public async Task<OrdemServicoViewModel> ObterPorIdAsync(ObterOrdemServicoPorIdQuery query, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ObterOrdemServicoExistenteAsync(query.Id, cancellationToken);
        return _mapper.Map<OrdemServicoViewModel>(ordemServico);
    }

    public async Task<IReadOnlyCollection<OrdemServicoViewModel>> ListarAsync(ListarOrdensServicoQuery query, CancellationToken cancellationToken = default)
    {
        var ordensServico = await _ordemServicoGateway.ListarAsync(cancellationToken);
        return _mapper.Map<IReadOnlyCollection<OrdemServicoViewModel>>(ordensServico);
    }

    public async Task<IReadOnlyCollection<OrdemServicoOrdenadasViewModel>> ListarOrdenadasAsync(ListarOrdensServicoOrdenadasQuery query, CancellationToken cancellationToken = default)
    {
        var ordensServico = await _ordemServicoGateway.ListarOrdenadasAsync(cancellationToken);
        return _mapper.Map<IReadOnlyCollection<OrdemServicoOrdenadasViewModel>>(ordensServico);
    }

    public async Task ExcluirAsync(ExcluirOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ObterOrdemServicoExistenteAsync(command.Id, cancellationToken);
        await _ordemServicoGateway.RemoverAsync(ordemServico, cancellationToken);
    }

    public async Task<AcompanhamentoOrdemServicoViewModel> ObterAcompanhamentoAsync(ObterAcompanhamentoOrdemServicoPorIdQuery query, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ObterOrdemServicoExistenteAsync(query.Id, cancellationToken);
        return _mapper.Map<AcompanhamentoOrdemServicoViewModel>(ordemServico);
    }

    public async Task<IReadOnlyCollection<AcompanhamentoOrdemServicoViewModel>> ListarPorClienteAsync(ListarOrdensServicoPorClienteQuery query, CancellationToken cancellationToken = default)
    {
        await ValidarClienteExistenteAsync(query.ClienteId, cancellationToken);

        var ordensServico = await _ordemServicoGateway.ListarPorClienteAsync(query.ClienteId, cancellationToken);
        return _mapper.Map<IReadOnlyCollection<AcompanhamentoOrdemServicoViewModel>>(ordensServico);
    }

    public async Task<OrdemServicoViewModel> AlterarStatusParaEmDiagnosticoAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ObterOrdemServicoExistenteAsync(command.Id, cancellationToken);
        var statusAnterior = ordemServico.Status;

        ordemServico.AlterarParaEmDiagnostico();
        await _ordemServicoGateway.AtualizarAsync(ordemServico, cancellationToken);

        await EnviarNotificacaoStatusAlteradoAsync(ordemServico, statusAnterior, cancellationToken);

        return _mapper.Map<OrdemServicoViewModel>(ordemServico);
    }

    public async Task<OrdemServicoViewModel> GerarOrcamentoAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ObterOrdemServicoExistenteAsync(command.Id, cancellationToken);
        await _ordemServicoServicesFacade.EstoqueService.VerificarDisponibilidadeParaOrcamentoAsync(ordemServico.Servicos, cancellationToken);
        ordemServico.GerarOrcamento();
        await _ordemServicoGateway.AtualizarAsync(ordemServico, cancellationToken);
        return _mapper.Map<OrdemServicoViewModel>(ordemServico);
    }

    public async Task EnviarOrcamentoPorEmailAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ObterOrdemServicoExistenteAsync(command.Id, cancellationToken);

        if (ordemServico.Orcamento is null)
        {
            throw new DomainException("A ordem de servico informada nao possui orcamento gerado.");
        }

        var cliente = await _clienteGateway.ObterPorIdAsync(ordemServico.ClienteId, cancellationToken)
            ?? throw new KeyNotFoundException("Cliente não encontrado.");

        if (string.IsNullOrWhiteSpace(cliente.Email))
        {
            throw new DomainException("O cliente da ordem de servico nao possui email cadastrado.");
        }

        await _ordemServicoServicesFacade.OrcamentoEmailSender.EnviarOrcamentoAsync(ordemServico, cliente.Email, cancellationToken);
    }

    public async Task<OrdemServicoViewModel> AprovarOrcamentoAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ObterOrdemServicoExistenteAsync(command.Id, cancellationToken);
        await _ordemServicoServicesFacade.EstoqueService.VerificarDisponibilidadeParaOrcamentoAsync(ordemServico.Servicos, cancellationToken);
        await _ordemServicoServicesFacade.EstoqueService.DebitarEstoqueParaOrdemServicoAsync(ordemServico.Servicos, cancellationToken);
        ordemServico.AprovarOrcamento();
        await _ordemServicoGateway.AtualizarAsync(ordemServico, cancellationToken);
        return _mapper.Map<OrdemServicoViewModel>(ordemServico);
    }

    public async Task<OrdemServicoViewModel> RecusarOrcamentoAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ObterOrdemServicoExistenteAsync(command.Id, cancellationToken);
        ordemServico.RecusarOrcamento();
        await _ordemServicoGateway.AtualizarAsync(ordemServico, cancellationToken);
        return _mapper.Map<OrdemServicoViewModel>(ordemServico);
    }

    public async Task<OrdemServicoViewModel> AlterarStatusParaEmExecucaoAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ObterOrdemServicoExistenteAsync(command.Id, cancellationToken);
        var statusAnterior = ordemServico.Status;

        ordemServico.AlterarParaEmExecucao();
        await _ordemServicoGateway.AtualizarAsync(ordemServico, cancellationToken);

        await EnviarNotificacaoStatusAlteradoAsync(ordemServico, statusAnterior, cancellationToken);

        return _mapper.Map<OrdemServicoViewModel>(ordemServico);
    }

    public async Task<OrdemServicoViewModel> AlterarStatusParaFinalizadaAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ObterOrdemServicoExistenteAsync(command.Id, cancellationToken);
        var statusAnterior = ordemServico.Status;

        ordemServico.AlterarParaFinalizada();
        await _ordemServicoGateway.AtualizarAsync(ordemServico, cancellationToken);

        await EnviarNotificacaoStatusAlteradoAsync(ordemServico, statusAnterior, cancellationToken);

        return _mapper.Map<OrdemServicoViewModel>(ordemServico);
    }

    public async Task<OrdemServicoViewModel> AlterarStatusParaEntregueAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ObterOrdemServicoExistenteAsync(command.Id, cancellationToken);
        var statusAnterior = ordemServico.Status;

        ordemServico.AlterarParaEntregue();
        await _ordemServicoGateway.AtualizarAsync(ordemServico, cancellationToken);

        await EnviarNotificacaoStatusAlteradoAsync(ordemServico, statusAnterior, cancellationToken);
        await _ordemServicoServicesFacade.IndicadorService.AtualizarAsync(cancellationToken);

        return _mapper.Map<OrdemServicoViewModel>(ordemServico);
    }

    private async Task<OrdemServico> ObterOrdemServicoExistenteAsync(Guid id, CancellationToken cancellationToken)
    {
        var ordemServico = await _ordemServicoGateway.ObterPorIdAsync(id, cancellationToken);

        if (ordemServico is null)
        {
            throw new KeyNotFoundException("Ordem de servico nao encontrada.");
        }

        return ordemServico;
    }

    private async Task EnviarNotificacaoStatusAlteradoAsync(OrdemServico ordemServico, StatusOrdemServico statusAnterior, CancellationToken cancellationToken)
    {
        try
        {
            var cliente = await _clienteGateway.ObterPorIdAsync(ordemServico.ClienteId, cancellationToken);

            if (cliente is not null && !string.IsNullOrWhiteSpace(cliente.Email))
            {
                await _ordemServicoServicesFacade.OrdemServicoStatusEmailSender.EnviarStatusAlteradoAsync(
                    ordemServico,
                    cliente.Email,
                    ordemServico.Status,
                    cancellationToken);
            }
        }
        catch
        {
            // Modo degradado: falhas no envio de email não interrompem o fluxo de atualização de status
        }
    }

    private async Task ValidarClienteExistenteAsync(Guid clienteId, CancellationToken cancellationToken)
    {
        var cliente = await _clienteGateway.ObterPorIdAsync(clienteId, cancellationToken);

        if (cliente is null)
        {
            throw new KeyNotFoundException("Cliente não encontrado.");
        }
    }

    private async Task<Veiculo> ObterVeiculoExistenteAsync(Guid veiculoId, CancellationToken cancellationToken)
    {
        var veiculo = await _veiculoGateway.ObterPorIdAsync(veiculoId, cancellationToken);

        if (veiculo is null)
        {
            throw new KeyNotFoundException("Veículo não encontrado.");
        }

        return veiculo;
    }

    private static void ValidarVeiculoPertenceAoCliente(Veiculo veiculo, Guid clienteId)
    {
        if (veiculo.ClienteId != clienteId)
        {
            throw new DomainException("O veiculo informado deve estar vinculado ao cliente da ordem de servico.");
        }
    }

    private async Task<IReadOnlyCollection<Servico>> ObterServicosAsync(IReadOnlyCollection<Guid> servicoIds, CancellationToken cancellationToken)
    {
        if (servicoIds is null || servicoIds.Count == 0)
        {
            throw new DomainException("A ordem de servico deve possuir ao menos um servico.");
        }

        if (servicoIds.Any(id => id == Guid.Empty))
        {
            throw new DomainException("Todos os servicos informados devem possuir identificador valido.");
        }

        var ids = servicoIds.Distinct().ToArray();
        var servicos = new List<Servico>(ids.Length);

        foreach (var servicoId in ids)
        {
            var servico = await _servicoGateway.ObterPorIdAsync(servicoId, cancellationToken);

            if (servico is null)
            {
                throw new KeyNotFoundException("Servico nao encontrado.");
            }

            servicos.Add(servico);
        }

        return servicos;
    }
}
