import { useEffect, useState } from "react";
import Chart from "react-apexcharts";
import { analiseService } from "../../config/services.js";
import { useIsMobile } from "../../hooks/useIsMobile.js";
import { tokens } from "../styles/tokens.js";

const CORES = {
  ph: "#2E86AB",
  cloroLivre: "#1a7a43",
  alcalinidade: "#8e5fd1",
  temperatura: "#c07a1e",
};

const STATUS_COR = {
  ideal: tokens.color.success,
  abaixo: tokens.color.error,
  acima: tokens.color.error,
  "sem-dados": tokens.color.textMuted,
};

// Card de qualidade da água por piscina, no período informado.
//
// Design deliberado (ver conversa sobre UX mobile): o resumo em texto
// (dados.resumo.textoResumo) fica SEMPRE visível, aberto — é o que
// importa pra quem só está de passagem lançando dados. O gráfico
// completo (ApexCharts) só é buscado/montado quando o usuário expande —
// no mobile por padrão fechado (a maioria do uso ali é lançamento, não
// análise); no desktop já vem aberto, contexto mais de gestão.
//
// Importante: o gráfico só MONTA quando `aberto` é true (renderização
// condicional, não display:none) — evita gastar a requisição e o SVG à
// toa quando o usuário nunca expande no mobile.
export default function QualidadeAguaHistoricoCard({ piscinaId, piscinaNome }) {
  const isMobile = useIsMobile();
  const [aberto, setAberto] = useState(!isMobile);
  const [dados, setDados] = useState(null);
  const [carregando, setCarregando] = useState(false);
  const [erro, setErro] = useState(null);

  useEffect(() => {
    if (!aberto || !piscinaId || dados) return;

    let cancelado = false;
    setCarregando(true);
    setErro(null);

    analiseService
      .obterQualidadeAgua(piscinaId)
      .then((res) => {
        if (!cancelado) setDados(res);
      })
      .catch((err) => {
        if (!cancelado)
          setErro(
            err.message ?? "Não foi possível carregar a qualidade da água.",
          );
      })
      .finally(() => {
        if (!cancelado) setCarregando(false);
      });

    return () => {
      cancelado = true;
    };
  }, [aberto, piscinaId, dados]);

  // Troca de piscina invalida o que já foi buscado.
  useEffect(() => {
    setDados(null);
    setErro(null);
  }, [piscinaId]);

  const resumo = dados?.resumo;

  return (
    <div
      style={{
        background: "#fff",
        borderRadius: 14,
        border: `1px solid ${tokens.color.border}`,
        overflow: "hidden",
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
            💧 Qualidade da água — {piscinaNome}
          </div>
          <div
            style={{
              fontSize: 12.5,
              marginTop: 2,
              color: resumo
                ? STATUS_COR[
                    [
                      resumo.cloroLivre,
                      resumo.ph,
                      resumo.alcalinidade,
                      resumo.temperatura,
                    ].find((p) => p.status === "abaixo" || p.status === "acima")
                      ?.status ?? "ideal"
                  ]
                : tokens.color.textMuted,
            }}
          >
            {resumo
              ? resumo.textoResumo
              : carregando
                ? "Carregando..."
                : "Toque para ver o histórico"}
          </div>
        </div>
        <span style={{ fontSize: 18, color: tokens.color.textMuted }}>
          {aberto ? "︿" : "﹀"}
        </span>
      </button>

      {aberto && (
        <div style={{ padding: "0 16px 16px" }}>
          {erro && (
            <p style={{ color: tokens.color.error, fontSize: 13 }}>{erro}</p>
          )}
          {carregando && !dados && (
            <p style={{ color: tokens.color.textMuted, fontSize: 13 }}>
              Carregando gráfico...
            </p>
          )}
          {dados && dados.pontos.length === 0 && (
            <p style={{ color: tokens.color.textMuted, fontSize: 13 }}>
              Nenhuma análise registrada no período.
            </p>
          )}
          {dados && dados.pontos.length > 0 && (
            <Chart
              type="line"
              height={280}
              series={montarSeries(dados)}
              options={montarOpcoes(dados)}
            />
          )}
        </div>
      )}
    </div>
  );
}

function montarSeries(dados) {
  const categorias = ["ph", "cloroLivre", "alcalinidade", "temperatura"];
  const nomes = {
    ph: "pH",
    cloroLivre: "Cloro livre",
    alcalinidade: "Alcalinidade",
    temperatura: "Temperatura",
  };

  return categorias
    .filter((chave) => dados.pontos.some((p) => p[chave] != null))
    .map((chave) => ({
      name: nomes[chave],
      data: dados.pontos.map((p) => [new Date(p.data).getTime(), p[chave]]),
    }));
}

function montarOpcoes(dados) {
  // Faixa ideal do pH sombreada no fundo — é o parâmetro mais lido/comum.
  // Poderia estender pra um seletor de qual faixa sombrear, se algum dia
  // fizer sentido mostrar mais de uma ao mesmo tempo.
  const faixaPh = dados.faixasIdeais.ph;

  return {
    chart: { toolbar: { show: false }, zoom: { enabled: false } },
    colors: [CORES.ph, CORES.cloroLivre, CORES.alcalinidade, CORES.temperatura],
    stroke: { width: 2, curve: "smooth" },
    xaxis: { type: "datetime" },
    legend: { position: "top", fontSize: "12px" },
    dataLabels: { enabled: false },
    annotations: {
      yaxis: [
        {
          y: faixaPh.min,
          y2: faixaPh.max,
          fillColor: CORES.ph,
          opacity: 0.08,
          label: { text: "Faixa ideal (pH)", style: { fontSize: "10px" } },
        },
      ],
    },
  };
}
