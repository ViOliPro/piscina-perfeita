import { Card } from "../../../components/ui/index.jsx";
import { ANALISE_FAIXAS } from "../../../config/index.js";
import { ParametroGauge } from "./ParametroGauge.jsx";
import { PhScale } from "./PhScale.jsx";
import styles from "./components.module.css";
import { useSuspenseQuery } from "@tanstack/react-query";
import { analiseService } from "../../../config/services.js";
import { qk } from "../../../helpers/queryKeys.js";

export function QualidadeAguaCard() {
  const { data: analises } = useSuspenseQuery({
    queryKey: qk.analises({ limit: 10 }),
    queryFn: () => analiseService.listar({ limit: 10 }),
  });
  const ultimaAnalise = analises[0];

  return (
    <Card
      title="Qualidade da água"
      titleExtra={
        <span className={styles.qualidadeSubtitle}>
          {ultimaAnalise?.piscina?.nome ?? "—"}
        </span>
      }
    >
      {ultimaAnalise ? (
        <>
          <div className={styles.qualidadeGaugesRow}>
            <ParametroGauge
              label="pH"
              value={ultimaAnalise.ph}
              faixa={ANALISE_FAIXAS.ph}
            />
            <ParametroGauge
              label="Cloro livre"
              value={ultimaAnalise.cloroLivre}
              faixa={ANALISE_FAIXAS.cloroLivre}
              unidade=" mg/L"
            />
            <ParametroGauge
              label="Alcalinidade"
              value={ultimaAnalise.alcalinidade}
              faixa={ANALISE_FAIXAS.alcalinidade}
              unidade=" mg/L"
            />
            <ParametroGauge
              label="Temperatura"
              value={ultimaAnalise.temperatura}
              faixa={ANALISE_FAIXAS.temperatura}
              unidade="°"
            />
          </div>
          <PhScale value={ultimaAnalise.ph} />
        </>
      ) : (
        <p className={styles.emptyText}>Nenhuma análise registrada.</p>
      )}
    </Card>
  );
}
