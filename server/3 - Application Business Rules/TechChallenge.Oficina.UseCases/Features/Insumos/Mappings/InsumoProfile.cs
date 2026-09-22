using AutoMapper;
using TechChallenge.Oficina.UseCases.Features.Insumos.ViewModels;
using TechChallenge.Oficina.Entities.Features.Insumos;

namespace TechChallenge.Oficina.UseCases.Features.Insumos.Mappings;

public sealed class InsumoProfile : Profile
{
    public InsumoProfile()
    {
        CreateMap<Insumo, InsumoViewModel>();
    }
}
