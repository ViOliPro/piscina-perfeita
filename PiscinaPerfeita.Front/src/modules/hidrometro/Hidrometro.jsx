// ============================================================
//  Piscina Perfeita — Módulo: Lançamentos de Hidrômetro
// ------------------------------------------------------------
//  Cadastro simples (leitura atual em m³ + data) que vira uma
//  visão analítica: consumo por lançamento, consumo médio e
//  total acumulado no mês.
//
//  ⚠️ API ainda não existe. Este módulo usa dados mock em memória
//  (MOCK_LANCAMENTOS) só pra a tela ser navegável. Quando o backend
//  estiver pronto, o plano é espelhar exatamente o padrão de
//  analiseService (config/services.js): listar/criar/excluir batendo
//  em API_ENDPOINTS.hidrometro, com fromApi/toApi em mappers.js.
//  Até lá, NENHUM dado aqui é persistido de verdade (some no refresh).
// ============================================================
import { useMemo, useState } from "react";
import {
  PageHeader,
  Card,
  KpiCard,
  Button,
  Modal,
  Toolbar,
  FilterSelect,
  DataTable,
  FormGrid,
  FormField,
  ErrorMessage,
} from "../../components/ui/index.jsx";
import { inputStyle } from "../../components/ui/styles.js";
import { PERMISSIONS } from "../../helpers/Permissions.js";
import ProtecaoDeRota from "../../helpers/ProtecaoDeRota.jsx";

// ----------------------------------------------------------
// Mock inicial — apenas para visualização da tela
// ----------------------------------------------------------
const MOCK_LANCAMENTOS = [
  { id: 1, dataLeitura: "2023-08-18", leituraAtual: 812.4 },
  { id: 2, dataLeitura: "2023-09-18", leituraAtual: 824.7 },
  { id: 3, dataLeitura: "2023-10-01", leituraAtual: 830.9 },
  { id: 4, dataLeitura: "2023-10-18", leituraAtual: 839.2 },
];

function hojeISO() {
  return new Date().toISOString().slice(0, 10);
}

function mesLabel(isoDate) {
  const [ano, mes] = isoDate.split("-");
  const nomes = [
    "Jan",
    "Fev",
    "Mar",
    "Abr",
    "Mai",
    "Jun",
    "Jul",
    "Ago",
    "Set",
    "Out",
    "Nov",
    "Dez",
  ];
  return `${nomes[Number(mes) - 1]}/${ano}`;
}

// ----------------------------------------------------------
// Deriva o consumo de cada lançamento a partir do anterior
// (ordenado por data crescente). O primeiro lançamento da
// série não tem "anterior", então consumo fica null.
// ----------------------------------------------------------
function comConsumoDerivado(lancamentos) {
  const ordenados = [...lancamentos].sort(
    (a, b) => new Date(a.dataLeitura) - new Date(b.dataLeitura),
  );
  return ordenados.map((l, i) => ({
    ...l,
    consumo:
      i === 0
        ? null
        : +(l.leituraAtual - ordenados[i - 1].leituraAtual).toFixed(2),
  }));
}

// ----------------------------------------------------------
// Formulário de novo lançamento (2 campos, como pedido)
// ----------------------------------------------------------
function LancamentoForm({ ultimaLeitura, onSubmit, onCancel, loading }) {
  const [form, setForm] = useState({
    dataLeitura: hojeISO(),
    leituraAtual: "",
  });
  const [erro, setErro] = useState(null);

  const set = (key) => (e) => setForm((f) => ({ ...f, [key]: e.target.value }));

  function handleSubmit(e) {
    e.preventDefault();
    const valor = parseFloat(form.leituraAtual);

    if (Number.isNaN(valor)) {
      setErro("Informe um valor numérico válido para a leitura.");
      return;
    }
    if (ultimaLeitura != null && valor < ultimaLeitura) {
      setErro(
        `A leitura deve ser maior ou igual à última registrada (${ultimaLeitura} m³) — hidrômetro não regride.`,
      );
      return;
    }
    setErro(null);
    onSubmit({ dataLeitura: form.dataLeitura, leituraAtual: valor });
  }

  return (
    <form onSubmit={handleSubmit}>
      {erro && <ErrorMessage message={erro} />}
      <FormGrid>
        <FormField label="Data da leitura *">
          <input
            type="date"
            required
            style={inputStyle}
            value={form.dataLeitura}
            onChange={set("dataLeitura")}
          />
        </FormField>
        <FormField label="Leitura atual (m³) *">
          <input
            type="number"
            step="0.01"
            min="0"
            required
            placeholder={
              ultimaLeitura != null
                ? `Ex.: ${ultimaLeitura + 5}`
                : "Ex.: 812.40"
            }
            style={inputStyle}
            value={form.leituraAtual}
            onChange={set("leituraAtual")}
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
        <Button variant="ghost" type="button" onClick={onCancel}>
          Cancelar
        </Button>
        <Button variant="primary" type="submit" disabled={loading}>
          {loading ? "Salvando…" : "Salvar lançamento"}
        </Button>
      </div>
    </form>
  );
}

