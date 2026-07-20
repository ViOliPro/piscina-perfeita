// ============================================================
//  Piscina Perfeita — AppLayout  (mobile-ready)
//
//  Desktop: sidebar fixa 200px à esquerda + topbar
//  Mobile:  topbar com hambúrguer → drawer lateral com overlay
//           + bottom navigation bar (5 ítens pinados)
// ============================================================
import { useState, useEffect } from "react";
import { useAuth } from "../../context/AuthContext.jsx";
import { useIsMobile } from "../../hooks/useIsMobile.js";
import { ROLES, ROLE_LABELS } from "../../config/index.js";
import { usuarioLocalService } from "../../config/services.js";
import { LogoIcon } from "../ui/Logo.jsx";

// ----------------------------------------------------------
// Definição da navegação
// ----------------------------------------------------------
export const NAV = [
  { id: "dashboard", label: "Dashboard", icon: "📊", section: "principal" },
  { id: "locais", label: "Locais", icon: "📍", section: "cadastros" },
  { id: "piscinas", label: "Piscinas", icon: "🏊", section: "cadastros" },
  { id: "usuarios", label: "Usuários", icon: "👥", section: "cadastros" },
  { id: "produtos", label: "Produtos", icon: "📦", section: "cadastros" },
  { id: "depositos", label: "Depósitos", icon: "🗄️", section: "cadastros" },
  { id: "analises", label: "Análises", icon: "🧪", section: "operacional" },
  { id: "aplicacoes", label: "Aplicações", icon: "💧", section: "operacional" },
  { id: "estoque", label: "Estoque", icon: "🗃️", section: "operacional" },
  {
    id: "movimentacoes",
    label: "Movimentações",
    icon: "↔️",
    section: "operacional",
  },
  {
    id: "inventario",
    label: "Contagem de Inventário",
    icon: "🔢",
    section: "operacional",
  },
];

// 5 ítens fixados na bottom nav mobile
const BOTTOM_NAV = [
  "dashboard",
  "analises",
  "estoque",
  "movimentacoes",
  "piscinas",
];

// ----------------------------------------------------------
// Item de navegação (sidebar)
// ----------------------------------------------------------
function NavItem({ item, active, onClick }) {
  const [hovered, setHovered] = useState(false);
  return (
    <div
      onClick={onClick}
      onMouseEnter={() => setHovered(true)}
      onMouseLeave={() => setHovered(false)}
      style={{
        display: "flex",
        alignItems: "center",
        gap: 10,
        padding: "10px 14px",
        margin: "1px 8px",
        borderRadius: 8,
        cursor: "pointer",
        fontSize: 13,
        background: active
          ? "rgba(94,192,235,.18)"
          : hovered
            ? "rgba(255,255,255,.06)"
            : "transparent",
        color: active ? "#5BC0EB" : hovered ? "#fff" : "rgba(255,255,255,.65)",
        transition: "background .15s, color .15s",
        WebkitTapHighlightColor: "transparent",
      }}
    >
      <span
        style={{ fontSize: 16, width: 20, textAlign: "center", flexShrink: 0 }}
      >
        {item.icon}
      </span>
      <span>{item.label}</span>
    </div>
  );
}

