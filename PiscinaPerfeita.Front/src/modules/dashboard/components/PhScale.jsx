import { ANALISE_FAIXAS } from "../../../config/index.js";
import styles from "./components.module.css";

export function PhScale({ value }) {
  const percentual = value ? ((value / 14) * 100).toFixed(1) : null;

  return (
    <div className={styles.phScaleWrap}>
      <div className={styles.phScaleHeader}>
        <span>pH escala</span>
        <span>
          ideal: {ANALISE_FAIXAS.ph.min} – {ANALISE_FAIXAS.ph.max}
        </span>
      </div>
      <div className={styles.phScaleTrack}>
        {percentual && (
          <div
            className={styles.phScaleIndicator}
            style={{ left: `${percentual}%` }}
          />
        )}
      </div>
      <div className={styles.phScaleLabels}>
        <span>0</span>
        <span>7</span>
        <span>14</span>
      </div>
    </div>
  );
}
