import { createContext, useCallback, useContext, useEffect, useState, type ReactNode } from 'react';
import { api, getSession, onSessionChanged, setSession } from '../api/client';
import type { AuthenticationResponse, UserAccount } from '../api/types';

type AuthContextValue = {
  session: AuthenticationResponse | null;
  user: UserAccount | null;
  login: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSessionState] = useState<AuthenticationResponse | null>(getSession());

  useEffect(() => onSessionChanged(() => setSessionState(getSession())), []);

  const login = useCallback(async (email: string, password: string) => {
    const result = await api.login(email, password);
    setSession(result);
  }, []);

  const logout = useCallback(async () => {
    await api.logout();
  }, []);

  const value: AuthContextValue = { session, user: session?.user ?? null, login, logout };
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth debe usarse dentro de AuthProvider.');
  }
  return context;
}
