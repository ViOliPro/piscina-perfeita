// ============================================================
//  Piscina Perfeita — Componentes UI compartilhados
// ============================================================
import { useState } from "react";

// ----------------------------------------------------------
// Badge
// variant: "ok" | "warn" | "bad" | "info" | "purple"
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
// Button
// variant: "primary" | "ghost" | "danger"
// size: "md" | "sm"
// ----------------------------------------------------------
export function Button({ variant = "ghost", size = "md", onClick, disabled, children, type = "button" }) {
  const base = {
    display: "inline-flex", alignItems: "center", gap: 6,
    borderRadius: 6, fontWeight: 500, cursor: disabled ? "not-allowed" : "pointer",
    opacity: disabled ? .5 : 1, border: "0.5px solid", transition: "background .15s",
    fontFamily: "inherit",
  };
  const sizes = {
    md: { padding: "0 14px", height: 32, fontSize: 12 },
    sm: { padding: "0 10px", height: 26, fontSize: 11 },
  };
  const variants = {
    primary: { background: "#2E86AB", color: "#fff", borderColor: "#2E86AB" },
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
// Modal
// ----------------------------------------------------------
export function Modal({ open, onClose, title, children, footer }) {
  if (!open) return null;
  return (
    <div onClick={(e) => e.target === e.currentTarget && onClose()}
      style={{
        position: "fixed", inset: 0, background: "rgba(10,22,40,.5)",
        display: "flex", alignItems: "center", justifyContent: "center",
        zIndex: 100,
      }}>
      <div style={{
        background: "var(--surface-2,#fff)", borderRadius: 14,
        width: 520, maxWidth: "95vw", maxHeight: "90vh", overflow: "auto",
        border: "0.5px solid var(--border)",
      }}>
        <div style={{
          padding: "18px 20px 14px", borderBottom: "0.5px solid var(--border)",
          display: "flex", alignItems: "center", justifyContent: "space-between",
        }}>
          <h3 style={{ fontSize: 16, fontWeight: 600, color: "#0A1628" }}>{title}</h3>
          <button onClick={onClose} style={{
            background: "none", border: "none", cursor: "pointer",
            color: "#6B8CAE", fontSize: 18, lineHeight: 1,
          }}>✕</button>
        </div>
        <div style={{ padding: "18px 20px" }}>{children}</div>
        {footer && (
          <div style={{
            padding: "14px 20px", borderTop: "0.5px solid var(--border)",
            display: "flex", justifyContent: "flex-end", gap: 8,
          }}>
            {footer}
          </div>
        )}
      </div>
    </div>
  );
}

// ----------------------------------------------------------
// FormField — label + input/select/textarea
// ----------------------------------------------------------
export function FormField({ label, fullWidth, children }) {
  return (
    <div style={{
      display: "flex", flexDirection: "column", gap: 5,
      gridColumn: fullWidth ? "1 / -1" : undefined,
    }}>
      <label style={{ fontSize: 12, fontWeight: 600, color: "#1E3A5F" }}>{label}</label>
      {children}
    </div>
  );
}

// Estilos de input reutilizáveis — aplique via spread
export const inputStyle = {
  border: "0.5px solid #ccc", borderRadius: 6,
  padding: "8px 10px", fontSize: 13,
  background: "var(--surface-2,#fff)",
  color: "var(--text-primary,#111)",
  fontFamily: "inherit", outline: "none", width: "100%",
};

// ----------------------------------------------------------
// FormGrid — grade 2 colunas responsiva
// ----------------------------------------------------------
export function FormGrid({ children }) {
  return (
    <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 14 }}>
      {children}
    </div>
  );
}

// ----------------------------------------------------------
// FormSection — divisor com rótulo dentro do FormGrid
// ----------------------------------------------------------
export function FormSection({ label }) {
  return (
    <div style={{
      gridColumn: "1 / -1", fontSize: 11, fontWeight: 600,
      color: "#6B8CAE", textTransform: "uppercase", letterSpacing: ".8px",
      paddingTop: 4, borderTop: "0.5px solid var(--border)", marginTop: 4,
    }}>
      {label}
    </div>
  );
}

