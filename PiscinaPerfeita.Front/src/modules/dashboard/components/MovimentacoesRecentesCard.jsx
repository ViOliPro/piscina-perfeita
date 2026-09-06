import { useSuspenseQuery } from "@tanstack/react-query";
import { qk, diasAtrasISO } from "../../../helpers/queryKeys.js";
import { movimentacaoService } from "../../../config/services.js";
import { TIPO_LABELS, TIPO_MOVIMENTACAO } from "../../../config/index.js";
import { Card } from "../../../components/ui/index.jsx";
import styles from "./components.module.css";

export function MovimentacoesRecentesCard() {
  const { data: movimentos } = useSuspenseQuery({
    queryKey: qk.movimentacoes({ dias: 14, limit: 5 }),
    queryFn: () =>
      movimentacaoService.listar({
        dataInicio: diasAtrasISO(14),
        limit: 5,
      }),
  });

  return (
    <Card title="Movimentações recentes">
      {movimentos.length === 0 ? (
        <p className={styles.emptyText}>Nenhuma movimentação registrada.</p>
      ) : (
        movimentos.map((mov) => {
          const isEntrada = mov.tipoMovimentacao === TIPO_MOVIMENTACAO.ENTRADA;
          return (
            <div key={mov.id} className={styles.movRow}>
              <div
                className={styles.movDot}
                data-tipo={isEntrada ? "entrada" : "saida"}
              />
              <div>
                <div className={styles.movText}>
                  {TIPO_LABELS[mov.tipoMovimentacao]} — {mov.produto?.nome}{" "}
                  <strong>
                    {isEntrada ? "+" : "-"}
                    {mov.quantidade} {mov.produto?.unidadeMedida}
                  </strong>
                </div>
                <div className={styles.movMeta}>
                  {new Date(mov.dataMovimentacao).toLocaleString("pt-BR")} ·{" "}
                  {mov.piscina?.nome}
                </div>
              </div>
            </div>
          );
        })
      )}
    </Card>
  );
}
