// ============================================================
//  Piscina Perfeita — Camada de mapeamento (mapper)
//
//  Por que existe?
//  O backend ASP.NET Core serializa por padrão em PascalCase
//  (ou camelCase dependendo da config do JsonSerializer).
//  Este arquivo centraliza a conversão entre o contrato da
//  API e os objetos que o frontend consome, garantindo que
//  uma mudança no backend seja corrigida em um único lugar.
//
//  Convenção:
//    fromApi*(raw)  → converte resposta da API → objeto do front
//    toApi*(dto)    → converte objeto do front → body da requisição
// ============================================================

// ----------------------------------------------------------
// Utilitário: normaliza um campo que pode vir como
// PascalCase ou camelCase (e.g. "Nome" ou "nome")
// ----------------------------------------------------------
function field(obj, ...keys) {
  for (const key of keys) {
    if (obj?.[key] !== undefined && obj[key] !== null) return obj[key];
  }
  return null;
}

// ----------------------------------------------------------
// Auth
// ----------------------------------------------------------

/**
 * AccountResponseDto  →  AuthUser
 * {
 *   AccessToken | accessToken
 *   TokenType   | tokenType
 *   ExpiresIn   | expiresIn
 *   User | user : UserResponseDto
 * }
 */
export function fromApiAuth(raw) {
  if (!raw) return null;
  return {
    accessToken: field(raw, "accessToken", "AccessToken"),
    tokenType: field(raw, "tokenType", "TokenType") ?? "Bearer",
    expiresIn: field(raw, "expiresIn", "ExpiresIn") ?? 28800,
    user: fromApiAuthUser(field(raw, "user", "User")),
  };
}

/**
 * UserResponseDto  →  AuthUser.user
 */
export function fromApiAuthUser(raw) {
  if (!raw) return null;
  return {
    userId: field(raw, "userId", "UserId"),
    nome: field(raw, "nome", "Nome"),
    email: field(raw, "email", "Email"),
    role: field(raw, "role", "Role") ?? 1,
    // Local ativo (condomínio/unidade) do usuário nesta sessão.
    // Guid.Empty (vindo do backend) é normalizado para null.
    localId: (() => {
      const v = field(raw, "localId", "LocalId");
      return v && v !== "00000000-0000-0000-0000-000000000000" ? v : null;
    })(),
    // Perfil no Local ativo (ou do vínculo pendente, se ainda não tiver
    // nenhum Local) — usado para guiar um Administrador sem local a criar
    // o primeiro Local antes de liberar o resto do sistema.
    perfil: field(raw, "perfil", "Perfil"),
  };
}

/**
 * Login request  →  AccountRequestDto
 */
export function toApiLogin({ email, password }) {
  return { Email: email, Password: password };
}

// ----------------------------------------------------------
// Usuario
// ----------------------------------------------------------

/**
 * UsuarioDto (resposta)  →  Usuario (front)
 * Espelha: Id, Nome, Email, SenhaHash, Role, CreatedAt
 */
export function fromApiUsuario(raw) {
  if (!raw) return null;
  return {
    id: field(raw, "id", "Id"),
    nome: field(raw, "nome", "Nome"),
    email: field(raw, "email", "Email"),
    cpf: field(raw, "cpf", "Cpf") ?? "",
    role: field(raw, "role", "Role") ?? 1,
    createdAt: field(raw, "createdAt", "CreatedAt") ?? null,
    // Local (condomínio/unidade) ao qual o usuário está vinculado.
    // Só é retornado pela API na criação; em updates costuma vir null.
    localId: field(raw, "localId", "LocalId") ?? null,
    ultimoLocalId: field(raw, "ultimoLocalId", "UltimoLocalId") ?? null,
    // Perfil (papel do usuário dentro do local: Administrador/Operador/Visualizador)
    perfil: field(raw, "perfil", "Perfil"),
  };
}

export function fromApiUsuarioList(rawList) {
  return (rawList ?? []).map(fromApiUsuario);
}

