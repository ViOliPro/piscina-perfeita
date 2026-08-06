import { useAuth, useCan } from "../../../context/AuthContext.jsx";
import { ROLES, ROLE_LABELS } from "../../../config/index.js";
import { LogoIcon } from "../../../components/ui/Logo.jsx";
import { NAV } from "../../nav.config.js";
import { NavItem } from "./NavItem.jsx";
import styles from "./components.module.css";

export function SidebarContent({ activePage, onNavigate, onClose }) {
  const { user, logout } = useAuth();
  const sections = [...new Set(NAV.map((n) => n.section))];

  const initials = user?.nome
    ? user.nome
        .split(" ")
        .slice(0, 2)
        .map((p) => p[0])
        .join("")
        .toUpperCase()
    : "?";
  const isAdmin = (user?.role ?? user?.Role) === ROLES.ADMIN;
  const roleLabel = user ? (ROLE_LABELS[user.role ?? user.Role] ?? "User") : "";

  function handleNavClick(id) {
    onNavigate(id);
    onClose?.();
  }

  function handleLogout() {
    logout();
    onClose?.();
  }

  return (
    <div className={styles.sidebarContent}>
      {/* Logo */}
      <div className={styles.sidebarLogoSection}>
        <div className={styles.sidebarLogoRow}>
          <LogoIcon height={36} />
          <div>
            <div className={styles.sidebarBrandName}>Piscina Perfeita</div>
            <div className={styles.sidebarBrandTagline}>gestão integrada</div>
          </div>
        </div>
      </div>

      {/* Itens de nav */}
      <div className={styles.sidebarNavArea}>
        {sections.map((section) => {
          const itemsDaSecao = NAV.filter(
            (n) => n.section === section && useCan(n.permissions),
          );

          // Se o usuário não tiver permissão para NENHUM item desta seção, nem mostramos o título
          if (itemsDaSecao.length === 0) return null;

          return (
            <div key={section}>
              <div className={styles.sidebarSectionTitle}>{section}</div>

              {itemsDaSecao.map((item) => (
                <NavItem
                  key={item.id}
                  item={item}
                  active={activePage === item.id}
                  onClick={() => handleNavClick(item.id)}
                />
              ))}
            </div>
          );
        })}
      </div>

      {/* Usuário no fundo da sidebar */}
      {user && (
        <div className={styles.sidebarUserSection}>
          <div className={styles.sidebarUserRow}>
            <div className={styles.sidebarAvatar}>{initials}</div>
            <div className={styles.sidebarUserTextWrap}>
              <div className={styles.sidebarUserName}>
                {user.nome ?? user.Nome}
              </div>
              <span
                className={`${styles.sidebarRoleBadge} ${
                  isAdmin
                    ? styles.sidebarRoleBadgeAdmin
                    : styles.sidebarRoleBadgeOther
                }`}
              >
                {roleLabel}
              </span>
            </div>
          </div>
          <button className={styles.sidebarLogoutButton} onClick={handleLogout}>
            <span>🚪</span> Sair
          </button>
        </div>
      )}
    </div>
  );
}
