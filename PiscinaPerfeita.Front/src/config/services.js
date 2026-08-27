// ============================================================
//  Piscina Perfeita — Camada de serviço
//  Todas as respostas da API passam pelo mapper antes de
//  chegar aos módulos. Todos os envios passam pelo toApi*.
// ============================================================
import { API_ENDPOINTS } from "./index.js";
import {
  fromApiAuth,
  toApiLogin,
  fromApiUsuario,
  fromApiUsuarioList,
  toApiUsuario,
  fromApiPiscina,
  fromApiPiscinaList,
  toApiPiscina,
  fromApiProduto,
  fromApiProdutoList,
  toApiProduto,
  fromApiDeposito,
  fromApiDepositoList,
  toApiDeposito,
  fromApiAnalise,
  fromApiAnaliseList,
  toApiAnalise,
  fromApiEstoque,
  fromApiEstoqueList,
  toApiEstoque,
  fromApiMovimentacao,
  fromApiMovimentacaoList,
  toApiMovimentacao,
  fromApiAplicacaoProduto,
  fromApiAplicacaoProdutoList,
  toApiAplicacaoProduto,
  fromApiLocal,
  fromApiLocalList,
  toApiLocal,
  fromApiUsuarioLocal,
  fromApiUsuarioLocalList,
  toApiUsuarioLocal,
  fromApiDashboardHidrometro,
  fromApiHidrometro,
  fromApiHidrometroList,
  toApiHidrometro,
  toApiGoogleLogin,
  toApiCompletarConviteGoogle,
} from "./mappers.js";

// ----------------------------------------------------------
// Helper base
// ----------------------------------------------------------
let refreshPromise = null;
let onSessaoRenovada = null;
let onSessaoExpirada = null;

// Token de acesso mantido só em memória (nunca em localStorage — decisão
// de segurança contra roubo via XSS, ver AuthContext.jsx). request()
// precisa lê-lo daqui; até 2026-08-13 ele vinha de localStorage("pp_token"),
// mas essa chave nunca é mais escrita desde a migração pra refresh via
// cookie — por isso nenhuma chamada autenticada levava o header
// Authorization, e tudo voltava 401 exceto login/refresh (que não passam
// por request()). setAccessToken é chamado pelo AuthContext sempre que o
// token muda (login, refresh, switchLocal, logout).
let currentAccessToken = null;

export function setAccessToken(token) {
  currentAccessToken = token;
}

export function registrarSessaoHandlers(handlers) {
  onSessaoRenovada = handlers.onSessaoRenovada;
  onSessaoExpirada = handlers.onSessaoExpirada;

  // Retorna uma função para desinscrever/limpar os handlers
  return () => {
    onSessaoRenovada = null;
    onSessaoExpirada = null;
  };
}

// Executa e centraliza todas as tentativas de Refresh Token
function tentarRefresh() {
  if (!refreshPromise) {
    refreshPromise = fetch(API_ENDPOINTS.refresh, {
      method: "POST",
      credentials: "include",
    })
      .then(async (res) => {
        if (!res.ok) throw new Error("Sessão expirada ou inválida.");
        const sessao = fromApiAuth(await res.json());
        onSessaoRenovada?.(sessao);
        return sessao;
      })
      .catch((err) => {
        onSessaoExpirada?.();
        throw err;
      })
      .finally(() => {
        // Evita zerar a promessa na mesma micro-task
        queueMicrotask(() => {
          refreshPromise = null;
        });
      });
  }
  return refreshPromise;
}