/**
 * Usuario (front)  →  UsuarioRequestDto (criação) / UsuarioRequestUpdateDto (edição)
 * senhaHash é enviado como "Password" — o backend faz o hash com BCrypt.
 * cpf, perfil e localId só são consumidos pelo backend na criação
 * (UsuarioRequestUpdateDto não os possui); enviá-los numa atualização é
 * inofensivo, pois o model binding do ASP.NET simplesmente os ignora.
 */
export function toApiUsuario({ nome, email, senhaHash, role, cpf, perfil, localId }) {
  const dto = { Nome: nome, Email: email, Role: role ?? 1 };
  if (senhaHash) dto.senhaHash = senhaHash; // nome do campo no request dto
  if (cpf) dto.Cpf = cpf;
  if (perfil !== undefined && perfil !== null && perfil !== "") dto.Perfil = parseInt(perfil);
  if (localId) dto.LocalId = localId;
  return dto;
}

// ----------------------------------------------------------
// Piscina
// ----------------------------------------------------------

/**
 * PiscinaDto  →  Piscina (front)
 * Espelha: Id, UsuarioId, Nome, VolumeLitros, ProfundidadeMedia, CreatedAt
 */
export function fromApiPiscina(raw) {
  if (!raw) return null;

  return {
    id: field(raw, "id", "Id"),
    // Campo escalar simples — necessário para pré-preencher o <select> do
    // formulário de edição. Antes essa chave era sobrescrita abaixo pelo
    // objeto de relacionamento (mesmo nome "usuario"), deixando o campo
    // "Responsável" sempre vazio ao editar uma piscina existente.
    usuarioId: field(raw, "usuarioId", "UsuarioId"),
    nome: field(raw, "nome", "Nome"),
    volumeLitros: field(raw, "volumeLitros", "VolumeLitros") ?? null,
    profundidadeMedia:
      field(raw, "profundidadeMedia", "ProfundidadeMedia") ?? null,
    createdAt: field(raw, "createdAt", "CreatedAt") ?? null,
    // Relacionamentos opcionais (se a API incluir via include/expand)
    usuario: raw?.usuarioPiscina
      ? fromApiUsuario(raw.usuarioPiscina)
      : raw?.Usuario
        ? fromApiUsuario(raw.Usuario)
        : null,
  };
}

export function fromApiPiscinaList(rawList) {
  return (rawList ?? []).map(fromApiPiscina);
}

export function toApiPiscina({
  nome,
  volumeLitros,
  profundidadeMedia,
  usuarioId,
}) {
  return {
    Nome: nome,
    VolumeLitros: volumeLitros ? parseFloat(volumeLitros) : null,
    ProfundidadeMedia: profundidadeMedia ? parseFloat(profundidadeMedia) : null,
    UsuarioId: usuarioId,
  };
}

// ----------------------------------------------------------
// Produto
// ----------------------------------------------------------

/**
 * ProdutoDto  →  Produto (front)
 * Espelha: Id, Nome, UnidadeMedida
 */
export function fromApiProduto(raw) {
  if (!raw) return null;
  return {
    id: field(raw, "id", "Id"),
    nome: field(raw, "nome", "Nome"),
    unidadeMedida: field(raw, "unidadeMedida", "UnidadeMedida") ?? "",
    fabricante: field(raw, "fabricante", "Fabricante") ?? "",
    marca: field(raw, "marca", "Marca") ?? "",
    observacoes: field(raw, "observacoes", "Observacoes") ?? "",
    categoria: field(raw, "categoria", "Categoria") ?? "",
  };
}

export function fromApiProdutoList(rawList) {
  return (rawList ?? []).map(fromApiProduto);
}

export function toApiProduto({ nome, unidadeMedida, fabricante, marca, observacoes, categoria }) {
  return {
    Nome: nome,
    UnidadeMedida: unidadeMedida,
    Fabricante: fabricante || null,
    Marca: marca || null,
    Observacoes: observacoes || null,
    Categoria: categoria || null,
  };
}

// ----------------------------------------------------------
// Analise
// ----------------------------------------------------------

/**
 * AnaliseDto  →  Analise (front)
 * Espelha: Id, PiscinaId, UsuarioId, DataAnalise,
 *          Ph, CloroLivre, Alcalinidade, Temperatura, Observacoes
 */
