import { ErrorMessage, LoadingSpinner } from "../../components/ui/index.jsx";
import { useIsMobile } from "../../hooks/useIsMobile.js";
import { useDashboardData } from "./hooks/useDashboardData.js";
import { KpiSummary } from "./components/KpiSummary.jsx";
import { QualidadeAguaCard } from "./components/QualidadeAguaCard.jsx";
import { EstoqueCriticoCard } from "./components/EstoqueCriticoCard.jsx";
import { UltimasAnalisesCard } from "./components/UltimasAnalisesCard.jsx";
import { MovimentacoesRecentesCard } from "./components/MovimentacoesRecentesCard.jsx";
import styles from "./Dashboard.module.css";

export default function Dashboard({ onNavigate }) {
  const isMobile = useIsMobile();
  const { loading, error, piscinas, analises, estoqueBaixo, movimentos } =
    useDashboardData();

  if (loading) return <LoadingSpinner />;
  if (error) return <ErrorMessage message={error} />;

  const ultimaAnalise = analises[0];
  const analisesHoje = analises.filter((analise) => {
    const data = new Date(analise.dataAnalise);
    const agora = new Date();
    return data.toDateString() === agora.toDateString();
  });

  return (
    <div>
      <KpiSummary
        totalPiscinas={piscinas.length}
        analisesHoje={analisesHoje.length}
        estoqueBaixoQtd={estoqueBaixo.length}
        movimentacoesQtd={movimentos.length}
      />

      <div
        className={`${styles.mainGrid} ${isMobile ? styles.mainGridMobile : ""}`}
      >
        <QualidadeAguaCard ultimaAnalise={ultimaAnalise} />
        <EstoqueCriticoCard
          estoqueBaixo={estoqueBaixo}
          onNavigate={onNavigate}
        />
        <UltimasAnalisesCard analises={analises} />
        <MovimentacoesRecentesCard movimentos={movimentos} />
      </div>
    </div>
  );
}