async function request(url, options = {}, _retry = true) {
  const token = currentAccessToken;

  const res = await fetch(url, {
    credentials: "include", // Envia cookies httpOnly do refresh token
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers,
    },
    ...options,
  });

  // Tratamento de Token Expirado (401)
  if (res.status === 401 && _retry) {
    try {
      await tentarRefresh();
      return request(url, options, false); // Repete a chamada 1x com novo token
    } catch {
      throw new Error("Sessão expirada. Faça login novamente.");
    }
  }

  if (!res.ok) {
    const erro = await res.json().catch(() => ({ message: res.statusText }));
    const error = new Error(erro.message ?? `Erro ${res.status}`);
    if (erro.erro) error.codigo = erro.erro;
    throw error;
  }

  if (res.status === 204) return null;

  // Nem todo 200 de sucesso tem corpo (ex.: endpoints que só confirmam uma
  // ação, como POST /account/aceitar-termos, retornam Ok() sem payload).
  // res.json() direto nesses casos estoura "Unexpected end of JSON input"
  // porque o corpo vem vazio — não é exclusividade do 204.
  const texto = await res.text();
  return texto ? JSON.parse(texto) : null;
}

const get = (url) => request(url);
const post = (url, body, opts = {}) =>
  request(
    url,
    { method: "POST", body: JSON.stringify(body) },
    opts.retry ?? true,
  );
const put = (url, body) =>
  request(url, { method: "PUT", body: JSON.stringify(body) });
const del = (url) => request(url, { method: "DELETE" });

// ----------------------------------------------------------
// Auth
// ----------------------------------------------------------
export const authService = {
  login: async (formDto) => {
    const res = await fetch(API_ENDPOINTS.login, {
      method: "POST",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(toApiLogin(formDto)),
    });
    if (!res.ok) {
      const err = await res.json().catch(() => ({ message: res.statusText }));
      throw new Error(err.message ?? `Erro ${res.status}`);
    }
    return fromApiAuth(await res.json());
  },

  loginGoogle: (idToken) =>
    post(API_ENDPOINTS.loginGoogle, toApiGoogleLogin({ idToken }), {
      retry: false,
    }).then(fromApiAuth),

  completarConviteGoogle: (idToken, cpf, aceiteTermos) =>
    post(
      API_ENDPOINTS.completarConviteGoogle,
      toApiCompletarConviteGoogle({ idToken, cpf, aceiteTermos }),
      { retry: false },
    ).then(fromApiAuth),

  forgotPassword: (dto) => post(API_ENDPOINTS.forgotPassword, dto),
  resetPassword: (dto) => post(API_ENDPOINTS.resetPassword, dto),
  completarConvite: (dto) => post(API_ENDPOINTS.completarConvite, dto),

  switchLocal: (newLocalId) =>
    post(`${API_ENDPOINTS.switchLocal}?newLocalId=${newLocalId}`).then(
      fromApiAuth,
    ),

  // Reaproveita o fluxo unificado de tentarRefresh
  refresh: () => tentarRefresh(),

  logout: () => post(API_ENDPOINTS.logout, {}),
  aceitarTermos: () => post(API_ENDPOINTS.aceitarTermos, {}),
};

// ----------------------------------------------------------
// Locais (condomínios/unidades)
// ----------------------------------------------------------
export const localService = {
  listar: () => get(API_ENDPOINTS.locais).then(fromApiLocalList),
  buscar: (id) => get(API_ENDPOINTS.localById(id)).then(fromApiLocal),
  criar: (dto) =>
    post(API_ENDPOINTS.locais, toApiLocal(dto)).then(fromApiLocal),
  atualizar: (id, dto) =>
    put(API_ENDPOINTS.localById(id), toApiLocal(dto)).then(fromApiLocal),
  excluir: (id) => del(API_ENDPOINTS.localById(id)),
};

// ----------------------------------------------------------
// Vínculos Usuário ↔ Local
// ----------------------------------------------------------
export const usuarioLocalService = {
  listar: () => get(API_ENDPOINTS.usuariosLocais).then(fromApiUsuarioLocalList),
  buscar: (id) =>
    get(API_ENDPOINTS.usuarioLocalById(id)).then(fromApiUsuarioLocal),
  meusLocais: () => get(API_ENDPOINTS.meusLocais).then(fromApiUsuarioLocalList),
  porUsuario: (usuarioId) =>
    get(API_ENDPOINTS.locaisPorUsuario(usuarioId)).then(
      fromApiUsuarioLocalList,
    ),
  criar: (dto) =>
    post(API_ENDPOINTS.usuariosLocais, toApiUsuarioLocal(dto)).then(
      fromApiUsuarioLocal,
    ),
  atualizar: (id, dto) =>
    put(API_ENDPOINTS.usuarioLocalById(id), toApiUsuarioLocal(dto)).then(
      fromApiUsuarioLocal,
    ),
  excluir: (id) => del(API_ENDPOINTS.usuarioLocalById(id)),
};

