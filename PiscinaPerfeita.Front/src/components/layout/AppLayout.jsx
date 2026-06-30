// ============================================================
//  Piscina Perfeita — Layout principal (Sidebar + Topbar)
// ============================================================
import { useState } from "react";
import { useAuth }  from "../../context/AuthContext.jsx";
import { ROLES, ROLE_LABELS } from "../../config/index.js";

const NAV = [
  { id: "dashboard",      label: "Dashboard",     icon: "📊", section: "principal" },
  { id: "piscinas",       label: "Piscinas",       icon: "🏊", section: "cadastros" },
  { id: "usuarios",       label: "Usuários",       icon: "👥", section: "cadastros" },
  { id: "produtos",       label: "Produtos",       icon: "📦", section: "cadastros" },
  { id: "analises",       label: "Análises",       icon: "🧪", section: "operacional" },
  { id: "estoque",        label: "Estoque",        icon: "🗃️", section: "operacional" },
  { id: "movimentacoes",  label: "Movimentações",  icon: "↔️", section: "operacional" },
];

// ----------------------------------------------------------
// Onda animada no logo
// ----------------------------------------------------------
function WaveAnimation() {
  return (
    <div style={{ display: "inline-flex", alignItems: "flex-end", gap: 2, height: 20 }}>
      {[0, 0.1, 0.2, 0.3, 0.4].map((delay, i) => (
        <span key={i} style={{
          width: 3, borderRadius: 1, background: "#5BC0EB",
          animation: `pp-wave 1.2s ease-in-out ${delay}s infinite`,
        }} />
      ))}
      <style>{`@keyframes pp-wave { 0%,100%{height:4px} 50%{height:16px} }`}</style>
    </div>
  );
}

// ----------------------------------------------------------
// Item de navegação
// ----------------------------------------------------------
function NavItem({ item, active, onClick }) {
  const [hovered, setHovered] = useState(false);
  return (
    <div
      onClick={onClick}
      onMouseEnter={() => setHovered(true)}
      onMouseLeave={() => setHovered(false)}
      style={{
        display: "flex", alignItems: "center", gap: 8,
        padding: "8px 12px", margin: "1px 8px", borderRadius: 6,
        cursor: "pointer", fontSize: 13,
        background: active
          ? "rgba(94,192,235,.18)"
          : hovered ? "rgba(255,255,255,.06)" : "transparent",
        color: active ? "#5BC0EB" : hovered ? "#fff" : "rgba(255,255,255,.65)",
        transition: "background .15s, color .15s",
      }}
    >
      <span style={{ fontSize: 14, width: 16, textAlign: "center" }}>{item.icon}</span>
      {item.label}
    </div>
  );
}

// ----------------------------------------------------------
// Sidebar
// ----------------------------------------------------------
export function Sidebar({ activePage, onNavigate }) {
  const sections = [...new Set(NAV.map((n) => n.section))];
  return (
    <nav style={{
      width: 200, background: "#0A1628",
      display: "flex", flexDirection: "column", flexShrink: 0,
    }}>
      <div style={{ padding: "20px 16px 16px", borderBottom: "1px solid rgba(255,255,255,.08)" }}>
        <WaveAnimation />
        <h1 style={{ fontSize: 15, fontWeight: 600, color: "#fff", marginTop: 6, letterSpacing: ".5px" }}>
          Piscina Perfeita
        </h1>
        <span style={{ fontSize: 11, color: "#5BC0EB", opacity: .8 }}>gestão integrada</span>
      </div>

      {sections.map((section) => (
        <div key={section}>
          <div style={{
            padding: "12px 8px 4px", fontSize: 10, fontWeight: 600,
            letterSpacing: 1, color: "#6B8CAE", textTransform: "uppercase",
          }}>
            {section}
          </div>
          {NAV.filter((n) => n.section === section).map((item) => (
            <NavItem
              key={item.id} item={item}
              active={activePage === item.id}
              onClick={() => onNavigate(item.id)}
            />
          ))}
        </div>
      ))}
    </nav>
  );
}

