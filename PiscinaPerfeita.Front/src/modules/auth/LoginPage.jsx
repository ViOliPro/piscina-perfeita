// ============================================================
//  Piscina Perfeita — Tela de Login
//
//  Fluxos:
//   "login"   → formulário principal (email + senha)
//   "forgot"  → solicitar redefinição (email) — EsqueciSenha.jsx
//   "reset"   → redefinir senha com token da URL — RedefinirSenha.jsx
//   "convite" → aceitar convite (/completar-cadastro?token=...) — CompletarConvite.jsx
// ============================================================
import { useState } from "react";
import { useAuth } from "../../context/AuthContext.jsx";
import { APP_META } from "../../config/index.js";
import { LogoIcon } from "../../components/ui/Logo.jsx";
import EsqueciSenha from "./EsqueciSenha.jsx";
import RedefinirSenha from "./RedefinirSenha.jsx";
import CompletarConvite from "./CompletarConvite.jsx";
import { GoogleLogin } from "@react-oauth/google";
import CompletarCadastroGoogle from "./CompletarCadastroGoogle.jsx";

// ----------------------------------------------------------
// Onda decorativa SVG
// ----------------------------------------------------------
function WaveSVG() {
  return (
    <svg
      viewBox="0 0 1200 120"
      preserveAspectRatio="none"
      style={{
        position: "absolute",
        bottom: 0,
        left: 0,
        width: "100%",
        height: 80,
        display: "block",
      }}
    >
      <path
        d="M0,40 C150,80 350,0 600,40 C850,80 1050,0 1200,40 L1200,120 L0,120 Z"
        fill="rgba(94,192,235,0.15)"
      />
      <path
        d="M0,60 C200,100 400,20 600,60 C800,100 1000,20 1200,60 L1200,120 L0,120 Z"
        fill="rgba(46,134,171,0.12)"
      />
    </svg>
  );
}

// ----------------------------------------------------------
// Bolhas decorativas animadas
// ----------------------------------------------------------
function Bubbles() {
  const bubbles = [
    { size: 60, left: "8%", delay: 0, duration: 7 },
    { size: 30, left: "20%", delay: 1.5, duration: 5 },
    { size: 80, left: "75%", delay: 0.5, duration: 9 },
    { size: 20, left: "88%", delay: 2, duration: 6 },
    { size: 45, left: "55%", delay: 3, duration: 8 },
    { size: 15, left: "40%", delay: 0.8, duration: 4 },
  ];

  return (
    <>
      <style>{`
        @keyframes pp-bubble {
          0%   { transform: translateY(0)   scale(1);   opacity: .25; }
          50%  { transform: translateY(-40px) scale(1.05); opacity: .15; }
          100% { transform: translateY(0)   scale(1);   opacity: .25; }
        }
      `}</style>
      {bubbles.map((b, i) => (
        <div
          key={i}
          style={{
            position: "absolute",
            bottom: `${10 + i * 8}%`,
            left: b.left,
            width: b.size,
            height: b.size,
            borderRadius: "50%",
            border: "1.5px solid rgba(94,192,235,0.4)",
            background: "rgba(94,192,235,0.06)",
            animation: `pp-bubble ${b.duration}s ease-in-out ${b.delay}s infinite`,
            pointerEvents: "none",
          }}
        />
      ))}
    </>
  );
}

// ----------------------------------------------------------
// Campo de input reutilizável interno
// ----------------------------------------------------------
function Field({
  label,
  type = "text",
  value,
  onChange,
  placeholder,
  autoComplete,
  required,
  disabled,
}) {
  const [focused, setFocused] = useState(false);
  return (
    <div style={{ display: "flex", flexDirection: "column", gap: 6 }}>
      <label style={{ fontSize: 12, fontWeight: 600, color: "#1E3A5F" }}>
        {label}
      </label>
      <input
        type={type}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        autoComplete={autoComplete}
        required={required}
        disabled={disabled}
        onFocus={() => setFocused(true)}
        onBlur={() => setFocused(false)}
        style={{
          border: `1.5px solid ${focused ? "#2E86AB" : "#c8dce8"}`,
          borderRadius: 8,
          padding: "10px 12px",
          fontSize: 14,
          background: "#fff",
          color: "#0A1628",
          fontFamily: "inherit",
          outline: "none",
          transition: "border-color .15s, box-shadow .15s",
          boxShadow: focused ? "0 0 0 3px rgba(46,134,171,.12)" : "none",
          opacity: disabled ? 0.6 : 1,
        }}
      />
    </div>
  );
}

