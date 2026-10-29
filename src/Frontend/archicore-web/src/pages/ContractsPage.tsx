import { useEffect, useState, type FormEvent } from 'react';
import { FileCheck2, Plus, RefreshCw, Search, X } from 'lucide-react';
import { api, ApiError } from '../api/client';
import type { Contract, Customer, PagedResult } from '../api/types';
import { ErrorMessage, LoadingState, SuccessMessage } from '../components/Feedback';
import { ModuleHeader } from '../components/ModuleHeader';
import { StatusBadge } from '../components/StatusBadge';
import { LifecycleActions, type LifecycleActionSpec } from '../components/LifecycleActions';

const contractLifecycleActions: LifecycleActionSpec[] = [
  { fromStatus: 'Draft', endpoint: 'activate', label: 'Activar', successStatus: 'Activo' },
  { fromStatus: 'Active', endpoint: 'suspend', label: 'Suspender', successStatus: 'Suspendido', confirmationMessage: '¿Suspender este contrato?' },
  { fromStatus: 'Suspended', endpoint: 'activate', label: 'Reactivar', successStatus: 'Activo' },
];

export function ContractsPage() {
  const [data, setData] = useState<PagedResult<Contract> | null>(null);
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [busy, setBusy] = useState(true);
  const [saving, setSaving] = useState(false);
  const [showCreate, setShowCreate] = useState(false);
  const [search, setSearch] = useState('');
  const [contractNumber, setContractNumber] = useState('');
  const [clientId, setClientId] = useState('');
  const [startDate, setStartDate] = useState(new Date().toISOString().slice(0, 10));
  const [totalValue, setTotalValue] = useState('0');
  const [currency, setCurrency] = useState('COP');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [transitioningId, setTransitioningId] = useState<string | null>(null);

  async function load() {
    setBusy(true); setError('');
    try {
      const [contracts, clients] = await Promise.all([
        api.get<PagedResult<Contract>>('/contracts?page=1&pageSize=100'),
        api.get<PagedResult<Customer>>('/customers?page=1&pageSize=100'),
      ]);
      setData(contracts); setCustomers(clients.items);
      if (!clientId && clients.items[0]) setClientId(clients.items[0].id);
    } catch (caught) { setError(caught instanceof ApiError ? caught.message : 'No se pudieron consultar los contratos.'); }
    finally { setBusy(false); }
  }
  useEffect(() => { void load(); }, []);

  async function create(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); setSaving(true); setError(''); setSuccess('');
    try {
      await api.post<Contract>('/contracts', {
        contractNumber: contractNumber.trim(),
        clientId,
        startDate: new Date(`${startDate}T00:00:00-05:00`).toISOString(),
        totalValue: Number(totalValue),
        currency: currency.trim().toUpperCase(),
      });
      setContractNumber(''); setTotalValue('0'); setShowCreate(false); setSuccess('Contrato creado correctamente.');
      await load();
    } catch (caught) { setError(caught instanceof ApiError ? caught.message : 'No se pudo crear el contrato.'); }
    finally { setSaving(false); }
  }

  async function transition(contract: Contract, action: LifecycleActionSpec) {
    setTransitioningId(contract.id);
    setError('');
    setSuccess('');
    try {
      await api.post<Contract>(`/contracts/${contract.id}/${action.endpoint}`);
      setSuccess(`Contrato ${contract.contractNumber}: estado actualizado a ${action.successStatus.toLowerCase()}.`);
      await load();
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : 'No se pudo actualizar el estado del contrato.');
    } finally {
      setTransitioningId(null);
    }
  }

  const rows = (data?.items ?? []).filter((c) => !search || c.contractNumber.toLowerCase().includes(search.toLowerCase()));
  return (
    <>
      <ModuleHeader eyebrow="RELACIÓN COMERCIAL" title="Contratos" description="Acuerdos que organizan los servicios y la ejecución documental." action={<><button className="secondary-button" onClick={() => void load()}><RefreshCw size={16} /> Actualizar</button><button className="primary-button" onClick={() => setShowCreate((v) => !v)}><Plus size={17} /> Nuevo contrato</button></>} />
      {error && <ErrorMessage>{error}</ErrorMessage>}{success && <SuccessMessage>{success}</SuccessMessage>}
      {showCreate && <section className="content-card create-card"><div className="section-heading"><div><p className="eyebrow">ALTA DE CONTRATO</p><h2>Datos comerciales</h2></div><button className="icon-button" aria-label="Cerrar formulario" onClick={() => setShowCreate(false)}><X size={18} /></button></div>
        {customers.length === 0 ? <ErrorMessage>Registra primero un cliente para poder asociar un contrato.</ErrorMessage> :
        <form className="form-grid" onSubmit={create}>
          <div><label htmlFor="contract-number">Número de contrato</label><input id="contract-number" required value={contractNumber} onChange={(e) => setContractNumber(e.target.value)} placeholder="CTR-2026-000001" /></div>
          <div><label htmlFor="contract-client">Cliente</label><select id="contract-client" required value={clientId} onChange={(e) => setClientId(e.target.value)}>{customers.map((c) => <option key={c.id} value={c.id}>{c.nit}</option>)}</select></div>
          <div><label htmlFor="contract-date">Fecha de inicio</label><input id="contract-date" type="date" required value={startDate} onChange={(e) => setStartDate(e.target.value)} /></div>
          <div><label htmlFor="contract-value">Valor total</label><input id="contract-value" type="number" min="0" step="0.01" required value={totalValue} onChange={(e) => setTotalValue(e.target.value)} /></div>
          <div><label htmlFor="contract-currency">Moneda</label><select id="contract-currency" value={currency} onChange={(e) => setCurrency(e.target.value)}><option value="COP">COP · Peso colombiano</option><option value="USD">USD · Dólar</option></select></div>
          <div className="form-actions"><button className="primary-button" type="submit" disabled={saving || !clientId}>{saving ? 'Guardando…' : 'Guardar contrato'}</button></div>
        </form>}
      </section>}
      <section className="content-card table-card"><div className="table-toolbar"><div><h2>Contratos registrados</h2><p>{data?.totalCount ?? 0} registros en el sistema</p></div><label className="search-field"><Search size={16} /><input value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Buscar número" aria-label="Buscar contrato" /></label></div>
        {busy ? <LoadingState label="Consultando contratos…" /> : rows.length === 0 ? <div className="empty-state"><FileCheck2 size={25} /><strong>No hay contratos que mostrar</strong><span>Registra un contrato asociado a un cliente.</span></div> :
        <div className="table-scroll"><table><thead><tr><th>CONTRATO</th><th>CLIENTE</th><th>VALOR</th><th>ESTADO</th><th>INICIO</th><th>ACCIONES</th></tr></thead><tbody>{rows.map((c) => <tr key={c.id}><td><span className="table-avatar table-avatar-blue"><FileCheck2 size={17} /></span><span className="main-cell"><strong>{c.contractNumber}</strong><small>ID {c.id.slice(0, 8).toUpperCase()}</small></span></td><td>{customers.find((cl) => cl.id === c.clientId)?.nit ?? c.clientId.slice(0, 8)}</td><td><strong>{new Intl.NumberFormat('es-CO', { style: 'currency', currency: c.currency, maximumFractionDigits: 0 }).format(c.totalValue)}</strong></td><td><StatusBadge status={c.status} /></td><td>{new Date(c.startDate).toLocaleDateString('es-CO')}</td><td><LifecycleActions status={c.status} actions={contractLifecycleActions} busy={transitioningId === c.id} onAction={(action) => void transition(c, action)} /></td></tr>)}</tbody></table></div>}
      </section>
    </>
  );
}
