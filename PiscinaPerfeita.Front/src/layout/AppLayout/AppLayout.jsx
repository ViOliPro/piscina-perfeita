import { useState } from "react";
import { useIsMobile } from "../../hooks/useIsMobile.js";
import { SidebarContent } from "./components/SidebarContent.jsx";
import { MobileDrawer } from "./components/MobileDrawer.jsx";
import { Topbar } from "./components/Topbar.jsx";
import { BottomNav } from "./components/BottomNav.jsx";
import styles from "./AppLayout.module.css";

export function AppLayout({ children, activePage, onNavigate }) {
  const isMobile = useIsMobile();
  const [drawerOpen, setDrawerOpen] = useState(false);

  // Fecha drawer ao mudar de página
  const handleNavigate = (id) => {
    onNavigate(id);
    setDrawerOpen(false);
  };

  return (
    <div className={styles.appShell}>
      {/* Sidebar desktop */}
      {!isMobile && (
        <div className={styles.sidebarDesktop}>
          <SidebarContent activePage={activePage} onNavigate={handleNavigate} />
        </div>
      )}

      {/* Drawer mobile */}
      {isMobile && (
        <MobileDrawer
          open={drawerOpen}
          activePage={activePage}
          onNavigate={handleNavigate}
          onClose={() => setDrawerOpen(false)}
        />
      )}

      {/* Conteúdo principal */}
      <div className={styles.mainColumn}>
        <Topbar
          activePage={activePage}
          onMenuToggle={() => setDrawerOpen((o) => !o)}
          onNavigate={handleNavigate}
        />

        <main
          className={`${styles.mainContent} ${
            isMobile ? styles.mainContentMobile : ""
          }`}
        >
          <div
            className={
              isMobile ? styles.contentInnerMobile : styles.contentInner
            }
          >
            {children}
          </div>
        </main>
      </div>

      {/* Bottom Nav mobile */}
      {isMobile && (
        <BottomNav activePage={activePage} onNavigate={handleNavigate} />
      )}
    </div>
  );
}
