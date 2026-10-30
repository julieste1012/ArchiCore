import { useState } from 'react';
import { AuditPage } from './pages/AuditPage';
import { ReceptionsPage } from './pages/ReceptionsPage';
import { BoxesPage } from './pages/BoxesPage';
import { SecurityAuditPage } from './pages/SecurityAuditPage';
import { ChangePasswordForm } from './components/ChangePasswordForm';
import { UsersPage } from './pages/UsersPage';
import { Activity, Archive, Bell, Boxes, ChevronDown, FileCheck2, FileClock, ClipboardList, FolderKanban, PackageSearch, KeyRound, LayoutDashboard, LogOut, PackageOpen, Settings2, ShieldCheck, ShieldAlert, UsersRound } from 'lucide-react';
import { useAuth } from './auth/AuthProvider';
import { LoginPage } from './pages/LoginPage';
import { DashboardPage } from './pages/DashboardPage';
import { CustomersPage } from './pages/CustomersPage';
import { ContractsPage } from './pages/ContractsPage';
import { ProjectsPage } from './pages/ProjectsPage';
import { WarehousesPage } from './pages/WarehousesPage';

export type ViewKey = 'dashboard' | 'customers' | 'contracts' | 'projects' | 'warehouses' | 'receptions' | 'boxes' | 'users' | 'audit' | 'securityAudit' | 'password';

const navItems: { id: ViewKey; label: string; icon: typeof LayoutDashboard; section: string }[] = [
  { id: 'dashboard', label: 'Panel general', icon: LayoutDashboard, section: 'ESPACIO DE TRABAJO' },
  { id: 'customers', label: 'Clientes', icon: UsersRound, section: 'GESTIÓN COMERCIAL' },
  { id: 'contracts', label: 'Contratos', icon: FileCheck2, section: 'GESTIÓN COMERCIAL' },
  { id: 'projects', label: 'Proyectos', icon: FolderKanban, section: 'OPERACIÓN DOCUMENTAL' },
  { id: 'warehouses', label: 'Bodegas', icon: PackageOpen, section: 'OPERACIÓN DOCUMENTAL' },
  { id: 'receptions', label: 'Recepciones', icon: ClipboardList, section: 'OPERACIÓN DOCUMENTAL' },
  { id: 'boxes', label: 'Cajas', icon: PackageSearch, section: 'OPERACIÓN DOCUMENTAL' },
  { id: 'users', label: 'Usuarios', icon: UsersRound, section: 'ADMINISTRACIÓN' },
  { id: 'audit', label: 'Eventos de dominio', icon: FileClock, section: 'ADMINISTRACIÓN' },
  { id: 'securityAudit', label: 'Seguridad', icon: ShieldAlert, section: 'ADMINISTRACIÓN' },
  { id: 'password', label: 'Mi contraseña', icon: KeyRound, section: 'MI CUENTA' },
];

const roleViewAccess: Record<string, ViewKey[]> = {
  SystemAdministrator: ['dashboard', 'customers', 'contracts', 'projects', 'warehouses', 'receptions', 'boxes', 'users', 'audit', 'securityAudit', 'password'],
  AdministrativeManager: ['dashboard', 'customers', 'contracts', 'projects', 'password'],
  ProjectCoordinator: ['dashboard', 'contracts', 'projects', 'warehouses', 'receptions', 'boxes', 'password'],
  ArchiveTechnician: ['dashboard', 'projects', 'warehouses', 'receptions', 'boxes', 'password'],
  ArchiveAssistant: ['dashboard', 'projects', 'warehouses', 'receptions', 'boxes', 'password'],
  DigitizationOperator: ['dashboard', 'projects', 'password'],
  BillingAssistant: ['dashboard', 'contracts', 'projects', 'password'],
  CustomerRepresentative: ['dashboard', 'password'],
};

const pageNames: Record<ViewKey, string> = {
  dashboard: 'Panel general',
  customers: 'Clientes',
  contracts: 'Contratos',
  projects: 'Proyectos',
  warehouses: 'Bodegas',
  receptions: 'Recepción documental',
  boxes: 'Cajas y códigos de barras',
  users: 'Usuarios',
  audit: 'Eventos de dominio',
  securityAudit: 'Auditoría de seguridad',
  password: 'Cambiar contraseña',
};

