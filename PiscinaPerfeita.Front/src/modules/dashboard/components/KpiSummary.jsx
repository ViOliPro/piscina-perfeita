import { KpiCard } from "../../../components/ui/index.jsx";

export function KpiSummary({
  totalPiscinas,
  analisesHoje,
  estoqueBaixoQtd,
  movimentacoesQtd,
}) {
  return (
    <div className="pp-kpi-grid">
      <KpiCard
        label="Piscinas"
        value={totalPiscinas}
        subLabel="cadastradas"
        subVariant="muted"
      />
      <KpiCard
        label="Análises hoje"
        value={analisesHoje}
        subLabel={`+${analisesHoje} vs ontem`}
        subVariant="ok"
      />
      <KpiCard
        label="Estoque baixo"
        value={estoqueBaixoQtd}
        subLabel={estoqueBaixoQtd > 0 ? "requer atenção" : "tudo ok"}
        subVariant={estoqueBaixoQtd > 0 ? "warn" : "ok"}
      />
      <KpiCard
        label="Movimentações"
        value={movimentacoesQtd}
        subLabel="esta semana"
        subVariant="muted"
      />
    </div>
  );
}
