import { useEffect, useState } from "react";
import {
  buscarMeuPerfil,
  atualizarMeuPerfil,
} from "../services/usuarioServiceAdditions";
import {
  tokens,
  cardStyle,
  labelStyle,
  inputStyle,
  primaryButtonStyle,
  errorTextStyle,
  successBannerStyle,
} from "../styles/tokens";
import AlterarSenhaForm from "./AlterarSenhaForm";

/**
 * Tela "Meu Cadastro". Dados de Perfil e Local são somente leitura aqui —
 * mudar Perfil ou vínculo de Local é responsabilidade do Administrador via
 * tela de gestão de usuários, não do próprio usuário.
 */
export default function MeuPerfil() {
  const [carregando, setCarregando] = useState(true);
  const [salvando, setSalvando] = useState(false);
  const [erro, setErro] = useState(null);
  const [sucesso, setSucesso] = useState(false);
  const [form, setForm] = useState({ nome: "", email: "" });
  const [perfilSomenteLeitura, setPerfilSomenteLeitura] = useState(null);

  useEffect(() => {
    buscarMeuPerfil()
      .then((usuario) => {
        setForm({ nome: usuario.nome, email: usuario.email });
        setPerfilSomenteLeitura({
          perfil: usuario.perfil,
          local: usuario.localAtualNome,
        });
      })
      .catch(() => setErro("Não foi possível carregar seus dados."))
      .finally(() => setCarregando(false));
  }, []);

  async function handleSubmit(e) {
    e.preventDefault();
    setErro(null);
    setSucesso(false);

    if (!form.nome.trim()) {
      setErro("Informe seu nome.");
      return;
    }
    if (!/^\S+@\S+\.\S+$/.test(form.email)) {
      setErro("Informe um e-mail válido.");
      return;
    }

    setSalvando(true);
    try {
      await atualizarMeuPerfil(form);
      setSucesso(true);
    } catch (err) {
      setErro(err?.message ?? "Não foi possível salvar as alterações.");
    } finally {
      setSalvando(false);
    }
  }

  if (carregando) {
    return <div style={{ padding: tokens.spacing(6) }}>Carregando...</div>;
  }

  return (
    <div
      style={{
        display: "flex",
        flexDirection: "column",
        gap: tokens.spacing(6),
        alignItems: "center",
        padding: tokens.spacing(4),
      }}
    >
      <div style={cardStyle}>
        <h2
          style={{
            margin: "0 0 4px",
            fontSize: "19px",
            color: tokens.color.text,
          }}
        >
          Meu cadastro
        </h2>
        <p
          style={{
            margin: "0 0 18px",
            fontSize: "13.5px",
            color: tokens.color.textMuted,
          }}
        >
          {perfilSomenteLeitura?.perfil}
          {perfilSomenteLeitura?.local
            ? ` · ${perfilSomenteLeitura.local}`
            : ""}
        </p>

        {sucesso && (
          <div style={successBannerStyle}>Dados atualizados com sucesso.</div>
        )}

        <form onSubmit={handleSubmit}>
          <div style={{ marginBottom: tokens.spacing(4) }}>
            <label style={labelStyle} htmlFor="nome">
              Nome
            </label>
            <input
              id="nome"
              style={inputStyle}
              value={form.nome}
              onChange={(e) => setForm((f) => ({ ...f, nome: e.target.value }))}
              autoComplete="name"
            />
          </div>

          <div style={{ marginBottom: tokens.spacing(4) }}>
            <label style={labelStyle} htmlFor="email">
              E-mail
            </label>
            <input
              id="email"
              type="email"
              style={inputStyle}
              value={form.email}
              onChange={(e) =>
                setForm((f) => ({ ...f, email: e.target.value }))
              }
              autoComplete="email"
            />
          </div>

          {erro && <p style={errorTextStyle}>{erro}</p>}

          <button
            type="submit"
            disabled={salvando}
            style={{
              ...primaryButtonStyle(salvando),
              marginTop: tokens.spacing(2),
            }}
          >
            {salvando ? "Salvando..." : "Salvar alterações"}
          </button>
        </form>
      </div>

      <div style={cardStyle}>
        <h3
          style={{
            margin: "0 0 14px",
            fontSize: "16px",
            color: tokens.color.text,
          }}
        >
          Alterar senha
        </h3>
        <AlterarSenhaForm />
      </div>
    </div>
  );
}