export function App() {
  const { user, logout } = useAuth();
  const [view, setView] = useState<ViewKey>('dashboard');

  if (!user) return <LoginPage />;


  const allowedViews = roleViewAccess[user.role] ?? ['dashboard', 'password'];
  const activeView = allowedViews.includes(view) ? view : 'dashboard';
  const visibleNavItems = navItems.filter((item) => allowedViews.includes(item.id));

  const initials = user.displayName.split(/\s+/).slice(0, 2).map((part) => part[0]?.toUpperCase()).join('');
  const viewContent = (() => {
    switch (activeView) {
      case 'customers': return <CustomersPage />;
      case 'contracts': return <ContractsPage canManage={user.role === 'SystemAdministrator' || user.role === 'AdministrativeManager'} />;
      case 'projects': return <ProjectsPage canOperate={user.role === 'SystemAdministrator' || user.role === 'ProjectCoordinator'} />;
      case 'warehouses': return <WarehousesPage canManage={user.role === 'SystemAdministrator'} />;
      case 'receptions': return <ReceptionsPage role={user.role} />;
      case 'boxes': return <BoxesPage role={user.role} />;
      case 'users': return <UsersPage />;
      case 'audit': return <AuditPage />;
      case 'securityAudit': return <SecurityAuditPage />;
      case 'password': return <ChangePasswordForm onComplete={() => void logout()} />;
      default: return <DashboardPage onNavigate={setView} role={user.role} />;
    }
  })();

  const orderedItems = visibleNavItems.filter((item, index, all) => all.findIndex((candidate) => candidate.section === item.section) === index);

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand sidebar-brand"><span className="brand-mark"><Boxes size={23} strokeWidth={2.1} /></span><span>ArchiCore</span><span className="brand-beta">ERP</span></div>
        <div className="workspace-chip"><span className="workspace-icon"><Archive size={17} /></span><span><strong>Multisertec LTDA</strong><small>Espacio de trabajo</small></span><ChevronDown size={14} /></div>
        <nav className="side-navigation" aria-label="Navegación principal">
          {orderedItems.map((group) => (
            <div className="nav-group" key={group.section}>
              <p className="nav-section-label">{group.section}</p>
              {visibleNavItems.filter((item) => item.section === group.section).map(({ id, label, icon: Icon }) => (
                <button key={id} className={`nav-item ${activeView === id ? 'nav-item-active' : ''}`} onClick={() => setView(id)}>
                  <Icon size={18} strokeWidth={1.8} /><span>{label}</span>{activeView === id && <span className="nav-active-dot" />}
                </button>
              ))}
            </div>
          ))}
        </nav>
        <div className="sidebar-bottom">
          <div className="side-help-card"><span className="help-symbol"><ShieldCheck size={18} /></span><div><strong>Sesión protegida</strong><small>Acceso por roles</small></div><Activity size={15} /></div>
          {user.role === 'SystemAdministrator' && <button className={`nav-item nav-settings ${view === 'users' ? 'nav-item-active' : ''}`} onClick={() => setView('users')}><Settings2 size={18} /><span>Administrar usuarios</span></button>}
          <div className="sidebar-user"><span className="user-avatar">{initials || 'AC'}</span><span className="user-details"><strong>{user.displayName}</strong><small>{user.role}</small></span><button className="icon-button logout-button" title="Cerrar sesión" aria-label="Cerrar sesión" onClick={() => void logout()}><LogOut size={17} /></button></div>
        </div>
      </aside>
      <main className="main-area">
        <header className="topbar"><div className="breadcrumb"><span>ArchiCore</span><span className="breadcrumb-slash">/</span><strong>{pageNames[activeView]}</strong></div><div className="topbar-actions"><span className="topbar-date">{new Date().toLocaleDateString('es-CO', { weekday: 'short', day: 'numeric', month: 'short' })}</span>{user.role === 'SystemAdministrator' && <button className={`icon-button notification-button ${view === 'audit' ? 'nav-item-active' : ''}`} title="Abrir auditoría" aria-label="Abrir auditoría" onClick={() => setView('audit')}><Bell size={18} /><span /></button>}<span className="topbar-divider" /><div className="topbar-user"><span className="user-avatar small-avatar">{initials || 'AC'}</span><span><strong>{user.displayName}</strong><small>{user.role}</small></span></div></div></header>
        <div className="page-content">{viewContent}</div>
        <footer className="app-footer"><span>ArchiCore ERP <span className="footer-separator">·</span> Núcleo operativo</span><span><span className="live-dot" /> API conectada <span className="footer-separator">·</span> v0.1.0</span></footer>
      </main>
    </div>
  );
}
