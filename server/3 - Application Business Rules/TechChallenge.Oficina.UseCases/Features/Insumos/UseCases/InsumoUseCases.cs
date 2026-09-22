using AutoMapper;
using TechChallenge.Oficina.UseCases.Features.Insumos.Commands;
using TechChallenge.Oficina.UseCases.Features.Insumos.Queries;
using TechChallenge.Oficina.UseCases.Features.Insumos.ViewModels;
using TechChallenge.Oficina.Entities.Features.Insumos;

namespace TechChallenge.Oficina.UseCases.Features.Insumos.UseCases;

public sealed class InsumoUseCases : IInsumoUseCases
{
    private readonly IMapper _mapper;
    private readonly IInsumoGateway _insumoGateway;

    public InsumoUseCases(IMapper mapper, IInsumoGateway insumoGateway)
    {
        _mapper = mapper;
        _insumoGateway = insumoGateway;
    }

    public async Task<InsumoViewModel> CriarAsync(CriarInsumoCommand command, CancellationToken cancellationToken = default)
    {
        var insumo = Insumo.Criar(command.Nome, command.Fabricante, command.QuantidadeDisponivel, command.ValorUnitario);

        await _insumoGateway.AdicionarAsync(insumo, cancellationToken);

        return _mapper.Map<InsumoViewModel>(insumo);
    }

    public async Task<InsumoViewModel> AtualizarAsync(AtualizarInsumoCommand command, CancellationToken cancellationToken = default)
    {
        var insumo = await ObterInsumoExistenteAsync(command.Id, cancellationToken);

        insumo.AtualizarNome(command.Nome);
        insumo.AtualizarFabricante(command.Fabricante);
        insumo.AtualizarQuantidadeDisponivel(command.QuantidadeDisponivel);
        insumo.AtualizarValorUnitario(command.ValorUnitario);

        await _insumoGateway.AtualizarAsync(insumo, cancellationToken);

        return _mapper.Map<InsumoViewModel>(insumo);
    }

    public async Task<InsumoViewModel> ObterPorIdAsync(ObterInsumoPorIdQuery query, CancellationToken cancellationToken = default)
    {
        var insumo = await ObterInsumoExistenteAsync(query.Id, cancellationToken);
        return _mapper.Map<InsumoViewModel>(insumo);
    }

    public async Task<IReadOnlyCollection<InsumoViewModel>> ListarAsync(ListarInsumosQuery query, CancellationToken cancellationToken = default)
    {
        var insumos = await _insumoGateway.ListarAsync(cancellationToken);
        return _mapper.Map<IReadOnlyCollection<InsumoViewModel>>(insumos);
    }

    public async Task ExcluirAsync(ExcluirInsumoCommand command, CancellationToken cancellationToken = default)
    {
        var insumo = await ObterInsumoExistenteAsync(command.Id, cancellationToken);
        await _insumoGateway.RemoverAsync(insumo, cancellationToken);
    }

    private async Task<Insumo> ObterInsumoExistenteAsync(Guid id, CancellationToken cancellationToken)
    {
        var insumo = await _insumoGateway.ObterPorIdAsync(id, cancellationToken);

        if (insumo is null)
        {
            throw new KeyNotFoundException("Insumo não encontrado.");
        }

        return insumo;
    }
}
