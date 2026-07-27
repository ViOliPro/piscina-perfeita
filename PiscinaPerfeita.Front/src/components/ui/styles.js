// inputStyle — padding maior em mobile via CSS clamp
export const inputStyle = {
  border: "1px solid #c8dce8",
  borderRadius: 8,
  padding: "10px 12px",
  fontSize: 14,
  background: "var(--surface-2,#fff)",
  color: "var(--text-primary,#111)",
  fontFamily: "inherit",
  outline: "none",
  width: "100%",
  WebkitAppearance: "none", // remove seta dupla iOS nos selects
  transition: "border-color .15s, box-shadow .15s",
};
