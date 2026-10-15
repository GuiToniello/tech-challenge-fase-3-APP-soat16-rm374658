# ADR-015: Comunicação Síncrona (HTTP/REST) via API Gateway

**Status**: Aceita

**Resumo**:
Toda a comunicação do sistema é **síncrona**: quem chama espera a resposta na hora. Os clientes falam com as APIs por HTTPS, no estilo REST e com JSON, sempre pela mesma porta de entrada, o Amazon API Gateway. As APIs não chamam umas às outras: elas compartilham o mesmo banco de dados. Não há filas nem mensageria. O desenho geral está no [diagrama de componentes](../arquitetura.md#2-diagrama-de-componentes).

**Contexto**:
O sistema tem 5 APIs .NET no Amazon EKS (monolith, createos, approval, getos e status), as Lambdas de autenticação (repo LAMBDA), o banco PostgreSQL no Amazon RDS (repo DB) e três serviços externos: Auth0, Resend e Datadog. Era preciso definir:
- como os clientes chegam às APIs;
- como as partes internas trocam dados entre si;
- como o sistema fala com os serviços externos.

Fatores de decisão:
- simplicidade: poucas peças para entender, testar e operar;
- custo baixo: projeto de estudo, com um cluster pequeno (2 nós `t3.small`);
- segurança: uma única entrada pública, com autenticação logo na entrada;
- os fluxos de negócio precisam de resposta imediata: abrir uma OS devolve o ID dela, e aprovar o orçamento já muda o status.

**Decisão**:

1. **Clientes → APIs: HTTP síncrono, estilo REST, com JSON.**
   - As APIs usam Minimal APIs do ASP.NET Core. Cada ação é uma rota, como `POST /api/ordens-servico/completa` ou `POST /api/approval/aprovar/{id}`.
   - As respostas usam os códigos HTTP para dizer o que aconteceu (201, 400, 404 etc.), como define a ADR-013.

2. **Uma única entrada: o Amazon API Gateway (HTTP API), só com HTTPS.**
   - O começo do caminho escolhe a API: `/monolith`, `/approval`, `/createos`, `/getos` e `/status`. O gateway remove esse prefixo antes de encaminhar: `/createos/api/...` chega à API como `/api/...`.
   - `POST /auth/token` vai para a Lambda que devolve o JWT (emitido pelo Auth0).
   - Nenhuma API fica exposta direto na internet. O caminho até os pods passa por um VPC Link e por um balanceador interno (NLB), detalhado no [ESTRUTURA.md](https://github.com/GuiToniello/tech-challenge-fase-3-K8S-soat16-rm374658/blob/main/infra/ESTRUTURA.md) do repo K8S.

3. **Autenticação na entrada e na API.** O Lambda authorizer confere o JWT (com a claim `cpf`) antes de a requisição entrar na rede do projeto, e a API confere o JWT de novo ([RFC-003](https://github.com/GuiToniello/tech-challenge-fase-3-LAMBDA-soat16-rm374658/blob/main/docs/rfc/RFC-003-Estrategia-de-Autenticacao.md)).

4. **Entre as APIs: banco compartilhado, sem chamadas diretas.**
   - As 5 APIs leem e gravam no mesmo PostgreSQL e usam o mesmo código de negócio (projetos Entities, UseCases e Controllers).
   - Exemplo: uma OS aberta pela createos-api é lida pela status-api direto no banco, sem uma API chamar a outra.

5. **Entre as Lambdas: chamada síncrona.** A Lambda que devolve o token (`issue-token`) chama a Lambda que consulta o banco (`buscar-cliente`) pela API da própria AWS Lambda (`Invoke`) e espera a resposta. A divisão em duas funções evita um NAT Gateway, que é cobrado por hora (RFC-003).

6. **Serviços externos: HTTPS síncrono.** Auth0 (emissão do token e chaves públicas), Resend (e-mail) e Datadog (o Datadog Agent envia os dados do cluster).

7. **Sem mensageria nesta fase.** Não há fila (Amazon SQS), tópico (Amazon SNS), barramento de eventos (Amazon EventBridge) nem Kafka. Quando um passo falha, a requisição falha, e o cliente pode tentar de novo.

**Consequências**:
- ✅ Fácil de entender, testar (Postman) e investigar: cada pedido tem uma resposta imediata, e o trace do Datadog mostra o caminho dentro da API (ADR-014).
- ✅ Menos peças para pagar e operar: sem servidor de mensagens, sem Ingress Controller e sem balanceador público.
- ✅ Uma só porta de entrada concentra o HTTPS, o roteamento e a autenticação.
- ✅ Com o banco compartilhado, as 5 APIs veem sempre os mesmos dados, sem cópias para sincronizar.
- ⚠ **Dependência em tempo real:** se o banco ou o Resend estiver fora do ar, a requisição falha na hora, sem fila nem nova tentativa automática. Exemplo: o envio do orçamento por e-mail responde 500 se o Resend falhar (ADR-011).
- ⚠ **Acoplamento pelo banco:** uma mudança nas tabelas afeta as 5 APIs, e 4 delas aplicam as migrations ao subir (ADR-009). O banco também é um ponto único de falha para todas.
- ⚠ **Latência somada:** cada requisição passa pelo Lambda authorizer, sem cache (a decisão depende da rota), e pela rede interna até o pod.
- ⚠ **Operações longas sem transação única:** a abertura completa da OS grava cliente, veículo, peças, serviços e OS em etapas separadas. Se uma etapa falhar, as anteriores continuam gravadas ([arquitetura.md](../arquitetura.md#4-diagrama-de-sequência-abertura-de-ordem-de-serviço), seção 4).

**Alternativas Rejeitadas**:
- Mensageria (SQS, SNS ou EventBridge) entre as APIs ou para o e-mail:
  - rejeitada por somar serviços, filas de erro e processamento em segundo plano que o escopo atual não exige. Pode ser uma evolução, começando pelo envio de e-mail (ADR-011).
- Chamadas HTTP entre as APIs, cada uma com o próprio banco (*database per service*):
  - rejeitada por exigir mais instâncias de banco (custo), cópia e sincronização de dados entre as APIs e tratamento de falhas em cada chamada.
- gRPC entre serviços:
  - rejeitada porque não há chamadas entre as APIs, e os clientes (collections do Postman) usam REST com JSON.
- Ingress Controller no cluster, a solução usada antes:
  - substituída pelo API Gateway, que oferece HTTPS sem domínio nem certificado próprios e permite o Lambda authorizer na entrada.
- Um balanceador público para cada API (Service do tipo `LoadBalancer`):
  - rejeitada pelo custo por hora de cada balanceador e por expor as APIs sem a autenticação da entrada.

**Relação com Outras ADRs**:
- **ADR-009**: as migrations aplicadas no startup são o lado prático do banco compartilhado.
- **ADR-011**: o envio de e-mail é síncrono, sem fila.
- **ADR-013**: os erros esperados viram respostas HTTP claras, parte do contrato REST.
- **ADR-014**: logs e traces acompanham cada requisição síncrona dentro das APIs.
- **ADR-016** (repo K8S): como as APIs não guardam estado e não há filas, elas escalam criando mais pods: [ADR-016](https://github.com/GuiToniello/tech-challenge-fase-3-K8S-soat16-rm374658/blob/main/docs/adr/ADR-016-Escalabilidade-com-HPA.md).
- **ADR-017** (repo DB): o banco PostgreSQL compartilhado: [ADR-017](https://github.com/GuiToniello/tech-challenge-fase-3-DB-soat16-rm374658/blob/main/docs/adr/ADR-017-PostgreSQL-no-Amazon-RDS-e-Modelo-de-Dados.md).
