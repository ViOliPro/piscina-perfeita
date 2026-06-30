// ============================================================
//  Piscina Perfeita — Componentes UI compartilhados (mobile-ready)
// ============================================================
import { useState, useEffect } from "react";
import { useIsMobile } from "../../hooks/useIsMobile.js";

// ----------------------------------------------------------
// Badge
// ----------------------------------------------------------
export function Badge({ variant = "info", children }) {
  const styles = {
    ok:     { background: "rgba(39,174,96,.12)",  color: "#1a7a43" },
    warn:   { background: "rgba(243,156,18,.12)", color: "#9a6300" },
    bad:    { background: "rgba(231,76,60,.12)",  color: "#c0392b" },
    info:   { background: "rgba(46,134,171,.12)", color: "#1a5f80" },
    purple: { background: "rgba(142,68,173,.12)", color: "#6c3483" },
  };
  return (
    <span style={{
      display: "inline-flex", alignItems: "center", gap: 4,
      padding: "2px 8px", borderRadius: 20,
      fontSize: 11, fontWeight: 600,
      ...styles[variant],
    }}>
      {children}
    </span>
  );
}

// ----------------------------------------------------------
// Button — tamanho touch-friendly em mobile (min 44px)
// ----------------------------------------------------------
export function Button({ variant = "ghost", size = "md", onClick, disabled, children, type = "button", fullWidth }) {
  const isMobile = useIsMobile();
  const base = {
    display: "inline-flex", alignItems: "center", justifyContent: "center", gap: 6,
    borderRadius: 8, fontWeight: 500, cursor: disabled ? "not-allowed" : "pointer",
    opacity: disabled ? .5 : 1, border: "0.5px solid", transition: "background .15s",
    fontFamily: "inherit", width: fullWidth ? "100%" : undefined,
  };
  const sizes = {
    md: { padding: "0 16px", height: isMobile ? 44 : 36, fontSize: isMobile ? 14 : 13 },
    sm: { padding: "0 12px", height: isMobile ? 36 : 28, fontSize: isMobile ? 13 : 11 },
  };
  const variants = {
    primary: { background: "#2E86AB", color: "#fff",     borderColor: "#2E86AB" },
    ghost:   { background: "transparent", color: "#2E86AB", borderColor: "#2E86AB" },
    danger:  { background: "transparent", color: "#E74C3C", borderColor: "#E74C3C" },
  };
  return (
    <button type={type} onClick={onClick} disabled={disabled}
      style={{ ...base, ...sizes[size], ...variants[variant] }}>
      {children}
    </button>
  );
}

// ----------------------------------------------------------
// Modal — center dialog desktop / bottom-sheet mobile
// ----------------------------------------------------------
export function Modal({ open, onClose, title, children, footer }) {
  const isMobile = useIsMobile();

  // Bloqueia scroll do body quando modal está aberto
  useEffect(() => {
    if (open) {
      document.body.style.overflow = "hidden";
      return () => { document.body.style.overflow = ""; };
    }
  }, [open]);

  if (!open) return null;

  return (
    <div
      onClick={(e) => e.target === e.currentTarget && onClose()}
      style={{
        position: "fixed", inset: 0,
        background: "rgba(10,22,40,.5)",
        display: "flex",
        alignItems: isMobile ? "flex-end" : "center",
        justifyContent: "center",
        zIndex: 100,
      }}
    >
      <div
        style={{
          background: "#fff",
          borderRadius: isMobile ? "16px 16px 0 0" : 14,
          width: isMobile ? "100%" : 520,
          maxWidth: isMobile ? "100%" : "95vw",
          maxHeight: isMobile ? "92vh" : "88vh",
          overflow: "auto",
          border: "0.5px solid var(--border)",
          animation: isMobile ? "pp-sheet-up .25s ease-out" : "pp-modal-in .2s ease-out",
        }}
      >
        <style>{`
          @keyframes pp-sheet-up  { from { transform: translateY(100%); } to { transform: translateY(0); } }
          @keyframes pp-modal-in  { from { opacity: 0; transform: scale(.97); } to { opacity: 1; transform: scale(1); } }
        `}</style>

        {/* Handle visual em mobile */}
        {isMobile && (
          <div style={{ display: "flex", justifyContent: "center", padding: "10px 0 4px" }}>
            <div style={{ width: 36, height: 4, borderRadius: 2, background: "rgba(30,58,95,.15)" }} />
          </div>
        )}

        {/* Header */}
        <div style={{
          padding: isMobile ? "10px 20px 14px" : "18px 20px 14px",
          borderBottom: "0.5px solid var(--border)",
          display: "flex", alignItems: "center", justifyContent: "space-between",
          position: "sticky", top: 0, background: "#fff", zIndex: 1,
        }}>
          <h3 style={{ fontSize: isMobile ? 17 : 16, fontWeight: 600, color: "#0A1628" }}>{title}</h3>
          <button onClick={onClose} style={{
            background: "none", border: "none", cursor: "pointer",
            color: "#6B8CAE", fontSize: 20, lineHeight: 1,
            width: 32, height: 32, borderRadius: 6,
            display: "flex", alignItems: "center", justifyContent: "center",
          }}>✕</button>
        </div>

        <div style={{ padding: isMobile ? "16px 16px 8px" : "18px 20px" }}>{children}</div>

        {footer && (
          <div style={{
            padding: isMobile ? "12px 16px 24px" : "14px 20px",
            borderTop: "0.5px solid var(--border)",
            display: "flex", justifyContent: "flex-end",
            flexDirection: isMobile ? "column-reverse" : "row",
            gap: 8,
            position: "sticky", bottom: 0, background: "#fff",
          }}>
            {footer}
          </div>
        )}
      </div>
    </div>
  );
}

