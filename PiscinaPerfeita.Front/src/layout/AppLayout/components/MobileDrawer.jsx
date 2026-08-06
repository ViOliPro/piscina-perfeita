import { useEffect } from "react";
import { SidebarContent } from "./SidebarContent.jsx";
import styles from "./components.module.css";

export function MobileDrawer({ open, activePage, onNavigate, onClose }) {
  // Bloqueia scroll do body quando drawer aberto
  useEffect(() => {
    document.body.style.overflow = open ? "hidden" : "";
    return () => {
      document.body.style.overflow = "";
    };
  }, [open]);

  return (
    <>
      {open && <div onClick={onClose} className={styles.drawerOverlay} />}

      <div
        className={`${styles.drawerPanel} ${open ? styles.drawerPanelOpen : ""}`}
      >
        <SidebarContent
          activePage={activePage}
          onNavigate={onNavigate}
          onClose={onClose}
        />
      </div>
    </>
  );
}
