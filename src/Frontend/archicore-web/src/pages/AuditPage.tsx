import { useEffect, useState, type FormEvent } from 'react';
import { Braces, ChevronDown, ChevronRight, FileClock, RefreshCw, Search } from 'lucide-react';
import { api, ApiError } from '../api/client';
import type { DomainEventAuditEntry, PagedResult } from '../api/types';
import { ErrorMessage, LoadingState } from '../components/Feedback';
import { ModuleHeader } from '../components/ModuleHeader';
import { StatusBadge } from '../components/StatusBadge';

function prettyPayload(payload: string): string {
  try {
    return JSON.stringify(JSON.parse(payload) as unknown, null, 2);
  } catch {
    return payload;
  }
}

export function AuditPage() {
  const [data, setData] = useState<PagedResult<DomainEventAuditEntry> | null>(null);
  const [eventTypeInput, setEventTypeInput] = useState('');
  const [eventType, setEventType] = useState('');
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');
  const [fromUtc, setFromUtc] = useState('');
  const [toUtc, setToUtc] = useState('');
  const [page, setPage] = useState(1);
  const [busy, setBusy] = useState(true);
  const [error, setError] = useState('');

  async function load(requestedPage = page) {
    setBusy(true);
    setError('');
    const query = new URLSearchParams({ page: String(requestedPage), pageSize: '20' });
    if (eventType) query.set('eventType', eventType);
    if (fromUtc) query.set('fromUtc', fromUtc);
    if (toUtc) query.set('toUtc', toUtc);

    try {
      setData(await api.get<PagedResult<DomainEventAuditEntry>>(`/audit/domain-events?${query.toString()}`));
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : 'No se pudo consultar la auditoría.');
    } finally {
      setBusy(false);
    }
  }

  useEffect(() => { void load(page); }, [page, eventType, fromUtc, toUtc]);

  function applyFilters(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setPage(1);
    setEventType(eventTypeInput.trim());
    setFromUtc(fromDate ? new Date(`${fromDate}T00:00:00.000Z`).toISOString() : '');
    setToUtc(toDate ? new Date(`${toDate}T23:59:59.999Z`).toISOString() : '');
  }

  return (
    <>
      <ModuleHeader eyebrow="TRAZABILIDAD" title="Auditoría de eventos" description="Consulta el registro durable de eventos de dominio procesados por el sistema." action={<button className="secondary-button" onClick={() => void load(page)}><RefreshCw size={16} /> Actualizar</button>} />
      <div className="feedback feedback-success"><FileClock size={17} /> La auditoría actual registra eventos de dominio. Todavía no representa una bitácora completa de acciones por usuario.</div>
      {error && <ErrorMessage>{error}</ErrorMessage>}
      <section className="content-card audit-filter-card">
        <form className="audit-filters" onSubmit={applyFilters}>
          <div className="audit-event-filter"><label htmlFor="audit-event-type">Tipo de evento</label><input id="audit-event-type" maxLength={512} value={eventTypeInput} onChange={(event) => setEventTypeInput(event.target.value)} placeholder="Ej. ArchiCore.Domain.Customers.Events.CustomerCreated" /></div>
          <div><label htmlFor="audit-from-date">Desde (UTC)</label><input id="audit-from-date" type="date" value={fromDate} onChange={(event) => setFromDate(event.target.value)} /></div>
          <div><label htmlFor="audit-to-date">Hasta (UTC)</label><input id="audit-to-date" type="date" value={toDate} onChange={(event) => setToDate(event.target.value)} /></div>
          <button className="primary-button" type="submit"><Search size={15} /> Aplicar</button>
        </form>
      </section>

      <section className="content-card table-card audit-table-card">
        <div className="table-toolbar"><div><h2>Eventos registrados</h2><p>{data?.totalCount ?? 0} eventos encontrados</p></div><span className="soft-pill"><Braces size={14} /> Payload JSON</span></div>
        {busy ? <LoadingState label="Consultando eventos…" /> : !data?.items.length ? <div className="empty-state"><FileClock size={25} /><strong>No hay eventos para mostrar</strong><span>Prueba otros filtros o vuelve a consultar más tarde.</span></div> :
          <div className="audit-event-list">{data.items.map((item) => (
            <article className="audit-event-row" key={item.id}>
              <div className="audit-event-marker"><span /></div>
              <div className="audit-event-main">
                <div className="audit-event-heading"><strong>{item.eventType}</strong><StatusBadge status="Procesado" /></div>
                <div className="audit-event-meta"><span>Ocurrió: {new Date(item.occurredOnUtc).toLocaleString('es-CO', { timeZone: 'UTC' })} UTC</span><span>Registrado: {new Date(item.recordedAtUtc).toLocaleString('es-CO', { timeZone: 'UTC' })} UTC</span></div>
                <details className="audit-payload">
                  <summary><ChevronRight className="audit-chevron" size={15} /><span>Ver payload del evento</span><ChevronDown className="audit-chevron-open" size={15} /></summary>
                  <pre>{prettyPayload(item.payload)}</pre>
                </details>
                <small className="audit-event-id">ID {item.id}</small>
              </div>
            </article>
          ))}</div>}
        <div className="table-pagination"><span>Página {data?.page ?? page} de {Math.max(data?.totalPages ?? 1, 1)}</span><div><button className="secondary-button" disabled={page <= 1 || busy} onClick={() => setPage((value) => Math.max(1, value - 1))}>Anterior</button><button className="secondary-button" disabled={busy || page >= (data?.totalPages ?? 1)} onClick={() => setPage((value) => value + 1)}>Siguiente</button></div></div>
      </section>
    </>
  );
}
