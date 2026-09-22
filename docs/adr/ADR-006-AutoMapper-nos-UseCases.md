# ADR-006: Mapeamento com AutoMapper na Application (UseCases)

**Status**: Aceita

**Contexto**:
Necessidade de transformar entidades do domínio em DTOs de resposta, evitando que adaptadores de interface (controllers da arquitetura) façam mapeamento manual e mantendo lógica centralizada.

**Decisão**:
- A responsabilidade de mapeamento entre entidades de domínio e DTOs/ViewModels é da camada **Application (UseCases)**.
- O registro de mapeamento é centralizado no módulo de DI da Application (`AddApplication`), que concentra os profiles usados pelos casos de uso.
- Profiles de mapeamento ficam em `Application/Features/{Feature}/Mappings/`.
- O `ClienteUseCases` (Application) faz o mapeamento antes de retornar ao adaptador de interface.
- Controllers da arquitetura nunca instanciam `IMapper`; sempre consomem ViewModels já mapeadas.
- Mapeamento complexo (ex: `Identificacao.Valor` para flat property) é feito no Profile.
- Eventual referência de pacote AutoMapper em projeto externo (ex.: API) não muda a regra arquitetural: o mapeamento de negócio permanece na Application e fora dos adaptadores de interface.

**Consequências**:
- ✅ Adaptadores de interface sem lógica de transformação.
- ✅ Mapeamento centralizado e reutilizável.
- ✅ Fácil testar profiles isoladamente.
- ✅ API desacoplada de detalhes internos do domínio.

**Exemplo**:
```csharp
// ClienteProfile.cs (Application)
CreateMap<Cliente, ClienteViewModel>()
    .ForMember(destino => destino.Identificacao, 
               origem => origem.MapFrom(cliente => cliente.Identificacao.Valor))
    .ForMember(destino => destino.TipoIdentificacao, 
               origem => origem.MapFrom(cliente => cliente.Identificacao.Tipo.ToString()));
```
