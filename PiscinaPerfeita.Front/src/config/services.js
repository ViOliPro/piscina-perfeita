// ============================================================
//  Piscina Perfeita — Camada de serviço (fetch helpers)
//  Todos os módulos importam daqui para chamar a API.
// ============================================================
import { API_ENDPOINTS } from "./index";

// ----------------------------------------------------------
// Helper base — envolve fetch com tratamento de erro padrão
// ----------------------------------------------------------
async function request(url, options = {}) {
  const token = localStorage.getItem("pp_token");

  const res = await fetch(url, {
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers,
    },
    ...options,
  });

  if (!res.ok) {
    const erro = await res.json().catch(() => ({ message: res.statusText }));
    throw new Error(erro.message ?? `Erro ${res.status}`);
  }

  // 204 No Content — não tem body
  if (res.status === 204) return null;
  return res.json();
}

// ----------------------------------------------------------
// Shorthand verbs
// ----------------------------------------------------------
const get    = (url)         => request(url);
const post   = (url, body)   => request(url, { method: "POST",   body: JSON.stringify(body) });
const put    = (url, body)   => request(url, { method: "PUT",    body: JSON.stringify(body) });
const del    = (url)         => request(url, { method: "DELETE" });

// ----------------------------------------------------------
// Serviços por módulo
// ----------------------------------------------------------

// — Autenticação
// POST /api/account/login  → { email, password }
// Resposta: { accessToken, tokenType, expiresIn, user: { nome, email, role } }
export const authService = {
  login: (dto) =>
    // Chamada sem o header Authorization (rota AllowAnonymous)
    fetch(API_ENDPOINTS.login, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(dto),
    }).then(async (res) => {
      if (!res.ok) {
        const err = await res.json().catch(() => ({ message: res.statusText }));
        throw new Error(err.message ?? `Erro ${res.status}`);
      }
      return res.json();
    }),

  // Stub — endpoint ainda não implementado no backend
  forgotPassword: (dto) => post(API_ENDPOINTS.forgotPassword, dto),

  // Stub — endpoint ainda não implementado no backend
  resetPassword: (dto) => post(API_ENDPOINTS.resetPassword, dto),
};

// — Usuários
export const usuarioService = {
  listar:   ()       => get(API_ENDPOINTS.usuarios),
  buscar:   (id)     => get(API_ENDPOINTS.usuarioById(id)),
  criar:    (dto)    => post(API_ENDPOINTS.usuarios, dto),
  atualizar:(id,dto) => put(API_ENDPOINTS.usuarioById(id), dto),
  excluir:  (id)     => del(API_ENDPOINTS.usuarioById(id)),
};

// — Piscinas
export const piscinaService = {
  listar:   ()       => get(API_ENDPOINTS.piscinas),
  buscar:   (id)     => get(API_ENDPOINTS.piscinaById(id)),
  criar:    (dto)    => post(API_ENDPOINTS.piscinas, dto),
  atualizar:(id,dto) => put(API_ENDPOINTS.piscinaById(id), dto),
  excluir:  (id)     => del(API_ENDPOINTS.piscinaById(id)),
};

// — Produtos
export const produtoService = {
  listar:   ()       => get(API_ENDPOINTS.produtos),
  buscar:   (id)     => get(API_ENDPOINTS.produtoById(id)),
  criar:    (dto)    => post(API_ENDPOINTS.produtos, dto),
  atualizar:(id,dto) => put(API_ENDPOINTS.produtoById(id), dto),
  excluir:  (id)     => del(API_ENDPOINTS.produtoById(id)),
};

// — Análises
export const analiseService = {
  listar:   ()       => get(API_ENDPOINTS.analises),
  buscar:   (id)     => get(API_ENDPOINTS.analiseById(id)),
  criar:    (dto)    => post(API_ENDPOINTS.analises, dto),
  excluir:  (id)     => del(API_ENDPOINTS.analiseById(id)),
};

// — Estoque
export const estoqueService = {
  listar:           ()          => get(API_ENDPOINTS.estoques),
  listarBaixo:      ()          => get(API_ENDPOINTS.estoqueBaixo),
  listarPorPiscina: (piscinaId) => get(API_ENDPOINTS.estoquesByPiscina(piscinaId)),
  buscar:           (id)        => get(API_ENDPOINTS.estoqueById(id)),
  criar:            (dto)       => post(API_ENDPOINTS.estoques, dto),
  atualizar:        (id, dto)   => put(API_ENDPOINTS.estoqueById(id), dto),
};

// — Movimentações
export const movimentacaoService = {
  listar:   ()       => get(API_ENDPOINTS.movimentacoes),
  buscar:   (id)     => get(API_ENDPOINTS.movimentacaoById(id)),
  criar:    (dto)    => post(API_ENDPOINTS.movimentacoes, dto),
};
