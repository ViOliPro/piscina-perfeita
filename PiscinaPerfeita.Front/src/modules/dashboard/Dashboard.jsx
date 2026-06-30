// ============================================================
//  Piscina Perfeita — Módulo: Dashboard
// ============================================================
import { useState, useEffect } from "react";
import { KpiCard, Card, Badge, LoadingSpinner, ErrorMessage } from "../../components/ui/index.jsx";
import { analiseService, estoqueService, movimentacaoService, piscinaService } from "../../config/services.js";
import { ANALISE_FAIXAS, TIPO_MOVIMENTACAO, TIPO_LABELS } from "../../config/index.js";
import { useIsMobile } from "../../hooks/useIsMobile.js";

// ----------------------------------------------------------
// Gauge circular de parâmetro de análise
// ----------------------------------------------------------
function ParametroGauge({ label, value, faixa, unidade = "" }) {
  const status =
    value === null || value === undefined ? "muted"
    : value >= faixa.min && value <= faixa.max ? "ok"
    : Math.abs(value - ((faixa.min + faixa.max) / 2)) <= 1 ? "warn"
    : "bad";

  const colors = {
    ok:   { border: "#27AE60", color: "#27AE60", bg: "rgba(39,174,96,.08)" },
    warn: { border: "#F39C12", color: "#F39C12", bg: "rgba(243,156,18,.08)" },
    bad:  { border: "#E74C3C", color: "#E74C3C", bg: "rgba(231,76,60,.08)" },
    muted:{ border: "#6B8CAE", color: "#6B8CAE", bg: "rgba(107,140,174,.08)" },
  };
  const c = colors[status];

  return (
    <div style={{ textAlign: "center", minWidth: 80 }}>
      <div style={{
        width: 72, height: 72, borderRadius: "50%",
        border: `3px solid ${c.border}`, color: c.color, background: c.bg,
        display: "flex", alignItems: "center", justifyContent: "center",
        fontSize: 15, fontWeight: 700, margin: "0 auto 6px",
      }}>
        {value !== null && value !== undefined ? `${value}${unidade}` : "—"}
      </div>
      <div style={{ fontSize: 11, color: "#6B8CAE" }}>{label}</div>
      <div style={{ fontSize: 10, color: c.color, marginTop: 2 }}>
        {faixa.min}–{faixa.max}
      </div>
    </div>
  );
}

// ----------------------------------------------------------
// Barra de escala de pH
// ----------------------------------------------------------
function PhScale({ value }) {
  const pct = value ? ((value / 14) * 100).toFixed(1) : null;
  return (
    <div style={{ marginTop: 14 }}>
      <div style={{
        display: "flex", justifyContent: "space-between",
        fontSize: 11, color: "#6B8CAE", marginBottom: 2,
      }}>
        <span>pH escala</span>
        <span>ideal: {ANALISE_FAIXAS.ph.min} – {ANALISE_FAIXAS.ph.max}</span>
      </div>
      <div style={{
        width: "100%", height: 10, borderRadius: 5,
        background: "linear-gradient(to right, #e74c3c 0%, #f39c12 25%, #27ae60 45%, #27ae60 65%, #3498db 85%, #8e44ad 100%)",
        position: "relative", margin: "6px 0 2px",
      }}>
        {pct && (
          <div style={{
            position: "absolute", top: -3, left: `${pct}%`,
            width: 6, height: 16, background: "#0A1628",
            borderRadius: 2, transform: "translateX(-3px)",
            boxShadow: "0 1px 4px rgba(0,0,0,.3)",
          }} />
        )}
      </div>
      <div style={{ display: "flex", justifyContent: "space-between", fontSize: 10, color: "#6B8CAE" }}>
        <span>0</span><span>7</span><span>14</span>
      </div>
    </div>
  );
}

