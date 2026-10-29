import { useEffect, useState, type FormEvent } from 'react';
import { FolderKanban, Plus, RefreshCw, Search, X } from 'lucide-react';
import { api, ApiError } from '../api/client';
import type { Contract, PagedResult, Project } from '../api/types';
import { ErrorMessage, LoadingState, SuccessMessage } from '../components/Feedback';
import { ModuleHeader } from '../components/ModuleHeader';
import { StatusBadge } from '../components/StatusBadge';

export function ProjectsPage() {
  const [data, setData] = useState<PagedResult<Project> | null>(null);
  const [contracts, setContracts] = useState<Contract[]>([]);
  const [busy, setBusy] = useState(true);
  const [saving, setSaving] = useState(false);
  const [showCreate, setShowCreate] = useState(false);
  const [search, setSearch] = useState('');
  const [projectCode, setProjectCode] = useState('');
  const [contractId, setContractId] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  async function load() {
    setBusy(true); setError('');
    try {
      const [projects, agreements] = await Promise.all([
        api.get<PagedResult<Project>>('/projects?page=1&pageSize=100'),
        api.get<PagedResult<Contract>>('/contracts?page=1&pageSize=100'),
      ]);
      setData(projects); setContracts(agreements.items);
      if (!contractId && agreements.items[0]) setContractId(agreements.items[0].id);
    } catch (caught) { setError(caught instanceof ApiError ? caught.message : 'No se pudieron consultar los proyectos.'); }
    finally { setBusy(false); }
  }
  useEffect(() => { void load(); }, []);

  async function create(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); setSaving(true); setError(''); setSuccess('');
    try {
      await api.post<Project>('/projects', { projectCode: projectCode.trim(), contractId });
      setProjectCode(''); setShowCreate(false); setSuccess('Proyecto creado correctamente.');
      await load();
    } catch (caught) { setError(caught instanceof ApiError ? caught.message : 'No se pudo crear el proyecto.'); }
    finally { setSaving(false); }
  }

  const rows = (data?.items ?? []).filter((p) => !search || p.projectCode.toLowerCase().includes(search.toLowerCase()));
  return (
    <>
      <ModuleHeader eyebrow="EJECUCIÓN OPERATIVA" title="Proyectos" description="Unidades de trabajo vinculadas a los contratos de servicio." action={<><button className="secondary-button" onClick={() => void load()}><RefreshCw size={16} /> Actualizar</button><button className="primary-button" onClick={() => setShowCreate((v) => !v)}><Plus size={17} /> Nuevo proyecto</button></>} />
      {error && <ErrorMessage>{error}</ErrorMessage>}{success && <SuccessMessage>{success}</SuccessMessage>}
      {showCreate && <section className="content-card create-card"><div className="section-heading"><div><p className="eyebrow">ALTA DE PROYECTO</p><h2>Datos de ejecución</h2></div><button className="icon-button" aria-label="Cerrar formulario" onClick={() => setShowCreate(false)}><X size={18} /></button></div>
        {contracts.length === 0 ? <ErrorMessage>Registra primero un contrato para poder asociar un proyecto.</ErrorMessage> :
        <form className="inline-form" onSubmit={create}><div className="field-grow"><label htmlFor="project-code">Código de proyecto</label><input id="project-code" required value={projectCode} onChange={(e) => setProjectCode(e.target.value)} placeholder="PRJ-2026-000001" /></div><div className="field-grow"><label htmlFor="project-contract">Contrato</label><select id="project-contract" required value={contractId} onChange={(e) => setContractId(e.target.value)}>{contracts.map((c) => <option key={c.id} value={c.id}>{c.contractNumber}</option>)}</select></div><button className="primary-button" type="submit" disabled={saving || !contractId}>{saving ? 'Guardando…' : 'Guardar proyecto'}</button></form>}
      </section>}
      <section className="content-card table-card"><div className="table-toolbar"><div><h2>Proyectos registrados</h2><p>{data?.totalCount ?? 0} registros en el sistema</p></div><label className="search-field"><Search size={16} /><input value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Buscar código" aria-label="Buscar proyecto" /></label></div>
        {busy ? <LoadingState label="Consultando proyectos…" /> : rows.length === 0 ? <div className="empty-state"><FolderKanban size={25} /><strong>No hay proyectos que mostrar</strong><span>Crea un proyecto vinculado a un contrato.</span></div> :
        <div className="table-scroll"><table><thead><tr><th>PROYECTO</th><th>CONTRATO</th><th>ESTADO</th><th>BODEGAS ASIGNADAS</th><th>INICIO</th></tr></thead><tbody>{rows.map((p) => <tr key={p.id}><td><span className="table-avatar table-avatar-purple"><FolderKanban size={17} /></span><span className="main-cell"><strong>{p.projectCode}</strong><small>ID {p.id.slice(0, 8).toUpperCase()}</small></span></td><td>{contracts.find((c) => c.id === p.contractId)?.contractNumber ?? p.contractId.slice(0, 8)}</td><td><StatusBadge status={p.status} /></td><td><span className="count-chip">{p.warehouseAssignments.length}</span></td><td>{p.startDate ? new Date(p.startDate).toLocaleDateString('es-CO') : '—'}</td></tr>)}</tbody></table></div>}
      </section>
    </>
  );
}
