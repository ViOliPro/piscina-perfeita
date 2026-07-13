// ============================================================
//  Piscina Perfeita — Módulo: Aplicações de Produto
//
//  Registra o uso de um produto em uma piscina (ex.: "500 mL de Algicida
//  na Piscina X"). Ao salvar, o backend automaticamente cria uma
//  MovimentacaoEstoque (Tipo=Aplicação) e dá baixa no Estoque do Depósito
//  de onde o produto saiu — convertendo a unidade se necessário (ex.:
//  produto cadastrado em L, aplicação lançada em mL).
// ============================================================
import { useState, useEffect } from "react";
import {
  PageHeader, Card, Button, Modal, Toolbar,
  SearchInput, DataTable, FormGrid, FormField,
  inputStyle, LoadingSpinner, ErrorMessage,
} from "../../components/ui/index.jsx";
import {
  aplicacaoProdutoService, piscinaService, produtoService,
  depositoService, analiseService,
} from "../../config/services.js";
import { UNIDADES_LANCAMENTO } from "../../config/index.js";
import { useAuth } from "../../context/AuthContext.jsx";

// ----------------------------------------------------------
// Formulário
// ----------------------------------------------------------
function AplicacaoForm({
  piscinas, produtos, depositos, analises, initial, onSubmit, onCancel, loading,
}) {
  const [form, setForm] = useState(
    initial ?? {
      piscinaId: "", produtoId: "", depositoId: "",
      quantidade: "", unidadeLancamento: "", analiseId: "",
      dataAplicacao: new Date().toISOString().slice(0, 16),
      observacoes: "",
    }
  );
  const set = (key) => (e) => setForm((f) => ({ ...f, [key]: e.target.value }));
  const produtoSelecionado = produtos.find((p) => p.id === form.produtoId);

  function handleSubmit(e) {
    e.preventDefault();
    onSubmit({ ...form, analiseId: form.analiseId || null });
  }

  // Análises da piscina selecionada — ajuda a vincular a aplicação ao
  // motivo real (ex.: pH baixo → aplicação de elevador de pH).
  const analisesDaPiscina = analises.filter((a) => a.piscinaId === form.piscinaId);

  return (
    <form onSubmit={handleSubmit}>
      <FormGrid>
        <FormField label="Piscina *">
          <select required style={inputStyle} value={form.piscinaId} onChange={set("piscinaId")}>
            <option value="">Selecione a piscina</option>
            {piscinas.map((p) => <option key={p.id} value={p.id}>{p.nome}</option>)}
          </select>
        </FormField>
        <FormField label="Análise relacionada (opcional)">
          <select style={inputStyle} value={form.analiseId} onChange={set("analiseId")}>
            <option value="">Nenhuma</option>
            {analisesDaPiscina.map((a) => (
              <option key={a.id} value={a.id}>
                {new Date(a.dataAnalise).toLocaleDateString("pt-BR")} — pH {a.ph ?? "—"} / Cloro {a.cloroLivre ?? "—"}
              </option>
            ))}
          </select>
        </FormField>
        <FormField label="Depósito *">
          <select required style={inputStyle} value={form.depositoId} onChange={set("depositoId")}>
            <option value="">Selecione o depósito</option>
            {depositos.map((d) => <option key={d.id} value={d.id}>{d.nome}</option>)}
          </select>
        </FormField>
        <FormField label="Produto *">
          <select required style={inputStyle} value={form.produtoId} onChange={set("produtoId")}>
            <option value="">Selecione o produto</option>
            {produtos.map((p) => <option key={p.id} value={p.id}>{p.nome} ({p.unidadeMedida})</option>)}
          </select>
        </FormField>
        <FormField label="Quantidade aplicada *">
          <input required type="number" step="0.0001" min="0.0001" placeholder="Ex.: 500"
            style={inputStyle} value={form.quantidade} onChange={set("quantidade")} />
        </FormField>
        <FormField label="Unidade da aplicação">
          <select style={inputStyle} value={form.unidadeLancamento} onChange={set("unidadeLancamento")}>
            <option value="">
              {produtoSelecionado ? `Mesma do produto (${produtoSelecionado.unidadeMedida})` : "Mesma unidade do produto"}
            </option>
            {UNIDADES_LANCAMENTO.map((u) => <option key={u} value={u}>{u}</option>)}
          </select>
        </FormField>
        <FormField label="Data e hora *">
          <input required type="datetime-local" style={inputStyle}
            value={form.dataAplicacao} onChange={set("dataAplicacao")} />
        </FormField>
        <FormField label="Observações" fullWidth>
          <input type="text" placeholder="Detalhes adicionais sobre a aplicação"
            style={inputStyle} value={form.observacoes} onChange={set("observacoes")} />
        </FormField>
      </FormGrid>
      <div style={{ display: "flex", justifyContent: "flex-end", gap: 8, marginTop: 16 }}>
        <Button variant="ghost" onClick={onCancel} type="button">Cancelar</Button>
        <Button variant="primary" type="submit" disabled={loading}>
          {loading ? "Salvando…" : "Registrar aplicação"}
        </Button>
      </div>
    </form>
  );
}

