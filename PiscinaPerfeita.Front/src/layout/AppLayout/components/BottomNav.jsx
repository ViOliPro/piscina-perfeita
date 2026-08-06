import { NAV, BOTTOM_NAV } from "../../nav.config.js";
import styles from "./components.module.css";

export function BottomNav({ activePage, onNavigate }) {
  const items = BOTTOM_NAV.map((id) => NAV.find((n) => n.id === id)).filter(
    Boolean,
  );

  return (
    <nav className={styles.bottomNav}>
      {items.map((item) => {
        const active = activePage === item.id;
        return (
          <button
            key={item.id}
            onClick={() => onNavigate(item.id)}
            className={`${styles.bottomNavButton} ${
              active ? styles.bottomNavButtonActive : ""
            }`}
            aria-label={item.label}
          >
            {active && <div className={styles.bottomNavIndicator} />}
            <span className={styles.bottomNavIcon}>{item.icon}</span>
            <span
              className={`${styles.bottomNavLabel} ${
                active ? styles.bottomNavLabelActive : ""
              }`}
            >
              {item.label}
            </span>
          </button>
        );
      })}
    </nav>
  );
}
