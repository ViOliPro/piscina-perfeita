// ============================================================
//  Piscina Perfeita — Módulo: Movimentações de Estoque
// ============================================================
import { useState, useEffect } from "react";
import {
  PageHeader,
  Card,
  Badge,
  Button,
  Modal,
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
  movimentacaoService,
  piscinaService,
  produtoService,
  usuarioService,
  depositoService,
} from "../../config/services.js";
import {
  TIPO_MOVIMENTACAO,
  TIPO_LABELS,
  TIPOS_MOVIMENTACAO_MANUAL,
  TIPOS_QUE_EXIGEM_PISCINA,
  UNIDADES_LANCAMENTO,
} from "../../config/index.js";

// Cor do badge por tipo — entradas em azul, saídas em roxo/vermelho,
// ajuste em amarelo (é sempre gerado automaticamente, nunca manual).
function corDoTipo(tipo) {
  if (tipo === TIPO_MOVIMENTACAO.ENTRADA || tipo === TIPO_MOVIMENTACAO.COMPRA)
    return "info";
  if (tipo === TIPO_MOVIMENTACAO.AJUSTE_INVENTARIO) return "warn";
  return "purple";
}

// ----------------------------------------------------------
// Formulário
// ----------------------------------------------------------
function MovimentacaoForm({
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
    depositoId: "",
    usuarioId: "",
    tipoMovimentacao: String(TIPO_MOVIMENTACAO.ENTRADA),
    quantidade: "",
    unidadeLancamento: "",
    dataMovimentacao: new Date().toISOString().slice(0, 16),
  });
  const set = (key) => (e) => setForm((f) => ({ ...f, [key]: e.target.value }));

  const tipoNum = parseInt(form.tipoMovimentacao);
  const piscinaObrigatoria = TIPOS_QUE_EXIGEM_PISCINA.includes(tipoNum);

  function handleSubmit(e) {
    e.preventDefault();
    onSubmit({
      ...form,
      piscinaId: form.piscinaId || null,
      tipoMovimentacao: tipoNum,
      quantidade: parseFloat(form.quantidade),
    });
  }

  return (
    <form onSubmit={handleSubmit}>
      <FormGrid>
        <FormField label="Tipo *">
          <select
            required
            style={inputStyle}
            value={form.tipoMovimentacao}
            onChange={set("tipoMovimentacao")}
          >
            {TIPOS_MOVIMENTACAO_MANUAL.map((t) => (
              <option key={t} value={t}>
                {TIPO_LABELS[t]}
              </option>
            ))}
          </select>
        </FormField>
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
        <FormField label={`Piscina ${piscinaObrigatoria ? "*" : "(opcional)"}`}>
          <select
            required={piscinaObrigatoria}
            style={inputStyle}
            value={form.piscinaId}
            onChange={set("piscinaId")}
          >
            <option value="">
              {piscinaObrigatoria
                ? "Selecione a piscina"
                : "Não se aplica a uma piscina específica"}
            </option>
            {piscinas.map((p) => (
              <option key={p.id} value={p.id}>
                {p.nome}
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
                {p.nome} ({p.unidadeMedida})
              </option>
            ))}
          </select>
        </FormField>
        <FormField label="Quantidade *">
          <input
            required
            type="number"
            step="0.0001"
            min="0.0001"
            placeholder="0.00"
            style={inputStyle}
            value={form.quantidade}
            onChange={set("quantidade")}
          />
        </FormField>
        <FormField label="Unidade do lançamento">
          <select
            style={inputStyle}
            value={form.unidadeLancamento}
            onChange={set("unidadeLancamento")}
          >
            <option value="">Mesma unidade do produto</option>
            {UNIDADES_LANCAMENTO.map((u) => (
              <option key={u} value={u}>
                {u}
              </option>
            ))}
          </select>
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
        <FormField label="Data e hora *">
          <input
            required
            type="datetime-local"
            style={inputStyle}
            value={form.dataMovimentacao}
            onChange={set("dataMovimentacao")}
          />
        </FormField>
      </FormGrid>
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
  const [piscinas, setPiscinas] = useState([]);
  const [produtos, setProdutos] = useState([]);
  const [usuarios, setUsuarios] = useState([]);
  const [depositos, setDepositos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [search, setSearch] = useState("");
  const [filtroTipo, setFiltroTipo] = useState("");

  useEffect(() => {
    async function load() {
      try {
        setLoading(true);
        const [m, p, pr, u, d] = await Promise.all([
          movimentacaoService.listar(),
          piscinaService.listar(),
          produtoService.listar(),
          usuarioService.listar(),
          depositoService.listar(),
        ]);
        setMovimentos(m ?? []);
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
      setError(null);
      const nova = await movimentacaoService.criar(dto);
      setMovimentos((prev) => [nova, ...prev]);
      setModalOpen(false);
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  }

  const filtered = movimentos.filter((m) => {
    const txt =
      `${m.produto?.nome} ${m.piscina?.nome} ${m.deposito?.nome}`.toLowerCase();
    return (
      txt.includes(search.toLowerCase()) &&
      (!filtroTipo || m.tipoMovimentacao === parseInt(filtroTipo))
    );
  });

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
      key: "piscina",
      label: "Piscina",
      render: (_, r) => r.piscina?.nome ?? "—",
    },
    {
      key: "tipoMovimentacao",
      label: "Tipo",
      render: (v) => (
        <Badge variant={corDoTipo(v)}>{TIPO_LABELS[v] ?? "—"}</Badge>
      ),
    },
    {
      key: "quantidade",
      label: "Quantidade",
      render: (v, r) => `${v ?? "—"} ${r.unidadeLancamento || ""}`,
    },
    {
      key: "dataMovimentacao",
      label: "Data",
      render: (v) => new Date(v).toLocaleString("pt-BR"),
    },
    {
      key: "usuarios",
      label: "Responsável",
      render: (_, r) => r.usuario?.nome ?? "—",
    },
  ];

  if (loading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader
        title="Movimentações de estoque"
        description="Entradas, saídas, aplicações e ajustes por depósito e produto"
        action={
          <Button variant="primary" onClick={() => setModalOpen(true)}>
            + Registrar movimentação
          </Button>
        }
      />

      {error && <ErrorMessage message={error} />}

      <Toolbar>
        <SearchInput
          value={search}
          onChange={setSearch}
          placeholder="Buscar produto, piscina ou depósito…"
        />
        <FilterSelect
          value={filtroTipo}
          onChange={setFiltroTipo}
          placeholder="Todos os tipos"
          options={Object.entries(TIPO_LABELS).map(([value, label]) => ({
            value,
            label,
          }))}
        />
      </Toolbar>

      <Card noPadding>
        <DataTable
          columns={columns}
          data={filtered}
          emptyMessage="Nenhuma movimentação encontrada."
        />
      </Card>

      <Modal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        title="Registrar movimentação"
      >
        <MovimentacaoForm
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
