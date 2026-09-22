# ADR-007: Entity Framework Core com PostgreSQL

**Status**: Aceita

**Contexto**:
Necessidade de persistência relacional com suporte a migrations e LINQ queries, usando PostgreSQL como banco de dados.

**Decisão**:
- EF Core 10.x com `Npgsql.EntityFrameworkCore.PostgreSQL`.
- DbContext centralizado no projeto `TechChallenge.Oficina.DB` em `Context/OficinaDbContext`.
- Configurações de entidades via `IEntityTypeConfiguration<T>` (em `Configurations/`).
- Connection string configurável via `appsettings.json` sob a seção `DatabaseSettings`.
- Migrations versionadas com timestamp (ex: `20260603151745_InitialMigration.cs`).

**Consequências**:
- ✅ Linguagem LINQ type-safe.
- ✅ Migrations automáticas rastreiam evolução do schema.
- ✅ Suporte nativo a compostos (owned types) como `IdentificacaoCliente`.
- ✅ Performance otimizável com índices e queries.

**Configuração**:
```csharp
// appsettings.json
{
  "DatabaseSettings": {
    "ConnectionString": "Host=localhost;Port=5432;Database=oficina;Username=postgres;Password=postgres"
  }
}
```
