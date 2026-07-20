# 🏊 Piscina Perfeita

Sistema de gestão para empresas e condomínios que administram piscinas — controle de estoque de produtos químicos e de limpeza, análises de água, aplicação de produtos e histórico completo de movimentação, com isolamento multi-tenant entre clientes (Locais).

## Funcionalidades

- **Multi-tenant por Local** — cada condomínio/cliente enxerga só os próprios dados, com controle de permissão por papel (`Administrador` / `Operador` / `Visualizador`).
- **Estoque com par level** — cada produto tem quantidade mínima e ideal por depósito; o sistema sugere automaticamente quanto pedir na próxima compra.
- **Histórico de movimentações** — toda entrada, saída, compra, aplicação, perda, descarte ou ajuste de inventário fica registrada e rastreável.
- **Análises de água** — registro de pH, cloro livre, alcalinidade e temperatura por piscina.
- **Aplicação de produto em piscina** — dá baixa automática no estoque do depósito de origem, com conversão de unidade.
- **Contagem de inventário** — fechamento de inventário físico com ajuste automático de diferenças.
- **PWA responsivo**, com navegação dedicada para mobile.

## Stack

- **Backend**: ASP.NET Core (.NET 10) + Entity Framework Core
- **Frontend**: React + Vite
- **Banco de dados**: PostgreSQL (Neon)
- **Autenticação**: JWT + BCrypt
- **Deploy**: Docker / Render

## Documentação

- [`Docs/Documentacao Tecnica.md`](./Docs/Documentacao%20Tecnica.md) — arquitetura, multi-tenancy, segurança, módulos e regras de negócio, mapa da API.
- [`Docs/DER.md`](./Docs/DER.md) — diagrama de entidade-relacionamento completo.
- [`Docs/Documentacao de contexto.md`](./Docs/Documentacao%20de%20contexto.md) — especificação original de requisitos e visão de produto.
- [`Roadmap.md`](./Roadmap.md) — histórico de versões entregues e plano de evolução.

## Rodando localmente

```bash
cp .env.example .env
# edite o .env com suas credenciais de banco e, se quiser testar sem
# as travas de produção (CORS aberto, rate limit alto), defina
# ASPNETCORE_ENVIRONMENT=Development

docker compose up --build
```

Consulte o `.env.example` para a lista completa de variáveis necessárias (connection string, JWT, credenciais do admin seed, domínio para CORS/Caddy).

## Estrutura do repositório

```
PiscinaPerfeita.Api/     # API ASP.NET Core
PiscinaPerfeita.Front/   # SPA React + Vite
Docs/                    # Documentação técnica e de produto
Roadmap.md               # Versões entregues e plano de evolução
docker-compose.yml       # Ambiente completo (API + front + Postgres + Caddy)
```
