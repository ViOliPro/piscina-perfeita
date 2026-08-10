// ============================================================
//  Piscina Perfeita — AuthContext
//  Gerencia estado de autenticação globalmente.
//
//  O backend retorna:
//  {
//    accessToken : string,
//    tokenType   : "Bearer",
//    expiresIn   : 28800,          // segundos (8h)
//    user: { nome, email, role }
//  }
// ============================================================
import { createContext, useContext, useState, useCallback } from "react";
import { authService } from "../config/services.js";
import { can } from "../helpers/Permissions.js";

const TOKEN_KEY = "pp_token";
const USER_KEY = "pp_user";
const EXPIRES_KEY = "pp_expires";

// ----------------------------------------------------------
// Helpers de storage
// ----------------------------------------------------------
function saveSession(accessToken, user, expiresIn) {
  const expiresAt = Date.now() + expiresIn * 1000;
  localStorage.setItem(TOKEN_KEY, accessToken);
  localStorage.setItem(USER_KEY, JSON.stringify(user));
  localStorage.setItem(EXPIRES_KEY, String(expiresAt));
}

function clearSession() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
  localStorage.removeItem(EXPIRES_KEY);
}

function readSession() {
  const token = localStorage.getItem(TOKEN_KEY);
  const userRaw = localStorage.getItem(USER_KEY);
  const expiresAt = Number(localStorage.getItem(EXPIRES_KEY) ?? 0);

  if (!token || !userRaw || Date.now() > expiresAt) {
    clearSession();
    return { token: null, user: null };
  }

  try {
    return { token, user: JSON.parse(userRaw) };
  } catch {
    clearSession();
    return { token: null, user: null };
  }
}

// ----------------------------------------------------------
// Context
// ----------------------------------------------------------
// ... (mantenha os imports e os helpers de storage iguais acima)

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const initial = readSession();
  const [token, setToken] = useState(initial.token);
  const [user, setUser] = useState(initial.user);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  // Login — chama a API e persiste a sessão
  const login = useCallback(async ({ email, password }) => {
    setLoading(true);
    setError(null);
    try {
      const res = await authService.login({ email, password });
      const accessToken = res.accessToken ?? res.AccessToken;
      const expiresIn = res.expiresIn ?? res.ExpiresIn ?? 28800;
      const userPayload = res.user ?? res.User;

      saveSession(accessToken, userPayload, expiresIn);
      setToken(accessToken);
      setUser(userPayload);
      return true;
    } catch (err) {
      setError(err.message ?? "Erro ao fazer login.");
      return false;
    } finally {
      setLoading(false);
    }
  }, []);

  // Login com Google
  const loginGoogle = useCallback(async (idToken) => {
    setLoading(true);
    setError(null);
    try {
      const res = await authService.loginGoogle(idToken);
      saveSession(res.accessToken, res.user, res.expiresIn);
      setToken(res.accessToken);
      setUser(res.user);
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

  // Completa o cadastro do convite ativo via Google
  const completarConviteGoogle = useCallback(async (idToken, cpf) => {
    setLoading(true);
    setError(null);
    try {
      const res = await authService.completarConviteGoogle(idToken, cpf);
      saveSession(res.accessToken, res.user, res.expiresIn);
      setToken(res.accessToken);
      setUser(res.user);
      return true;
    } catch (err) {
      setError(err.message ?? "Não foi possível concluir seu cadastro agora.");
      return false;
    } finally {
      setLoading(false);
    }
  }, []);

  // Logout — limpa sessão local
  const logout = useCallback(() => {
    clearSession();
    setToken(null);
    setUser(null);
    setError(null);
  }, []);

  // Troca o Local ativo
  const switchLocal = useCallback(async (newLocalId) => {
    setLoading(true);
    setError(null);
    try {
      const res = await authService.switchLocal(newLocalId);
      const accessToken = res.accessToken;
      const expiresIn = res.expiresIn ?? 28800;
      const userPayload = res.user;

      saveSession(accessToken, userPayload, expiresIn);
      setToken(accessToken);
      setUser(userPayload);
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
        loading,
        error,
        loginGoogle,
        completarConviteGoogle,
        login,
        logout,
        switchLocal,
        setError,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

// Custom Hooks (devem ficar fora de AuthProvider)
export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth deve ser usado dentro de <AuthProvider>");
  return ctx;
}

export function useCan(requiredPermission) {
  const { user } = useAuth();
  return can(requiredPermission, user);
}
