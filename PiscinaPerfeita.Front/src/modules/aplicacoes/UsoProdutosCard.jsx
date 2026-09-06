import { useEffect, useState, lazy, Suspense } from "react";
import { useQuery } from "@tanstack/react-query";
import { aplicacaoProdutoService } from "../../config/services.js";
import { qk } from "../../helpers/queryKeys.js";
import { useIsMobile } from "../../hooks/useIsMobile.js";
import { tokens } from "../styles/tokens.js";

// Lazy: ApexCharts só entra no bundle quando o card monta o gráfico.
const Chart = lazy(() => import("react-apexcharts"));

const CORES_BARRA = [
  "#2E86AB",
  "#1a7a43",
  "#8e5fd1",
  "#c07a1e",
  "#c0392b",
  "#16a085",
  "#2980b9",
  "#d35400",
];

/**
 * Card de uso de produtos por piscina (últimos 30 dias por padrão).
 *
 * UX alinhada ao QualidadeAguaHistoricoCard:
 * - resumo textual sempre visível
 * - gráfico só monta quando expandido (mobile fecha por padrão)
 */
export default function UsoProdutosCard({ piscinaId, piscinaNome }) {
  const isMobile = useIsMobile();
  const [aberto, setAberto] = useState(!isMobile);

  const {
    data,
    isLoading: carregando,
    error,
  } = useQuery({
    queryKey: qk.usoProdutos({ piscinaId }),
    queryFn: () => aplicacaoProdutoService.obterUsoProdutos(piscinaId),
    enabled: Boolean(aberto && piscinaId),
    staleTime: 5 * 60_000,
  });

  // Troca de piscina: o queryKey já muda; só garantimos UI limpa.
  useEffect(() => {
    // noop — TanStack invalida pela chave
  }, [piscinaId]);

  const itens = data?.itens ?? [];
  const textoResumo = data?.textoResumo;

  return (
    <div
      style={{
        background: "#fff",
        borderRadius: 14,
        border: `1px solid ${tokens.color.border}`,
        overflow: "hidden",
        marginBottom: 16,
      }}
    >
      <button
        type="button"
        onClick={() => setAberto((v) => !v)}
        style={{
          width: "100%",
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
          gap: 12,
          padding: "14px 16px",
          background: "transparent",
          border: "none",
          cursor: "pointer",
          textAlign: "left",
        }}
      >
        <div>
          <div
            style={{ fontSize: 13, fontWeight: 600, color: tokens.color.text }}
          >
            🧪 Uso de produtos — {piscinaNome || "Piscina"}
          </div>
          <div
            style={{
              fontSize: 12.5,
              marginTop: 2,
              color: tokens.color.textMuted,
            }}
          >
            {textoResumo
              ? textoResumo
              : carregando
                ? "Carregando…"
                : "Toque para ver o uso no período"}
          </div>
        </div>
        <span style={{ fontSize: 18, color: tokens.color.textMuted }}>
          {aberto ? "︿" : "﹀"}
        </span>
      </button>

      {aberto && (
        <div style={{ padding: "0 16px 16px" }}>
          {error && (
            <p style={{ color: tokens.color.error, fontSize: 13 }}>
              {error.message ?? "Não foi possível carregar o uso de produtos."}
            </p>
          )}
          {carregando && !data && (
            <p style={{ color: tokens.color.textMuted, fontSize: 13 }}>
              Carregando gráfico…
            </p>
          )}
          {data && itens.length === 0 && (
            <p style={{ color: tokens.color.textMuted, fontSize: 13 }}>
              Nenhuma aplicação registrada no período.
            </p>
          )}
          {data && itens.length > 0 && (
            <Suspense
              fallback={
                <p style={{ color: tokens.color.textMuted, fontSize: 13 }}>
                  Carregando gráfico…
                </p>
              }
            >
              <Chart
                type="bar"
                height={Math.max(220, itens.length * 36)}
                series={[
                  {
                    name: "Quantidade",
                    data: itens.map((i) =>
                      Number(Number(i.quantidadeTotal).toFixed(4)),
                    ),
                  },
                ]}
                options={{
                  chart: {
                    toolbar: { show: false },
                    fontFamily: "inherit",
                  },
                  colors: CORES_BARRA,
                  plotOptions: {
                    bar: {
                      horizontal: true,
                      borderRadius: 4,
                      distributed: true,
                      barHeight: "70%",
                    },
                  },
                  dataLabels: {
                    enabled: true,
                    formatter: (val, opts) => {
                      const item = itens[opts.dataPointIndex];
                      return `${val} ${item?.unidade ?? ""}`;
                    },
                    style: { fontSize: "11px" },
                  },
                  xaxis: {
                    categories: itens.map((i) => i.produtoNome),
                    labels: { style: { fontSize: "11px" } },
                  },
                  yaxis: {
                    labels: { style: { fontSize: "12px" } },
                  },
                  legend: { show: false },
                  tooltip: {
                    y: {
                      formatter: (val, opts) => {
                        const item = itens[opts.dataPointIndex];
                        return `${val} ${item?.unidade ?? ""} · ${item?.quantidadeAplicacoes ?? 0} aplicação(ões)`;
                      },
                    },
                  },
                  grid: { borderColor: "#eef3f7" },
                }}
              />
            </Suspense>
          )}
        </div>
      )}
    </div>
  );
}