// ----------------------------------------------------------
// Topbar — nome do usuário + logout
// ----------------------------------------------------------
function Topbar({ activePage }) {
  const { user, logout } = useAuth();
  const [menuOpen, setMenuOpen] = useState(false);

  const pageTitle = NAV.find((n) => n.id === activePage)?.label ?? "Dashboard";
  const roleLabel = user ? (ROLE_LABELS[user.role ?? user.Role] ?? "User") : "";
  const isAdmin   = (user?.role ?? user?.Role) === ROLES.ADMIN;
  const initials  = user?.nome
    ? user.nome.split(" ").slice(0, 2).map((p) => p[0]).join("").toUpperCase()
    : "?";

  return (
    <header style={{
      height: 56, background: "#fff",
      borderBottom: "0.5px solid rgba(30,58,95,.10)",
      display: "flex", alignItems: "center",
      padding: "0 24px", gap: 12, flexShrink: 0,
      position: "relative", zIndex: 10,
    }}>
      {/* Título da página */}
      <div style={{ flex: 1, fontSize: 15, fontWeight: 600, color: "#0A1628" }}>
        {pageTitle}
      </div>

      {/* Usuário */}
      {user && (
        <div style={{ position: "relative" }}>
          <button
            onClick={() => setMenuOpen((o) => !o)}
            style={{
              display: "flex", alignItems: "center", gap: 10,
              background: "none", border: "0.5px solid rgba(30,58,95,.12)",
              borderRadius: 8, padding: "5px 10px 5px 6px",
              cursor: "pointer", fontFamily: "inherit",
            }}
          >
            {/* Avatar */}
            <div style={{
              width: 30, height: 30, borderRadius: "50%",
              background: "linear-gradient(135deg, #2E86AB, #5BC0EB)",
              display: "flex", alignItems: "center", justifyContent: "center",
              fontSize: 12, fontWeight: 700, color: "#fff", flexShrink: 0,
            }}>
              {initials}
            </div>
            <div style={{ textAlign: "left" }}>
              <div style={{ fontSize: 13, fontWeight: 600, color: "#0A1628", lineHeight: 1.2 }}>
                {user.nome ?? user.Nome}
              </div>
              <div style={{ fontSize: 11, color: "#6B8CAE", lineHeight: 1.2 }}>
                <span style={{
                  display: "inline-block", padding: "1px 6px", borderRadius: 10,
                  fontSize: 10, fontWeight: 600,
                  background: isAdmin ? "rgba(46,134,171,.12)" : "rgba(39,174,96,.12)",
                  color: isAdmin ? "#1a5f80" : "#1a7a43",
                }}>
                  {roleLabel}
                </span>
              </div>
            </div>
            <span style={{ fontSize: 10, color: "#6B8CAE", marginLeft: 2 }}>▾</span>
          </button>

          {/* Dropdown */}
          {menuOpen && (
            <>
              {/* Overlay para fechar ao clicar fora */}
              <div
                onClick={() => setMenuOpen(false)}
                style={{ position: "fixed", inset: 0, zIndex: 20 }}
              />
              <div style={{
                position: "absolute", top: "calc(100% + 6px)", right: 0,
                background: "#fff", borderRadius: 10, minWidth: 200,
                boxShadow: "0 8px 24px rgba(10,22,40,.15)",
                border: "0.5px solid rgba(30,58,95,.10)",
                zIndex: 21, overflow: "hidden",
              }}>
                {/* Cabeçalho do menu */}
                <div style={{ padding: "12px 14px", borderBottom: "0.5px solid rgba(30,58,95,.08)" }}>
                  <div style={{ fontSize: 13, fontWeight: 600, color: "#0A1628" }}>
                    {user.nome ?? user.Nome}
                  </div>
                  <div style={{ fontSize: 11, color: "#6B8CAE", marginTop: 1 }}>
                    {user.email ?? user.Email}
                  </div>
                </div>

                {/* Ações */}
                <div style={{ padding: "6px 0" }}>
                  <MenuItem
                    icon="👤"
                    label="Meu perfil"
                    onClick={() => { setMenuOpen(false); /* TODO: abrir modal de perfil */ }}
                    disabled
                    hint="em breve"
                  />
                  <div style={{ height: "0.5px", background: "rgba(30,58,95,.08)", margin: "4px 0" }} />
                  <MenuItem
                    icon="🚪"
                    label="Sair"
                    onClick={() => { setMenuOpen(false); logout(); }}
                    danger
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

function MenuItem({ icon, label, onClick, danger, disabled, hint }) {
  const [hovered, setHovered] = useState(false);
  return (
    <button
      onClick={disabled ? undefined : onClick}
      onMouseEnter={() => setHovered(true)}
      onMouseLeave={() => setHovered(false)}
      disabled={disabled}
      style={{
        display: "flex", alignItems: "center", gap: 10,
        width: "100%", padding: "8px 14px",
        background: hovered && !disabled ? (danger ? "rgba(231,76,60,.06)" : "#E8F4FD") : "transparent",
        border: "none", cursor: disabled ? "default" : "pointer",
        fontFamily: "inherit", fontSize: 13,
        color: danger ? "#c0392b" : disabled ? "#aaa" : "#0A1628",
        opacity: disabled ? .6 : 1,
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
// AppLayout — envolve toda a aplicação autenticada
// ----------------------------------------------------------
export function AppLayout({ children, activePage, onNavigate }) {
  return (
    <div style={{ display: "flex", height: "100vh", overflow: "hidden" }}>
      <Sidebar activePage={activePage} onNavigate={onNavigate} />
      <div style={{ flex: 1, display: "flex", flexDirection: "column", overflow: "hidden" }}>
        <Topbar activePage={activePage} />
        <main style={{ flex: 1, overflow: "auto", background: "#E8F4FD" }}>
          <div style={{ padding: 24 }}>
            {children}
          </div>
        </main>
      </div>
    </div>
  );
}
