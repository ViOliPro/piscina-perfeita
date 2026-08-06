// ============================================================
//  Piscina Perfeita — Style tokens
//
//  Paleta reaproveitada do restante do app (ver LoginPage.jsx) para as
//  telas de auth (Esqueci senha / Redefinir senha / Meu Perfil). Este
//  arquivo era importado mas nunca existiu — por isso essas telas nunca
//  compilavam.
// ============================================================
export const tokens = {
  color: {
    text: "#0A1628",
    textMuted: "#6B8CAE",
    border: "#c8dce8",
    accent: "#2E86AB",
    success: "#1a7a43",
    error: "#c0392b",
  },
  spacing: (n) => n * 4,
};

export const cardStyle = {
  background: "#fff",
  borderRadius: 16,
  padding: "28px 24px",
  width: "100%",
  maxWidth: 420,
  boxShadow: "0 8px 32px rgba(10,22,40,.1)",
};

export const labelStyle = {
  display: "block",
  fontSize: 12,
  fontWeight: 600,
  color: tokens.color.text,
  marginBottom: 6,
};

export const inputStyle = {
  width: "100%",
  border: `1.5px solid ${tokens.color.border}`,
  borderRadius: 8,
  padding: "10px 12px",
  fontSize: 14,
  background: "#fff",
  color: tokens.color.text,
  fontFamily: "inherit",
  outline: "none",
  boxSizing: "border-box",
};

export const primaryButtonStyle = (disabled) => ({
  width: "100%",
  background: disabled ? "#6B8CAE" : tokens.color.accent,
  color: "#fff",
  border: "none",
  borderRadius: 8,
  padding: "12px 0",
  fontSize: 14,
  fontWeight: 600,
  cursor: disabled ? "not-allowed" : "pointer",
  fontFamily: "inherit",
  transition: "background .15s",
});

export const errorTextStyle = {
  fontSize: 13,
  color: tokens.color.error,
  margin: "8px 0 0",
};

export const helperTextStyle = {
  fontSize: 12,
  color: tokens.color.textMuted,
  listStyle: "none",
  padding: 0,
};

export const successBannerStyle = {
  background: "rgba(39,174,96,.08)",
  border: "0.5px solid rgba(39,174,96,.3)",
  borderRadius: 8,
  padding: "10px 14px",
  fontSize: 13,
  color: tokens.color.success,
  marginBottom: 12,
};
