using AutoMapper;
using TechChallenge.Oficina.UseCases.Features.Clientes.ViewModels;
using TechChallenge.Oficina.Entities.Features.Clientes;

namespace TechChallenge.Oficina.UseCases.Features.Clientes.Mappings;

public sealed class ClienteProfile : Profile
{
    public ClienteProfile()
    {
        CreateMap<Cliente, ClienteViewModel>()
            .ForMember(destino => destino.Identificacao, origem => origem.MapFrom(cliente => cliente.Identificacao.Valor))
            .ForMember(destino => destino.TipoIdentificacao, origem => origem.MapFrom(cliente => cliente.Identificacao.Tipo.ToString()))
            .ForMember(destino => destino.Email, origem => origem.MapFrom(cliente => cliente.Email));
    }
}
