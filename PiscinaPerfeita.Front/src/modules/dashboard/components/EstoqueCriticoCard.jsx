import { Badge, Card } from "../../../components/ui/index.jsx";
import styles from "./components.module.css";

export function EstoqueCriticoCard({ estoqueBaixo, onNavigate }) {
  return (
    <Card
      title="Estoque crítico"
      titleExtra={<Badge variant="bad">{estoqueBaixo.length} itens</Badge>}
    >
      {estoqueBaixo.length === 0 ? (
        <p className={styles.emptyTextOk}>
          Todos os produtos estão em nível adequado.
        </p>
      ) : (
        <>
          <div className={styles.tableWrap}>
            <table className={styles.table}>
              <thead>
                <tr>
                  {["Produto", "Qtd.", "Status"].map((h) => (
                    <th key={h} className={styles.thPlain}>
                      {h}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {estoqueBaixo.map((item) => (
                  <tr key={item.id} className={styles.trWarn}>
                    <td className={styles.td}>{item.produto?.nome}</td>
                    <td className={styles.td}>
                      {item.quantidadeAtual} {item.produto?.unidadeMedida}
                    </td>
                    <td className={styles.td}>
                      <Badge
                        variant={item.quantidadeAtual <= 1 ? "bad" : "warn"}
                      >
                        {item.quantidadeAtual <= 1 ? "Baixo" : "Atenção"}
                      </Badge>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className={styles.estoqueActionWrap}>
            <button
              onClick={() => onNavigate("estoque")}
              className={styles.estoqueActionButton}
            >
              Ver pedido de orçamento →
            </button>
          </div>
        </>
      )}
    </Card>
  );
}
