# ADR-001: Clean Architecture com Folder-by-Feature

**Status**: Aceita (atualizada)

**Contexto**:
A arquitetura inicialmente descrita como N-Layered evoluiu para um modelo aderente a **Clean Architecture**, mantendo o princípio de organização por **folder-by-feature**.

Com a evolução do projeto, a separação de responsabilidades passou a ser guiada por anéis de dependência e pela regra de inversão de dependência, reduzindo acoplamento entre regras de negócio e detalhes de framework/infraestrutura.

**Decisão**:
Adotar explicitamente **Clean Architecture** com os seguintes papéis:


- **Entities** (`TechChallenge.Oficina.Entities.csproj`):
	Entidades, Value Objects, exceções e regras de negócio puras. Não depende de infraestrutura nem de frameworks.


- **Application (UseCases)** (`TechChallenge.Oficina.UseCases.csproj`):
	Casos de uso, contratos (commands/queries/viewmodels), orquestração de fluxo e interfaces de portas de saída (Gateways) para dependências externas. Depende apenas do domínio.


- **Controllers** (`TechChallenge.Oficina.Controllers.csproj`):
	Adaptadores de entrada/saída da Clean Architecture, incluindo controllers (internos da arquitetura) e mapeamentos entre transporte HTTP e casos de uso.


- **Infraestrutura Técnica** (`TechChallenge.Oficina.Infra.csproj`, `TechChallenge.Oficina.DB.csproj`, `TechChallenge.Oficina.Email.csproj`):
	Implementações concretas de persistência, e-mail, configuração e integrações com bibliotecas/frameworks externos.

- **API (Composition Root)** (`TechChallenge.Oficina.Monolith.API.csproj`):
	Ponto de entrada da aplicação, responsável por configuração de middleware, DI, endpoints (Minimal APIs) e composição dos módulos.

**Regra de Dependência**:
- Dependências apontam para dentro (camadas externas dependem das internas).
- Regras de negócio (Entities/Application) não conhecem detalhes de infraestrutura.
- Implementações concretas são conectadas no composition root via injeção de dependência.

**Organização**:
Cada projeto mantém estrutura **folder-by-feature** (ex.: `Features/Clientes`, `Features/OrdensServico`, etc.), favorecendo coesão funcional sem quebrar os limites arquiteturais dos anéis.

**Consequências**:
- Maior isolamento do núcleo de negócio contra mudanças tecnológicas.
- Testabilidade aprimorada por separação clara entre casos de uso e adaptadores/infra.
- Melhor evolução incremental do sistema por feature, mantendo fronteiras arquiteturais.
- Maior disciplina de arquitetura necessária para evitar violação de dependências entre anéis.
