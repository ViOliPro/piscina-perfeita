// ============================================================
//  Piscina Perfeita — Módulo: Estoque
// ============================================================
import { useState, useEffect } from "react";
import {
  PageHeader,
  Card,
  Badge,
  Button,
  Modal,
  Tabs,
  Toolbar,
  SearchInput,
  FilterSelect,
  DataTable,
  FormGrid,
  FormField,
  LoadingSpinner,
  ErrorMessage,
} from "../../components/ui/index.jsx";
import { inputStyle } from "../../components/ui/styles.js";
import {
  estoqueService,
  piscinaService,
  produtoService,
  depositoService,
} from "../../config/services.js";
import { ESTOQUE_LIMITES, APP_META } from "../../config/index.js";
import { PERMISSIONS } from "../../helpers/Permissions.js";
import ProtecaoDeRota from "../../helpers/ProtecaoDeRota.jsx";
import { useUsuariosSelecionaveis } from "../../hooks/useUsuariosSelecionaveis.js";

// ----------------------------------------------------------
// Helpers
// ----------------------------------------------------------
function statusEstoque(qtd) {
  if (qtd === null || qtd === undefined) return { variant: "info", label: "—" };
  if (qtd <= ESTOQUE_LIMITES.BAIXO) return { variant: "bad", label: "Baixo" };
  if (qtd <= ESTOQUE_LIMITES.ATENCAO)
    return { variant: "warn", label: "Atenção" };
  return { variant: "ok", label: "Normal" };
}

function isBaixoOuAtencao(e) {
  return (e.quantidadeAtual ?? 0) <= ESTOQUE_LIMITES.ATENCAO;
}

// Quantidade sugerida para o pedido de orçamento.
// Regra nova: se o item já tem mínimo e ideal configurados, usa
// EstoqueIdeal - QuantidadeAtual (nunca negativo).
// Fallback: itens antigos, ainda sem esses campos preenchidos,
// continuam usando a heurística fixa baseada em ESTOQUE_LIMITES.
function calcularQtdSugerida(item) {
  const atual = item.quantidadeAtual ?? 0;

  if (item.quantidadeMinima != null && item.estoqueIdeal != null) {
    return Math.max(0, item.estoqueIdeal - atual);
  }

  return Math.max(20, (ESTOQUE_LIMITES.ATENCAO - atual) * 3);
}

// Um item usa a heurística antiga (ainda não configurado) quando
// não tem mínimo e ideal definidos.
function usaEstimativaPadrao(item) {
  return item.quantidadeMinima == null || item.estoqueIdeal == null;
}

