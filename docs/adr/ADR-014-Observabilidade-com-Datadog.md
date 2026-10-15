# ADR-014: Observabilidade com Datadog

**Status**: Aceita

**Contexto**:
O sistema da oficina tem 5 APIs .NET 10 no Amazon EKS (monolith, approval, createos, getos e status). Elas ficam atrás de um Amazon API Gateway com Lambda authorizer e usam o Amazon RDS PostgreSQL e o envio de e-mail via Resend. Até esta decisão, não havia observabilidade:
- o único log era um `LogError` em texto no middleware global (ADR-013);
- não havia traces, métricas de negócio, monitores nem dashboards;
- o `/health` não verificava o banco.

Requisitos de observabilidade:
- R1: latência das APIs;
- R2: consumo de recursos do Kubernetes na AWS;
- R3: healthchecks e uptime;
- R4: falhas no processamento de ordens de serviço (OS);
- R5: logs estruturados, em JSON legível;
- R6: correlação entre logs e traces;
- R7: um dashboard com os indicadores de negócio e técnicos.

Fatores de decisão:
- simplicidade: o caminho mais curto que funcione de ponta a ponta, com uma única ferramenta;
- mínimo de mudança: nada nos Dockerfiles e, nas camadas internas, só `ILogger<T>`;
- custo zero: projeto de estudo, sem cartão de crédito cadastrado;
- cluster pequeno: 2 nós `t3.small`, com 11 pods e cerca de 1,4 GiB alocáveis por nó.

**Decisão**:
Adotar o **Datadog** para logs, traces (APM), métricas, monitores, testes sintéticos e dashboard. Toda a infraestrutura do Datadog fica em Terraform no repositório K8S (`infra/addons`). Este repositório só emite os logs e expõe os health checks.

1. **Agent no cluster**
   - Helm chart `datadog/datadog` 3.255.0 via `helm_release`, no mesmo padrão do Metrics Server: 1 Agent por nó (DaemonSet) e 1 Cluster Agent.
   - Operator e system-probe desligados, requests fixos e PriorityClass, para caber no cluster.
   - Coleta o stdout dos containers (exceto `kube-system` e `datadog`) e as métricas de pods e nós, e roda o `http_check` no `/health/ready` das 5 APIs.

2. **APM sem mudar a imagem**
   - Single Step Instrumentation restrita ao namespace `oficina`: o admission controller injeta o tracer .NET (v3) nos pods, sem mudança em Dockerfile ou código.
   - Os Deployments do repo K8S recebem as labels `tags.datadoghq.com/env` (`study`) e `tags.datadoghq.com/service` (`monolith-api`, `approval-api`, `createos-api`, `getos-api` e `status-api`). Com elas, logs, traces e métricas saem com as mesmas tags `env` e `service`.
   - `DD_LOGS_INJECTION` ligado: o tracer grava `dd_trace_id` e `dd_span_id` em cada log emitido durante uma requisição. As requisições `GET /health*` ficam fora do APM.

3. **Logs estruturados nas APIs**
   - Serilog (`Serilog.AspNetCore` + `Serilog.Expressions`) com `ExpressionTemplate`, configurado em `Extensions/LoggingExtension.cs` de cada API: 1 JSON por linha no stdout.
   - Chaves que o Datadog reconhece sem pipeline customizado: `timestamp`, `level`, `message`, `evento`, `logger.name`, `dd.*` (`dd.trace_id`, `dd.span_id`, `dd.service`, `dd.env`, `dd.version`) e `error.*` (`error.kind`, `error.message`, `error.stack`).
   - 1 log por requisição (`UseSerilogRequestLogging`), com `RouteTemplate`. As requisições a `/health*` não geram log, nem o `503` do `/health/ready` com o banco fora: o `HealthCheckService` já registra o resultado `Unhealthy` como erro.
   - O nível de log fica na seção `Serilog` do `appsettings.json` (variáveis `Serilog__*` no `docker-compose.yml` e nos ConfigMaps do repo K8S).

4. **Eventos de negócio**
   - Catálogo de `EventId` em `OrdemServicoLogEventos.cs` (projeto UseCases): `os.criada`, `os.status_alterado`, `os.encerrada`, `os.orcamento_gerado`, `os.orcamento_enviado`, `os.falha` e `integracao.falha`. Os middlewares globais emitem `http.erro_nao_tratado`, e o `Program.cs` das APIs com migrations emite `app.migracao_falhou`.
   - Os eventos de OS saem do use case e do controller de ordens de serviço, compartilhados pelas 5 APIs, via `ILogger<T>`. OS encerrada é a OS entregue (status `Entregue`).
   - O catálogo, as propriedades e os valores permitidos são o contrato com o Terraform do repo K8S e estão em [docs/observabilidade.md](../observabilidade.md).

