// ============================================================
//  Piscina Perfeita — App.jsx
//  Orquestra autenticação: exibe LoginPage ou AppLayout.
// ============================================================
import { AuthProvider, useAuth } from "./context/AuthContext.jsx";
import { AppLayout } from "./components/layout/AppLayout.jsx";
import LoginPage from "./modules/auth/LoginPage.jsx";
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
import { PERFIS } from "./config/index.js";
import { useState } from "react";

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
};

// ----------------------------------------------------------
// Conteúdo protegido — só renderiza após login
// ----------------------------------------------------------
function AuthenticatedApp() {
  const { isAuthenticated, user } = useAuth();
  const [activePage, setActivePage] = useState("dashboard");
  // Preenchimento inicial da tela de Aplicações quando acionada a partir
  // do botão "Registrar aplicação" em uma Análise (ver Analises.jsx).
  const [prefillAplicacao, setPrefillAplicacao] = useState(null);

  if (!isAuthenticated) return <LoginPage />;

  // Um Administrador criado sem nenhum Local vinculado (ex.: síndico
  // profissional recém-cadastrado) precisa criar seu primeiro condomínio
  // antes de usar o resto do sistema — todo o resto depende de um Local
  // ativo para funcionar.
  const precisaCriarPrimeiroLocal =
    (user?.perfil ?? user?.Perfil) === PERFIS.ADMINISTRADOR && !user?.localId;

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
  return (
    <AuthProvider>
      <AuthenticatedApp />
    </AuthProvider>
  );
}
