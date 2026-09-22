# ADR-004: Gateways e Persistência

**Status**: Aceita (atualizada)

**Contexto**:
Com a adoção de Clean Architecture no projeto, a abstração de acesso a dados deixou de ser tratada como "Repository" no núcleo e passou a seguir o conceito de **porta de saída (Gateway)** dos casos de uso.

Na implementação atual:
- os contratos de persistência ficam no projeto **Application (UseCases)** (`TechChallenge.Oficina.UseCases.csproj`), como `IClienteGateway`, `IOrdemServicoGateway`, `IServicoGateway`, `IInsumoGateway`, `IVeiculoGateway` e `IIndicadorGateway`;
- as implementações concretas ficam no projeto **DB** (`TechChallenge.Oficina.DB.csproj`), como `ClienteGateway`, `OrdemServicoGateway`, `ServicoGateway`, etc;
- o vínculo contrato/implementação é feito por DI no módulo `AddInfraData`.

**Decisão**:
- Padronizar a nomenclatura e o papel como **porta de saída (Gateway)** para persistência.
- Manter contratos de portas de saída (Gateways) no projeto **Application (UseCases)**, alinhados aos casos de uso.
- Manter implementações de portas de saída (Gateways) no projeto **DB** usando EF Core.
- Métodos de escrita (`AdicionarAsync`, `AtualizarAsync`, `RemoverAsync`) persistem alterações com `SaveChangesAsync()`.
- Métodos de leitura retornam entidades e coleções materializadas (`IReadOnlyCollection<>`), evitando expor `IQueryable` para fora da infraestrutura.

**Consequências**:
- A nomenclatura fica consistente com o modelo de portas e adaptadores da Clean Architecture.
- Casos de uso permanecem desacoplados de tecnologia de persistência.
- Testes da camada Application (UseCases) continuam simples via mocks de `I*Gateway`.
- A infraestrutura mantém responsabilidade explícita por consulta e persistência.

**Exemplo de assinatura (atual)**:
```csharp
public interface IClienteGateway
{
    Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Cliente cliente, CancellationToken cancellationToken = default);
    Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Cliente>> ListarAsync(CancellationToken cancellationToken = default);
    Task<bool> ExisteComIdentificacaoAsync(string identificacaoNormalizada, Guid? ignorarClienteId = null, CancellationToken cancellationToken = default);
    Task RemoverAsync(Cliente cliente, CancellationToken cancellationToken = default);
}
```
