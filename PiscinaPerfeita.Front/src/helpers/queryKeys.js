// ============================================================
//  Piscina Perfeita — Query keys (TanStack Query)
//  Chaves estáveis e hierárquicas para cache compartilhado.
// ============================================================

export const qk = {
  piscinas: ["piscinas"],
  produtos: ["produtos"],
  depositos: ["depositos"],
  usuarios: ["usuarios"],

  estoques: (status = "todos") => ["estoques", status],

  analises: (filtros = {}) => ["analises", filtros],

  movimentacoes: (filtros = {}) => ["movimentacoes", filtros],
};

/** Helpers de data para filtros padrão */
export function diasAtrasISO(dias) {
  const d = new Date();
  d.setDate(d.getDate() - dias);
  d.setHours(0, 0, 0, 0);
  return d.toISOString();
}

export function inicioDoDiaISO(dataYmd) {
  return dataYmd ? new Date(`${dataYmd}T00:00:00`).toISOString() : undefined;
}

export function fimDoDiaISO(dataYmd) {
  return dataYmd
    ? new Date(`${dataYmd}T23:59:59.999`).toISOString()
    : undefined;
}
