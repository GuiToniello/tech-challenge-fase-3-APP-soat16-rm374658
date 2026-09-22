using AutoMapper;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.Entities.Features.Orcamentos;
using TechChallenge.Oficina.Entities.Features.OrdensServico;
using TechChallenge.Oficina.Entities.Features.OrdensServico.Enums;
using TechChallenge.Oficina.Entities.Features.OrdensServico.VOs;
using TechChallenge.Oficina.Entities.Features.Servicos;

namespace TechChallenge.Oficina.UseCases.Features.OrdensServico.Mappings;

public sealed class OrdemServicoProfile : Profile
{
    public OrdemServicoProfile()
    {
        CreateMap<Servico, ServicoResumoOrdemServicoViewModel>();
        CreateMap<HistoricoStatusOrdemServico, HistoricoStatusOrdemServicoViewModel>();
        CreateMap<OrcamentoServico, OrcamentoServicoViewModel>();
        CreateMap<Orcamento, OrcamentoViewModel>();

        CreateMap<OrdemServico, AcompanhamentoOrdemServicoViewModel>()
            .ForMember(destino => destino.Status, configuracao => configuracao.MapFrom(origem => (int)origem.Status))
            .ForMember(destino => destino.StatusDescricao, configuracao => configuracao.MapFrom(origem => origem.Status.ObterDescricao()))
            .ForMember(destino => destino.HistoricoStatus, configuracao => configuracao.MapFrom(origem => origem.HistoricoStatus));

        CreateMap<OrdemServico, OrdemServicoViewModel>()
            .ForMember(destino => destino.Status, configuracao => configuracao.MapFrom(origem => (int)origem.Status))
            .ForMember(destino => destino.StatusDescricao, configuracao => configuracao.MapFrom(origem => origem.Status.ObterDescricao()));

        CreateMap<OrdemServico, OrdemServicoOrdenadasViewModel>()
            .ForMember(destino => destino.Status, configuracao => configuracao.MapFrom(origem => (int)origem.Status))
            .ForMember(destino => destino.StatusDescricao, configuracao => configuracao.MapFrom(origem => origem.Status.ObterDescricao()))
            .ForMember(destino => destino.DataAlteracao, configuracao => configuracao.MapFrom(origem =>
                origem.HistoricoStatus.FirstOrDefault() != null
                    ? origem.HistoricoStatus.OrderBy(h => h.DataAlteracao).First().DataAlteracao
                    : default(DateTime)));
    }
}