// ----------------------------------------------------------
// SidebarContent — conteúdo compartilhado entre sidebar e drawer
// ----------------------------------------------------------
function SidebarContent({ activePage, onNavigate, onClose }) {
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

  return (
    <div
      style={{
        display: "flex",
        flexDirection: "column",
        height: "100%",
        overflowY: "auto",
      }}
    >
      {/* Logo */}
      <div
        style={{
          padding: "16px 16px 14px",
          borderBottom: "1px solid rgba(255,255,255,.08)",
          flexShrink: 0,
        }}
      >
        <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
          <LogoIcon height={36} />
          <div>
            <div
              style={{
                fontSize: 14,
                fontWeight: 700,
                color: "#fff",
                letterSpacing: ".3px",
                lineHeight: 1.2,
              }}
            >
              Piscina Perfeita
            </div>
            <div style={{ fontSize: 10, color: "#5BC0EB", opacity: 0.8 }}>
              gestão integrada
            </div>
          </div>
        </div>
      </div>

      {/* Itens de nav */}
      <div style={{ flex: 1, paddingBottom: 8 }}>
        {sections.map((section) => (
          <div key={section}>
            <div
              style={{
                padding: "14px 12px 4px",
                fontSize: 10,
                fontWeight: 600,
                letterSpacing: 1,
                color: "#6B8CAE",
                textTransform: "uppercase",
              }}
            >
              {section}
            </div>
            {NAV.filter((n) => n.section === section).map((item) => (
              <NavItem
                key={item.id}
                item={item}
                active={activePage === item.id}
                onClick={() => {
                  onNavigate(item.id);
                  onClose?.();
                }}
              />
            ))}
          </div>
        ))}
      </div>

      {/* Usuário no fundo da sidebar */}
      {user && (
        <div
          style={{
            borderTop: "1px solid rgba(255,255,255,.08)",
            padding: "12px 16px",
            flexShrink: 0,
          }}
        >
          <div
            style={{
              display: "flex",
              alignItems: "center",
              gap: 10,
              marginBottom: 10,
            }}
          >
            <div
              style={{
                width: 36,
                height: 36,
                borderRadius: "50%",
                background: "linear-gradient(135deg, #2E86AB, #5BC0EB)",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                fontSize: 13,
                fontWeight: 700,
                color: "#fff",
                flexShrink: 0,
              }}
            >
              {initials}
            </div>
            <div style={{ overflow: "hidden" }}>
              <div
                style={{
                  fontSize: 13,
                  fontWeight: 600,
                  color: "#fff",
                  overflow: "hidden",
                  textOverflow: "ellipsis",
                  whiteSpace: "nowrap",
                }}
              >
                {user.nome ?? user.Nome}
              </div>
              <span
                style={{
                  display: "inline-block",
                  padding: "1px 6px",
                  borderRadius: 10,
                  fontSize: 10,
                  fontWeight: 600,
                  background: isAdmin
                    ? "rgba(46,134,171,.25)"
                    : "rgba(39,174,96,.25)",
                  color: isAdmin ? "#a8d8ed" : "#a8dbbe",
                }}
              >
                {roleLabel}
              </span>
            </div>
          </div>
          <button
            onClick={() => {
              logout();
              onClose?.();
            }}
            style={{
              display: "flex",
              alignItems: "center",
              gap: 8,
              width: "100%",
              background: "rgba(231,76,60,.12)",
              border: "0.5px solid rgba(231,76,60,.3)",
              borderRadius: 8,
              padding: "8px 12px",
              cursor: "pointer",
              color: "#ff9f8a",
              fontSize: 13,
              fontFamily: "inherit",
              WebkitTapHighlightColor: "transparent",
            }}
          >
            <span>🚪</span> Sair
          </button>
        </div>
      )}
    </div>
  );
}

