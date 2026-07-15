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
  inputStyle,
  LoadingSpinner,
  ErrorMessage,
} from "../../components/ui/index.jsx";
import {
  estoqueService,
  piscinaService,
  produtoService,
  usuarioService,
  depositoService,
} from "../../config/services.js";
import { ESTOQUE_LIMITES, APP_META } from "../../config/index.js";

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
  depositos,
  onSubmit,
  onCancel,
  loading,
}) {
  const [form, setForm] = useState({
    piscinaId: "",
    produtoId: "",
    usuarioId: "",
    depositoId: "",
    quantidadeAtual: "",
    quantidadeMinima: "",
    estoqueIdeal: "",
  });
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
      quantidadeAtual: parseFloat(form.quantidadeAtual),
      quantidadeMinima,
      estoqueIdeal,
    });
  }

  return (
    <form onSubmit={handleSubmit}>
      <FormGrid>
        <FormField label="Depósito *">
          <select
            required
            style={inputStyle}
            value={form.depositoId}
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
            value={form.produtoId}
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
        <FormField label="Responsável *">
          <select
            required
            style={inputStyle}
            value={form.usuarioId}
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
        <FormField label="Estoque mínimo *">
          <input
            required
            type="number"
            step="0.01"
            min="0"
            placeholder="0.00"
            style={inputStyle}
            value={form.quantidadeMinima}
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
            value={form.estoqueIdeal}
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
        <Button variant="ghost" onClick={onCancel} type="button">
          Cancelar
        </Button>
        <Button variant="primary" type="submit" disabled={loading}>
          {loading ? "Salvando…" : "Registrar"}
        </Button>
      </div>
    </form>
  );
}

// ----------------------------------------------------------
// Aba: pedido de orçamento
// ----------------------------------------------------------
function PedidoOrcamento({ itens }) {
  const hoje = new Date().toLocaleDateString("pt-BR");

  function copiar() {
    const linhas = [
      "#\tProduto\tUnidade\tQtd. solicitada\tUso\tValor unit. (R$)\tValor total (R$)\tPrazo entrega",
    ];
    itens.forEach((item, i) => {
      const qtdSugerida = calcularQtdSugerida(item);
      linhas.push(
        `${String(i + 1).padStart(2, "0")}\t${item.produto?.nome}\t${item.produto?.unidadeMedida}\t${qtdSugerida}\tTratamento piscinas\t___\t___\t___`,
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
        }}
      >
        <div>
          <div style={{ fontSize: 13, fontWeight: 600, color: "#0A1628" }}>
            Solicitação de orçamento — fornecedores
          </div>
          <div style={{ fontSize: 12, color: "#6B8CAE" }}>
            Itens com estoque baixo ou em atenção · gerado em {hoje}
          </div>
        </div>
        <div style={{ display: "flex", gap: 8 }}>
          <Button variant="ghost" size="sm" onClick={copiar}>
            📋 Copiar
          </Button>
          <Button variant="primary" size="sm" onClick={imprimir}>
            🖨️ Imprimir / PDF
          </Button>
        </div>
      </div>

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
                  "Uso",
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
                    <td style={{ padding: "7px 10px", color: "#6B8CAE" }}>
                      Tratamento piscinas
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
  const [usuarios, setUsuarios] = useState([]);
  const [depositos, setDepositos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [tab, setTab] = useState("todos");
  const [search, setSearch] = useState("");
  const [filtroDeposito, setFiltroDeposito] = useState("");

  useEffect(() => {
    async function load() {
      try {
        setLoading(true);
        const [e, p, pr, u, d] = await Promise.all([
          estoqueService.listar(),
          piscinaService.listar(),
          produtoService.listar(),
          usuarioService.listar(),
          depositoService.listar(),
        ]);
        setEstoques(e ?? []);
        setPiscinas(p ?? []);
        setProdutos(pr ?? []);
        setUsuarios(u ?? []);
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
      const novo = await estoqueService.criar(dto);
      setEstoques((prev) => [novo, ...prev]);
      setModalOpen(false);
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
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
      key: "_edit",
      label: "",
      render: (_, r) => (
        <Button variant="ghost" size="sm">
          Editar
        </Button>
      ),
    },
  ];

  if (loading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader
        title="Estoque"
        description="Controle de produtos por piscina"
        action={
          <Button variant="primary" onClick={() => setModalOpen(true)}>
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
        <PedidoOrcamento itens={baixoItens} />
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
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        title="Registrar entrada de estoque"
      >
        <EstoqueForm
          piscinas={piscinas}
          produtos={produtos}
          usuarios={usuarios}
          depositos={depositos}
          onSubmit={handleSave}
          onCancel={() => setModalOpen(false)}
          loading={saving}
        />
      </Modal>
    </div>
  );
}
