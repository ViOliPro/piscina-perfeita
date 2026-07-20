# Documentação Técnica — Piscina Perfeita

_Gerada a partir do estado real do código em 19/07/2026 (v1.3.0 + patches de EstoqueIdeal, hardening de segurança e importação de dados). Para a visão original de produto e o escopo aspiracional do projeto, ver [`Documentacao de contexto.md`](./Documentacao%20de%20contexto.md). Para o modelo de dados detalhado, ver [`DER.md`](./DER.md). Para o plano de evolução, ver [`Roadmap.md`](../Roadmap.md)._

---

## 1. Visão geral

Piscina Perfeita é um sistema de gestão para empresas/condomínios que administram piscinas — controle de estoque de produtos químicos e de limpeza, registro de análises de água, aplicação de produtos nas piscinas e histórico de movimentação. É multi-tenant: uma mesma instalação atende vários "Locais" (condomínios/clientes) de forma isolada.

## 2. Stack e arquitetura

| Camada | Tecnologia |
|---|---|
| Frontend | React + Vite, servido via Nginx em produção |
| Backend | ASP.NET Core Web API, .NET 10 |
| ORM | Entity Framework Core (Code-First Migrations), convenção de nomes via `EFCore.NamingConventions` (colunas em minúsculo no Postgres) |
| Banco de dados | PostgreSQL, hospedado na **Neon** (serverless, com autosuspend e pooler PgBouncer) |
| Autenticação | JWT Bearer + BCrypt para hash de senha |
| Deploy | Render (containers via Docker/Blueprint), Caddy como proxy TLS |

Arquitetura desacoplada clássica: SPA React consumindo uma API REST stateless. Sem sessão de servidor — todo o estado de autenticação/autorização vem do JWT em cada requisição.

## 3. Multi-tenancy e autorização

