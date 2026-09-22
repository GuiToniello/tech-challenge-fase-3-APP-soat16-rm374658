using TechChallenge.Oficina.Entities.Exceptions;
using TechChallenge.Oficina.Entities.Features.Orcamentos;
using TechChallenge.Oficina.Entities.Features.OrdensServico.Enums;
using TechChallenge.Oficina.Entities.Features.OrdensServico.VOs;
using TechChallenge.Oficina.Entities.Features.Servicos;

namespace TechChallenge.Oficina.Entities.Features.OrdensServico;

public class OrdemServico
{
    private readonly List<HistoricoStatusOrdemServico> _historicoStatus = [];
    private readonly List<Servico> _servicos = [];

    public Guid Id { get; private set; }
    public Guid ClienteId { get; private set; }
    public Guid VeiculoId { get; private set; }
    public StatusOrdemServico Status { get; private set; }
    public Orcamento? Orcamento { get; private set; }
    public IReadOnlyCollection<Servico> Servicos => _servicos.AsReadOnly();
    public IReadOnlyCollection<HistoricoStatusOrdemServico> HistoricoStatus => _historicoStatus.AsReadOnly();

    public static OrdemServico Criar(Guid clienteId, Guid veiculoId, IReadOnlyCollection<Servico> servicos, DateTime? dataCadastro = null)
    {
        return new OrdemServico(clienteId, veiculoId, servicos, dataCadastro);
    }

    private OrdemServico()
    {
    }

    private OrdemServico(Guid clienteId, Guid veiculoId, IReadOnlyCollection<Servico> servicos, DateTime? dataCadastro)
    {
        Id = Guid.NewGuid();
        AtualizarClienteId(clienteId);
        AtualizarVeiculoId(veiculoId);
        DefinirServicos(servicos);
        DefinirStatus(StatusOrdemServico.Recebida, dataCadastro ?? DateTime.UtcNow);
    }

    public void AtualizarClienteId(Guid clienteId)
    {
        if (clienteId == Guid.Empty)
        {
            throw new DomainException("O cliente da ordem de servico e obrigatorio.");
        }

        ClienteId = clienteId;
    }

    public void AtualizarVeiculoId(Guid veiculoId)
    {
        if (veiculoId == Guid.Empty)
        {
            throw new DomainException("O veiculo da ordem de servico e obrigatorio.");
        }

        VeiculoId = veiculoId;
    }

    public void DefinirServicos(IReadOnlyCollection<Servico> servicos)
    {
        if (servicos is null || servicos.Count == 0)
        {
            throw new DomainException("A ordem de servico deve possuir ao menos um servico.");
        }

        _servicos.Clear();

        var servicosIdsAdicionados = new HashSet<Guid>();
        _servicos.AddRange(servicos.Where(servico => servicosIdsAdicionados.Add(servico.Id)));

        if (_servicos.Count == 0)
        {
            throw new DomainException("A ordem de servico deve possuir ao menos um servico.");
        }
    }

    public void AlterarParaEmDiagnostico(DateTime? dataAlteracao = null)
    {
        if (Status != StatusOrdemServico.Recebida)
        {
            throw new DomainException("Somente ordem de servico recebida pode ser alterada para em diagnostico.");
        }

        DefinirStatus(StatusOrdemServico.EmDiagnostico, dataAlteracao ?? DateTime.UtcNow);
    }

    public void AlterarParaEmExecucao(DateTime? dataAlteracao = null)
    {
        if (Status != StatusOrdemServico.EmDiagnostico && Status != StatusOrdemServico.AguardandoAprovacao)
        {
            throw new DomainException("Somente ordem de servico em diagnostico ou aguardando aprovacao pode ser alterada para em execucao.");
        }

        DefinirStatus(StatusOrdemServico.EmExecucao, dataAlteracao ?? DateTime.UtcNow);
    }

    public void AlterarParaFinalizada(DateTime? dataAlteracao = null)
    {
        if (Status != StatusOrdemServico.EmExecucao)
        {
            throw new DomainException("Somente ordem de servico em execucao pode ser alterada para finalizada.");
        }

        DefinirStatus(StatusOrdemServico.Finalizada, dataAlteracao ?? DateTime.UtcNow);
    }

    public void AlterarParaEntregue(DateTime? dataAlteracao = null)
    {
        if (Status != StatusOrdemServico.Finalizada)
        {
            throw new DomainException("Somente ordem de servico finalizada pode ser alterada para entregue.");
        }

        DefinirStatus(StatusOrdemServico.Entregue, dataAlteracao ?? DateTime.UtcNow);
    }

    public void GerarOrcamento(DateTime? dataGeracao = null)
    {
        Orcamento = Orcamento.Criar(Id, Servicos, dataGeracao ?? DateTime.UtcNow);

        if (Status != StatusOrdemServico.AguardandoAprovacao)
        {
            DefinirStatus(StatusOrdemServico.AguardandoAprovacao, dataGeracao ?? DateTime.UtcNow);
        }
    }

    public void AprovarOrcamento(DateTime? dataAprovacao = null)
    {
        if (Status != StatusOrdemServico.AguardandoAprovacao)
        {
            throw new DomainException("Somente ordem de servico aguardando aprovacao pode ser aprovada.");
        }

        DefinirStatus(StatusOrdemServico.EmExecucao, dataAprovacao ?? DateTime.UtcNow);
    }

    public void RecusarOrcamento(DateTime? dataRecusa = null)
    {
        if (Status != StatusOrdemServico.AguardandoAprovacao)
        {
            throw new DomainException("Somente ordem de servico aguardando aprovacao pode ser recusada.");
        }

        DefinirStatus(StatusOrdemServico.Finalizada, dataRecusa ?? DateTime.UtcNow);
    }

    public TimeSpan ObterTempoExecucao()
    {
        var inicio = ObterDataHistoricoObrigatoria(StatusOrdemServico.EmExecucao);
        var fim = ObterDataHistoricoObrigatoria(StatusOrdemServico.Finalizada);
        return fim - inicio;
    }

    public TimeSpan ObterTempoEntrega()
    {
        var recebida = ObterDataHistoricoObrigatoria(StatusOrdemServico.Recebida);
        var entregue = ObterDataHistoricoObrigatoria(StatusOrdemServico.Entregue);
        return entregue - recebida;
    }

    private void DefinirStatus(StatusOrdemServico novoStatus, DateTime dataAlteracao)
    {
        Status = novoStatus;
        _historicoStatus.Add(HistoricoStatusOrdemServico.Criar(novoStatus, dataAlteracao));
    }

    private DateTime ObterDataHistoricoObrigatoria(StatusOrdemServico status)
    {
        var historico = _historicoStatus.LastOrDefault(item => item.Status == status);

        if (historico is null)
        {
            throw new DomainException("A ordem de servico nao possui historico suficiente para calcular os indicadores.");
        }

        return historico.DataAlteracao;
    }
}
