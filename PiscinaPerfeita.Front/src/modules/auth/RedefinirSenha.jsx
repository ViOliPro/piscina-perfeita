import { useState } from "react";
import { useSearchParams, useNavigate } from "react-router-dom"; // ajuste se usarem outro router
import { redefinirSenha } from "../services/usuarioServiceAdditions";
import {
  tokens,
  cardStyle,
  labelStyle,
  inputStyle,
  primaryButtonStyle,
  errorTextStyle,
  helperTextStyle,
} from "../styles/tokens";

const REQUISITOS_SENHA = [
  { regex: /.{8,}/, label: "Ao menos 8 caracteres" },
  { regex: /[A-Z]/, label: "Uma letra maiúscula" },
  { regex: /[0-9]/, label: "Um número" },
];

export default function RedefinirSenha() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const token = searchParams.get("token");

  const [form, setForm] = useState({ novaSenha: "", confirmar: "" });
  const [erro, setErro] = useState(null);
  const [enviando, setEnviando] = useState(false);
  const [sucesso, setSucesso] = useState(false);

  const requisitosFaltando = REQUISITOS_SENHA.filter(
    (r) => !r.regex.test(form.novaSenha),
  );

  if (!token) {
    return (
      <div
        style={{
          display: "flex",
          justifyContent: "center",
          padding: tokens.spacing(6),
        }}
      >
        <div style={cardStyle}>
          <p style={errorTextStyle}>
            Link inválido. Solicite um novo link de redefinição de senha.
          </p>
        </div>
      </div>
    );
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setErro(null);

    if (requisitosFaltando.length > 0) {
      setErro("A senha não atende aos requisitos abaixo.");
      return;
    }
    if (form.novaSenha !== form.confirmar) {
      setErro("A confirmação não corresponde à nova senha.");
      return;
    }

    setEnviando(true);
    try {
      await redefinirSenha({ token, novaSenha: form.novaSenha });
      setSucesso(true);
      setTimeout(() => navigate("/login"), 2500);
    } catch (err) {
      setErro(
        err?.response?.status === 400
          ? "Este link expirou ou já foi usado. Solicite um novo."
          : "Não foi possível redefinir a senha agora.",
      );
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
          Definir nova senha
        </h2>

        {sucesso ? (
          <p style={{ fontSize: "14px", color: tokens.color.success }}>
            Senha redefinida. Redirecionando para o login...
          </p>
        ) : (
          <form onSubmit={handleSubmit}>
            <div style={{ marginBottom: tokens.spacing(4) }}>
              <label style={labelStyle} htmlFor="novaSenha">
                Nova senha
              </label>
              <input
                id="novaSenha"
                type="password"
                style={inputStyle}
                value={form.novaSenha}
                onChange={(e) =>
                  setForm((f) => ({ ...f, novaSenha: e.target.value }))
                }
                autoComplete="new-password"
                autoFocus
              />
              <ul
                style={{
                  ...helperTextStyle,
                  margin: "6px 0 0",
                  paddingLeft: "18px",
                }}
              >
                {REQUISITOS_SENHA.map((r) => (
                  <li
                    key={r.label}
                    style={{
                      color: r.regex.test(form.novaSenha)
                        ? tokens.color.success
                        : tokens.color.textMuted,
                    }}
                  >
                    {r.label}
                  </li>
                ))}
              </ul>
            </div>

            <div style={{ marginBottom: tokens.spacing(4) }}>
              <label style={labelStyle} htmlFor="confirmar">
                Confirmar nova senha
              </label>
              <input
                id="confirmar"
                type="password"
                style={inputStyle}
                value={form.confirmar}
                onChange={(e) =>
                  setForm((f) => ({ ...f, confirmar: e.target.value }))
                }
                autoComplete="new-password"
              />
            </div>

            {erro && <p style={errorTextStyle}>{erro}</p>}

            <button
              type="submit"
              disabled={enviando}
              style={primaryButtonStyle(enviando)}
            >
              {enviando ? "Salvando..." : "Redefinir senha"}
            </button>
          </form>
        )}
      </div>
    </div>
  );
}