// ----------------------------------------------------------
// Alerta de erro / sucesso
// ----------------------------------------------------------
function Alert({ variant, message }) {
  if (!message) return null;
  const styles = {
    error: {
      bg: "rgba(231,76,60,.08)",
      border: "rgba(231,76,60,.3)",
      color: "#c0392b",
      icon: "⚠️",
    },
    success: {
      bg: "rgba(39,174,96,.08)",
      border: "rgba(39,174,96,.3)",
      color: "#1a7a43",
      icon: "✅",
    },
    info: {
      bg: "rgba(46,134,171,.08)",
      border: "rgba(46,134,171,.3)",
      color: "#1a5f80",
      icon: "ℹ️",
    },
  };
  const s = styles[variant] ?? styles.info;
  return (
    <div
      style={{
        background: s.bg,
        border: `0.5px solid ${s.border}`,
        borderRadius: 8,
        padding: "10px 14px",
        fontSize: 13,
        color: s.color,
        display: "flex",
        alignItems: "flex-start",
        gap: 8,
      }}
    >
      <span>{s.icon}</span>
      <span>{message}</span>
    </div>
  );
}

// ----------------------------------------------------------
// Formulário de Login
// ----------------------------------------------------------
function LoginForm({ onForgot }) {
  const { login, loginGoogle, loading, error, setError } = useAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPass, setShowPass] = useState(false);

  async function handleSubmit(e) {
    e.preventDefault();
    await login({ email, password });
  }

  return (
    <div>
      <form
        onSubmit={handleSubmit}
        style={{ display: "flex", flexDirection: "column", gap: 16 }}
      >
        <div style={{ textAlign: "center", marginBottom: 4 }}>
          <h2 style={{ fontSize: 22, fontWeight: 700, color: "#0A1628" }}>
            Bem-vindo de volta
          </h2>
          <p style={{ fontSize: 13, color: "#6B8CAE", marginTop: 4 }}>
            Acesse sua conta para continuar
          </p>
        </div>

        {error && <Alert variant="error" message={error} />}

        <Field
          label="E-mail"
          type="email"
          value={email}
          onChange={(v) => {
            setEmail(v);
            setError(null);
          }}
          placeholder="seu@email.com.br"
          autoComplete="email"
          required
          disabled={loading}
        />

        <div style={{ display: "flex", flexDirection: "column", gap: 6 }}>
          <div
            style={{
              display: "flex",
              justifyContent: "space-between",
              alignItems: "center",
            }}
          >
            <label style={{ fontSize: 12, fontWeight: 600, color: "#1E3A5F" }}>
              Senha
            </label>
            <button
              type="button"
              onClick={onForgot}
              style={{
                background: "none",
                border: "none",
                cursor: "pointer",
                fontSize: 12,
                color: "#2E86AB",
                fontFamily: "inherit",
                padding: 0,
              }}
            >
              Esqueci minha senha
            </button>
          </div>
          <div style={{ position: "relative" }}>
            <input
              type={showPass ? "text" : "password"}
              value={password}
              onChange={(e) => {
                setPassword(e.target.value);
                setError(null);
              }}
              placeholder="••••••••"
              autoComplete="current-password"
              required
              disabled={loading}
              style={{
                border: "1.5px solid #c8dce8",
                borderRadius: 8,
                padding: "10px 40px 10px 12px",
                fontSize: 14,
                background: "#fff",
                color: "#0A1628",
                fontFamily: "inherit",
                outline: "none",
                width: "100%",
                opacity: loading ? 0.6 : 1,
              }}
            />
            <button
              type="button"
              onClick={() => setShowPass((s) => !s)}
              style={{
                position: "absolute",
                right: 10,
                top: "50%",
                transform: "translateY(-50%)",
                background: "none",
                border: "none",
                cursor: "pointer",
                color: "#6B8CAE",
                fontSize: 16,
                padding: 0,
                lineHeight: 1,
              }}
              aria-label={showPass ? "Ocultar senha" : "Mostrar senha"}
            >
              {showPass ? "🙈" : "👁️"}
            </button>
          </div>
        </div>

        <button
          type="submit"
          disabled={loading}
          style={{
            background: loading ? "#6B8CAE" : "#2E86AB",
            color: "#fff",
            border: "none",
            borderRadius: 8,
            padding: "12px 0",
            fontSize: 14,
            fontWeight: 600,
            cursor: loading ? "not-allowed" : "pointer",
            fontFamily: "inherit",
            transition: "background .15s",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            gap: 8,
            marginTop: 4,
          }}
        >
          {loading ? (
            <>
              <span
                style={{
                  width: 16,
                  height: 16,
                  border: "2px solid rgba(255,255,255,.4)",
                  borderTopColor: "#fff",
                  borderRadius: "50%",
                  animation: "pp-spin .7s linear infinite",
                  display: "inline-block",
                }}
              />
              Entrando…
            </>
          ) : (
            "Entrar"
          )}
        </button>
      </form>

      <div
        style={{
          display: "flex",
          alignItems: "center",
          gap: 10,
          margin: "18px 0",
        }}
      >
        <div style={{ flex: 1, height: 1, background: "#e3edf3" }} />
        <span style={{ fontSize: 11, color: "#6B8CAE" }}>ou</span>
        <div style={{ flex: 1, height: 1, background: "#e3edf3" }} />
      </div>

      <div style={{ display: "flex", justifyContent: "center" }}>
        <GoogleLogin
          onSuccess={async (credentialResponse) => {
            const idToken = credentialResponse.credential;
            const resultado = await loginGoogle(idToken);
            if (resultado.convitePendente) {
              onConvitePendente?.(idToken);
            }
            // Sucesso: sessão já salva pelo AuthContext, o app troca de tela
            // sozinho. Falha "normal": o erro já aparece no Alert acima,
            // igual ao login por e-mail/senha.
          }}
          onError={() =>
            setError("Não foi possível entrar com o Google agora.")
          }
        />
      </div>
    </div>
  );
}

