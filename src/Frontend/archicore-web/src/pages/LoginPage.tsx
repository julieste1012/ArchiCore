import { useState, type FormEvent } from 'react';
import { ArrowUpRight, Boxes, Eye, EyeOff, Fingerprint, LockKeyhole } from 'lucide-react';
import { useAuth } from '../auth/AuthProvider';
import { ApiError } from '../api/client';
import { ErrorMessage } from '../components/Feedback';

export function LoginPage() {
  const { login } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError('');
    setBusy(true);
    try {
      await login(email.trim(), password);
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : 'No fue posible iniciar sesión.');
    } finally {
      setBusy(false);
    }
  }

  return (
    <main className="login-screen">
      <section className="login-visual">
        <div className="brand brand-light">
          <span className="brand-mark"><Boxes size={25} strokeWidth={2.1} /></span>
          <span>ArchiCore</span>
        </div>
        <div className="login-art">
          <div className="art-orbit orbit-one" />
          <div className="art-orbit orbit-two" />
          <div className="art-document">
            <span className="document-ribbon">DMS / ERP</span>
            <div className="document-symbol"><Boxes size={53} strokeWidth={1.3} /></div>
            <span className="doc-line line-long" />
            <span className="doc-line line-mid" />
            <span className="doc-line line-short" />
            <span className="document-foot">MULTISERTEC · CONTROL DOCUMENTAL</span>
          </div>
          <div className="art-note note-top"><Fingerprint size={17} /> Trazabilidad por diseño</div>
          <div className="art-note note-bottom"><span className="live-dot" /> Plataforma operativa</div>
        </div>
        <div className="login-visual-footer">
          <p className="visual-kicker">ORDEN EN CADA DOCUMENTO.</p>
          <h2>La información correcta, en el lugar correcto.</h2>
          <p>Custodia, operación y seguimiento documental en una sola plataforma.</p>
        </div>
      </section>

      <section className="login-panel">
        <div className="login-panel-top"><span>PORTAL INTERNO</span><span className="secure-label"><LockKeyhole size={13} /> Acceso seguro</span></div>
        <div className="login-form-wrap">
          <p className="eyebrow">BIENVENIDO DE NUEVO</p>
          <h1>Ingresa a tu espacio.</h1>
          <p className="login-intro">Usa tus credenciales de ArchiCore para continuar con la operación.</p>
          <form className="login-form" onSubmit={handleSubmit}>
            <label htmlFor="email">Correo electrónico</label>
            <input
              id="email"
              type="email"
              autoComplete="username"
              required
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              placeholder="nombre@empresa.com"
            />
            <div className="password-label-row"><label htmlFor="password">Contraseña</label></div>
            <div className="password-input-wrap">
              <input
                id="password"
                type={showPassword ? 'text' : 'password'}
                autoComplete="current-password"
                required
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                placeholder="Ingresa tu contraseña"
              />
              <button className="icon-button reveal-button" type="button" aria-label={showPassword ? 'Ocultar contraseña' : 'Mostrar contraseña'} onClick={() => setShowPassword((value) => !value)}>
                {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
              </button>
            </div>
            {error && <ErrorMessage>{error}</ErrorMessage>}
            <button className="primary-button login-submit" type="submit" disabled={busy}>
              {busy ? 'Validando…' : 'Iniciar sesión'} <ArrowUpRight size={18} />
            </button>
          </form>
          <p className="login-security-note"><LockKeyhole size={14} /> Tu sesión no se guarda en el almacenamiento persistente del navegador.</p>
        </div>
        <footer className="login-footer"><span>© 2026 ArchiCore</span><span>Gestión documental · Colombia</span></footer>
      </section>
    </main>
  );
}
