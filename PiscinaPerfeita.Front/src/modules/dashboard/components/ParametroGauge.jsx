import styles from "./components.module.css";

function statusDoParametro(value, faixa) {
  if (value === null || value === undefined) return "muted";
  if (value >= faixa.min && value <= faixa.max) return "ok";
  const centro = (faixa.min + faixa.max) / 2;
  return Math.abs(value - centro) <= 1 ? "warn" : "bad";
}

export function ParametroGauge({ label, value, faixa, unidade = "" }) {
  const status = statusDoParametro(value, faixa);

  return (
    <div className={styles.gaugeWrap}>
      <div className={styles.gaugeCircle} data-status={status}>
        {value !== null && value !== undefined ? `${value}${unidade}` : "—"}
      </div>
      <div className={styles.gaugeLabel}>{label}</div>
      <div className={styles.gaugeRange} data-status={status}>
        {faixa.min}–{faixa.max}
      </div>
    </div>
  );
}
