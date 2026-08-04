import { useState } from "react";
import { solicitarRedefinicaoSenha } from "../services/usuarioServiceAdditions";
import {
  tokens,
  cardStyle,
  labelStyle,
  inputStyle,
  primaryButtonStyle,
  errorTextStyle,
} from "../styles/tokens";

export default function EsqueciSenha({ onBack }) {
  const [email, setEmail] = useState("");
  const [enviado, setEnviado] = useState(false);
  const [erro, setErro] = useState(null);
  const [enviando, setEnviando] = useState(false);

  async function handleSubmit(e) {
    e.preventDefault();
    setErro(null);

    if (!/^\S+@\S+\.\S+$/.test(email)) {
      setErro("Informe um e-mail válido.");
      return;
    }

    setEnviando(true);
    try {
      await solicitarRedefinicaoSenha({ email });
      // Sempre mostra a mesma mensagem, exista ou não o e-mail —
      // o backend também não diferencia a resposta (evita enumerar contas).
      setEnviado(true);
    } catch {
      setErro("Não foi possível processar o pedido agora. Tente novamente.");
    } finally {
      setEnviando(false);
    }
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
          Esqueci minha senha
        </h2>

        {enviado ? (
          <p
            style={{
              fontSize: "14px",
              color: tokens.color.text,
              lineHeight: 1.5,
            }}
          >
            Se houver uma conta com o e-mail informado, você vai receber um link
            para redefinir sua senha em instantes. O link expira em 1 hora.
          </p>
        ) : (
          <form onSubmit={handleSubmit}>
            <p
              style={{
                margin: "0 0 16px",
                fontSize: "13.5px",
                color: tokens.color.textMuted,
              }}
            >
              Informe o e-mail cadastrado para receber o link de redefinição.
            </p>

            <div style={{ marginBottom: tokens.spacing(4) }}>
              <label style={labelStyle} htmlFor="email">
                E-mail
              </label>
              <input
                id="email"
                type="email"
                style={inputStyle}
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                autoComplete="email"
                autoFocus
              />
            </div>

            {erro && <p style={errorTextStyle}>{erro}</p>}

            <button
              type="submit"
              disabled={enviando}
              style={primaryButtonStyle(enviando)}
            >
              {enviando ? "Enviando..." : "Enviar link de redefinição"}
            </button>
          </form>
        )}

        {onBack && (
          <button
            type="button"
            onClick={onBack}
            style={{
              background: "none",
              border: "none",
              cursor: "pointer",
              fontSize: 13,
              color: tokens.color.accent,
              fontFamily: "inherit",
              display: "flex",
              alignItems: "center",
              gap: 4,
              margin: "16px auto 0",
            }}
          >
            ← Voltar ao login
          </button>
        )}
      </div>
    </div>
  );
}
