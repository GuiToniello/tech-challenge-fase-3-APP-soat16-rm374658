# ADR-002: Domain-Driven Design e Entidades Ricas

**Status**: Aceita

**Contexto**:
Necessidade de manter lógica de negócio no domínio, evitando entidades anêmicas que delegam toda a validação para serviços.

**Decisão**:
- Entidade `Cliente` encapsula comportamentos como `AtualizarNomeCompleto()` e `AtualizarIdentificacao()`.
- Métodos construtores privados forçam uso de factory method `Cliente.Criar()`.
- Validações de domínio ocorrem na entidade, não na camada de use cases nem nos adaptadores de interface (controllers da arquitetura).
- Lançamento de `DomainException` para erros de negócio.

**Consequências**:
- ✅ Lógica de negócio protegida contra estados inválidos.
- ✅ Facilita testes da entidade independentes de infraestrutura.
- ✅ Código autodescritivo sobre regras de negócio.
- ⚠ Requer compreensão de DDD para manutenção.

**Exemplo**:
```csharp
public void AtualizarNomeCompleto(string nomeCompleto)
{
    if (string.IsNullOrWhiteSpace(nomeCompleto))
        throw new DomainException("O nome completo do cliente é obrigatório.");
    if (nomeCompleto.Trim().Length < 3)
        throw new DomainException("O nome completo do cliente deve possuir ao menos 3 caracteres.");
    NomeCompleto = nomeCompleto.Trim();
}
```