// ----------------------------------------------------------
// Módulo principal
// ----------------------------------------------------------
export default function Aplicacoes({ prefill, onPrefillConsumed }) {
  const { user } = useAuth();
  const [aplicacoes, setAplicacoes] = useState([]);
  const [piscinas, setPiscinas] = useState([]);
  const [produtos, setProdutos] = useState([]);
  const [depositos, setDepositos] = useState([]);
  const [analises, setAnalises] = useState([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [modal, setModal] = useState({ open: false, initial: null });
  const [search, setSearch] = useState("");

  useEffect(() => {
    async function load() {
      try {
        setLoading(true);
        const [ap, p, pr, d, an] = await Promise.all([
          aplicacaoProdutoService.listar(),
          piscinaService.listar(),
          produtoService.listar(),
          depositoService.listar(),
          analiseService.listar(),
        ]);
        setAplicacoes(ap ?? []);
        setPiscinas(p ?? []);
        setProdutos(pr ?? []);
        setDepositos(d ?? []);
        setAnalises(an ?? []);
      } catch (err) { setError(err.message); }
      finally { setLoading(false); }
    }
    load();
  }, []);

  // Vindo do botão "Registrar aplicação" na tela de Análises: abre o modal
  // já com a piscina e a análise pré-selecionadas.
  useEffect(() => {
    if (prefill) {
      setModal({
        open: true,
        initial: {
          piscinaId: prefill.piscinaId ?? "", produtoId: "", depositoId: "",
          quantidade: "", unidadeLancamento: "", analiseId: prefill.analiseId ?? "",
          dataAplicacao: new Date().toISOString().slice(0, 16), observacoes: "",
        },
      });
      onPrefillConsumed?.();
    }
  }, [prefill]);

  async function handleSave(dto) {
    try {
      setSaving(true);
      setError(null);
      const nova = await aplicacaoProdutoService.criar({ ...dto, usuarioId: user?.userId });
      setAplicacoes((prev) => [nova, ...prev]);
      setModal({ open: false, initial: null });
    } catch (err) { setError(err.message); }
    finally { setSaving(false); }
  }

  const filtered = aplicacoes.filter((a) =>
    `${a.produto?.nome} ${a.piscina?.nome}`.toLowerCase().includes(search.toLowerCase())
  );

  const columns = [
    { key: "piscina",  label: "Piscina",  render: (_, r) => r.piscina?.nome ?? "—" },
    { key: "produto",  label: "Produto",  render: (_, r) => r.produto?.nome ?? "—" },
    { key: "deposito", label: "Depósito", render: (_, r) => r.deposito?.nome ?? "—" },
    {
      key: "quantidade", label: "Quantidade",
      render: (v, r) => `${v} ${r.unidadeLancamento || ""}`,
    },
    {
      key: "dataAplicacao", label: "Data",
      render: (v) => new Date(v).toLocaleString("pt-BR"),
    },
    {
      key: "analiseId", label: "Análise relacionada",
      render: (v) => v ? "Vinculada" : "—",
    },
  ];

  if (loading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader
        title="Aplicações de produto"
        description="Registre o uso de produtos nas piscinas — o estoque é atualizado automaticamente"
        action={
          <Button variant="primary" onClick={() => setModal({ open: true, initial: null })}>
            + Registrar aplicação
          </Button>
        }
      />

      {error && <ErrorMessage message={error} />}

      <Toolbar>
        <SearchInput value={search} onChange={setSearch} placeholder="Buscar produto ou piscina…" />
      </Toolbar>

      <Card noPadding>
        <DataTable columns={columns} data={filtered} emptyMessage="Nenhuma aplicação registrada." />
      </Card>

      <Modal
        open={modal.open}
        onClose={() => setModal({ open: false, initial: null })}
        title="Registrar aplicação de produto"
      >
        <AplicacaoForm
          piscinas={piscinas}
          produtos={produtos}
          depositos={depositos}
          analises={analises}
          initial={modal.initial}
          onSubmit={handleSave}
          onCancel={() => setModal({ open: false, initial: null })}
          loading={saving}
        />
      </Modal>
    </div>
  );
}
