using TechChallenge.Oficina.Entities.Exceptions;
using TechChallenge.Oficina.Entities.Features.Insumos;
using TechChallenge.Oficina.Entities.Features.OrdensServico;
using TechChallenge.Oficina.Entities.Features.OrdensServico.Enums;
using TechChallenge.Oficina.Entities.Features.Servicos;
using Xunit;

namespace TechChallenge.Oficina.Entities.Tests.Features.OrdensServico;

public sealed class OrdemServicoDomainTests
{
    private static Servico CriarServico(string nome = "Revisao") => Servico.Criar(nome, "Servico completo", []);

    private static Servico CriarServicoComItens(string nome, params (int quantidade, decimal valorUnitario)[] itens)
    {
        var itensServico = itens
            .Select((item, indice) =>
            {
                var insumo = Insumo.Criar($"Insumo {indice + 1}", "Fabricante", 10, item.valorUnitario);
                return ItemServico.Criar(insumo, item.quantidade);
            })
            .ToArray();

        return Servico.Criar(nome, "Servico completo", itensServico);
    }

    [Fact]
    public void OrdemServico_Criar_DeveIniciarComStatusRecebidaEHistorico()
    {
        var clienteId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();
        var servico = CriarServico();

        var ordemServico = OrdemServico.Criar(clienteId, veiculoId, [servico], new DateTime(2024, 01, 01, 10, 00, 00, DateTimeKind.Utc));

        Assert.NotEqual(Guid.Empty, ordemServico.Id);
        Assert.Equal(clienteId, ordemServico.ClienteId);
        Assert.Equal(veiculoId, ordemServico.VeiculoId);
        Assert.Equal(StatusOrdemServico.Recebida, ordemServico.Status);
        Assert.Single(ordemServico.Servicos);
        Assert.Single(ordemServico.HistoricoStatus);
        Assert.Equal(StatusOrdemServico.Recebida, ordemServico.HistoricoStatus.Single().Status);
    }

