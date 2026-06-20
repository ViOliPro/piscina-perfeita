# ESPECIFICAÇÃO DE SOFTWARE

## Projeto

Pool Manager - Sistema de Gestão e Monitoramento de Piscinas

Versão: 1.0

Autor: Vinicius Oliveira

---

# 1. VISÃO GERAL

## Objetivo

Desenvolver uma plataforma web e mobile para monitoramento e controle de piscinas, permitindo registrar análises químicas, controlar estoque de produtos, acompanhar custos operacionais e gerar recomendações de tratamento.

O sistema deverá atender inicialmente proprietários de piscinas residenciais, podendo futuramente ser expandido para síndicos, empresas de manutenção e administradoras de condomínios.

---

# 2. OBJETIVOS DO NEGÓCIO

O sistema deverá permitir:

- Registrar análises da água.
- Registrar aplicações de produtos químicos.
- Controlar estoque.
- Monitorar custos.
- Emitir alertas.
- Gerar histórico de manutenção.
- Auxiliar na tomada de decisões.

---

# 3. PERFIS DE USUÁRIO

## Proprietário

Pode:

- Cadastrar piscinas.
- Registrar medições.
- Consultar relatórios.
- Controlar estoque.

## Técnico

Pode:

- Registrar medições.
- Registrar aplicações.
- Consultar histórico.

## Administrador

Pode:

- Gerenciar usuários.
- Gerenciar permissões.
- Consultar todas as informações.

---

# 4. REQUISITOS FUNCIONAIS

## RF001 - Cadastro de Usuário

O sistema deve permitir:

- Nome
- E-mail
- Senha

Validações:

- E-mail único.
- Senha mínima de 8 caracteres.

---

## RF002 - Autenticação

O sistema deve permitir:

- Login
- Logout
- Recuperação de senha

Tecnologia:

JWT Authentication

---

## RF003 - Cadastro de Piscina

Campos:

- Nome
- Volume em litros
- Profundidade média
- Tipo de revestimento
- Tipo de tratamento

---

## RF004 - Registro de Análises

Campos:

- Data
- pH
- Cloro Livre
- Alcalinidade
- Temperatura
- Observações

---

## RF005 - Registro de Produtos Aplicados

Campos:

- Produto
- Quantidade
- Unidade
- Custo
- Data

---

## RF006 - Controle de Estoque

Entradas:

- Compra de produtos

Saídas:

- Aplicações registradas

O estoque deve ser atualizado automaticamente.

---

## RF007 - Dashboard

Exibir:

- Último pH
- Último Cloro
- Última Alcalinidade
- Estoque atual
- Custos do mês

---

## RF008 - Histórico

Permitir consulta por:

- Data
- Piscina
- Produto

---

## RF009 - Relatórios

Gerar:

- Consumo de produtos
- Evolução do pH
- Evolução do Cloro
- Evolução da Alcalinidade
- Gastos mensais

---

## RF010 - Alertas

Exibir alertas quando:

- pH < 7.0
- pH > 7.8
- Cloro < 1 ppm
- Cloro > 5 ppm
- Alcalinidade fora da faixa configurada

---

# 5. REQUISITOS NÃO FUNCIONAIS

## RNF001

Sistema responsivo.

## RNF002

Compatível com dispositivos móveis.

## RNF003

Tempo máximo de resposta inferior a 2 segundos.

## RNF004

Utilizar HTTPS.

## RNF005

Armazenamento seguro de senhas.

Hash:

BCrypt

---

# 6. ARQUITETURA

Frontend:

React + Vite

Backend:

ASP.NET Core Web API

Banco:

PostgreSQL

Autenticação:

JWT

Hospedagem:

Frontend: Vercel

Backend: Railway ou Render

Banco: PostgreSQL Railway

---

# 7. MODELAGEM DO BANCO

## Tabela Usuarios

Id
Nome
Email
SenhaHash
DataCadastro

---

## Tabela Piscinas

Id
UsuarioId
Nome
VolumeLitros
ProfundidadeMedia
TipoRevestimento
TipoTratamento

---

## Tabela Analises

Id
PiscinaId
DataAnalise
Ph
CloroLivre
Alcalinidade
Temperatura
Observacoes

---

## Tabela Produtos

Id
Nome
UnidadeMedida

---

## Tabela Estoque

Id
PiscinaId
ProdutoId
QuantidadeAtual

---

## Tabela MovimentacoesEstoque

Id
PiscinaId
ProdutoId
TipoMovimentacao
Quantidade
Valor
DataMovimentacao

---

# 8. ENDPOINTS DA API

## Autenticação

POST /api/auth/login

POST /api/auth/register

POST /api/auth/refresh-token

---

## Piscinas

GET /api/piscinas

GET /api/piscinas/{id}

POST /api/piscinas

PUT /api/piscinas/{id}

DELETE /api/piscinas/{id}

---

## Análises

GET /api/analises

GET /api/analises/{id}

POST /api/analises

PUT /api/analises/{id}

DELETE /api/analises/{id}

---

## Produtos

GET /api/produtos

POST /api/produtos

PUT /api/produtos/{id}

DELETE /api/produtos/{id}

---

## Estoque

GET /api/estoque

POST /api/estoque/entrada

POST /api/estoque/saida

---

## Dashboard

GET /api/dashboard

---

# 9. TELAS

## Tela de Login

Campos:

- E-mail
- Senha

Botões:

- Entrar
- Criar Conta

---

## Dashboard

Cards:

- pH Atual
- Cloro Atual
- Alcalinidade Atual
- Estoque

Gráficos:

- pH
- Cloro
- Alcalinidade

---

## Cadastro de Piscina

Formulário de cadastro.

---

## Registro de Análise

Campos:

- pH
- Cloro
- Alcalinidade
- Temperatura

---

## Estoque

Lista de produtos.

Entradas e saídas.

---

## Relatórios

Filtros:

- Período
- Piscina

Exportação:

PDF
Excel

---

# 10. ROADMAP FUTURO

Versão 2.0

- Aplicativo React Native
- Notificações Push
- Fotos da piscina
- QR Code da piscina
- Compartilhamento com técnicos

Versão 3.0

- Inteligência Artificial
- Sugestões automáticas
- Reconhecimento visual da água
- Previsão de consumo

---

# 11. CRONOGRAMA

FASE 1

Infraestrutura

Duração:
1 semana

Entregas:

- [*] Banco PostgreSQL - 06/06/26 Todas as tabelas criadas no Postgress
- Projeto Backend
- Projeto Frontend

---

FASE 2

Autenticação

Duração:
1 semana

Entregas:

- Cadastro
- Login
- JWT

---

FASE 3

Piscinas

Duração:
1 semana

Entregas:

- CRUD completo

---

FASE 4

Análises

Duração:
2 semanas

Entregas:

- Cadastro
- Histórico

---

FASE 5

Produtos e Estoque

Duração:
2 semanas

Entregas:

- Controle de estoque
- Movimentações

---

FASE 6

Dashboard

Duração:
2 semanas

Entregas:

- Indicadores
- Gráficos

---

FASE 7

Deploy

Duração:
1 semana

Entregas:

- Sistema publicado

Tempo estimado total:

10 semanas
