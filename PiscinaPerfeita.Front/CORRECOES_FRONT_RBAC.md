# Alinhamento Front-end × API — RBAC, Fluxos e Correções

## 1. Correção necessária na API (para viabilizar o RBAC do front)

O endpoint `GET /usuarios` (usado por Administrador para listar usuários do
seu Local) nunca preenchia o campo `Perfil` na resposta — como o valor
default do enum é `0` (Administrador), **todo usuário aparecia como
"Administrador"** no front, tornando impossível filtrar corretamente por
Perfil (Operador/Administrador) nos seletores de "Responsável". Corrigido em
`UsuarioRepository.FilterRoleUsuario` para refletir o Perfil do vínculo
ativo do usuário no Local consultado.

## 2. Fluxo "Registrar Aplicação" (módulo Aplicações)

- Quando aberto a partir da tela de uma Análise, os campos **Piscina** e
  **Análise** agora ficam **travados** (`disabled`) — eles chegam
  pré-preenchidos e o usuário não pode trocá-los sem querer no meio do
  fluxo. Quando aberto pela navegação geral, ambos continuam editáveis e
  Análise continua opcional (como já era).
- **Filtro em cadeia Depósito → Produto**: o campo Produto agora só habilita
  depois de escolher um Depósito, e a lista mostra estritamente os produtos
  que têm estoque registrado naquele Depósito (cruzando com os registros de
  `Estoque` já carregados). Trocar o Depósito limpa a seleção de Produto se
  ela não for mais válida.

## 3. "Solicitar Orçamento" (módulo Estoque)

- Coluna **"Uso"** removida da tabela e da exportação por texto (botão
  Copiar).
- Itens cuja **quantidade sugerida calculada é 0** não entram mais na
  solicitação (não faz sentido pedir orçamento de 0 unidades).
- Novo filtro de **Escopo**: "Estoque baixo/atenção" (padrão, comportamento
  anterior) ou "Todos os produtos" (todos os produtos elegíveis do
  estoque).
- Novos filtros por **Depósito** e por **Categoria** de produto, aplicáveis
  em conjunto com o escopo.

## 4. RBAC (permissões por perfil)

### Bugs de segurança corrigidos
- `Estoque.jsx`: o botão **"+ Registrar entrada"** usava a prop errada
  (`permissao=` em vez de `permission=`), então ficava visível para
  **qualquer perfil, inclusive Visualizador**. Corrigido.
- `Usuarios.jsx`: o botão de vincular usuário a um Local referenciava
  `PERMISSIONS.USUARIOS.VINCULo` (typo, "o" minúsculo) — uma permissão
  inexistente sempre resolve como "sem restrição". Corrigido para
  `VINCULO`.
- `Analises.jsx`: o campo "Responsável" usava `podeMostrar ?? (<FormField>...)`
  em vez de `&&` — como `??` só cai no fallback quando o valor é
  `null`/`undefined` (nunca quando é booleano `false`/`true`), **o campo
  nunca era renderizado para ninguém**, nem para Administrador/SuperAdmin.
  Corrigido.

### Regra "usuarioId oculto para Operador"
Criado o hook `hooks/useUsuariosSelecionaveis.js`, que centraliza a regra
pedida e agora é usado em **todos** os formulários com campo de usuário
(Estoque, Movimentações, Análises, Piscinas):

- Operador/Visualizador: o campo de usuário fica oculto e **a API de
  usuários não é chamada** (o hook só dispara a requisição quando o perfil
  tem a permissão `GERAL.VIEW_USUARIO_SELETOR`, que só Administrador e
  SuperAdmin possuem).
- Administrador/SuperAdmin: veem a lista de usuários do Local atual,
  restrita a Perfil Operador ou Administrador (Visualizador nunca aparece
  como "responsável", já que não atua).
- Em todos os formulários, o payload enviado à API agora força
  `usuarioId: null` quando o campo não é exibido, mesmo que o estado
  interno do formulário tenha algum resíduo.
- Consolidei duas permissões redundantes e específicas de módulo
  (`MOVIMENTACOES.VIEW_INPUT_USUARIOS`, `ANALISES.VIEW_BTN`) em uma única
  permissão transversal: `PERMISSIONS.GERAL.VIEW_USUARIO_SELETOR`.

### Administrador Pai (alinhado com a API)
- `config/mappers.js`: `fromApiUsuarioLocal` agora mapeia o novo campo
  `ehAdministradorPai` vindo da API corrigida.
- `Usuarios.jsx` → `VinculosLocaisModal`: vínculos marcados como
  Administrador Pai exibem um badge "Administrador responsável" e o botão
  "Remover" fica oculto para quem não é SuperAdmin (a API já bloqueia essa
  ação no backend; aqui é só a antecipação visual para não gerar um erro
  desnecessário).

## 5. Mobile-first

A maior parte da base já usa uma infraestrutura mobile-first robusta
(`DataTable` alterna para cartões em telas pequenas via `useIsMobile`,
`Modal` vira bottom-sheet, `FormGrid`/`Toolbar`/`PageHeader` colapsam para
1 coluna, sidebar vira drawer deslizante). Ainda assim, encontrei dois
problemas reais:

- **`LoginPage.jsx` (bug real, tela mais importante do app)**: o painel do
  formulário tinha `width: 420` fixo. Em telas ≤720px o painel de branding
  é ocultado via CSS, sobrando só esse painel — em qualquer celular com
  menos de 420px de largura (a imensa maioria), o conteúdo estourava a
  viewport, forçando rolagem horizontal ou cortando o formulário de login.
  Corrigido para `width: "100%", maxWidth: 420`, com padding reduzido via
  media query em telas pequenas.
- Duas tabelas HTML puras no Dashboard (estoque crítico e últimas análises)
  sem rolagem horizontal em telas estreitas — adicionado wrapper
  `overflowX: auto`.

Não encontrei outras larguras fixas grandes (300–999px) sem guarda
responsiva no restante do código (varredura feita em todo o `src`).

## 6. Verificação

- `npm install && npm run build` rodado com sucesso neste ambiente (Vite
  5.4, 60 módulos, sem erros) — confirma que todas as alterações são
  sintaticamente válidas e os imports resolvem corretamente.
- `npm run lint` não pôde rodar (o projeto está com ESLint 9 mas ainda usa
  config no formato antigo `.eslintrc.*` — pré-existente, não é algo
  introduzido por esta revisão; recomendo migrar para `eslint.config.js`
  quando possível).

## 7. Pendências / próximos passos sugeridos

- Auditoria mobile-first mais profunda módulo a módulo (o essencial já
  funciona via `DataTable`/`FilterSelect`; vale um passe visual real em
  dispositivo para ajustes finos de espaçamento).
- Considerar expor `ehAdministradorPai` também na listagem principal de
  Usuários (hoje só aparece no modal de vínculos).
- Rodar a suíte de smoke tests manuais com um usuário Operador e um
  Visualizador reais para validar visualmente que nenhum botão/campo
  proibido aparece.