export function fromApiAnalise(raw) {
  if (!raw) return null;
  return {
    id: field(raw, "id", "Id"),
    piscinaId: field(raw, "piscinaId", "PiscinaId"),
    usuarioId: field(raw, "usuarioId", "UsuarioId"),
    dataAnalise: field(raw, "dataAnalise", "DataAnalise"),
    ph: field(raw, "ph", "Ph") ?? null,
    cloroLivre: field(raw, "cloroLivre", "CloroLivre") ?? null,
    alcalinidade: field(raw, "alcalinidade", "Alcalinidade") ?? null,
    temperatura: field(raw, "temperatura", "Temperatura") ?? null,
    observacoes: field(raw, "observacoes", "Observacoes") ?? "",
    // Relacionamentos
    piscina: raw?.piscina
      ? fromApiPiscina(raw.piscina)
      : raw?.Piscina
        ? fromApiPiscina(raw.Piscina)
        : null,
    usuario: raw?.usuario
      ? fromApiUsuario(raw.usuario)
      : raw?.Usuario
        ? fromApiUsuario(raw.Usuario)
        : null,
  };
}

export function fromApiAnaliseList(rawList) {
  return (rawList ?? []).map(fromApiAnalise);
}

export function toApiAnalise({
  piscinaId,
  usuarioId,
  dataAnalise,
  ph,
  cloroLivre,
  alcalinidade,
  temperatura,
  observacoes,
}) {
  return {
    PiscinaId: piscinaId,
    UsuarioId: usuarioId,
    DataAnalise: dataAnalise,
    Ph: ph != null ? parseFloat(ph) : null,
    CloroLivre: cloroLivre != null ? parseFloat(cloroLivre) : null,
    Alcalinidade: alcalinidade != null ? parseFloat(alcalinidade) : null,
    Temperatura: temperatura != null ? parseFloat(temperatura) : null,
    Observacoes: observacoes ?? "",
  };
}

// ----------------------------------------------------------
// Estoque
// ----------------------------------------------------------

/**
 * EstoqueDto  →  Estoque (front)
 * Espelha: Id, PiscinaId, ProdutoId, UsuarioId, QuantidadeAtual
 */
// ----------------------------------------------------------
// Deposito (vinculado ao Local)
// ----------------------------------------------------------
export function fromApiDeposito(raw) {
  if (!raw) return null;
  return {
    id: field(raw, "id", "Id"),
    nome: field(raw, "nome", "Nome"),
    observacao: field(raw, "observacao", "Observacao") ?? "",
  };
}

export function fromApiDepositoList(rawList) {
  return (rawList ?? []).map(fromApiDeposito);
}

export function toApiDeposito({ nome, observacao }) {
  return { Nome: nome, Observacao: observacao || null };
}

export function fromApiEstoque(raw) {
  if (!raw) return null;
  return {
    id: field(raw, "id", "Id"),
    piscinaId: field(raw, "piscinaId", "PiscinaId"),
    produtoId: field(raw, "produtoId", "ProdutoId"),
    usuarioId: field(raw, "usuarioId", "UsuarioId"),
    depositoId: field(raw, "depositoId", "DepositoId"),
    quantidadeAtual: field(raw, "quantidadeAtual", "QuantidadeAtual") ?? 0,
    // Relacionamentos
    piscina: raw?.piscina
      ? fromApiPiscina(raw.piscina)
      : raw?.Piscina
        ? fromApiPiscina(raw.Piscina)
        : null,
    produto: raw?.produto
      ? fromApiProduto(raw.produto)
      : raw?.Produto
        ? fromApiProduto(raw.Produto)
        : null,
    usuario: raw?.usuario
      ? fromApiUsuario(raw.usuario)
      : raw?.Usuario
        ? fromApiUsuario(raw.Usuario)
        : null,
    // Novo: { id, nome } — a API só traz esses dois campos, não o objeto
    // completo de Deposito (sem Observacao).
    deposito: (() => {
      const d = field(raw, "deposito", "Deposito");
      if (!d) return null;
      return { id: field(d, "id", "Id"), nome: field(d, "nome", "Nome") };
    })(),
  };
}

