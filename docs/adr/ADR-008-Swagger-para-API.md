# ADR-008: Swagger para Documentação de API

**Status**: Aceita

**Contexto**:
Necessidade de documentar endpoints de API automaticamente, permitindo testes interativos (Try it out) e facilitando integração com cliente frontend.

**Decisão**:
- Swagger/OpenAPI via `Swashbuckle.AspNetCore` (v6.6.2).
- Swagger UI disponível na raiz: `http://localhost:5154/`.
- Configuração em `Program.cs`:
  ```csharp
  builder.Services.AddSwaggerGen();
  app.UseSwagger();
  app.UseSwaggerUI(options =>
  {
      options.SwaggerEndpoint("/swagger/v1/swagger.json", "TechChallenge.Oficina.Monolith.API v1");
      options.RoutePrefix = string.Empty; // Raiz
  });
  ```
- Metadados de resposta (`Produces`, `Produces<T>`) declarados nos endpoints Minimal APIs para documentação automática de códigos HTTP.

**Consequências**:
- ✅ Documentação auto-atualizada com código.
- ✅ Reduz necessidade de documentação manual.
- ✅ Facilita testes de API sem ferramentas externas.
- ✅ Swagger disponível em desenvolvimento.
