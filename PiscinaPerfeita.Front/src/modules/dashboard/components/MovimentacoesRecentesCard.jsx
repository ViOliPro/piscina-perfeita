import { Card } from "../../../components/ui/index.jsx";
import { TIPO_LABELS, TIPO_MOVIMENTACAO } from "../../../config/index.js";
import styles from "./components.module.css";

export function MovimentacoesRecentesCard({ movimentos }) {
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
