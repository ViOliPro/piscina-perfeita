# Roadmap de Evolução e Cronograma do Projeto
## Piscina Perfeita — Sistema SaaS de Gestão de Piscinas

---

## 1. Histórico de Entregas

### [CONCLUÍDO] Versão 1.0.0 — Mínimo Produto Viável (MVP)
**Objetivo:** Primeiro sistema estável e utilizável em ambiente de produção.

- Infraestrutura básica e banco de dados PostgreSQL estruturado.
- Autenticação e controle de sessões via JWT.
- CRUD completo para cadastro de piscinas.
- Módulo de lançamento de análises básicas e controle manual de estoque.
- Dashboard simples com indicadores de medição.
- Deploy efetuado com sucesso (Render + Neon).

---

## 2. Roadmap Evolutivo

### [CONCLUÍDO] Versão 0.9.0 / 1.1.0 — Refatoração da Base e Escopo Territorial
**Objetivo:** Corrigir a modelagem e implementar o suporte a múltiplos Condomínios (Local).

**Banco de Dados:**
- [x] Remover `PiscinaId` da tabela Estoque.
- [x] Remover `Valor` da tabela MovimentacaoEstoque.
- [x] Tornar parâmetros de Análise opcionais (Nullable).
- [x] Adicionar campo `QuantidadeMinima` ao estoque.
- [x] Criar entidade `Local` e vincular Piscinas e Estoque a ela.
- [x] Gerar e rodar as Migrations correspondentes.

**Backend & Frontend:**
- [x] Ajustar as camadas de Services, Repositories e DTOs.
- [x] Atualizar interfaces visuais de estoque e análises para lidar com campos nulos e com a nova lógica de Locais.

### [CONCLUÍDO] Versão 1.2.0 — Controle de Permissões (RBAC)
**Objetivo:** Permitir perfis dinâmicos de acesso.

- Implementação da tabela associativa `UsuarioLocal`. O mesmo usuário pode ser administrador em um condomínio e apenas operador ou visualizador em outro.

### [CONCLUÍDO] Versão 1.3.0 — Racionalização de Almoxarifado e Estoque Crítico
**Objetivo:** Evitar falta de insumos.

- Criação do conceito de Categoria de produtos (Limpeza, Elétrica, Jardinagem). Alertas visuais e notificações no dashboard para itens com estoque abaixo do nível de segurança.

**Banco de dados:**
- [x] Adicionar campo `Categoria` ao produto.
- [x] Adicionar campo `Observacoes` ao produto.
- [x] Adicionar campo `Fabricante` ao produto.
- [x] Adicionar campo `Marca` ao produto.
- [x] Criar Migrations das alterações.
- [x] Atualizar o Front para refletir as mudanças.

### [CONCLUÍDO] Versão 1.4.0 — Múltiplos Depósitos Físicos
**Objetivo:** Mapear a realidade logística de grandes condomínios.

- Criação da entidade `Deposito` vinculada ao Local. O estoque passa a pertencer a depósitos específicos (ex: "Almoxarifado Central" vs. "Quarto de Máquinas").

**Banco de dados:**
- [x] Criação da entidade Deposito — Atributos: Id, Nome, LocalId (FK).
- [x] Vincular Deposito à tabela Estoque.
- [x] Gerar Migration.
- [x] Atualizar o front.

### [CONCLUÍDO] Versão 1.4.1 — Estoque Ideal e Hardening Reativo
**Objetivo:** Corrigir uma lacuna de segurança crítica encontrada em auditoria e refinar o cálculo de reposição de estoque.

**Banco de dados:**
- [x] Adicionar campo `EstoqueIdeal` (par level) à tabela Estoque, complementando o `QuantidadeMinima` já existente.
- [x] Gerar Migration.

**Backend:**
- [x] Validação: `EstoqueIdeal` deve ser maior que `QuantidadeMinima` quando ambos preenchidos.
- [x] `[Authorize]` no `UsuariosController` (estava 100% público).
- [x] Corrigido bug de escalação de privilégio no Update de usuário.
- [x] CORS restrito fora de Development.
- [x] Rate limiting no login.
- [x] Handler global de exceção.

**Frontend:**
- [x] Campos "Estoque mínimo" e "Estoque ideal" obrigatórios no formulário de Estoque.
- [x] Nova função de cálculo do pedido de orçamento usando `EstoqueIdeal - QuantidadeAtual`, com fallback para heurística antiga.
- [x] Corrigido corte de conteúdo e vazamento da sidebar ao imprimir o pedido de orçamento.