export function fromApiEstoqueList(rawList) {
  return (rawList ?? []).map(fromApiEstoque);
}

export function toApiEstoque({
  piscinaId,
  produtoId,
  usuarioId,
  depositoId,
  quantidadeAtual,
}) {
  return {
    PiscinaId: piscinaId,
    ProdutoId: produtoId,
    UsuarioId: usuarioId,
    DepositoId: depositoId,
    QuantidadeAtual:
      quantidadeAtual != null ? parseFloat(quantidadeAtual) : null,
  };
}

// ----------------------------------------------------------
// MovimentacaoEstoque
// ----------------------------------------------------------

/**
 * MovimentacaoEstoqueDto  →  Movimentacao (front)
 * Espelha: Id, PiscinaId, ProdutoId, UsuarioId,
 *          TipoMovimentacao, Quantidade, Valor, DataMovimentacao
 */
export function fromApiMovimentacao(raw) {
  if (!raw) return null;
  return {
    id: field(raw, "id", "Id"),
    piscinaId: field(raw, "piscinaId", "PiscinaId"),
    produtoId: field(raw, "produtoId", "ProdutoId"),
    usuarioId: field(raw, "usuarioId", "UsuarioId"),
    depositoId: field(raw, "depositoId", "DepositoId"),
    tipoMovimentacao: field(raw, "tipoMovimentacao", "TipoMovimentacao") ?? 0,
    quantidade: field(raw, "quantidade", "Quantidade") ?? null,
    unidadeLancamento: field(raw, "unidadeLancamento", "UnidadeLancamento") ?? "",
    valor: field(raw, "valor", "Valor") ?? null,
    dataMovimentacao: field(raw, "dataMovimentacao", "DataMovimentacao"),
    // Relacionamentos — a API retorna {Id, Nome} (NomeIdDto), não o objeto
    // completo; por isso extraímos direto em vez de usar fromApiPiscina/etc.
    piscina: (() => {
      const p = field(raw, "piscina", "Piscina");
      return p ? { id: field(p, "id", "Id"), nome: field(p, "nome", "Nome") } : null;
    })(),
    produto: (() => {
      const p = field(raw, "produto", "Produto");
      return p ? { id: field(p, "id", "Id"), nome: field(p, "nome", "Nome") } : null;
    })(),
    deposito: (() => {
      const d = field(raw, "deposito", "Deposito");
      return d ? { id: field(d, "id", "Id"), nome: field(d, "nome", "Nome") } : null;
    })(),
    usuario: (() => {
      const u = field(raw, "usuario", "Usuario");
      return u ? { id: field(u, "id", "Id"), nome: field(u, "nome", "Nome") } : null;
    })(),
  };
}

export function fromApiMovimentacaoList(rawList) {
  return (rawList ?? []).map(fromApiMovimentacao);
}

export function toApiMovimentacao({
  piscinaId,
  produtoId,
  usuarioId,
  depositoId,
  tipoMovimentacao,
  quantidade,
  unidadeLancamento,
  valor,
  dataMovimentacao,
}) {
  return {
    PiscinaId: piscinaId || null,
    ProdutoId: produtoId,
    UsuarioId: usuarioId,
    DepositoId: depositoId,
    TipoMovimentacao: tipoMovimentacao != null ? parseInt(tipoMovimentacao) : 0,
    Quantidade: quantidade != null ? parseFloat(quantidade) : null,
    UnidadeLancamento: unidadeLancamento || null,
    Valor: valor != null ? parseFloat(valor) : null,
    DataMovimentacao: dataMovimentacao,
  };
}

// ----------------------------------------------------------
// Local (condomínio/unidade)
// ----------------------------------------------------------

/**
 * LocalResponseDto  →  Local (front)
 */
export function fromApiLocal(raw) {
  if (!raw) return null;
  return {
    id: field(raw, "id", "Id"),
    nome: field(raw, "nome", "Nome"),
    descricao: field(raw, "descricao", "Descricao") ?? "",
    telefone: field(raw, "telefone", "Telefone") ?? "",
    observacoes: field(raw, "observacoes", "Observacoes") ?? "",
    endereco: field(raw, "endereco", "Endereco") ?? "",
    cidade: field(raw, "cidade", "Cidade") ?? "",
    estado: field(raw, "estado", "Estado") ?? "",
    pais: field(raw, "pais", "Pais") ?? "",
    cep: field(raw, "cep", "Cep") ?? "",
  };
}