// ----------------------------------------------------------
// Usuários
// ----------------------------------------------------------
export const usuarioService = {
  listar: () => get(API_ENDPOINTS.usuarios).then(fromApiUsuarioList),
  buscar: (id) => get(API_ENDPOINTS.usuarioById(id)).then(fromApiUsuario),
  criar: (dto) =>
    post(API_ENDPOINTS.usuarios, toApiUsuario(dto)).then(fromApiUsuario),
  atualizar: (id, dto) =>
    put(API_ENDPOINTS.usuarioById(id), toApiUsuario(dto)).then(fromApiUsuario),
  excluir: (id) => del(API_ENDPOINTS.usuarioById(id)),
  meuPerfil: () => get(API_ENDPOINTS.meuPerfil).then(fromApiUsuario),
  atualizarMeuPerfil: (dto) =>
    put(API_ENDPOINTS.meuPerfil, dto).then(fromApiUsuario),
  alterarSenha: (dto) => put(API_ENDPOINTS.authPasswordSenhaAtualENova, dto),
  criarConvite: (dto) => post(API_ENDPOINTS.criarConvite, dto),
};

// ----------------------------------------------------------
// Piscinas
// ----------------------------------------------------------
export const piscinaService = {
  listar: () => get(API_ENDPOINTS.piscinas).then(fromApiPiscinaList),
  buscar: (id) => get(API_ENDPOINTS.piscinaById(id)).then(fromApiPiscina),
  criar: (dto) =>
    post(API_ENDPOINTS.piscinas, toApiPiscina(dto)).then(fromApiPiscina),
  atualizar: (id, dto) =>
    put(API_ENDPOINTS.piscinaById(id), toApiPiscina(dto)).then(fromApiPiscina),
  excluir: (id) => del(API_ENDPOINTS.piscinaById(id)),
};

// ----------------------------------------------------------
// Produtos
// ----------------------------------------------------------
export const produtoService = {
  listar: () => get(API_ENDPOINTS.produtos).then(fromApiProdutoList),
  buscar: (id) => get(API_ENDPOINTS.produtoById(id)).then(fromApiProduto),
  criar: (dto) =>
    post(API_ENDPOINTS.produtos, toApiProduto(dto)).then(fromApiProduto),
  atualizar: (id, dto) =>
    put(API_ENDPOINTS.produtoById(id), toApiProduto(dto)).then(fromApiProduto),
  excluir: (id) => del(API_ENDPOINTS.produtoById(id)),
};

// ----------------------------------------------------------
// Depósitos
// ----------------------------------------------------------
export const depositoService = {
  listar: () => get(API_ENDPOINTS.depositos).then(fromApiDepositoList),
  buscar: (id) => get(API_ENDPOINTS.depositoById(id)).then(fromApiDeposito),
  criar: (dto) =>
    post(API_ENDPOINTS.depositos, toApiDeposito(dto)).then(fromApiDeposito),
  atualizar: (id, dto) =>
    put(API_ENDPOINTS.depositoById(id), toApiDeposito(dto)).then(
      fromApiDeposito,
    ),
  excluir: (id) => del(API_ENDPOINTS.depositoById(id)),
};

// ----------------------------------------------------------
// Análises
// ----------------------------------------------------------
export const analiseService = {
  listar: () => get(API_ENDPOINTS.analises).then(fromApiAnaliseList),
  buscar: (id) => get(API_ENDPOINTS.analiseById(id)).then(fromApiAnalise),
  criar: (dto) =>
    post(API_ENDPOINTS.analises, toApiAnalise(dto)).then(fromApiAnalise),
  excluir: (id) => del(API_ENDPOINTS.analiseById(id)),
};