### [EM ANDAMENTO] Versão 1.4.2 — Base de Qualidade e Documentação
**Objetivo:** Ter terreno firme (dados reais, documentação, bugs mapeados) antes de empilhar feature nova em cima.

- [x] Importar dados legados de planilhas (produtos/estoque) via script SQL.
- [x] Documentação técnica completa + DER (diagrama de entidade-relacionamento).
- [ ] Teste caixa-preta completo da aplicação, com revisão de regras de negócio.

### Versão 1.4.3 — Mobile First
**Objetivo:** Garantir a experiência completa em celular antes de adicionar telas novas.

- [ ] Revisão de layout em todas as telas no mobile (formulários grandes, navegação, listas).

### Versão 1.4.4 — Conta e Autenticação
**Objetivo:** Fechar o ciclo de conta do usuário.

- [ ] Reset de senha com verificação por e-mail.
- [ ] Login via Google (OAuth).
- [ ] Painel "Meu perfil" (dados do usuário, troca de senha).
- [ ] Autenticação multifator (MFA) — opcional, priorizar para clientes empresariais.
- [ ] Refresh Token (renovação de sessão sem novo login).

### Versão 1.5.0 — Conciliação e Inventários Periódicos
**Objetivo:** Mitigar perdas ou desvios de produtos.

- Tela para inserção de contagem física. O sistema calcula automaticamente a diferença com o estoque lógico e gera uma `MovimentacaoEstoque` do tipo "Ajuste".
- *(ideia agregada)* Indicador de percentual de divergência/perda por depósito, alimentando o dashboard administrativo.

### Versão 1.6.0 — Fluxo de Compras (Procurement)
**Objetivo:** Automatizar o reabastecimento.

- Geração de solicitações de compra automáticas assim que o estoque atinge o nível crítico, guiando o administrador desde a aprovação até a entrada da nota fiscal.
- *(ideia agregada)* Catálogo de fornecedores com comparação de preços e histórico de preço por produto.
- *(ideia agregada)* Consumo médio e previsão de reposição com base em séries históricas.
- *(ideia agregada)* Alertas de produtos próximos do vencimento.

### Versão 1.7.0 — Telemetria de Hidrômetros
**Objetivo:** Economia de água e segurança predial.

- Módulo para registro de leituras de hidrômetros com geração de gráficos de consumo hídrico e disparo automático de alertas em caso de suspeita de vazamento.
- *Observação:* cobre o lançamento de dados do hidrômetro da Copasa mencionado no backlog — confirmar formato de leitura (manual vs. API da concessionária) quando chegar a vez.

### Versão 1.8.0 — Análises e Qualidade da Água *(nova, a partir das ideias)*
**Objetivo:** Melhorar o controle da qualidade da água antes da virada para SaaS.

- Histórico completo e evolução dos indicadores por piscina.
- Gráficos e comparação entre períodos / média mensal.
- Faixas ideais configuráveis por parâmetro, com alertas automáticos quando fora da faixa.
- IA sugerindo correções quando os parâmetros da água estiverem fora do ideal (ver seção de Inteligência, v2.2).

### Versão 1.9.0 — Relatórios *(nova, a partir das ideias)*
**Objetivo:** Entregar valor tangível e "fechar o ciclo" antes da comercialização.

- Exportação em PDF e Excel.
- Relatório mensal e anual.
- Custos por piscina / consumo por produto / produtos utilizados.
- Resumo executivo e histórico de análises consolidado.
- Geração automática de laudos técnicos em PDF, com espaço para assinatura digital do técnico responsável.

---

## 3. Preparação para Comercialização — Compliance e Documentos Legais

**Objetivo:** deixar o sistema juridicamente pronto para operar como produto comercial antes da Versão 2.0.0 (SaaS). Esta etapa é pré-requisito para abrir cadastro público, cobrar assinaturas e aceitar clientes empresariais (síndicos, administradoras).

### 3.1 Conjunto de documentos a produzir
- [ ] Política de Privacidade (LGPD)
- [ ] Termos de Uso
- [ ] Política de Cookies
- [ ] Aviso de Tratamento de Dados *(opcional, complementar)*
- [ ] Política de Segurança da Informação *(opcional, forte diferencial para clientes empresariais)*

