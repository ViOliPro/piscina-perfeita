import { useState } from "react";
import { useAuth } from "../../context/AuthContext.jsx";
import {
  tokens,
  cardStyle,
  labelStyle,
  inputStyle,
  primaryButtonStyle,
  errorTextStyle,
  helperTextStyle,
} from "../styles/tokens";

// Tela específica do login com Google quando há convite ativo pro e-mail,
// mas o cadastro ainda não foi concluído. Diferente de CompletarConvite.jsx
// (fluxo por link de e-mail com token de convite): aqui não há nome nem
// senha pra preencher — o Google já garante nome/e-mail, e reenviamos o
// mesmo idToken usado no botão "Entrar com Google" pro backend confirmar a
// identidade de novo antes de criar a conta.
export default function CompletarCadastroGoogle({ idToken, onVoltar }) {
  const { completarConviteGoogle, loading, error, setError } = useAuth();
  const [cpf, setCpf] = useState("");
  const [enviado, setEnviado] = useState(false);

  async function handleSubmit(e) {
    e.preventDefault();
    setError(null);
    const ok = await completarConviteGoogle(idToken, cpf.trim() || null);
    if (ok) setEnviado(true);
    // Sucesso já salva a sessão no AuthContext — o app troca de tela
    // sozinho quando isAuthenticated vira true, não navegamos daqui.
  }

  return (
    <div
      style={{
        display: "flex",
        justifyContent: "center",
        padding: tokens.spacing(6),
      }}
    >
      <div style={cardStyle}>
        <h2
          style={{
            margin: "0 0 6px",
            fontSize: "19px",
            color: tokens.color.text,
          }}
        >
          Quase lá
        </h2>
        <p
          style={{
            margin: "0 0 16px",
            fontSize: "13px",
            color: tokens.color.textMuted,
          }}
        >
          Encontramos um convite pendente para o seu e-mail do Google. Confirme
          abaixo para concluir seu cadastro.
        </p>

        {enviado ? (
          <p style={{ fontSize: "14px", color: tokens.color.success }}>
            Cadastro concluído! Entrando...
          </p>
        ) : (
          <form onSubmit={handleSubmit}>
            <div style={{ marginBottom: tokens.spacing(4) }}>
              <label style={labelStyle} htmlFor="cpf">
                CPF (opcional)
              </label>
              <input
                id="cpf"
                type="text"
                style={inputStyle}
                value={cpf}
                onChange={(e) => setCpf(e.target.value)}
                placeholder="000.000.000-00"
                autoComplete="off"
                disabled={loading}
              />
              <p style={{ ...helperTextStyle, margin: "6px 0 0" }}>
                Pode preencher depois em "Meu cadastro", se preferir.
              </p>
            </div>

            {error && <p style={errorTextStyle}>{error}</p>}

            <button
              type="submit"
              disabled={loading}
              style={primaryButtonStyle(loading)}
            >
              {loading ? "Confirmando..." : "Concluir cadastro"}
            </button>
          </form>
        )}

        {onVoltar && !enviado && (
          <button
            type="button"
            onClick={onVoltar}
            style={{
              background: "none",
              border: "none",
              cursor: "pointer",
              fontSize: 13,
              color: tokens.color.accent,
              fontFamily: "inherit",
              display: "block",
              margin: "16px auto 0",
            }}
          >
            ← Voltar
          </button>
        )}
      </div>
    </div>
  );
}
