using AutoMapper;
using TechChallenge.Oficina.UseCases.Features.Servicos.ViewModels;
using TechChallenge.Oficina.Entities.Features.Insumos;
using TechChallenge.Oficina.Entities.Features.Servicos;

namespace TechChallenge.Oficina.UseCases.Features.Servicos.Mappings;

public sealed class ServicoProfile : Profile
{
    public ServicoProfile()
    {
        CreateMap<Insumo, InsumoResumoViewModel>();
        CreateMap<Servico, ServicoViewModel>();
        CreateMap<ItemServico, ItemServicoViewModel>()
            .ForMember(dest => dest.InsumoNome, opt => opt.MapFrom(src => src.Insumo.Nome));
    }
}