Cada documento será redigido sob medida para o Piscina Perfeita — não um texto genérico — cobrindo LGPD, boas práticas da ANPD, linguagem clara para o usuário final e estrutura profissional (estimativa de 10 a 15 páginas formatadas), incluindo cláusulas específicas do sistema:
- isolamento de dados entre condomínios (Locais) — cada cliente acessa apenas seus próprios dados;
- autenticação via JWT e armazenamento de senhas com hash;
- uso de HTTPS em toda a aplicação;
- controle de permissões por perfil (SuperAdmin, Administrador e demais usuários);
- backups e demais medidas de segurança adotadas;
- estrutura preparada para futuras integrações sem precisar reescrever a política do zero.

### 3.2 Checklist de informações a levantar antes da redação

**a) Identificação da empresa**
- [ ] Nome da empresa ou pessoa responsável
- [ ] CNPJ (se já possuir)
- [ ] Cidade e Estado
- [ ] E-mail de contato
- [ ] Site

**b) Sobre o sistema**
- [ ] Nome do sistema: Piscina Perfeita (confirmar se é o nome comercial final)
- [ ] Descrição resumida do que o sistema faz
- [ ] Público-alvo / quem pode utilizar
- [ ] Modelo de cobrança: gratuito, pago, ou ambos (Free/Pro/Enterprise)

**c) Dados coletados** — confirmar lista atual e complementar se houver mais:
- [ ] Nome
- [ ] E-mail
- [ ] Senha (armazenada como hash)
- [ ] Perfil de acesso
- [ ] Local/Condomínio
- [ ] Registros de análises
- [ ] Estoque
- [ ] Movimentações
- [ ] Logs de acesso (se existirem)
- [ ] Outros dados ainda não mapeados?

**d) Infraestrutura e serviços de terceiros**
- [x] Frontend hospedado na Render
- [x] Backend hospedado na Render
- [x] Banco PostgreSQL na Neon
- [x] HTTPS
- [x] JWT
- [x] Senhas com hash
- [ ] Google Analytics — pretende usar?
- [ ] Meta Pixel — pretende usar?
- [ ] Hotjar — pretende usar?
- [ ] Alguma IA integrada (ex.: sugestões automáticas)?
- [ ] Firebase — pretende usar?
- [ ] Envio de e-mails transacionais (qual provedor)?

**e) Compartilhamento de dados com terceiros**
- [ ] Compartilha ou pretende compartilhar dados com contador, administradoras de condomínio ou empresas parceiras?

**f) Direitos do usuário a garantir**
- [ ] Solicitar exclusão da conta
- [ ] Exportação dos dados
- [ ] Alteração dos dados
- [ ] Solicitar cópia dos dados

**g) Cookies**
- [ ] Uso hoje restrito a cookies necessários (autenticação)?
- [ ] Pretende usar cookies de estatística/marketing no futuro?

**h) Contato para assuntos de LGPD**
- [ ] Definir e-mail de contato dedicado (ex.: `privacidade@seudominio.com.br`)

> Assim que essas informações forem preenchidas, os cinco documentos podem ser redigidos quase prontos para uso, restando apenas ajustar CNPJ, e-mail, endereço e detalhes pontuais conforme o sistema evolui.

---

## 4. Continuação do Roadmap Evolutivo — Rumo à Plataforma Completa

### Versão 2.0.0 — Virada de Chave SaaS (Multi-Condomínio)
**Objetivo:** Comercialização em larga escala.

- Painel "Super Admin" para gerenciamento de assinaturas e controle unificado.
- Um síndico profissional consegue alternar entre múltiplos condomínios de forma fluida.
- Planos Free, Pro e Enterprise, com cobrança recorrente/assinatura.
- Portal do cliente para acompanhamento de análises e relatórios.
- Gestão de licenças e Multi Tenant completo (documentos legais da seção 3 devem estar publicados antes do lançamento comercial deste marco).

### Versão 2.1.0 — Dashboard Gerencial e Inteligência de Dados (BI)
**Objetivo:** Painel analítico de alto nível.

- Relatórios consolidados de gastos mensais.
- Projeção de consumo de produtos químicos por sazonalidade (inverno vs. verão).
- Ranking de funcionários/equipes mais ativos.
- Análise financeira preditiva.

### Versão 2.2.0 — Aplicativo Mobile *(a partir das ideias)*
**Objetivo:** Trabalho em campo para técnicos e administradores.