// ----------------------------------------------------------
// FormField
// ----------------------------------------------------------
export function FormField({ label, fullWidth, children }) {
  return (
    <div style={{
      display: "flex", flexDirection: "column", gap: 6,
      gridColumn: fullWidth ? "1 / -1" : undefined,
    }}>
      <label style={{ fontSize: 13, fontWeight: 600, color: "#1E3A5F" }}>{label}</label>
      {children}
    </div>
  );
}

// inputStyle — padding maior em mobile via CSS clamp
export const inputStyle = {
  border: "1px solid #c8dce8", borderRadius: 8,
  padding: "10px 12px", fontSize: 14,
  background: "var(--surface-2,#fff)",
  color: "var(--text-primary,#111)",
  fontFamily: "inherit", outline: "none", width: "100%",
  WebkitAppearance: "none",        // remove seta dupla iOS nos selects
  transition: "border-color .15s, box-shadow .15s",
};

// ----------------------------------------------------------
// FormGrid — 2 colunas desktop, 1 coluna mobile
// ----------------------------------------------------------
export function FormGrid({ children }) {
  const isMobile = useIsMobile();
  return (
    <div style={{
      display: "grid",
      gridTemplateColumns: isMobile ? "1fr" : "1fr 1fr",
      gap: isMobile ? 14 : 14,
    }}>
      {children}
    </div>
  );
}

// ----------------------------------------------------------
// FormSection
// ----------------------------------------------------------
export function FormSection({ label }) {
  return (
    <div style={{
      gridColumn: "1 / -1", fontSize: 11, fontWeight: 600,
      color: "#6B8CAE", textTransform: "uppercase", letterSpacing: ".8px",
      paddingTop: 6, borderTop: "0.5px solid var(--border)", marginTop: 6,
    }}>
      {label}
    </div>
  );
}

// ----------------------------------------------------------
// MobileCard — representa uma linha de tabela em mobile
// ----------------------------------------------------------
function MobileCard({ columns, row, index }) {
  // Primeira coluna vira título, demais viram linhas de detalhe
  const [titleCol, ...detailCols] = columns.filter((c) => c.key !== "_acoes" && c.key !== "_edit" && c.key !== "_status");
  const actionCol  = columns.find((c) => c.key === "_acoes" || c.key === "_edit");
  const statusCol  = columns.find((c) => c.key === "_status");

  return (
    <div style={{
      background: "#fff", borderRadius: 10,
      border: "0.5px solid var(--border)",
      padding: "14px 16px", marginBottom: 10,
    }}>
      {/* Cabeçalho do card: título + status */}
      <div style={{ display: "flex", alignItems: "flex-start", justifyContent: "space-between", marginBottom: 10 }}>
        <div style={{ fontSize: 14, fontWeight: 600, color: "#0A1628", flex: 1, paddingRight: 8 }}>
          {titleCol ? (titleCol.render ? titleCol.render(row[titleCol.key], row) : row[titleCol.key] ?? "—") : "—"}
        </div>
        {statusCol && (
          <div>{statusCol.render(row[statusCol.key], row)}</div>
        )}
      </div>

      {/* Detalhes em grade 2×N */}
      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "8px 12px" }}>
        {detailCols.map((col) => (
          <div key={col.key}>
            <div style={{ fontSize: 10, fontWeight: 600, color: "#6B8CAE", textTransform: "uppercase", letterSpacing: ".4px", marginBottom: 2 }}>
              {col.label}
            </div>
            <div style={{ fontSize: 13, color: "#0A1628" }}>
              {col.render ? col.render(row[col.key], row) : row[col.key] ?? "—"}
            </div>
          </div>
        ))}
      </div>

      {/* Ações */}
      {actionCol && (
        <div style={{ marginTop: 12, paddingTop: 10, borderTop: "0.5px solid var(--border)" }}>
          {actionCol.render(row[actionCol.key], row)}
        </div>
      )}
    </div>
  );
}

