import { useState } from "react";
import { alterarSenha } from "../services/usuarioServiceAdditions";
import {
  labelStyle,
  inputStyle,
  primaryButtonStyle,
  errorTextStyle,
  helperTextStyle,
  successBannerStyle,
  tokens,
} from "../styles/tokens";

const REQUISITOS_SENHA = [
  { regex: /.{8,}/, label: "Ao menos 8 caracteres" },
  { regex: /[A-Z]/, label: "Uma letra maiúscula" },
  { regex: /[0-9]/, label: "Um número" },
];

export default function AlterarSenhaForm() {
  const [form, setForm] = useState({
    senhaAtual: "",
    novaSenha: "",
    confirmar: "",
  });
  const [erro, setErro] = useState(null);
  const [sucesso, setSucesso] = useState(false);
  const [enviando, setEnviando] = useState(false);

  const requisitosFaltando = REQUISITOS_SENHA.filter(
    (r) => !r.regex.test(form.novaSenha),
  );

  async function handleSubmit(e) {
    e.preventDefault();
    setErro(null);
    setSucesso(false);

    if (!form.senhaAtual) {
      setErro("Informe sua senha atual.");
      return;
    }
    if (requisitosFaltando.length > 0) {
      setErro("A nova senha não atende aos requisitos abaixo.");
      return;
    }
    if (form.novaSenha !== form.confirmar) {
      setErro("A confirmação não corresponde à nova senha.");
      return;
    }
    if (form.novaSenha === form.senhaAtual) {
      setErro("A nova senha deve ser diferente da atual.");
      return;
    }

    setEnviando(true);
    try {
      await alterarSenha({
        senhaAtual: form.senhaAtual,
        novaSenha: form.novaSenha,
      });
      setSucesso(true);
      setForm({ senhaAtual: "", novaSenha: "", confirmar: "" });
    } catch (err) {
      // request() (config/services.js) lança um Error simples com a
      // mensagem da API — não existe err.response neste projeto (sem axios).
      setErro(err?.message ?? "Não foi possível alterar a senha agora.");
    } finally {
      setEnviando(false);
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      {sucesso && (
        <div style={successBannerStyle}>Senha alterada com sucesso.</div>
      )}

      <div style={{ marginBottom: tokens.spacing(4) }}>
        <label style={labelStyle} htmlFor="senhaAtual">
          Senha atual
        </label>
        <input
          id="senhaAtual"
          type="password"
          style={inputStyle}
          value={form.senhaAtual}
          onChange={(e) =>
            setForm((f) => ({ ...f, senhaAtual: e.target.value }))
          }
          autoComplete="current-password"
        />
      </div>

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
        />
        <ul
          style={{ ...helperTextStyle, margin: "6px 0 0", paddingLeft: "18px" }}
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
        {enviando ? "Alterando..." : "Alterar senha"}
      </button>
    </form>
  );
}
