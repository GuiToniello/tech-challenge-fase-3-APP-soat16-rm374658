using AutoMapper;
using TechChallenge.Oficina.UseCases.Features.Veiculos.ViewModels;
using TechChallenge.Oficina.Entities.Features.Veiculos;

namespace TechChallenge.Oficina.UseCases.Features.Veiculos.Mappings;

public sealed class VeiculoProfile : Profile
{
    public VeiculoProfile()
    {
        CreateMap<Veiculo, VeiculoViewModel>()
            .ForMember(destino => destino.Placa, origem => origem.MapFrom(veiculo => veiculo.Placa.Valor));
    }
}
