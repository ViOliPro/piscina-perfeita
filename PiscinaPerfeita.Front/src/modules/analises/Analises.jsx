// ============================================================
//  Piscina Perfeita — Módulo: Análises
// ============================================================
import { useState, lazy, Suspense } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
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
  FormSection,
  LoadingSpinner,
  ErrorMessage,
} from "../../components/ui/index.jsx";
import { inputStyle } from "../../components/ui/styles.js";
import { analiseService, piscinaService } from "../../config/services.js";
import { qk, diasAtrasISO } from "../../helpers/queryKeys.js";
import { ANALISE_FAIXAS } from "../../config/index.js";
import { getLocalDateTimeInput } from "../../utils/getLocalDateTimeInput.js";
import { PERMISSIONS } from "../../helpers/Permissions.js";
import ProtecaoDeRota from "../../helpers/ProtecaoDeRota.jsx";
import { useUsuariosSelecionaveis } from "../../hooks/useUsuariosSelecionaveis.js";

// Lazy: o ApexCharts sozinho quase triplicou o bundle (~310KB → ~1.23MB).
// Com import() em vez de import estático, esse peso só é baixado quando a
// tela de Análises efetivamente monta o card — não em toda visita ao app,
// nem em telas que nunca chegam a renderizar este componente.
const QualidadeAguaHistoricoCard = lazy(
  () => import("./QualidadeAguaHistoricoCard.jsx"),
);

// ----------------------------------------------------------
// Helpers
// ----------------------------------------------------------
function calcStatus(analise) {
  const { ph, cloroLivre } = analise;
  if (!ph && !cloroLivre) return "muted";
  const phOk = ph && ph >= ANALISE_FAIXAS.ph.min && ph <= ANALISE_FAIXAS.ph.max;
  const cloroOk =
    cloroLivre &&
    cloroLivre >= ANALISE_FAIXAS.cloroLivre.min &&
    cloroLivre <= ANALISE_FAIXAS.cloroLivre.max;
  if (phOk && cloroOk) return "ok";
  if (!phOk) return "bad";
  return "warn";
}
const STATUS_LABELS = {
  ok: "Normal",
  warn: "Atenção",
  bad: "Ajustar pH",
  muted: "—",
};

