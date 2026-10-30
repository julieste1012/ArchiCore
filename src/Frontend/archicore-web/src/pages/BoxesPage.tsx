import { useEffect, useMemo, useState, type FormEvent } from 'react';
import { Barcode, Boxes, Plus, RefreshCw, Search, Tag, X } from 'lucide-react';
import { api, ApiError } from '../api/client';
import type { Box, PagedResult } from '../api/types';
import { ErrorMessage, LoadingState, SuccessMessage } from '../components/Feedback';
import { ModuleHeader } from '../components/ModuleHeader';
import { StatusBadge } from '../components/StatusBadge';

type BoxesPageProps = { role: string };

function shortId(value: string): string {
  return value.slice(0, 8).toUpperCase();
}

export function BoxesPage({ role }: BoxesPageProps) {
  const canRegister = role === 'SystemAdministrator' || role === 'ArchiveTechnician';
  const [data, setData] = useState<PagedResult<Box> | null>(null);
  const [busy, setBusy] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [search, setSearch] = useState('');
  const [showCreate, setShowCreate] = useState(false);
  const [boxCode, setBoxCode] = useState('');
  const [barcodeDrafts, setBarcodeDrafts] = useState<Record<string, string>>({});
  const [assigningId, setAssigningId] = useState<string | null>(null);

  async function load() {
    setBusy(true);
    setError('');
    try {
      const result = await api.get<PagedResult<Box>>('/boxes?page=1&pageSize=100');
      setData(result);
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : 'No se pudo consultar el inventario de cajas.');
    } finally {
      setBusy(false);
    }
  }

  useEffect(() => { void load(); }, []);

  const rows = useMemo(() => {
    const term = search.trim().toLocaleLowerCase();
    if (!term) return data?.items ?? [];
    return (data?.items ?? []).filter((box) =>
      box.boxCode.toLocaleLowerCase().includes(term)
      || box.barcode?.toLocaleLowerCase().includes(term),
    );
  }, [data, search]);

  async function createBox(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSaving(true);
    setError('');
    setSuccess('');
    try {
      const created = await api.post<Box>('/boxes', { boxCode: boxCode.trim() });
      setBoxCode('');
      setShowCreate(false);
      setSuccess(`Caja ${created.boxCode} registrada correctamente.`);
      await load();
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : 'No se pudo registrar la caja.');
    } finally {
      setSaving(false);
    }
  }

  async function assignBarcode(event: FormEvent<HTMLFormElement>, box: Box) {
    event.preventDefault();
    const barcode = (barcodeDrafts[box.id] ?? '').trim();
    if (!barcode) return;

    setAssigningId(box.id);
    setError('');
    setSuccess('');
    try {
      await api.post<Box>(`/boxes/${box.id}/barcode`, { barcode });
      setBarcodeDrafts((current) => ({ ...current, [box.id]: '' }));
      setSuccess(`Código de barras asignado a ${box.boxCode}.`);
      await load();
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : 'No se pudo asignar el código de barras.');
    } finally {
      setAssigningId(null);
    }
  }

  return (
    <>
      <ModuleHeader
        eyebrow="IDENTIFICACIÓN DOCUMENTAL"
        title="Cajas y códigos de barras"
        description="Registra cada unidad física y asocia su identificador de lectura. Los movimientos y posiciones físicas se habilitarán con el modelo de ubicaciones."
        action={<><button className="secondary-button" onClick={() => void load()}><RefreshCw size={16} /> Actualizar</button>{canRegister && <button className="primary-button" onClick={() => setShowCreate((value) => !value)}><Plus size={17} /> Nueva caja</button>}</>}
      />
      {error && <ErrorMessage>{error}</ErrorMessage>}
      {success && <SuccessMessage>{success}</SuccessMessage>}

      {canRegister && showCreate && (
        <section className="content-card create-card box-create-card">
          <div className="section-heading">
            <div><p className="eyebrow">REGISTRO DE UNIDAD</p><h2>Crear caja documental</h2></div>
            <button className="icon-button" aria-label="Cerrar formulario" onClick={() => setShowCreate(false)}><X size={18} /></button>
          </div>
          <form className="box-create-form" onSubmit={(event) => void createBox(event)}>
            <div className="field-grow">
              <label htmlFor="new-box-code">Código de caja</label>
              <input
                id="new-box-code"
                value={boxCode}
                onChange={(event) => setBoxCode(event.target.value.toUpperCase())}
                placeholder="BOX-000001"
                maxLength={20}
                required
              />
              <small>Formato de referencia: BOX-000001. El código debe ser único.</small>
            </div>
            <button className="primary-button" type="submit" disabled={saving || !boxCode.trim()}>{saving ? 'Guardando…' : 'Registrar caja'}</button>
          </form>
        </section>
      )}

      <section className="content-card table-card">
        <div className="table-toolbar">
          <div><h2>Registro de cajas</h2><p>{data?.totalCount ?? 0} unidades registradas</p></div>
          <label className="search-field">
            <Search size={16} />
            <input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Buscar código o barcode" aria-label="Buscar por código o barcode" />
          </label>
        </div>
        {busy ? <LoadingState label="Consultando cajas…" /> :
          rows.length === 0 ? <div className="empty-state"><Boxes size={25} /><strong>No hay cajas que mostrar</strong><span>Prueba otra búsqueda o registra una nueva unidad.</span></div> :
          <div className="table-scroll">
            <table>
              <thead><tr><th>CÓDIGO</th><th>CÓDIGO DE BARRAS</th><th>ESTADO</th><th>REGISTRO</th>{canRegister && <th>ROTULADO</th>}</tr></thead>
              <tbody>
                {rows.map((box) => (
                  <tr key={box.id}>
                    <td>
                      <span className="table-avatar table-avatar-green"><Boxes size={17} /></span>
                      <span className="main-cell"><strong>{box.boxCode}</strong><small>ID {shortId(box.id)}</small></span>
                    </td>
                    <td>{box.barcode
                      ? <span className="box-barcode-value"><Barcode size={15} /> {box.barcode}</span>
                      : <span className="count-chip">Sin asignar</span>}</td>
                    <td><StatusBadge status={box.status} /></td>
                    <td>{new Date(box.createdAt).toLocaleDateString('es-CO')}</td>
                    {canRegister && <td>
                      {box.barcode
                        ? <span className="box-assigned-label"><Tag size={14} /> Etiquetada</span>
                        : <form className="box-barcode-form" onSubmit={(event) => void assignBarcode(event, box)}>
                            <input
                              aria-label={`Código de barras para ${box.boxCode}`}
                              value={barcodeDrafts[box.id] ?? ''}
                              onChange={(event) => setBarcodeDrafts((current) => ({ ...current, [box.id]: event.target.value.toUpperCase() }))}
                              placeholder="Escanear o escribir"
                              maxLength={100}
                              required
                            />
                            <button className="table-action" type="submit" disabled={assigningId === box.id || !(barcodeDrafts[box.id] ?? '').trim()}>
                              {assigningId === box.id ? 'Guardando…' : 'Asignar'}
                            </button>
                          </form>}
                    </td>}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>}
      </section>

      <div className="box-module-footnote">
        <Barcode size={15} />
        <span>El código de caja y el código de barras son únicos. Esta versión cubre registro y rotulado; no permite movimientos, asignación a estantes ni asociación de carpetas hasta que la especificación de ubicación física esté cerrada.</span>
      </div>
    </>
  );
}