// ----------------------------------------------------------
// Formulário de entrada de estoque
// ----------------------------------------------------------
function EstoqueForm({
  piscinas,
  produtos,
  usuarios,
  podeVerUsuario,
  depositos,
  onSubmit,
  onCancel,
  loading,
  initial,
}) {
  // Inicialização segura utilizando os dados de 'initial' (quando for edição)
  const [form, setForm] = useState(() => ({
    piscinaId: initial?.piscinaId ?? initial?.piscina?.id ?? "",
    produtoId: initial?.produtoId ?? initial?.produto?.id ?? "",
    usuarioId: initial?.usuarioId ?? initial?.usuario?.id ?? null,
    depositoId: initial?.depositoId ?? initial?.deposito?.id ?? "",
    quantidadeAtual: initial?.quantidadeAtual ?? "",
    quantidadeMinima: initial?.quantidadeMinima ?? "",
    estoqueIdeal: initial?.estoqueIdeal ?? "",
  }));

  const [formError, setFormError] = useState(null);
  const set = (key) => (e) => setForm((f) => ({ ...f, [key]: e.target.value }));

  function handleSubmit(e) {
    e.preventDefault();

    const quantidadeMinima = parseFloat(form.quantidadeMinima);
    const estoqueIdeal = parseFloat(form.estoqueIdeal);

    if (estoqueIdeal <= quantidadeMinima) {
      setFormError("O estoque ideal deve ser maior que o estoque mínimo.");
      return;
    }

    setFormError(null);
    onSubmit({
      ...form,
      quantidadeAtual: parseFloat(form.quantidadeAtual) || 0,
      quantidadeMinima,
      estoqueIdeal,
      // Operador/Visualizador nunca enviam usuarioId — mesmo que o campo
      // nunca apareça na UI para esses perfis, garantimos aqui que o
      // payload nunca carregue um valor residual.
      usuarioId: podeVerUsuario ? form.usuarioId || null : null,
    });
  }

  return (
    <form onSubmit={handleSubmit}>
      <FormGrid>
        <FormField label="Depósito *">
          <select
            required
            style={inputStyle}
            value={form?.depositoId ?? form?.deposito?.id}
            onChange={set("depositoId")}
          >
            <option value="">Selecione o depósito</option>
            {depositos.map((d) => (
              <option key={d.id} value={d.id}>
                {d.nome}
              </option>
            ))}
          </select>
        </FormField>

        <FormField label="Produto *">
          <select
            required
            style={inputStyle}
            value={form.produtoId ?? form?.produto?.id}
            onChange={set("produtoId")}
          >
            <option value="">Selecione o produto</option>
            {produtos.map((p) => (
              <option key={p.id} value={p.id}>
                {p.nome}
              </option>
            ))}
          </select>
        </FormField>
        <FormField label="Quantidade atual *">
          <input
            required
            type="number"
            step="0.01"
            min="0"
            placeholder="0.00"
            style={inputStyle}
            value={form.quantidadeAtual}
            onChange={set("quantidadeAtual")}
          />
        </FormField>
        {podeVerUsuario && (
          <FormField label="Responsável">
            <select
              style={inputStyle}
              value={form.usuarioId ?? form?.usuario?.id}
              onChange={set("usuarioId")}
            >
              <option value="">Selecione o usuário</option>
              {usuarios.map((u) => (
                <option key={u.id} value={u.id}>
                  {u.nome}
                </option>
              ))}
            </select>
          </FormField>
        )}
        <FormField label="Estoque mínimo *">
          <input
            required
            type="number"
            step="0.01"
            min="0"
            placeholder="0.00"
            style={inputStyle}
            value={form.quantidadeMinima == null ? "" : form.quantidadeMinima}
            onChange={set("quantidadeMinima")}
          />
        </FormField>
        <FormField label="Estoque ideal *">
          <input
            required
            type="number"
            step="0.01"
            min="0"
            placeholder="0.00"
            style={inputStyle}
            value={form.estoqueIdeal == null ? "" : form.estoqueIdeal}
            onChange={set("estoqueIdeal")}
          />
        </FormField>
      </FormGrid>
      {formError && <ErrorMessage message={formError} />}
      <div
        style={{
          display: "flex",
          justifyContent: "flex-end",
          gap: 8,
          marginTop: 16,
        }}
      >
        <Button
          variant="ghost"
          onClick={onCancel}
          type="button"
          permission={PERMISSIONS.ESTOQUES.CREATE}
        >
          Cancelar
        </Button>
        <Button
          variant="primary"
          type="submit"
          disabled={loading}
          permission={PERMISSIONS.ESTOQUES.CREATE}
        >
          {loading ? "Salvando…" : initial ? "Salvar alterações" : "Registrar"}
        </Button>
      </div>
    </form>
  );
}

