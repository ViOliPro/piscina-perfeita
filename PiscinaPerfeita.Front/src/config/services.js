// ============================================================
//  Piscina Perfeita — Camada de serviço
//  Todas as respostas da API passam pelo mapper antes de
//  chegar aos módulos. Todos os envios passam pelo toApi*.
// ============================================================
import { API_ENDPOINTS } from "./index.js";
import {
  fromApiAuth,      toApiLogin,
  fromApiUsuario,   fromApiUsuarioList,   toApiUsuario,
  fromApiPiscina,   fromApiPiscinaList,   toApiPiscina,
  fromApiProduto,   fromApiProdutoList,   toApiProduto,
  fromApiDeposito,  fromApiDepositoList,  toApiDeposito,
  fromApiAnalise,   fromApiAnaliseList,   toApiAnalise,
  fromApiEstoque,   fromApiEstoqueList,   toApiEstoque,
  fromApiMovimentacao, fromApiMovimentacaoList, toApiMovimentacao,
  fromApiAplicacaoProduto, fromApiAplicacaoProdutoList, toApiAplicacaoProduto,
  fromApiLocal,     fromApiLocalList,     toApiLocal,
  fromApiUsuarioLocal, fromApiUsuarioLocalList, toApiUsuarioLocal,
} from "./mappers.js";

// ----------------------------------------------------------
// Helper base
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
  if (res.status === 204) return null;
  return res.json();
}

const get  = (url)        => request(url);
const post = (url, body)  => request(url, { method: "POST",   body: JSON.stringify(body) });
const put  = (url, body)  => request(url, { method: "PUT",    body: JSON.stringify(body) });
const del  = (url)        => request(url, { method: "DELETE" });

// ----------------------------------------------------------
// Auth
// ----------------------------------------------------------
export const authService = {
  login: async (formDto) => {
    const res = await fetch(API_ENDPOINTS.login, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(toApiLogin(formDto)),
    });
    if (!res.ok) {
      const err = await res.json().catch(() => ({ message: res.statusText }));
      throw new Error(err.message ?? `Erro ${res.status}`);
    }
    return fromApiAuth(await res.json());
  },

  forgotPassword: (dto) => post(API_ENDPOINTS.forgotPassword, dto),
  resetPassword:  (dto) => post(API_ENDPOINTS.resetPassword,  dto),

  // Troca o Local (condomínio/unidade) ativo do usuário logado e emite um
  // novo token JWT já com o novo local_id no claim. O backend identifica o
  // usuário pelo token (Authorization header) — não é mais necessário (nem
  // aceito) enviar o userId no corpo da requisição.
  switchLocal: (newLocalId) =>
    post(`${API_ENDPOINTS.switchLocal}?newLocalId=${newLocalId}`).then(fromApiAuth),
};

// ----------------------------------------------------------
// Locais (condomínios/unidades)
// ----------------------------------------------------------
export const localService = {
  listar:    ()          => get(API_ENDPOINTS.locais).then(fromApiLocalList),
  buscar:    (id)        => get(API_ENDPOINTS.localById(id)).then(fromApiLocal),
  criar:     (dto)       => post(API_ENDPOINTS.locais,          toApiLocal(dto)).then(fromApiLocal),
  atualizar: (id, dto)   => put(API_ENDPOINTS.localById(id),    toApiLocal(dto)).then(fromApiLocal),
  excluir:   (id)        => del(API_ENDPOINTS.localById(id)),
};

// ----------------------------------------------------------
// Vínculos Usuário ↔ Local (Perfil por Local)
// ----------------------------------------------------------
export const usuarioLocalService = {
  listar:        ()            => get(API_ENDPOINTS.usuariosLocais).then(fromApiUsuarioLocalList),
  buscar:        (id)          => get(API_ENDPOINTS.usuarioLocalById(id)).then(fromApiUsuarioLocal),
  // Locais vinculados ao usuário autenticado — alimenta o seletor "Trocar Local".
  meusLocais:    ()            => get(API_ENDPOINTS.meusLocais).then(fromApiUsuarioLocalList),
  // Locais vinculados a um usuário específico — usado na administração de usuários.
  porUsuario:    (usuarioId)   => get(API_ENDPOINTS.locaisPorUsuario(usuarioId)).then(fromApiUsuarioLocalList),
  criar:         (dto)         => post(API_ENDPOINTS.usuariosLocais,       toApiUsuarioLocal(dto)).then(fromApiUsuarioLocal),
  atualizar:     (id, dto)     => put(API_ENDPOINTS.usuarioLocalById(id),  toApiUsuarioLocal(dto)).then(fromApiUsuarioLocal),
  excluir:       (id)          => del(API_ENDPOINTS.usuarioLocalById(id)),
};

// ----------------------------------------------------------
// Usuários
// ----------------------------------------------------------
export const usuarioService = {
  listar:    ()          => get(API_ENDPOINTS.usuarios).then(fromApiUsuarioList),
  buscar:    (id)        => get(API_ENDPOINTS.usuarioById(id)).then(fromApiUsuario),
  criar:     (dto)       => post(API_ENDPOINTS.usuarios,          toApiUsuario(dto)).then(fromApiUsuario),
  atualizar: (id, dto)   => put(API_ENDPOINTS.usuarioById(id),    toApiUsuario(dto)).then(fromApiUsuario),
  excluir:   (id)        => del(API_ENDPOINTS.usuarioById(id)),
};

