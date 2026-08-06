import styles from "./components.module.css";

export function DropdownItem({ icon, label, onClick, danger, disabled, hint }) {
  const classes = [
    styles.dropdownItem,
    danger ? styles.dropdownItemDanger : "",
    disabled ? styles.dropdownItemDisabled : "",
  ]
    .filter(Boolean)
    .join(" ");

  return (
    <button
      onClick={disabled ? undefined : onClick}
      disabled={disabled}
      className={classes}
    >
      <span className={styles.dropdownItemIcon}>{icon}</span>
      <span className={styles.dropdownItemLabel}>{label}</span>
      {hint && <span className={styles.dropdownItemHint}>{hint}</span>}
    </button>
  );
}
