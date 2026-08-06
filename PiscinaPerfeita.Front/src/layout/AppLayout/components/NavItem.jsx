import styles from "./components.module.css";

export function NavItem({ item, active, onClick }) {
  return (
    <div
      onClick={onClick}
      className={`${styles.navItem} ${active ? styles.navItemActive : ""}`}
    >
      <span className={styles.navItemIcon}>{item.icon}</span>
      <span>{item.label}</span>
    </div>
  );
}