// ----------------------------------------------------------
// Dashboard principal
// ----------------------------------------------------------
export default function Dashboard({ onNavigate }) {
  const [loading, setLoading]         = useState(true);
  const [error, setError]             = useState(null);
  const [piscinas, setPiscinas]       = useState([]);
  const [analises, setAnalises]       = useState([]);
  const [estoqueBaixo, setEstoqueBaixo] = useState([]);
  const [movimentos, setMovimentos]   = useState([]);

  useEffect(() => {
    async function load() {
      try {
        setLoading(true);
        const [p, a, e, m] = await Promise.all([
          piscinaService.listar(),
          analiseService.listar(),
          estoqueService.listarBaixo(),
          movimentacaoService.listar(),
        ]);
        setPiscinas(p ?? []);
        setAnalises(a ?? []);
        setEstoqueBaixo(e ?? []);
        setMovimentos((m ?? []).slice(0, 5));
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    }
    load();
  }, []);

  const isMobile = useIsMobile();

  if (loading) return <LoadingSpinner />;
  if (error)   return <ErrorMessage message={error} />;

  const ultimaAnalise = analises[0];
  const hoje = analises.filter((a) => {
    const d = new Date(a.dataAnalise);
    const now = new Date();
    return d.toDateString() === now.toDateString();
  });

  return (
    <div>
      {/* KPIs */}
      <div style={{ display: "grid", gridTemplateColumns: isMobile ? "1fr 1fr" : "repeat(auto-fit,minmax(140px,1fr))", gap: 10, marginBottom: 16 }}>
        <KpiCard label="Piscinas"        value={piscinas.length} subLabel="cadastradas" subVariant="muted" />
        <KpiCard label="Análises hoje"   value={hoje.length}     subLabel={`+${hoje.length} vs ontem`} subVariant="ok" />
        <KpiCard label="Estoque baixo"   value={estoqueBaixo.length} subLabel={estoqueBaixo.length > 0 ? "requer atenção" : "tudo ok"} subVariant={estoqueBaixo.length > 0 ? "warn" : "ok"} />
        <KpiCard label="Movimentações"   value={movimentos.length}   subLabel="esta semana" subVariant="muted" />
      </div>

      <div style={{ display: "grid", gridTemplateColumns: isMobile ? "1fr" : "1fr 1fr", gap: 14 }}>
        {/* Gauges da última análise */}
        <Card title="Qualidade da água" titleExtra={
          <span style={{ fontSize: 11, color: "#6B8CAE", fontWeight: 400 }}>
            {ultimaAnalise?.piscina?.nome ?? "—"}
          </span>
        }>
          {ultimaAnalise ? (
            <>
              <div style={{ display: "flex", gap: 10, flexWrap: "wrap" }}>
                <ParametroGauge label="pH"           value={ultimaAnalise.ph}           faixa={ANALISE_FAIXAS.ph} />
                <ParametroGauge label="Cloro livre"  value={ultimaAnalise.cloroLivre}   faixa={ANALISE_FAIXAS.cloroLivre} unidade=" mg/L" />
                <ParametroGauge label="Alcalinidade" value={ultimaAnalise.alcalinidade} faixa={ANALISE_FAIXAS.alcalinidade} unidade=" mg/L" />
                <ParametroGauge label="Temperatura"  value={ultimaAnalise.temperatura}  faixa={ANALISE_FAIXAS.temperatura} unidade="°" />
              </div>
              <PhScale value={ultimaAnalise.ph} />
            </>
          ) : (
            <p style={{ color: "#6B8CAE", fontSize: 13 }}>Nenhuma análise registrada.</p>
          )}
        </Card>

        {/* Estoque crítico */}
        <Card title="Estoque crítico" titleExtra={<Badge variant="bad">{estoqueBaixo.length} itens</Badge>}>
          {estoqueBaixo.length === 0 ? (
            <p style={{ color: "#27AE60", fontSize: 13 }}>Todos os produtos estão em nível adequado.</p>
          ) : (
            <>
              <table style={{ width: "100%", borderCollapse: "collapse", fontSize: 13 }}>
                <thead>
                  <tr>
                    {["Produto", "Qtd.", "Status"].map((h) => (
                      <th key={h} style={{ textAlign: "left", padding: "6px 0", fontSize: 11, color: "#6B8CAE", fontWeight: 600, borderBottom: "0.5px solid var(--border)" }}>{h}</th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {estoqueBaixo.map((e) => (
                    <tr key={e.id} style={{ background: "rgba(231,76,60,.03)" }}>
                      <td style={{ padding: "7px 0", color: "var(--text-primary)" }}>{e.produto?.nome}</td>
                      <td style={{ padding: "7px 0", color: "var(--text-primary)" }}>{e.quantidadeAtual} {e.produto?.unidadeMedida}</td>
                      <td style={{ padding: "7px 0" }}><Badge variant={e.quantidadeAtual <= 1 ? "bad" : "warn"}>{e.quantidadeAtual <= 1 ? "Baixo" : "Atenção"}</Badge></td>
                    </tr>
                  ))}
                </tbody>
              </table>
              <div style={{ marginTop: 10 }}>
                <button
                  onClick={() => onNavigate("estoque")}
                  style={{
                    display: "inline-flex", alignItems: "center", gap: 6,
                    padding: "0 14px", height: 28, borderRadius: 6, fontSize: 12,
                    fontWeight: 500, cursor: "pointer",
                    background: "#2E86AB", color: "#fff", border: "none",
                  }}
                >
                  Ver pedido de orçamento →
                </button>
              </div>
            </>
          )}
        </Card>

        {/* Últimas análises */}
        <Card title="Últimas análises" noPadding>
          <table style={{ width: "100%", borderCollapse: "collapse", fontSize: 13 }}>
            <thead>
              <tr>
                {["Piscina", "Data", "pH", "Status"].map((h) => (
                  <th key={h} style={{
                    background: "#E8F4FD", color: "#1E3A5F", fontWeight: 600,
                    fontSize: 11, textTransform: "uppercase", letterSpacing: ".5px",
                    padding: "8px 12px", textAlign: "left", borderBottom: "1px solid var(--border)",
                  }}>{h}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {analises.slice(0, 4).map((a) => {
                const phOk = a.ph >= ANALISE_FAIXAS.ph.min && a.ph <= ANALISE_FAIXAS.ph.max;
                const cloroOk = a.cloroLivre >= ANALISE_FAIXAS.cloroLivre.min;
                const variant = phOk && cloroOk ? "ok" : !phOk ? "bad" : "warn";
                const statusLabel = variant === "ok" ? "Normal" : variant === "warn" ? "Atenção" : "Ajustar";
                return (
                  <tr key={a.id} style={{ borderBottom: "0.5px solid var(--border)" }}>
                    <td style={{ padding: "9px 12px" }}>{a.piscina?.nome ?? "—"}</td>
                    <td style={{ padding: "9px 12px", color: "#6B8CAE" }}>
                      {new Date(a.dataAnalise).toLocaleDateString("pt-BR")}
                    </td>
                    <td style={{ padding: "9px 12px" }}>{a.ph ?? "—"}</td>
                    <td style={{ padding: "9px 12px" }}><Badge variant={variant}>{statusLabel}</Badge></td>
                  </tr>
                );
              })}
              {analises.length === 0 && (
                <tr><td colSpan={4} style={{ padding: "20px 12px", textAlign: "center", color: "#6B8CAE" }}>Nenhuma análise.</td></tr>
              )}
            </tbody>
          </table>
        </Card>

        {/* Movimentações recentes */}
        <Card title="Movimentações recentes">
          {movimentos.length === 0 ? (
            <p style={{ color: "#6B8CAE", fontSize: 13 }}>Nenhuma movimentação registrada.</p>
          ) : (
            movimentos.map((m) => (
              <div key={m.id} style={{
                display: "flex", alignItems: "flex-start", gap: 10,
                padding: "8px 0", borderBottom: "0.5px solid var(--border)",
              }}>
                <div style={{
                  width: 8, height: 8, borderRadius: "50%", flexShrink: 0, marginTop: 4,
                  background: m.tipoMovimentacao === TIPO_MOVIMENTACAO.ENTRADA ? "#27AE60" : "#E74C3C",
                }} />
                <div>
                  <div style={{ fontSize: 13 }}>
                    {TIPO_LABELS[m.tipoMovimentacao]} — {m.produto?.nome} <strong>{m.tipoMovimentacao === TIPO_MOVIMENTACAO.ENTRADA ? "+" : "-"}{m.quantidade} {m.produto?.unidadeMedida}</strong>
                  </div>
                  <div style={{ fontSize: 11, color: "#6B8CAE", marginTop: 1 }}>
                    {new Date(m.dataMovimentacao).toLocaleString("pt-BR")} · {m.piscina?.nome}
                  </div>
                </div>
              </div>
            ))
          )}
        </Card>
      </div>
    </div>
  );
}