- App em .NET MAUI com login, registro de análises e controle de estoque.
- Registro fotográfico antes/depois das manutenções.
- Leitura de QR Code em cada piscina para abrir sua ficha técnica diretamente.
- Modo offline com sincronização posterior.
- Geolocalização opcional da visita técnica.
- Assinatura digital do técnico responsável ao concluir uma visita.

### Versão 2.3.0 — Inteligência e Automação de Decisões *(a partir das ideias)*
**Objetivo:** Automatizar decisões que hoje dependem de julgamento humano.

- Sugestão automática de produtos e cálculo automático da dosagem com base no volume da piscina.
- Previsão de consumo e comparação entre piscinas/condomínios.
- IA para recomendações de correção de parâmetros da água.
- Indicadores automáticos e histórico inteligente.

### Versão 2.4.0 — Comunicação com o Cliente *(a partir das ideias)*
**Objetivo:** Manter clientes informados sem esforço manual.

- E-mail automático, WhatsApp e notificações push.
- Aviso de estoque baixo, análises pendentes e manutenção agendada.

### Versão 2.5.0 — Empresas de Manutenção *(a partir das ideias)*
**Objetivo:** Expandir o mercado para prestadoras de serviço com múltiplos clientes.

- Agenda de visitas, rotas e equipes de técnicos.
- Ordens de serviço com checklist.
- Histórico por cliente e ranking de eficiência entre equipes.

### Versão 2.6.0 — Financeiro *(a partir das ideias)*
**Objetivo:** Gestão financeira completa da operação.

- Compras, contas a pagar e fornecedores.
- Fluxo de caixa e centro de custos.
- Margem por contrato.

### Versão 3.0.0 — Plataforma Completa *(a partir das ideias)*
**Objetivo:** Tornar-se referência no segmento.

- API pública e Webhooks para integração com ERPs e sistemas de gestão condominial.
- Marketplace de plugins.
- BI com painéis personalizados.
- White Label — empresas de manutenção podem revender a plataforma com marca própria.

---

## 5. Roadmap Técnico Paralelo

Enquanto as funcionalidades evoluem, o roadmap técnico deve seguir em paralelo, sem depender de uma versão específica de negócio:

**Arquitetura**
- Clean Architecture, CQRS, MediatR, Domain Events
- Repository Pattern refinado, Specification Pattern

**Qualidade**
- xUnit — testes unitários e de integração
- Cobertura de testes, testes de carga

**Observabilidade**
- Serilog com logs estruturados e Correlation ID
- Health Checks, OpenTelemetry
- Dashboard de métricas e monitoramento de erros

**Performance**
- Redis / cache distribuído, compressão
- Background Services, filas, processamento assíncrono

**Infraestrutura**
- GitHub Actions (CI/CD), Docker Compose, Kubernetes (quando necessário)
- Backup automático, múltiplos ambientes, Feature Flags

**Segurança** *(complementa a seção 3)*
- MFA, Rate Limiting (login já implementado — expandir para outras rotas sensíveis)
- Auditoria completa, criptografia de dados sensíveis
- Política de senhas, conformidade contínua com a LGPD

**Experiência do Usuário**
- Tema escuro, dashboard configurável, atalhos
- Acessibilidade e internacionalização

---

## 6. Diferenciais Competitivos — Backlog de Ideias

Funcionalidades que fariam alguém olhar para o Piscina Perfeita e perceber que é diferente da concorrência:

- 📱 QR Code em cada piscina para abrir sua ficha técnica.
- 📸 Registro fotográfico antes e depois das manutenções.
- ✍️ Assinatura digital do técnico responsável.
- 📍 Geolocalização opcional da visita técnica.
- 📅 Agenda inteligente com recorrência de manutenções.
- 🤖 IA sugerindo correções quando os parâmetros da água estiverem fora do ideal.
- 📊 Dashboard executivo para síndicos e gestores.
- 💧 Cálculo automático da dosagem de produtos com base no volume da piscina.
- 🧾 Geração automática de laudos técnicos em PDF.
- 📦 Catálogo de fornecedores com comparação de preços.
- 🔔 Alertas por e-mail, WhatsApp e push.
- 🌐 Portal do cliente para acompanhar análises e relatórios.
- 📈 Indicadores de economia e consumo de produtos ao longo do tempo.
- 🏆 Ranking de eficiência entre equipes de manutenção.
- 🔌 API pública para integração com ERPs e sistemas de gestão condominial.
- 🏷️ White Label para empresas de manutenção venderem a plataforma com sua própria marca.
