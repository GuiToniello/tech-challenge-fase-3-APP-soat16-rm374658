using AutoMapper;
using TechChallenge.Oficina.UseCases.Features.Veiculos.Commands;
using TechChallenge.Oficina.UseCases.Features.Veiculos.Queries;
using TechChallenge.Oficina.UseCases.Features.Veiculos.ViewModels;
using TechChallenge.Oficina.Entities.Exceptions;
using TechChallenge.Oficina.Entities.Features.Veiculos;
using TechChallenge.Oficina.Entities.Features.Veiculos.VOs;
using TechChallenge.Oficina.UseCases.Features.Clientes.UseCases;

namespace TechChallenge.Oficina.UseCases.Features.Veiculos.UseCases;

public sealed class VeiculoUseCases : IVeiculoUseCases
{
    private readonly IMapper _mapper;
    private readonly IVeiculoGateway _veiculoGateway;
    private readonly IClienteGateway _clienteGateway;

    public VeiculoUseCases(IMapper mapper, IVeiculoGateway veiculoGateway, IClienteGateway clienteGateway)
    {
        _mapper = mapper;
        _veiculoGateway = veiculoGateway;
        _clienteGateway = clienteGateway;
    }

    public async Task<VeiculoViewModel> CriarAsync(CriarVeiculoCommand command, CancellationToken cancellationToken = default)
    {
        await ValidarClienteExistenteAsync(command.ClienteId, cancellationToken);
        await ValidarDuplicidadePlacaAsync(command.Placa, null, cancellationToken);

        var veiculo = Veiculo.Criar(command.Placa, command.Marca, command.Modelo, command.Ano, command.Renavam, command.ClienteId);

        await _veiculoGateway.AdicionarAsync(veiculo, cancellationToken);

        return _mapper.Map<VeiculoViewModel>(veiculo);
    }

    public async Task<VeiculoViewModel> AtualizarAsync(AtualizarVeiculoCommand command, CancellationToken cancellationToken = default)
    {
        var veiculo = await ObterVeiculoExistenteAsync(command.Id, cancellationToken);

        await ValidarClienteExistenteAsync(command.ClienteId, cancellationToken);
        await ValidarDuplicidadePlacaAsync(command.Placa, veiculo.Id, cancellationToken);

        veiculo.AtualizarPlaca(command.Placa);
        veiculo.AtualizarMarca(command.Marca);
        veiculo.AtualizarModelo(command.Modelo);
        veiculo.AtualizarAno(command.Ano);
        veiculo.AtualizarRenavam(command.Renavam);
        veiculo.AtualizarClienteId(command.ClienteId);

        await _veiculoGateway.AtualizarAsync(veiculo, cancellationToken);

        return _mapper.Map<VeiculoViewModel>(veiculo);
    }

    public async Task<VeiculoViewModel> ObterPorIdAsync(ObterVeiculoPorIdQuery query, CancellationToken cancellationToken = default)
    {
        var veiculo = await ObterVeiculoExistenteAsync(query.Id, cancellationToken);
        return _mapper.Map<VeiculoViewModel>(veiculo);
    }

    public async Task<IReadOnlyCollection<VeiculoViewModel>> ListarAsync(ListarVeiculosQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Veiculo> veiculos;

        if (query.ClienteId.HasValue)
        {
            veiculos = await _veiculoGateway.ListarPorClienteAsync(query.ClienteId.Value, cancellationToken);
        }
        else
        {
            veiculos = await _veiculoGateway.ListarAsync(cancellationToken);
        }

        return _mapper.Map<IReadOnlyCollection<VeiculoViewModel>>(veiculos);
    }

    public async Task ExcluirAsync(ExcluirVeiculoCommand command, CancellationToken cancellationToken = default)
    {
        var veiculo = await ObterVeiculoExistenteAsync(command.Id, cancellationToken);
        await _veiculoGateway.RemoverAsync(veiculo, cancellationToken);
    }

    private async Task<Veiculo> ObterVeiculoExistenteAsync(Guid id, CancellationToken cancellationToken)
    {
        var veiculo = await _veiculoGateway.ObterPorIdAsync(id, cancellationToken);

        if (veiculo is null)
        {
            throw new KeyNotFoundException("Veículo não encontrado.");
        }

        return veiculo;
    }

    private async Task ValidarClienteExistenteAsync(Guid clienteId, CancellationToken cancellationToken)
    {
        var cliente = await _clienteGateway.ObterPorIdAsync(clienteId, cancellationToken);

        if (cliente is null)
        {
            throw new KeyNotFoundException("Cliente não encontrado.");
        }
    }

    private async Task ValidarDuplicidadePlacaAsync(string placa, Guid? veiculoId, CancellationToken cancellationToken)
    {
        var placaNormalizada = PlacaMercosul.Criar(placa).Valor;

        if (await _veiculoGateway.ExisteComPlacaAsync(placaNormalizada, veiculoId, cancellationToken))
        {
            throw new DomainException("Já existe um veículo cadastrado com a placa informada.");
        }
    }
}
