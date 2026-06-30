// ============================================================
//  Piscina Perfeita — Módulo: Movimentações de Estoque
// ============================================================
import { useState, useEffect } from "react";
import {
  PageHeader, Card, Badge, Button, Modal, Toolbar,
  SearchInput, FilterSelect, DataTable, FormGrid, FormField,
  inputStyle, LoadingSpinner, ErrorMessage,
} from "../../components/ui/index.jsx";
import {
  movimentacaoService, piscinaService,
  produtoService, usuarioService,
} from "../../config/services.js";
import { TIPO_MOVIMENTACAO, TIPO_LABELS } from "../../config/index.js";

// ----------------------------------------------------------
// Formulário
// ----------------------------------------------------------
function MovimentacaoForm({ piscinas, produtos, usuarios, onSubmit, onCancel, loading }) {
  const [form, setForm] = useState({
    piscinaId: "", produtoId: "", usuarioId: "",
    tipoMovimentacao: String(TIPO_MOVIMENTACAO.ENTRADA),
    quantidade: "", valor: "",
    dataMovimentacao: new Date().toISOString().slice(0, 16),
  });
  const set = (key) => (e) => setForm((f) => ({ ...f, [key]: e.target.value }));

  function handleSubmit(e) {
    e.preventDefault();
    onSubmit({
      ...form,
      tipoMovimentacao: parseInt(form.tipoMovimentacao),
      quantidade: parseFloat(form.quantidade),
      valor: form.valor ? parseFloat(form.valor) : null,
    });
  }

  return (
    <form onSubmit={handleSubmit}>
      <FormGrid>
        <FormField label="Piscina *">
          <select required style={inputStyle} value={form.piscinaId} onChange={set("piscinaId")}>
            <option value="">Selecione a piscina</option>
            {piscinas.map((p) => <option key={p.id} value={p.id}>{p.nome}</option>)}
          </select>
        </FormField>
        <FormField label="Produto *">
          <select required style={inputStyle} value={form.produtoId} onChange={set("produtoId")}>
            <option value="">Selecione o produto</option>
            {produtos.map((p) => <option key={p.id} value={p.id}>{p.nome}</option>)}
          </select>
        </FormField>
        <FormField label="Tipo *">
          <select required style={inputStyle} value={form.tipoMovimentacao} onChange={set("tipoMovimentacao")}>
            <option value={TIPO_MOVIMENTACAO.ENTRADA}>Entrada</option>
            <option value={TIPO_MOVIMENTACAO.SAIDA}>Saída</option>
          </select>
        </FormField>
        <FormField label="Responsável *">
          <select required style={inputStyle} value={form.usuarioId} onChange={set("usuarioId")}>
            <option value="">Selecione o usuário</option>
            {usuarios.map((u) => <option key={u.id} value={u.id}>{u.nome}</option>)}
          </select>
        </FormField>
        <FormField label="Quantidade *">
          <input required type="number" step="0.01" min="0.01" placeholder="0.00"
            style={inputStyle} value={form.quantidade} onChange={set("quantidade")} />
        </FormField>
        <FormField label="Valor (R$)">
          <input type="number" step="0.01" min="0" placeholder="0.00"
            style={inputStyle} value={form.valor} onChange={set("valor")} />
        </FormField>
        <FormField label="Data e hora *">
          <input required type="datetime-local"
            style={inputStyle} value={form.dataMovimentacao} onChange={set("dataMovimentacao")} />
        </FormField>
      </FormGrid>
      <div style={{ display: "flex", justifyContent: "flex-end", gap: 8, marginTop: 16 }}>
        <Button variant="ghost" onClick={onCancel} type="button">Cancelar</Button>
        <Button variant="primary" type="submit" disabled={loading}>
          {loading ? "Salvando…" : "Salvar"}
        </Button>
      </div>
    </form>
  );
}

