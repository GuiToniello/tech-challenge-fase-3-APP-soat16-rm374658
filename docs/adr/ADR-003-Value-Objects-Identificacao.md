# ADR-003: Value Objects para Validação de Identificação (CPF/CNPJ)

**Status**: Aceita

**Contexto**:
Necessidade de validar CPF e CNPJ segundo regras específicas, reutilizar essa validação em múltiplos contextos, e manter a lógica centralizada no domínio.

**Decisão**:
- Criar `Value Objects` (VOs) `Cpf` e `Cnpj` que encapsulam validação individual.
- Value Object `IdentificacaoCliente` atua como composição desses VOs.
- Método `IdentificacaoCliente.Criar()` detecta o tipo (CPF ou CNPJ) automaticamente.
- VOs implementam `IEquatable<>` para comparação por valor.
- Validação inclui verificação de dígitos verificadores e normalização (remoção de caracteres especiais).

**Consequências**:
- ✅ Validação centralizada e reutilizável.
- ✅ Impossível criar identificação inválida no domínio.
- ✅ Type-safe: `IdentificacaoCliente` garante tipo válido.
- ✅ Normaliza entrada (CNPJ alfanumérico suportado).

**Exemplo de Normalização**:
```
"12.345.678/0001-95" → "12345678000195" (CNPJ)
"123.456.789-09"     → "12345678909"    (CPF)
"12345678000195"     → "12345678000195" (já normalizado)
```
