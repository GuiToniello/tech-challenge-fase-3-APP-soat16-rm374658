using TechChallenge.Oficina.Entities.Features.OrdensServico;
using TechChallenge.Oficina.Entities.Features.OrdensServico.Enums;

namespace TechChallenge.Oficina.UseCases.Features.OrdensServico.UseCases;

public interface IOrdemServicoStatusEmailSender
{
    Task EnviarStatusAlteradoAsync(
        OrdemServico ordemServico,
        string emailDestino,
        StatusOrdemServico novoStatus,
        CancellationToken cancellationToken = default);
}
