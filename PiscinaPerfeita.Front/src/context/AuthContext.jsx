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

const TOKEN_KEY    = "pp_token";
const USER_KEY     = "pp_user";
const EXPIRES_KEY  = "pp_expires";

// ----------------------------------------------------------
// Helpers de storage
// ----------------------------------------------------------
function saveSession(accessToken, user, expiresIn) {
  const expiresAt = Date.now() + expiresIn * 1000;
  localStorage.setItem(TOKEN_KEY,   accessToken);
  localStorage.setItem(USER_KEY,    JSON.stringify(user));
  localStorage.setItem(EXPIRES_KEY, String(expiresAt));
}

function clearSession() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
  localStorage.removeItem(EXPIRES_KEY);
}

function readSession() {
  const token     = localStorage.getItem(TOKEN_KEY);
  const userRaw   = localStorage.getItem(USER_KEY);
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
const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const initial = readSession();
  const [token,  setToken]  = useState(initial.token);
  const [user,   setUser]   = useState(initial.user);
  const [loading, setLoading] = useState(false);
  const [error,   setError]   = useState(null);

  // Login — chama a API e persiste a sessão
  const login = useCallback(async ({ email, password }) => {
    setLoading(true);
    setError(null);
    try {
      const res = await authService.login({ email, password });
      // Normaliza para camelCase (a API pode retornar AccessToken ou accessToken)
      const accessToken = res.accessToken ?? res.AccessToken;
      const expiresIn   = res.expiresIn   ?? res.ExpiresIn   ?? 28800;
      const userPayload = res.user        ?? res.User;

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

  // Logout — limpa sessão local
  const logout = useCallback(() => {
    clearSession();
    setToken(null);
    setUser(null);
    setError(null);
  }, []);

  const isAuthenticated = !!token;

  return (
    <AuthContext.Provider value={{ token, user, isAuthenticated, loading, error, login, logout, setError }}>
      {children}
    </AuthContext.Provider>
  );
}

// Hook de conveniência
export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth deve ser usado dentro de <AuthProvider>");
  return ctx;
}
