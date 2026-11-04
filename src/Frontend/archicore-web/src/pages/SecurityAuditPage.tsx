import { useEffect, useState, type FormEvent } from 'react';
import { RefreshCw, Search, ShieldAlert } from 'lucide-react';
import { api, ApiError } from '../api/client';
import type { PagedResult, SecurityAuditEntry } from '../api/types';
import { ErrorMessage, LoadingState } from '../components/Feedback';
import { ModuleHeader } from '../components/ModuleHeader';
import { StatusBadge } from '../components/StatusBadge';

function formatDate(value: string): string {
  return new Date(value).toLocaleString('es-CO', {
    dateStyle: 'medium',
    timeStyle: 'short',
    timeZone: 'UTC',
  }) + ' UTC';
}

function shortId(value: string | null): string {
  return value ? value.slice(0, 8).toUpperCase() : 'Sistema / no identificado';
}

export function SecurityAuditPage() {
  const [data, setData] = useState<PagedResult<SecurityAuditEntry> | null>(null);
  const [actionInput, setActionInput] = useState('');
  const [action, setAction] = useState('');
  const [outcome, setOutcome] = useState('');
  const [page, setPage] = useState(1);
  const [busy, setBusy] = useState(true);
  const [error, setError] = useState('');

  async function load(requestedPage = page) {
    setBusy(true);
    setError('');
    const query = new URLSearchParams({
      page: String(requestedPage),
      pageSize: '25',
    });
    if (action) query.set('action', action);
    if (outcome) query.set('outcome', outcome);

    try {
      setData(await api.get<PagedResult<SecurityAuditEntry>>(
        `/audit/security-events?${query.toString()}`,
      ));
    } catch (caught) {
      setError(caught instanceof ApiError
        ? caught.message
        : 'No se pudo consultar la auditoría de seguridad.');
    } finally {
      setBusy(false);
    }
  }

  useEffect(() => { void load(page); }, [page, action, outcome]);

  function applyFilters(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setPage(1);
    setAction(actionInput.trim());
  }

  return (
    <>
      <ModuleHeader
        eyebrow="ADMINISTRACIÓN Y CONTROL"
        title="Auditoría de seguridad"
        description="Trazabilidad de autenticaciones y cambios de cuentas. Esta bitácora es independiente de los eventos del dominio."
        action={<button className="secondary-button" onClick={() => void load(page)}><RefreshCw size={16} /> Actualizar</button>}
      />
      <div className="feedback feedback-success">
        <ShieldAlert size={17} />
        Se registran acciones, resultado, actor y usuario objetivo. Nunca se almacenan contraseñas, JWT ni refresh tokens.
      </div>
      {error && <ErrorMessage>{error}</ErrorMessage>}

      <section className="content-card audit-filter-card">
        <form className="audit-filters" onSubmit={applyFilters}>
          <div className="audit-event-filter">
            <label htmlFor="security-audit-action">Acción</label>
            <input
              id="security-audit-action"
              maxLength={128}
              value={actionInput}
              onChange={(event) => setActionInput(event.target.value)}
              placeholder="Ej. User.Created"
            />
          </div>
          <div>
            <label htmlFor="security-audit-outcome">Resultado</label>
            <select id="security-audit-outcome" value={outcome} onChange={(event) => { setPage(1); setOutcome(event.target.value); }}>
              <option value="">Todos</option>
              <option value="Succeeded">Exitoso</option>
              <option value="Failed">Fallido</option>
            </select>
          </div>
          <button className="primary-button" type="submit"><Search size={15} /> Aplicar</button>
        </form>
      </section>

      <section className="content-card table-card audit-table-card">
        <div className="table-toolbar">
          <div><h2>Acciones de seguridad</h2><p>{data?.totalCount ?? 0} registros encontrados</p></div>
          <span className="soft-pill"><ShieldAlert size={14} /> Solo administrador</span>
        </div>
        {busy ? <LoadingState label="Consultando bitácora…" /> :
          !data?.items.length ? <div className="empty-state"><ShieldAlert size={25} /><strong>No hay eventos para mostrar</strong><span>Prueba otro filtro o vuelve a consultar más tarde.</span></div> :
          <div className="table-scroll">
            <table>
              <thead><tr><th>FECHA (UTC)</th><th>ACCIÓN</th><th>RESULTADO</th><th>ACTOR</th><th>OBJETIVO</th></tr></thead>
              <tbody>
                {data.items.map((entry) => (
                  <tr key={entry.id}>
                    <td>{formatDate(entry.occurredOnUtc)}</td>
                    <td><span className="main-cell"><strong>{entry.action}</strong><small>ID {entry.id.slice(0, 8).toUpperCase()}</small></span></td>
                    <td><StatusBadge status={entry.outcome === 'Succeeded' ? 'Completado' : 'Fallido'} /></td>
                    <td><span className="main-cell"><strong>{entry.actorRole ?? 'No autenticado'}</strong><small>{shortId(entry.actorUserId)}</small></span></td>
                    <td><span className="main-cell"><strong>{entry.targetRole ?? '—'}</strong><small>{shortId(entry.targetUserId)}</small></span></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>}
        <div className="table-pagination">
          <span>Página {data?.page ?? page} de {Math.max(data?.totalPages ?? 1, 1)}</span>
          <div>
            <button className="secondary-button" disabled={page <= 1 || busy} onClick={() => setPage((value) => Math.max(1, value - 1))}>Anterior</button>
            <button className="secondary-button" disabled={busy || page >= (data?.totalPages ?? 1)} onClick={() => setPage((value) => value + 1)}>Siguiente</button>
          </div>
        </div>
      </section>
    </>
  );
}
