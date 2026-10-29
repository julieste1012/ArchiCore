import { useEffect, useState, type FormEvent } from 'react';
import { CheckCircle2, LockKeyhole, Plus, RefreshCw, Search, ShieldCheck, UserCog, UserRoundX, X } from 'lucide-react';
import { api, ApiError } from '../api/client';
import type { PagedResult, UserAccount } from '../api/types';
import { ErrorMessage, LoadingState, SuccessMessage } from '../components/Feedback';
import { ModuleHeader } from '../components/ModuleHeader';
import { StatusBadge } from '../components/StatusBadge';
import { useAuth } from '../auth/AuthProvider';

const roles = [
  { value: 'SystemAdministrator', label: 'Administrador del sistema' },
  { value: 'AdministrativeManager', label: 'Gerencia administrativa' },
  { value: 'ProjectCoordinator', label: 'Coordinador de proyectos' },
  { value: 'ArchiveTechnician', label: 'Técnico de archivo' },
  { value: 'ArchiveAssistant', label: 'Auxiliar de archivo' },
  { value: 'DigitizationOperator', label: 'Operador de digitalización' },
  { value: 'BillingAssistant', label: 'Auxiliar de facturación' },
  { value: 'CustomerRepresentative', label: 'Representante del cliente' },
];

function strongPassword(value: string) {
  return value.length >= 12
    && /[A-Z]/.test(value)
    && /[a-z]/.test(value)
    && /\d/.test(value)
    && /[^A-Za-z0-9]/.test(value);
}