// ----------------------------------------------------------
// Módulo principal
// ----------------------------------------------------------
export default function Movimentacoes() {
  const [movimentos, setMovimentos] = useState([]);
  const [piscinas,   setPiscinas]   = useState([]);
  const [produtos,   setProdutos]   = useState([]);
  const [usuarios,   setUsuarios]   = useState([]);
  const [loading,    setLoading]    = useState(true);
  const [saving,     setSaving]     = useState(false);
  const [error,      setError]      = useState(null);
  const [modalOpen,  setModalOpen]  = useState(false);
  const [search,     setSearch]     = useState("");
  const [filtroTipo, setFiltroTipo] = useState("");

  useEffect(() => {
    async function load() {
      try {
        setLoading(true);
        const [m, p, pr, u] = await Promise.all([
          movimentacaoService.listar(),
          piscinaService.listar(),
          produtoService.listar(),
          usuarioService.listar(),
        ]);
        setMovimentos(m ?? []);
        setPiscinas(p ?? []);
        setProdutos(pr ?? []);
        setUsuarios(u ?? []);
      } catch (err) { setError(err.message); }
      finally { setLoading(false); }
    }
    load();
  }, []);

  async function handleSave(dto) {
    try {
      setSaving(true);
      const nova = await movimentacaoService.criar(dto);
      setMovimentos((prev) => [nova, ...prev]);
      setModalOpen(false);
    } catch (err) { setError(err.message); }
    finally { setSaving(false); }
  }

  const filtered = movimentos.filter((m) => {
    const txt = `${m.produto?.nome} ${m.piscina?.nome}`.toLowerCase();
    return txt.includes(search.toLowerCase()) &&
           (!filtroTipo || m.tipoMovimentacao === parseInt(filtroTipo));
  });

  const columns = [
    { key: "produto",  label: "Produto",  render: (_, r) => r.produto?.nome ?? "—" },
    { key: "piscina",  label: "Piscina",  render: (_, r) => r.piscina?.nome ?? "—" },
    {
      key: "tipoMovimentacao", label: "Tipo",
      render: (v) => (
        <Badge variant={v === TIPO_MOVIMENTACAO.ENTRADA ? "info" : "purple"}>
          {TIPO_LABELS[v]}
        </Badge>
      ),
    },
    {
      key: "quantidade", label: "Quantidade",
      render: (v, r) => `${v ?? "—"} ${r.produto?.unidadeMedida ?? ""}`,
    },
    {
      key: "valor", label: "Valor",
      render: (v) => v ? v.toLocaleString("pt-BR", { style: "currency", currency: "BRL" }) : "—",
    },
    {
      key: "dataMovimentacao", label: "Data",
      render: (v) => new Date(v).toLocaleString("pt-BR"),
    },
    { key: "usuarios", label: "Responsável", render: (_, r) => r.usuarios?.nome ?? "—" },
  ];

  if (loading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader
        title="Movimentações de estoque"
        description="Entradas e saídas por piscina e produto"
        action={<Button variant="primary" onClick={() => setModalOpen(true)}>+ Registrar movimentação</Button>}
      />

      {error && <ErrorMessage message={error} />}

      <Toolbar>
        <SearchInput value={search} onChange={setSearch} placeholder="Buscar produto ou piscina…" />
        <FilterSelect
          value={filtroTipo} onChange={setFiltroTipo}
          placeholder="Todos os tipos"
          options={[
            { value: String(TIPO_MOVIMENTACAO.ENTRADA), label: "Entrada" },
            { value: String(TIPO_MOVIMENTACAO.SAIDA),   label: "Saída" },
          ]}
        />
      </Toolbar>

      <Card noPadding>
        <DataTable columns={columns} data={filtered} emptyMessage="Nenhuma movimentação encontrada." />
      </Card>

      <Modal open={modalOpen} onClose={() => setModalOpen(false)} title="Registrar movimentação">
        <MovimentacaoForm
          piscinas={piscinas} produtos={produtos} usuarios={usuarios}
          onSubmit={handleSave} onCancel={() => setModalOpen(false)} loading={saving}
        />
      </Modal>
    </div>
  );
}
