import { useEffect, useState } from 'react';
import { ArrowDownRight, ArrowRight, ArrowUpRight, Archive, Boxes, ClipboardList, FileCheck2, FolderKanban, PackageOpen, RefreshCw, ShieldCheck } from 'lucide-react';
import { api, ApiError } from '../api/client';
import type { Contract, Customer, PagedResult, Project, Warehouse } from '../api/types';
import { ErrorMessage, LoadingState } from '../components/Feedback';
import { ModuleHeader } from '../components/ModuleHeader';
import type { ViewKey } from '../App';

type DashboardStats = { customers: number; contracts: number; projects: number; warehouses: number; receptions: number; boxes: number };
type ResourceKey = keyof DashboardStats;
const zeroStats: DashboardStats = { customers: 0, contracts: 0, projects: 0, warehouses: 0, receptions: 0, boxes: 0 };

const resourcesByRole: Record<string, Array<[ResourceKey, string]>> = {
  SystemAdministrator: [
    ['customers', '/customers'], ['contracts', '/contracts'], ['projects', '/projects'], ['warehouses', '/warehouses'], ['receptions', '/receptions'], ['boxes', '/boxes'], ['boxes', '/boxes'], ['boxes', '/boxes'],
  ],
  AdministrativeManager: [
    ['customers', '/customers'], ['contracts', '/contracts'], ['projects', '/projects'],
  ],
  ProjectCoordinator: [
    ['contracts', '/contracts'], ['projects', '/projects'], ['warehouses', '/warehouses'], ['receptions', '/receptions'],
  ],
  ArchiveTechnician: [
    ['projects', '/projects'], ['warehouses', '/warehouses'], ['receptions', '/receptions'],
  ],
  ArchiveAssistant: [
    ['projects', '/projects'], ['warehouses', '/warehouses'], ['receptions', '/receptions'],
  ],
  DigitizationOperator: [
    ['projects', '/projects'],
  ],
  BillingAssistant: [
    ['contracts', '/contracts'], ['projects', '/projects'],
  ],
};

type DashboardPageProps = { onNavigate: (view: ViewKey) => void; role: string };

