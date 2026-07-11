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
export function toApiUsuario({
  nome,
  email,
  senhaHash,
  role,
  cpf,
  perfil,
  localId,
}) {
  const dto = { Nome: nome, Email: email, Role: role ?? 1 };
  if (senhaHash) dto.senhaHash = senhaHash; // nome do campo no request dto
  if (cpf) dto.Cpf = cpf;
  if (perfil !== undefined && perfil !== null && perfil !== "")
    dto.Perfil = parseInt(perfil);
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
  };
}

export function fromApiProdutoList(rawList) {
  return (rawList ?? []).map(fromApiProduto);
}

export function toApiProduto({ nome, unidadeMedida }) {
  return { Nome: nome, UnidadeMedida: unidadeMedida };
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
export function fromApiEstoque(raw) {
  if (!raw) return null;
  return {
    id: field(raw, "id", "Id"),
    piscinaId: field(raw, "piscinaId", "PiscinaId"),
    produtoId: field(raw, "produtoId", "ProdutoId"),
    usuarioId: field(raw, "usuarioId", "UsuarioId"),
    quantidadeAtual: field(raw, "quantidadeAtual", "QuantidadeAtual") ?? 0,
    quantidadeMinima: field(raw, "quantidadeMinima", "QuantidadeMinima") ?? 5,
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
  };
}

export function fromApiEstoqueList(rawList) {
  return (rawList ?? []).map(fromApiEstoque);
}

export function toApiEstoque({
  piscinaId,
  produtoId,
  usuarioId,
  quantidadeAtual,
  quantidadeMinima,
}) {
  return {
    PiscinaId: piscinaId,
    ProdutoId: produtoId,
    UsuarioId: usuarioId,
    QuantidadeAtual:
      quantidadeAtual != null ? parseFloat(quantidadeAtual) : null,
    QuantidadeMinima:
      quantidadeMinima != null ? parseFloat(quantidadeMinima) : null,
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
    tipoMovimentacao: field(raw, "tipoMovimentacao", "TipoMovimentacao") ?? 0,
    quantidade: field(raw, "quantidade", "Quantidade") ?? null,
    valor: field(raw, "valor", "Valor") ?? null,
    dataMovimentacao: field(raw, "dataMovimentacao", "DataMovimentacao"),
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
    // O campo no modelo C# é "Usuarios" (plural) — normalizamos para "usuario"
    usuario: raw?.usuarios
      ? fromApiUsuario(raw.usuarios)
      : raw?.Usuarios
        ? fromApiUsuario(raw.Usuarios)
        : raw?.usuario
          ? fromApiUsuario(raw.usuario)
          : raw?.Usuario
            ? fromApiUsuario(raw.Usuario)
            : null,
  };
}

export function fromApiMovimentacaoList(rawList) {
  return (rawList ?? []).map(fromApiMovimentacao);
}

export function toApiMovimentacao({
  piscinaId,
  produtoId,
  usuarioId,
  tipoMovimentacao,
  quantidade,
  valor,
  dataMovimentacao,
}) {
  return {
    PiscinaId: piscinaId,
    ProdutoId: produtoId,
    UsuarioId: usuarioId,
    TipoMovimentacao: tipoMovimentacao != null ? parseInt(tipoMovimentacao) : 0,
    Quantidade: quantidade != null ? parseFloat(quantidade) : null,
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
  nome,
  descricao,
  telefone,
  observacoes,
  endereco,
  cidade,
  estado,
  pais,
  cep,
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
