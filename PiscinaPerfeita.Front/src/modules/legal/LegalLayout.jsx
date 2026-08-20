import { tokens } from "../styles/tokens";

// Layout simples para as páginas legais públicas (Termos de Uso, Política
// de Privacidade, Política de Cookies). Acessível sem login: são
// renderizadas direto pelo App.jsx com base no pathname, antes de
// qualquer checagem de autenticação — ver App.jsx e isLegalPath().
export default function LegalLayout({ title, updatedAt, children }) {
  return (
    <div
      style={{
        minHeight: "100vh",
        background: "#f4f8fb",
        padding: `${tokens.spacing(8)}px ${tokens.spacing(4)}px`,
      }}
    >
      <div
        style={{
          maxWidth: 720,
          margin: "0 auto",
          background: "#fff",
          borderRadius: 16,
          padding: "32px 28px",
          boxShadow: "0 8px 32px rgba(10,22,40,.08)",
          color: tokens.color.text,
          lineHeight: 1.6,
          fontSize: 14.5,
        }}
      >
        <a
          href="/"
          style={{
            fontSize: 13,
            color: tokens.color.accent,
            textDecoration: "none",
          }}
        >
          ← Voltar
        </a>

        <h1 style={{ fontSize: 22, margin: "16px 0 4px" }}>{title}</h1>
        <p style={{ fontSize: 12.5, color: tokens.color.textMuted, margin: "0 0 24px" }}>
          Última atualização: {updatedAt}
        </p>

        {children}
      </div>
    </div>
  );
}

export const h2Style = { fontSize: 16, margin: "24px 0 8px", color: tokens.color.text };
export const pStyle = { margin: "0 0 12px" };
export const ulStyle = { margin: "0 0 12px", paddingLeft: 20 };
export const tableStyle = {
  width: "100%",
  borderCollapse: "collapse",
  fontSize: 13,
  margin: "0 0 16px",
};
export const thStyle = {
  textAlign: "left",
  borderBottom: `1.5px solid ${tokens.color.border}`,
  padding: "6px 8px",
  color: tokens.color.text,
};
export const tdStyle = {
  borderBottom: `1px solid ${tokens.color.border}`,
  padding: "6px 8px",
  verticalAlign: "top",
};
