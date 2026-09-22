# ADR-009: Migrações Automáticas no Startup

**Status**: Aceita

**Contexto**:
Necessidade de garantir schema do banco atualizado ao iniciar a aplicação, sem intervenção manual.

**Decisão**:
- Método de extensão `ApplyMigrations()` no projeto `TechChallenge.Oficina.DB` (arquivo `Extensions.cs`).
- Chamado no `Program.cs` antes de iniciar o pipeline da API:
  ```csharp
  var app = builder.Build();
  app.Services.ApplyMigrations(); // Executa migrations pendentes
  app.UseSwagger();
  // ...
  app.Run();
  ```
- Usa `dbContext.Database.Migrate()` do EF Core.
- **Comportamento**:
  - Se nenhuma migration foi aplicada, cria tabelas.
  - Se migrations pendentes existem, aplica.
  - Se banco já está atualizado, não faz nada.

**Consequências**:
- ✅ Zero-config deployment (sem scripts SQL manual).
- ✅ Cria banco automaticamente se não existir.
- ✅ Garante consistência em múltiplos ambientes.
- ⚠ Em produção, recomenda-se validação prévia (dry-run).