// ----------------------------------------------------------
// Formulário Esqueci minha senha / Redefinir senha
//
// CORRIGIDO: removidos os ForgotForm/ResetForm embutidos (que tinham uma
// mensagem hardcoded dizendo que o backend ainda não existia). Trocados
// pelos componentes dedicados EsqueciSenha.jsx e RedefinirSenha.jsx, que
// agora falam com endpoints reais.
// ----------------------------------------------------------

// (ResetForm removido — ver RedefinirSenha.jsx)

// ----------------------------------------------------------
// Componente principal da página de login
// ----------------------------------------------------------
export default function LoginPage() {
  // Detecta token na query string: ?token=...
  // (nome alinhado com os links que o backend gera em
  // UsuarioService.PasswordResetToken/CriarConvite — antes esta página lia
  // "reset_token", que nunca existia no link real)
  //
  // Reset de senha e convite usam o mesmo nome de parâmetro, então
  // distinguimos pelo path — o app não usa router, mas window.location.pathname
  // continua disponível normalmente.
  const params = new URLSearchParams(window.location.search);
  const token = params.get("token");
  const isConvite = window.location.pathname === "/completar-cadastro";
  const resetToken = !isConvite ? token : null;
  const conviteToken = isConvite ? token : null;

  const [view, setView] = useState(
    conviteToken ? "convite" : resetToken ? "reset" : "login",
  );

  const [googleIdToken, setGoogleIdToken] = useState(null);
  return (
    <div
      style={{
        minHeight: "100vh",
        display: "flex",
        background:
          "linear-gradient(160deg, #0A1628 0%, #1E3A5F 50%, #2E86AB 100%)",
        position: "relative",
        overflow: "hidden",
      }}
    >
      <style>{`
        @keyframes pp-spin { to { transform: rotate(360deg); } }
        @keyframes pp-card-in {
          from { opacity: 0; transform: translateY(24px) scale(.97); }
          to   { opacity: 1; transform: translateY(0)    scale(1);   }
        }
      `}</style>

      <Bubbles />
      <WaveSVG />

      {/* Painel esquerdo — branding */}
      <div
        style={{
          flex: 1,
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
          justifyContent: "center",
          padding: "40px 60px",
          color: "#fff",
          position: "relative",
          zIndex: 1,
        }}
        className="login-brand-panel"
      >
        {/* Logo / ícone */}
        <div style={{ marginBottom: 20 }}>
          <LogoIcon height={64} />
        </div>

        <h1
          style={{
            fontSize: 38,
            fontWeight: 800,
            margin: "0 0 10px",
            letterSpacing: "-0.5px",
            lineHeight: 1.1,
          }}
        >
          Piscina
          <br />
          Perfeita
        </h1>
        <p
          style={{
            fontSize: 15,
            opacity: 0.75,
            maxWidth: 300,
            textAlign: "center",
            lineHeight: 1.6,
          }}
        >
          Gestão integrada de piscinas, análises de qualidade e controle de
          estoque.
        </p>

        <div
          style={{
            marginTop: 40,
            display: "flex",
            flexDirection: "column",
            gap: 16,
            width: "100%",
            maxWidth: 280,
          }}
        >
          {[
            { icon: "🧪", text: "Análises de pH, cloro e temperatura" },
            { icon: "📦", text: "Controle de estoque e movimentações" },
            { icon: "📊", text: "Dashboard com indicadores em tempo real" },
          ].map((item) => (
            <div
              key={item.text}
              style={{
                display: "flex",
                alignItems: "center",
                gap: 12,
                background: "rgba(255,255,255,.06)",
                borderRadius: 10,
                padding: "10px 14px",
              }}
            >
              <span style={{ fontSize: 20 }}>{item.icon}</span>
              <span style={{ fontSize: 13, opacity: 0.85 }}>{item.text}</span>
            </div>
          ))}
        </div>
      </div>

      {/* Painel direito — formulário */}
      <div
        className="login-form-panel"
        style={{
          width: "100%",
          maxWidth: 420,
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          padding: 32,
          position: "relative",
          zIndex: 1,
        }}
      >
        <div
          style={{
            background: "#fff",
            borderRadius: 16,
            padding: "36px 32px",
            width: "100%",
            boxShadow: "0 24px 64px rgba(0,0,0,.25)",
            animation: "pp-card-in .3s ease-out",
          }}
        >
          {/* Cabeçalho do card */}
          <div
            style={{
              display: "flex",
              alignItems: "center",
              gap: 10,
              marginBottom: 28,
            }}
          >
            <LogoIcon height={38} />
            <div>
              <div style={{ fontSize: 13, fontWeight: 700, color: "#0A1628" }}>
                {APP_META.name}
              </div>
              <div style={{ fontSize: 11, color: "#6B8CAE" }}>
                {view === "login" && "Acesso ao sistema"}
                {view === "forgot" && "Recuperação de acesso"}
                {view === "reset" && "Redefinição de senha"}
              </div>
            </div>
          </div>

          {/* Formulário ativo */}
          {view === "login" && (
            <LoginForm
              onForgot={() => setView("forgot")}
              onConvitePendente={(idToken) => {
                setGoogleIdToken(idToken);
                setView("convite-google");
              }}
            />
          )}
          {view === "forgot" && (
            <EsqueciSenha onBack={() => setView("login")} />
          )}
          {view === "reset" && (
            <RedefinirSenha
              token={resetToken}
              onDone={() => {
                window.history.replaceState({}, "", window.location.pathname);
                setView("login");
              }}
            />
          )}
          {view === "convite" && (
            <CompletarConvite
              token={conviteToken}
              onDone={() => {
                window.history.replaceState({}, "", "/");
                setView("login");
              }}
            />
          )}
          {view === "convite-google" && (
            <CompletarCadastroGoogle
              idToken={googleIdToken}
              onVoltar={() => {
                setGoogleIdToken(null);
                setView("login");
              }}
            />
          )}

          {/* Rodapé */}
          <div
            style={{
              marginTop: 24,
              paddingTop: 16,
              borderTop: "0.5px solid rgba(30,58,95,.1)",
              textAlign: "center",
              fontSize: 11,
              color: "#6B8CAE",
            }}
          >
            {APP_META.name} v{APP_META.version}
          </div>
        </div>
      </div>

      {/* Ocultar painel esquerdo em telas pequenas */}
      <style>{`
        @media (max-width: 720px) {
          .login-brand-panel { display: none !important; }
          .login-form-panel { padding: 20px !important; }
        }
      `}</style>
    </div>
  );
}
