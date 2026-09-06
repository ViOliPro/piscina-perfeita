import { useSuspenseQueries } from "@tanstack/react-query";
import { qk, diasAtrasISO } from "../../../helpers/queryKeys.js";
import { piscinaService } from "../../../config/services.js";
import { analiseService } from "../../../config/services.js";
import { estoqueService } from "../../../config/services.js";
import { movimentacaoService } from "../../../config/services.js";
import { KpiCard } from "../../../components/ui/index.jsx";

export function KpiSummary() {
  const [qPiscinas, qAnalises, qEstoque, qMov] = useSuspenseQueries({
    queries: [
      {
        queryKey: qk.piscinas,
        queryFn: () => piscinaService.listar(),
        staleTime: 10 * 60_000,
      },
      {
        queryKey: qk.analises({ limit: 10 }),
        queryFn: () => analiseService.listar({ limit: 10 }),
      },
      {
        queryKey: qk.estoques("baixo"),
        queryFn: () => estoqueService.listarBaixo(),
      },
      {
        queryKey: qk.movimentacoes({ dias: 14, limit: 5 }),
        queryFn: () =>
          movimentacaoService.listar({
            dataInicio: diasAtrasISO(14),
            limit: 5,
          }),
      },
    ],
  });

  const piscinas = qPiscinas.data;
  const analises = qAnalises.data;
  const estoqueBaixo = qEstoque.data;
  const movimentos = qMov.data;

  const analisesHoje = analises.filter((a) => {
    const data = new Date(a.dataAnalise);
    return data.toDateString() === new Date().toDateString();
  }).length;

  return (
    <div className="pp-kpi-grid">
      <KpiCard
        label="Piscinas"
        value={piscinas.length}
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
        value={estoqueBaixo.length}
        subLabel={estoqueBaixo.length > 0 ? "requer atenção" : "tudo ok"}
        subVariant={estoqueBaixo.length > 0 ? "warn" : "ok"}
      />
      <KpiCard
        label="Movimentações"
        value={movimentos.length}
        subLabel="esta semana"
        subVariant="muted"
      />
    </div>
  );
}
