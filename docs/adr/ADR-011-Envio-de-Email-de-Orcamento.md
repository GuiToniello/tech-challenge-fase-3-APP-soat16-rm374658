# ADR-011: Envio de E-mail de Orçamento com Resend

**Status**: Aceita

**Contexto**:
Necessidade de notificar o cliente por e-mail quando um orçamento de ordem de serviço estiver disponível, mantendo separação de camadas e baixo acoplamento com provedor externo.

Atualmente o fluxo exige:
- Uso de uma porta de aplicação para envio de e-mail (`IOrcamentoEmailSender`), sem dependência da aplicação para SDKs externos.
- Integração com provedor de e-mail transacional.
- Validação de configurações críticas (`ApiKey` e remetente) no momento do envio.
- Testabilidade da integração sem acoplar testes ao cliente real do provedor.

**Decisão**:
- O contrato de envio de orçamento por e-mail fica no projeto **Application (UseCases)** (`IOrcamentoEmailSender`), como porta de saída da aplicação.
- O caso de uso no projeto **Application (UseCases)** (`OrdemServicoUseCases.EnviarOrcamentoPorEmailAsync`) orquestra regras de negócio e dispara a porta de saída:
  - exige orçamento gerado;
  - exige cliente existente;
  - exige e-mail do cliente preenchido.
- A implementação concreta fica no projeto **Email** (`OrcamentoEmailSender`) e monta assunto/corpo HTML com dados da ordem de serviço e do orçamento.
- O provedor escolhido é o **Resend**, encapsulado por um adapter (`ResendClientAdapter`) por meio da interface interna `IResendClient`, isolando o SDK externo do restante da aplicação.
- O registro de DI em `AddInfraEmail(...)` centraliza a inicialização dos serviços de e-mail e permite subir a aplicação sem credenciais válidas de envio (modo degradado para essa funcionalidade).
- A validação de `ResendSettings.ApiKey` e `ResendSettings.FromEmail` ocorre em runtime no `OrcamentoEmailSender`, apenas quando o envio é solicitado.
- A API carrega `ResendSettings` via configuração (`appsettings`/environment variables) e inicializa o módulo de e-mail no startup.

**Consequências**:
- ✅ Mantém arquitetura em camadas: aplicação define a porta, infraestrutura implementa.
- ✅ Facilita troca futura de provedor (SendGrid, SES etc.) sem alterar regras de negócio dos projetos Application/Entities.
- ✅ Melhora testabilidade com mocks de `IOrcamentoEmailSender` e `IResendClient`.
- ✅ Permite executar a API sem bloquear o startup quando a funcionalidade de e-mail não estiver configurada.
- ⚠ Em caso de configuração ausente/inválida, a falha aparece em runtime ao tentar enviar orçamento por e-mail.
- ⚠ O corpo do e-mail está atualmente montado em string HTML na infraestrutura, exigindo cuidado de manutenção/layout.
- ⚠ Falhas de provedor externo podem impactar o envio síncrono do caso de uso (sem fila/retry nesta etapa).

**Alternativas Rejeitadas**:
- Chamar o SDK do Resend diretamente no projeto Application (UseCases):
  - rejeitada por acoplamento indevido da camada de aplicação com tecnologia externa.
- Colocar lógica de envio no projeto Entities:
  - rejeitada por violar responsabilidade da camada de domínio (integração externa).
- Colocar o contrato em Entities:
  - rejeitada no estado atual por manter o contrato próximo ao caso de uso que o consome e por simplificar evolução dos fluxos de aplicação.
- Envio assíncrono via fila já nesta fase:
  - rejeitada por aumento de complexidade operacional para o escopo atual; pode ser evolução futura.