5. **Métricas de negócio derivadas dos logs**
   - 5 métricas de log (log-based metrics) criadas por Terraform: `oficina.os.criadas`, `oficina.os.encerradas`, `oficina.os.tempo_no_status`, `oficina.os.falhas` e `oficina.integracao.erros`.
   - Sem DogStatsD nem código de métricas nas APIs.

6. **Health checks e uptime**
   - Novo `/health/ready`, anônimo, com check do banco (`AddDbContextCheck`, tag `ready`), consultado só pelo `http_check` do Agent.
   - O `/health` não muda: continua sem depender do banco e segue usado pelas probes do Kubernetes, pelo NLB, pelo API Gateway e pelo `HEALTHCHECK` dos Dockerfiles. O `readinessProbe` também não muda.
   - Uptime externo: 5 testes Synthetics em `GET /<api>/health` pelo API Gateway e 1 SLO de 99% em 7 dias.

7. **Monitores e dashboard**
   - 10 monitores: latência p95 (R1); CPU e memória acima do limit, CrashLoopBackOff, restarts e nós prontos (R2); réplicas indisponíveis e `http.can_connect` (R3); 5xx por rota e erros de integração (R4).
   - 1 dashboard com seções de Ordens de Serviço, APIs, Kubernetes, Disponibilidade e logs de erro recentes (R7).

8. **Conta e liga/desliga**
   - Trial de 14 dias do Datadog, sem cartão, no site padrão US1 (`datadoghq.com`), informado na variable `DD_SITE` do repo K8S.
   - Variable `DD_ENABLED` do repo K8S (padrão `true`): com `false`, o próximo apply do addons (Bootstrap do repo K8S) remove o Agent e todos os recursos do Datadog, sem mudar código.

**Consequências**:
- ✅ Atende R1 a R7 com uma única ferramenta e sem alterar os Dockerfiles: o tracer é injetado no cluster e os logs saem no stdout.
- ✅ Logs e traces ficam ligados dentro das APIs: do log se abre o trace da requisição ("View trace") e, do trace, os logs dela.
- ✅ Métricas de negócio sem código de métricas: mudar uma métrica, um monitor ou o dashboard é mudar Terraform no repo K8S, sem novo build das imagens.
- ✅ As camadas internas continuam dependendo só de `ILogger<T>`; o Serilog fica nos projetos das APIs.
- ✅ O mesmo JSON sai localmente (`docker compose`), só sem o objeto `dd`, porque não há tracer.
- ✅ A infraestrutura do Datadog é código, criada e destruída com o ambiente pelas pipelines que já existem no repo K8S, sem job novo.
- ✅ Liga/desliga por `DD_ENABLED`, sem mudar código: serve para o fim do trial ou para um problema na conta.
- ⚠ **Trial de 14 dias**: custo zero e sem cartão, com APM, logs e synthetics liberados. Os 14 dias contam do cadastro, então a conta só deve ser criada quando o ambiente subir para a demonstração. Depois, a organização cai no plano Free: só infraestrutura (até 5 hosts, retenção de 1 dia), **sem APM, logs e synthetics**, e os widgets de negócio (métricas de log) param de atualizar. Antes de o trial acabar, desligar com `DD_ENABLED=false` + Bootstrap do repo K8S e K8s Apply com `restart-pods=true` (ou rodar o Destroy); passo a passo no `infra/DATADOG.md` do repo K8S. O plano pago custaria cerca de US$ 110–150/mês: nunca cadastrar cartão.
- ⚠ **Capacidade**: o Datadog soma 3 pods (2 agents e 1 cluster-agent) e cerca de 0,9 GiB de requests de memória. O cluster passa de 12 para 15 pods, de 22 possíveis (pico de 20 no rollout restart das 5 APIs), com cerca de 1,9 GiB de requests de memória em regime (2,5 GiB no restart) contra ~2,9 GiB alocáveis. Cabe, mas apertado. A contingência (`t3.medium`) substitui o node group, o que é disruptivo.
- ⚠ **HPA**: com o tracer, a memória das APIs passaria dos ~102Mi (80% de 128Mi) e o HPA escalaria até faltar pod. O alvo de memória dos HPAs sobe de 80% para 150%, com os mesmos requests. Validar depois do deploy e, se ainda escalar à toa, deixar só CPU.
- ⚠ **Segurança das chaves**: API Key e Application Key ficam nos GitHub Secrets do repo K8S (`DD_API_KEY` e `DD_APP_KEY`) e chegam ao Terraform como variáveis sensíveis. A API Key vai ao chart por `set_sensitive`, que a esconde do plan, mas fica gravada no state do `infra/addons` (bucket S3 com criptografia e sem acesso público) e num Secret do namespace `datadog`. A Application Key é **não-escopada** (tem as permissões do usuário que a criou), porque é a credencial do provider Terraform. Revogar as duas ao fim do estudo. `DD_ALERT_RECIPIENTS` também é secret, porque o repo é público e o plan imprime as mensagens dos monitores.
- ⚠ **Dados pessoais (PII)**: os logs só levam GUIDs e valores fixos. CPF/CNPJ, e-mail, nome, placa e JWT nunca são logados. O `Motivo` de `os.falha` é a mensagem da exceção de domínio, que não traz dados pessoais. Os ids (`OrdemServicoId`, `ClienteId`, `VeiculoId`) são atributos de log, nunca tags de métrica. Cada novo ponto de log precisa seguir essa regra, conferida na revisão de código.
- ⚠ **Chaves inválidas ou trial vencido**: o plan, o apply e o destroy do `infra/addons` falham. O Destroy da foundation roda mesmo assim, para o EKS não ficar ligado cobrando. A recuperação (renovar as chaves, `DD_ENABLED=false` ou `terraform state rm`) está no `infra/DATADOG.md` do repo K8S.
- ⚠ **Sem transação no débito de estoque**: limitação pré-existente (ADR-012), fora do escopo. A aprovação agora valida o status antes de verificar e debitar o estoque, mas o débito ainda grava insumo por insumo. Uma falha no meio (estoque consumido por outra OS ou banco indisponível) deixa parte dos insumos debitada e a OS sem aprovação. O caso fica visível (`os.falha` ou `http.erro_nao_tratado`), mas não é corrigido por esta decisão.
- ⚠ **Métricas de log sem backfill**: só contam os eventos ingeridos depois que a métrica existe.