// ----------------------------------------------------------
// Formulário de nova análise
// ----------------------------------------------------------
function AnaliseForm({ piscinas, onSubmit, onCancel, loading }) {
  const [form, setForm] = useState({
    piscinaId: "",
    usuarioId: "",
    dataAnalise: getLocalDateTimeInput(),
    ph: "",
    cloroLivre: "",
    alcalinidade: "",
    temperatura: "",
    observacoes: "",
  });

  const set = (key) => (e) => setForm((f) => ({ ...f, [key]: e.target.value }));

  const { usuarios, podeVerUsuario } = useUsuariosSelecionaveis();

  function handleSubmit(e) {
    e.preventDefault();
    onSubmit({
      ...form,
      ph: form.ph ? parseFloat(form.ph) : null,
      cloroLivre: form.cloroLivre ? parseFloat(form.cloroLivre) : null,
      alcalinidade: form.alcalinidade ? parseFloat(form.alcalinidade) : null,
      temperatura: form.temperatura ? parseFloat(form.temperatura) : null,
      // Operador/Visualizador nunca enviam usuarioId — mesmo que o campo
      // nunca apareça na UI para esses perfis, garantimos aqui que o
      // payload nunca carregue um valor residual.
      usuarioId: podeVerUsuario && form.usuarioId ? form.usuarioId : null,
    });
  }

  return (
    <form onSubmit={handleSubmit}>
      <FormGrid>
        <FormField label="Piscina *">
          <select
            required
            style={inputStyle}
            value={form.piscinaId}
            onChange={set("piscinaId")}
          >
            <option value="">Selecione a piscina</option>
            {piscinas.map((p) => (
              <option key={p.id} value={p.id}>
                {p.nome}
              </option>
            ))}
          </select>
        </FormField>
        {podeVerUsuario && (
          <FormField label="Responsável">
            <select
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
        )}
        <FormField label="Data e hora">
          <input
            type="datetime-local"
            style={inputStyle}
            value={form.dataAnalise}
            onChange={set("dataAnalise")}
          />
        </FormField>
        <div /> {/* spacer */}
        <FormSection label="Parâmetros físico-químicos" />
        <FormField
          label={`pH  (ideal: ${ANALISE_FAIXAS.ph.min}–${ANALISE_FAIXAS.ph.max})`}
        >
          <input
            type="number"
            step="0.1"
            min="0"
            max="14"
            placeholder="Ex.: 7.4"
            style={inputStyle}
            value={form.ph}
            onChange={set("ph")}
          />
        </FormField>
        <FormField
          label={`Cloro livre mg/L  (ideal: ${ANALISE_FAIXAS.cloroLivre.min}–${ANALISE_FAIXAS.cloroLivre.max})`}
        >
          <input
            type="number"
            step="0.1"
            min="0"
            placeholder="Ex.: 1.2"
            style={inputStyle}
            value={form.cloroLivre}
            onChange={set("cloroLivre")}
          />
        </FormField>
        <FormField
          label={`Alcalinidade mg/L  (ideal: ${ANALISE_FAIXAS.alcalinidade.min}–${ANALISE_FAIXAS.alcalinidade.max})`}
        >
          <input
            type="number"
            step="1"
            min="0"
            placeholder="Ex.: 90"
            style={inputStyle}
            value={form.alcalinidade}
            onChange={set("alcalinidade")}
          />
        </FormField>
        <FormField
          label={`Temperatura °C  (ideal: ${ANALISE_FAIXAS.temperatura.min}–${ANALISE_FAIXAS.temperatura.max})`}
        >
          <input
            type="number"
            step="0.5"
            min="0"
            placeholder="Ex.: 28"
            style={inputStyle}
            value={form.temperatura}
            onChange={set("temperatura")}
          />
        </FormField>
        <FormField label="Observações" fullWidth>
          <textarea
            style={{ ...inputStyle, minHeight: 70, resize: "vertical" }}
            placeholder="Condições da água, ações tomadas…"
            value={form.observacoes}
            onChange={set("observacoes")}
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
        <Button
          variant="ghost"
          onClick={onCancel}
          type="button"
          permission={PERMISSIONS.ANALISES.CREATE}
        >
          Cancelar
        </Button>
        <Button
          variant="primary"
          type="submit"
          disabled={loading}
          permission={PERMISSIONS.ANALISES.CREATE}
        >
          {loading ? "Salvando…" : "Salvar análise"}
        </Button>
      </div>
    </form>
  );
}

