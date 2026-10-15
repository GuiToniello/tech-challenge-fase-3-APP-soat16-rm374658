using AutoMapper;
using Microsoft.Extensions.Logging;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.Commands;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.Logging;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.Queries;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.Entities.Exceptions;
using TechChallenge.Oficina.Entities.Features.Clientes;
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
    private readonly ILogger<OrdemServicoUseCases> _logger;

    public OrdemServicoUseCases(IMapper mapper, IOrdemServicoGateway ordemServicoGateway, IClienteGateway clienteGateway, IVeiculoGateway veiculoGateway, IServicoGateway servicoGateway, IOrdemServicoUseCasesFacade ordemServicoServicesFacade, ILogger<OrdemServicoUseCases> logger)
    {
        _mapper = mapper;
        _ordemServicoGateway = ordemServicoGateway;
        _clienteGateway = clienteGateway;
        _veiculoGateway = veiculoGateway;
        _servicoGateway = servicoGateway;
        _ordemServicoServicesFacade = ordemServicoServicesFacade;
        _logger = logger;
    }

    public async Task<OrdemServicoViewModel> CriarAsync(CriarOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        await ValidarClienteExistenteAsync(command.ClienteId, cancellationToken);
        var veiculo = await ObterVeiculoExistenteAsync(command.VeiculoId, cancellationToken);
        ValidarVeiculoPertenceAoCliente(veiculo, command.ClienteId);
        var servicos = await ObterServicosAsync(command.ServicoIds, cancellationToken);

        var ordemServico = OrdemServico.Criar(command.ClienteId, command.VeiculoId, servicos);

        await _ordemServicoGateway.AdicionarAsync(ordemServico, cancellationToken);

        _logger.LogInformation(
            OrdemServicoLogEventos.OsCriada,
            "Ordem de servico {OrdemServicoId} criada para o cliente {ClienteId} e o veiculo {VeiculoId} com {QuantidadeServicos} servicos.",
            ordemServico.Id,
            ordemServico.ClienteId,
            ordemServico.VeiculoId,
            ordemServico.Servicos.Count);

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
        var dataEntradaStatusAnterior = ordemServico.HistoricoStatus.Max(historico => historico.DataAlteracao);
        var agora = DateTime.UtcNow;

        ordemServico.AlterarParaEmDiagnostico(agora);
        await _ordemServicoGateway.AtualizarAsync(ordemServico, cancellationToken);
        RegistrarTransicao(ordemServico, OrdemServicoLogEventos.Etapas.Diagnostico, statusAnterior, dataEntradaStatusAnterior, agora);

        await EnviarNotificacaoStatusAlteradoAsync(ordemServico, statusAnterior, cancellationToken);

        return _mapper.Map<OrdemServicoViewModel>(ordemServico);
    }

    public async Task<OrdemServicoViewModel> GerarOrcamentoAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ObterOrdemServicoExistenteAsync(command.Id, cancellationToken);
        ordemServico.ValidarGeracaoOrcamento();
        await _ordemServicoServicesFacade.EstoqueService.VerificarDisponibilidadeParaOrcamentoAsync(ordemServico.Servicos, cancellationToken);

        var statusAnterior = ordemServico.Status;
        var dataEntradaStatusAnterior = ordemServico.HistoricoStatus.Max(historico => historico.DataAlteracao);
        var agora = DateTime.UtcNow;
        var regerado = ordemServico.Orcamento is not null;

        ordemServico.GerarOrcamento(agora);
        await _ordemServicoGateway.AtualizarAsync(ordemServico, cancellationToken);

        if (ordemServico.Status != statusAnterior)
        {
            RegistrarTransicao(ordemServico, OrdemServicoLogEventos.Etapas.OrcamentoGeracao, statusAnterior, dataEntradaStatusAnterior, agora);
        }

        _logger.LogInformation(
            OrdemServicoLogEventos.OsOrcamentoGerado,
            "Orcamento da ordem de servico {OrdemServicoId} gerado com valor total {ValorTotal} (regerado: {Regerado}).",
            ordemServico.Id,
            ordemServico.Orcamento!.ValorTotal,
            regerado);

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

        // O timeout do HttpClient também é OperationCanceledException; só o cancelamento da requisição sai sem registro.
        try
        {
            await _ordemServicoServicesFacade.OrcamentoEmailSender.EnviarOrcamentoAsync(ordemServico, cliente.Email, cancellationToken);
        }
        catch (Exception ex) when (ex is not DomainException && (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested))
        {
            _logger.LogError(
                OrdemServicoLogEventos.IntegracaoFalha,
                ex,
                "Falha na integracao {Integracao} durante a operacao {Operacao} da ordem de servico {OrdemServicoId}.",
                OrdemServicoLogEventos.Integracoes.Resend,
                OrdemServicoLogEventos.Operacoes.EnvioOrcamento,
                ordemServico.Id);
            throw;
        }

        _logger.LogInformation(
            OrdemServicoLogEventos.OsOrcamentoEnviado,
            "Orcamento da ordem de servico {OrdemServicoId} enviado por email com valor total {ValorTotal}.",
            ordemServico.Id,
            ordemServico.Orcamento.ValorTotal);
    }

    public async Task<OrdemServicoViewModel> AprovarOrcamentoAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ObterOrdemServicoExistenteAsync(command.Id, cancellationToken);
        ordemServico.ValidarAprovacaoOrcamento();
        await _ordemServicoServicesFacade.EstoqueService.VerificarDisponibilidadeParaOrcamentoAsync(ordemServico.Servicos, cancellationToken);
        await _ordemServicoServicesFacade.EstoqueService.DebitarEstoqueParaOrdemServicoAsync(ordemServico.Servicos, cancellationToken);

        var statusAnterior = ordemServico.Status;
        var dataEntradaStatusAnterior = ordemServico.HistoricoStatus.Max(historico => historico.DataAlteracao);
        var agora = DateTime.UtcNow;

        ordemServico.AprovarOrcamento(agora);
        await _ordemServicoGateway.AtualizarAsync(ordemServico, cancellationToken);
        RegistrarTransicao(ordemServico, OrdemServicoLogEventos.Etapas.OrcamentoAprovacao, statusAnterior, dataEntradaStatusAnterior, agora);

        return _mapper.Map<OrdemServicoViewModel>(ordemServico);
    }

    public async Task<OrdemServicoViewModel> RecusarOrcamentoAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ObterOrdemServicoExistenteAsync(command.Id, cancellationToken);
        var statusAnterior = ordemServico.Status;
        var dataEntradaStatusAnterior = ordemServico.HistoricoStatus.Max(historico => historico.DataAlteracao);
        var agora = DateTime.UtcNow;

        ordemServico.RecusarOrcamento(agora);
        await _ordemServicoGateway.AtualizarAsync(ordemServico, cancellationToken);
        RegistrarTransicao(ordemServico, OrdemServicoLogEventos.Etapas.OrcamentoRecusa, statusAnterior, dataEntradaStatusAnterior, agora);

        return _mapper.Map<OrdemServicoViewModel>(ordemServico);
    }

    public async Task<OrdemServicoViewModel> AlterarStatusParaEmExecucaoAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ObterOrdemServicoExistenteAsync(command.Id, cancellationToken);
        var statusAnterior = ordemServico.Status;
        var dataEntradaStatusAnterior = ordemServico.HistoricoStatus.Max(historico => historico.DataAlteracao);
        var agora = DateTime.UtcNow;

        ordemServico.AlterarParaEmExecucao(agora);
        await _ordemServicoGateway.AtualizarAsync(ordemServico, cancellationToken);
        RegistrarTransicao(ordemServico, OrdemServicoLogEventos.Etapas.Execucao, statusAnterior, dataEntradaStatusAnterior, agora);

        await EnviarNotificacaoStatusAlteradoAsync(ordemServico, statusAnterior, cancellationToken);

        return _mapper.Map<OrdemServicoViewModel>(ordemServico);
    }

    public async Task<OrdemServicoViewModel> AlterarStatusParaFinalizadaAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ObterOrdemServicoExistenteAsync(command.Id, cancellationToken);
        var statusAnterior = ordemServico.Status;
        var dataEntradaStatusAnterior = ordemServico.HistoricoStatus.Max(historico => historico.DataAlteracao);
        var agora = DateTime.UtcNow;

        ordemServico.AlterarParaFinalizada(agora);
        await _ordemServicoGateway.AtualizarAsync(ordemServico, cancellationToken);
        RegistrarTransicao(ordemServico, OrdemServicoLogEventos.Etapas.Finalizacao, statusAnterior, dataEntradaStatusAnterior, agora);

        await EnviarNotificacaoStatusAlteradoAsync(ordemServico, statusAnterior, cancellationToken);

        return _mapper.Map<OrdemServicoViewModel>(ordemServico);
    }

    public async Task<OrdemServicoViewModel> AlterarStatusParaEntregueAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ObterOrdemServicoExistenteAsync(command.Id, cancellationToken);
        var statusAnterior = ordemServico.Status;
        var dataEntradaStatusAnterior = ordemServico.HistoricoStatus.Max(historico => historico.DataAlteracao);
        var agora = DateTime.UtcNow;

        ordemServico.AlterarParaEntregue(agora);
        await _ordemServicoGateway.AtualizarAsync(ordemServico, cancellationToken);
        RegistrarTransicao(ordemServico, OrdemServicoLogEventos.Etapas.Entrega, statusAnterior, dataEntradaStatusAnterior, agora);

        _logger.LogInformation(
            OrdemServicoLogEventos.OsEncerrada,
            "Ordem de servico {OrdemServicoId} encerrada apos {TempoTotalSegundos} segundos desde o recebimento.",
            ordemServico.Id,
            ordemServico.ObterTempoEntrega().TotalSeconds);

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

    private void RegistrarTransicao(OrdemServico ordemServico, string etapa, StatusOrdemServico statusAnterior, DateTime dataEntradaStatusAnterior, DateTime dataAlteracao)
    {
        _logger.LogInformation(
            OrdemServicoLogEventos.OsStatusAlterado,
            "Ordem de servico {OrdemServicoId} alterada de {StatusAnterior} para {StatusNovo} na etapa {Etapa} apos {TempoNoStatusAnteriorSegundos} segundos no status anterior.",
            ordemServico.Id,
            OrdemServicoLogEventos.StatusParaLog(statusAnterior),
            OrdemServicoLogEventos.StatusParaLog(ordemServico.Status),
            etapa,
            (dataAlteracao - dataEntradaStatusAnterior).TotalSeconds);
    }

    private async Task EnviarNotificacaoStatusAlteradoAsync(OrdemServico ordemServico, StatusOrdemServico statusAnterior, CancellationToken cancellationToken)
    {
        // Modo degradado: falhas na busca do cliente ou no envio de email (inclusive timeout) não interrompem o fluxo de atualização de status; só o cancelamento da requisição é propagado
        Cliente? cliente;

        try
        {
            cliente = await _clienteGateway.ObterPorIdAsync(ordemServico.ClienteId, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                ex,
                "Falha ao buscar o cliente da ordem de servico {OrdemServicoId} para notificar a alteracao de status.",
                ordemServico.Id);
            return;
        }

        if (cliente is null || string.IsNullOrWhiteSpace(cliente.Email))
        {
            return;
        }

        try
        {
            await _ordemServicoServicesFacade.OrdemServicoStatusEmailSender.EnviarStatusAlteradoAsync(
                ordemServico,
                cliente.Email,
                ordemServico.Status,
                cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                OrdemServicoLogEventos.IntegracaoFalha,
                ex,
                "Falha na integracao {Integracao} durante a operacao {Operacao} da ordem de servico {OrdemServicoId} com status {StatusNovo}.",
                OrdemServicoLogEventos.Integracoes.Resend,
                OrdemServicoLogEventos.Operacoes.NotificacaoStatus,
                ordemServico.Id,
                OrdemServicoLogEventos.StatusParaLog(ordemServico.Status));
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
