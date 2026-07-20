# ROADMAP DE EVOLUÇÃO E CRONOGRAMA DO PROJETO

## Projeto: Pool Manager — Sistema SaaS de Gestão de Piscinas

---

## 1. HISTÓRICO DE ENTREGAS (HISTÓRICO DO MVP)

### [CONCLUÍDO] Versão 1.0.0 — Mínimo Produto Viável (MVP)

- **Objetivo:** Primeiro sistema estável e utilizável em ambiente de produção.
- **Entregas Realizadas:**
  - Infraestrutura básica e banco de dados PostgreSQL estruturado.
  - Autenticação e controle de sessões via JWT.
  - CRUD completo para cadastro de piscinas.
  - Módulo de lançamento de análises básicas e controle manual de estoque.
  - Dashboard simples com indicadores de medição.
  - Deploy efetuado com sucesso (Vercel + Railway/Render).

---

## 2. ROADMAP EVOLUTIVO (FUTURO)

### Versão 0.9.0 / 1.1.0 — Refatoração da Base e Escopo Territorial

- **Objetivo:** Corrigir a modelagem e implementar o suporte a múltiplos Condomínios (`Local`).
- **Banco de Dados:**
  - [ x ] Remover `PiscinaId` da tabela `Estoque`.
  - [ x ] Remover `Valor` da tabela `MovimentacaoEstoque`.
  - [ x ] Tornar parâmetros de `Analise` opcionais (`Nullable`).
  - [ x ] Adicionar campo `QuantidadeMinima` ao estoque.
  - [ x ] Criar entidade `Local` e vincular `Piscinas` e `Estoque` a ela.
  - [ ] Gerar e rodar as Migrations correspondentes.
- **Backend & Frontend:**
  - [ x ] Ajustar as camadas de Services, Repositories e DTOs.
  - [ x ] Atualizar interfaces visuais de estoque e análises para lidar com campos nulos e com a nova lógica de Locais.

### Versão 1.2.0 — Controle de Permissões (RBAC)

- **Objetivo:** Permitir perfis dinâmicos de acesso.
- **Funcionalidade:** Implementação da tabela associativa `UsuarioLocal`. O mesmo usuário pode ser administrador em um condomínio e apenas operador ou visualizador em outro.

### Versão 1.3.0 — Racionalização de Almoxarifado e Estoque Crítico

- **Objetivo:** Evitar falta de insumos.
- **Funcionalidade:** Criação do conceito de `Categoria` de produtos (Limpeza, Elétrica, Jardinagem). Alertas visuais e notificações no dashboard para itens com estoque abaixo do nível de segurança.

- **Banco de dados:**
  - [ x ] Adicionar campo `Categoria` ao produto.
  - [ x ] Adicionar campo `Observacoes` ao produto.
  - [ x ] Adicionar campo `Fabricante` ao produto.
  - [ x ] Adicionar campo `Marca` ao produto.
  - [ x ] Criar Migrations das alteracoes.
  - [ x ] Atualizar o Front para refletir as mudancas.

### [CONCLUÍDO] Versão 1.4.0 — Múltiplos Depósitos Físicos

- **Objetivo:** Mapear a realidade logística de grandes condomínios.
- **Funcionalidade:** Criação da entidade `Deposito` vinculada ao `Local`. O estoque passa a pertencer a depósitos específicos (ex: "Almoxarifado Central" vs. "Quarto de Máquinas").

- **Banco de dados:**
- [ x ] Criacao da entidade Deposito. - Atributos: Id, Nome, LocalId (FK).
- [ x ] Vincular Deposito a tabela Estoque.
- [ x ] Gerar MIgration.
- [ x ] Atualizar o front.

### [CONCLUÍDO] Versão 1.4.1 — Estoque Ideal e Hardening Reativo

- **Objetivo:** Corrigir uma lacuna de segurança crítica encontrada em auditoria e refinar o cálculo de reposição de estoque. Trabalho reativo, fora da sequência original do roadmap.
- **Banco de dados:**
  - [ x ] Adicionar campo `EstoqueIdeal` (par level) à tabela `Estoque`, complementando o `QuantidadeMinima` já existente.
  - [ x ] Gerar Migration.
