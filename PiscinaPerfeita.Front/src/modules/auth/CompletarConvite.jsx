import { useState } from "react";
import { authService } from "../../config/services.js";
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

// Tela de "aceitar convite": o Admin/SuperAdmin já decidiu e-mail, perfil
// e Local (se aplicável) ao gerar o convite — aqui o convidado só define
// nome e senha. Sem react-router (mesmo padrão de RedefinirSenha.jsx):
// token e onDone chegam como props vindas de LoginPage.jsx.
export default function CompletarConvite({ token, onDone }) {
  const [form, setForm] = useState({ nome: "", senha: "", confirmar: "" });
  const [erro, setErro] = useState(null);
  const [enviando, setEnviando] = useState(false);
  const [sucesso, setSucesso] = useState(false);

  const requisitosFaltando = REQUISITOS_SENHA.filter(
    (r) => !r.regex.test(form.senha),
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
            Link de convite inválido. Peça para quem te convidou enviar um
            novo link.
          </p>
        </div>
      </div>
    );
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setErro(null);

    if (!form.nome.trim()) {
      setErro("Informe seu nome.");
      return;
    }
    if (requisitosFaltando.length > 0) {
      setErro("A senha não atende aos requisitos abaixo.");
      return;
    }
    if (form.senha !== form.confirmar) {
      setErro("A confirmação não corresponde à senha.");
      return;
    }

    setEnviando(true);
    try {
      await authService.completarConvite({
        token,
        nome: form.nome.trim(),
        senha: form.senha,
      });
      setSucesso(true);
      setTimeout(() => onDone?.(), 2500);
    } catch (err) {
      // request() (config/services.js) lança um Error simples com a
      // mensagem da API — não existe err.response neste projeto (sem axios).
      setErro(err?.message ?? "Não foi possível concluir o cadastro agora.");
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
          Completar cadastro
        </h2>
        <p
          style={{
            margin: "0 0 16px",
            fontSize: "13px",
            color: tokens.color.textMuted,
          }}
        >
          Você foi convidado para o PiscinaPerfeita. Defina seu nome e senha
          para continuar.
        </p>

        {sucesso ? (
          <p style={{ fontSize: "14px", color: tokens.color.success }}>
            Cadastro concluído! Redirecionando para o login...
          </p>
        ) : (
          <form onSubmit={handleSubmit}>
            <div style={{ marginBottom: tokens.spacing(4) }}>
              <label style={labelStyle} htmlFor="nome">
                Nome completo
              </label>
              <input
                id="nome"
                type="text"
                style={inputStyle}
                value={form.nome}
                onChange={(e) =>
                  setForm((f) => ({ ...f, nome: e.target.value }))
                }
                autoComplete="name"
                autoFocus
              />
            </div>

            <div style={{ marginBottom: tokens.spacing(4) }}>
              <label style={labelStyle} htmlFor="senha">
                Senha
              </label>
              <input
                id="senha"
                type="password"
                style={inputStyle}
                value={form.senha}
                onChange={(e) =>
                  setForm((f) => ({ ...f, senha: e.target.value }))
                }
                autoComplete="new-password"
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
                      color: r.regex.test(form.senha)
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
                Confirmar senha
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
              {enviando ? "Salvando..." : "Concluir cadastro"}
            </button>
          </form>
        )}
      </div>
    </div>
  );
}
