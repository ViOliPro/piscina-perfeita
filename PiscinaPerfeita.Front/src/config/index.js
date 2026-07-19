// ============================================================
//  Piscina Perfeita — Arquivo de configuração central
//  Edite este arquivo para ajustar endpoints, limites e tema.
// ============================================================

// ----------------------------------------------------------
// API
// ----------------------------------------------------------
export const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5258/api";

export const API_ENDPOINTS = {
  // Autenticação  (AccountController → api/account/...)
  login: `${API_BASE_URL}/account/login`,
  switchLocal: `${API_BASE_URL}/account/SwitchLocal`,
  forgotPassword: `${API_BASE_URL}/account/forgot-password`, // a implementar no backend
  resetPassword: `${API_BASE_URL}/account/reset-password`, // a implementar no backend

  // Usuários
  usuarios: `${API_BASE_URL}/usuarios`,
  usuarioById: (id) => `${API_BASE_URL}/usuarios/${id}`,

  // Piscinas
  piscinas: `${API_BASE_URL}/piscinas`,
  piscinaById: (id) => `${API_BASE_URL}/piscinas/${id}`,

  // Produtos
  produtos: `${API_BASE_URL}/produtos`,
  produtoById: (id) => `${API_BASE_URL}/produtos/${id}`,

  // Depósitos (vinculados ao Local)
  depositos: `${API_BASE_URL}/depositos`,
  depositoById: (id) => `${API_BASE_URL}/depositos/${id}`,

  // Análises
  analises: `${API_BASE_URL}/analises`,
  analiseById: (id) => `${API_BASE_URL}/analises/${id}`,

  // Estoque
  estoques: `${API_BASE_URL}/estoques`,
  estoqueById: (id) => `${API_BASE_URL}/estoques/${id}`,
  estoquesByPiscina: (piscinaId) =>
    `${API_BASE_URL}/estoques?piscinaId=${piscinaId}`,
  estoqueBaixo: `${API_BASE_URL}/estoques?status=baixo`,

  // Movimentações
  movimentacoes: `${API_BASE_URL}/movimentacoes`,
  movimentacaoById: (id) => `${API_BASE_URL}/movimentacoes/${id}`,
  contagemInventario: `${API_BASE_URL}/movimentacoes/contagem-inventario`,

  // Aplicações de produto (gera movimentação + baixa de estoque automaticamente)
  aplicacoesProduto: `${API_BASE_URL}/aplicacoesproduto`,
  aplicacaoProdutoById: (id) => `${API_BASE_URL}/aplicacoesproduto/${id}`,

  // Locais (condomínios/unidades)
  locais: `${API_BASE_URL}/locais`,
  localById: (id) => `${API_BASE_URL}/locais/${id}`,

  // Vínculos Usuário ↔ Local (UsuariosLocaisController)
  usuariosLocais: `${API_BASE_URL}/usuarioslocais`,
  usuarioLocalById: (id) => `${API_BASE_URL}/usuarioslocais/${id}`,
  meusLocais: `${API_BASE_URL}/usuarioslocais/meus`,
  locaisPorUsuario: (usuarioId) =>
    `${API_BASE_URL}/usuarioslocais/usuario/${usuarioId}`,
};

// ----------------------------------------------------------
// Estoque — limites para classificação de status
// ----------------------------------------------------------
export const ESTOQUE_LIMITES = {
  BAIXO: 5, // qtd ≤ este valor  → badge "Baixo"   (vermelho)
  ATENCAO: 15, // qtd ≤ este valor  → badge "Atenção"  (amarelo)
  // qtd > ATENCAO     → badge "Normal"   (verde)
};

// ----------------------------------------------------------
// Análises — faixas ideais (referência visual nos gauges)
// ----------------------------------------------------------
export const ANALISE_FAIXAS = {
  ph: { min: 7.2, max: 7.8 },
  cloroLivre: { min: 1.0, max: 3.0 },
  alcalinidade: { min: 80, max: 120 },
  temperatura: { min: 26, max: 30 },
};

// ----------------------------------------------------------
// Roles de usuário (espelha o enum C# Role)
// ----------------------------------------------------------
export const ROLES = {
  ADMIN: 0, // Role.SuperAdmin no backend
  USER: 1, // Role.Usuario no backend
};

export const ROLE_LABELS = {
  [ROLES.ADMIN]: "SuperAdmin",
  [ROLES.USER]: "Usuário",
};

// ----------------------------------------------------------
// Perfis de usuário por Local (espelha o enum C# Perfil)
// Diferente de Role (acesso global ao sistema), o Perfil define o
// nível de permissão do usuário dentro de um Local/condomínio específico.
// ----------------------------------------------------------
export const PERFIS = {
  ADMINISTRADOR: 0,
  OPERADOR: 1,
  VISUALIZADOR: 2,
};

