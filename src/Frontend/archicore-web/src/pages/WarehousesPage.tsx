import { useEffect, useState, type FormEvent } from 'react';
import { Plus, Search, RefreshCw, Warehouse as WarehouseIcon, X } from 'lucide-react';
import { api, ApiError } from '../api/client';
import type { PagedResult, Warehouse } from '../api/types';
import { ErrorMessage, LoadingState, SuccessMessage } from '../components/Feedback';
import { ModuleHeader } from '../components/ModuleHeader';
import { StatusBadge } from '../components/StatusBadge';

export function WarehousesPage() {
  const [data, setData] = useState<PagedResult<Warehouse> | null>(null);
  const [busy, setBusy] = useState(true);
  const [saving, setSaving] = useState(false);
  const [showCreate, setShowCreate] = useState(false);
  const [code, setCode] = useState('');
  const [search, setSearch] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  async function load() {
    setBusy(true); setError('');
    try { setData(await api.get<PagedResult<Warehouse>>('/warehouses?page=1&pageSize=100')); }
    catch (caught) { setError(caught instanceof ApiError ? caught.message : 'No se pudieron consultar las bodegas.'); }
    finally { setBusy(false); }
  }
  useEffect(() => { void load(); }, []);

  async function create(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); setSaving(true); setError(''); setSuccess('');
    try {
      await api.post<Warehouse>('/warehouses', { warehouseCode: code.trim() });
      setCode(''); setShowCreate(false); setSuccess('Bodega creada correctamente.');
      await load();
    } catch (caught) { setError(caught instanceof ApiError ? caught.message : 'No se pudo crear la bodega.'); }
    finally { setSaving(false); }
  }

  const rows = (data?.items ?? []).filter((w) => !search || w.warehouseCode.toLowerCase().includes(search.toLowerCase()));

  return (
    <>
      <ModuleHeader eyebrow="INFRAESTRUCTURA" title="Bodegas" description="Espacios de custodia asociados a la operación de los proyectos." action={<><button className="secondary-button" onClick={() => void load()}><RefreshCw size={16} /> Actualizar</button><button className="primary-button" onClick={() => setShowCreate((v) => !v)}><Plus size={17} /> Nueva bodega</button></>} />
      {error && <ErrorMessage>{error}</ErrorMessage>}
      {success && <SuccessMessage>{success}</SuccessMessage>}
      {showCreate && <section className="content-card create-card"><div className="section-heading"><div><p className="eyebrow">ALTA DE BODEGA</p><h2>Identificador de la bodega</h2></div><button className="icon-button" aria-label="Cerrar formulario" onClick={() => setShowCreate(false)}><X size={18} /></button></div><form className="inline-form" onSubmit={create}><div className="field-grow"><label htmlFor="warehouse-code">Código</label><input id="warehouse-code" required value={code} onChange={(e) => setCode(e.target.value)} placeholder="Ej. WH-000123" autoFocus /></div><button className="primary-button" type="submit" disabled={saving}>{saving ? 'Guardando…' : 'Guardar bodega'}</button></form><p className="form-hint">Los pasillos, estantes y posiciones físicas requieren completar el modelo WarehouseLocation.</p></section>}
      <section className="content-card table-card">
        <div className="table-toolbar"><div><h2>Inventario de bodegas</h2><p>{data?.totalCount ?? 0} registros en el sistema</p></div><label className="search-field"><Search size={16} /><input value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Buscar por código" aria-label="Buscar por código" /></label></div>
        {busy ? <LoadingState label="Consultando bodegas…" /> : rows.length === 0 ? <div className="empty-state"><WarehouseIcon size={25} /><strong>No hay bodegas que mostrar</strong><span>Prueba otra búsqueda o registra una bodega.</span></div> :
          <div className="table-scroll"><table><thead><tr><th>CÓDIGO</th><th>ESTADO</th><th>PROYECTOS</th><th>FECHA DE REGISTRO</th><th></th></tr></thead><tbody>
            {rows.map((warehouse) => <tr key={warehouse.id}><td><span className="table-avatar table-avatar-green"><WarehouseIcon size={17} /></span><span className="main-cell"><strong>{warehouse.warehouseCode}</strong><small>ID {warehouse.id.slice(0, 8).toUpperCase()}</small></span></td><td><StatusBadge status={warehouse.status} /></td><td><span className="count-chip">Con asignación por proyecto</span></td><td>{new Date(warehouse.createdAt).toLocaleDateString('es-CO')}</td><td><button className="table-action" title="Buscar bodega" onClick={() => setSearch(warehouse.warehouseCode)}>Ver ficha</button></td></tr>)}
          </tbody></table></div>}
      </section>
    </>
  );
}
