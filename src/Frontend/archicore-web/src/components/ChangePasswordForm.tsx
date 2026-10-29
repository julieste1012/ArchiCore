import { useState, type FormEvent } from 'react';
import { KeyRound } from 'lucide-react';
import { api, ApiError } from '../api/client';
import { ErrorMessage, SuccessMessage } from './Feedback';

type ChangePasswordFormProps = {
  onComplete: () => void;
  compact?: boolean;
};

function meetsPasswordPolicy(password: string): boolean {
  return password.length >= 12
    && /[A-Z]/.test(password)
    && /[a-z]/.test(password)
    && /\d/.test(password)
    && /[^A-Za-z0-9]/.test(password);
}

export function ChangePasswordForm({ onComplete, compact = false }: ChangePasswordFormProps) {
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [completed, setCompleted] = useState(false);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError('');

    if (!meetsPasswordPolicy(newPassword)) {
      setError('La nueva contraseña debe tener al menos 12 caracteres e incluir mayúscula, minúscula, número y símbolo.');
      return;
    }
    if (newPassword !== confirmPassword) {
      setError('La confirmación no coincide con la nueva contraseña.');
      return;
    }
    if (newPassword === currentPassword) {
      setError('La nueva contraseña debe ser diferente de la actual.');
      return;
    }

    setSaving(true);
    try {
      await api.post<void>('/auth/change-password', { currentPassword, newPassword });
      setCurrentPassword('');
      setNewPassword('');
      setConfirmPassword('');
      setCompleted(true);
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : 'No se pudo cambiar la contraseña.');
    } finally {
      setSaving(false);
    }
  }

  return (
    <section className={`content-card create-card password-change-card${compact ? ' password-change-compact' : ''}`}>
      <div className="section-heading">
        <div><p className="eyebrow">SEGURIDAD DE CUENTA</p><h2>{completed ? 'Contraseña actualizada' : 'Cambiar contraseña'}</h2></div>
        <span className="password-icon"><KeyRound size={19} /></span>
      </div>
      {error && <ErrorMessage>{error}</ErrorMessage>}
      {completed ? (
        <>
          <SuccessMessage>Tu contraseña cambió correctamente y se revocaron las sesiones anteriores.</SuccessMessage>
          <p className="form-hint">Por seguridad, cierra esta sesión e ingresa de nuevo con tu nueva contraseña.</p>
          <button type="button" className="primary-button" onClick={onComplete}>Cerrar sesión y volver a ingresar</button>
        </>
      ) : (
        <form className="form-grid" onSubmit={(event) => void submit(event)}>
          <div>
            <label htmlFor="current-password">Contraseña actual</label>
            <input id="current-password" type="password" autoComplete="current-password" maxLength={256} required value={currentPassword} onChange={(event) => setCurrentPassword(event.target.value)} />
          </div>
          <div>
            <label htmlFor="new-password">Nueva contraseña</label>
            <input id="new-password" type="password" autoComplete="new-password" minLength={12} maxLength={256} required value={newPassword} onChange={(event) => setNewPassword(event.target.value)} />
          </div>
          <div>
            <label htmlFor="confirm-password">Confirmar nueva contraseña</label>
            <input id="confirm-password" type="password" autoComplete="new-password" minLength={12} maxLength={256} required value={confirmPassword} onChange={(event) => setConfirmPassword(event.target.value)} />
          </div>
          <p className="form-hint">Debe incluir mayúscula, minúscula, número y símbolo. El sistema no almacena la contraseña en texto legible.</p>
          <div className="form-actions"><button className="primary-button" type="submit" disabled={saving}>{saving ? 'Actualizando…' : 'Actualizar contraseña'}</button></div>
        </form>
      )}
    </section>
  );
}
