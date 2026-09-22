using TechChallenge.Oficina.Entities.Features.OrdensServico;

namespace TechChallenge.Oficina.UseCases.Features.OrdensServico.UseCases;

public interface IOrcamentoEmailSender
{
    Task EnviarOrcamentoAsync(OrdemServico ordemServico, string emailDestino, CancellationToken cancellationToken = default);
}
