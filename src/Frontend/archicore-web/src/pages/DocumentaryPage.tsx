import { useEffect, useState, type FormEvent } from 'react';
import { Archive, Barcode, Boxes, FileText, Folder, HardDrive, Layers3, Plus, RefreshCw, ScanLine, Upload } from 'lucide-react';
import { api, ApiError } from '../api/client';
import type { ArchiveTree, Box, DigitizationRequest, FolderRecord, Loan, PagedResult, Project, Warehouse, WarehouseLocation } from '../api/types';
import { ErrorMessage, LoadingState, SuccessMessage } from '../components/Feedback';
import { ModuleHeader } from '../components/ModuleHeader';
import { StatusBadge } from '../components/StatusBadge';

type Tab = 'archive' | 'boxes' | 'folders' | 'loans' | 'digitization';
type Props = { role: string };
function message(error: unknown, fallback: string) { return error instanceof ApiError ? error.message : fallback; }
function localDateTime(value: Date) {
  const adjusted = new Date(value.getTime() - value.getTimezoneOffset() * 60000);
  return adjusted.toISOString().slice(0, 16);
}

export function DocumentaryPage({ role }: Props) {
  const canClassify = ['SystemAdministrator', 'ProjectCoordinator', 'ArchiveTechnician'].includes(role);
  const canCreateBoxes = ['SystemAdministrator', 'ArchiveTechnician'].includes(role);
  const canReadPhysical = ['SystemAdministrator', 'ProjectCoordinator', 'ArchiveTechnician', 'ArchiveAssistant'].includes(role);
  const canMovePhysical = ['SystemAdministrator', 'ArchiveAssistant'].includes(role);
  const canRequestLoan = ['SystemAdministrator', 'ProjectCoordinator'].includes(role);
  const canDeliverLoan = ['SystemAdministrator', 'ArchiveAssistant'].includes(role);
  const canRequestDigitization = ['SystemAdministrator', 'ProjectCoordinator', 'ArchiveTechnician'].includes(role);
  const canDigitize = ['SystemAdministrator', 'DigitizationOperator'].includes(role);

  const [tab, setTab] = useState<Tab>('archive');
  const [projects, setProjects] = useState<Project[]>([]);
  const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
  const [projectId, setProjectId] = useState('');
  const [archive, setArchive] = useState<ArchiveTree | null>(null);
  const [archiveMissing, setArchiveMissing] = useState(false);
  const [boxes, setBoxes] = useState<Box[]>([]);
  const [folders, setFolders] = useState<FolderRecord[]>([]);
  const [locations, setLocations] = useState<WarehouseLocation[]>([]);
  const [loans, setLoans] = useState<Loan[]>([]);
  const [digitizations, setDigitizations] = useState<DigitizationRequest[]>([]);
  const [warehouseId, setWarehouseId] = useState('');
  const [boxId, setBoxId] = useState('');
  const [folderId, setFolderId] = useState('');
  const [expedientId, setExpedientId] = useState('');
  const [sectionId, setSectionId] = useState('');
  const [seriesId, setSeriesId] = useState('');
  const [subseriesId, setSubseriesId] = useState('');
  const [boxCode, setBoxCode] = useState('');
  const [barcode, setBarcode] = useState('');
  const [locationCode, setLocationCode] = useState('');
  const [locationType, setLocationType] = useState('Aisle');
  const [parentLocationId, setParentLocationId] = useState('');
  const [capacity, setCapacity] = useState('1');
  const [destinationLocationId, setDestinationLocationId] = useState('');
  const [sectionCode, setSectionCode] = useState('');
  const [sectionName, setSectionName] = useState('');
  const [seriesCode, setSeriesCode] = useState('');
  const [seriesName, setSeriesName] = useState('');
  const [subseriesCode, setSubseriesCode] = useState('');
  const [subseriesName, setSubseriesName] = useState('');
  const [expedientReference, setExpedientReference] = useState('');
  const [expedientTitle, setExpedientTitle] = useState('');
  const [folderCode, setFolderCode] = useState('');
  const [documentTitle, setDocumentTitle] = useState('');
  const [dueAt, setDueAt] = useState(localDateTime(new Date(Date.now() + 7 * 86400000)));
  const [files, setFiles] = useState<File[]>([]);
  const [busy, setBusy] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const selectedBox = boxes.find(item => item.id === boxId);
  const selectedFolder = folders.find(item => item.id === folderId);
  const sections = archive?.sections ?? [];
  const section = sections.find(item => item.id === sectionId) ?? sections[0];
  const expedients = sections.flatMap(item => item.series).flatMap(item => item.subseries).flatMap(item => item.expedients);
  const positions = locations.filter(item => item.type === 'Position');
  const parentChoices = locations.filter(item => (
    locationType === 'Shelf' ? item.type === 'Aisle' :
    locationType === 'Level' ? item.type === 'Shelf' :
    locationType === 'Position' ? item.type === 'Level' : false
  ));

  async function loadInitial() {
    setBusy(true); setError('');
    try {
      const emptyPage = <T,>(): PagedResult<T> => ({ page: 1, pageSize: 100, totalCount: 0, totalPages: 0, items: [] });
      const [projectResult, warehouseResult, boxResult] = await Promise.all([
        api.get<PagedResult<Project>>('/projects?page=1&pageSize=100'),
        canReadPhysical ? api.get<PagedResult<Warehouse>>('/warehouses?page=1&pageSize=100') : Promise.resolve(emptyPage<Warehouse>()),
        canReadPhysical ? api.get<PagedResult<Box>>('/boxes?page=1&pageSize=100') : Promise.resolve(emptyPage<Box>()),
      ]);
      setProjects(projectResult.items);
      setWarehouses(warehouseResult.items);
      setBoxes(boxResult.items);
      if (!projectId && projectResult.items[0]) setProjectId(projectResult.items[0].id);
      if (!warehouseId && warehouseResult.items[0]) setWarehouseId(warehouseResult.items[0].id);
      if (!boxId && boxResult.items[0]) setBoxId(boxResult.items[0].id);
    } catch (caught) { setError(message(caught, 'No se pudo cargar la operación documental.')); }
    finally { setBusy(false); }
  }

  async function reloadBoxes() {
    const result = await api.get<PagedResult<Box>>('/boxes?page=1&pageSize=100');
    setBoxes(result.items);
  }

  async function reloadArchive() {
    if (!projectId) return;
    try {
      const result = await api.get<ArchiveTree>('/projects/' + projectId + '/documentary-archive');
      setArchive(result); setArchiveMissing(false);
      const list = await api.get<PagedResult<FolderRecord>>('/archives/' + result.id + '/folders?page=1&pageSize=100');
      setFolders(list.items);
      if (!folderId && list.items[0]) setFolderId(list.items[0].id);
    } catch (caught) {
      if (caught instanceof ApiError && caught.status === 404) {
        setArchive(null); setArchiveMissing(true); setFolders([]); setFolderId('');
      } else setError(message(caught, 'No se pudo consultar el Fondo documental.'));
    }
  }

  async function reloadLocations() {
    if (!warehouseId) { setLocations([]); return; }
    try { setLocations(await api.get<WarehouseLocation[]>('/warehouses/' + warehouseId + '/locations')); }
    catch (caught) { setError(message(caught, 'No se pudieron consultar las ubicaciones.')); }
  }

  async function reloadActivity() {
    if (!folderId) { setLoans([]); setDigitizations([]); return; }
    try {
      const [loanResult, digitizationResult] = await Promise.all([
        canRequestLoan || canDeliverLoan
          ? api.get<Loan[]>('/folders/' + folderId + '/loans')
          : Promise.resolve([] as Loan[]),
        role !== 'BillingAssistant' && role !== 'CustomerRepresentative'
          ? api.get<DigitizationRequest[]>('/folders/' + folderId + '/digitizations')
          : Promise.resolve([] as DigitizationRequest[]),
      ]);
      setLoans(loanResult); setDigitizations(digitizationResult);
    } catch (caught) { setError(message(caught, 'No se pudo consultar la actividad de la carpeta.')); }
  }

  useEffect(() => { void loadInitial(); }, []);
  useEffect(() => { void reloadArchive(); }, [projectId]);
  useEffect(() => { void reloadLocations(); }, [warehouseId]);
  useEffect(() => { void reloadActivity(); }, [folderId]);

  async function run(action: () => Promise<unknown>, done: string, refresh?: () => Promise<void>) {
    setSaving(true); setError(''); setSuccess('');
    try { await action(); setSuccess(done); if (refresh) await refresh(); }
    catch (caught) { setError(message(caught, 'No fue posible completar la operación.')); }
    finally { setSaving(false); }
  }

  async function createArchive() {
    if (!projectId) return;
    await run(() => api.post('/projects/' + projectId + '/documentary-archive'), 'Fondo documental creado.', reloadArchive);
  }

  async function addClassification(event: FormEvent<HTMLFormElement>, kind: 'sections' | 'series' | 'subseries' | 'expedients') {
    event.preventDefault();
    if (!archive) return;
    let endpoint = '/archives/' + archive.id + '/sections';
    let body: object = { code: sectionCode, name: sectionName };
    if (kind === 'series') { endpoint += '/' + (section?.id ?? '') + '/series'; body = { code: seriesCode, name: seriesName }; }
    if (kind === 'subseries') { endpoint = '/archives/' + archive.id + '/series/' + seriesId + '/subseries'; body = { code: subseriesCode, name: subseriesName }; }
    if (kind === 'expedients') { endpoint = '/archives/' + archive.id + '/subseries/' + subseriesId + '/expedients'; body = { reference: expedientReference, title: expedientTitle }; }
    await run(() => api.post(endpoint, body), 'Elemento de clasificación creado.', reloadArchive);
  }

  async function createBox(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await run(async () => { await api.post('/boxes', { boxCode: boxCode.trim() }); setBoxCode(''); }, 'Caja creada.', reloadBoxes);
  }

  async function assignBarcode(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!boxId) return;
    await run(async () => { await api.post('/boxes/' + boxId + '/barcode', { barcode: barcode.trim() }); setBarcode(''); }, 'Código de barras asignado.', reloadBoxes);
  }

  async function createLocation(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!warehouseId) return;
    await run(() => api.post('/warehouses/' + warehouseId + '/locations', {
      code: locationCode.trim(), type: locationType,
      parentLocationId: parentLocationId || null,
      capacity: locationType === 'Position' ? Number(capacity) : null,
    }), 'Ubicación creada.', async () => { setLocationCode(''); await reloadLocations(); });
  }

  async function storeOrRelocateBox() {
    if (!boxId || !warehouseId || !destinationLocationId || !selectedBox) return;
    const isStored = selectedBox.status === 'InStorage';
    const path = '/boxes/' + boxId + (isStored ? '/relocate' : '/store');
    const body = isStored
      ? { sourceWarehouseId: warehouseId, destinationWarehouseId: warehouseId, destinationLocationId }
      : { warehouseId, locationId: destinationLocationId };
    await run(() => api.post(path, body), isStored ? 'Caja trasladada.' : 'Caja almacenada.',
      async () => { await reloadBoxes(); await reloadLocations(); });
  }

  async function createFolder(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!archive || !expedientId) return;
    await run(async () => {
      await api.post('/archives/' + archive.id + '/folders', { folderCode: folderCode.trim(), expedientId });
      setFolderCode('');
    }, 'Carpeta creada.', reloadArchive);
  }

  async function updateFolder(operation: 'receive' | 'organize') {
    if (!folderId) return;
    await run(() => api.post('/folders/' + folderId + '/' + operation),
      operation === 'receive' ? 'Recepción de carpeta registrada.' : 'Carpeta organizada.', reloadArchive);
  }

  async function addDocument(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!folderId) return;
    await run(async () => {
      await api.post('/folders/' + folderId + '/documents', { title: documentTitle.trim(), documentDate: null });
      setDocumentTitle('');
    }, 'Documento registrado.', reloadArchive);
  }

  async function requestLoan(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!projectId || !folderId) return;
    await run(() => api.post('/projects/' + projectId + '/folders/' + folderId + '/loans', { dueAtUtc: new Date(dueAt).toISOString() }),
      'Solicitud de préstamo registrada.', reloadActivity);
  }

  async function requestDigitization() {
    if (!folderId) return;
    await run(() => api.post('/folders/' + folderId + '/digitizations'), 'Solicitud de digitalización creada.', reloadActivity);
  }

  async function uploadFiles(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const pending = digitizations.find(item => item.status === 'Processing');
    if (!pending || files.length === 0) return;
    const form = new FormData();
    files.forEach(file => form.append('files', file));
    await run(async () => {
      await api.upload('/digitizations/' + pending.id + '/files', form);
      setFiles([]);
    }, 'PDF almacenados y registrados.', reloadActivity);
  }

  const tabs: { id: Tab; label: string; icon: typeof Archive }[] = [
    { id: 'archive', label: 'Clasificación', icon: Archive },
    { id: 'boxes', label: 'Cajas y bodegas', icon: Boxes },
    { id: 'folders', label: 'Carpetas', icon: Folder },
    { id: 'loans', label: 'Préstamos', icon: FileText },
    { id: 'digitization', label: 'Digitalización', icon: ScanLine },
  ];

  return <>
    <ModuleHeader eyebrow="GESTIÓN DOCUMENTAL" title="Operación documental"
      description="Clasificación archivística, custodia física, préstamos y digitalización."
      action={<button className="secondary-button" onClick={() => { void loadInitial(); void reloadArchive(); void reloadBoxes(); }}><RefreshCw size={16} /> Actualizar</button>} />
    {error && <ErrorMessage>{error}</ErrorMessage>}
    {success && <SuccessMessage>{success}</SuccessMessage>}
    <section className="content-card documentary-context">
      <div className="documentary-context-label"><Layers3 size={16} /> CONTEXTO DE OPERACIÓN</div>
      <div className="documentary-context-controls">
        <div className="field-grow"><label htmlFor="documentary-project">Proyecto</label>
          <select id="documentary-project" value={projectId} onChange={e => setProjectId(e.target.value)}>
            <option value="">Selecciona un proyecto</option>
            {projects.map(item => <option key={item.id} value={item.id}>{item.projectCode}</option>)}
          </select>
        </div>
        <div className="documentary-context-note"><strong>{archive ? 'Fondo documental vinculado' : archiveMissing ? 'Proyecto sin Fondo documental' : 'Selecciona el proyecto'}</strong>
          <span>{archive ? sections.length + ' secciones registradas' : 'La clasificación y las carpetas dependen del proyecto seleccionado.'}</span></div>
      </div>
    </section>
    <div className="documentary-tabs" role="tablist" aria-label="Módulos documentales">
      {tabs.map(item => { const Icon = item.icon; return <button type="button" role="tab" aria-selected={tab === item.id}
        className={tab === item.id ? 'documentary-tab active' : 'documentary-tab'} key={item.id} onClick={() => setTab(item.id)}>
        <Icon size={16} />{item.label}</button>; })}
    </div>
    {busy && <LoadingState label="Cargando operación documental…" />}

    {tab === 'archive' && <>
      {archiveMissing && <section className="content-card create-card"><h2>Este proyecto aún no tiene Fondo documental</h2>
        <p className="muted-copy">Crea el Fondo para construir su clasificación archivística y registrar expedientes.</p>
        {canClassify && <button className="primary-button" disabled={saving || !projectId} onClick={() => void createArchive()}><Plus size={15} /> Crear Fondo documental</button>}</section>}
      {archive && <div className="documentary-grid">
        <section className="content-card"><div className="section-heading"><div><p className="eyebrow">ESTRUCTURA ARCHIVÍSTICA</p><h2>Fondo documental</h2></div></div>
          {sections.length === 0 ? <div className="empty-state"><Archive size={24} /><strong>Clasificación vacía</strong><span>Agrega una sección para comenzar.</span></div> :
            <div className="archive-tree">{sections.map(s => <div className="archive-tree-section" key={s.id}><strong>{s.code} · {s.name}</strong>
              {s.series.map(ser => <div className="archive-tree-series" key={ser.id}><span>{ser.code} · {ser.name}</span>
                {ser.subseries.map(sub => <div className="archive-tree-subseries" key={sub.id}><span>{sub.code} · {sub.name}</span>
                  {sub.expedients.map(exp => <div className="archive-tree-expedient" key={exp.id}><FileText size={13} /><span>{exp.reference} · {exp.title}</span></div>)}</div>)}</div>)}</div>)}</div>}
        </section>
        {canClassify && <section className="content-card"><p className="eyebrow">CONSTRUIR CLASIFICACIÓN</p><h2>Nuevos elementos</h2>
          <form className="documentary-mini-form" onSubmit={e => void addClassification(e, 'sections')}><strong>Sección</strong><div className="form-grid"><div><label>Código</label><input required value={sectionCode} onChange={e => setSectionCode(e.target.value)} /></div><div><label>Nombre</label><input required value={sectionName} onChange={e => setSectionName(e.target.value)} /></div></div><button className="secondary-button" disabled={saving}><Plus size={13} /> Agregar sección</button></form>
          <form className="documentary-mini-form" onSubmit={e => void addClassification(e, 'series')}><strong>Serie</strong><select value={sectionId} onChange={e => setSectionId(e.target.value)} required><option value="">Sección</option>{sections.map(item => <option key={item.id} value={item.id}>{item.code} · {item.name}</option>)}</select><div className="form-grid"><div><label>Código</label><input required value={seriesCode} onChange={e => setSeriesCode(e.target.value)} /></div><div><label>Nombre</label><input required value={seriesName} onChange={e => setSeriesName(e.target.value)} /></div></div><button className="secondary-button" disabled={saving || !sections.length}><Plus size={13} /> Agregar serie</button></form>
          <form className="documentary-mini-form" onSubmit={e => void addClassification(e, 'subseries')}><strong>Subserie</strong><select value={seriesId} onChange={e => setSeriesId(e.target.value)} required><option value="">Serie</option>{sections.flatMap(item => item.series).map(item => <option key={item.id} value={item.id}>{item.code} · {item.name}</option>)}</select><div className="form-grid"><div><label>Código</label><input required value={subseriesCode} onChange={e => setSubseriesCode(e.target.value)} /></div><div><label>Nombre</label><input required value={subseriesName} onChange={e => setSubseriesName(e.target.value)} /></div></div><button className="secondary-button" disabled={saving}><Plus size={13} /> Agregar subserie</button></form>
          <form className="documentary-mini-form" onSubmit={e => void addClassification(e, 'expedients')}><strong>Expediente</strong><select value={subseriesId} onChange={e => setSubseriesId(e.target.value)} required><option value="">Subserie</option>{sections.flatMap(item => item.series).flatMap(item => item.subseries).map(item => <option key={item.id} value={item.id}>{item.code} · {item.name}</option>)}</select><div className="form-grid"><div><label>Referencia</label><input required value={expedientReference} onChange={e => setExpedientReference(e.target.value)} /></div><div><label>Título</label><input required value={expedientTitle} onChange={e => setExpedientTitle(e.target.value)} /></div></div><button className="secondary-button" disabled={saving}><Plus size={13} /> Crear expediente</button></form>
        </section>}
      </div>}
    </>}

    {tab === 'boxes' && <div className="documentary-grid">
      <section className="content-card"><div className="section-heading"><div><p className="eyebrow">INVENTARIO FÍSICO</p><h2>Cajas registradas</h2></div><span className="soft-pill">{boxes.length} cajas</span></div>
        {canCreateBoxes && <form className="inline-form documentary-create-row" onSubmit={createBox}><div className="field-grow"><label>Código de caja</label><input value={boxCode} onChange={e => setBoxCode(e.target.value)} placeholder="BOX-000123" required /></div><button className="primary-button" disabled={saving}><Plus size={14} /> Crear</button></form>}
        {boxes.length === 0 ? <div className="empty-state"><Boxes size={24} /><strong>No hay cajas</strong><span>Registra una caja para controlar su ubicación.</span></div> :
          <div className="table-scroll"><table><thead><tr><th>CÓDIGO</th><th>BARCODE</th><th>ESTADO</th></tr></thead><tbody>{boxes.map(item => <tr key={item.id} onClick={() => setBoxId(item.id)} className={boxId === item.id ? 'row-selected' : ''}><td><strong>{item.boxCode}</strong></td><td>{item.barcode ?? 'Sin asignar'}</td><td><StatusBadge status={item.status} /></td></tr>)}</tbody></table></div>}
      </section>
      <section className="content-card"><div className="section-heading"><div><p className="eyebrow">BODEGAS Y POSICIONES</p><h2>{selectedBox?.boxCode ?? 'Selecciona una caja'}</h2></div><Barcode size={19} /></div>
        {selectedBox && <>
          {canCreateBoxes && selectedBox.status === 'Created' && <form className="documentary-mini-form" onSubmit={assignBarcode}><strong>Asignar código de barras</strong><div className="inline-form"><div className="field-grow"><input value={barcode} onChange={e => setBarcode(e.target.value)} placeholder="Código escaneado" required /></div><button className="secondary-button" disabled={saving}><Barcode size={13} /> Asignar</button></div></form>}
          {canCreateBoxes && selectedBox.status === 'Created' && selectedBox.barcode && <button className="secondary-button" disabled={saving} onClick={() => void run(() => api.post('/boxes/' + selectedBox.id + '/available'), 'Caja habilitada.', reloadBoxes)}>Marcar disponible</button>}
          <div className="documentary-mini-form"><strong>Ubicación física</strong><label>Bodega</label><select value={warehouseId} onChange={e => setWarehouseId(e.target.value)}><option value="">Bodega</option>{warehouses.map(item => <option key={item.id} value={item.id}>{item.warehouseCode}</option>)}</select><label>Posición</label><select value={destinationLocationId} onChange={e => setDestinationLocationId(e.target.value)}><option value="">Posición</option>{positions.map(item => <option key={item.id} value={item.id}>{item.code} ({item.occupiedCapacity}/{item.capacity})</option>)}</select>
            {canMovePhysical && ['Available', 'InStorage'].includes(selectedBox.status) && <button className="primary-button" disabled={saving || !destinationLocationId} onClick={() => void storeOrRelocateBox()}><HardDrive size={14} /> {selectedBox.status === 'InStorage' ? 'Trasladar' : 'Almacenar'}</button>}</div>
        </>}
        {canMovePhysical && <form className="documentary-mini-form" onSubmit={createLocation}><strong>Crear ubicación</strong><div className="form-grid"><div><label>Nivel</label><select value={locationType} onChange={e => { setLocationType(e.target.value); setParentLocationId(''); }}><option value="Aisle">Pasillo</option><option value="Shelf">Estante</option><option value="Level">Nivel</option><option value="Position">Posición</option></select></div><div><label>Código</label><input required value={locationCode} onChange={e => setLocationCode(e.target.value)} placeholder="A-01" /></div></div>{locationType !== 'Aisle' && <><label>Ubicación padre</label><select value={parentLocationId} onChange={e => setParentLocationId(e.target.value)} required><option value="">Superior</option>{parentChoices.map(item => <option key={item.id} value={item.id}>{item.code}</option>)}</select></>}{locationType === 'Position' && <><label>Capacidad de cajas</label><input type="number" min="1" value={capacity} onChange={e => setCapacity(e.target.value)} /></>}<button className="secondary-button" disabled={saving || !warehouseId || (locationType !== 'Aisle' && !parentLocationId)}><Plus size={13} /> Crear ubicación</button><p className="form-hint">Pasillo → estante → nivel → posición. Solo las posiciones almacenan cajas.</p></form>}
      </section>
    </div>}

    {tab === 'folders' && <>
      {!archive && <section className="content-card"><div className="empty-state"><Folder size={24} /><strong>Primero crea el Fondo documental</strong><span>Selecciona un proyecto y regístralo en la pestaña Clasificación.</span></div></section>}
      {archive && <div className="documentary-grid">
        <section className="content-card"><div className="section-heading"><div><p className="eyebrow">INVENTARIO DOCUMENTAL</p><h2>Carpetas</h2></div><span className="soft-pill">{folders.length} registros</span></div>
          {canClassify && <form className="documentary-mini-form" onSubmit={createFolder}><strong>Nueva carpeta</strong><label>Código</label><input required value={folderCode} onChange={e => setFolderCode(e.target.value)} placeholder="FOL-000123" /><label>Expediente</label><select value={expedientId} onChange={e => setExpedientId(e.target.value)} required><option value="">Selecciona expediente</option>{expedients.map(item => <option key={item.id} value={item.id}>{item.reference} · {item.title}</option>)}</select><button className="primary-button" disabled={saving || !expedients.length}><Plus size={14} /> Crear carpeta</button></form>}
          <div className="table-scroll"><table><thead><tr><th>CÓDIGO</th><th>ESTADO</th><th>CAJA</th></tr></thead><tbody>{folders.map(item => <tr key={item.id} onClick={() => setFolderId(item.id)} className={folderId === item.id ? 'row-selected' : ''}><td><strong>{item.folderCode}</strong></td><td><StatusBadge status={item.status} /></td><td>{item.boxId ? item.boxId.slice(0, 8).toUpperCase() : 'Sin caja'}</td></tr>)}</tbody></table></div>
        </section>
        <section className="content-card"><div className="section-heading"><div><p className="eyebrow">DETALLE</p><h2>{selectedFolder?.folderCode ?? 'Selecciona una carpeta'}</h2></div><FileText size={18} /></div>
          {selectedFolder && <><div className="documentary-detail-row"><span>Estado</span><StatusBadge status={selectedFolder.status} /></div><div className="documentary-detail-row"><span>Documentos</span><strong>{selectedFolder.documents.length}</strong></div>
            {canClassify && selectedFolder.status === 'Created' && <button className="secondary-button" disabled={saving} onClick={() => void updateFolder('receive')}>Registrar recepción</button>}
            {canClassify && selectedFolder.status === 'Received' && <button className="secondary-button" disabled={saving} onClick={() => void updateFolder('organize')}>Marcar organizada</button>}
            {canClassify && <form className="documentary-mini-form" onSubmit={addDocument}><strong>Registrar documento</strong><input required value={documentTitle} onChange={e => setDocumentTitle(e.target.value)} placeholder="Título del documento" /><button className="secondary-button" disabled={saving}><Plus size={13} /> Agregar</button></form>}
            <div className="documentary-documents">{selectedFolder.documents.map(doc => <div key={doc.id}><FileText size={13} /><span>{doc.title}</span><small>{doc.folios.length} folios · {doc.annexes.length} anexos</small></div>)}</div>
            {canMovePhysical && boxes.length > 0 && <div className="documentary-mini-form"><strong>Asignar o mover a caja</strong><select value={boxId} onChange={e => setBoxId(e.target.value)}>{boxes.map(item => <option key={item.id} value={item.id}>{item.boxCode}</option>)}</select><button className="secondary-button" disabled={saving || !boxId} onClick={() => void run(() => api.post('/folders/' + selectedFolder.id + (selectedFolder.status === 'Stored' ? '/move-to-box/' : '/box/') + boxId), 'Relación de carpeta y caja actualizada.', reloadArchive)}>Guardar caja</button></div>}
          </>}
        </section>
      </div>}
    </>}

    {tab === 'loans' && <div className="documentary-grid">
      <section className="content-card"><p className="eyebrow">CUSTODIA TEMPORAL</p><h2>Solicitar préstamo</h2><label>Carpeta</label><select value={folderId} onChange={e => setFolderId(e.target.value)}><option value="">Selecciona carpeta</option>{folders.map(item => <option key={item.id} value={item.id}>{item.folderCode} · {item.status}</option>)}</select><form className="documentary-mini-form" onSubmit={requestLoan}><label>Fecha prevista de devolución</label><input type="datetime-local" value={dueAt} onChange={e => setDueAt(e.target.value)} required />{canRequestLoan && <button className="primary-button" disabled={saving || !folderId}>Registrar solicitud</button>}</form><p className="form-hint">Coordinación autoriza el préstamo; el asistente registra entrega y devolución.</p></section>
      <section className="content-card"><p className="eyebrow">HISTORIAL</p><h2>Préstamos</h2>{loans.length === 0 ? <div className="empty-state"><FileText size={22} /><strong>Sin préstamos registrados</strong><span>Selecciona una carpeta con historial.</span></div> : <div className="documentary-activity-list">{loans.map(item => <article key={item.id}><div><strong>{new Date(item.requestedAtUtc).toLocaleDateString('es-CO')}</strong><small>Vence: {new Date(item.dueAtUtc).toLocaleDateString('es-CO')}</small></div><StatusBadge status={item.status} />{canRequestLoan && item.status === 'Requested' && <button className="secondary-button compact-button" disabled={saving} onClick={() => void run(() => api.post('/loans/' + item.id + '/authorize'), 'Préstamo autorizado.', reloadActivity)}>Autorizar</button>}{canDeliverLoan && item.status === 'Authorized' && <button className="secondary-button compact-button" disabled={saving} onClick={() => void run(() => api.post('/loans/' + item.id + '/deliver'), 'Entrega registrada.', reloadActivity)}>Entregar</button>}{canDeliverLoan && ['Delivered', 'Overdue'].includes(item.status) && <button className="secondary-button compact-button" disabled={saving} onClick={() => void run(() => api.post('/loans/' + item.id + '/return'), 'Devolución registrada.', reloadActivity)}>Devolución</button>}</article>)}</div>}</section>
    </div>}

    {tab === 'digitization' && <div className="documentary-grid">
      <section className="content-card"><p className="eyebrow">CONVERSIÓN DIGITAL</p><h2>Digitalizar carpeta</h2><label>Carpeta</label><select value={folderId} onChange={e => setFolderId(e.target.value)}><option value="">Selecciona carpeta</option>{folders.map(item => <option key={item.id} value={item.id}>{item.folderCode}</option>)}</select>{canRequestDigitization && <button className="primary-button documentary-button-gap" disabled={saving || !folderId} onClick={() => void requestDigitization()}><Plus size={14} /> Solicitar digitalización</button>}<p className="form-hint">La API valida los PDF, calcula SHA-256, cuenta páginas y guarda los archivos en el almacenamiento configurado.</p></section>
      <section className="content-card"><p className="eyebrow">SOLICITUDES Y ARCHIVOS</p><h2>Actividad de digitalización</h2>{digitizations.length === 0 ? <div className="empty-state"><ScanLine size={22} /><strong>Sin solicitudes</strong><span>Registra una solicitud para comenzar.</span></div> : <div className="documentary-activity-list">{digitizations.map(item => <article key={item.id}><div><strong>{new Date(item.requestedAtUtc).toLocaleDateString('es-CO')}</strong><small>{item.files.length} archivos</small></div><StatusBadge status={item.status} />{canDigitize && item.status === 'Requested' && <button className="secondary-button compact-button" disabled={saving} onClick={() => void run(() => api.post('/digitizations/' + item.id + '/start'), 'Digitalización iniciada.', reloadActivity)}>Iniciar</button>}{canDigitize && item.status === 'Processing' && <form className="digitization-upload" onSubmit={uploadFiles}><input type="file" accept="application/pdf,.pdf" multiple onChange={e => setFiles(Array.from(e.target.files ?? []))} /><button className="secondary-button compact-button" disabled={saving || !files.length}><Upload size={13} /> Subir PDF</button></form>}{item.files.map(file => <span className="documentary-file-chip" key={file.id}>{file.originalFileName} · {file.pageCount} páginas</span>)}</article>)}</div>}</section>
    </div>}
  </>;
}
