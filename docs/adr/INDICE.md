# ADRs - Architecture Decision Records

A visão geral da arquitetura (diagramas de componentes e de sequência) e o índice das RFCs estão em [docs/arquitetura.md](../arquitetura.md).

## Índice

1. [ADR-001: Clean Architecture com Folder-by-Feature](ADR-001-Clean-Architecture.md)
2. [ADR-002: Domain-Driven Design e Entidades Ricas](ADR-002-DDD-e-Entidades-Ricas.md)
3. [ADR-003: Value Objects para Validação de Identificação (CPF/CNPJ)](ADR-003-Value-Objects-Identificacao.md)
4. [ADR-004: Gateways e Persistência](ADR-004-Gateways-e-Persistencia.md)
5. [ADR-005: Separação de Contratos em Commands/Queries/ViewModels](ADR-005-Separacao-Commands-Queries-ViewModels.md)
6. [ADR-006: Mapeamento com AutoMapper na Application (UseCases)](ADR-006-AutoMapper-nos-UseCases.md)
7. [ADR-007: Entity Framework Core com PostgreSQL](ADR-007-EFCore-com-PostgreSQL.md)
8. [ADR-008: Swagger para Documentação de API](ADR-008-Swagger-para-API.md)
9. [ADR-009: Migrações Automáticas no Startup](ADR-009-Migracoes-Automaticas-no-Startup.md)
10. [ADR-011: Envio de E-mail de Orçamento com Resend](ADR-011-Envio-de-Email-de-Orcamento.md)
11. [ADR-012: Controle de Estoque de Insumos](ADR-012-Controle-de-Estoque-de-Insumos.md)
12. [ADR-013: Estratégia de Tratamento de Erros em Duas Camadas](ADR-013-Estrategia-de-Tratamento-de-Erros-em-Duas-Camadas.md)
13. [ADR-014: Observabilidade com Datadog](ADR-014-Observabilidade-com-Datadog.md)
14. [ADR-015: Comunicação Síncrona (HTTP/REST) via API Gateway](ADR-015-Comunicacao-Sincrona-via-API-Gateway.md)

### ADRs em outros repositórios

A numeração das ADRs é única entre os quatro repositórios. As decisões de infraestrutura ficam no repositório que as implementa:

15. [ADR-016: Escalabilidade das APIs com HPA](https://github.com/GuiToniello/tech-challenge-fase-3-K8S-soat16-rm374658/blob/main/docs/adr/ADR-016-Escalabilidade-com-HPA.md) (repo K8S)
16. [ADR-017: PostgreSQL no Amazon RDS e Modelo de Dados](https://github.com/GuiToniello/tech-challenge-fase-3-DB-soat16-rm374658/blob/main/docs/adr/ADR-017-PostgreSQL-no-Amazon-RDS-e-Modelo-de-Dados.md) (repo DB)

## Resumo de Contextos

### Contexto de Arquitetura
- **ADR-001**: Clean Architecture com folder-by-feature e regra de dependência
- **ADR-004**: Portas de saída (Gateways) de persistência no projeto Application (UseCases) com implementação no projeto DB

### Contexto de Domain-Driven Design
- **ADR-002**: Entidades ricas com comportamento validador
- **ADR-003**: Value Objects para validação de identificação

### Contexto de Separação de Responsabilidades
- **ADR-005**: Contratos segregados (Commands/Queries/ViewModels)
- **ADR-006**: AutoMapper centralizado no projeto Application (UseCases)

### Contexto de Infraestrutura de Dados
- **ADR-007**: EF Core + PostgreSQL com migrations versionadas
- **ADR-009**: Migrações automáticas no startup
- **ADR-017** (repo DB): PostgreSQL no Amazon RDS, com justificativa formal, diagrama ER e relacionamentos

### Contexto de Comunicação entre Componentes
- **ADR-015**: Comunicação síncrona (HTTP/REST) com entrada única pelo API Gateway, APIs compartilhando o banco e sem mensageria

### Contexto de Escalabilidade
- **ADR-016** (repo K8S): HPA por API, de 1 a 10 pods conforme CPU e memória, com os nós do cluster fixos

### Contexto de Exposição da API
- **ADR-008**: Swagger para documentação auto-atualizada

### Contexto de Comunicação com Cliente
- **ADR-011**: Envio de orçamento por e-mail via Resend com porta de saída na Application (UseCases)

### Contexto de Controle de Estoque
- **ADR-012**: Verificação e débito de estoque de insumos em ordens de serviço

### Contexto de Segurança e Resiliência
- **ADR-013**: Tratamento de erros esperados na borda (endpoints + adapters/controllers da arquitetura) e erros inesperados no middleware global

### Contexto de Observabilidade
- **ADR-014**: Logs JSON no stdout (Serilog) com eventos de negócio, APM injetado no cluster e métricas, monitores e dashboard no Datadog

