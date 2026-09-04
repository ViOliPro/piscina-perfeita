import { useQueries } from "@tanstack/react-query";
import {
  analiseService,
  estoqueService,
  movimentacaoService,
  piscinaService,
} from "../../../config/services.js";
import { qk, diasAtrasISO } from "../../../helpers/queryKeys.js";

/**
 * Dashboard: cache via TanStack Query.
 * - piscinas: staleTime alto (dados de referência)
 * - analises: limit 10 no backend
 * - estoque baixo: status=baixo
 * - movimentações: últimos 14 dias + limit 5
 */
export function useDashboardData() {
  const results = useQueries({
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

  const [qPiscinas, qAnalises, qEstoque, qMov] = results;
  const loading = results.some((r) => r.isLoading);
  const firstError = results.find((r) => r.isError);

  return {
    loading,
    error: firstError?.error?.message ?? null,
    piscinas: qPiscinas.data ?? [],
    analises: qAnalises.data ?? [],
    estoqueBaixo: qEstoque.data ?? [],
    movimentos: qMov.data ?? [],
  };
}