export function UsersPage() {
  const { user: currentUser } = useAuth();
  const [data, setData] = useState<PagedResult<UserAccount> | null>(null);
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [busy, setBusy] = useState(true);
  const [saving, setSaving] = useState(false);
  const [showCreate, setShowCreate] = useState(false);
  const [email, setEmail] = useState('');
  const [displayName, setDisplayName] = useState('');
  const [password, setPassword] = useState('');
  const [role, setRole] = useState('ArchiveTechnician');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  async function load(requestedPage = page) {
    setBusy(true);
    setError('');
    try {
      setData(await api.get<PagedResult<UserAccount>>(`/auth/users?page=${requestedPage}&pageSize=20`));
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : 'No se pudo cargar el directorio de usuarios.');
    } finally {
      setBusy(false);
    }
  }

  useEffect(() => { void load(page); }, [page]);

  async function create(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError('');
    setSuccess('');

    if (!strongPassword(password)) {
      setError('La contraseña debe tener al menos 12 caracteres, mayúscula, minúscula, número y símbolo.');
      return;
    }

    setSaving(true);
    try {
      await api.post<UserAccount>('/auth/users', {
        email: email.trim(),
        displayName: displayName.trim(),
        password,
        role,
      });
      setEmail('');
      setDisplayName('');
      setPassword('');
      setRole('ArchiveTechnician');
      setShowCreate(false);
      setPage(1);
      setSuccess('Usuario creado correctamente. Comparte sus credenciales por un canal seguro.');
      await load(1);
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : 'No se pudo crear el usuario.');
    } finally {
      setSaving(false);
    }
  }

  async function toggleStatus(account: UserAccount) {
    const activating = !account.isActive;
    const action = activating ? 'activar' : 'desactivar';
    if (account.id === currentUser?.id && !activating) {
      setError('No puedes desactivar tu propia cuenta de administrador.');
      return;
    }
    if (!window.confirm(`¿Confirmas ${action} la cuenta de ${account.displayName}?${activating ? '' : ' Se revocarán sus sesiones activas.'}`)) return;

    setError('');
    setSuccess('');
    try {
      await api.put<UserAccount>(`/auth/users/${account.id}/status`, { isActive: activating });
      setSuccess(`Cuenta ${activating ? 'activada' : 'desactivada'} correctamente.`);
      await load(page);
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : `No se pudo ${action} la cuenta.`);
    }
  }

  const rows = (data?.items ?? []).filter((account) =>
    !search || `${account.displayName} ${account.email} ${account.role}`.toLowerCase().includes(search.toLowerCase()),
  );

  return (
    <>
      <ModuleHeader
        eyebrow="SEGURIDAD Y ACCESOS"
        title="Usuarios"
        description="Administra cuentas, roles empresariales y estado de acceso."
        action={<><button className="secondary-button" onClick={() => void load(page)}><RefreshCw size={16} /> Actualizar</button><button className="primary-button" onClick={() => setShowCreate((value) => !value)}><Plus size={17} /> Nuevo usuario</button></>}
      />
      <div className="feedback feedback-success"><ShieldCheck size={17} /> La matriz de permisos por rol sigue pendiente de aprobación. Por ahora, los módulos operativos y la auditoría están restringidos a SystemAdministrator.</div>
      {error && <ErrorMessage>{error}</ErrorMessage>}
      {success && <SuccessMessage>{success}</SuccessMessage>}

      {showCreate && (
        <section className="content-card create-card">
          <div className="section-heading"><div><p className="eyebrow">ALTA DE CUENTA</p><h2>Datos del usuario</h2></div><button className="icon-button" aria-label="Cerrar formulario" onClick={() => setShowCreate(false)}><X size={18} /></button></div>
          <form className="form-grid" onSubmit={create}>
            <div><label htmlFor="user-name">Nombre completo</label><input id="user-name" required minLength={2} maxLength={120} value={displayName} onChange={(event) => setDisplayName(event.target.value)} placeholder="Nombre y apellido" /></div>
            <div><label htmlFor="user-email">Correo electrónico</label><input id="user-email" type="email" required maxLength={254} value={email} onChange={(event) => setEmail(event.target.value)} placeholder="persona@empresa.com" /></div>
            <div><label htmlFor="user-role">Rol empresarial</label><select id="user-role" value={role} onChange={(event) => setRole(event.target.value)}>{roles.map((item) => <option key={item.value} value={item.value}>{item.label}</option>)}</select></div>
            <div><label htmlFor="user-password">Contraseña temporal</label><input id="user-password" type="password" autoComplete="new-password" required minLength={12} maxLength={256} value={password} onChange={(event) => setPassword(event.target.value)} placeholder="12+ caracteres, mayúscula, número y símbolo" /></div>
            <div className="form-actions"><button className="primary-button" type="submit" disabled={saving}>{saving ? 'Creando…' : 'Crear usuario'}</button></div>
          </form>
          <p className="form-hint"><LockKeyhole size={13} /> Las contraseñas se guardan como hash. El sistema no envía invitaciones automáticamente: entrega la contraseña temporal por un canal seguro y solicita al usuario cambiarla en un siguiente incremento.</p>
        </section>
      )}

      <section className="content-card table-card">
        <div className="table-toolbar">
          <div><h2>Directorio de usuarios</h2><p>{data?.totalCount ?? 0} cuentas registradas</p></div>
          <label className="search-field"><Search size={16} /><input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Buscar por nombre, correo o rol" aria-label="Buscar usuarios" /></label>
        </div>
        {busy ? <LoadingState label="Consultando usuarios…" /> : rows.length === 0 ? <div className="empty-state"><UserCog size={25} /><strong>No hay usuarios que mostrar</strong><span>Registra una cuenta o prueba otra búsqueda.</span></div> :
          <div className="table-scroll"><table><thead><tr><th>USUARIO</th><th>ROL</th><th>ESTADO</th><th>FECHA DE ALTA</th><th>ACCESO</th></tr></thead><tbody>
            {rows.map((account) => <tr key={account.id}>
              <td><span className="table-avatar"><UserCog size={17} /></span><span className="main-cell"><strong>{account.displayName}</strong><small>{account.email}{account.id === currentUser?.id ? ' · Tú' : ''}</small></span></td>
              <td>{roles.find((item) => item.value === account.role)?.label ?? account.role}</td>
              <td><StatusBadge status={account.isActive ? 'Activo' : 'Inactivo'} /></td>
              <td>{new Date(account.createdAtUtc).toLocaleDateString('es-CO')}</td>
              <td><button className="table-action" disabled={account.id === currentUser?.id && account.isActive} onClick={() => void toggleStatus(account)}>{account.isActive ? <><UserRoundX size={14} /> Desactivar</> : <><CheckCircle2 size={14} /> Reactivar</>}</button></td>
            </tr>)}
          </tbody></table></div>}
        <div className="table-pagination"><span>Página {data?.page ?? page} de {Math.max(data?.totalPages ?? 1, 1)}</span><div><button className="secondary-button" disabled={page <= 1 || busy} onClick={() => setPage((value) => Math.max(1, value - 1))}>Anterior</button><button className="secondary-button" disabled={busy || page >= (data?.totalPages ?? 1)} onClick={() => setPage((value) => value + 1)}>Siguiente</button></div></div>
      </section>
    </>
  );
}
