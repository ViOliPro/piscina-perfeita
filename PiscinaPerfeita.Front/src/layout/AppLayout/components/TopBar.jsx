import { useState } from "react";
import { useAuth } from "../../../context/AuthContext.jsx";
import { useIsMobile } from "../../../hooks/useIsMobile.js";
import { ROLES, ROLE_LABELS } from "../../../config/index.js";
import { NAV } from "../../nav.config.js";
import { LocalSwitcher } from "./LocalSwitcher.jsx";
import { DropdownItem } from "./DropdownItem.jsx";
import styles from "./components.module.css";

export function Topbar({ activePage, onMenuToggle, onNavigate }) {
  const isMobile = useIsMobile();
  const { user, logout } = useAuth();
  const [menuOpen, setMenuOpen] = useState(false);

  const pageTitle = NAV.find((n) => n.id === activePage)?.label ?? "Dashboard";
  const initials = user?.nome
    ? user.nome
        .split(" ")
        .slice(0, 2)
        .map((p) => p[0])
        .join("")
        .toUpperCase()
    : "?";
  const isAdmin = (user?.role ?? user?.Role) === ROLES.ADMIN;

  return (
    <header className={styles.topbarHeader}>
      {/* Hambúrguer (mobile) */}
      {isMobile && (
        <button
          onClick={onMenuToggle}
          className={styles.topbarHamburger}
          aria-label="Abrir menu"
        >
          {[0, 1, 2].map((i) => (
            <span key={i} className={styles.topbarHamburgerBar} />
          ))}
        </button>
      )}

      {/* Título */}
      <div className={styles.topbarTitleWrap}>
        <span className={styles.topbarPageTitle}>{pageTitle}</span>
        {!isMobile && <LocalSwitcher />}
      </div>

      {/* Avatar + menu */}
      {user && (
        <div className={styles.topbarUserWrap}>
          <button
            onClick={() => setMenuOpen((o) => !o)}
            className={`${styles.topbarUserButton} ${
              isMobile ? styles.topbarUserButtonMobile : ""
            }`}
          >
            <div className={styles.topbarAvatar}>{initials}</div>
            {!isMobile && (
              <>
                <div className={styles.topbarUserInfo}>
                  <div className={styles.topbarUserName}>
                    {user.nome ?? user.Nome}
                  </div>
                  <span
                    className={`${styles.topbarRoleBadge} ${
                      isAdmin
                        ? styles.topbarRoleBadgeAdmin
                        : styles.topbarRoleBadgeOther
                    }`}
                  >
                    {ROLE_LABELS[user.role ?? user.Role] ?? "User"}
                  </span>
                </div>
                <span className={styles.topbarChevron}>▾</span>
              </>
            )}
          </button>

          {/* Dropdown */}
          {menuOpen && (
            <>
              <div
                onClick={() => setMenuOpen(false)}
                className={styles.topbarDropdownOverlay}
              />
              <div className={styles.topbarDropdownMenu}>
                <div className={styles.topbarDropdownHeader}>
                  <div className={styles.topbarDropdownUserName}>
                    {user.nome ?? user.Nome}
                  </div>
                  <div className={styles.topbarDropdownUserEmail}>
                    {user.email ?? user.Email}
                  </div>
                </div>
                <div className={styles.topbarDropdownList}>
                  <DropdownItem
                    icon="👤"
                    label="Meu perfil"
                    onClick={() => {
                      setMenuOpen(false);
                      onNavigate("meuPerfil");
                    }}
                  />
                  <div className={styles.dropdownDivider} />
                  <DropdownItem
                    icon="🚪"
                    label="Sair"
                    danger
                    onClick={() => {
                      setMenuOpen(false);
                      logout();
                    }}
                  />
                </div>
              </div>
            </>
          )}
        </div>
      )}
    </header>
  );
}