// ----------------------------------------------------------
// Piscinas
// ----------------------------------------------------------
export const piscinaService = {
  listar:    ()          => get(API_ENDPOINTS.piscinas).then(fromApiPiscinaList),
  buscar:    (id)        => get(API_ENDPOINTS.piscinaById(id)).then(fromApiPiscina),
  criar:     (dto)       => post(API_ENDPOINTS.piscinas,          toApiPiscina(dto)).then(fromApiPiscina),
  atualizar: (id, dto)   => put(API_ENDPOINTS.piscinaById(id),    toApiPiscina(dto)).then(fromApiPiscina),
  excluir:   (id)        => del(API_ENDPOINTS.piscinaById(id)),
};

// ----------------------------------------------------------
// Produtos
// ----------------------------------------------------------
export const produtoService = {
  listar:    ()          => get(API_ENDPOINTS.produtos).then(fromApiProdutoList),
  buscar:    (id)        => get(API_ENDPOINTS.produtoById(id)).then(fromApiProduto),
  criar:     (dto)       => post(API_ENDPOINTS.produtos,          toApiProduto(dto)).then(fromApiProduto),
  atualizar: (id, dto)   => put(API_ENDPOINTS.produtoById(id),    toApiProduto(dto)).then(fromApiProduto),
  excluir:   (id)        => del(API_ENDPOINTS.produtoById(id)),
};

// ----------------------------------------------------------
// Depósitos (vinculados ao Local)
// ----------------------------------------------------------
export const depositoService = {
  listar:    ()          => get(API_ENDPOINTS.depositos).then(fromApiDepositoList),
  buscar:    (id)        => get(API_ENDPOINTS.depositoById(id)).then(fromApiDeposito),
  criar:     (dto)       => post(API_ENDPOINTS.depositos,          toApiDeposito(dto)).then(fromApiDeposito),
  atualizar: (id, dto)   => put(API_ENDPOINTS.depositoById(id),    toApiDeposito(dto)).then(fromApiDeposito),
  excluir:   (id)        => del(API_ENDPOINTS.depositoById(id)),
};

// ----------------------------------------------------------
// Análises
// ----------------------------------------------------------
export const analiseService = {
  listar:  ()    => get(API_ENDPOINTS.analises).then(fromApiAnaliseList),
  buscar:  (id)  => get(API_ENDPOINTS.analiseById(id)).then(fromApiAnalise),
  criar:   (dto) => post(API_ENDPOINTS.analises, toApiAnalise(dto)).then(fromApiAnalise),
  excluir: (id)  => del(API_ENDPOINTS.analiseById(id)),
};

// ----------------------------------------------------------
// Estoque
// ----------------------------------------------------------
export const estoqueService = {
  listar:           ()          => get(API_ENDPOINTS.estoques).then(fromApiEstoqueList),
  listarBaixo:      ()          => get(API_ENDPOINTS.estoqueBaixo).then(fromApiEstoqueList),
  listarPorPiscina: (piscinaId) => get(API_ENDPOINTS.estoquesByPiscina(piscinaId)).then(fromApiEstoqueList),
  buscar:           (id)        => get(API_ENDPOINTS.estoqueById(id)).then(fromApiEstoque),
  criar:            (dto)       => post(API_ENDPOINTS.estoques,        toApiEstoque(dto)).then(fromApiEstoque),
  atualizar:        (id, dto)   => put(API_ENDPOINTS.estoqueById(id),  toApiEstoque(dto)).then(fromApiEstoque),
};

// ----------------------------------------------------------
// Movimentações
// ----------------------------------------------------------
export const movimentacaoService = {
  listar:  ()    => get(API_ENDPOINTS.movimentacoes).then(fromApiMovimentacaoList),
  buscar:  (id)  => get(API_ENDPOINTS.movimentacaoById(id)).then(fromApiMovimentacao),
  criar:   (dto) => post(API_ENDPOINTS.movimentacoes, toApiMovimentacao(dto)).then(fromApiMovimentacao),

  // Feature de contagem física / Ajuste de Inventário: envia a contagem de
  // vários produtos de um depósito de uma vez; a API calcula a diferença
  // contra o estoque lógico e gera os ajustes necessários.
  contagemInventario: (depositoId, usuarioId, itens) =>
    post(API_ENDPOINTS.contagemInventario, {
      DepositoId: depositoId,
      UsuarioId: usuarioId || null,
      Itens: itens.map((i) => ({
        ProdutoId: i.produtoId,
        QuantidadeContada: parseFloat(i.quantidadeContada),
      })),
    }),
};

// ----------------------------------------------------------
// Aplicações de produto (Piscina + Produto + Quantidade + Data +
// Análise relacionada) — ao criar, o backend gera automaticamente a
// MovimentacaoEstoque e atualiza o Estoque do Depósito informado.
// ----------------------------------------------------------
export const aplicacaoProdutoService = {
  listar: ()     => get(API_ENDPOINTS.aplicacoesProduto).then(fromApiAplicacaoProdutoList),
  buscar: (id)   => get(API_ENDPOINTS.aplicacaoProdutoById(id)).then(fromApiAplicacaoProduto),
  criar:  (dto)  => post(API_ENDPOINTS.aplicacoesProduto, toApiAplicacaoProduto(dto)).then(fromApiAplicacaoProduto),
};
