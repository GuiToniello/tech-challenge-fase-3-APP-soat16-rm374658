# ADR-013: Estratégia de Tratamento de Erros em Duas Camadas

**Status**: Aceita

**Contexto**:
A API possui dois pontos de tratamento de exceções com objetivos complementares:

- Camada de interface (endpoints Minimal APIs + adapters/controllers da arquitetura): tratamento de erros esperados de negócio para mapear respostas HTTP adequadas (4xx) e manter contrato funcional da API.
- Middleware global: tratamento de erros inesperados para evitar vazamento de detalhes internos, registrar incidentes e garantir resposta segura padrão (500).

Essa separação foi mantida por atender requisitos de robustez operacional e segurança observados em validações automatizadas (incluindo recomendações de hardening e não exposição de stack trace em ferramentas como OWASP ZAP).

**Decisão**:
Adotar e documentar explicitamente uma estratégia em duas camadas:

1. **Tratamento de erros esperados na borda da aplicação**
  - Realizado nos adapters/controllers da Clean Architecture, invocados pelos endpoints Minimal APIs.
   - Abrange exceções previsíveis de domínio e regras de negócio (ex.: `DomainException`, `KeyNotFoundException`).
   - Resultado: resposta HTTP semântica (400, 404 etc.) com mensagem controlada para o cliente.

2. **Tratamento de erros inesperados no middleware global**
   - Realizado por middleware no pipeline HTTP.
   - Abrange exceções não mapeadas localmente, falhas técnicas e cenários não previstos.
   - Resultado: resposta 500 genérica, sem detalhes sensíveis, com logging para investigação.

**Responsabilidades**:
- Adapters/controllers da Clean Architecture:
  - traduzem casos de erro de negócio conhecidos para o contrato HTTP.
  - não substituem o middleware global para falhas inesperadas.
- Middleware global:
  - atua como última barreira de proteção.
  - padroniza resposta de erro interno e reduz exposição de informações técnicas.

**Consequências**:
- ✅ Preserva semântica de negócio na API para erros esperados.
- ✅ Aumenta resiliência contra falhas inesperadas no runtime.
- ✅ Reduz risco de disclosure de detalhes internos (stack trace, tipos, caminhos).
- ✅ Alinha o comportamento de erro a práticas de segurança defensiva.
- ⚠ Exige disciplina para manter mapeamentos de exceção consistentes nos adapters/controllers da arquitetura.
- ⚠ Não substitui outros controles de segurança (rate limiting, validação de entrada, autenticação/autorização, observabilidade).

**Alternativas Rejeitadas**:
- Tratar tudo apenas no middleware global:
  - rejeitada por perder granularidade de erros de negócio e degradar contrato da API para respostas 500 genéricas.
- Tratar tudo apenas nos adapters/controllers da arquitetura:
  - rejeitada por deixar o sistema sem barreira final para exceções inesperadas e aumentar risco de exposição acidental.

**Relação com Outras ADRs**:
- **ADR-001**: reforça a separação de responsabilidades entre adaptadores de interface e composição/pipeline da API.
- **ADR-005**: preserva clareza de contratos na camada de aplicação e sua tradução para HTTP.
