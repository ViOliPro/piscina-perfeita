// ============================================================
//  Piscina Perfeita — Módulo: Análises
// ============================================================
import { useState, useEffect } from "react";
import {
  PageHeader, Card, Badge, Button, Modal, Toolbar, SearchInput,
  FilterSelect, DataTable, FormGrid, FormField, FormSection,
  inputStyle, LoadingSpinner, ErrorMessage,
} from "../../components/ui/index.jsx";
import { analiseService, piscinaService, usuarioService } from "../../config/services.js";
import { ANALISE_FAIXAS } from "../../config/index.js";

// ----------------------------------------------------------
// Helpers
// ----------------------------------------------------------
function calcStatus(analise) {
  const { ph, cloroLivre } = analise;
  if (!ph && !cloroLivre) return "muted";
  const phOk    = ph    && ph    >= ANALISE_FAIXAS.ph.min    && ph    <= ANALISE_FAIXAS.ph.max;
  const cloroOk = cloroLivre && cloroLivre >= ANALISE_FAIXAS.cloroLivre.min && cloroLivre <= ANALISE_FAIXAS.cloroLivre.max;
  if (phOk && cloroOk) return "ok";
  if (!phOk)           return "bad";
  return "warn";
}
const STATUS_LABELS = { ok: "Normal", warn: "Atenção", bad: "Ajustar pH", muted: "—" };