// ----------------------------------------------------------
// Drawer mobile (slide da esquerda)
// ----------------------------------------------------------
function MobileDrawer({ open, activePage, onNavigate, onClose }) {
  // Bloqueia scroll do body quando drawer aberto
  useEffect(() => {
    document.body.style.overflow = open ? "hidden" : "";
    return () => {
      document.body.style.overflow = "";
    };
  }, [open]);

  return (
    <>
      <style>{`
        @keyframes pp-drawer-in  { from { transform: translateX(-100%); } to { transform: translateX(0); } }
        @keyframes pp-drawer-out { from { transform: translateX(0); } to { transform: translateX(-100%); } }
      `}</style>

      {/* Overlay */}
      {open && (
        <div
          onClick={onClose}
          style={{
            position: "fixed",
            inset: 0,
            background: "rgba(10,22,40,.55)",
            zIndex: 200,
            WebkitTapHighlightColor: "transparent",
          }}
        />
      )}

      {/* Painel */}
      <div
        style={{
          position: "fixed",
          top: 0,
          left: 0,
          bottom: 0,
          width: 260,
          background: "#0A1628",
          zIndex: 201,
          transform: open ? "translateX(0)" : "translateX(-100%)",
          transition: "transform .25s cubic-bezier(.4,0,.2,1)",
          boxShadow: open ? "4px 0 24px rgba(0,0,0,.3)" : "none",
        }}
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

// ----------------------------------------------------------
// Topbar
// ----------------------------------------------------------
function LocalSwitcher() {
  const { user, switchLocal } = useAuth();
  const [locais, setLocais] = useState([]);
  const [open, setOpen] = useState(false);
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    let ativo = true;
    usuarioLocalService
      .meusLocais()
      .then((res) => {
        if (ativo) setLocais(res ?? []);
      })
      .catch(() => {
        /* silencioso: indicador é acessório, não trava a tela */
      });
    return () => {
      ativo = false;
    };
  }, [user?.localId]);

  const localAtual = locais.find((l) => l.localId === user?.localId);
  const nomeAtual = localAtual?.localNome ?? "Local não definido";

  // Só um local vinculado (ou nenhum ainda carregado): mostra badge estático
  if (locais.length <= 1) {
    return (
      <span
        title={nomeAtual}
        style={{
          fontSize: 10,
          fontWeight: 600,
          color: "#1a5f80",
          background: "rgba(46,134,171,.10)",
          borderRadius: 10,
          padding: "2px 8px",
          whiteSpace: "nowrap",
          maxWidth: 160,
          overflow: "hidden",
          textOverflow: "ellipsis",
        }}
      >
        📍 {nomeAtual}
      </span>
    );
  }

  async function handleSwitch(localId) {
    if (localId === user?.localId) {
      setOpen(false);
      return;
    }
    setBusy(true);
    const ok = await switchLocal(localId);
    console.log(ok);
    setBusy(false);
    setOpen(false);
    if (ok) window.location.reload(); // garante que todas as telas recarreguem com o novo local
  }

  return (
    <div style={{ position: "relative" }}>
      <button
        onClick={() => setOpen((o) => !o)}
        disabled={busy}
        style={{
          display: "flex",
          alignItems: "center",
          gap: 4,
          fontSize: 10,
          fontWeight: 600,
          color: "#1a5f80",
          background: "rgba(46,134,171,.10)",
          border: "none",
          borderRadius: 10,
          padding: "2px 8px",
          cursor: "pointer",
          maxWidth: 180,
        }}
      >
        📍{" "}
        <span
          style={{
            overflow: "hidden",
            textOverflow: "ellipsis",
            whiteSpace: "nowrap",
          }}
        >
          {nomeAtual}
        </span>{" "}
        ▾
      </button>
      {open && (
        <div
          style={{
            position: "absolute",
            top: "calc(100% + 4px)",
            left: 0,
            background: "#fff",
            borderRadius: 8,
            boxShadow: "0 4px 20px rgba(0,0,0,.15)",
            minWidth: 200,
            zIndex: 200,
            overflow: "hidden",
          }}
        >
          {locais.map((l) => (
            <button
              key={l.id}
              onClick={() => handleSwitch(l.localId)}
              style={{
                display: "block",
                width: "100%",
                textAlign: "left",
                padding: "8px 12px",
                border: "none",
                cursor: "pointer",
                background:
                  l.localId === user?.localId ? "rgba(46,134,171,.08)" : "#fff",
                fontSize: 13,
                color: "#0A1628",
              }}
            >
              {l.localId === user?.localId ? "✓ " : ""}
              {l.localNome ?? "Local sem nome"}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}

function Topbar({ activePage, onMenuToggle }) {
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
    <header
      className="pp-topbar"
      style={{
        height: 56,
        background: "#fff",
        borderBottom: "0.5px solid rgba(30,58,95,.10)",
        display: "flex",
        alignItems: "center",
        padding: "0 16px",
        gap: 10,
        flexShrink: 0,
        position: "sticky",
        top: 0,
        zIndex: 100,
      }}
    >
      {/* Hambúrguer (mobile) */}
      {isMobile && (
        <button
          onClick={onMenuToggle}
          style={{
            background: "none",
            border: "none",
            cursor: "pointer",
            padding: 6,
            borderRadius: 8,
            display: "flex",
            flexDirection: "column",
            gap: 4,
            alignItems: "center",
            justifyContent: "center",
            WebkitTapHighlightColor: "transparent",
          }}
          aria-label="Abrir menu"
        >
          {[0, 1, 2].map((i) => (
            <span
              key={i}
              style={{
                display: "block",
                width: 20,
                height: 2,
                background: "#0A1628",
                borderRadius: 1,
              }}
            />
          ))}
        </button>
      )}

      {/* Título */}
      <div
        style={{
          flex: 1,
          display: "flex",
          alignItems: "center",
          gap: 8,
          minWidth: 0,
        }}
      >
        <span
          style={{
            fontSize: isMobile ? 15 : 15,
            fontWeight: 600,
            color: "#0A1628",
          }}
        >
          {pageTitle}
        </span>
        {!isMobile && <LocalSwitcher />}
      </div>

      {/* Avatar + menu (desktop) / apenas avatar (mobile já tem sidebar) */}
      {user && (
        <div style={{ position: "relative" }}>
          <button
            onClick={() => setMenuOpen((o) => !o)}
            style={{
              display: "flex",
              alignItems: "center",
              gap: isMobile ? 0 : 8,
              background: "none",
              border: isMobile ? "none" : "0.5px solid rgba(30,58,95,.12)",
              borderRadius: 8,
              padding: isMobile ? 4 : "5px 10px 5px 6px",
              cursor: "pointer",
              fontFamily: "inherit",
              WebkitTapHighlightColor: "transparent",
            }}
          >
            <div
              style={{
                width: 32,
                height: 32,
                borderRadius: "50%",
                background: "linear-gradient(135deg, #2E86AB, #5BC0EB)",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                fontSize: 12,
                fontWeight: 700,
                color: "#fff",
              }}
            >
              {initials}
            </div>
            {!isMobile && (
              <>
                <div style={{ textAlign: "left" }}>
                  <div
                    style={{
                      fontSize: 13,
                      fontWeight: 600,
                      color: "#0A1628",
                      lineHeight: 1.2,
                    }}
                  >
                    {user.nome ?? user.Nome}
                  </div>
                  <span
                    style={{
                      display: "inline-block",
                      padding: "1px 6px",
                      borderRadius: 10,
                      fontSize: 10,
                      fontWeight: 600,
                      background: isAdmin
                        ? "rgba(46,134,171,.12)"
                        : "rgba(39,174,96,.12)",
                      color: isAdmin ? "#1a5f80" : "#1a7a43",
                    }}
                  >
                    {ROLE_LABELS[user.role ?? user.Role] ?? "User"}
                  </span>
                </div>
                <span style={{ fontSize: 10, color: "#6B8CAE" }}>▾</span>
              </>
            )}
          </button>

          {/* Dropdown */}
          {menuOpen && (
            <>
              <div
                onClick={() => setMenuOpen(false)}
                style={{ position: "fixed", inset: 0, zIndex: 110 }}
              />
              <div
                style={{
                  position: "absolute",
                  top: "calc(100% + 6px)",
                  right: 0,
                  background: "#fff",
                  borderRadius: 10,
                  minWidth: 200,
                  boxShadow: "0 8px 24px rgba(10,22,40,.15)",
                  border: "0.5px solid rgba(30,58,95,.10)",
                  zIndex: 111,
                  overflow: "hidden",
                }}
              >
                <div
                  style={{
                    padding: "12px 14px",
                    borderBottom: "0.5px solid rgba(30,58,95,.08)",
                  }}
                >
                  <div
                    style={{ fontSize: 13, fontWeight: 600, color: "#0A1628" }}
                  >
                    {user.nome ?? user.Nome}
                  </div>
                  <div style={{ fontSize: 11, color: "#6B8CAE", marginTop: 1 }}>
                    {user.email ?? user.Email}
                  </div>
                </div>
                <div style={{ padding: "6px 0" }}>
                  <DropdownItem
                    icon="👤"
                    label="Meu perfil"
                    disabled
                    hint="em breve"
                    onClick={() => setMenuOpen(false)}
                  />
                  <div
                    style={{
                      height: "0.5px",
                      background: "rgba(30,58,95,.08)",
                      margin: "4px 0",
                    }}
                  />
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

function DropdownItem({ icon, label, onClick, danger, disabled, hint }) {
  const [hov, setHov] = useState(false);
  return (
    <button
      onClick={disabled ? undefined : onClick}
      onMouseEnter={() => setHov(true)}
      onMouseLeave={() => setHov(false)}
      disabled={disabled}
      style={{
        display: "flex",
        alignItems: "center",
        gap: 10,
        width: "100%",
        padding: "9px 14px",
        background:
          hov && !disabled
            ? danger
              ? "rgba(231,76,60,.06)"
              : "#E8F4FD"
            : "transparent",
        border: "none",
        cursor: disabled ? "default" : "pointer",
        fontFamily: "inherit",
        fontSize: 13,
        color: danger ? "#c0392b" : disabled ? "#aaa" : "#0A1628",
        opacity: disabled ? 0.6 : 1,
        textAlign: "left",
      }}
    >
      <span style={{ fontSize: 15 }}>{icon}</span>
      <span style={{ flex: 1 }}>{label}</span>
      {hint && <span style={{ fontSize: 10, color: "#6B8CAE" }}>{hint}</span>}
    </button>
  );
}

// ----------------------------------------------------------
// Bottom Navigation Bar (mobile)
// ----------------------------------------------------------
function BottomNav({ activePage, onNavigate }) {
  const items = BOTTOM_NAV.map((id) => NAV.find((n) => n.id === id)).filter(
    Boolean,
  );
  return (
    <nav
      style={{
        position: "fixed",
        bottom: 0,
        left: 0,
        right: 0,
        zIndex: 90,
        background: "#fff",
        borderTop: "0.5px solid rgba(30,58,95,.12)",
        display: "flex",
        alignItems: "stretch",
        height: 60,
        paddingBottom: "env(safe-area-inset-bottom)",
      }}
    >
      {items.map((item) => {
        const active = activePage === item.id;
        return (
          <button
            key={item.id}
            onClick={() => onNavigate(item.id)}
            style={{
              flex: 1,
              display: "flex",
              flexDirection: "column",
              alignItems: "center",
              justifyContent: "center",
              gap: 3,
              background: "none",
              border: "none",
              cursor: "pointer",
              color: active ? "#2E86AB" : "#6B8CAE",
              fontFamily: "inherit",
              WebkitTapHighlightColor: "transparent",
              position: "relative",
            }}
            aria-label={item.label}
          >
            {/* Indicador ativo */}
            {active && (
              <div
                style={{
                  position: "absolute",
                  top: 0,
                  left: "20%",
                  right: "20%",
                  height: 2,
                  borderRadius: "0 0 2px 2px",
                  background: "#2E86AB",
                }}
              />
            )}
            <span style={{ fontSize: 20, lineHeight: 1 }}>{item.icon}</span>
            <span
              style={{
                fontSize: 10,
                fontWeight: active ? 600 : 400,
                lineHeight: 1,
              }}
            >
              {item.label}
            </span>
          </button>
        );
      })}
    </nav>
  );
}

// ----------------------------------------------------------
// AppLayout — orquestra tudo
// ----------------------------------------------------------
export function AppLayout({ children, activePage, onNavigate }) {
  const isMobile = useIsMobile();
  const [drawerOpen, setDrawerOpen] = useState(false);

  // Fecha drawer ao mudar de página
  const handleNavigate = (id) => {
    onNavigate(id);
    setDrawerOpen(false);
  };

  return (
    <div
      className="pp-app-shell"
      style={{ display: "flex", height: "100dvh", overflow: "hidden" }}
    >
      {/* Sidebar desktop */}
      {!isMobile && (
        <div
          className="pp-sidebar"
          style={{ width: 200, background: "#0A1628", flexShrink: 0 }}
        >
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
      <div
        className="pp-main-col"
        style={{
          flex: 1,
          display: "flex",
          flexDirection: "column",
          overflow: "hidden",
          minWidth: 0,
        }}
      >
        <Topbar
          activePage={activePage}
          onMenuToggle={() => setDrawerOpen((o) => !o)}
        />

        <main
          className="pp-main"
          style={{
            flex: 1,
            overflowY: "auto",
            background: "#E8F4FD",
            // Espaço para a bottom nav em mobile
            paddingBottom: isMobile ? 68 : 0,
          }}
        >
          <div style={{ padding: isMobile ? 14 : 24 }}>{children}</div>
        </main>
      </div>

      {/* Bottom Nav mobile */}
      {isMobile && (
        <BottomNav activePage={activePage} onNavigate={handleNavigate} />
      )}
    </div>
  );
}
