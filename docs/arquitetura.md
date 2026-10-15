# Arquitetura da solução

Este documento dá a visão geral de como o sistema da oficina funciona de ponta a ponta. Ele junta as peças dos quatro repositórios do projeto: **APP** (este, com o código das APIs), [K8S](https://github.com/GuiToniello/tech-challenge-fase-3-K8S-soat16-rm374658) (rede, Kubernetes, API Gateway e Datadog), [DB](https://github.com/GuiToniello/tech-challenge-fase-3-DB-soat16-rm374658) (banco de dados) e [LAMBDA](https://github.com/GuiToniello/tech-challenge-fase-3-LAMBDA-soat16-rm374658) (autenticação).

Neste documento:
1. Glossário rápido.
2. Diagrama de componentes: a nuvem, as APIs, o banco e o monitoramento.
3. Diagrama de sequência da autenticação.
4. Diagrama de sequência da abertura de uma ordem de serviço.
5. Índice das decisões de arquitetura (RFCs e ADRs) dos quatro repositórios.

Os diagramas usam [Mermaid](https://mermaid.js.org/), que o GitHub desenha sozinho. Eles são uma **visão geral**: escondem detalhes como a rede interna e as pipelines, que estão nos documentos indicados ao longo do texto.

## 1. Glossário rápido

| Termo | O que significa aqui |
|---|---|
| API | Programa que recebe pedidos pela internet (requisições HTTP) e responde em JSON. O sistema tem 5 APIs em .NET |
| OS | Ordem de serviço: o pedido de conserto de um veículo, com os serviços e as peças (insumos) |
| JWT (token) | Um "crachá digital" assinado, que prova quem é o usuário. Vai em cada requisição, no header `Authorization` |
| Claim | Uma informação gravada dentro do JWT. Aqui, a mais importante é o `cpf` do cliente |
| AWS | A nuvem da Amazon, onde o sistema roda, na região `us-east-1` (leste dos EUA) |
| API Gateway | A porta de entrada pública. Recebe todas as requisições e envia cada uma para o lugar certo |
| Lambda | Função que roda na AWS só quando é chamada, sem servidor para administrar |
| VPC | A rede do projeto na AWS. O banco fica numa parte dela sem acesso à internet |
| Kubernetes (EKS) | Sistema que roda as APIs em containers e as reinicia se caírem. O Amazon EKS é o Kubernetes gerenciado pela AWS |
| Pod | Uma cópia da API rodando no Kubernetes |
| HPA | Regra do Kubernetes que cria ou remove pods de uma API conforme o uso de CPU e memória |
| RDS (PostgreSQL) | Banco de dados relacional (organizado em tabelas), gerenciado pela AWS |
| Auth0 | Serviço externo que guarda os usuários (e-mail e senha) e emite os JWTs |
| Datadog | Serviço externo de monitoramento: recebe logs, métricas e traces, mostra painéis e envia alertas |
| Resend | Serviço externo de envio de e-mail |

## 2. Diagrama de componentes

O diagrama mostra as peças principais do sistema e quem conversa com quem.

```mermaid
flowchart LR
  Usuario([Usuario<br/>Postman])

  subgraph AWS[AWS - regiao us-east-1]
    GW[API Gateway<br/>entrada unica, HTTPS]
    Lambdas[Lambdas de autenticacao<br/>pedem o JWT ao Auth0<br/>e conferem o JWT]

    subgraph VPC[VPC - rede do projeto]
      subgraph EKS[Kubernetes - Amazon EKS]
        APIs[5 APIs .NET<br/>monolith, createos, approval,<br/>getos e status<br/>HPA: 1 a 10 pods por API]
      end
      DB[(PostgreSQL<br/>Amazon RDS)]
    end
  end

  subgraph Externos[Servicos externos]
    Auth0[Auth0<br/>usuarios e JWT]
    Resend[Resend<br/>e-mail]
    Datadog[Datadog<br/>monitoramento]
  end

  Usuario -->|HTTPS| GW
  GW -->|login e autorizacao| Lambdas
  GW -->|requisicoes autorizadas| APIs
  Lambdas -->|consulta o CPF| DB
  Lambdas -->|pede o JWT| Auth0
  APIs -->|le e grava| DB
  APIs -->|envia o orcamento| Resend
  APIs -.->|logs, metricas e traces| Datadog
```

Como ler: a seta cheia é uma chamada feita durante uma requisição; a tracejada é o envio dos dados de monitoramento.

### 2.1. As peças do diagrama

| Peça | Papel | Repositório |
|---|---|---|
| API Gateway | Única entrada pública. O começo do caminho escolhe a API (`/monolith`, `/createos`, `/approval`, `/getos` e `/status`), e `POST /auth/token` vai para a Lambda de token | K8S |
| Lambdas de autenticação | A Lambda de token confere o CPF, pede ao Auth0 o JWT com o CPF do cliente e devolve esse JWT (seção 3). O Lambda authorizer confere o JWT antes de cada requisição chegar às APIs (seção 4) | LAMBDA |
| 5 APIs .NET no EKS | **monolith**: cadastros e ciclo completo da OS; **createos**: abertura da OS; **approval**: aprovação ou recusa do orçamento; **getos**: lista de OS por prioridade; **status**: acompanhamento da OS. O HPA ajusta de 1 a 10 pods por API | APP (código) e K8S (cluster) |
| PostgreSQL no Amazon RDS | Banco único das 5 APIs, sem acesso pela internet | DB |
| Auth0 | Guarda os usuários e emite e assina os JWTs | Externo (configurado no LAMBDA) |
| Resend | Envia o orçamento por e-mail | Externo |
| Datadog | Monitoramento: logs, métricas, traces, alertas e dashboard | Externo (configurado no K8S) |

Detalhes que o diagrama esconde:
- **Rede:** entre o API Gateway e as APIs há um VPC Link e um balanceador interno (NLB), e o banco fica numa sub-rede sem internet. Veja o [ESTRUTURA.md](https://github.com/GuiToniello/tech-challenge-fase-3-K8S-soat16-rm374658/blob/main/infra/ESTRUTURA.md) do repo K8S.
- **Kubernetes:** Deployments, Services, ConfigMaps, Secret e HPAs de cada API estão no [k8s/README.md](https://github.com/GuiToniello/tech-challenge-fase-3-K8S-soat16-rm374658/blob/main/k8s/README.md) do repo K8S.
- **Monitoramento:** o Datadog Agent roda dentro do cluster e envia os dados ao Datadog. Veja [observabilidade.md](./observabilidade.md) e o [DATADOG.md](https://github.com/GuiToniello/tech-challenge-fase-3-K8S-soat16-rm374658/blob/main/infra/DATADOG.md) do repo K8S.
- **Entrega de versões (CI/CD):** o GitHub Actions de cada repositório cria a infraestrutura e publica as imagens das APIs no Amazon ECR. Veja as pipelines do [APP](../.github/workflows/README.md), do [K8S](https://github.com/GuiToniello/tech-challenge-fase-3-K8S-soat16-rm374658/blob/main/.github/workflows/README.md), do [DB](https://github.com/GuiToniello/tech-challenge-fase-3-DB-soat16-rm374658/blob/main/.github/workflows/README.md) e do [LAMBDA](https://github.com/GuiToniello/tech-challenge-fase-3-LAMBDA-soat16-rm374658#9-pipelines-github-actions).

## 3. Diagrama de sequência: autenticação

Um diagrama de sequência mostra, de cima para baixo, a ordem das mensagens trocadas entre as peças. Este mostra como um cliente **já cadastrado** consegue o JWT com o seu CPF, exigido em todas as rotas das APIs.

```mermaid
sequenceDiagram
  autonumber
  actor Usuario
  participant GW as API Gateway
  participant Lambda as Lambda de token
  participant DB as PostgreSQL
  participant Auth0

  Usuario->>GW: POST /auth/token com email, senha e cpf
  GW->>Lambda: Encaminha o pedido
  Lambda->>DB: Este CPF tem cadastro?
  DB-->>Lambda: Sim
  Lambda->>Auth0: Pede o token com email, senha e cpf
  Auth0-->>Lambda: JWT assinado, com a claim cpf
  Lambda-->>GW: 200 com o JWT
  GW-->>Usuario: 200 com o JWT
  Note over Usuario,Auth0: CPF sem cadastro = 404. Email ou senha errados = 401.
```

Pontos importantes:
- **Quem cria e assina o token é o Auth0.** A Lambda confere se o CPF tem cadastro, pede o token ao Auth0 e devolve esse token ao usuário.
- **A "Lambda de token" do diagrama é formada por duas funções:** uma consulta o banco, dentro da rede do projeto, e a outra fala com o Auth0, fora dela. Assim o banco continua sem acesso à internet.
- **Cliente novo:** quem ainda não tem cadastro pega um token direto no Auth0 (sem `cpf`), se cadastra em `POST /monolith/api/clientes`, a única rota que aceita esse token, e depois segue o fluxo acima.
- Respostas de erro e configuração do Auth0: [README do repo LAMBDA](https://github.com/GuiToniello/tech-challenge-fase-3-LAMBDA-soat16-rm374658#2-fluxos). Opções avaliadas: [RFC-003](https://github.com/GuiToniello/tech-challenge-fase-3-LAMBDA-soat16-rm374658/blob/main/docs/rfc/RFC-003-Estrategia-de-Autenticacao.md).

## 4. Diagrama de sequência: abertura de ordem de serviço

Este diagrama mostra a abertura de uma OS pela createos-api, que recebe cliente, veículo, serviços e peças em uma única chamada e devolve o identificador (ID) da OS. O usuário já tem o JWT da seção 3.

```mermaid
sequenceDiagram
  autonumber
  actor Usuario
  participant GW as API Gateway
  participant Authz as Lambda authorizer
  participant API as createos-api
  participant DB as PostgreSQL

  Usuario->>GW: POST /createos/api/ordens-servico/completa<br/>com o JWT e os dados da OS
  GW->>Authz: O JWT e valido e tem o cpf?
  Authz-->>GW: Sim
  GW->>API: Encaminha o pedido
  API->>DB: Grava cliente, veiculo, servicos e pecas
  API->>DB: Grava a OS com status Recebida
  API-->>GW: 201 com o ID da OS
  GW-->>Usuario: 201 com o ID da OS
  Note over Usuario,DB: Sem token = 401. Token invalido ou sem cpf = 403. Dados invalidos = 400.
```

Pontos importantes:
- **O token é conferido duas vezes:** pelo Lambda authorizer, na entrada, e pela própria API. Se uma das camadas falhar, a outra ainda protege.
- **A OS nasce com o status `Recebida`** e com a primeira linha do histórico de status, usado na ordenação por antiguidade e nos indicadores.
- **Cada gravação é feita separadamente, sem uma transação única.** Se uma etapa falhar no meio (por exemplo, placa já cadastrada), o que foi gravado antes continua no banco. Nesse caso, abra a OS pela rota da Monolith que usa cadastros existentes: `POST /monolith/api/ordens-servico`.
- O padrão de comunicação (tudo síncrono, sem filas, com as APIs compartilhando o banco) está na [ADR-015](./adr/ADR-015-Comunicacao-Sincrona-via-API-Gateway.md). Os logs gerados (`os.criada` e `os.falha`) estão em [observabilidade.md](./observabilidade.md).

## 5. Decisões de arquitetura (RFCs e ADRs)

As decisões ficam registradas em dois tipos de documento:
- **RFC (Request for Comments):** proposta escrita que apresenta um problema, compara as opções e recomenda uma. Serve para discutir antes de decidir.
- **ADR (Architecture Decision Record):** registro de uma decisão tomada, com o contexto, as consequências e as alternativas rejeitadas. Explica "por que é assim".

Cada documento fica no repositório que implementa a decisão, e a numeração é única entre os quatro repositórios.

### 5.1. RFCs

| RFC | Assunto | Resultado | Repositório |
|---|---|---|---|
| [RFC-001](https://github.com/GuiToniello/tech-challenge-fase-3-K8S-soat16-rm374658/blob/main/docs/rfc/RFC-001-Escolha-da-Nuvem.md) | Escolha da nuvem | AWS, região `us-east-1` | K8S |
| [RFC-002](https://github.com/GuiToniello/tech-challenge-fase-3-DB-soat16-rm374658/blob/main/docs/rfc/RFC-002-Escolha-do-Banco-de-Dados.md) | Escolha do banco de dados | PostgreSQL no Amazon RDS, registrado na ADR-017 | DB |
| [RFC-003](https://github.com/GuiToniello/tech-challenge-fase-3-LAMBDA-soat16-rm374658/blob/main/docs/rfc/RFC-003-Estrategia-de-Autenticacao.md) | Estratégia de autenticação | O Auth0 continua emitindo o JWT, e o API Gateway exige a claim `cpf` com um Lambda authorizer | LAMBDA |

### 5.2. ADRs

| ADR | Assunto | Repositório |
|---|---|---|
| ADR-001 a ADR-014 | Organização do código, persistência, e-mail, estoque, tratamento de erros e observabilidade. Índice em [docs/adr/INDICE.md](./adr/INDICE.md) | APP |
| [ADR-015](./adr/ADR-015-Comunicacao-Sincrona-via-API-Gateway.md) | Padrão de comunicação: síncrono (HTTP/REST) via API Gateway, com as APIs compartilhando o banco e sem filas | APP |
| [ADR-016](https://github.com/GuiToniello/tech-challenge-fase-3-K8S-soat16-rm374658/blob/main/docs/adr/ADR-016-Escalabilidade-com-HPA.md) | Escalabilidade das APIs com HPA | K8S |
| [ADR-017](https://github.com/GuiToniello/tech-challenge-fase-3-DB-soat16-rm374658/blob/main/docs/adr/ADR-017-PostgreSQL-no-Amazon-RDS-e-Modelo-de-Dados.md) | Banco PostgreSQL no Amazon RDS: justificativa formal, diagrama ER e relacionamentos | DB |