// ----------------------------------------------------------
// Formulário de nova análise
// ----------------------------------------------------------
function AnaliseForm({ piscinas, usuarios, onSubmit, onCancel, loading }) {
  const [form, setForm] = useState({
    piscinaId: "", usuarioId: "",
    dataAnalise: new Date().toISOString().slice(0, 16),
    ph: "", cloroLivre: "", alcalinidade: "", temperatura: "", observacoes: "",
  });

  const set = (key) => (e) => setForm((f) => ({ ...f, [key]: e.target.value }));

  function handleSubmit(e) {
    e.preventDefault();
    onSubmit({
      ...form,
      ph:           form.ph          ? parseFloat(form.ph)          : null,
      cloroLivre:   form.cloroLivre  ? parseFloat(form.cloroLivre)  : null,
      alcalinidade: form.alcalinidade? parseFloat(form.alcalinidade) : null,
      temperatura:  form.temperatura ? parseFloat(form.temperatura)  : null,
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
        <FormField label="Responsável *">
          <select required style={inputStyle} value={form.usuarioId} onChange={set("usuarioId")}>
            <option value="">Selecione o usuário</option>
            {usuarios.map((u) => <option key={u.id} value={u.id}>{u.nome}</option>)}
          </select>
        </FormField>
        <FormField label="Data e hora *">
          <input required type="datetime-local" style={inputStyle} value={form.dataAnalise} onChange={set("dataAnalise")} />
        </FormField>
        <div /> {/* spacer */}

        <FormSection label="Parâmetros físico-químicos" />

        <FormField label={`pH  (ideal: ${ANALISE_FAIXAS.ph.min}–${ANALISE_FAIXAS.ph.max})`}>
          <input type="number" step="0.1" min="0" max="14" placeholder="Ex.: 7.4"
            style={inputStyle} value={form.ph} onChange={set("ph")} />
        </FormField>
        <FormField label={`Cloro livre mg/L  (ideal: ${ANALISE_FAIXAS.cloroLivre.min}–${ANALISE_FAIXAS.cloroLivre.max})`}>
          <input type="number" step="0.1" min="0" placeholder="Ex.: 1.2"
            style={inputStyle} value={form.cloroLivre} onChange={set("cloroLivre")} />
        </FormField>
        <FormField label={`Alcalinidade mg/L  (ideal: ${ANALISE_FAIXAS.alcalinidade.min}–${ANALISE_FAIXAS.alcalinidade.max})`}>
          <input type="number" step="1" min="0" placeholder="Ex.: 90"
            style={inputStyle} value={form.alcalinidade} onChange={set("alcalinidade")} />
        </FormField>
        <FormField label={`Temperatura °C  (ideal: ${ANALISE_FAIXAS.temperatura.min}–${ANALISE_FAIXAS.temperatura.max})`}>
          <input type="number" step="0.5" min="0" placeholder="Ex.: 28"
            style={inputStyle} value={form.temperatura} onChange={set("temperatura")} />
        </FormField>
        <FormField label="Observações" fullWidth>
          <textarea style={{ ...inputStyle, minHeight: 70, resize: "vertical" }}
            placeholder="Condições da água, ações tomadas…"
            value={form.observacoes} onChange={set("observacoes")} />
        </FormField>
      </FormGrid>

      <div style={{ display: "flex", justifyContent: "flex-end", gap: 8, marginTop: 16 }}>
        <Button variant="ghost" onClick={onCancel} type="button">Cancelar</Button>
        <Button variant="primary" type="submit" disabled={loading}>
          {loading ? "Salvando…" : "Salvar análise"}
        </Button>
      </div>
    </form>
  );
}

// ----------------------------------------------------------
// Módulo principal
// ----------------------------------------------------------
export default function Analises() {
  const [analises,  setAnalises]  = useState([]);
  const [piscinas,  setPiscinas]  = useState([]);
  const [usuarios,  setUsuarios]  = useState([]);
  const [loading,   setLoading]   = useState(true);
  const [saving,    setSaving]    = useState(false);
  const [error,     setError]     = useState(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [search,    setSearch]    = useState("");
  const [filtroStatus, setFiltroStatus] = useState("");

  useEffect(() => {
    async function load() {
      try {
        setLoading(true);
        const [a, p, u] = await Promise.all([
          analiseService.listar(),
          piscinaService.listar(),
          usuarioService.listar(),
        ]);
        setAnalises(a ?? []);
        setPiscinas(p ?? []);
        setUsuarios(u ?? []);
      } catch (err) { setError(err.message); }
      finally { setLoading(false); }
    }
    load();
  }, []);

  async function handleSave(dto) {
    try {
      setSaving(true);
      const nova = await analiseService.criar(dto);
      setAnalises((prev) => [nova, ...prev]);
      setModalOpen(false);
    } catch (err) { setError(err.message); }
    finally { setSaving(false); }
  }

  async function handleDelete(id) {
    if (!confirm("Excluir esta análise?")) return;
    try {
      await analiseService.excluir(id);
      setAnalises((prev) => prev.filter((a) => a.id !== id));
    } catch (err) { setError(err.message); }
  }

  const filtered = analises.filter((a) => {
    const txt = `${a.piscina?.nome} ${a.usuario?.nome}`.toLowerCase();
    const status = calcStatus(a);
    return txt.includes(search.toLowerCase()) &&
           (!filtroStatus || status === filtroStatus);
  });

  const columns = [
    { key: "piscina",  label: "Piscina",     render: (_, r) => r.piscina?.nome ?? "—" },
    { key: "usuario",  label: "Responsável", render: (_, r) => r.usuario?.nome ?? "—" },
    { key: "dataAnalise", label: "Data",     render: (v) => new Date(v).toLocaleDateString("pt-BR") },
    { key: "ph",           label: "pH",          render: (v) => v ?? "—" },
    { key: "cloroLivre",   label: "Cloro livre", render: (v) => v ? `${v} mg/L` : "—" },
    { key: "alcalinidade",  label: "Alcalinidade",render: (v) => v ? `${v} mg/L` : "—" },
    { key: "temperatura",   label: "Temp.",       render: (v) => v ? `${v}°C` : "—" },
    {
      key: "_status", label: "Status",
      render: (_, r) => {
        const s = calcStatus(r);
        return <Badge variant={s === "muted" ? "info" : s}>{STATUS_LABELS[s]}</Badge>;
      },
    },
    {
      key: "_acoes", label: "",
      render: (_, r) => (
        <Button variant="danger" size="sm" onClick={() => handleDelete(r.id)}>Excluir</Button>
      ),
    },
  ];

  if (loading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader
        title="Análises"
        description="Registro de qualidade da água por piscina"
        action={
          <Button variant="primary" onClick={() => setModalOpen(true)}>
            + Nova análise
          </Button>
        }
      />

      {error && <ErrorMessage message={error} />}

      <Toolbar>
        <SearchInput value={search} onChange={setSearch} placeholder="Buscar piscina ou responsável…" />
        <FilterSelect
          value={filtroStatus} onChange={setFiltroStatus}
          placeholder="Todos os status"
          options={[
            { value: "ok",   label: "Normal" },
            { value: "warn", label: "Atenção" },
            { value: "bad",  label: "Ajustar pH" },
          ]}
        />
      </Toolbar>

      <Card noPadding>
        <DataTable
          columns={columns}
          data={filtered}
          emptyMessage="Nenhuma análise encontrada com os filtros aplicados."
        />
      </Card>

      <Modal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        title="Nova análise"
      >
        <AnaliseForm
          piscinas={piscinas}
          usuarios={usuarios}
          onSubmit={handleSave}
          onCancel={() => setModalOpen(false)}
          loading={saving}
        />
      </Modal>
    </div>
  );
}
