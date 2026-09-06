// ============================================================
//  Shell do Dashboard por Piscina (Suspense por card)
//
//  Uso futuro: quando o usuário abrir "Detalhes / Dashboard"
//  de uma piscina, montar esta tela. A query pesada só roda aqui.
//
//  Padrão idêntico ao Dashboard geral:
//  - ErrorBoundary + CardSkeleton
//  - useSuspenseQuery por card (ou useSuspenseQueries no KPI)
//  - chave qk.piscinaDashboard — NÃO reutiliza qk.piscinas
// ============================================================
import { Suspense, useState } from "react";
import { useSuspenseQuery } from "@tanstack/react-query";
import {
  PageHeader,
  Card,
  Button,
  ErrorMessage,
  LoadingSpinner,
} from "../../components/ui/index.jsx";
import { piscinaService } from "../../config/services.js";
import { qk, diasAtrasISO } from "../../helpers/queryKeys.js";

function CardSkeleton({ height = 160 }) {
  return (
    <Card>
      <div
        style={{
          height,
          background: "linear-gradient(90deg, #e8f4fd 25%, #f0f7ff 50%, #e8f4fd 75%)",
          backgroundSize: "200% 100%",
          borderRadius: 8,
          animation: "pulse 1.2s ease-in-out infinite",
        }}
      />
    </Card>
  );
}

/** KPI / contagens — uma query do dashboard */
function ContagensCard({ piscinaId, inicio, fim }) {
  const { data } = useSuspenseQuery({
    queryKey: qk.piscinaDashboard({
      id: piscinaId,
      inicio,
      fim,
      limitAnalises: 5,
      limitMovimentacoes: 5,
    }),
    queryFn: () =>
      piscinaService.dashboard(piscinaId, {
        inicio,
        fim,
        limitAnalises: 5,
        limitMovimentacoes: 5,
      }),
    staleTime: 60 * 1000,
  });

  const c = data?.contagens ?? {};
  return (
    <Card>
      <h3 style={{ marginTop: 0 }}>{data?.piscina?.nome ?? "Piscina"}</h3>
      <div style={{ display: "flex", gap: 16, flexWrap: "wrap" }}>
        <div>
          <div style={{ fontSize: 12, opacity: 0.7 }}>Análises</div>
          <strong style={{ fontSize: 22 }}>{c.analises ?? 0}</strong>
        </div>
        <div>
          <div style={{ fontSize: 12, opacity: 0.7 }}>Movimentações</div>
          <strong style={{ fontSize: 22 }}>{c.movimentacoes ?? 0}</strong>
        </div>
        <div>
          <div style={{ fontSize: 12, opacity: 0.7 }}>Aplicações</div>
          <strong style={{ fontSize: 22 }}>{c.aplicacoes ?? 0}</strong>
        </div>
      </div>
    </Card>
  );
}

/** Últimas análises (mesma query; em produção pode ser card separado) */
function UltimasAnalisesCard({ piscinaId, inicio, fim }) {
  const { data } = useSuspenseQuery({
    queryKey: qk.piscinaDashboard({
      id: piscinaId,
      inicio,
      fim,
      limitAnalises: 10,
      limitMovimentacoes: 5,
    }),
    queryFn: () =>
      piscinaService.dashboard(piscinaId, {
        inicio,
        fim,
        limitAnalises: 10,
        limitMovimentacoes: 5,
      }),
    staleTime: 60 * 1000,
  });

  const lista = data?.ultimasAnalises ?? [];
  return (
    <Card>
      <h3 style={{ marginTop: 0 }}>Últimas análises</h3>
      {lista.length === 0 ? (
        <p style={{ opacity: 0.7 }}>Nenhuma análise no período.</p>
      ) : (
        <ul style={{ margin: 0, paddingLeft: 18 }}>
          {lista.map((a) => (
            <li key={a.id}>
              {new Date(a.dataAnalise).toLocaleString("pt-BR")}
              {" — "}
              pH {a.ph ?? "—"} / Cl {a.cloroLivre ?? "—"}
            </li>
          ))}
        </ul>
      )}
    </Card>
  );
}

/**
 * Shell completo — passar piscinaId ao navegar da listagem.
 * Cards de qualidade-agua e uso-produtos podem ser adicionados depois
 * reutilizando endpoints já existentes (Suspense isolado por card).
 */
export default function PiscinaDashboardShell({
  piscinaId,
  onBack,
  diasPadrao = 30,
}) {
  const [inicio] = useState(() => diasAtrasISO(diasPadrao));
  const [fim] = useState(() => new Date().toISOString());

  if (!piscinaId) {
    return <ErrorMessage message="Piscina não informada." />;
  }

  return (
    <div>
      <PageHeader
        title="Dashboard da piscina"
        description="Visão analítica sob demanda (não afeta a listagem)"
        action={
          onBack ? (
            <Button variant="ghost" onClick={onBack}>
              ← Voltar
            </Button>
          ) : null
        }
      />

      <div
        style={{
          display: "grid",
          gap: 16,
          gridTemplateColumns: "repeat(auto-fit, minmax(280px, 1fr))",
        }}
      >
        <Suspense fallback={<CardSkeleton height={120} />}>
          <ContagensCard piscinaId={piscinaId} inicio={inicio} fim={fim} />
        </Suspense>

        <Suspense fallback={<CardSkeleton height={200} />}>
          <UltimasAnalisesCard piscinaId={piscinaId} inicio={inicio} fim={fim} />
        </Suspense>
      </div>

      {/*
        Próximos cards (mesmo padrão):
        - QualidadeAguaCard → analiseService.obterQualidadeAgua(piscinaId, { inicio, fim })
        - UsoProdutosCard   → já existe em aplicações (uso-produtos)
      */}
    </div>
  );
}
