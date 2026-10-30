import { useEffect, useState, type FormEvent } from 'react';
import { Plus, Search, RefreshCw, UserRound, X } from 'lucide-react';
import { api, ApiError } from '../api/client';
import type { Customer, PagedResult } from '../api/types';
import { ErrorMessage, LoadingState, SuccessMessage } from '../components/Feedback';
import { ModuleHeader } from '../components/ModuleHeader';
import { StatusBadge } from '../components/StatusBadge';
import { LifecycleActions, type LifecycleActionSpec } from '../components/LifecycleActions';

const customerLifecycleActions: LifecycleActionSpec[] = [
  { fromStatus: 'Prospect', endpoint: 'activate', label: 'Activar', successStatus: 'Activo' },
  { fromStatus: 'Active', endpoint: 'deactivate', label: 'Desactivar', successStatus: 'Inactivo', confirmationMessage: '¿Desactivar este cliente? Dejará de estar activo para la operación.' },
  { fromStatus: 'Inactive', endpoint: 'archive', label: 'Archivar', successStatus: 'Archivado', confirmationMessage: '¿Archivar este cliente? Esta acción cambia su estado de forma permanente.' },
];

export function CustomersPage() {
  const [data, setData] = useState<PagedResult<Customer> | null>(null);
  const [busy, setBusy] = useState(true);
  const [saving, setSaving] = useState(false);
  const [showCreate, setShowCreate] = useState(false);
  const [nit, setNit] = useState('');
  const [search, setSearch] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [transitioningId, setTransitioningId] = useState<string | null>(null);

  async function load() {
    setBusy(true); setError('');
    try { setData(await api.get<PagedResult<Customer>>('/customers?page=1&pageSize=100')); }
    catch (caught) { setError(caught instanceof ApiError ? caught.message : 'No se pudieron consultar los clientes.'); }
    finally { setBusy(false); }
  }
  useEffect(() => { void load(); }, []);

  async function create(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); setSaving(true); setError(''); setSuccess('');
    try {
      await api.post<Customer>('/customers', { nit: nit.trim() });
      setNit(''); setShowCreate(false); setSuccess('Cliente creado correctamente.');
      await load();
    } catch (caught) { setError(caught instanceof ApiError ? caught.message : 'No se pudo crear el cliente.'); }
    finally { setSaving(false); }
  }

  async function transition(customer: Customer, action: LifecycleActionSpec) {
    setTransitioningId(customer.id);
    setError('');
    setSuccess('');
    try {
      await api.post<Customer>(`/customers/${customer.id}/${action.endpoint}`);
      setSuccess(`Cliente ${customer.nit}: estado actualizado a ${action.successStatus.toLowerCase()}.`);
      await load();
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : 'No se pudo actualizar el estado del cliente.');
    } finally {
      setTransitioningId(null);
    }
  }

  const rows = (data?.items ?? []).filter((c) => !search || c.nit.toLowerCase().includes(search.toLowerCase()));

  return (
    <>
      <ModuleHeader eyebrow="DIRECTORIO COMERCIAL" title="Clientes" description="Organizaciones contratantes registradas en ArchiCore." action={<><button className="secondary-button" onClick={() => void load()}><RefreshCw size={16} /> Actualizar</button><button className="primary-button" onClick={() => setShowCreate((v) => !v)}><Plus size={17} /> Nuevo cliente</button></>} />
      {error && <ErrorMessage>{error}</ErrorMessage>}
      {success && <SuccessMessage>{success}</SuccessMessage>}
      {showCreate && <section className="content-card create-card"><div className="section-heading"><div><p className="eyebrow">ALTA DE CLIENTE</p><h2>Identificación tributaria</h2></div><button className="icon-button" aria-label="Cerrar formulario" onClick={() => setShowCreate(false)}><X size={18} /></button></div><form className="inline-form" onSubmit={create}><div className="field-grow"><label htmlFor="nit">NIT</label><input id="nit" required value={nit} onChange={(e) => setNit(e.target.value)} placeholder="Ej. 900123456-7" autoFocus /></div><button className="primary-button" type="submit" disabled={saving}>{saving ? 'Guardando…' : 'Guardar cliente'}</button></form><p className="form-hint">Por ahora el modelo de dominio requiere NIT. Los campos de perfil y contacto dependen de la especificación pendiente.</p></section>}
      <section className="content-card table-card">
        <div className="table-toolbar"><div><h2>Directorio de clientes</h2><p>{data?.totalCount ?? 0} registros en el sistema</p></div><label className="search-field"><Search size={16} /><input value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Buscar por NIT" aria-label="Buscar por NIT" /></label></div>
        {busy ? <LoadingState label="Consultando clientes…" /> : rows.length === 0 ? <div className="empty-state"><UserRound size={25} /><strong>No hay clientes que mostrar</strong><span>Prueba otra búsqueda o registra un nuevo cliente.</span></div> :
          <div className="table-scroll"><table><thead><tr><th>CLIENTE / NIT</th><th>ESTADO</th><th>CONTRATOS</th><th>FECHA DE REGISTRO</th><th>ACCIONES</th></tr></thead><tbody>
            {rows.map((customer) => <tr key={customer.id}><td><span className="table-avatar"><UserRound size={17} /></span><span className="main-cell"><strong>{customer.nit}</strong><small>ID {customer.id.slice(0, 8).toUpperCase()}</small></span></td><td><StatusBadge status={customer.status} /></td><td><span className="count-chip">{customer.contracts.length}</span></td><td>{new Date(customer.createdAt).toLocaleDateString('es-CO')}</td><td><LifecycleActions status={customer.status} actions={customerLifecycleActions} busy={transitioningId === customer.id} onAction={(action) => void transition(customer, action)} /></td></tr>)}
          </tbody></table></div>}
      </section>
    </>
  );
}
