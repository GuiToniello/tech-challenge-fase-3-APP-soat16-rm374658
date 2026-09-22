using TechChallenge.Oficina.UseCases.Features.Clientes.Commands;
using TechChallenge.Oficina.UseCases.Features.Clientes.UseCases;
using TechChallenge.Oficina.UseCases.Features.Insumos.Commands;
using TechChallenge.Oficina.UseCases.Features.Insumos.UseCases;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.Commands;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.Queries;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.UseCases;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.UseCases.Features.Servicos.Commands;
using TechChallenge.Oficina.UseCases.Features.Servicos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Veiculos.Commands;
using TechChallenge.Oficina.UseCases.Features.Veiculos.UseCases;
using TechChallenge.Oficina.Entities.Exceptions;

namespace TechChallenge.Oficina.Controllers.Features.OrdensServico
{
    // Esta classe atua como facade de entrada para orquestrar a abertura completa da ordem de servico em um unico fluxo.
    public class OrdensServicoController : IOrdensServicoController
    {
        private readonly IClienteUseCases _clienteUseCases;
        private readonly IVeiculoUseCases _veiculoUseCases;
        private readonly IInsumoUseCases _insumoUseCases;
        private readonly IServicoUseCases _servicoUseCases;
        private readonly IOrdemServicoUseCases _ordemServicoService;
        private readonly IOrdensServicoAdapter _ordensServicoAdapter;

        public OrdensServicoController(
            IClienteUseCases clienteUseCases,
            IVeiculoUseCases veiculoUseCases,
            IInsumoUseCases insumoUseCases,
            IServicoUseCases servicoUseCases,
            IOrdemServicoUseCases ordemServicoUseCases,
            IOrdensServicoAdapter ordensServicoAdapter)
        {
            _clienteUseCases = clienteUseCases;
            _veiculoUseCases = veiculoUseCases;
            _insumoUseCases = insumoUseCases;
            _servicoUseCases = servicoUseCases;
            _ordemServicoService = ordemServicoUseCases;
            _ordensServicoAdapter = ordensServicoAdapter;
        }

        public async Task<object> Post(CriarOrdemServicoCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var ordemServico = await _ordemServicoService.CriarAsync(command, cancellationToken);
                var result = OrdensServicoResult.From(ordemServico);

                return _ordensServicoAdapter.Adapt(result, true);
            }
            catch (DomainException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);

                return _ordensServicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);

