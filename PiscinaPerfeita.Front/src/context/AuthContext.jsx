// ============================================================
//  Piscina Perfeita — AuthContext
//  Gerencia estado de autenticação globalmente.
// ============================================================
import {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
} from "react";
import { authService, registrarSessaoHandlers } from "../config/services.js";
import { can } from "../helpers/Permissions.js";

// Cache otimista para evitar flashing da tela —
// a fonte de verdade é o refresh de token no boot.
const USER_KEY = "pp_user";

// ----------------------------------------------------------
// Helpers de Storage Cache
// ----------------------------------------------------------
function readCachedUser() {
  try {
    const raw = localStorage.getItem(USER_KEY);
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
}

function saveUserCache(user) {
  localStorage.setItem(USER_KEY, JSON.stringify(user));
}

function clearUserCache() {
  localStorage.removeItem(USER_KEY);
}

// ----------------------------------------------------------
// Context
// ----------------------------------------------------------
const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [token, setToken] = useState(null); // mantido apenas em memória
  const [user, setUser] = useState(readCachedUser()); // estado otimista
  const [bootstrapping, setBootstrapping] = useState(true);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    registrarSessaoHandlers({
      onSessaoRenovada: (sessao) => {
        setToken(sessao.accessToken);
        setUser(sessao.user);
        saveUserCache(sessao.user);
      },
      onSessaoExpirada: () => {
        clearUserCache();
        setToken(null);
        setUser(null);
      },
    });
  }, []);

  // Tentativa inicial de restauração da sessão via Refresh Token
  useEffect(() => {
    authService
      .refresh()
      .then((sessao) => {
        setToken(sessao.accessToken);
        setUser(sessao.user);
        saveUserCache(sessao.user);
      })
      .catch(() => {
        clearUserCache();
        setUser(null);
      })
      .finally(() => setBootstrapping(false));
  }, []);

  const login = useCallback(async ({ email, password }) => {
    setLoading(true);
    setError(null);
    try {
      const res = await authService.login({ email, password });
      const accessToken = res.accessToken ?? res.AccessToken;
      const userPayload = res.user ?? res.User;

      setToken(accessToken);
      setUser(userPayload);
      saveUserCache(userPayload);
      return true;
    } catch (err) {
      setError(err.message ?? "Erro ao fazer login.");
      return false;
    } finally {
      setLoading(false);
    }
  }, []);

  const logout = useCallback(() => {
    authService.logout().catch(() => {});
    clearUserCache();
    setToken(null);
    setUser(null);
    setError(null);
  }, []);

  // Login com Google. Retorna { ok, convitePendente } em vez de só
  // true/false: "convite pendente" não é bem um erro — é um sinal pra
  // LoginPage trocar de tela, por isso não usa o `error` global nesse caso.
  const loginGoogle = useCallback(async (idToken) => {
    setLoading(true);
    setError(null);
    try {
      const res = await authService.loginGoogle(idToken);
      setToken(res.accessToken);
      setUser(res.user);
      saveUserCache(res.user);
      return { ok: true };
    } catch (err) {
      if (err.codigo === "ConvitePendente") {
        return { ok: false, convitePendente: true };
      }
      setError(err.message ?? "Erro ao fazer login com Google.");
      return { ok: false };
    } finally {
      setLoading(false);
    }
  }, []);

  // Completa o cadastro do convite ativo via Google (Cpf opcional).
  const completarConviteGoogle = useCallback(async (idToken, cpf) => {
    setLoading(true);
    setError(null);
    try {
      const res = await authService.completarConviteGoogle(idToken, cpf);
      setToken(res.accessToken);
      setUser(res.user);
      saveUserCache(res.user);
      return true;
    } catch (err) {
      setError(err.message ?? "Não foi possível concluir seu cadastro agora.");
      return false;
    } finally {
      setLoading(false);
    }
  }, []);

  // Troca o Local (condomínio/unidade) ativo — usada quando o usuário tem
  // vínculo com mais de um Local. Emite um novo token JWT (com o novo
  // local_id) e atualiza a sessão.
  const switchLocal = useCallback(async (newLocalId) => {
    setLoading(true);
    setError(null);
    try {
      const res = await authService.switchLocal(newLocalId);
      setToken(res.accessToken);
      setUser(res.user);
      saveUserCache(res.user);
      return true;
    } catch (err) {
      setError(err.message ?? "Erro ao trocar de local.");
      return false;
    } finally {
      setLoading(false);
    }
  }, []);
  const isAuthenticated = !!token;

  return (
    <AuthContext.Provider
      value={{
        token,
        user,
        isAuthenticated,
        bootstrapping,
        loading,
        error,
        login,
        loginGoogle,
        completarConviteGoogle,
        logout,
        switchLocal,
        setError,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

// Custom Hooks
export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth deve ser usado dentro de <AuthProvider>");
  return ctx;
}

export function useCan(requiredPermission) {
  const { user } = useAuth();
  return can(requiredPermission, user);
}