// ----------------------------------------------------------
// Módulo principal
// ----------------------------------------------------------
export default function Hidrometro() {
  const [lancamentos, setLancamentos] = useState(MOCK_LANCAMENTOS);
  const [modalOpen, setModalOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [filtroMes, setFiltroMes] = useState("");

  const comConsumo = useMemo(
    () => comConsumoDerivado(lancamentos),
    [lancamentos],
  );

  const opcoesMes = useMemo(() => {
    const meses = [
      ...new Set(comConsumo.map((l) => l.dataLeitura.slice(0, 7))),
    ];
    return meses
      .sort()
      .reverse()
      .map((ym) => ({ value: ym, label: mesLabel(`${ym}-01`) }));
  }, [comConsumo]);

  const exibidos = useMemo(() => {
    const lista = filtroMes
      ? comConsumo.filter((l) => l.dataLeitura.startsWith(filtroMes))
      : comConsumo;
    return [...lista].sort(
      (a, b) => new Date(b.dataLeitura) - new Date(a.dataLeitura),
    );
  }, [comConsumo, filtroMes]);

  // KPIs sempre calculados sobre a série completa, não sobre o filtro
  const comConsumoValido = comConsumo.filter((l) => l.consumo != null);
  const ultimo = comConsumoValido.at(-1);
  const penultimo = comConsumoValido.at(-2);
  const deltaUltimo =
    ultimo && penultimo
      ? +(ultimo.consumo - penultimo.consumo).toFixed(2)
      : null;

  const mediaConsumo =
    comConsumoValido.length > 0
      ? +(
          comConsumoValido.reduce((acc, l) => acc + l.consumo, 0) /
          comConsumoValido.length
        ).toFixed(2)
      : null;

  const mesAtual = hojeISO().slice(0, 7);
  const totalMesAtual = comConsumoValido
    .filter((l) => l.dataLeitura.startsWith(mesAtual))
    .reduce((acc, l) => acc + l.consumo, 0);

  function handleSave(dto) {
    setSaving(true);
    try {
      // TODO(API): trocar por hidrometroService.criar(dto) quando o
      // backend existir — mesmo formato usado por analiseService.criar.
      const novo = { id: Date.now(), ...dto };
      setLancamentos((prev) => [...prev, novo]);
      setModalOpen(false);
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  }

  function handleDelete(id) {
    if (!confirm("Excluir este lançamento?")) return;
    // TODO(API): trocar por hidrometroService.excluir(id).
    setLancamentos((prev) => prev.filter((l) => l.id !== id));
  }

  const columns = [
    {
      key: "dataLeitura",
      label: "Data da leitura",
      render: (v) => new Date(`${v}T00:00:00`).toLocaleDateString("pt-BR"),
    },
    {
      key: "leituraAtual",
      label: "Leitura (m³)",
      render: (v) => v.toFixed(2),
    },
    {
      key: "consumo",
      label: "Consumo (m³)",
      render: (v) => (v == null ? "—" : `${v > 0 ? "+" : ""}${v.toFixed(2)}`),
    },
    {
      key: "_acoes",
      label: "",
      render: (_, r) => (
        <Button variant="danger" size="sm" onClick={() => handleDelete(r.id)}>
          Excluir
        </Button>
      ),
    },
  ];

  return (
    <ProtecaoDeRota permissao={PERMISSIONS.HIDROMETRO.VIEW}>
      <div>
        <PageHeader
          title="Lançamentos de Hidrômetro"
          description="Leituras do hidrômetro e consumo de água calculado entre lançamentos"
          action={
            <Button
              variant="primary"
              onClick={() => setModalOpen(true)}
              permission={PERMISSIONS.HIDROMETRO.CREATE}
            >
              + Novo lançamento
            </Button>
          }
        />

        {error && <ErrorMessage message={error} />}

        <div
          style={{
            display: "grid",
            gridTemplateColumns: "repeat(auto-fit, minmax(180px, 1fr))",
            gap: 12,
            marginBottom: 16,
          }}
        >
          <KpiCard
            label="Último consumo"
            value={ultimo ? `${ultimo.consumo.toFixed(2)} m³` : "—"}
            subLabel={
              deltaUltimo == null
                ? "Sem lançamento anterior suficiente"
                : `${deltaUltimo > 0 ? "+" : ""}${deltaUltimo.toFixed(2)} m³ vs. anterior`
            }
            subVariant={
              deltaUltimo == null ? "muted" : deltaUltimo > 0 ? "warn" : "ok"
            }
          />
          <KpiCard
            label="Consumo médio"
            value={mediaConsumo != null ? `${mediaConsumo.toFixed(2)} m³` : "—"}
            subLabel={
              comConsumoValido.length > 0
                ? `Baseado em ${comConsumoValido.length} lançamento(s)`
                : "Sem dados suficientes"
            }
          />
          <KpiCard
            label="Total acumulado no mês"
            value={`${totalMesAtual.toFixed(2)} m³`}
            subLabel={mesLabel(`${mesAtual}-01`)}
          />
        </div>

        <Toolbar>
          <FilterSelect
            value={filtroMes}
            onChange={setFiltroMes}
            placeholder="Todos os lançamentos"
            options={opcoesMes}
          />
        </Toolbar>

        <Card title={`Lista de lançamentos (${exibidos.length})`} noPadding>
          <DataTable
            columns={columns}
            data={exibidos}
            emptyMessage="Nenhum lançamento encontrado para o período selecionado."
          />
        </Card>

        <Modal
          open={modalOpen}
          onClose={() => setModalOpen(false)}
          title="Novo lançamento de hidrômetro"
        >
          <LancamentoForm
            ultimaLeitura={comConsumo.at(-1)?.leituraAtual ?? null}
            onSubmit={handleSave}
            onCancel={() => setModalOpen(false)}
            loading={saving}
          />
        </Modal>
      </div>
    </ProtecaoDeRota>
  );
}
