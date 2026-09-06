import { Suspense } from "react";
import { useQueryErrorResetBoundary } from "@tanstack/react-query";
import { ErrorBoundary } from "../../components/ui/ErrorBoundary.jsx";
import { useIsMobile } from "../../hooks/useIsMobile.js";
import { KpiSummary } from "./components/KpiSummary.jsx";
import { QualidadeAguaCard } from "./components/QualidadeAguaCard.jsx";
import { EstoqueCriticoCard } from "./components/EstoqueCriticoCard.jsx";
import { UltimasAnalisesCard } from "./components/UltimasAnalisesCard.jsx";
import { MovimentacoesRecentesCard } from "./components/MovimentacoesRecentesCard.jsx";
import { CardSkeleton } from "./components/CardSkeleton.jsx";
import styles from "./Dashboard.module.css";

function Section({ children, fallback }) {
  const { reset } = useQueryErrorResetBoundary();
  return (
    <ErrorBoundary onReset={reset}>
      <Suspense fallback={fallback}>{children}</Suspense>
    </ErrorBoundary>
  );
}

export default function Dashboard({ onNavigate }) {
  const isMobile = useIsMobile();

  return (
    <div>
      <Section
        fallback={<div className="pp-kpi-grid">{/* 4 placeholders */}</div>}
      >
        <KpiSummary />
      </Section>

      <div
        className={`${styles.mainGrid} ${isMobile ? styles.mainGridMobile : ""}`}
      >
        <Section
          fallback={<CardSkeleton title="Qualidade da água" lines={5} />}
        >
          <QualidadeAguaCard />
        </Section>

        <Section fallback={<CardSkeleton title="Estoque crítico" lines={4} />}>
          <EstoqueCriticoCard onNavigate={onNavigate} />
        </Section>

        <Section fallback={<CardSkeleton title="Últimas análises" lines={5} />}>
          <UltimasAnalisesCard />
        </Section>

        <Section
          fallback={<CardSkeleton title="Movimentações recentes" lines={4} />}
        >
          <MovimentacoesRecentesCard />
        </Section>
      </div>
    </div>
  );
}