// ----------------------------------------------------------
// Estoque
// ----------------------------------------------------------
export const estoqueService = {
  listar: () => get(API_ENDPOINTS.estoques).then(fromApiEstoqueList),
  listarBaixo: () => get(API_ENDPOINTS.estoqueBaixo).then(fromApiEstoqueList),
  listarPorPiscina: (piscinaId) =>
    get(API_ENDPOINTS.estoquesByPiscina(piscinaId)).then(fromApiEstoqueList),
  buscar: (id) => get(API_ENDPOINTS.estoqueById(id)).then(fromApiEstoque),
  criar: (dto) =>
    post(API_ENDPOINTS.estoques, toApiEstoque(dto)).then(fromApiEstoque),
  atualizar: (id, dto) =>
    put(API_ENDPOINTS.estoqueById(id), toApiEstoque(dto)).then(fromApiEstoque),
  excluir: (id) => del(API_ENDPOINTS.estoqueById(id)),
};

// ----------------------------------------------------------
// Hidrômetros
// ----------------------------------------------------------
export const hidrometroService = {
  listar: () => get(API_ENDPOINTS.hidrometros).then(fromApiHidrometroList),
  buscar: (id) => get(API_ENDPOINTS.hidrometroById(id)).then(fromApiHidrometro),
  dashboard: () =>
    get(API_ENDPOINTS.hidrometroDashboard).then(fromApiDashboardHidrometro),
  criar: (dto) =>
    post(API_ENDPOINTS.hidrometros, toApiHidrometro(dto)).then(
      fromApiHidrometro,
    ),
  atualizar: (id, dto) =>
    put(API_ENDPOINTS.hidrometroById(id), toApiHidrometro(dto)).then(
      fromApiHidrometro,
    ),
  excluir: (id) => del(API_ENDPOINTS.hidrometroById(id)),
};

// ----------------------------------------------------------
// Movimentações
// ----------------------------------------------------------
export const movimentacaoService = {
  listar: ({ dataInicio, dataFim, piscinaId } = {}) => {
    const params = {};

    if (dataInicio) params.dataInicio = dataInicio;
    if (dataFim) params.dataFim = dataFim;
    if (piscinaId) params.piscinaId = piscinaId;

    return get(API_ENDPOINTS.movimentacoes, { params }).then(
      fromApiMovimentacaoList,
    );
  },

  buscar: (id) =>
    get(API_ENDPOINTS.movimentacaoById(id)).then(fromApiMovimentacao),

  criar: (dto) =>
    post(API_ENDPOINTS.movimentacoes, toApiMovimentacao(dto)).then(
      fromApiMovimentacao,
    ),

  contagemInventario: (depositoId, usuarioId, itens) =>
    post(API_ENDPOINTS.contagemInventario, {
      DepositoId: depositoId,
      UsuarioId: usuarioId || null,
      Itens: itens.map((i) => ({
        ProdutoId: i.produtoId,
        QuantidadeContada: parseFloat(i.quantidadeContada),
      })),
    }),

  lancarLoteInventario: ({ depositoId, usuarioId, itens, tipoMovimentacao }) =>
    post(API_ENDPOINTS.lancarLoteInventario, {
      DepositoId: depositoId,
      tipoMovimentacao: tipoMovimentacao,
      UsuarioId: usuarioId || null,
      Itens: itens.map((i) => ({
        ProdutoId: i.produtoId,
        QuantidadeContada: parseFloat(i.quantidade), // Lê 'quantidade' diretamente
      })),
    }),
};

// ----------------------------------------------------------
// Aplicações de Produto
// ----------------------------------------------------------
export const aplicacaoProdutoService = {
  listar: () =>
    get(API_ENDPOINTS.aplicacoesProduto).then(fromApiAplicacaoProdutoList),
  buscar: (id) =>
    get(API_ENDPOINTS.aplicacaoProdutoById(id)).then(fromApiAplicacaoProduto),
  criar: (dto) =>
    post(API_ENDPOINTS.aplicacoesProduto, toApiAplicacaoProduto(dto)).then(
      fromApiAplicacaoProduto,
    ),
};