### 3.1. Isolamento por `LocalId`
Ver detalhes completos em [`DER.md`](./DER.md#isolamento-multi-tenant-localid). Resumo: todo `SELECT` em tabelas que pertencem a um Local é automaticamente filtrado pelo EF Core via **global query filter**, usando o claim `local_id` do JWT da requisição. Não existe filtro manual espalhado pelos services — é uma trava centralizada no `PiscinaPerfeitaContext`.

### 3.2. Dois níveis de papel
- **`Role`** (`Usuario.Role`): `SuperAdmin` (enxerga todos os Locais, ignora o filtro) ou `Usuario` (comum).
- **`Perfil`** (`UsuarioLocal.Perfil`, por vínculo Usuário↔Local): `Administrador`, `Operador` ou `Visualizador`. Um usuário pode ter perfis diferentes em Locais diferentes.

### 3.3. JWT
Claims emitidos no login (`AccountService`): `NameIdentifier` (Id do usuário), `Email`, `Role`, `Name`, `local_id` (Local ativo na sessão) e `perfil` (Perfil nesse Local). Trocar de Local ativo (`POST /api/account/switchlocal`) reemite o token com `local_id`/`perfil` atualizados.

### 3.4. Autorização por endpoint
Todos os controllers têm `[Authorize]` no nível de classe. Exceções explícitas: `POST /api/account/login` (`[AllowAnonymous]`). Ações administrativas sensíveis usam `[Authorize(Roles = "SuperAdmin")]` — hoje isso se aplica à exclusão de usuário (`DELETE /api/usuarios/{id}`), porque o service ainda não valida escopo por Local nessa operação.

## 4. Módulos e regras de negócio

### 4.1. Locais, Piscinas, Usuários
- Um `Local` agrupa Piscinas, Depósitos, Produtos e todo o restante dos dados operacionais.
- Um `Usuario` se vincula a um ou mais Locais via `UsuarioLocal`, cada vínculo com seu próprio `Perfil` e status `Ativo`.

### 4.2. Análises de água
Registro de medições químicas de uma Piscina (`Ph`, `CloroLivre`, `Alcalinidade`, `Temperatura`), todos os campos numéricos opcionais — permite salvar leitura parcial. Uma Análise pode motivar uma Aplicação de Produto subsequente.

### 4.3. Estoque
Cada combinação Produto+Depósito tem um registro em `Estoque` com três quantidades independentes:
- `QuantidadeAtual` — saldo físico corrente.
- `QuantidadeMinima` — ponto de reposição/estoque de segurança (abaixo disso, é hora de agir).
- `EstoqueIdeal` — par level (até quanto reabastecer numa compra). Adicionado na v1.4.1 complementando o `QuantidadeMinima`, que sozinho não indicava quanto comprar.

**Cálculo do pedido de orçamento** (front, `Estoque.jsx`): quando `EstoqueIdeal` e `QuantidadeMinima` estão preenchidos, `qtdSugerida = EstoqueIdeal - QuantidadeAtual`. Para estoques ainda não configurados (qualquer um dos dois `null`), cai num fallback heurístico baseado nos limiares fixos `ESTOQUE_LIMITES` (`BAIXO=5`, `ATENCAO=15`).

### 4.4. Movimentações de estoque
Toda alteração de saldo gera um registro em `MovimentacaoEstoque` com um `TipoMovimentacao` que determina o sinal da operação — tabela completa em [`DER.md`](./DER.md#movimentacoesestoque-é-o-histórico-central). O endpoint `POST /api/movimentacoes/contagem-inventario` faz o fechamento de inventário físico (ajusta o saldo pra bater com a contagem real, gerando um `AjusteInventario` com a diferença).

### 4.5. Aplicação de produto em piscina
Ao aplicar um produto químico numa Piscina (`AplicacaoProduto`), o sistema:
1. Registra a aplicação (quantidade, unidade, piscina, produto, depósito de origem).
2. Gera automaticamente uma `MovimentacaoEstoque` (tipo `Aplicacao`) que dá baixa no `Estoque` correspondente.
3. Opcionalmente vincula a uma `Analise` que motivou a aplicação (ex: pH baixo → aplicação de elevador de pH).

A conversão de unidade (ex: produto cadastrado em L, aplicação lançada em mL) é feita automaticamente na baixa do estoque.

## 5. Segurança

Hardening aplicado (v1.4.1), documentado em detalhe no histórico de commits da branch `security/hardening-usuarios-cors-ratelimit`:

- **CORS**: `AllowAnyOrigin` só é aceito com `ASPNETCORE_ENVIRONMENT=Development`. Fora disso, a API recusa subir se `Cors:AllowedOrigins` não estiver configurado (fail-fast em vez de abrir geral silenciosamente).
- **Rate limiting**: fixed window no login — 5 tentativas/min em produção, 1000/min em Development.
- **JWT**: `RequireHttpsMetadata=true` fora de Development.
- **Handler global de exceção**: qualquer erro não tratado retorna uma mensagem genérica ao cliente (nunca o detalhe interno/stack trace), logando o erro completo só no servidor.
- **Headers de segurança**: `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`.
- **Senhas**: BCrypt (nunca SHA/MD5).
- **SQL Injection**: não aplicável — todo acesso a dados passa por LINQ/EF Core, que parametriza nativamente. Não há SQL raw/concatenado no projeto.
- **CSRF**: risco estruturalmente baixo — a API usa Bearer token no header, não cookie de sessão, então o vetor clássico de CSRF (o navegador reenviando cookie automaticamente) não se aplica.

### Débitos de segurança conhecidos (não implementados ainda)
- Nenhum controller usa `[Authorize(Roles=...)]` de forma granular além do `Delete` de usuário — um `Perfil=Operador` autenticado consegue chamar qualquer endpoint de qualquer controller, inclusive ações que deveriam ser só de Administrador.
- `Update`/`GetById`/`Delete` de usuário não validam se o usuário logado pertence ao mesmo Local do usuário-alvo (risco de IDOR entre Locais diferentes, mitigado parcialmente restringindo `Delete` a `SuperAdmin`).

## 6. API — mapa de controllers

| Controller | Rota base | Autorização |
|---|---|---|
| `AccountController` | `/api/account` | `login` público; `switchlocal` autenticado |
| `LocaisController` | `/api/locais` | autenticado |
| `UsuariosController` | `/api/usuarios` | autenticado; `Delete` restrito a `SuperAdmin` |
| `UsuariosLocaisController` | `/api/usuarioslocais` | autenticado |
| `PiscinasController` | `/api/piscinas` | autenticado |
| `AnalisesController` | `/api/analises` | autenticado |
| `DepositosController` | `/api/depositos` | autenticado |
| `ProdutosController` | `/api/produtos` | autenticado |
| `EstoquesController` | `/api/estoques` | autenticado |
| `MovimentacoesController` | `/api/movimentacoes` | autenticado (+ `contagem-inventario`) |
| `AplicacoesProdutoController` | `/api/aplicacoesproduto` | autenticado |

Todos seguem o padrão REST convencional (`GET`/`GET {id}`/`POST`/`PUT {id}`/`DELETE {id}`), com exceção das rotas extras citadas acima.

## 7. Frontend

Módulos (`PiscinaPerfeita.Front/src/modules/`): `auth`, `dashboard`, `locais`, `usuarios`, `piscinas`, `analises`, `depositos`, `produtos`, `estoque`, `movimentacoes`, `aplicacoes`, `inventario`, `onboarding`. PWA com manifest (instalável, sem service worker ativo hoje). Layout responsivo com navegação dedicada para mobile (drawer lateral + bottom nav), prioridade confirmada do produto já que a maioria dos usuários acessa por celular.

## 8. Deploy e infraestrutura

- **Render**: hospeda os containers da API (.NET) e do Frontend (Nginx estático), via Blueprint (`render.yaml`). `ASPNETCORE_ENVIRONMENT` é `Production` por padrão no `Dockerfile` — só vira `Development` se explicitamente setado no `.env` local, o que relaxa CORS/rate-limit pra facilitar teste manual sem afetar produção.
- **Neon**: Postgres serverless. Usa endpoint `-pooler` (PgBouncer) com `SSL Mode=VerifyFull` e `Channel Binding=Require` — configuração validada como estável após o incidente de julho/2026 (ver histórico de commits de `docker-compose.yml`/`.env.example`).
- **Docker Compose local**: replica o ambiente de produção pra teste (API + front + Postgres local + Caddy), com toggles de ambiente documentados em `.env.example`.

## 9. Convenções de código relevantes

- Migrations do EF Core devem ser geradas com `dotnet ef migrations add` no ambiente com SDK — migrations escritas à mão (sem `.Designer.cs`/atualização do `ModelSnapshot`) quebram `dotnet ef database update`.
- Nomes de coluna no Postgres são sempre minúsculos (convenção do `EFCore.NamingConventions`), mesmo que o model C# use PascalCase.
- Valores de enum já gravados no banco (`Tipo.Entrada=0`, `Tipo.Saida=1`) nunca são renumerados — extensões do enum sempre entram com número novo no final.
