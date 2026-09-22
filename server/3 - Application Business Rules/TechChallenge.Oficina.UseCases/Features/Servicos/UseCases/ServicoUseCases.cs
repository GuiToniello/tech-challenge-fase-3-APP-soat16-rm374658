using AutoMapper;
using TechChallenge.Oficina.UseCases.Features.Servicos.Commands;
using TechChallenge.Oficina.UseCases.Features.Servicos.Queries;
using TechChallenge.Oficina.UseCases.Features.Servicos.ViewModels;
using TechChallenge.Oficina.Entities.Exceptions;
using TechChallenge.Oficina.Entities.Features.Servicos;
using TechChallenge.Oficina.UseCases.Features.Insumos.UseCases;

namespace TechChallenge.Oficina.UseCases.Features.Servicos.UseCases;

public sealed class ServicoUseCases : IServicoUseCases
{
    private readonly IMapper _mapper;
    private readonly IServicoGateway _servicoGateway;
    private readonly IInsumoGateway _insumoGateway;

    public ServicoUseCases(IMapper mapper, IServicoGateway servicoGateway, IInsumoGateway insumoGateway)
    {
        _mapper = mapper;
        _servicoGateway = servicoGateway;
        _insumoGateway = insumoGateway;
    }

    public async Task<ServicoViewModel> CriarAsync(CriarServicoCommand command, CancellationToken cancellationToken = default)
    {
        var itensServico = await ObterItensServicoAsync(command.ItensServico, cancellationToken);
        var servico = Servico.Criar(command.Nome, command.Descricao, itensServico);

        await _servicoGateway.AdicionarAsync(servico, cancellationToken);

        return _mapper.Map<ServicoViewModel>(servico);
    }

    public async Task<ServicoViewModel> AtualizarAsync(AtualizarServicoCommand command, CancellationToken cancellationToken = default)
    {
        var servico = await ObterServicoExistenteAsync(command.Id, cancellationToken);
        var itensServico = await ObterItensServicoAsync(command.ItensServico, cancellationToken);

        servico.AtualizarNome(command.Nome);
        servico.AtualizarDescricao(command.Descricao);
        servico.DefinirItensServico(itensServico);

        await _servicoGateway.AtualizarAsync(servico, cancellationToken);

        return _mapper.Map<ServicoViewModel>(servico);
    }

    public async Task<ServicoViewModel> ObterPorIdAsync(ObterServicoPorIdQuery query, CancellationToken cancellationToken = default)
    {
        var servico = await ObterServicoExistenteAsync(query.Id, cancellationToken);
        return _mapper.Map<ServicoViewModel>(servico);
    }

    public async Task<IReadOnlyCollection<ServicoViewModel>> ListarAsync(ListarServicosQuery query, CancellationToken cancellationToken = default)
    {
        var servicos = await _servicoGateway.ListarAsync(cancellationToken);
        return _mapper.Map<IReadOnlyCollection<ServicoViewModel>>(servicos);
    }

    public async Task ExcluirAsync(ExcluirServicoCommand command, CancellationToken cancellationToken = default)
    {
        var servico = await ObterServicoExistenteAsync(command.Id, cancellationToken);
        await _servicoGateway.RemoverAsync(servico, cancellationToken);
    }

    private async Task<Servico> ObterServicoExistenteAsync(Guid id, CancellationToken cancellationToken)
    {
        var servico = await _servicoGateway.ObterPorIdAsync(id, cancellationToken);

        if (servico is null)
        {
            throw new KeyNotFoundException("Servico nao encontrado.");
        }

        return servico;
    }

    private async Task<IReadOnlyCollection<ItemServico>> ObterItensServicoAsync(IReadOnlyCollection<ItemServicoCommand>? itensServicoCommand, CancellationToken cancellationToken)
    {
        if (itensServicoCommand is null || itensServicoCommand.Count == 0)
        {
            return [];
        }

        if (itensServicoCommand.Any(item => item is null))
        {
            throw new DomainException("Todos os itens de servico informados devem ser validos.");
        }

        if (itensServicoCommand.Any(item => item.InsumoId == Guid.Empty))
        {
            throw new DomainException("Todos os insumos informados devem possuir identificador valido.");
        }

        if (itensServicoCommand.Any(item => item.Quantidade <= 0))
        {
            throw new DomainException("Todos os itens de servico devem possuir quantidade maior que zero.");
        }

        var itensAgrupados = itensServicoCommand
            .GroupBy(item => item.InsumoId)
            .Select(group => new { InsumoId = group.Key, Quantidade = group.Sum(item => item.Quantidade) })
            .ToArray();

        var itensServico = new List<ItemServico>(itensAgrupados.Length);

        foreach (var item in itensAgrupados)
        {
            var insumo = await _insumoGateway.ObterPorIdAsync(item.InsumoId, cancellationToken);

            if (insumo is null)
            {
                throw new KeyNotFoundException("Insumo nao encontrado.");
            }

            itensServico.Add(ItemServico.Criar(insumo, item.Quantidade));
        }

        return itensServico;
    }
}