// ----------------------------------------------------------
// Aba: pedido de orçamento
// ----------------------------------------------------------
function PedidoOrcamento({ estoques, depositos }) {
  const hoje = new Date().toLocaleDateString("pt-BR");

  // Escopo: por padrão só itens baixos/em atenção (comportamento antigo),
  // mas o usuário pode pedir orçamento para todos os produtos elegíveis.
  const [escopo, setEscopo] = useState("baixo");
  const [filtroDeposito, setFiltroDeposito] = useState("");
  const [filtroCategoria, setFiltroCategoria] = useState("");

  const categorias = Array.from(
    new Set(estoques.map((e) => e.produto?.categoria).filter(Boolean)),
  );

  const itens = estoques
    .filter((e) => (escopo === "todos" ? true : isBaixoOuAtencao(e)))
    .filter((e) => !filtroDeposito || e?.deposito?.id === filtroDeposito)
    .filter((e) => !filtroCategoria || e.produto?.categoria === filtroCategoria)
    // Não faz sentido pedir orçamento de 0 unidades (item já no nível
    // ideal, ou sem quantidade sugerida a repor).
    .filter((e) => calcularQtdSugerida(e) > 0);

  function copiar() {
    const linhas = [
      "#\tProduto\tUnidade\tQtd. solicitada",
    ];
    itens.forEach((item, i) => {
      const qtdSugerida = calcularQtdSugerida(item);
      linhas.push(
        `${String(i + 1).padStart(2, "0")}\t${item.produto?.nome}\t${item.produto?.unidadeMedida}\t${qtdSugerida}`,
      );
    });
    navigator.clipboard
      .writeText(linhas.join("\n"))
      .then(() => alert("Copiado para a área de transferência!"));
  }

  function imprimir() {
    window.print();
  }

  return (
    <div>
      <div
        style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
          marginBottom: 12,
          flexWrap: "wrap",
          gap: 8,
        }}
      >
        <div>
          <div style={{ fontSize: 13, fontWeight: 600, color: "#0A1628" }}>
            Solicitação de orçamento — fornecedores
          </div>
          <div style={{ fontSize: 12, color: "#6B8CAE" }}>
            {escopo === "todos"
              ? "Todos os produtos elegíveis do estoque"
              : "Itens com estoque baixo ou em atenção"}{" "}
            · gerado em {hoje}
          </div>
        </div>
        <div style={{ display: "flex", gap: 8 }}>
          <Button
            variant="ghost"
            size="sm"
            onClick={copiar}
            permission={PERMISSIONS.ESTOQUES.INVENTARIO}
          >
            📋 Copiar
          </Button>
          <Button
            variant="primary"
            size="sm"
            onClick={imprimir}
            permission={PERMISSIONS.ESTOQUES.INVENTARIO}
          >
            🖨️ Imprimir / PDF
          </Button>
        </div>
      </div>

      <Toolbar>
        <FilterSelect
          value={escopo}
          onChange={setEscopo}
          placeholder="Escopo"
          options={[
            { value: "baixo", label: "Estoque baixo/atenção" },
            { value: "todos", label: "Todos os produtos" },
          ]}
        />
        <FilterSelect
          value={filtroDeposito}
          onChange={setFiltroDeposito}
          placeholder="Todos os depósitos"
          options={depositos.map((d) => ({ value: d.id, label: d.nome }))}
        />
        <FilterSelect
          value={filtroCategoria}
          onChange={setFiltroCategoria}
          placeholder="Todas as categorias"
          options={categorias.map((c) => ({ value: c, label: c }))}
        />
      </Toolbar>

      <Card noPadding>
        <div
          style={{
            padding: "16px 20px",
            borderBottom: "0.5px solid var(--border)",
          }}
        >
          <div style={{ fontSize: 13, fontWeight: 600, color: "#0A1628" }}>
            {APP_META.name.toUpperCase()} — Pedido de cotação
          </div>
          <div style={{ fontSize: 12, color: "#6B8CAE" }}>
            Prezado fornecedor, solicitamos cotação para os itens abaixo. Enviar
            para: <strong>{APP_META.contact}</strong>
          </div>
        </div>

        <div style={{ overflowX: "auto" }}>
          <table
            style={{ width: "100%", borderCollapse: "collapse", fontSize: 12 }}
          >
            <thead>
              <tr>
                {[
                  "#",
                  "Produto",
                  "Und.",
                  "Qtd. solicitada",
                  "Valor unit. (R$)",
                  "Valor total (R$)",
                  "Prazo entrega",
                ].map((h) => (
                  <th
                    key={h}
                    style={{
                      background: "#E8F4FD",
                      color: "#1E3A5F",
                      fontWeight: 600,
                      fontSize: 10,
                      textTransform: "uppercase",
                      letterSpacing: ".5px",
                      padding: "8px 10px",
                      textAlign: "left",
                      borderBottom: "1px solid var(--border)",
                    }}
                  >
                    {h}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {itens.map((item, i) => {
                const qtdSugerida = calcularQtdSugerida(item);
                const fallback = usaEstimativaPadrao(item);
                return (
                  <tr
                    key={item.id}
                    style={{ borderBottom: "0.5px solid var(--border)" }}
                  >
                    <td style={{ padding: "7px 10px", color: "#6B8CAE" }}>
                      {String(i + 1).padStart(2, "0")}
                    </td>
                    <td style={{ padding: "7px 10px", fontWeight: 500 }}>
                      {item.produto?.nome}
                    </td>
                    <td style={{ padding: "7px 10px", color: "#6B8CAE" }}>
                      {item.produto?.unidadeMedida}
                    </td>
                    <td style={{ padding: "7px 10px" }}>
                      {qtdSugerida}
                      {fallback && (
                        <span
                          title="Estoque ideal não configurado — usando estimativa padrão"
                          style={{
                            marginLeft: 6,
                            fontSize: 10,
                            color: "#B7791F",
                          }}
                        >
                          ⓘ estimativa
                        </span>
                      )}
                    </td>
                    <td style={{ padding: "7px 10px", color: "#aaa" }}>
                      ___________
                    </td>
                    <td style={{ padding: "7px 10px", color: "#aaa" }}>
                      ___________
                    </td>
                    <td style={{ padding: "7px 10px", color: "#aaa" }}>
                      ___________
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>

        <div
          style={{
            padding: "12px 20px",
            fontSize: 11,
            color: "#6B8CAE",
            borderTop: "0.5px solid var(--border)",
          }}
        >
          Validade da cotação: 5 dias úteis · Condições: à vista e parcelado
        </div>
      </Card>
    </div>
  );
}

// ----------------------------------------------------------
// Módulo principal
// ----------------------------------------------------------
export default function Estoque() {
  const [estoques, setEstoques] = useState([]);
  const [piscinas, setPiscinas] = useState([]);
  const [produtos, setProdutos] = useState([]);
  const { usuarios, podeVerUsuario } = useUsuariosSelecionaveis();
  const [depositos, setDepositos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [modal, setModal] = useState({ open: false, editing: null });
  const [tab, setTab] = useState("todos");
  const [search, setSearch] = useState("");
  const [filtroDeposito, setFiltroDeposito] = useState("");

  useEffect(() => {
    async function load() {
      try {
        setLoading(true);
        const [e, p, pr, d] = await Promise.all([
          estoqueService.listar(),
          piscinaService.listar(),
          produtoService.listar(),
          depositoService.listar(),
        ]);
        setEstoques(e ?? []);
        setPiscinas(p ?? []);
        setProdutos(pr ?? []);
        setDepositos(d ?? []);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    }
    load();
  }, []);

  async function handleSave(dto) {
    try {
      setSaving(true);
      if (modal.editing) {
        const atualizado = await estoqueService.atualizar(
          modal.editing.id,
          dto,
        );
        setEstoques((prev) =>
          prev.map((l) => (l.id === atualizado.id ? atualizado : l)),
        );
      } else {
        const novo = await estoqueService.criar(dto);
        setEstoques((prev) => [novo, ...prev]);
        setModalOpen(false);
      }
      setModal({ open: false, editing: null });
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(id) {
    if (!confirm("Excluir este estoque?")) return;
    try {
      await estoqueService.excluir(id);
      setEstoques((prev) => prev.filter((d) => d.id !== id));
    } catch (err) {
      setError(err.message);
    }
  }

  const filtered = estoques.filter((e) => {
    const txt = `${e.produto?.nome} ${e.deposito?.nome}`.toLowerCase();
    const matchSearch = txt.includes(search.toLowerCase());
    const matchDeposito = !filtroDeposito || e.depositoId === filtroDeposito;
    const matchTab =
      tab === "todos" ? true : tab === "baixo" ? isBaixoOuAtencao(e) : true;
    return matchSearch && matchDeposito && matchTab;
  });

  const baixoItens = estoques.filter(isBaixoOuAtencao);

  const columns = [
    {
      key: "produto",
      label: "Produto",
      render: (_, r) => r.produto?.nome ?? "—",
    },
    {
      key: "deposito",
      label: "Depósito",
      render: (_, r) => r.deposito?.nome ?? "—",
    },
    {
      key: "unidade",
      label: "Unidade",
      render: (_, r) => r.produto?.unidadeMedida ?? "—",
    },
    { key: "quantidadeAtual", label: "Qtd. atual", render: (v) => v ?? "—" },
    {
      key: "_status",
      label: "Status",
      render: (_, r) => {
        const s = statusEstoque(r.quantidadeAtual);
        return <Badge variant={s.variant}>{s.label}</Badge>;
      },
    },
    {
      key: "_acoes",
      label: "",
      render: (_, r) => (
        <div style={{ display: "flex", gap: 6 }}>
          <Button
            variant="ghost"
            size="sm"
            onClick={() => setModal({ open: true, editing: r })}
            permission={PERMISSIONS.ESTOQUES.CREATE}
          >
            Editar
          </Button>

          <Button
            variant="danger"
            size="sm"
            onClick={() => handleDelete(r.id)}
            permission={PERMISSIONS.ESTOQUES.DELETE}
          >
            Excluir
          </Button>
        </div>
      ),
    },
  ];

  return (
    <ProtecaoDeRota permissao={PERMISSIONS.ESTOQUES.VIEW}>
      {loading ? (
        <LoadingSpinner />
      ) : (
        <div>
          <PageHeader
            title="Estoque"
            description="Controle de produtos"
            action={
              <Button
                variant="primary"
                onClick={() => setModal({ open: true, editing: false })}
                permission={PERMISSIONS.ESTOQUES.CREATE}
              >
                + Registrar entrada
              </Button>
            }
          />

          {error && <ErrorMessage message={error} />}

          <Tabs
            active={tab}
            onChange={setTab}
            tabs={[
              { id: "todos", label: "Todos" },
              { id: "baixo", label: `⚠️ Estoque baixo (${baixoItens.length})` },
              { id: "orcamento", label: "📄 Pedido de orçamento" },
            ]}
          />

          {tab !== "orcamento" && (
            <Toolbar>
              <SearchInput
                value={search}
                onChange={setSearch}
                placeholder="Buscar produto ou depósito…"
              />
              <FilterSelect
                value={filtroDeposito}
                onChange={setFiltroDeposito}
                placeholder="Todos os depósitos"
                options={depositos.map((d) => ({ value: d.id, label: d.nome }))}
              />
            </Toolbar>
          )}

          {tab === "orcamento" ? (
            <PedidoOrcamento estoques={estoques} depositos={depositos} />
          ) : (
            <Card noPadding>
              <DataTable
                columns={columns}
                data={filtered}
                emptyMessage={
                  tab === "baixo"
                    ? "Nenhum item com estoque baixo. Tudo em ordem!"
                    : "Nenhum item encontrado."
                }
              />
            </Card>
          )}

          <Modal
            open={modal.open}
            onClose={() => setModal({ open: false, editing: null })}
            title={
              modal.editing ? "Registrar entrada de estoque" : "Novo estoque"
            }
          >
            <EstoqueForm
              initial={modal.editing}
              piscinas={piscinas}
              produtos={produtos}
              usuarios={usuarios}
              podeVerUsuario={podeVerUsuario}
              depositos={depositos}
              onSubmit={handleSave}
              onCancel={() => setModal({ open: false, editing: null })}
              loading={saving}
            />
          </Modal>
        </div>
      )}
    </ProtecaoDeRota>
  );
}