// ----------------------------------------------------------
// Módulo principal
// ----------------------------------------------------------
export default function Analises({ onRegistrarAplicacao }) {
  const queryClient = useQueryClient();
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [search, setSearch] = useState("");
  const [filtroStatus, setFiltroStatus] = useState("");
  const [piscinaHistorico, setPiscinaHistorico] = useState("");

  const filtrosAnalises = { dataInicio: diasAtrasISO(30) };

  const {
    data: analises = [],
    isLoading: loadingAnalises,
    error: errorAnalises,
  } = useQuery({
    queryKey: qk.analises(filtrosAnalises),
    queryFn: () => analiseService.listar(filtrosAnalises),
  });

  const {
    data: piscinas = [],
    isLoading: loadingPiscinas,
    error: errorPiscinas,
  } = useQuery({
    queryKey: qk.piscinas,
    queryFn: () => piscinaService.listar(),
    staleTime: 10 * 60_000,
  });

  const loading = loadingAnalises || loadingPiscinas;

  const piscinaHistoricoEfetivo = piscinaHistorico || (piscinas[0]?.id ?? "");

  async function handleSave(dto) {
    try {
      setSaving(true);
      setError(null);
      await analiseService.criar(dto);
      await queryClient.invalidateQueries({ queryKey: ["analises"] });
      setModalOpen(false);
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(id) {
    if (!confirm("Excluir esta análise?")) return;
    try {
      setError(null);
      await analiseService.excluir(id);
      await queryClient.invalidateQueries({ queryKey: ["analises"] });
    } catch (err) {
      setError(err.message);
    }
  }

  const filtered = analises.filter((a) => {
    const txt = `${a.piscina?.nome} ${a.usuario?.nome}`.toLowerCase();
    const status = calcStatus(a);
    return (
      txt.includes(search.toLowerCase()) &&
      (!filtroStatus || status === filtroStatus)
    );
  });

  const columns = [
    {
      key: "piscina",
      label: "Piscina",
      render: (_, r) => r.piscina?.nome ?? "—",
    },
    {
      key: "usuario",
      label: "Responsável",
      render: (_, r) => r.usuario?.nome ?? "—",
    },
    {
      key: "dataAnalise",
      label: "Data",
      render: (v) => new Date(v).toLocaleDateString("pt-BR"),
    },
    { key: "ph", label: "pH", render: (v) => v ?? "—" },
    {
      key: "cloroLivre",
      label: "Cloro livre",
      render: (v) => (v ? `${v} mg/L` : "—"),
    },
    {
      key: "alcalinidade",
      label: "Alcalinidade",
      render: (v) => (v ? `${v} mg/L` : "—"),
    },
    { key: "temperatura", label: "Temp.", render: (v) => (v ? `${v}°C` : "—") },
    {
      key: "_status",
      label: "Status",
      render: (_, r) => {
        const s = calcStatus(r);
        return (
          <Badge variant={s === "muted" ? "info" : s}>{STATUS_LABELS[s]}</Badge>
        );
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
            onClick={() => onRegistrarAplicacao?.(r?.piscina?.id, r.id)}
            title="Registrar a aplicação de um produto motivada por esta análise"
            permission={PERMISSIONS.ANALISES.CREATE}
          >
            Registrar aplicação
          </Button>
          <Button
            variant="danger"
            size="sm"
            onClick={() => handleDelete(r.id)}
            permission={PERMISSIONS.ANALISES.CREATE}
          >
            Excluir
          </Button>
        </div>
      ),
    },
  ];

  if (loading) return <LoadingSpinner />;

  return (
    <ProtecaoDeRota permissao={PERMISSIONS.ANALISES.VIEW}>
      <div>
        <PageHeader
          title="Análises"
          description="Registro de qualidade da água por piscina"
          action={
            <Button
              variant="primary"
              onClick={() => setModalOpen(true)}
              permission={PERMISSIONS.ANALISES.CREATE}
            >
              + Nova análise
            </Button>
          }
        />

        {(error || errorAnalises || errorPiscinas) && (
          <ErrorMessage
            message={error || errorAnalises?.message || errorPiscinas?.message}
          />
        )}

        <Toolbar>
          <SearchInput
            value={search}
            onChange={setSearch}
            placeholder="Buscar piscina ou responsável…"
          />
          <FilterSelect
            value={filtroStatus}
            onChange={setFiltroStatus}
            placeholder="Todos os status"
            options={[
              { value: "ok", label: "Normal" },
              { value: "warn", label: "Atenção" },
              { value: "bad", label: "Ajustar pH" },
            ]}
          />
        </Toolbar>

        {piscinas.length > 0 && (
          <div style={{ marginBottom: 16 }}>
            <label
              style={{
                display: "block",
                fontSize: 12,
                fontWeight: 600,
                marginBottom: 6,
                color: "#0A1628",
              }}
            >
              Histórico de qualidade da água
            </label>
            <select
              style={{ ...inputStyle, marginBottom: 10, maxWidth: 320 }}
              value={piscinaHistoricoEfetivo}
              onChange={(e) => setPiscinaHistorico(e.target.value)}
            >
              {piscinas.map((p) => (
                <option key={p.id} value={p.id}>
                  {p.nome}
                </option>
              ))}
            </select>
            {piscinaHistoricoEfetivo && (
              <Suspense
                fallback={
                  <p style={{ color: "#6B8CAE", fontSize: 13 }}>
                    Carregando gráfico...
                  </p>
                }
              >
                <QualidadeAguaHistoricoCard
                  key={piscinaHistoricoEfetivo}
                  piscinaId={piscinaHistoricoEfetivo}
                  piscinaNome={
                    piscinas.find((p) => p.id === piscinaHistoricoEfetivo)
                      ?.nome ?? ""
                  }
                />
              </Suspense>
            )}
          </div>
        )}

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
            onSubmit={handleSave}
            onCancel={() => setModalOpen(false)}
            loading={saving}
          />
        </Modal>
      </div>
    </ProtecaoDeRota>
  );
}
