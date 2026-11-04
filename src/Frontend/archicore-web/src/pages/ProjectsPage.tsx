import { Fragment, useEffect, useState, type FormEvent } from 'react';
import { FolderKanban, Plus, RefreshCw, Search, X } from 'lucide-react';
import { api, ApiError } from '../api/client';
import type { Contract, PagedResult, Project, Warehouse } from '../api/types';
import { ErrorMessage, LoadingState, SuccessMessage } from '../components/Feedback';
import { ModuleHeader } from '../components/ModuleHeader';
import { StatusBadge } from '../components/StatusBadge';
import { LifecycleActions, type LifecycleActionSpec } from '../components/LifecycleActions';

function toLocalDateTimeInputValue(value = new Date()): string {
  const local = new Date(value.getTime() - value.getTimezoneOffset() * 60_000);
  return local.toISOString().slice(0, 16);
}

const projectLifecycleActions: LifecycleActionSpec[] = [
  { fromStatus: 'Draft', endpoint: 'plan', label: 'Planificar', successStatus: 'Planificado' },
  { fromStatus: 'Planned', endpoint: 'start', label: 'Iniciar', successStatus: 'Activo' },
  { fromStatus: 'Active', endpoint: 'suspend', label: 'Suspender', successStatus: 'Suspendido', confirmationMessage: '¿Suspender este proyecto?' },
  { fromStatus: 'Suspended', endpoint: 'resume', label: 'Reanudar', successStatus: 'Activo' },
  { fromStatus: 'Active', endpoint: 'complete', label: 'Completar', successStatus: 'Completado', confirmationMessage: '¿Marcar este proyecto como completado?' },
  { fromStatus: 'Completed', endpoint: 'close', label: 'Cerrar', successStatus: 'Cerrado', confirmationMessage: '¿Cerrar este proyecto? No podrá continuar su operación.' },
  { fromStatus: 'Closed', endpoint: 'archive', label: 'Archivar', successStatus: 'Archivado', confirmationMessage: '¿Archivar este proyecto? Esta acción cambia su estado de forma permanente.' },
];

type ProjectsPageProps = { canOperate?: boolean };