// ----------------------------------------------------------
// DataTable — tabela desktop / cards mobile
// ----------------------------------------------------------
export function DataTable({ columns, data, emptyMessage = "Nenhum registro encontrado." }) {
  const isMobile = useIsMobile();

  if (data.length === 0) {
    return (
      <div style={{ padding: "32px 16px", textAlign: "center", color: "#6B8CAE", fontSize: 13 }}>
        {emptyMessage}
      </div>
    );
  }

  // Mobile: cards
  if (isMobile) {
    return (
      <div style={{ padding: "12px 12px 4px" }}>
        {data.map((row, i) => (
          <MobileCard key={row.id ?? i} columns={columns} row={row} index={i} />
        ))}
      </div>
    );
  }

  // Desktop: tabela clássica
  return (
    <div style={{ overflowX: "auto" }}>
      <table style={{ width: "100%", borderCollapse: "collapse", fontSize: 13 }}>
        <thead>
          <tr>
            {columns.map((col) => (
              <th key={col.key} style={{
                background: "#E8F4FD", color: "#1E3A5F",
                fontWeight: 600, fontSize: 11, textTransform: "uppercase",
                letterSpacing: ".5px", padding: "10px 12px", textAlign: "left",
                borderBottom: "1px solid var(--border)",
              }}>
                {col.label}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {data.map((row, i) => (
            <tr key={row.id ?? i} style={{ borderBottom: "0.5px solid var(--border)" }}
              onMouseEnter={(e) => e.currentTarget.style.background = "#E8F4FD"}
              onMouseLeave={(e) => e.currentTarget.style.background = "transparent"}>
              {columns.map((col) => (
                <td key={col.key} style={{ padding: "10px 12px", color: "var(--text-primary)" }}>
                  {col.render ? col.render(row[col.key], row) : row[col.key] ?? "—"}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

// ----------------------------------------------------------
// Toolbar — flex-row desktop / flex-col mobile
// ----------------------------------------------------------
export function Toolbar({ children }) {
  const isMobile = useIsMobile();
  return (
    <div style={{
      display: "flex",
      flexDirection: isMobile ? "column" : "row",
      alignItems: isMobile ? "stretch" : "center",
      gap: 8, marginBottom: 14,
    }}>
      {children}
    </div>
  );
}

export function SearchInput({ value, onChange, placeholder = "Buscar…" }) {
  const isMobile = useIsMobile();
  return (
    <input
      type="search" value={value} onChange={(e) => onChange(e.target.value)}
      placeholder={placeholder}
      style={{
        ...inputStyle,
        height: isMobile ? 44 : 36,
        minWidth: isMobile ? "auto" : 200,
      }}
    />
  );
}

export function FilterSelect({ value, onChange, options, placeholder = "Todos" }) {
  const isMobile = useIsMobile();
  return (
    <select value={value} onChange={(e) => onChange(e.target.value)}
      style={{
        ...inputStyle,
        height: isMobile ? 44 : 36,
        minWidth: isMobile ? "auto" : 130,
      }}>
      <option value="">{placeholder}</option>
      {options.map((opt) => (
        <option key={opt.value ?? opt} value={opt.value ?? opt}>
          {opt.label ?? opt}
        </option>
      ))}
    </select>
  );
}

// ----------------------------------------------------------
// Card
// ----------------------------------------------------------
export function Card({ title, titleExtra, noPadding, children }) {
  return (
    <div style={{
      background: "var(--surface-2,#fff)", borderRadius: 12,
      border: "0.5px solid var(--border)", padding: noPadding ? 0 : "16px",
      marginBottom: 16,
    }}>
      {title && (
        <div style={{
          fontSize: 14, fontWeight: 600, color: "#0A1628",
          marginBottom: 12, display: "flex", alignItems: "center",
          justifyContent: "space-between",
          padding: noPadding ? "16px 16px 0" : 0,
        }}>
          {title}
          {titleExtra}
        </div>
      )}
      {children}
    </div>
  );
}

// ----------------------------------------------------------
// KpiCard
// ----------------------------------------------------------
export function KpiCard({ label, value, subLabel, subVariant = "muted" }) {
  const subColors = { ok: "#27AE60", warn: "#F39C12", bad: "#E74C3C", muted: "#6B8CAE" };
  return (
    <div style={{ background: "var(--surface-1,#F0F7FF)", borderRadius: 12, padding: "14px 16px" }}>
      <div style={{ fontSize: 11, color: "#6B8CAE", fontWeight: 500, textTransform: "uppercase", letterSpacing: ".5px", marginBottom: 4 }}>
        {label}
      </div>
      <div style={{ fontSize: 26, fontWeight: 700, color: "#0A1628" }}>{value}</div>
      {subLabel && (
        <div style={{ fontSize: 11, marginTop: 3, color: subColors[subVariant] }}>{subLabel}</div>
      )}
    </div>
  );
}

// ----------------------------------------------------------
// PageHeader — título + botão de ação
// Em mobile: empilhados verticalmente, botão full-width
// ----------------------------------------------------------
export function PageHeader({ title, description, action }) {
  const isMobile = useIsMobile();
  return (
    <div style={{
      marginBottom: 20,
      display: "flex",
      flexDirection: isMobile ? "column" : "row",
      alignItems: isMobile ? "stretch" : "flex-start",
      justifyContent: "space-between",
      gap: isMobile ? 12 : 0,
    }}>
      <div>
        <h2 style={{ fontSize: isMobile ? 18 : 20, fontWeight: 600, color: "#0A1628" }}>{title}</h2>
        {description && (
          <p style={{ fontSize: 13, color: "#6B8CAE", marginTop: 2 }}>{description}</p>
        )}
      </div>
      {action && (
        <div style={{ display: "flex" }}>
          {/* Clona o botão com fullWidth em mobile */}
          {isMobile
            ? <div style={{ width: "100%" }}>{action}</div>
            : action
          }
        </div>
      )}
    </div>
  );
}

// ----------------------------------------------------------
// LoadingSpinner
// ----------------------------------------------------------
export function LoadingSpinner() {
  return (
    <div style={{ display: "flex", justifyContent: "center", padding: "48px 0" }}>
      <div style={{
        width: 32, height: 32, border: "3px solid #E8F4FD",
        borderTopColor: "#2E86AB", borderRadius: "50%",
        animation: "pp-spin 0.7s linear infinite",
      }} />
      <style>{`@keyframes pp-spin{to{transform:rotate(360deg)}}`}</style>
    </div>
  );
}

// ----------------------------------------------------------
// ErrorMessage
// ----------------------------------------------------------
export function ErrorMessage({ message }) {
  return (
    <div style={{
      background: "rgba(231,76,60,.08)", border: "0.5px solid rgba(231,76,60,.3)",
      borderRadius: 10, padding: "12px 16px", fontSize: 13, color: "#c0392b",
      marginBottom: 12,
    }}>
      {message}
    </div>
  );
}

// ----------------------------------------------------------
// Tabs — scroll horizontal em mobile
// ----------------------------------------------------------
export function Tabs({ tabs, active, onChange }) {
  const isMobile = useIsMobile();
  return (
    <div style={{
      display: "flex", gap: 2, marginBottom: 16,
      background: "#E8F4FD", borderRadius: 10, padding: 3,
      width: isMobile ? "100%" : "fit-content",
      overflowX: isMobile ? "auto" : "visible",
      WebkitOverflowScrolling: "touch",
    }}>
      {tabs.map((tab) => (
        <button key={tab.id} onClick={() => onChange(tab.id)}
          style={{
            padding: isMobile ? "8px 14px" : "6px 14px",
            borderRadius: 8, fontSize: isMobile ? 13 : 12, fontWeight: 500,
            cursor: "pointer", border: "none",
            transition: "background .15s, color .15s",
            background: active === tab.id ? "#fff" : "transparent",
            color: active === tab.id ? "#2E86AB" : "#6B8CAE",
            boxShadow: active === tab.id ? "0 1px 3px rgba(0,0,0,.08)" : "none",
            whiteSpace: "nowrap", flexShrink: 0,
          }}>
          {tab.label}
        </button>
      ))}
    </div>
  );
}
