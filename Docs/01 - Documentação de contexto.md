# ESPECIFICAÇÃO DE REQUISITOS E ARQUITETURA DE SOFTWARE (ERAS)

## Projeto: Pool Manager — Sistema SaaS de Gestão, Consumo e Monitoramento de Piscinas Coletivas

**Versão:** 1.1.0 (Ciclo de Evolução Pós-MVP)  
**Autor:** Vinicius Oliveira

---

## 1. VISÃO GERAL E ESCOPO

### 1.1. Objetivo do Sistema

O **Pool Manager** é uma plataforma distribuída (Web e Mobile) projetada para a automação, controle químico, rastreabilidade de insumos e auditoria de consumo hídrico em parques aquáticos residenciais e corporativos. O ecossistema evoluirá de um gerenciador isolado para uma solução escalável de **Software as a Service (SaaS)** dedicada a síndicos profissionais, administradoras de condomínios, hotéis e empresas de manutenção especializada.

---

## 2. ARQUITETURA E MATRIZ DE DEPLOY

O sistema adota o padrão de arquitetura desacoplada baseada em APIs RESTful, garantindo isolamento de camadas e persistência relacional robusta.

- **Camada de Apresentação (Frontend):** React.js + Vite (SPA), TailwindCSS (Interface Responsiva).
- **Camada de Negócio e Serviços (Backend):** ASP.NET Core Web API (.NET 8).
- **Persistência de Dados (Database):** PostgreSQL.
- **Mapeamento Objeto-Relacional (ORM):** Entity Framework Core via _Code-First Migrations_.
- **Segurança e Sessão:** Autenticação baseada em Claims com Tokens JWT (JSON Web Tokens) e criptografia de credenciais via BCrypt.

### 2.1. Hospedagem e Infraestrutura

- **Frontend:** Vercel
- **Backend:** Railway / Render
- **Banco de Dados:** PostgreSQL (Railway)

---

## 3. MODELAGEM DO BANCO DE DADOS (DDL REVISADA)

### 3.1. Entidades do Sistema

#### Tabela: `Usuarios`

- `Id` (Guid, PK)
- `Nome` (Varchar(150))
- `Email` (Varchar(100), Unique)
- `SenhaHash` (Varchar(255))
- `DataCadastro` (DateTime)

#### Tabela: `Locais`

- `Id` (Guid, PK)
- `Nome` (Varchar(150))
- `Endereco` (Varchar(255))
- `Telefone` (Varchar(20))
- `Observacoes` (Text, Nullable)

#### Tabela: `UsuarioLocais` _(Matriz de Permissão/RBAC)_

- `Id` (Guid, PK)
- `UsuarioId` (Guid, FK -> Usuarios)
- `LocalId` (Guid, FK -> Locais)
- `Perfil` (Enum: Administrator, Operator, Viewer)

#### Tabela: `Piscinas`

- `Id` (Guid, PK)
- `LocalId` (Guid, FK -> Locais)
- `Nome` (Varchar(100))
- `VolumeLitros` (Integer)
- `ProfundidadeMedia` (Decimal(5,2))
- `TipoRevestimento` (Varchar(50))
- `TipoTratamento` (Varchar(50))

#### Tabela: `Analises`

- `Id` (Guid, PK)
- `PiscinaId` (Guid, FK -> Piscinas)
- `DataAnalise` (DateTime)
- `Ph` (Decimal(4,2), Nullable)
- `CloroLivre` (Decimal(4,2), Nullable)
- `Alcalinidade` (Decimal(5,2), Nullable)
- `Temperatura` (Decimal(4,1), Nullable)
- `Observacoes` (Text, Nullable)

#### Tabela: `Depositos`

- `Id` (Guid, PK)
- `LocalId` (Guid, FK -> Locais)
- `Nome` (Varchar(100))

#### Tabela: `Produtos`

- `Id` (Guid, PK)
- `CategoriaId` (Guid, FK -> Categorias)
- `Nome` (Varchar(100))
- `UnidadeMedida` (Varchar(20))

#### Tabela: `Estoque`

- `Id` (Guid, PK)
- `DepositoId` (Guid, FK -> Depositos)
- `ProdutoId` (Guid, FK -> Produtos)
- `QuantidadeAtual` (Decimal(10,2))
- `QuantidadeMinima` (Decimal(10,2))

#### Tabela: `MovimentacoesEstoque`

- `Id` (Guid, PK)
- `DepositoId` (Guid, FK -> Depositos)
- `ProdutoId` (Guid, FK -> Produtos)
- `TipoMovimentacao` (Enum: Entrada_Compra, Saida_Aplicacao, Ajuste_Inventario)
- `Quantidade` (Decimal(10,2))
- `DataMovimentacao` (DateTime)

#### Tabela: `Hidrometros`

- `Id` (Guid, PK)
- `LocalId` (Guid, FK -> Locais)
- `Identificador` (Varchar(50))

#### Tabela: `LeiturasHidrometro`

- `Id` (Guid, PK)
- `HidrometroId` (Guid, FK -> Hidrometros)
- `DataLeitura` (DateTime)
- `ValorLeitura` (Decimal(12,3))

---

## 4. MAPEAMENTO DE ENDPOINTS DA API (RESTFUL)

### 4.1. Módulo de Autenticação (`/api/auth`)

- `POST /api/auth/login` -> Autentica usuário e retorna token JWT.
- `POST /api/auth/register` -> Cria uma nova credencial no sistema.
- `POST /api/auth/refresh-token` -> Atualiza a expiração da sessão do cliente.

### 4.2. Módulo de Gestão Territorial (`/api/locais`)

- `GET /api/locais` -> Retorna locais vinculados às claims do usuário.
- `POST /api/locais` -> Registra um novo condomínio/sede.
- `GET /api/locais/{id}/subestrutura` -> Retorna árvore hierárquica completa (Piscinas, Depósitos, Hidrômetros).

### 4.3. Módulo de Insumos e Logística (`/api/estoque`)

- `GET /api/estoque/status-critico` -> Lista produtos com estoque baixo.
- `POST /api/estoque/inventario/ajuste` -> Realiza fechamento de inventário físico.

---

## 5. CATALOGAÇÃO DE REQUISITOS ESSENCIAIS

### 5.1. Requisitos Funcionais (RF)

- **RF001 - Controle de Escopo por Claims:** O sistema deve filtrar toda consulta HTTP baseando-se no `LocalId` do token JWT, impedindo vazamento de dados entre condomínios diferentes.
- **RF002 - Flexibilidade nas Análises:** O preenchimento dos parâmetros químicos em `Analises` deve ser opcional (Nullable), permitindo salvar medições parciais.
- **RF003 - Gatilho de Alerta Crítico:** O sistema disparará avisos se os parâmetros saírem da faixa operacional aceitável (pH < 7.0 ou > 7.8; Cloro < 1.0 ppm ou > 5.0 ppm).
- **RF004 - Rastreabilidade Automática:** Ao computar a aplicação de produto químico, o sistema deverá realizar o decremento automático proporcional no estoque correspondente.

### 5.2. Requisitos Não Funcionais (RNF)

- **RNF001 - Latência Operacional:** Consultas de endpoints críticos (como gráficos do dashboard) não devem exceder 500ms.
- **RNF002 - Estratégia de Migração:** Toda e qualquer alteração estrutural no banco PostgreSQL deve utilizar exclusivamente Entity Framework Core Migrations.
- **RNF003 - Segurança de Senhas:** Senhas devem obrigatoriamente ser hasheadas com o algoritmo adaptativo BCrypt.