export function ProjectsPage({ canOperate = true }: ProjectsPageProps) {
  const [data, setData] = useState<PagedResult<Project> | null>(null);
  const [contracts, setContracts] = useState<Contract[]>([]);
  const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
  const [busy, setBusy] = useState(true);
  const [saving, setSaving] = useState(false);
  const [showCreate, setShowCreate] = useState(false);
  const [search, setSearch] = useState('');
  const [projectCode, setProjectCode] = useState('');
  const [contractId, setContractId] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [transitioningId, setTransitioningId] = useState<string | null>(null);
  const [managingProjectId, setManagingProjectId] = useState<string | null>(null);
  const [assignmentWarehouseId, setAssignmentWarehouseId] = useState('');
  const [assignmentDateTime, setAssignmentDateTime] = useState(() => toLocalDateTimeInputValue());
  const [assignmentBusyProjectId, setAssignmentBusyProjectId] = useState<string | null>(null);

  async function load() {
    setBusy(true); setError('');
    try {
      const projects = await api.get<PagedResult<Project>>('/projects?page=1&pageSize=100');
      setData(projects);
      if (canOperate) {
        const [agreements, warehouseResult] = await Promise.all([
          api.get<PagedResult<Contract>>('/contracts?page=1&pageSize=100'),
          api.get<PagedResult<Warehouse>>('/warehouses?page=1&pageSize=100'),
        ]);
        setContracts(agreements.items);
        setWarehouses(warehouseResult.items);
        if (!contractId && agreements.items[0]) setContractId(agreements.items[0].id);
      } else {
        setContracts([]);
        setWarehouses([]);
      }
    } catch (caught) { setError(caught instanceof ApiError ? caught.message : 'No se pudieron consultar los proyectos.'); }
    finally { setBusy(false); }
  }
  useEffect(() => { void load(); }, [canOperate]);

  async function create(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); setSaving(true); setError(''); setSuccess('');
    try {
      await api.post<Project>('/projects', { projectCode: projectCode.trim(), contractId });
      setProjectCode(''); setShowCreate(false); setSuccess('Proyecto creado correctamente.');
      await load();
    } catch (caught) { setError(caught instanceof ApiError ? caught.message : 'No se pudo crear el proyecto.'); }
    finally { setSaving(false); }
  }

  async function transition(project: Project, action: LifecycleActionSpec) {
    setTransitioningId(project.id);
    setError('');
    setSuccess('');
    try {
      await api.post<Project>(`/projects/${project.id}/${action.endpoint}`);
      setSuccess(`Proyecto ${project.projectCode}: estado actualizado a ${action.successStatus.toLowerCase()}.`);
      await load();
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : 'No se pudo actualizar el estado del proyecto.');
    } finally {
      setTransitioningId(null);
    }
  }

  async function assignWarehouse(project: Project) {
    if (!assignmentWarehouseId || !assignmentDateTime) return;

    setAssignmentBusyProjectId(project.id);
    setError('');
    setSuccess('');
    try {
      await api.post<Project>(`/projects/${project.id}/warehouses`, {
        warehouseId: assignmentWarehouseId,
        assignedFrom: new Date(assignmentDateTime).toISOString(),
      });
      const assignedCode = warehouses.find((warehouse) => warehouse.id === assignmentWarehouseId)?.warehouseCode;
      setSuccess(`Bodega ${assignedCode ?? ''} asignada al proyecto ${project.projectCode}.`.trim());
      setAssignmentWarehouseId('');
      await load();
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : 'No se pudo asignar la bodega al proyecto.');
    } finally {
      setAssignmentBusyProjectId(null);
    }
  }

  async function releaseWarehouse(project: Project, warehouseId: string, assignedFrom: string) {
    const warehouseCode = warehouses.find((warehouse) => warehouse.id === warehouseId)?.warehouseCode ?? warehouseId;
    if (!window.confirm(`¿Liberar la bodega ${warehouseCode} del proyecto ${project.projectCode}? Se conservará el historial de la asignación.`)) return;

    setAssignmentBusyProjectId(project.id);
    setError('');
    setSuccess('');
    try {
      const earliestRelease = new Date(assignedFrom).getTime() + 1_000;
      const releasedOn = new Date(Math.max(Date.now(), earliestRelease)).toISOString();
      await api.post<Project>(`/projects/${project.id}/warehouses/release`, {
        warehouseId,
        releasedOn,
      });
      setSuccess(`La bodega ${warehouseCode} fue liberada del proyecto ${project.projectCode}.`);
      await load();
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : 'No se pudo liberar la bodega del proyecto.');
    } finally {
      setAssignmentBusyProjectId(null);
    }
  }

  const rows = (data?.items ?? []).filter((p) => !search || p.projectCode.toLowerCase().includes(search.toLowerCase()));
  return (
    <>
      <ModuleHeader eyebrow="EJECUCIÓN OPERATIVA" title="Proyectos" description="Unidades de trabajo vinculadas a los contratos de servicio." action={<><button className="secondary-button" onClick={() => void load()}><RefreshCw size={16} /> Actualizar</button>{canOperate && <button className="primary-button" onClick={() => setShowCreate((v) => !v)}><Plus size={17} /> Nuevo proyecto</button>}</>} />
      {error && <ErrorMessage>{error}</ErrorMessage>}{success && <SuccessMessage>{success}</SuccessMessage>}
      {canOperate && showCreate && <section className="content-card create-card"><div className="section-heading"><div><p className="eyebrow">ALTA DE PROYECTO</p><h2>Datos de ejecución</h2></div><button className="icon-button" aria-label="Cerrar formulario" onClick={() => setShowCreate(false)}><X size={18} /></button></div>
        {contracts.length === 0 ? <ErrorMessage>Registra primero un contrato para poder asociar un proyecto.</ErrorMessage> :
        <form className="inline-form" onSubmit={create}><div className="field-grow"><label htmlFor="project-code">Código de proyecto</label><input id="project-code" required value={projectCode} onChange={(e) => setProjectCode(e.target.value)} placeholder="PRJ-2026-000001" /></div><div className="field-grow"><label htmlFor="project-contract">Contrato</label><select id="project-contract" required value={contractId} onChange={(e) => setContractId(e.target.value)}>{contracts.map((c) => <option key={c.id} value={c.id}>{c.contractNumber}</option>)}</select></div><button className="primary-button" type="submit" disabled={saving || !contractId}>{saving ? 'Guardando…' : 'Guardar proyecto'}</button></form>}
      </section>}
      <section className="content-card table-card"><div className="table-toolbar"><div><h2>Proyectos registrados</h2><p>{data?.totalCount ?? 0} registros en el sistema</p></div><label className="search-field"><Search size={16} /><input value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Buscar código" aria-label="Buscar proyecto" /></label></div>
        {busy ? <LoadingState label="Consultando proyectos…" /> : rows.length === 0 ? <div className="empty-state"><FolderKanban size={25} /><strong>No hay proyectos que mostrar</strong><span>Crea un proyecto vinculado a un contrato.</span></div> :
        <div className="table-scroll"><table><thead><tr><th>PROYECTO</th><th>CONTRATO</th><th>ESTADO</th><th>BODEGAS ACTIVAS</th><th>INICIO</th>{canOperate && <th>ACCIONES</th>}</tr></thead><tbody>{rows.map((p) => (
          <Fragment key={p.id}>
            <tr>
              <td><span className="table-avatar table-avatar-purple"><FolderKanban size={17} /></span><span className="main-cell"><strong>{p.projectCode}</strong><small>ID {p.id.slice(0, 8).toUpperCase()}</small></span></td>
              <td>{contracts.find((c) => c.id === p.contractId)?.contractNumber ?? p.contractId.slice(0, 8)}</td>
              <td><StatusBadge status={p.status} /></td>
              <td><span className="count-chip">{p.warehouseAssignments.filter((assignment) => assignment.isActive).length}</span></td>
              <td>{p.startDate ? new Date(p.startDate).toLocaleDateString('es-CO') : '—'}</td>
              {canOperate && <td>
                <div className="project-row-actions">
                  <LifecycleActions status={p.status} actions={projectLifecycleActions} busy={transitioningId === p.id || assignmentBusyProjectId === p.id} onAction={(action) => void transition(p, action)} />
                  <button type="button" className="table-action" onClick={() => setManagingProjectId((current) => current === p.id ? null : p.id)} aria-expanded={managingProjectId === p.id}>
                    {managingProjectId === p.id ? 'Cerrar bodegas' : 'Gestionar bodegas'}
                  </button>
                </div>
              </td>}
            </tr>
            {canOperate && managingProjectId === p.id && (
              <tr className="project-assignment-row">
                <td colSpan={6}>
                  <section className="assignment-manager">
                    <div className="assignment-manager-heading">
                      <div><p className="eyebrow">ASIGNACIÓN DE ESPACIO</p><h3>Bodegas del proyecto {p.projectCode}</h3></div>
                      <span className="soft-pill">{p.warehouseAssignments.filter((assignment) => assignment.isActive).length} activas</span>
                    </div>
                    {p.warehouseAssignments.length === 0 ? (
                      <p className="assignment-empty">Este proyecto aún no tiene historial de asignaciones de bodega.</p>
                    ) : (
                      <div className="assignment-list">
                        {p.warehouseAssignments.slice().sort((left, right) => right.assignedFrom.localeCompare(left.assignedFrom)).map((assignment) => {
                          const warehouse = warehouses.find((item) => item.id === assignment.warehouseId);
                          return (
                            <div className="assignment-item" key={`${assignment.warehouseId}-${assignment.assignedFrom}`}>
                              <div className="assignment-item-title">
                                <strong>{warehouse?.warehouseCode ?? `Bodega ${assignment.warehouseId.slice(0, 8)}`}</strong>
                                <StatusBadge status={assignment.isActive ? 'Asignada' : 'Liberada'} />
                              </div>
                              <p>Asignada desde {new Date(assignment.assignedFrom).toLocaleString('es-CO')}{assignment.releasedOn ? ` · Liberada ${new Date(assignment.releasedOn).toLocaleString('es-CO')}` : ''}</p>
                              {assignment.isActive && (
                                <button type="button" className="table-action assignment-release" disabled={assignmentBusyProjectId === p.id} onClick={() => void releaseWarehouse(p, assignment.warehouseId, assignment.assignedFrom)}>
                                  {assignmentBusyProjectId === p.id ? 'Guardando…' : 'Liberar bodega'}
                                </button>
                              )}
                            </div>
                          );
                        })}
                      </div>
                    )}
                    <form className="assignment-form" onSubmit={(event) => { event.preventDefault(); void assignWarehouse(p); }}>
                      <div className="assignment-form-field">
                        <label htmlFor={`warehouse-${p.id}`}>Bodega disponible</label>
                        <select id={`warehouse-${p.id}`} required value={assignmentWarehouseId} onChange={(event) => setAssignmentWarehouseId(event.target.value)}>
                          <option value="">Selecciona una bodega</option>
                          {warehouses.filter((warehouse) => !p.warehouseAssignments.some((assignment) => assignment.isActive && assignment.warehouseId === warehouse.id)).map((warehouse) => (
                            <option key={warehouse.id} value={warehouse.id}>{warehouse.warehouseCode} · {warehouse.status}</option>
                          ))}
                        </select>
                      </div>
                      <div className="assignment-form-field">
                        <label htmlFor={`assigned-from-${p.id}`}>Fecha y hora de asignación</label>
                        <input id={`assigned-from-${p.id}`} type="datetime-local" required value={assignmentDateTime} onChange={(event) => setAssignmentDateTime(event.target.value)} />
                      </div>
                      <div className="assignment-form-action">
                        <button className="primary-button" type="submit" disabled={assignmentBusyProjectId === p.id || !assignmentWarehouseId || warehouses.length === 0}>
                          {assignmentBusyProjectId === p.id ? 'Guardando…' : 'Asignar bodega'}
                        </button>
                      </div>
                    </form>
                  </section>
                </td>
              </tr>
            )}
          </Fragment>
        ))}</tbody></table></div>}
      </section>
    </>
  );
}
