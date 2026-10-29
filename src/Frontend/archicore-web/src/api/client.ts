import type { ApiProblem, AuthenticationResponse } from './types';

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL || '/api').replace(/\/$/, '');

export class ApiError extends Error {
  readonly status: number;
  readonly code?: string;

  constructor(message: string, status: number, code?: string) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.code = code;
  }
}

let activeSession: AuthenticationResponse | null = null;
const sessionEvent = 'archicore:session-changed';

export function getSession(): AuthenticationResponse | null {
  return activeSession;
}

export function setSession(next: AuthenticationResponse | null): void {
  activeSession = next;
  if (typeof window !== 'undefined') {
    window.dispatchEvent(new Event(sessionEvent));
  }
}

export function onSessionChanged(listener: () => void): () => void {
  window.addEventListener(sessionEvent, listener);
  return () => window.removeEventListener(sessionEvent, listener);
}

async function readProblem(response: Response): Promise<ApiError> {
  let message = `La solicitud falló (HTTP ${response.status}).`;
  let code: string | undefined;

  try {
    const body = (await response.json()) as ApiProblem;
    code = body.code;
    message = body.detail || body.title || message;
  } catch {
    // Some proxy/network errors do not contain a JSON body.
  }

  return new ApiError(message, response.status, code);
}

async function send<T>(
  path: string,
  init: RequestInit = {},
  retried = false,
): Promise<T> {
  const headers = new Headers(init.headers);
  if (init.body && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json');
  }

  const excludesRefresh = ['/auth/login', '/auth/refresh', '/auth/logout'].some((prefix) =>
    path.startsWith(prefix),
  );

  if (activeSession?.accessToken && !headers.has('Authorization')) {
    headers.set('Authorization', `Bearer ${activeSession.accessToken}`);
  }

  let response: Response;
  try {
    response = await fetch(`${API_BASE_URL}${path}`, { ...init, headers });
  } catch {
    throw new ApiError('No fue posible conectar con la API. Verifica que el backend esté ejecutándose.', 0);
  }

  if (response.status === 401 && !retried && !excludesRefresh && activeSession?.refreshToken) {
    const oldSession = activeSession;
    try {
      const refreshResponse = await fetch(`${API_BASE_URL}/auth/refresh`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken: oldSession.refreshToken }),
      });

      if (!refreshResponse.ok) {
        setSession(null);
        throw await readProblem(refreshResponse);
      }

      setSession((await refreshResponse.json()) as AuthenticationResponse);
      return send<T>(path, init, true);
    } catch (error) {
      setSession(null);
      if (error instanceof ApiError) throw error;
      throw new ApiError('La sesión expiró. Inicia sesión de nuevo.', 401);
    }
  }

  if (!response.ok) {
    throw await readProblem(response);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

export const api = {
  get: <T>(path: string) => send<T>(path),
  post: <T>(path: string, body?: unknown) =>
    send<T>(path, { method: 'POST', body: body === undefined ? undefined : JSON.stringify(body) }),
  login: (email: string, password: string) =>
    send<AuthenticationResponse>('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    }),
  logout: async () => {
    const session = activeSession;
    if (session?.refreshToken) {
      await send<void>('/auth/logout', {
        method: 'POST',
        body: JSON.stringify({ refreshToken: session.refreshToken }),
      }).catch(() => undefined);
    }
    setSession(null);
  },
};