export function fromApiLocalList(rawList) {
  return (rawList ?? []).map(fromApiLocal);
}

export function toApiLocal({
  nome, descricao, telefone, observacoes, endereco, cidade, estado, pais, cep,
}) {
  return {
    Nome: nome,
    Descricao: descricao || null,
    Telefone: telefone || null,
    Observacoes: observacoes || null,
    Endereco: endereco || null,
    Cidade: cidade || null,
    Estado: estado || null,
    Pais: pais || null,
    Cep: cep || null,
  };
}

// ----------------------------------------------------------
// UsuarioLocal (vínculo usuário ↔ local, com Perfil)
// ----------------------------------------------------------

/**
 * UsuarioLocalResponseDto  →  UsuarioLocal (front)
 */
export function fromApiUsuarioLocal(raw) {
  if (!raw) return null;
  return {
    id: field(raw, "id", "Id"),
    usuarioId: field(raw, "usuarioId", "UsuarioId"),
    localId: field(raw, "localId", "LocalId") ?? null,
    localNome: field(raw, "localNome", "LocalNome") ?? null,
    perfil: field(raw, "perfil", "Perfil"),
    createdAt: field(raw, "createdAt", "CreatedAt") ?? null,
    ativo: field(raw, "ativo", "Ativo") ?? true,
  };
}

export function fromApiUsuarioLocalList(rawList) {
  return (rawList ?? []).map(fromApiUsuarioLocal);
}

export function toApiUsuarioLocal({ usuarioId, localId, perfil }) {
  return {
    UsuarioId: usuarioId,
    LocalId: localId,
    Perfil: perfil != null ? parseInt(perfil) : 2,
  };
}

// ----------------------------------------------------------
// AplicacaoProduto (aplicação de produto em uma piscina — gera
// automaticamente uma MovimentacaoEstoque e dá baixa no Estoque)
// ----------------------------------------------------------
export function fromApiAplicacaoProduto(raw) {
  if (!raw) return null;
  const rel = (key1, key2) => {
    const r = field(raw, key1, key2);
    return r ? { id: field(r, "id", "Id"), nome: field(r, "nome", "Nome") } : null;
  };
  return {
    id: field(raw, "id", "Id"),
    piscina: rel("piscina", "Piscina"),
    produto: rel("produto", "Produto"),
    deposito: rel("deposito", "Deposito"),
    usuario: rel("usuario", "Usuario"),
    analiseId: field(raw, "analiseId", "AnaliseId") ?? null,
    movimentacaoEstoqueId: field(raw, "movimentacaoEstoqueId", "MovimentacaoEstoqueId") ?? null,
    quantidade: field(raw, "quantidade", "Quantidade") ?? 0,
    unidadeLancamento: field(raw, "unidadeLancamento", "UnidadeLancamento") ?? "",
    dataAplicacao: field(raw, "dataAplicacao", "DataAplicacao"),
    observacoes: field(raw, "observacoes", "Observacoes") ?? "",
  };
}

export function fromApiAplicacaoProdutoList(rawList) {
  return (rawList ?? []).map(fromApiAplicacaoProduto);
}

export function toApiAplicacaoProduto({
  piscinaId,
  produtoId,
  depositoId,
  usuarioId,
  analiseId,
  quantidade,
  unidadeLancamento,
  dataAplicacao,
  observacoes,
}) {
  return {
    PiscinaId: piscinaId,
    ProdutoId: produtoId,
    DepositoId: depositoId,
    UsuarioId: usuarioId || null,
    AnaliseId: analiseId || null,
    Quantidade: quantidade != null ? parseFloat(quantidade) : null,
    UnidadeLancamento: unidadeLancamento || null,
    DataAplicacao: dataAplicacao || null,
    Observacoes: observacoes || null,
  };
}