export function DashboardPage({ onNavigate, role }: DashboardPageProps) {
  const [stats, setStats] = useState<DashboardStats>(zeroStats);
  const [busy, setBusy] = useState(true);
  const [error, setError] = useState('');

  async function load() {
    setBusy(true);
    setError('');
    try {
      const resources = resourcesByRole[role] ?? [];
      const results = await Promise.all(resources.map(async ([key, path]) => {
        const result = await api.get<PagedResult<Customer | Contract | Project | Warehouse>>(`${path}?page=1&pageSize=1`);
        return [key, result.totalCount] as const;
      }));
      const nextStats = { ...zeroStats };
      for (const [key, count] of results) nextStats[key] = count;
      setStats(nextStats);
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : 'No se pudo cargar el resumen.');
    } finally {
      setBusy(false);
    }
  }

  useEffect(() => { void load(); }, [role]);

  const metrics = [
    { key: 'customers' as const, label: 'Clientes registrados', value: stats.customers, icon: Archive, view: 'customers' as const, note: 'Organizaciones atendidas', trend: '+ Directorio' },
    { key: 'contracts' as const, label: 'Contratos', value: stats.contracts, icon: FileCheck2, view: 'contracts' as const, note: 'Relación comercial', trend: 'Ver contratos' },
    { key: 'projects' as const, label: 'Proyectos', value: stats.projects, icon: FolderKanban, view: 'projects' as const, note: 'Ejecución operativa', trend: 'Ver proyectos' },
    { key: 'warehouses' as const, label: 'Bodegas', value: stats.warehouses, icon: PackageOpen, view: 'warehouses' as const, note: 'Infraestructura de custodia', trend: 'Ver bodegas' },
    { key: 'receptions' as const, label: 'Recepciones', value: stats.receptions, icon: ClipboardList, view: 'receptions' as const, note: 'Ingresos documentales', trend: 'Ver recepciones' },
    { key: 'boxes' as const, label: 'Cajas registradas', value: stats.boxes, icon: Boxes, view: 'boxes' as const, note: 'Identificación física', trend: 'Ver cajas' },
  ].filter((metric) => (resourcesByRole[role] ?? []).some(([key]) => key === metric.key));

  const quickActions = [
    { key: 'customers' as const, title: 'Gestionar clientes', description: 'Identificación y estado', icon: Archive, view: 'customers' as const },
    { key: 'contracts' as const, title: 'Consultar contratos', description: 'Vinculados a los clientes', icon: FileCheck2, view: 'contracts' as const },
    { key: 'projects' as const, title: 'Seguir proyectos', description: 'Estado de ejecución', icon: FolderKanban, view: 'projects' as const },
    { key: 'warehouses' as const, title: 'Consultar bodegas', description: 'Infraestructura de custodia', icon: PackageOpen, view: 'warehouses' as const },
    { key: 'receptions' as const, title: 'Gestionar recepciones', description: 'Inspección y conteo', icon: ClipboardList, view: 'receptions' as const },
    { key: 'boxes' as const, title: 'Administrar cajas', description: 'Códigos y etiquetas', icon: Boxes, view: 'boxes' as const },
  ].filter((action) => (resourcesByRole[role] ?? []).some(([key]) => key === action.key));

  return (
    <>
      <ModuleHeader eyebrow="VISTA GENERAL" title="Panel de operación." description="Este es el pulso de los módulos disponibles para tu cargo." action={<button className="secondary-button" onClick={() => void load()}><RefreshCw size={16} /> Actualizar</button>} />
      {error && <ErrorMessage>{error}</ErrorMessage>}
      {metrics.length > 0 && <div className="metric-grid">
        {metrics.map(({ label, value, icon: Icon, view, note, trend }, index) => (
          <button className="metric-card" key={label} onClick={() => onNavigate(view)}>
            <div className="metric-top"><span className={`metric-icon metric-icon-${index}`}><Icon size={19} /></span><ArrowUpRight className="metric-arrow" size={18} /></div>
            <p className="metric-label">{label}</p>
            <div className="metric-value-row">{busy ? <span className="skeleton-number" /> : <span className="metric-value">{value.toLocaleString('es-CO')}</span>}</div>
            <div className="metric-bottom"><span>{note}</span><span className="metric-link">{trend}</span></div>
          </button>
        ))}
      </div>}

      <div className="dashboard-lower-grid">
        <section className="content-card priority-card">
          <div className="section-heading"><div><p className="eyebrow">SIGUIENTE PASO</p><h2>Tu operación empieza aquí</h2></div><span className="soft-pill"><ShieldCheck size={14} /> Núcleo operativo</span></div>
          <p className="muted-copy">Accede a las herramientas habilitadas para las responsabilidades de tu cargo. Los módulos documentales especializados se irán activando a medida que sus flujos estén conectados.</p>
          {quickActions.length > 0 ? <div className="quick-actions">
            {quickActions.map(({ title, description, icon: Icon, view }) => (
              <button key={view} onClick={() => onNavigate(view)}><span className="quick-action-icon"><Icon size={18} /></span><span><strong>{title}</strong><small>{description}</small></span><ArrowRight size={17} /></button>
            ))}
          </div> : <p className="muted-copy">Tu espacio está autenticado. Los módulos propios de este cargo están en preparación.</p>}
        </section>
        <section className="content-card system-card">
          <div className="section-heading"><div><p className="eyebrow">SISTEMA</p><h2>Conectividad</h2></div><span className="health-mark"><span /></span></div>
          <div className="system-status-row"><span className="system-status-icon"><Boxes size={18} /></span><span><strong>ArchiCore API</strong><small>REST · ASP.NET Core 10</small></span><span className="status-badge status-positive">Activa</span></div>
          <div className="system-status-row"><span className="system-status-icon"><PackageOpen size={18} /></span><span><strong>Base de datos</strong><small>PostgreSQL · Persistencia</small></span><span className="status-badge status-positive">Activa</span></div>
          <div className="system-footnote"><ArrowDownRight size={15} /> Los indicadores consultan solamente endpoints autorizados para tu cargo.</div>
          {busy && <LoadingState label="Actualizando indicadores…" />}
        </section>
      </div>
    </>
  );
}