**Limitações**:
- **R6 só dentro das APIs**: o API Gateway e as Lambdas (authorizer e `POST /auth/token`) não são rastreados, então o trace começa no pod. A integração AWS/CloudWatch exigiria IAM role e permissões que o usuário `terraform` não tem, e a Lambda que consulta o banco não tem saída para a internet.
- **Synthetics fora do authorizer**: `GET /<api>/health` é uma rota pública do API Gateway, que não passa pelo Lambda authorizer. Os testes medem a disponibilidade das APIs pelo API Gateway, não a do authorizer nem a do Auth0.
- **Sem versão**: as imagens usam `:latest` e não há `DD_VERSION`, então `dd.version` não aparece e o APM não compara versões.
- **Sem pipeline de logs nem facets customizados**: as chaves do JSON já são reconhecidas pelo Datadog, e as métricas e os monitores filtram por atributos, que não precisam de facet.

**Alternativas Rejeitadas**:
- Datadog Operator:
  - rejeitada por somar 1 pod e CRDs num cluster já no limite, sem ganho sobre o chart para um único Agent.
- EKS add-on do Datadog:
  - rejeitada por exigir passos manuais fora do Terraform (assinatura no AWS Marketplace) para entregar o mesmo Agent.
- Integração AWS (CloudWatch) no lugar do Agent:
  - rejeitada por trazer só métricas do CloudWatch, sem APM, logs dos pods ou métricas de pod, e por exigir IAM role cross-account.
- Tracer embutido na imagem (pacote NuGet ou instalação no Dockerfile):
  - rejeitada por exigir mudança nos 5 Dockerfiles ou no código e novo build a cada versão do tracer; a Single Step Instrumentation injeta o tracer no cluster.
- `CompactJsonFormatter` do Serilog:
  - rejeitada porque grava chaves próprias (`@t`, `@mt`, `@l`, `@x`) e `dd_trace_id` em vez de `dd.trace_id`. Exigiria um pipeline de logs no Datadog para reconhecer data, status, erro e trace; o `ExpressionTemplate` já grava as chaves reservadas.
- DogStatsD (métricas enviadas pelo código):
  - rejeitada por exigir código de métricas em cada ponto, um cliente novo e acesso dos pods ao Agent; as métricas de log derivam as mesmas séries dos eventos que já são logados.
- `readinessProbe` em `/health/ready` (dependente do banco):
  - rejeitada porque, com o banco fora, todos os pods sairiam do Service ao mesmo tempo e o tráfego cairia inteiro. Também acoplaria a ordem de deploy entre os repos APP e K8S; o `/health/ready` é usado só pelo `http_check` do Datadog.

**Relação com Outras ADRs**:
- **ADR-001**: o Serilog e a configuração de logging ficam na API (composition root, `Extensions/LoggingExtension.cs`). Application (UseCases) e Controllers usam só a abstração `ILogger<T>`, sem conhecer Serilog nem Datadog.
- **ADR-013**: as duas camadas de tratamento de erros viram a fonte dos eventos de falha. Os erros esperados, tratados nos controllers, geram `os.falha`. O middleware global registra os inesperados como `http.erro_nao_tratado`, com `error.*`, `RouteTemplate` e `Integracao=postgres` quando o banco está indisponível, sem expor detalhes na resposta.
