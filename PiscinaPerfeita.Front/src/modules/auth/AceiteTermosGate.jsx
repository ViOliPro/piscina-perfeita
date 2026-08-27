import { useRef, useState } from "react";
import { useAuth } from "../../context/AuthContext.jsx";
import AceiteTermosCheckbox from "./AceiteTermosCheckbox.jsx";

// Bloqueia o acesso ao app até o usuário aceitar a versão atual dos Termos
// de Uso/Política de Privacidade — cobre contas que existiam antes desse
// recurso (ex.: usuário seed) e por isso nunca passaram pela tela de
// CompletarConvite, onde o aceite normalmente é coletado. Ver
// user.precisaAceitarTermos (calculado pelo backend em cada login/refresh)
// e AuthenticatedApp em App.jsx, que renderiza este componente antes de
// qualquer página do sistema quando a flag vem true.
export default function AceiteTermosGate() {
  const { aceitarTermos, logout } = useAuth();
  const [aceite, setAceite] = useState(false);
  const [enviando, setEnviando] = useState(false);
  const [erro, setErro] = useState(null);
  const enviandoRef = useRef(false); // trava síncrona — o disabled do botão só

  // reflete o state depois do próximo render, e um clique duplo bem rápido
  // pode disparar handleConfirmar duas vezes antes disso acontecer.

  async function handleConfirmar() {
    if (enviandoRef.current) return;

    if (!aceite) {
      setErro(
        "É necessário aceitar os Termos de Uso e a Política de Privacidade.",
      );
      return;
    }
    enviandoRef.current = true;
    setEnviando(true);
    setErro(null);
    try {
      await aceitarTermos();
    } catch (err) {
      setErro(
        err.message ??
          "Não foi possível registrar o aceite agora. Tente novamente.",
      );
      enviandoRef.current = false;
      setEnviando(false);
    }
  }

  return (
    <div
      style={{
        minHeight: "100vh",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        background: "#f4f8fb",
        padding: 24,
      }}
    >
      <div
        style={{
          maxWidth: 440,
          width: "100%",
          background: "#fff",
          borderRadius: 16,
          padding: "32px 28px",
          boxShadow: "0 8px 32px rgba(10,22,40,.08)",
        }}
      >
        <h1 style={{ fontSize: 19, margin: "0 0 8px", color: "#0A1628" }}>
          Atualizamos nossos Termos
        </h1>
        <p
          style={{
            fontSize: 13.5,
            color: "#3d4c5e",
            lineHeight: 1.5,
            margin: "0 0 20px",
          }}
        >
          Antes de continuar, precisamos que você confirme que leu e aceita os
          Termos de Uso e a Política de Privacidade do Piscina Perfeita.
        </p>

        <AceiteTermosCheckbox
          checked={aceite}
          onChange={setAceite}
          disabled={enviando}
        />

        {erro && (
          <p style={{ color: "#c0392b", fontSize: 13, margin: "8px 0 0" }}>
            {erro}
          </p>
        )}

        <div style={{ display: "flex", gap: 12, marginTop: 16 }}>
          <button
            type="button"
            onClick={handleConfirmar}
            disabled={enviando || !aceite}
            style={{
              flex: 1,
              padding: "10px 16px",
              borderRadius: 8,
              border: "none",
              background: enviando || !aceite ? "#9db6c9" : "#2E86AB",
              color: "#fff",
              fontWeight: 600,
              fontSize: 14,
              cursor: enviando || !aceite ? "default" : "pointer",
            }}
          >
            {enviando ? "Confirmando..." : "Aceitar e continuar"}
          </button>
          <button
            type="button"
            onClick={logout}
            disabled={enviando}
            style={{
              padding: "10px 16px",
              borderRadius: 8,
              border: "1.5px solid #c8dce8",
              background: "#fff",
              color: "#3d4c5e",
              fontSize: 14,
              cursor: enviando ? "default" : "pointer",
            }}
          >
            Sair
          </button>
        </div>
      </div>
    </div>
  );
}
