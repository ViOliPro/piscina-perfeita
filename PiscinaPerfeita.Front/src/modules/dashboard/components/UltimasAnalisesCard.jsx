import { Badge, Card } from "../../../components/ui/index.jsx";
import { ANALISE_FAIXAS } from "../../../config/index.js";
import styles from "./components.module.css";

function statusDaAnalise(analise) {
  const phOk =
    analise.ph >= ANALISE_FAIXAS.ph.min && analise.ph <= ANALISE_FAIXAS.ph.max;
  const cloroOk = analise.cloroLivre >= ANALISE_FAIXAS.cloroLivre.min;

  if (phOk && cloroOk) return { variant: "ok", label: "Normal" };
  if (!phOk) return { variant: "bad", label: "Ajustar" };
  return { variant: "warn", label: "Atenção" };
}

export function UltimasAnalisesCard({ analises }) {
  const exibidas = analises.slice(0, 4);

  return (
    <Card title="Últimas análises" noPadding>
      <div className={styles.tableWrap}>
        <table className={styles.table}>
          <thead>
            <tr>
              {["Piscina", "Data", "pH", "Status"].map((h) => (
                <th key={h} className={styles.thHeader}>
                  {h}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {exibidas.map((analise) => {
              const { variant, label } = statusDaAnalise(analise);
              return (
                <tr key={analise.id} className={styles.trBordered}>
                  <td className={styles.tdSpaced}>
                    {analise.piscina?.nome ?? "—"}
                  </td>
                  <td className={`${styles.tdSpaced} ${styles.tdMuted}`}>
                    {new Date(analise.dataAnalise).toLocaleDateString("pt-BR")}
                  </td>
                  <td className={styles.tdSpaced}>{analise.ph ?? "—"}</td>
                  <td className={styles.tdSpaced}>
                    <Badge variant={variant}>{label}</Badge>
                  </td>
                </tr>
              );
            })}
            {exibidas.length === 0 && (
              <tr>
                <td colSpan={4} className={styles.emptyRow}>
                  Nenhuma análise.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </Card>
  );
}
