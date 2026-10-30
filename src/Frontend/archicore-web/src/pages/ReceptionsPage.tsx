import { useEffect, useState, type FormEvent } from 'react';
import { CheckCheck, ClipboardList, Plus, RefreshCw, ShieldAlert, X } from 'lucide-react';
import { api, ApiError } from '../api/client';
import type { Contract, DocumentaryReception, PagedResult } from '../api/types';
import { ErrorMessage, LoadingState, SuccessMessage } from '../components/Feedback';
import { ModuleHeader } from '../components/ModuleHeader';
import { StatusBadge } from '../components/StatusBadge';

type ReceptionsPageProps = { role: string };
type ReceptionAction = 'inspection' | 'counting' | 'suspend' | null;

function formatDate(value: string): string {
  return new Date(value).toLocaleString('es-CO');
}

function shortId(value: string): string {
  return value.slice(0, 8).toUpperCase();
}

export function ReceptionsPage({ role }: ReceptionsPageProps) {
  const canReceive = role === 'SystemAdministrator' || role === 'ProjectCoordinator';
  const canInspectOrCount = canReceive || role === 'ArchiveAssistant';
  const canApprove = canReceive;

  const [data, setData] = useState<PagedResult<DocumentaryReception> | null>(null);
  const [activeContracts, setActiveContracts] = useState<Contract[]>([]);
  const [busy, setBusy] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [showCreate, setShowCreate] = useState(false);
  const [contractId, setContractId] = useState('');
  const [initialObservations, setInitialObservations] = useState('');
  const [activeReceptionId, setActiveReceptionId] = useState<string | null>(null);
  const [activeAction, setActiveAction] = useState<ReceptionAction>(null);
  const [safeToAccept, setSafeToAccept] = useState('true');
  const [inspectionObservations, setInspectionObservations] = useState('');
  const [declaredBoxes, setDeclaredBoxes] = useState('0');
  const [declaredFolders, setDeclaredFolders] = useState('0');
  const [countedBoxes, setCountedBoxes] = useState('0');
  const [countedFolders, setCountedFolders] = useState('0');
  const [discrepancyReport, setDiscrepancyReport] = useState('');
  const [suspensionReason, setSuspensionReason] = useState('');
  const [workingId, setWorkingId] = useState<string | null>(null);

  async function load() {
    setBusy(true);
    setError('');
    try {
      const receptionRequest = api.get<PagedResult<DocumentaryReception>>('/receptions?page=1&pageSize=100');
      const contractRequest = canReceive
        ? api.get<PagedResult<Contract>>('/contracts?page=1&pageSize=100')
        : Promise.resolve(null);
      const [receptions, contracts] = await Promise.all([receptionRequest, contractRequest]);
      setData(receptions);
      if (contracts) {
        const active = contracts.items.filter((item) => item.status === 'Active');
        setActiveContracts(active);
        if (!contractId && active[0]) setContractId(active[0].id);
      } else {
        setActiveContracts([]);
      }
    } catch (caught) {
      setError(caught instanceof ApiError
        ? caught.message
        : 'No se pudo consultar el registro de recepciones.');
    } finally {
      setBusy(false);
    }
  }

  useEffect(() => { void load(); }, [role]);

  function closeAction() {
    setActiveReceptionId(null);
    setActiveAction(null);
    setInspectionObservations('');
    setDiscrepancyReport('');
    setSuspensionReason('');
  }

  async function createReception(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSaving(true);
    setError('');
    setSuccess('');
    try {
      await api.post<DocumentaryReception>('/receptions', {
        contractId,
        initialObservations: initialObservations.trim() || null,
      });
      setInitialObservations('');
      setShowCreate(false);
      setSuccess('Recepción registrada. El siguiente paso es completar la inspección física.');
      await load();
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : 'No se pudo registrar la recepción.');
    } finally {
      setSaving(false);
    }
  }

  async function submitInspection(event: FormEvent<HTMLFormElement>, reception: DocumentaryReception) {
    event.preventDefault();
    setWorkingId(reception.id);
    setError('');
    setSuccess('');
    try {
      await api.post<DocumentaryReception>(`/receptions/${reception.id}/inspection`, {
        safeToAccept: safeToAccept === 'true',
        observations: inspectionObservations.trim() || null,
      });
      setSuccess(safeToAccept === 'true'
        ? 'Inspección completada. La recepción está lista para conteo.'
        : 'Recepción suspendida por la novedad física registrada.');
      closeAction();
      await load();
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : 'No se pudo registrar la inspección.');
    } finally {
      setWorkingId(null);
    }
  }

  async function submitCounting(event: FormEvent<HTMLFormElement>, reception: DocumentaryReception) {
    event.preventDefault();
    setWorkingId(reception.id);
    setError('');
    setSuccess('');
    try {
      await api.post<DocumentaryReception>(`/receptions/${reception.id}/counting`, {
        declaredBoxCount: Number(declaredBoxes),
        declaredFolderCount: Number(declaredFolders),
        countedBoxCount: Number(countedBoxes),
        countedFolderCount: Number(countedFolders),
        discrepancyReport: discrepancyReport.trim() || null,
      });
      setSuccess('Conteo registrado. Coordinación debe revisar y aprobar el resultado.');
      closeAction();
      await load();
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : 'No se pudo registrar el conteo.');
    } finally {
      setWorkingId(null);
    }
  }

  async function approveCounting(reception: DocumentaryReception) {
    if (!window.confirm('¿Confirmas la aprobación del conteo y el paso a verificación documental?')) return;
    setWorkingId(reception.id);
    setError('');
    setSuccess('');
    try {
      await api.post<DocumentaryReception>(`/receptions/${reception.id}/counting/approve`);
      setSuccess('Conteo aprobado. La recepción quedó lista para verificación y clasificación.');
      await load();
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : 'No se pudo aprobar el conteo.');
    } finally {
      setWorkingId(null);
    }
  }

  async function submitSuspension(event: FormEvent<HTMLFormElement>, reception: DocumentaryReception) {
    event.preventDefault();
    setWorkingId(reception.id);
    setError('');
    setSuccess('');
    try {
      await api.post<DocumentaryReception>(`/receptions/${reception.id}/suspend`, {
        reason: suspensionReason.trim(),
      });
      setSuccess('Recepción suspendida. El motivo quedó registrado.');
      closeAction();
      await load();
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : 'No se pudo suspender la recepción.');
    } finally {
      setWorkingId(null);
    }
  }

  const rows = data?.items ?? [];

  return (
    <>
      <ModuleHeader
        eyebrow="OPERACIÓN DOCUMENTAL · BP-001 / BP-002"
        title="Recepción y conteo"
        description="Registra la recepción física, documenta la inspección inicial y compara el inventario declarado con el conteo real."
        action={<><button className="secondary-button" onClick={() => void load()}><RefreshCw size={16} /> Actualizar</button>{canReceive && <button className="primary-button" onClick={() => setShowCreate((value) => !value)}><Plus size={17} /> Nueva recepción</button>}</>}
      />
      {error && <ErrorMessage>{error}</ErrorMessage>}
      {success && <SuccessMessage>{success}</SuccessMessage>}

      {canReceive && showCreate && (
        <section className="content-card create-card">
          <div className="section-heading">
            <div><p className="eyebrow">BP-001 · RECEPCIÓN</p><h2>Registrar entrega documental</h2></div>
            <button className="icon-button" aria-label="Cerrar formulario" onClick={() => setShowCreate(false)}><X size={18} /></button>
          </div>
          <form className="reception-create-form" onSubmit={createReception}>
            <div className="field-grow">
              <label htmlFor="reception-contract">Contrato activo</label>
              <select id="reception-contract" required value={contractId} onChange={(event) => setContractId(event.target.value)}>
                <option value="">Selecciona un contrato</option>
                {activeContracts.map((contract) => <option key={contract.id} value={contract.id}>{contract.contractNumber}</option>)}
              </select>
            </div>
            <div className="field-grow">
              <label htmlFor="reception-observations">Observaciones iniciales</label>
              <input id="reception-observations" maxLength={2000} value={initialObservations} onChange={(event) => setInitialObservations(event.target.value)} placeholder="Inventario recibido, documentos de entrega, novedades…" />
            </div>
            <button className="primary-button" type="submit" disabled={saving || !contractId}>{saving ? 'Guardando…' : 'Registrar recepción'}</button>
          </form>
          <p className="form-hint">La API verifica que el contrato esté activo. La programación de la entrega y la disponibilidad de capacidad física se verifican por Coordinación porque todavía no existe un modelo aprobado de capacidad por ubicación.</p>
        </section>
      )}

      <section className="content-card table-card">
        <div className="table-toolbar">
          <div><h2>Registro de recepciones</h2><p>{data?.totalCount ?? 0} recepciones registradas</p></div>
          <span className="soft-pill"><ClipboardList size={14} /> Trazabilidad por contrato</span>
        </div>
        {busy ? <LoadingState label="Consultando recepciones…" /> :
          rows.length === 0 ? <div className="empty-state"><ClipboardList size={25} /><strong>No hay recepciones registradas</strong><span>Cuando se reciba material documental, aparecerá en este registro.</span></div> :
          <div className="table-scroll">
            <table>
              <thead><tr><th>RECEPCIÓN</th><th>CONTRATO</th><th>FECHA</th><th>CONTEO (CAJAS / CARPETAS)</th><th>ESTADO</th><th>ACCIONES</th></tr></thead>
              <tbody>
                {rows.map((reception) => {
                  const isOpen = activeReceptionId === reception.id;
                  const isTerminal = reception.status === 'Counted' || reception.status === 'Suspended';
                  return (
                    <>
                      <tr key={reception.id}>
                        <td><span className="main-cell"><strong>{shortId(reception.id)}</strong><small>Cliente {shortId(reception.customerId)}</small></span></td>
                        <td>{shortId(reception.contractId)}</td>
                        <td>{formatDate(reception.receivedAtUtc)}</td>
                        <td>{reception.countedBoxCount == null ? 'Pendiente' : `${reception.countedBoxCount} / ${reception.countedFolderCount}`}</td>
                        <td><StatusBadge status={reception.status} /></td>
                        <td>
                          <div className="reception-row-actions">
                            {reception.status === 'Received' && canInspectOrCount && <button className="table-action" onClick={() => { setActiveReceptionId(reception.id); setActiveAction('inspection'); }}>Inspeccionar</button>}
                            {reception.status === 'Inspected' && canInspectOrCount && <button className="table-action" onClick={() => { setDeclaredBoxes('0'); setDeclaredFolders('0'); setCountedBoxes('0'); setCountedFolders('0'); setActiveReceptionId(reception.id); setActiveAction('counting'); }}>Registrar conteo</button>}
                            {reception.status === 'CountingRecorded' && canApprove && <button className="table-action" disabled={workingId === reception.id} onClick={() => void approveCounting(reception)}><CheckCheck size={14} /> Aprobar</button>}
                            {!isTerminal && canApprove && <button className="table-action reception-suspend-action" onClick={() => { setActiveReceptionId(reception.id); setActiveAction('suspend'); }}>Suspender</button>}
                            {!canInspectOrCount && !canApprove && <span className="form-hint">Consulta</span>}
                          </div>
                        </td>
                      </tr>
                      {isOpen && (
                        <tr className="reception-detail-row" key={`${reception.id}-detail`}>
                          <td colSpan={6}>
                            <section className="reception-action-panel">
                              <div className="reception-detail-summary">
                                <div><strong>Recepción {shortId(reception.id)}</strong><small>Recibió: {shortId(reception.receivedByUserId)}</small></div>
                                {reception.initialObservations && <p>{reception.initialObservations}</p>}
                              </div>
                              {activeAction === 'inspection' && (
                                <form className="reception-action-form" onSubmit={(event) => void submitInspection(event, reception)}>
                                  <div><label htmlFor={`inspection-result-${reception.id}`}>Resultado físico</label><select id={`inspection-result-${reception.id}`} value={safeToAccept} onChange={(event) => setSafeToAccept(event.target.value)}><option value="true">Apta para continuar</option><option value="false">Suspender por novedad</option></select></div>
                                  <div className="field-grow"><label htmlFor={`inspection-notes-${reception.id}`}>Observaciones de inspección</label><input id={`inspection-notes-${reception.id}`} maxLength={2000} required={safeToAccept === 'false'} value={inspectionObservations} onChange={(event) => setInspectionObservations(event.target.value)} placeholder="Estado de cajas, etiquetas, humedad, sellos…" /></div>
                                  <button className="primary-button" type="submit" disabled={workingId === reception.id}>{workingId === reception.id ? 'Guardando…' : 'Guardar inspección'}</button>
                                  <button className="secondary-button" type="button" onClick={closeAction}>Cancelar</button>
                                </form>
                              )}
                              {activeAction === 'counting' && (
                                <form className="reception-count-form" onSubmit={(event) => void submitCounting(event, reception)}>
                                  <div><label htmlFor={`declared-boxes-${reception.id}`}>Cajas declaradas</label><input id={`declared-boxes-${reception.id}`} type="number" min="0" required value={declaredBoxes} onChange={(event) => setDeclaredBoxes(event.target.value)} /></div>
                                  <div><label htmlFor={`declared-folders-${reception.id}`}>Carpetas declaradas</label><input id={`declared-folders-${reception.id}`} type="number" min="0" required value={declaredFolders} onChange={(event) => setDeclaredFolders(event.target.value)} /></div>
                                  <div><label htmlFor={`counted-boxes-${reception.id}`}>Cajas contadas</label><input id={`counted-boxes-${reception.id}`} type="number" min="0" required value={countedBoxes} onChange={(event) => setCountedBoxes(event.target.value)} /></div>
                                  <div><label htmlFor={`counted-folders-${reception.id}`}>Carpetas contadas</label><input id={`counted-folders-${reception.id}`} type="number" min="0" required value={countedFolders} onChange={(event) => setCountedFolders(event.target.value)} /></div>
                                  <div className="field-grow"><label htmlFor={`discrepancy-report-${reception.id}`}>Informe de discrepancias</label><input id={`discrepancy-report-${reception.id}`} maxLength={4000} value={discrepancyReport} onChange={(event) => setDiscrepancyReport(event.target.value)} placeholder="Obligatorio cuando los valores no coinciden" /></div>
                                  <button className="primary-button" type="submit" disabled={workingId === reception.id}>{workingId === reception.id ? 'Guardando…' : 'Registrar conteo'}</button>
                                  <button className="secondary-button" type="button" onClick={closeAction}>Cancelar</button>
                                </form>
                              )}
                              {activeAction === 'suspend' && (
                                <form className="reception-action-form" onSubmit={(event) => void submitSuspension(event, reception)}>
                                  <div className="field-grow"><label htmlFor={`suspension-reason-${reception.id}`}>Motivo de suspensión</label><input id={`suspension-reason-${reception.id}`} maxLength={1000} required value={suspensionReason} onChange={(event) => setSuspensionReason(event.target.value)} placeholder="Capacidad excedida, discrepancia severa, novedad pendiente…" /></div>
                                  <button className="primary-button" type="submit" disabled={workingId === reception.id}>{workingId === reception.id ? 'Guardando…' : 'Confirmar suspensión'}</button>
                                  <button className="secondary-button" type="button" onClick={closeAction}>Cancelar</button>
                                </form>
                              )}
                              {reception.status !== 'Received' && reception.inspectionObservations && <p className="form-hint">Inspección: {reception.inspectionObservations}</p>}
                              {reception.discrepancyReport && <p className="form-hint">Discrepancias: {reception.discrepancyReport}</p>}
                              {reception.suspensionReason && <p className="form-hint">Suspensión: {reception.suspensionReason}</p>}
                              <div className="reception-detail-footer"><span>Recibido por {shortId(reception.receivedByUserId)}</span>{reception.inspectedByUserId && <span>Inspeccionó {shortId(reception.inspectedByUserId)}</span>}{reception.countedByUserId && <span>Contó {shortId(reception.countedByUserId)}</span>}{reception.approvedByUserId && <span>Aprobó {shortId(reception.approvedByUserId)}</span>}</div>
                            </section>
                          </td>
                        </tr>
                      )}
                    </>
                  );
                })}
              </tbody>
            </table>
          </div>}
      </section>
      <div className="reception-footnote"><ShieldAlert size={15} /> La recepción y el conteo están implementados. La asignación a estantes/posiciones, la capacidad física y el inventario por caja/carpeta se incorporarán cuando el modelo WarehouseLocation y la jerarquía documental estén aprobados.</div>
    </>
  );
}
