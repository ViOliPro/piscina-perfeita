import { Card, KpiCard } from "../../../components/ui/index.jsx";
import {
  formatarDataHora,
  formatarMetrosCubicos,
  formatarNumero,
} from "../helpers/hidrometroUtils.js";
import styles from "./components.module.css";

export function HidrometroDashboard({ dashboard }) {
  if (!dashboard) {
    return (
      <Card title="Indicadores do hidrômetro">
        Os indicadores estarão disponíveis quando a API de dashboard for
        habilitada.
      </Card>
    );
  }

  return (
    <section
      aria-label="Indicadores do hidrômetro"
      className={styles.dashboardSection}
    >
      <div className={styles.dashboardGrid}>
        <KpiCard
          label="Última leitura"
          value={formatarMetrosCubicos(dashboard.ultimaLeitura)}
          subLabel={formatarDataHora(dashboard.dataUltimaLeitura)}
        />
        <KpiCard
          label="Último consumo"
          value={formatarMetrosCubicos(dashboard.ultimoConsumo)}
          subLabel={
            dashboard.periodoUltimoConsumo ?? "Desde a leitura anterior"
          }
        />
        <KpiCard
          label="Consumo médio"
          value={formatarMetrosCubicos(dashboard.consumoMedio)}
          subLabel={dashboard.periodoMedia ?? "Período informado pela API"}
        />
        <KpiCard
          label="Consumo no mês"
          value={formatarMetrosCubicos(dashboard.consumoMes)}
          subLabel={dashboard.mesReferencia ?? "Mês atual"}
        />
        <KpiCard
          label="Dias sem leitura"
          value={formatarNumero(dashboard.diasSemLeitura, 0)}
          subLabel={
            dashboard.diasSemLeitura === 0
              ? "Leitura atualizada"
              : "Desde o último registro"
          }
          subVariant={dashboard.diasSemLeitura > 1 ? "warn" : "ok"}
        />
      </div>
    </section>
  );
}