- **Backend:**
  - [ x ] Validação: `EstoqueIdeal` deve ser maior que `QuantidadeMinima` quando ambos preenchidos.
  - [ x ] `[Authorize]` no `UsuariosController` (estava 100% público — listar/criar/editar/apagar usuário sem autenticação).
  - [ x ] Corrigido bug de escalação de privilégio no Update de usuário (usuário comum conseguia se autopromover a SuperAdmin).
  - [ x ] CORS restrito fora de `Development` (antes liberava qualquer origem sempre).
  - [ x ] Rate limiting no login (força bruta sem limite antes).
  - [ x ] Handler global de exceção (evita vazar detalhe interno do banco pro cliente).
- **Frontend:**
  - [ x ] Campos "Estoque mínimo" e "Estoque ideal" obrigatórios no formulário de Estoque.
  - [ x ] Nova função de cálculo do pedido de orçamento usando `EstoqueIdeal - QuantidadeAtual`, com fallback pra heurística antiga em estoques ainda não configurados.
  - [ x ] Corrigido corte de conteúdo e vazamento da sidebar ao imprimir o pedido de orçamento.

### [EM ANDAMENTO] Versão 1.4.2 — Base de Qualidade e Documentação

- **Objetivo:** Ter terreno firme (dados reais, documentação, bugs mapeados) antes de empilhar feature nova em cima.
- **Tarefas:**
  - [ x ] Importar dados legados de planilhas (produtos/estoque) via script SQL.
  - [ x ] Documentação técnica completa + DER (diagrama de entidade-relacionamento).
  - [ ] Teste caixa-preta completo da aplicação, com revisão de regras de negócio.

### Versão 1.4.3 — Mobile First

- **Objetivo:** Garantir a experiência completa em celular antes de adicionar telas novas — maioria dos usuários acessa por mobile.
- **Tarefas:**
  - [ ] Revisão de layout em todas as telas no mobile (formulários grandes, navegação, listas).

### Versão 1.4.4 — Conta e Autenticação

- **Objetivo:** Fechar o ciclo de conta do usuário.
- **Tarefas:**
  - [ ] Reset de senha com verificação por e-mail.
  - [ ] Login via Google (OAuth).
  - [ ] Painel "Meu perfil" (dados do usuário, troca de senha).

### Versão 1.5.0 — Conciliação e Inventários Periódicos

- **Objetivo:** Mitigar perdas ou desvios de produtos.
- **Funcionalidade:** Tela para inserção de contagem física. O sistema calcula automaticamente a diferença com o estoque lógico e gera uma `MovimentacaoEstoque` do tipo "Ajuste".

### Versão 1.6.0 — Fluxo de Compras (Procurement)

- **Objetivo:** Automatizar o reabastecimento.
- **Funcionalidade:** Geração de solicitações de compra automáticas assim que o estoque atinge o nível crítico, guiando o administrador desde a aprovação até a entrada da nota fiscal.

### Versão 1.7.0 — Telemetria de Hidrômetros

- **Objetivo:** Economia de água e segurança predial.
- **Funcionalidade:** Módulo para registro de leituras de hidrômetros com geração de gráficos de consumo hídrico e disparo automático de alertas em caso de suspeita de vazamento.
- **Observação:** esta versão cobre o lançamento de dados do hidrômetro da Copasa mencionado no backlog — confirmar formato de leitura (manual vs. API da concessionária) quando chegar a vez.

### Versão 2.0.0 — Virada de Chave SaaS (Multi-Condomínio)

- **Objetivo:** Comercialização em larga escala.
- **Funcionalidade:** Painel "Super Admin" para gerenciamento de assinaturas e controle unificado. Um síndico profissional consegue alternar entre múltiplos condomínios de forma fluida.

### Versão 2.1.0 — Dashboard Gerencial e Inteligência de Dados (BI)

- **Objetivo:** Painel analítico de alto nível.
- **Funcionalidade:** Relatórios consolidados de gastos mensais, projeção de consumo de produtos químicos por sazonalidade (inverno vs. verão), ranking de funcionários mais ativos e análise financeira preditiva.
