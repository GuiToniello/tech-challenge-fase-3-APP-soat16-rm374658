using TechChallenge.Oficina.UseCases.Features.OrdensServico.Commands;

namespace TechChallenge.Oficina.Controllers.Features.OrdensServico
{
    public interface IOrdensServicoController
    {
        Task<object> Post(CriarOrdemServicoCommand command, CancellationToken cancellationToken);

        Task<object> PostCompleta(AbrirOrdemServicoCompletaCommand command, CancellationToken cancellationToken);

        Task<object> GetById(Guid id, CancellationToken cancellationToken);

        Task<object> Get(CancellationToken cancellationToken);

        Task<object> GetOrdenadas(CancellationToken cancellationToken);

        Task<object> GetAcompanhamento(Guid id, CancellationToken cancellationToken);

        Task<object> GetByCliente(Guid clienteId, CancellationToken cancellationToken);

        Task<object> Put(AtualizarOrdemServicoCommand command, CancellationToken cancellationToken);

        Task<object> Delete(Guid id, CancellationToken cancellationToken);

        Task<object> AlterarParaEmDiagnostico(Guid id, CancellationToken cancellationToken);

        Task<object> AlterarParaEmExecucao(Guid id, CancellationToken cancellationToken);

        Task<object> AlterarParaFinalizada(Guid id, CancellationToken cancellationToken);

        Task<object> AlterarParaEntregue(Guid id, CancellationToken cancellationToken);

        Task<object> GerarOrcamento(Guid id, CancellationToken cancellationToken);

        Task<object> EnviarOrcamento(Guid id, CancellationToken cancellationToken);

        Task<object> AprovarOrcamento(Guid id, CancellationToken cancellationToken);

        Task<object> RecusarOrcamento(Guid id, CancellationToken cancellationToken);
    }
}