    [Fact]
    public void OrdemServico_AlteracoesDeStatus_DevemRegistrarHistorico()
    {
        var ordemServico = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), [CriarServico()], new DateTime(2024, 01, 01, 8, 00, 00, DateTimeKind.Utc));

        ordemServico.AlterarParaEmDiagnostico(new DateTime(2024, 01, 01, 9, 00, 00, DateTimeKind.Utc));
        ordemServico.AlterarParaEmExecucao(new DateTime(2024, 01, 01, 10, 00, 00, DateTimeKind.Utc));
        ordemServico.AlterarParaFinalizada(new DateTime(2024, 01, 01, 12, 00, 00, DateTimeKind.Utc));
        ordemServico.AlterarParaEntregue(new DateTime(2024, 01, 01, 14, 00, 00, DateTimeKind.Utc));

        Assert.Equal(5, ordemServico.HistoricoStatus.Count);
        Assert.Equal(StatusOrdemServico.Entregue, ordemServico.Status);
    }

    [Fact]
    public void OrdemServico_DeveCalcularTempoExecucaoEEntrega()
    {
        var ordemServico = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), [CriarServico()], new DateTime(2024, 01, 01, 8, 00, 00, DateTimeKind.Utc));
        ordemServico.AlterarParaEmDiagnostico(new DateTime(2024, 01, 01, 9, 00, 00, DateTimeKind.Utc));
        ordemServico.AlterarParaEmExecucao(new DateTime(2024, 01, 01, 10, 00, 00, DateTimeKind.Utc));
        ordemServico.AlterarParaFinalizada(new DateTime(2024, 01, 01, 13, 00, 00, DateTimeKind.Utc));
        ordemServico.AlterarParaEntregue(new DateTime(2024, 01, 01, 14, 00, 00, DateTimeKind.Utc));

        Assert.Equal(TimeSpan.FromHours(3), ordemServico.ObterTempoExecucao());
        Assert.Equal(TimeSpan.FromHours(6), ordemServico.ObterTempoEntrega());
    }

    [Fact]
    public void OrdemServico_AtualizarClienteId_DeveLancarQuandoClienteForInvalido()
    {
        var ordemServico = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), [CriarServico()]);

        var action = () => ordemServico.AtualizarClienteId(Guid.Empty);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("O cliente da ordem de servico e obrigatorio.", exception.Message);
    }

    [Fact]
    public void OrdemServico_AtualizarVeiculoId_DeveLancarQuandoVeiculoForInvalido()
    {
        var ordemServico = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), [CriarServico()]);

        var action = () => ordemServico.AtualizarVeiculoId(Guid.Empty);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("O veiculo da ordem de servico e obrigatorio.", exception.Message);
    }

    [Fact]
    public void OrdemServico_DefinirServicos_DeveLancarQuandoColecaoEstiverVazia()
    {
        var ordemServico = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), [CriarServico()]);

        var action = () => ordemServico.DefinirServicos([]);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("A ordem de servico deve possuir ao menos um servico.", exception.Message);
    }

    [Fact]
    public void OrdemServico_DefinirServicos_DeveRemoverDuplicadosPorId()
    {
        var servico = CriarServico();
        var ordemServico = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), [servico]);

        ordemServico.DefinirServicos([servico, servico]);

        Assert.Single(ordemServico.Servicos);
    }

    [Theory]
    [InlineData(StatusOrdemServico.Recebida, "Recebida")]
    [InlineData(StatusOrdemServico.EmDiagnostico, "Em diagnóstico")]
    [InlineData(StatusOrdemServico.AguardandoAprovacao, "Aguardando aprovação")]
    [InlineData(StatusOrdemServico.EmExecucao, "Em execução")]
    [InlineData(StatusOrdemServico.Finalizada, "Finalizada")]
    [InlineData(StatusOrdemServico.Entregue, "Entregue")]
    [InlineData(StatusOrdemServico.Encerrada, "Encerrada")]
    public void StatusOrdemServicoExtensions_ObterDescricao_DeveRetornarDescricaoEsperada(StatusOrdemServico status, string descricaoEsperada)
    {
        var descricao = status.ObterDescricao();

        Assert.Equal(descricaoEsperada, descricao);
    }

    [Fact]
    public void OrdemServico_AlterarParaEmDiagnostico_DeveAlterarStatus_QuandoStatusForRecebida()
    {
        var ordemServico = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), [CriarServico()]);

        ordemServico.AlterarParaEmDiagnostico();

        Assert.Equal(StatusOrdemServico.EmDiagnostico, ordemServico.Status);
    }

    [Fact]
    public void OrdemServico_AlterarParaEmDiagnostico_DeveLancar_QuandoStatusForDiferente()
    {
        var ordemServico = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), [CriarServico()]);
        ordemServico.AlterarParaEmDiagnostico();

        var action = () => ordemServico.AlterarParaEmDiagnostico();

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("Somente ordem de servico recebida pode ser alterada para em diagnostico.", exception.Message);
    }

    [Fact]
    public void OrdemServico_AlterarParaEmExecucao_DeveAlterarStatus_QuandoStatusForEmDiagnostico()
    {
        var ordemServico = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), [CriarServico()]);
        ordemServico.AlterarParaEmDiagnostico();

        ordemServico.AlterarParaEmExecucao();

        Assert.Equal(StatusOrdemServico.EmExecucao, ordemServico.Status);
    }

    [Fact]
    public void OrdemServico_AlterarParaEmExecucao_DeveAlterarStatus_QuandoStatusForAguardandoAprovacao()
    {
        var ordemServico = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), [CriarServico()]);
        ordemServico.AlterarParaEmDiagnostico();

        var statusField = typeof(OrdemServico).GetProperty("Status", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        statusField?.SetValue(ordemServico, StatusOrdemServico.AguardandoAprovacao);

        ordemServico.AlterarParaEmExecucao();

        Assert.Equal(StatusOrdemServico.EmExecucao, ordemServico.Status);
    }

    [Fact]
    public void OrdemServico_AlterarParaEmExecucao_DeveLancar_QuandoStatusForRecebida()
    {
        var ordemServico = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), [CriarServico()]);

        var action = () => ordemServico.AlterarParaEmExecucao();

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("Somente ordem de servico em diagnostico ou aguardando aprovacao pode ser alterada para em execucao.", exception.Message);
    }

    [Fact]
    public void OrdemServico_AlterarParaFinalizada_DeveAlterarStatus_QuandoStatusForEmExecucao()
    {
        var ordemServico = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), [CriarServico()]);
        ordemServico.AlterarParaEmDiagnostico();
        ordemServico.AlterarParaEmExecucao();

        ordemServico.AlterarParaFinalizada();

        Assert.Equal(StatusOrdemServico.Finalizada, ordemServico.Status);
    }

    [Fact]
    public void OrdemServico_AlterarParaFinalizada_DeveLancar_QuandoStatusForEmDiagnostico()
    {
        var ordemServico = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), [CriarServico()]);
        ordemServico.AlterarParaEmDiagnostico();

        var action = () => ordemServico.AlterarParaFinalizada();

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("Somente ordem de servico em execucao pode ser alterada para finalizada.", exception.Message);
    }

    [Fact]
    public void OrdemServico_AlterarParaEntregue_DeveAlterarStatus_QuandoStatusForFinalizada()
    {
        var ordemServico = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), [CriarServico()]);
        ordemServico.AlterarParaEmDiagnostico();
        ordemServico.AlterarParaEmExecucao();
        ordemServico.AlterarParaFinalizada();

        ordemServico.AlterarParaEntregue();

        Assert.Equal(StatusOrdemServico.Entregue, ordemServico.Status);
    }

    [Fact]
    public void OrdemServico_AlterarParaEntregue_DeveLancar_QuandoStatusForDiferente()
    {
        var ordemServico = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), [CriarServico()]);

        var action = () => ordemServico.AlterarParaEntregue();

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("Somente ordem de servico finalizada pode ser alterada para entregue.", exception.Message);
    }

    [Fact]
    public void OrdemServico_GerarOrcamento_DeveCalcularValoresTotaisEAlterarStatus()
    {
        var servicoA = CriarServicoComItens("Troca de oleo", (2, 15.50m), (1, 10m));
        var servicoB = CriarServicoComItens("Alinhamento", (1, 30m));
        var ordemServico = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), [servicoA, servicoB], new DateTime(2024, 01, 01, 8, 00, 00, DateTimeKind.Utc));

        ordemServico.GerarOrcamento(new DateTime(2024, 01, 01, 9, 00, 00, DateTimeKind.Utc));

        Assert.NotNull(ordemServico.Orcamento);
        Assert.Equal(71m, ordemServico.Orcamento!.ValorTotal);
        Assert.Equal(StatusOrdemServico.AguardandoAprovacao, ordemServico.Status);
        Assert.Contains(ordemServico.HistoricoStatus, h => h.Status == StatusOrdemServico.AguardandoAprovacao);
        Assert.Equal(41m, ordemServico.Orcamento.Servicos.Single(s => s.ServicoId == servicoA.Id).ValorTotal);
        Assert.Equal(30m, ordemServico.Orcamento.Servicos.Single(s => s.ServicoId == servicoB.Id).ValorTotal);
    }

    [Fact]
    public void OrdemServico_GerarOrcamento_DuasVezes_DeveManterSomenteUltimoResultado()
    {
        var servico = CriarServicoComItens("Troca", (1, 50m));
        var ordemServico = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), [servico]);

        ordemServico.GerarOrcamento(new DateTime(2024, 01, 01, 9, 00, 00, DateTimeKind.Utc));
        servico.ItensServico.Single().Insumo.AtualizarValorUnitario(80m);

        ordemServico.GerarOrcamento(new DateTime(2024, 01, 01, 10, 00, 00, DateTimeKind.Utc));

        Assert.NotNull(ordemServico.Orcamento);
        Assert.Equal(80m, ordemServico.Orcamento!.ValorTotal);
        Assert.Equal(2, ordemServico.HistoricoStatus.Count);
        Assert.Single(ordemServico.HistoricoStatus.Where(h => h.Status == StatusOrdemServico.AguardandoAprovacao));
    }

    [Fact]
    public void Orcamento_Criar_DeveLancarQuandoOrdemServicoIdForInvalido()
    {
        var servico = CriarServicoComItens("Troca", (1, 10m));

        var action = () => TechChallenge.Oficina.Entities.Features.Orcamentos.Orcamento.Criar(Guid.Empty, [servico]);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("A ordem de servico do orcamento e obrigatoria.", exception.Message);
    }

    [Fact]
    public void Orcamento_Criar_DeveLancarQuandoDataGeracaoForInvalida()
    {
        var servico = CriarServicoComItens("Troca", (1, 10m));

        var action = () => TechChallenge.Oficina.Entities.Features.Orcamentos.Orcamento.Criar(Guid.NewGuid(), [servico], default(DateTime));

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("A data de geracao do orcamento e obrigatoria.", exception.Message);
    }

    [Fact]
    public void Orcamento_Criar_DeveLancarQuandoNaoPossuirServicos()
    {
        var action = () => TechChallenge.Oficina.Entities.Features.Orcamentos.Orcamento.Criar(Guid.NewGuid(), []);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("A ordem de servico deve possuir ao menos um servico para gerar orcamento.", exception.Message);
    }

    [Fact]
    public void OrcamentoServico_Criar_DeveLancarQuandoServicoForNulo()
    {
        var action = () => TechChallenge.Oficina.Entities.Features.Orcamentos.OrcamentoServico.Criar(null!);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("O servico do orcamento e obrigatorio.", exception.Message);
    }

    [Fact]
    public void OrdemServico_AprovarOrcamento_DeveAlterarParaEmExecucao_QuandoAguardandoAprovacao()
    {
        var ordemServico = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), [CriarServico()]);
        ordemServico.GerarOrcamento();

        ordemServico.AprovarOrcamento();

        Assert.Equal(StatusOrdemServico.EmExecucao, ordemServico.Status);
    }

    [Fact]
    public void OrdemServico_AprovarOrcamento_DeveLancarQuandoStatusInvalido()
    {
        var ordemServico = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), [CriarServico()]);

        var action = () => ordemServico.AprovarOrcamento();

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("Somente ordem de servico aguardando aprovacao pode ser aprovada.", exception.Message);
    }
}