                return _ordensServicoAdapter.Adapt(result);
            }
        }

        public async Task<object> PostCompleta(AbrirOrdemServicoCompletaCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var cliente = await _clienteUseCases.CriarAsync(
                    new CriarClienteCommand
                    {
                        NomeCompleto = command.Cliente.NomeCompleto,
                        Identificacao = command.Cliente.Identificacao,
                        Email = command.Cliente.Email
                    },
                    cancellationToken);

                var veiculo = await _veiculoUseCases.CriarAsync(
                    new CriarVeiculoCommand
                    {
                        Placa = command.Veiculo.Placa,
                        Marca = command.Veiculo.Marca,
                        Modelo = command.Veiculo.Modelo,
                        Ano = command.Veiculo.Ano,
                        Renavam = command.Veiculo.Renavam,
                        ClienteId = cliente.Id
                    },
                    cancellationToken);

                var servicoIds = new List<Guid>(command.Servicos.Count);

                foreach (var servicoCommand in command.Servicos)
                {
                    var itensServico = new List<ItemServicoCommand>(servicoCommand.ItensServico.Count);

                    foreach (var itemCommand in servicoCommand.ItensServico)
                    {
                        var insumo = await _insumoUseCases.CriarAsync(
                            new CriarInsumoCommand
                            {
                                Nome = itemCommand.Insumo.Nome,
                                Fabricante = itemCommand.Insumo.Fabricante,
                                QuantidadeDisponivel = itemCommand.Insumo.QuantidadeDisponivel,
                                ValorUnitario = itemCommand.Insumo.ValorUnitario
                            },
                            cancellationToken);

                        itensServico.Add(new ItemServicoCommand
                        {
                            InsumoId = insumo.Id,
                            Quantidade = itemCommand.Quantidade
                        });
                    }

                    var servico = await _servicoUseCases.CriarAsync(
                        new CriarServicoCommand
                        {
                            Nome = servicoCommand.Nome,
                            Descricao = servicoCommand.Descricao,
                            ItensServico = itensServico
                        },
                        cancellationToken);

                    servicoIds.Add(servico.Id);
                }

                var ordemServico = await _ordemServicoService.CriarAsync(
                    new CriarOrdemServicoCommand
                    {
                        ClienteId = cliente.Id,
                        VeiculoId = veiculo.Id,
                        ServicoIds = servicoIds
                    },
                    cancellationToken);

                var abertura = new AberturaOrdemServicoViewModel
                {
                    OrdemServicoId = ordemServico.Id
                };
                var result = OrdensServicoResult.From(abertura);

                return _ordensServicoAdapter.Adapt(result, true);
            }
            catch (DomainException exception)
            {
                var result = OrdensServicoResult.FromError<AberturaOrdemServicoViewModel>(exception);

                return _ordensServicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = OrdensServicoResult.FromError<AberturaOrdemServicoViewModel>(exception);

                return _ordensServicoAdapter.Adapt(result);
            }
        }

        public async Task<object> GetById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var query = new ObterOrdemServicoPorIdQuery { Id = id };
                var ordemServico = await _ordemServicoService.ObterPorIdAsync(query, cancellationToken);
                var result = OrdensServicoResult.From(ordemServico);

                return _ordensServicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
        }

        public async Task<object> Get(CancellationToken cancellationToken)
        {
            var ordensServico = await _ordemServicoService.ListarAsync(new ListarOrdensServicoQuery(), cancellationToken);
            var result = OrdensServicoResult.From(ordensServico);

            return _ordensServicoAdapter.Adapt(result);
        }

        public async Task<object> GetOrdenadas(CancellationToken cancellationToken)
        {
            var ordensServico = await _ordemServicoService.ListarOrdenadasAsync(new ListarOrdensServicoOrdenadasQuery(), cancellationToken);
            var result = OrdensServicoResult.From(ordensServico);

            return _ordensServicoAdapter.Adapt(result);
        }

        public async Task<object> GetAcompanhamento(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var acompanhamento = await _ordemServicoService.ObterAcompanhamentoAsync(new ObterAcompanhamentoOrdemServicoPorIdQuery { Id = id }, cancellationToken);
                var result = OrdensServicoResult.From(acompanhamento);

                return _ordensServicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = OrdensServicoResult.FromError<AcompanhamentoOrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
        }

        public async Task<object> GetByCliente(Guid clienteId, CancellationToken cancellationToken)
        {
            try
            {
                var acompanhamentos = await _ordemServicoService.ListarPorClienteAsync(new ListarOrdensServicoPorClienteQuery { ClienteId = clienteId }, cancellationToken);
                var result = OrdensServicoResult.From(acompanhamentos);

                return _ordensServicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = OrdensServicoResult.FromError<IReadOnlyCollection<AcompanhamentoOrdemServicoViewModel>>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
        }

        public async Task<object> Put(AtualizarOrdemServicoCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var ordemServico = await _ordemServicoService.AtualizarAsync(command, cancellationToken);
                var result = OrdensServicoResult.From(ordemServico);

                return _ordensServicoAdapter.Adapt(result);
            }
            catch (DomainException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);

                return _ordensServicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);

                return _ordensServicoAdapter.Adapt(result);
            }
        }

        public async Task<object> Delete(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new ExcluirOrdemServicoCommand { Id = id };
                await _ordemServicoService.ExcluirAsync(command, cancellationToken);

                return _ordensServicoAdapter.Empty();
            }
            catch (KeyNotFoundException exception)
            {
                var result = OrdensServicoResult.FromError<bool>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
        }

        public async Task<object> AlterarParaEmDiagnostico(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var ordemServico = await _ordemServicoService.AlterarStatusParaEmDiagnosticoAsync(new AlterarStatusOrdemServicoCommand { Id = id }, cancellationToken);
                var result = OrdensServicoResult.From(ordemServico);

                return _ordensServicoAdapter.Adapt(result);
            }
            catch (DomainException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
        }

        public async Task<object> AlterarParaEmExecucao(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var ordemServico = await _ordemServicoService.AlterarStatusParaEmExecucaoAsync(new AlterarStatusOrdemServicoCommand { Id = id }, cancellationToken);
                var result = OrdensServicoResult.From(ordemServico);

                return _ordensServicoAdapter.Adapt(result);
            }
            catch (DomainException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
        }

        public async Task<object> AlterarParaFinalizada(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var ordemServico = await _ordemServicoService.AlterarStatusParaFinalizadaAsync(new AlterarStatusOrdemServicoCommand { Id = id }, cancellationToken);
                var result = OrdensServicoResult.From(ordemServico);

                return _ordensServicoAdapter.Adapt(result);
            }
            catch (DomainException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
        }

        public async Task<object> AlterarParaEntregue(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var ordemServico = await _ordemServicoService.AlterarStatusParaEntregueAsync(new AlterarStatusOrdemServicoCommand { Id = id }, cancellationToken);
                var result = OrdensServicoResult.From(ordemServico);

                return _ordensServicoAdapter.Adapt(result);
            }
            catch (DomainException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
        }

        public async Task<object> GerarOrcamento(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var ordemServico = await _ordemServicoService.GerarOrcamentoAsync(new AlterarStatusOrdemServicoCommand { Id = id }, cancellationToken);
                var result = OrdensServicoResult.From(ordemServico);

                return _ordensServicoAdapter.Adapt(result);
            }
            catch (DomainException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
        }

        public async Task<object> EnviarOrcamento(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                await _ordemServicoService.EnviarOrcamentoPorEmailAsync(new AlterarStatusOrdemServicoCommand { Id = id }, cancellationToken);

                return _ordensServicoAdapter.Empty();
            }
            catch (DomainException exception)
            {
                var result = OrdensServicoResult.FromError<bool>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = OrdensServicoResult.FromError<bool>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
        }

        public async Task<object> AprovarOrcamento(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var ordemServico = await _ordemServicoService.AprovarOrcamentoAsync(new AlterarStatusOrdemServicoCommand { Id = id }, cancellationToken);
                var result = OrdensServicoResult.From(ordemServico);

                return _ordensServicoAdapter.Adapt(result);
            }
            catch (DomainException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
        }

        public async Task<object> RecusarOrcamento(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var ordemServico = await _ordemServicoService.RecusarOrcamentoAsync(new AlterarStatusOrdemServicoCommand { Id = id }, cancellationToken);
                var result = OrdensServicoResult.From(ordemServico);

                return _ordensServicoAdapter.Adapt(result);
            }
            catch (DomainException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
        }
    }
}