export const PERFIL_LABELS = {
  [PERFIS.ADMINISTRADOR]: "Administrador",
  [PERFIS.OPERADOR]: "Operador",
  [PERFIS.VISUALIZADOR]: "Visualizador",
};

// ----------------------------------------------------------
// Tipos de movimentação (espelha o enum C# Tipo)
// IMPORTANTE: ENTRADA=0 e SAIDA=1 são os valores originais já gravados no
// banco — não mude esses números. Os tipos novos entram sempre com
// números novos no final, nunca no meio da lista.
// ----------------------------------------------------------
export const TIPO_MOVIMENTACAO = {
  ENTRADA: 0,
  SAIDA: 1,
  COMPRA: 2,
  APLICACAO: 3,
  PERDA: 4,
  DESCARTE: 5,
  AJUSTE_INVENTARIO: 6,
};

export const TIPO_LABELS = {
  [TIPO_MOVIMENTACAO.ENTRADA]: "Entrada",
  [TIPO_MOVIMENTACAO.SAIDA]: "Saída",
  [TIPO_MOVIMENTACAO.COMPRA]: "Compra",
  [TIPO_MOVIMENTACAO.APLICACAO]: "Aplicação",
  [TIPO_MOVIMENTACAO.PERDA]: "Perda",
  [TIPO_MOVIMENTACAO.DESCARTE]: "Descarte",
  [TIPO_MOVIMENTACAO.AJUSTE_INVENTARIO]: "Ajuste de Inventário",
};

// Tipos que o usuário pode escolher ao registrar uma movimentação manual —
// Ajuste de Inventário é sempre gerado automaticamente pela tela de
// contagem física, nunca escolhido diretamente (ver ContagemInventario.jsx).
export const TIPOS_MOVIMENTACAO_MANUAL = [
  TIPO_MOVIMENTACAO.ENTRADA,
  TIPO_MOVIMENTACAO.COMPRA,
  TIPO_MOVIMENTACAO.SAIDA,
  TIPO_MOVIMENTACAO.PERDA,
  TIPO_MOVIMENTACAO.DESCARTE,
];

// Tipos que exigem uma Piscina associada (mesma regra do backend)
export const TIPOS_QUE_EXIGEM_PISCINA = [
  TIPO_MOVIMENTACAO.ENTRADA,
  TIPO_MOVIMENTACAO.SAIDA,
  TIPO_MOVIMENTACAO.APLICACAO,
];

// Unidades de lançamento aceitas para conversão (mesma família de
// UNIDADES_MEDIDA, mas incluindo submúltiplos como mg/mL que não fazem
// sentido como unidade "base" de um produto, só como lançamento).
export const UNIDADES_LANCAMENTO = [
  "kg",
  "g",
  "mg",
  "L",
  "mL",
  "un",
  "cx",
  "sc",
];

// ----------------------------------------------------------
// Unidades de medida disponíveis para Produto
// ----------------------------------------------------------
export const UNIDADES_MEDIDA = ["kg", "g", "L", "mL", "un", "cx", "sc"];

// Sugestões de categoria de produto — Categoria é um campo de texto livre
// na API (não um enum), então isso só alimenta um <datalist> de apoio,
// sem travar o usuário a essas opções.
export const CATEGORIAS_PRODUTO_SUGESTOES = [
  "Químicos",
  "Equipamentos",
  "Acessórios",
  "Filtragem",
  "Limpeza",
  "Segurança",
];

// ----------------------------------------------------------
// Paginação padrão
// ----------------------------------------------------------
export const PAGINATION = {
  DEFAULT_PAGE_SIZE: 20,
  PAGE_SIZE_OPTIONS: [10, 20, 50, 100],
};

// ----------------------------------------------------------
// Tema — paleta Piscina Perfeita
// Estes valores CSS são aplicados via :root no index.css.
// Mantenha sincronizado com tailwind.config.js se usar Tailwind.
// ----------------------------------------------------------
export const THEME = {
  colors: {
    deep: "#0A1628", // sidebar, headings principais
    mid: "#1E3A5F", // labels, hover sidebar
    water: "#2E86AB", // accent primário (botões, bordas de foco)
    sky: "#5BC0EB", // links, badges info, nav ativo
    foam: "#E8F4FD", // fundo de página, fundo de thead
    ice: "#F0F7FF", // fundo de hover de linha
    muted: "#6B8CAE", // texto secundário, placeholders
    low: "#E74C3C", // estoque baixo / erro
    ok: "#27AE60", // status normal / sucesso
    warn: "#F39C12", // atenção / aviso
  },
};

// ----------------------------------------------------------
// Metadados da aplicação
// ----------------------------------------------------------
export const APP_META = {
  name: import.meta.env.VITE_APP_META_NAME,
  version: import.meta.env.VITE_APP_META_VERSION,
  contact: "compras@piscinaperfeita.com.br",
};
