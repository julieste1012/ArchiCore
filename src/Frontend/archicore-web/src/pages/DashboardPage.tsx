import { useEffect, useState } from 'react';
import { ArrowDownRight, ArrowRight, ArrowUpRight, Archive, Boxes, FileCheck2, FolderKanban, PackageOpen, RefreshCw, ShieldCheck } from 'lucide-react';
import { api, ApiError } from '../api/client';
import type { Contract, Customer, PagedResult, Project, Warehouse } from '../api/types';
import { ErrorMessage, LoadingState } from '../components/Feedback';
import { ModuleHeader } from '../components/ModuleHeader';
import type { ViewKey } from '../App';

type DashboardStats = { customers: number; contracts: number; projects: number; warehouses: number };
const zeroStats: DashboardStats = { customers: 0, contracts: 0, projects: 0, warehouses: 0 };

type DashboardPageProps = { onNavigate: (view: ViewKey) => void };

export function DashboardPage({ onNavigate }: DashboardPageProps) {
  const [stats, setStats] = useState<DashboardStats>(zeroStats);
  const [busy, setBusy] = useState(true);
  const [error, setError] = useState('');

  async function load() {
    setBusy(true);
    setError('');
    try {
      const [customers, contracts, projects, warehouses] = await Promise.all([
        api.get<PagedResult<Customer>>('/customers?page=1&pageSize=1'),
        api.get<PagedResult<Contract>>('/contracts?page=1&pageSize=1'),
        api.get<PagedResult<Project>>('/projects?page=1&pageSize=1'),
        api.get<PagedResult<Warehouse>>('/warehouses?page=1&pageSize=1'),
      ]);
      setStats({
        customers: customers.totalCount,
        contracts: contracts.totalCount,
        projects: projects.totalCount,
        warehouses: warehouses.totalCount,
      });
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : 'No se pudo cargar el resumen.');
    } finally {
      setBusy(false);
    }
  }

  useEffect(() => { void load(); }, []);

  const metrics = [
    { label: 'Clientes registrados', value: stats.customers, icon: Archive, view: 'customers' as const, note: 'Organizaciones atendidas', trend: '+ Directorio' },
    { label: 'Contratos', value: stats.contracts, icon: FileCheck2, view: 'contracts' as const, note: 'Relación comercial', trend: 'Ver contratos' },
    { label: 'Proyectos', value: stats.projects, icon: FolderKanban, view: 'projects' as const, note: 'Ejecución operativa', trend: 'Ver proyectos' },
    { label: 'Bodegas', value: stats.warehouses, icon: PackageOpen, view: 'warehouses' as const, note: 'Infraestructura de custodia', trend: 'Ver bodegas' },
  ];

  return (
    <>
      <ModuleHeader eyebrow="VISTA GENERAL" title="Buenos días." description="Este es el pulso actual de tu operación documental." action={<button className="secondary-button" onClick={() => void load()}><RefreshCw size={16} /> Actualizar</button>} />
      {error && <ErrorMessage>{error}</ErrorMessage>}
      <div className="metric-grid">
        {metrics.map(({ label, value, icon: Icon, view, note, trend }, index) => (
          <button className="metric-card" key={label} onClick={() => onNavigate(view)}>
            <div className="metric-top"><span className={`metric-icon metric-icon-${index}`}><Icon size={19} /></span><ArrowUpRight className="metric-arrow" size={18} /></div>
            <p className="metric-label">{label}</p>
            <div className="metric-value-row">{busy ? <span className="skeleton-number" /> : <span className="metric-value">{value.toLocaleString('es-CO')}</span>}</div>
            <div className="metric-bottom"><span>{note}</span><span className="metric-link">{trend}</span></div>
          </button>
        ))}
      </div>

      <div className="dashboard-lower-grid">
        <section className="content-card priority-card">
          <div className="section-heading"><div><p className="eyebrow">SIGUIENTE PASO</p><h2>Tu operación empieza aquí</h2></div><span className="soft-pill"><ShieldCheck size={14} /> Núcleo operativo</span></div>
          <p className="muted-copy">Registra clientes, vincula contratos y organiza los proyectos que soportan el servicio documental.</p>
          <div className="quick-actions">
            <button onClick={() => onNavigate('customers')}><span className="quick-action-icon"><Archive size={18} /></span><span><strong>Gestionar clientes</strong><small>Identificación y estado</small></span><ArrowRight size={17} /></button>
            <button onClick={() => onNavigate('contracts')}><span className="quick-action-icon"><FileCheck2 size={18} /></span><span><strong>Consultar contratos</strong><small>Vinculados a los clientes</small></span><ArrowRight size={17} /></button>
            <button onClick={() => onNavigate('projects')}><span className="quick-action-icon"><FolderKanban size={18} /></span><span><strong>Seguir proyectos</strong><small>Estado de ejecución</small></span><ArrowRight size={17} /></button>
          </div>
        </section>
        <section className="content-card system-card">
          <div className="section-heading"><div><p className="eyebrow">SISTEMA</p><h2>Conectividad</h2></div><span className="health-mark"><span /></span></div>
          <div className="system-status-row"><span className="system-status-icon"><Boxes size={18} /></span><span><strong>ArchiCore API</strong><small>REST · ASP.NET Core 10</small></span><span className="status-badge status-positive">Activa</span></div>
          <div className="system-status-row"><span className="system-status-icon"><PackageOpen size={18} /></span><span><strong>Base de datos</strong><small>PostgreSQL · Persistencia</small></span><span className="status-badge status-positive">Activa</span></div>
          <div className="system-footnote"><ArrowDownRight size={15} /> Los conteos se consultan directamente desde la API.</div>
          {busy && <LoadingState label="Actualizando indicadores…" />}
        </section>
      </div>
    </>
  );
}
