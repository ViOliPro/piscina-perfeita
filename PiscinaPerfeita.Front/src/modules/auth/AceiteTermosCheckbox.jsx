import { tokens } from "../styles/tokens";

// Checkbox de aceite obrigatório dos Termos de Uso / Política de
// Privacidade, usado nas duas telas de conclusão de cadastro
// (CompletarConvite.jsx e CompletarCadastroGoogle.jsx). Os links abrem em
// nova aba para as páginas públicas (acessíveis sem login — ver
// App.jsx), já que o usuário precisa poder ler os documentos antes de
// aceitar, não só depois de logado.
export default function AceiteTermosCheckbox({ checked, onChange, disabled }) {
  return (
    <label
      style={{
        display: "flex",
        alignItems: "flex-start",
        gap: 8,
        fontSize: 12.5,
        color: tokens.color.textMuted,
        cursor: disabled ? "default" : "pointer",
        marginBottom: tokens.spacing(4),
      }}
    >
      <input
        type="checkbox"
        checked={checked}
        disabled={disabled}
        onChange={(e) => onChange(e.target.checked)}
        style={{ marginTop: 2 }}
      />
      <span>
        Li e aceito os{" "}
        <a
          href="/termos-de-uso"
          target="_blank"
          rel="noopener noreferrer"
          style={{ color: tokens.color.accent }}
        >
          Termos de Uso
        </a>{" "}
        e a{" "}
        <a
          href="/politica-de-privacidade"
          target="_blank"
          rel="noopener noreferrer"
          style={{ color: tokens.color.accent }}
        >
          Política de Privacidade
        </a>
        .
      </span>
    </label>
  );
}
