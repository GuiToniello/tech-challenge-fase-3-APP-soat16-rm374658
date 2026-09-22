# ADR-005: Separação de Contratos em Commands/Queries/ViewModels

**Status**: Aceita

**Contexto**:
Necessidade de separar modelos de entrada (commands/queries) de saída (viewmodels), evitando acoplamento entre camadas e permitindo evolução independente dos contratos.

**Decisão**:
- **Commands**: DTOs que representam ações (criar, atualizar, deletar). Ficam no projeto **Application (UseCases)**.
- **Queries**: DTOs que representam buscas (listar, obter por ID). Ficam no projeto **Application (UseCases)**.
- **ViewModels**: DTOs que representam resposta ao cliente. Ficam no projeto **Application (UseCases)**.
- API consome esses contratos, não cria seus próprios.
- Não há duplicação ou alias de contratos na API.

**Consequências**:
- ✅ Contrato estável entre API e Application (UseCases).
- ✅ Mudanças internas do domínio não quebram API.
- ✅ Possibilita validação de entrada no command.
- ⚠ Adiciona classes DTOs (mais linhas de código).

**Estrutura**:
```
UseCases/
├── Features/Clientes/
│   ├── Commands/
│   │   ├── CriarClienteCommand.cs
│   │   ├── AtualizarClienteCommand.cs
│   │   └── ExcluirClienteCommand.cs
│   ├── Queries/
│   │   ├── ObterClientePorIdQuery.cs
│   │   └── ListarClientesQuery.cs
│   └── ViewModels/
│       └── ClienteViewModel.cs
```
