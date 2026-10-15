# Projeto Oficina

## 1. Identificação

Software de gestão para uma oficina mecânica.

Tech Challenge da Fase 3 do curso SOAT16 da FIAP. Este repositório guarda a aplicação; a infraestrutura fica nos repositórios K8S e DB (seção 6).

Grupo:
-  Guilherme Toniello Vieira -  SOAT16 - rm374658

### 1.2 Links Úteis

Acesse o blueprint no Miro: http://miro.com/app/board/uXjVHVfHuvI=/?share_link_id=633470424823

Você pode acessar o relatório completo da Fase 2 [aqui](./docs/relatório-completo.pdf). Ele descreve o monorepo da Fase 2, antes da divisão em repositórios.

Vídeo completo da Fase 2 [aqui](https://youtu.be/BRWWTNrfGdY).

## 2. Arquitetura

Nessa seção, será descrita a arquitetura em alto nível e organização da solução.

A visão geral do sistema inteiro está em [docs/arquitetura.md](./docs/arquitetura.md): diagrama de componentes (nuvem, APIs, banco e monitoramento), diagramas de sequência da autenticação e da abertura de OS, e o índice das RFCs e ADRs dos quatro repositórios.

A imagem abaixo mostra a organização interna do código de cada API (Clean Architecture, ADR-001).

![Arquitetura](./docs/diagram/arquitetura.png)

### 2.1. Visão Geral

As decisões arquiteturais estão documentadas individualmente como ADRs (Architecture Decision Records) em `docs/adr/`. As ADRs de infraestrutura (ADR-016 e ADR-017) e as RFCs (Request for Comments: propostas que compararam opções antes de uma decisão) ficam no repositório que implementa cada decisão e também aparecem nas tabelas abaixo.

Abaixo, você encontra um resumo.

| ADR | Resumo | Link |
|-----|--------|------|
| ADR-001 — Clean Architecture com Folder-by-Feature | Adota Clean Architecture com 4 anéis de dependência (Entities, UseCases, Controllers, API/Infra), organizados por feature. Define separação de responsabilidades guiada pela regra de inversão de dependência. | [ver](./docs/adr/ADR-001-Clean-Architecture.md) |
| ADR-002 — DDD e Entidades Ricas | Entidades encapsulam comportamentos e validações de domínio via factory methods obrigatórios. Evita entidades anêmicas; erros de negócio são sinalizados com `DomainException`. | [ver](./docs/adr/ADR-002-DDD-e-Entidades-Ricas.md) |
| ADR-003 — Value Objects para CPF/CNPJ | CPF e CNPJ são encapsulados em Value Objects com validação de dígitos verificadores. `IdentificacaoCliente` detecta o tipo automaticamente e garante que apenas valores válidos existam no domínio. | [ver](./docs/adr/ADR-003-Value-Objects-Identificacao.md) |
| ADR-004 — Gateways e Persistência | Contratos de persistência (Gateways) residem nos UseCases; implementações ficam no projeto DB via EF Core. Leituras retornam coleções materializadas; escritas persistem via `SaveChangesAsync`. | [ver](./docs/adr/ADR-004-Gateways-e-Persistencia.md) |
| ADR-005 — Separação de Commands/Queries/ViewModels | Commands (escrita), Queries (leitura) e ViewModels (resposta) são contratos explícitos na camada de UseCases. A API consome esses contratos diretamente, sem criar os seus próprios. | [ver](./docs/adr/ADR-005-Separacao-Commands-Queries-ViewModels.md) |
| ADR-006 — AutoMapper nos UseCases | Mapeamento entre entidades de domínio e DTOs é responsabilidade exclusiva dos UseCases via AutoMapper. Controllers nunca instanciam `IMapper`; recebem apenas ViewModels já mapeadas. | [ver](./docs/adr/ADR-006-AutoMapper-nos-UseCases.md) |
| ADR-007 — EF Core com PostgreSQL | EF Core 10.x com Npgsql como ORM para PostgreSQL, com migrations versionadas. Configurações de mapeamento via `IEntityTypeConfiguration<T>` e connection string via `appsettings.json`. | [ver](./docs/adr/ADR-007-EFCore-com-PostgreSQL.md) |
| ADR-008 — Swagger para Documentação | Documentação automática de endpoints via Swashbuckle com suporte a autenticação JWT no UI. Swagger UI disponível na raiz da aplicação. | [ver](./docs/adr/ADR-008-Swagger-para-API.md) |
| ADR-009 — Migrações Automáticas no Startup | Migrations pendentes são aplicadas automaticamente ao iniciar a API via `ApplyMigrations()`. Elimina intervenção manual no deploy, incluindo ambientes de container. | [ver](./docs/adr/ADR-009-Migracoes-Automaticas-no-Startup.md) |
| ADR-011 — Envio de E-mail de Orçamento | Notifica o cliente por e-mail ao gerar orçamento via porta de saída `IOrcamentoEmailSender` e provedor Resend. A API funciona normalmente sem a ApiKey; o envio de e-mail é desabilitado. | [ver](./docs/adr/ADR-011-Envio-de-Email-de-Orcamento.md) |
| ADR-012 — Controle de Estoque de Insumos | Verifica estoque ao gerar orçamento (sem reserva) e debita automaticamente ao aprovar. Lança `DomainException` se houver estoque insuficiente em qualquer etapa. | [ver](./docs/adr/ADR-012-Controle-de-Estoque-de-Insumos.md) |
| ADR-013 — Tratamento de Erros em Duas Camadas | Erros de negócio esperados são tratados nos controllers com resposta HTTP semântica (4xx). Erros inesperados são capturados pelo middleware global, evitando vazamento de stack trace. | [ver](./docs/adr/ADR-013-Estrategia-de-Tratamento-de-Erros-em-Duas-Camadas.md) |
| ADR-014 — Observabilidade com Datadog | As APIs escrevem logs JSON no stdout (Serilog) com eventos de negócio, e o tracer é injetado no cluster, sem mudar a imagem. Métricas de negócio derivadas dos logs, monitores, synthetics e dashboard ficam em Terraform no repo K8S. | [ver](./docs/adr/ADR-014-Observabilidade-com-Datadog.md) |
| ADR-015 — Comunicação Síncrona via API Gateway | Os clientes falam com as APIs por HTTPS (REST e JSON), sempre pelo API Gateway. As APIs não chamam umas às outras: compartilham o mesmo banco. Não há filas nem mensageria. | [ver](./docs/adr/ADR-015-Comunicacao-Sincrona-via-API-Gateway.md) |
| ADR-016 — Escalabilidade com HPA (repo K8S) | Cada API tem um HPA que mantém de 1 a 10 pods conforme o uso de CPU (alvo de 30%) e de memória (alvo de 150%), medido pelo Metrics Server. Os 2 nós do cluster são fixos. | [ver](https://github.com/GuiToniello/tech-challenge-fase-3-K8S-soat16-rm374658/blob/main/docs/adr/ADR-016-Escalabilidade-com-HPA.md) |
| ADR-017 — PostgreSQL no Amazon RDS e Modelo de Dados (repo DB) | Justificativa formal da escolha do PostgreSQL no Amazon RDS, com o diagrama ER e a explicação dos relacionamentos entre as tabelas. | [ver](https://github.com/GuiToniello/tech-challenge-fase-3-DB-soat16-rm374658/blob/main/docs/adr/ADR-017-PostgreSQL-no-Amazon-RDS-e-Modelo-de-Dados.md) |

| RFC | Resumo | Link |
|-----|--------|------|
| RFC-001 — Escolha da nuvem (repo K8S) | Compara AWS, Azure e Google Cloud. Resultado: AWS, na região `us-east-1`. | [ver](https://github.com/GuiToniello/tech-challenge-fase-3-K8S-soat16-rm374658/blob/main/docs/rfc/RFC-001-Escolha-da-Nuvem.md) |
| RFC-002 — Escolha do banco de dados (repo DB) | Compara PostgreSQL, Aurora, MySQL, SQL Server, DynamoDB, MongoDB e PostgreSQL dentro do EKS. Resultado: PostgreSQL no Amazon RDS. | [ver](https://github.com/GuiToniello/tech-challenge-fase-3-DB-soat16-rm374658/blob/main/docs/rfc/RFC-002-Escolha-do-Banco-de-Dados.md) |
| RFC-003 — Estratégia de autenticação (repo LAMBDA) | Compara onde validar o token e como gravar o CPF nele. Resultado: Lambda authorizer exigindo a claim `cpf`, com o JWT emitido pelo Auth0. | [ver](https://github.com/GuiToniello/tech-challenge-fase-3-LAMBDA-soat16-rm374658/blob/main/docs/rfc/RFC-003-Estrategia-de-Autenticacao.md) |

### 2.2. Autenticação

O projeto usa autenticação via JWT emitido pelo `Auth0` (http://auth0.com/) como provedor de identidade.

**Como funciona na prática:**

A API valida cada requisição verificando o JWT no header `Authorization: Bearer <token>`. A integração usa o pacote `Microsoft.AspNetCore.Authentication.JwtBearer`. As configurações ficam em `appsettings.json` na chave `AuthSettings`:

```json
"AuthSettings": {
  "Authority": "<seu-dominio>.us.auth0.com",
  "Audience": "http://localhost:7194"
}
```

O `Authority` aponta para o domínio Auth0, que expõe o endpoint `/.well-known/openid-configuration` usado pelo middleware para buscar as chaves públicas de validação. O `Audience` identifica esta API junto ao Auth0.

Todos os endpoints exigem autenticação por padrão (política de fallback `RequireAuthenticatedUser`). Os únicos endpoints públicos são `/health` e `/health/ready`, que também verifica o banco (seção 6.3).

**Você não precisa criar uma conta no Auth0.** A Collection do Postman em `/e2e` já possui um request configurado com as credenciais de demonstração para obter o token de acesso via password grant (Resource Owner Password).

Na AWS, o API Gateway (repo K8S) tem um Lambda authorizer (repo LAMBDA) que exige, além do JWT válido, a claim `cpf` do cliente. Esse token é gerado pela Lambda `POST /auth/token` (`{"email","senha","cpf"}`), que confere o cliente no banco e pede ao Auth0 o JWT já com a claim. Só o cadastro de cliente (`POST /monolith/api/clientes`) aceita o token direto do Auth0, sem `cpf`. O código das APIs não muda: elas continuam validando o JWT normalmente. O fluxo está no [diagrama de sequência da autenticação](./docs/arquitetura.md#3-diagrama-de-sequência-autenticação), e as opções avaliadas, na [RFC-003](https://github.com/GuiToniello/tech-challenge-fase-3-LAMBDA-soat16-rm374658/blob/main/docs/rfc/RFC-003-Estrategia-de-Autenticacao.md) do repo LAMBDA.

### 2.3. CQRS (CQS)

O padrão implementado é o **CQS (Command Query Separation)** — o nível mais básico do CQRS — sem separação de banco de dados para leitura e escrita.

A separação acontece na **camada de UseCases** (`3 - Application Business Rules`). Cada feature é organizada com as seguintes pastas:

- `Commands/` — objetos de entrada para operações de escrita (ex: `CriarClienteCommand`, `AtualizarClienteCommand`)
- `Queries/` — objetos de entrada para operações de leitura (ex: `ListarClientesQuery`, `ObterClientePorIdQuery`)
- `ViewModels/` — objetos de saída (resposta da API)
- `UseCases/` — a classe que executa a operação, recebendo um `Command` ou uma `Query` como parâmetro tipado

O benefício é tornar a **intenção explícita no contrato**: ao receber um `Command`, sabe-se que haverá efeito colateral; ao receber uma `Query`, sabe-se que é apenas leitura. Não há mediator (ex: MediatR), o roteamento é feito diretamente via injeção de dependência.

## 3. Referências Bibliográficas

### 3.1. Padrões de Arquitetura

Esse projeto foi construído usando os padrões descritos abaixo.

- Clean Architecture (4 anéis e regra de dependência)
  - ADR: [ADR-001](./docs/adr/ADR-001-Clean-Architecture.md)
  - Codigo: [server/](./server/)
- Domain-Driven Design (DDD)
  - ADR: [ADR-002](./docs/adr/ADR-002-DDD-e-Entidades-Ricas.md)
  - Codigo: [Entidades de dominio](./server/4%20-%20Enterprise%20Business%20Rules/TechChallenge.Oficina.Entities/)
- CQS (separação entre Commands e Queries)
  - ADR: [ADR-005](./docs/adr/ADR-005-Separacao-Commands-Queries-ViewModels.md)
  - Codigo: [UseCases](./server/3%20-%20Application%20Business%20Rules/TechChallenge.Oficina.UseCases/)

### 3.2. Padrões Estruturais

- Folder-by-Feature
  - ADR: [ADR-001](./docs/adr/ADR-001-Clean-Architecture.md)
- Gateway/Repository para persistência
  - ADR: [ADR-004](./docs/adr/ADR-004-Gateways-e-Persistencia.md)
  - Codigo: [Gateways DB](./server/1%20-%20Frameworks%20%26%20Drivers/TechChallenge.Oficina.DB/)
- Mapper/Adapter (AutoMapper nos UseCases)
  - ADR: [ADR-006](./docs/adr/ADR-006-AutoMapper-nos-UseCases.md)
  - Codigo: [UseCases](./server/3%20-%20Application%20Business%20Rules/TechChallenge.Oficina.UseCases/)
- Dependency Injection (Inversion of Control)
  - Codigo: [Program.cs](./server/1%20-%20Frameworks%20%26%20Drivers/TechChallenge.Oficina.Monolith.API/Program.cs)
- Service Layer (aplicada na camada de UseCases)
  - Codigo: [UseCases](./server/3%20-%20Application%20Business%20Rules/TechChallenge.Oficina.UseCases/)

### 3.3. Padrões de Design

- Factory Method (entidades ricas com métodos de criação)
  - ADR: [ADR-002](./docs/adr/ADR-002-DDD-e-Entidades-Ricas.md)
- Value Object (CPF/CNPJ e identificação)
  - ADR: [ADR-003](./docs/adr/ADR-003-Value-Objects-Identificacao.md)
- DTO/ViewModel Pattern
  - ADR: [ADR-005](./docs/adr/ADR-005-Separacao-Commands-Queries-ViewModels.md)
- Centralized Exception Handling (duas camadas: controller + middleware)
  - ADR: [ADR-013](./docs/adr/ADR-013-Estrategia-de-Tratamento-de-Erros-em-Duas-Camadas.md)
  - Codigo: [Middleware](./server/1%20-%20Frameworks%20%26%20Drivers/TechChallenge.Oficina.Monolith.API/Middleware/) e [Controllers](./server/2%20-%20Interface%20Adapters/TechChallenge.Oficina.Controllers/)

### 3.4. Padrões Comportamentais

- CQS (Command Query Separation)
  - ADR: [ADR-005](./docs/adr/ADR-005-Separacao-Commands-Queries-ViewModels.md)
  - Implementacao: Commands (escrita), Queries (leitura), ViewModels (resposta)
- Domain Exceptions para regras de negócio e invariantes
  - ADR: [ADR-002](./docs/adr/ADR-002-DDD-e-Entidades-Ricas.md)
  - Codigo: [Exceptions](./server/4%20-%20Enterprise%20Business%20Rules/TechChallenge.Oficina.Entities/Exceptions/)

### 3.5. Referências

#### 3.5.1. Livros e Artigos

MARTIN, Robert C. Clean Architecture: A Craftsman's Guide to Software Structure and Design. Boston: Prentice Hall, 2017.

EVANS, Eric. Domain-Driven Design: Tackling Complexity in the Heart of Software. Boston: Addison-Wesley, 2003.

VERNON, Vaughn. Implementing Domain-Driven Design. Boston: Addison-Wesley, 2013.

FOWLER, Martin. Patterns of Enterprise Application Architecture. Boston: Addison-Wesley, 2003.

GAMMA, Erich; HELM, Richard; JOHNSON, Ralph; VLISSIDES, John. Design Patterns: Elements of Reusable Object-Oriented Software. Boston: Addison-Wesley, 1994.

MARTIN, Robert C. Agile Software Development, Principles, Patterns, and Practices. Upper Saddle River: Prentice Hall, 2002.

#### 3.5.2. Material de Apoio

FOWLER, Martin. CQRS. Martin Fowler, 14 jul. 2011. Disponível em: http://martinfowler.com/bliki/CQRS.html. Acesso em: 10 jun. 2026.

FOWLER, Martin. Inversion of Control Containers and the Dependency Injection pattern. Martin Fowler, 23 jan. 2004. Disponível em: http://martinfowler.com/articles/injection.html. Acesso em: 02 jun. 2026.

## 4. Executando o Projeto

Para executar o projeto localmente, na sua maquina dev, temos 3 alternativas descritas nas subseções `4.2`, `4.3` e `4.4`.

Siga apenas 1 delas.

### 4.1. Pré-requisito

Se você for rodar sem container, vai precisar:

- dotnet 10.x
http://dotnet.microsoft.com/pt-br/download/dotnet/thank-you/sdk-10.0.301-windows-x64-installer

- Postgres instalado e com instância ativa: http://www.postgresql.org/

Para containers, precisa do docker (http://www.docker.com/) ou Podman (http://podman.io/) instalado.

Já temos docker-compose pronto com todas as configurações.

> Recomenda-se o uso de containers

### 4.1.1. Configuração por .env (recomendado para container)

Existe um arquivo de exemplo na raiz do projeto: `.env.example`.

Passo 1 - gere o arquivo `.env` a partir do exemplo.

- Linux/macOS: `cp .env.example .env`
- Windows PowerShell: `Copy-Item .env.example .env`

Passo 2 - ajuste as variáveis conforme necessário, principalmente:

- `POSTGRES_PORT`
- `API_MONOLITH_PORT`
- `DATABASE_CONNECTION_STRING`
- `RESEND_API_KEY` (opcional)

### 4.2. Alternativa A - Containers (docker, podman, ...)

Passo 1 - Com o console apontado para o root do repositório, execute `docker compose up -d --build`

Se estiver usando o podman, use `podman compose up -d --build`

E pronto!

O banco de dados `postgres` e a `api` estarão disponíveis.

Passo 2 - Use `http://localhost:7194/index.html` para acessar o swagger.

### 4.3. Alternativa B - Local com dotnet cli

Passo 1 - rode o `postgres` - pode ser uma instância local ou via container `docker compose up -d postgres`.

Passo 2 - aponte o console para a pasta `server/1 - Frameworks & Drivers/TechChallenge.Oficina.Monolith.API/` e então execute `dotnet run`

Pronto!

Vai subir a API usando http com um certificado autoassinado do dotnet.

Passo 3 - Use `http://localhost:7194/index.html` para acessar o Swagger.

### 4.4. Alternativa C - Local com Visual Studio 2026 ou VS Code

Passo 1 - rode o `postgres` - pode ser uma instância local ou via container `docker compose up -d postgres`.

Passo 2 - abra o arquivo `.slnx` em `/server`

Passo 3 - no Visual Studio, rode usando o perfil `http`.

Passo 4 - Use `http://localhost:7194/index.html` para acessar o Swagger.


### 4.5  Banco de dados

Para popular o banco de dados, use a Collection do Postman na pasta `/e2e`.

Não temos scripts SQL ou endpoint específico, apenas use a collection que ela irá popular o banco e executar demais operações de demonstração.

Se você estiver rodando em container, não é preciso configurar mais nada, apenas rodar, a connection string já está certa.

Se você está rodando o Postgres localmente, com instância não containerizada, precisa configurar a connection string em `server/1 - Frameworks & Drivers/TechChallenge.Oficina.Monolith.API/appsettings.json`, na chave `DatabaseSettings:ConnectionString`.

Na AWS, o banco é o Amazon RDS do repositório DB.

### 4.6. Rodando os testes

Os testes automatizados estão organizados em `server/Tests/` por camada da arquitetura.

Para executar todos os testes da solução, use:

`dotnet test ./server/TechChallenge.Oficina.API.slnx`

Se quiser executar apenas um projeto de teste específico, use o `.csproj` correspondente em `server/Tests/`.

### 4.7. Problemas comuns (troubleshooting)

1. Porta 5432 em uso
Se o Postgres não subir na porta padrão, altere `POSTGRES_PORT` no arquivo `.env` (por exemplo, `5433`) e ajuste também `DATABASE_CONNECTION_STRING` para usar a mesma porta.

2. Erro de autenticação JWT (401)
Verifique se `AuthSettings:Authority` e `AuthSettings:Audience` no `appsettings.json` correspondem exatamente ao token usado pela collection.

3. E-mail não está sendo enviado
Sem `RESEND_API_KEY` (container) ou `ApiKey` no `appsettings.json` (execução local), a API continua funcionando, mas o envio de e-mail fica desabilitado por configuração. O e-mail de mudança de status também depende de `ResendSettings:SendEmailOnStatusChange`, que é `false` nos `appsettings.json`. Para ligá-lo, defina `ResendSettings__SendEmailOnStatusChange=true`.

## 5. Requisitos Implementados

Abaixo, você encontra uma tabela com o resumo dos requisitos implementados: os de negócio da Fase 2 e os de observabilidade (R1 a R7).

| Requisito | Atende? | Observação de escopo | API | Evidências |
|---|---|---|---|---|
| Consulta de status da Ordem de Serviço (OS) | Sim | Consulta de status disponível no fluxo de OS. | `TechChallenge.Oficina.StatusService.API` (API dedicada para consulta de status): [StatusOrdemServicoEndpoints](./server/1%20-%20Frameworks%20%26%20Drivers/TechChallenge.Oficina.StatusService.API/Features/OrdensServico/StatusOrdemServicoEndpoints.cs) | [Endpoints OS](./server/1%20-%20Frameworks%20%26%20Drivers/TechChallenge.Oficina.Monolith.API/Features/OrdensServico/OrdensServicoEndpoints.cs) · [Controller OS](./server/2%20-%20Interface%20Adapters/TechChallenge.Oficina.Controllers/Features/OrdensServico/OrdensServicoController.cs) |
| Aprovação/recusa de orçamento por endpoint externo | Sim | Aprovação e recusa implementadas no fluxo da OS. | `TechChallenge.Oficina.ApprovalService.API` (API dedicada para aprovação/recusa de orçamento): [ApprovalServiceEndpoints](./server/1%20-%20Frameworks%20%26%20Drivers/TechChallenge.Oficina.ApprovalService.API/Features/OrdensServico/ApprovalServiceEndpoints.cs) | [Endpoints OS](./server/1%20-%20Frameworks%20%26%20Drivers/TechChallenge.Oficina.Monolith.API/Features/OrdensServico/OrdensServicoEndpoints.cs) · [UseCases OS](./server/3%20-%20Application%20Business%20Rules/TechChallenge.Oficina.UseCases/Features/OrdensServico/UseCases/OrdemServicoUseCases.cs) |
| Abertura de OS com cliente, veículo, serviços e peças, retornando ID único | Sim | Abertura completa com retorno da identificação da OS. | `TechChallenge.Oficina.CreateOSService.API` (API dedicada para abertura da OS): [CriarOSCompletaEndpoints](./server/1%20-%20Frameworks%20%26%20Drivers/TechChallenge.Oficina.CreateOSService.API/Features/OrdensServico/CriarOSCompletaEndpoints.cs) | [Controller OS](./server/2%20-%20Interface%20Adapters/TechChallenge.Oficina.Controllers/Features/OrdensServico/OrdensServicoController.cs) · [ViewModel Abertura](./server/3%20-%20Application%20Business%20Rules/TechChallenge.Oficina.UseCases/Features/OrdensServico/ViewModels/AberturaOrdemServicoViewModel.cs) |
| Listagem de OS por prioridade + antiguidade, excluindo finalizadas/entregues | Sim | Escopo acordado: endpoint ordenado /ordenadas. | `TechChallenge.Oficina.GetOSService.API` (API dedicada para consultas e listagem de OS): [GetOrdensServicoEndpoints](./server/1%20-%20Frameworks%20%26%20Drivers/TechChallenge.Oficina.GetOSService.API/Features/OrdensServico/GetOrdensServicoEndpoints.cs) | [Gateway OS](./server/1%20-%20Frameworks%20%26%20Drivers/TechChallenge.Oficina.DB/Features/OrdensServico/OrdemServicoGateway.cs) · [Query Ordenada](./server/3%20-%20Application%20Business%20Rules/TechChallenge.Oficina.UseCases/Features/OrdensServico/Queries/ListarOrdensServicoOrdenadasQuery.cs) |
| Atualização de status da OS com notificação ao cliente | Sim | A cada mudança de status da OS, o cliente é notificado por e-mail no fluxo de controller/use case, independentemente da API que executa a alteração. | Atualmente, os endpoints de alteração de status estão em `TechChallenge.Oficina.Monolith.API`: [OrdensServicoEndpoints](./server/1%20-%20Frameworks%20%26%20Drivers/TechChallenge.Oficina.Monolith.API/Features/OrdensServico/OrdensServicoEndpoints.cs). A regra de notificação é centralizada no controller/use case e pode ser reutilizada por APIs dedicadas. | [UseCases OS](./server/3%20-%20Application%20Business%20Rules/TechChallenge.Oficina.UseCases/Features/OrdensServico/UseCases/OrdemServicoUseCases.cs) · [Sender Status](./server/1%20-%20Frameworks%20%26%20Drivers/TechChallenge.Oficina.Email/Features/OrdensServico/OrdemServicoStatusEmailSender.cs) |
| R1 — Latência das APIs | Sim | APM com o tracer .NET injetado pelo Datadog Agent no cluster, sem mudança no código ou no Dockerfile. Latência p95 por serviço no monitor e no dashboard; por rota, na página do serviço no APM. | As 5 APIs (`service` = `monolith-api`, `approval-api`, `createos-api`, `getos-api` e `status-api`) | [ADR-014](./docs/adr/ADR-014-Observabilidade-com-Datadog.md) · `infra/DATADOG.md` do repo [K8S](https://github.com/GuiToniello/tech-challenge-fase-3-K8S-soat16-rm374658) |
| R2 — Consumo de recursos do Kubernetes na AWS | Sim | Datadog Agent (DaemonSet + Cluster Agent) instalado pelo repo K8S. Monitores de CPU e memória em relação ao limit, restarts, CrashLoopBackOff e nós, mais widgets. | Cluster EKS (repo K8S) | [ADR-014](./docs/adr/ADR-014-Observabilidade-com-Datadog.md) · `infra/DATADOG.md` do repo [K8S](https://github.com/GuiToniello/tech-challenge-fase-3-K8S-soat16-rm374658) |
| R3 — Healthchecks e uptime | Sim | `/health/ready` verifica o banco e é consultado pelo `http_check` do Datadog Agent; o `/health` não muda. Uptime externo por Synthetics em `GET /<api>/health` pelo API Gateway, com SLO. | As 5 APIs: `/health` e `/health/ready` | [Program.cs (Monolith)](./server/1%20-%20Frameworks%20%26%20Drivers/TechChallenge.Oficina.Monolith.API/Program.cs) · [Health check do banco](./server/1%20-%20Frameworks%20%26%20Drivers/TechChallenge.Oficina.DB/Extensions.cs) · [docs/observabilidade.md](./docs/observabilidade.md) (seção 6) · `infra/DATADOG.md` do repo [K8S](https://github.com/GuiToniello/tech-challenge-fase-3-K8S-soat16-rm374658) |
| R4 — Falhas no processamento de OS | Sim | Eventos de log `os.falha` por etapa (regra de negócio), `integracao.falha` (Resend) e `http.erro_nao_tratado` (500, identificando banco indisponível), mais 5xx por rota no APM. Viram métricas, monitores e widgets. | As 5 APIs (o controller e o use case de OS são compartilhados) | [docs/observabilidade.md](./docs/observabilidade.md) (seções 3 e 5) · [Eventos OS](./server/3%20-%20Application%20Business%20Rules/TechChallenge.Oficina.UseCases/Features/OrdensServico/Logging/OrdemServicoLogEventos.cs) · [Controller OS](./server/2%20-%20Interface%20Adapters/TechChallenge.Oficina.Controllers/Features/OrdensServico/OrdensServicoController.cs) · [UseCases OS](./server/3%20-%20Application%20Business%20Rules/TechChallenge.Oficina.UseCases/Features/OrdensServico/UseCases/OrdemServicoUseCases.cs) · [Middleware](./server/1%20-%20Frameworks%20%26%20Drivers/TechChallenge.Oficina.Monolith.API/Middleware/GlobalExceptionHandlerMiddleware.cs) |
| R5 — Logs estruturados (JSON) | Sim | Serilog com 1 JSON por linha no stdout e chaves que o Datadog reconhece sem configuração (`timestamp`, `level`, `message`, `evento`, `dd.*` e `error.*`). | As 5 APIs | [LoggingExtension (Monolith)](./server/1%20-%20Frameworks%20%26%20Drivers/TechChallenge.Oficina.Monolith.API/Extensions/LoggingExtension.cs) · [docs/observabilidade.md](./docs/observabilidade.md) (seção 2) |
| R6 — Correlação entre logs e traces | Sim | Dentro das APIs: o tracer injeta `dd.trace_id` e `dd.span_id` em cada log, que abre o trace no APM. API Gateway e Lambdas não são rastreados (limitação registrada na ADR-014). | As 5 APIs | [LoggingExtension (Monolith)](./server/1%20-%20Frameworks%20%26%20Drivers/TechChallenge.Oficina.Monolith.API/Extensions/LoggingExtension.cs) · [ADR-014](./docs/adr/ADR-014-Observabilidade-com-Datadog.md) |
| R7 — Dashboard | Sim | 1 dashboard: OS criadas e encerradas por dia, tempo médio por status, falhas (de negócio por etapa e 5xx por rota), erros de integração e seções técnicas de R1 a R3. Os widgets de negócio só recebem dados durante o trial de 14 dias do Datadog. | Datadog (repo K8S) | [docs/observabilidade.md](./docs/observabilidade.md) (seção 5) · `infra/DATADOG.md` do repo [K8S](https://github.com/GuiToniello/tech-challenge-fase-3-K8S-soat16-rm374658) |


## 6. Infraestrutura e repositórios

Na Fase 3, o projeto está dividido em quatro repositórios. Este (**APP**) guarda o código da aplicação, os testes, os Dockerfiles e as pipelines que buildam as imagens e as publicam no Amazon ECR.

| Repositório | Responsabilidade |
|---|---|
| **APP** (este) | Código .NET, testes, imagens Docker e push para o ECR |
| [K8S](https://github.com/GuiToniello/tech-challenge-fase-3-K8S-soat16-rm374658) | VPC, Amazon EKS, Amazon API Gateway (acesso externo às APIs), addons (Metrics Server e Datadog Agent), recursos do Datadog (métricas de log, monitores, synthetics e dashboard) e manifests Kubernetes das APIs. Também gera o Secret com a connection string do RDS e a chave do Resend |
| [DB](https://github.com/GuiToniello/tech-challenge-fase-3-DB-soat16-rm374658) | Amazon RDS PostgreSQL. O schema é criado pelas migrations das APIs, no startup (ADR-009) |
| [LAMBDA](https://github.com/GuiToniello/tech-challenge-fase-3-LAMBDA-soat16-rm374658) | Lambda authorizer do API Gateway (JWT do Auth0 com a claim `cpf`) e Lambda `POST /auth/token`, que gera o JWT com o CPF do cliente |

Ordem de deploy entre os repositórios: **K8S** Bootstrap → **DB** Bootstrap → **APP** Bootstrap (imagens no ECR) / **LAMBDA** → **K8S** K8s Apply. O último passo é disparado automaticamente por este repositório (seção 7). O APP não tem nada a destruir: o ECR é criado manualmente e fica fora de qualquer `destroy`.

```mermaid
flowchart LR
  subgraph APP[Repo APP]
    Code[server/\nCodigo + testes] --> Images[docker compose build\n5 imagens]
  end

  Images -->|push :latest| ECR[Amazon ECR\ncriado manualmente]
  Images -.->|dispara K8s Apply\nrestart-pods=true| K8S[Repo K8S]
  K8S -->|rollout restart| EKS[Amazon EKS\nPods das APIs]
  EKS -->|pull| ECR
  EKS -->|PostgreSQL 5432| RDS[Amazon RDS\nrepo DB]
```

### 6.1 Contrato com os outros repositórios

| Item | Valor |
|---|---|
| Imagens | `903936907231.dkr.ecr.us-east-1.amazonaws.com/techchallenge-oficina-{monolith,approval,createos,getos,status}:latest`, os mesmos nomes do `docker-compose.yml`. Os manifests do repo K8S usam `:latest` com `imagePullPolicy: Always` |
| Porta e health | Todas as APIs escutam em `8080` e expõem `/health` sem autenticação (probes do K8S, health check do NLB do API Gateway e synthetics do Datadog) |
| Health com banco | Todas as APIs expõem `/health/ready` sem autenticação: verifica o banco e responde `Healthy` (200) ou `Unhealthy` (503). É consultado só pelo `http_check` do Datadog Agent do repo K8S. O `readinessProbe` e o NLB continuam em `/health`, que não depende do banco |
| Rotas | As rotas ficam na raiz (`/api/...` e `/health`), sem `PathBase`. O API Gateway do repo K8S recebe `/<api>/...` (`/monolith`, `/approval`, `/createos`, `/getos` e `/status`) e remove o prefixo antes de encaminhar |
| Configuração | Em AWS, `DatabaseSettings__ConnectionString` e `ResendSettings__ApiKey` vêm do Secret `oficina-api-secrets`, gerado pelo repo K8S. Os ConfigMaps do repo K8S definem só `ASPNETCORE_*`, `AllowedHosts` e o nível de log (`Serilog__MinimumLevel__Default` e `Serilog__MinimumLevel__Override__Microsoft.AspNetCore`, que sobrescrevem a seção `Serilog` do `appsettings.json`). `AuthSettings` e o restante de `ResendSettings` (`FromEmail`, `SendEmailOnStatusChange`) vêm do `appsettings.json` da imagem |
| Logs | JSON no stdout, 1 por linha (Serilog), com `timestamp`, `level`, `message`, `evento`, `logger.name`, `dd.*`, `error.*` e as propriedades dos eventos. O Datadog Agent do repo K8S coleta o stdout dos pods, e as métricas de log, os monitores e o dashboard de lá dependem dos nomes de eventos, propriedades e valores de [docs/observabilidade.md](./docs/observabilidade.md) |
| Datadog | Os Deployments do repo K8S têm as labels `tags.datadoghq.com/env: study` e `tags.datadoghq.com/service: <api>-api`, que definem as tags `env` e `service` de logs, traces e métricas. O Agent injeta o tracer .NET nos pods do namespace `oficina`. Labels, tracer e chaves do Datadog ficam no repo K8S: nada disso está neste repositório |
| Banco | As migrations do EF Core rodam no startup de 4 das 5 APIs (Monolith, CreateOS, GetOS e Status). A ApprovalService não aplica migrations. Com o RDS novo (vazio), as tabelas são criadas quando as APIs sobem |

### 6.2 ECR (manual)

Crie uma vez, em `us-east-1`, os 5 repositórios privados com *tag mutability* **MUTABLE**. A tag `latest` é sobrescrita a cada push, e com IMMUTABLE o segundo push falha:

- `techchallenge-oficina-monolith`
- `techchallenge-oficina-approval`
- `techchallenge-oficina-createos`
- `techchallenge-oficina-getos`
- `techchallenge-oficina-status`

### 6.3 Observabilidade (Datadog)

As 5 APIs escrevem logs em JSON no stdout, 1 por linha (Serilog), com eventos de negócio do fluxo de OS (`os.criada`, `os.status_alterado`, `os.falha` etc.) e erros com `error.*`. Na AWS, o Datadog Agent do repo K8S coleta esses logs e injeta o tracer .NET nos pods (APM), sem mudança no código ou no Dockerfile, e cada log traz o `dd.trace_id` da requisição. O `/health/ready` verifica o banco para o `http_check` do Agent.

- Decisão, consequências e limitações: [ADR-014](./docs/adr/ADR-014-Observabilidade-com-Datadog.md).
- Contrato com o repo K8S (formato do log, catálogo de eventos, valores permitidos, mapa evento → métrica → dashboard e consultas úteis): [docs/observabilidade.md](./docs/observabilidade.md).
- Agent, métricas, monitores, synthetics, dashboard, secrets e liga/desliga (`DD_ENABLED`): `infra/DATADOG.md` do repositório [K8S](https://github.com/GuiToniello/tech-challenge-fase-3-K8S-soat16-rm374658).

Neste repositório não há nada a configurar: as chaves do Datadog ficam no repo K8S. Localmente, o `docker compose` mostra o mesmo JSON, só sem o objeto `dd`, porque não há tracer. A conta do Datadog é um trial de 14 dias; depois dele, APM, logs e synthetics param (ADR-014).

## 7. Fluxo de Deploy (CI/CD)

O CI/CD usa GitHub Actions. Dois workflows de entrada chamam três workflows reutilizáveis:

- **Bootstrap** (manual): build + testes → build e push das 5 imagens no ECR → dispara o K8s Apply no repo K8S. Use-o na primeira publicação, depois do Bootstrap do repo DB, e para republicar a HEAD da `main`.
- **Deploy**:
  - Pull Request para a `main`: build + testes. É o check obrigatório `build-test / build-test`.
  - Push na `main` com mudança em `server/**`, `docker-compose.yml` ou nos workflows: build + testes → push no ECR → dispara o K8s Apply.

```mermaid
flowchart LR
  subgraph Reusable[Workflows reutilizaveis]
    BT[_build-test.yml\nrestore, build e testes]
    BP[_build-push.yml\ndocker compose build/push]
    KR[_k8s-restart.yml\ngh workflow run no repo K8S]
  end

  B[Bootstrap\nmanual] --> BT
  PUSH[Deploy - push na main] --> BT
  BT -->|so no Bootstrap e no push| BP --> KR
  PR[Deploy - pull_request] -->|so build + testes| BT
```

O push das imagens só roda a partir da `main`: disparado em outra branch, o job falha com erro.

**Primeiro push na `main`:** o primeiro push deste código já roda o Deploy completo, porque muda `server/**`. Para ele funcionar, crie antes os repositórios ECR (6.2) e configure os secrets e variables. Se o cluster ou o RDS ainda não existirem, as imagens são publicadas, mas o K8s Apply disparado falha no repo K8S. Nesse caso, siga a ordem da seção 6 e rode o Bootstrap depois. **Desative também os workflows do repositório da fase 2**, que publicam nos mesmos repositórios ECR e com a mesma tag. Os detalhes, os secrets e as variables estão em [.github/workflows/README.md](./.github/workflows/README.md).

## 8. Finalização

Nessa seção, você encontra observações gerais.

- Para fazer requisições, use as Collections do Postman na pasta `/e2e`
  - importe as collections no Postman
  - cada API tem um collection
  - Em `variables`, altere para apontar a URL para onde está rodando o projeto.
  - Na AWS, importe também o environment `e2e/AWS.postman_environment.json`, selecione-o e preencha só `gatewayUrl` com a URL do API Gateway (output `api_gateway_endpoint` da foundation do repo K8S). Ele monta as URLs de cada API com o prefixo do gateway (`/monolith`, `/approval`, `/createos`, `/getos` e `/status`) e o `tokenUrl` (`/auth/token`). O endpoint só aceita HTTPS, e o Swagger UI não abre atrás do prefixo (use as collections).
  - O "00 - Setup" de cada collection:
    1. pega o token direto do Auth0;
    2. cadastra um cliente com ele;
    3. gera, pela Lambda `/auth/token`, o JWT com a claim `cpf` desse cliente, usado em todos os requests seguintes.
  - Rodando local (sem gateway), `tokenUrl` fica vazio: a geração pela Lambda é pulada e a collection segue com o token direto do Auth0.

- Para o envio de e-mails, é preciso configurar `ApiKey` no appsettings.json ou `ResendSettings__ApiKey` para container

Você pode criar uma conta em http://resend.com/ e gerar a ApiKey.

Sem a ApiKey, a API funciona normalmente, só não envia os e-mails.
