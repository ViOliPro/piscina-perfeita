// ============================================================
//  Piscina Perfeita — App.jsx
//  Orquestra autenticação: exibe LoginPage ou AppLayout.
// ============================================================
import { AuthProvider, useAuth } from "./context/AuthContext.jsx";
import { AppLayout } from "./components/layout/AppLayout.jsx";
import LoginPage from "./modules/auth/LoginPage.jsx";
import MeuPerfil from "./modules/auth/MeuPerfil.jsx";
import AceiteTermosGate from "./modules/auth/AceiteTermosGate.jsx";
import PrimeiroLocal from "./modules/onboarding/PrimeiroLocal.jsx";
import Dashboard from "./modules/dashboard/Dashboard.jsx";
import Analises from "./modules/analises/Analises.jsx";
import Estoque from "./modules/estoque/Estoque.jsx";
import Movimentacoes from "./modules/movimentacoes/Movimentacoes.jsx";
import Piscinas from "./modules/piscinas/Piscinas.jsx";
import Produtos from "./modules/produtos/Produtos.jsx";
import Usuarios from "./modules/usuarios/Usuarios.jsx";
import Locais from "./modules/locais/Locais.jsx";
import Depositos from "./modules/depositos/Depositos.jsx";
import Aplicacoes from "./modules/aplicacoes/Aplicacoes.jsx";
import ContagemInventario from "./modules/inventario/ContagemInventario.jsx";
import Hidrometro from "./modules/hidrometro/Hidrometro.jsx";
import TermosDeUsoPage from "./modules/legal/TermosDeUsoPage.jsx";
import PoliticaDePrivacidadePage from "./modules/legal/PoliticaDePrivacidadePage.jsx";
import PoliticaDeCookiesPage from "./modules/legal/PoliticaDeCookiesPage.jsx";
import { PERFIS, ROLES } from "./config/index.js";
import { useState, useEffect } from "react";
import { LoadingSpinner } from "./components/ui/index.jsx";

// Páginas legais são públicas de propósito: o usuário precisa poder lê-las
// antes de aceitar (no cadastro) ou a qualquer momento, sem precisar estar
// logado. Checadas antes de qualquer coisa relacionada a autenticação.
const LEGAL_PAGES = {
  "/termos-de-uso": TermosDeUsoPage,
  "/politica-de-privacidade": PoliticaDePrivacidadePage,
  "/politica-de-cookies": PoliticaDeCookiesPage,
};

const PAGES = {
  dashboard: Dashboard,
  analises: Analises,
  estoque: Estoque,
  movimentacoes: Movimentacoes,
  piscinas: Piscinas,
  produtos: Produtos,
  usuarios: Usuarios,
  locais: Locais,
  depositos: Depositos,
  aplicacoes: Aplicacoes,
  inventario: ContagemInventario,
  meuPerfil: MeuPerfil,
  hidrometro: Hidrometro,
};

// ----------------------------------------------------------
// Conteúdo protegido — só renderiza após login
// ----------------------------------------------------------
function AuthenticatedApp() {
  const { isAuthenticated, bootstrapping, user } = useAuth();
  const [activePage, setActivePage] = useState("dashboard");
  // Preenchimento inicial da tela de Aplicações quando acionada a partir
  // do botão "Registrar aplicação" em uma Análise (ver Analises.jsx).
  const [prefillAplicacao, setPrefillAplicacao] = useState(null);

  // Mesmo com sessão válida (ex.: computador compartilhado), um link de
  // redefinição/convite (?token=...) deve levar à LoginPage, não pular
  // direto pro Dashboard. Cobre tanto /redefinir-senha?token=... quanto
  // /completar-cadastro?token=... (LoginPage.jsx distingue pelo path).
  const hasAuthToken = new URLSearchParams(window.location.search).has("token");

  if (bootstrapping) {
    return (
      <>
        <style>{`
        @keyframes logoPulse {
          0%, 100% {
            transform: scale(1);
            opacity: 0.72;
            filter: saturate(0.85) drop-shadow(0 0 0 rgba(56, 189, 248, 0));
          }

          50% {
            transform: scale(1.1);
            opacity: 1;
            filter: saturate(1.2) drop-shadow(0 0 10px rgba(56, 189, 248, 0.35));
          }
        }

        @media (prefers-reduced-motion: reduce) {
          img[alt="PiscinaPerfeita"] {
            animation: none !important;
          }
        }
      `}</style>

        <div
          style={{
            display: "flex",
            flexDirection: "column",
            justifyContent: "center",
            alignItems: "center",
            minHeight: "100vh",
            gap: 16,
          }}
        >
          <img
            src="/favicon.svg"
            alt="PiscinaPerfeita"
            style={{
              width: 64,
              height: 64,
              animation: "logoPulse 2.4s ease-in-out infinite",
            }}
          />
          <LoadingSpinner />
        </div>
      </>
    );
  }

  if (!isAuthenticated || hasAuthToken) return <LoginPage />;

  // Pré-requisito pra tudo, inclusive criar o primeiro Local — cobre
  // contas que existiam antes desse recurso (ex.: usuário seed) e nunca
  // passaram pela tela de convite, onde o aceite normalmente é coletado.
  if (user?.precisaAceitarTermos) return <AceiteTermosGate />;

  // Um Administrador criado sem nenhum Local vinculado (ex.: síndico
  // profissional recém-cadastrado) precisa criar seu primeiro condomínio
  // antes de usar o resto do sistema — todo o resto depende de um Local
  // ativo para funcionar.
  const precisaCriarPrimeiroLocal =
    (user?.perfil ?? user?.Perfil) === PERFIS.ADMINISTRADOR &&
    !user?.localId &&
    (user?.role ?? user?.Role) === ROLES.User;

  if (precisaCriarPrimeiroLocal) return <PrimeiroLocal />;

  const PageComponent = PAGES[activePage] ?? Dashboard;

  function handleRegistrarAplicacao(piscinaId, analiseId) {
    setPrefillAplicacao({ piscinaId, analiseId });
    setActivePage("aplicacoes");
  }

  return (
    <AppLayout activePage={activePage} onNavigate={setActivePage}>
      <PageComponent
        onNavigate={setActivePage}
        onRegistrarAplicacao={handleRegistrarAplicacao}
        prefill={activePage === "aplicacoes" ? prefillAplicacao : null}
        onPrefillConsumed={() => setPrefillAplicacao(null)}
      />
    </AppLayout>
  );
}

// ----------------------------------------------------------
// Raiz — envolve tudo com AuthProvider
// ----------------------------------------------------------
export default function App() {
  const LegalPage = LEGAL_PAGES[window.location.pathname];

  if (LegalPage) return <LegalPage />;

  return (
    <AuthProvider>
      <AuthenticatedApp />
    </AuthProvider>
  );
}