// ----------------------------------------------------------
// DataTable — tabela padrão com cabeçalho e linhas hover
// columns: [{ key, label, render? }]
// data: array de objetos
// ----------------------------------------------------------
export function DataTable({ columns, data, emptyMessage = "Nenhum registro encontrado." }) {
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
          {data.length === 0 ? (
            <tr>
              <td colSpan={columns.length} style={{
                padding: "24px 12px", textAlign: "center",
                color: "#6B8CAE", fontSize: 13,
              }}>
                {emptyMessage}
              </td>
            </tr>
          ) : (
            data.map((row, i) => (
              <tr key={row.id ?? i} style={{ borderBottom: "0.5px solid var(--border)" }}
                onMouseEnter={(e) => e.currentTarget.style.background = "#E8F4FD"}
                onMouseLeave={(e) => e.currentTarget.style.background = "transparent"}>
                {columns.map((col) => (
                  <td key={col.key} style={{ padding: "10px 12px", color: "var(--text-primary)" }}>
                    {col.render ? col.render(row[col.key], row) : row[col.key] ?? "—"}
                  </td>
                ))}
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
}

// ----------------------------------------------------------
// Toolbar — barra de filtros/ações acima das tabelas
// ----------------------------------------------------------
export function Toolbar({ children }) {
  return (
    <div style={{
      display: "flex", alignItems: "center", gap: 8,
      marginBottom: 14, flexWrap: "wrap",
    }}>
      {children}
    </div>
  );
}

export function SearchInput({ value, onChange, placeholder = "Buscar…" }) {
  return (
    <input
      type="text" value={value} onChange={(e) => onChange(e.target.value)}
      placeholder={placeholder}
      style={{ ...inputStyle, width: "auto", minWidth: 200, height: 32, padding: "0 10px" }}
    />
  );
}

export function FilterSelect({ value, onChange, options, placeholder = "Todos" }) {
  return (
    <select value={value} onChange={(e) => onChange(e.target.value)}
      style={{ ...inputStyle, width: "auto", minWidth: 130, height: 32, padding: "0 10px" }}>
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
// Card — container de seção
// ----------------------------------------------------------
export function Card({ title, titleExtra, noPadding, children }) {
  return (
    <div style={{
      background: "var(--surface-2,#fff)", borderRadius: 12,
      border: "0.5px solid var(--border)", padding: noPadding ? 0 : "16px 20px",
      marginBottom: 16,
    }}>
      {title && (
        <div style={{
          fontSize: 14, fontWeight: 600, color: "#0A1628",
          marginBottom: 12, display: "flex", alignItems: "center",
          justifyContent: "space-between", padding: noPadding ? "16px 20px 0" : 0,
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
// KpiCard — número resumo
// ----------------------------------------------------------
export function KpiCard({ label, value, subLabel, subVariant = "muted" }) {
  const subColors = {
    ok:   "#27AE60",
    warn: "#F39C12",
    bad:  "#E74C3C",
    muted:"#6B8CAE",
  };
  return (
    <div style={{
      background: "var(--surface-1,#f7f7f5)", borderRadius: 10,
      padding: "14px 16px",
    }}>
      <div style={{ fontSize: 11, color: "#6B8CAE", fontWeight: 500,
        textTransform: "uppercase", letterSpacing: ".5px", marginBottom: 4 }}>
        {label}
      </div>
      <div style={{ fontSize: 24, fontWeight: 600, color: "#0A1628" }}>{value}</div>
      {subLabel && (
        <div style={{ fontSize: 11, marginTop: 3, color: subColors[subVariant] }}>
          {subLabel}
        </div>
      )}
    </div>
  );
}

// ----------------------------------------------------------
// PageHeader
// ----------------------------------------------------------
export function PageHeader({ title, description, action }) {
  return (
    <div style={{
      marginBottom: 20, display: "flex",
      alignItems: "flex-start", justifyContent: "space-between",
    }}>
      <div>
        <h2 style={{ fontSize: 20, fontWeight: 600, color: "#0A1628" }}>{title}</h2>
        {description && (
          <p style={{ fontSize: 13, color: "#6B8CAE", marginTop: 2 }}>{description}</p>
        )}
      </div>
      {action}
    </div>
  );
}

// ----------------------------------------------------------
// LoadingSpinner
// ----------------------------------------------------------
export function LoadingSpinner() {
  return (
    <div style={{ display: "flex", justifyContent: "center", padding: "40px 0" }}>
      <div style={{
        width: 28, height: 28, border: "3px solid #E8F4FD",
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
      borderRadius: 8, padding: "12px 16px", fontSize: 13, color: "#c0392b",
      marginBottom: 12,
    }}>
      {message}
    </div>
  );
}

// ----------------------------------------------------------
// Tabs — aba simples
// tabs: [{ id, label }]
// ----------------------------------------------------------
export function Tabs({ tabs, active, onChange }) {
  return (
    <div style={{
      display: "flex", gap: 2, marginBottom: 16,
      background: "#E8F4FD", borderRadius: 8, padding: 3, width: "fit-content",
    }}>
      {tabs.map((tab) => (
        <button key={tab.id} onClick={() => onChange(tab.id)}
          style={{
            padding: "6px 14px", borderRadius: 6, fontSize: 12, fontWeight: 500,
            cursor: "pointer", border: "none", transition: "background .15s, color .15s",
            background: active === tab.id ? "var(--surface-2,#fff)" : "transparent",
            color: active === tab.id ? "#2E86AB" : "#6B8CAE",
            boxShadow: active === tab.id ? "0 1px 3px rgba(0,0,0,.08)" : "none",
          }}>
          {tab.label}
        </button>
      ))}
    </div>
  );
}
