import { useState } from 'react';
import { AuditPage } from './pages/AuditPage';
import { ChangePasswordForm } from './components/ChangePasswordForm';
import { UsersPage } from './pages/UsersPage';
import { Activity, Archive, Bell, Boxes, ChevronDown, FileCheck2, FileClock, FolderKanban, KeyRound, LayoutDashboard, LogOut, PackageOpen, Settings2, ShieldCheck, UsersRound } from 'lucide-react';
import { useAuth } from './auth/AuthProvider';
import { LoginPage } from './pages/LoginPage';
import { DashboardPage } from './pages/DashboardPage';
import { CustomersPage } from './pages/CustomersPage';
import { ContractsPage } from './pages/ContractsPage';
import { ProjectsPage } from './pages/ProjectsPage';
import { WarehousesPage } from './pages/WarehousesPage';

export type ViewKey = 'dashboard' | 'customers' | 'contracts' | 'projects' | 'warehouses' | 'users' | 'audit' | 'password';

const navItems: { id: ViewKey; label: string; icon: typeof LayoutDashboard; section: string }[] = [
  { id: 'dashboard', label: 'Panel general', icon: LayoutDashboard, section: 'ESPACIO DE TRABAJO' },
  { id: 'customers', label: 'Clientes', icon: UsersRound, section: 'GESTIÓN COMERCIAL' },
  { id: 'contracts', label: 'Contratos', icon: FileCheck2, section: 'GESTIÓN COMERCIAL' },
  { id: 'projects', label: 'Proyectos', icon: FolderKanban, section: 'OPERACIÓN DOCUMENTAL' },
  { id: 'warehouses', label: 'Bodegas', icon: PackageOpen, section: 'OPERACIÓN DOCUMENTAL' },
  { id: 'users', label: 'Usuarios', icon: UsersRound, section: 'ADMINISTRACIÓN' },
  { id: 'audit', label: 'Auditoría', icon: FileClock, section: 'ADMINISTRACIÓN' },
  { id: 'password', label: 'Mi contraseña', icon: KeyRound, section: 'MI CUENTA' },
];

const pageNames: Record<ViewKey, string> = {
  dashboard: 'Panel general',
  customers: 'Clientes',
  contracts: 'Contratos',
  projects: 'Proyectos',
  warehouses: 'Bodegas',
  users: 'Usuarios',
  audit: 'Auditoría',
  password: 'Cambiar contraseña',
};

export function App() {
  const { user, logout } = useAuth();
  const [view, setView] = useState<ViewKey>('dashboard');

  if (!user) return <LoginPage />;

  if (user.role !== 'SystemAdministrator') {
    return (
      <main className="access-pending-screen">
        <div className="access-pending-card">
          <span className="brand-mark"><ShieldCheck size={25} /></span>
          <p className="eyebrow">CUENTA AUTENTICADA</p>
          <h1>Estamos preparando tu espacio.</h1>
          <p>Tu cuenta está activa con el rol <strong>{user.role}</strong>. La matriz de permisos de cada módulo está pendiente de aprobación, por lo que las pantallas operativas permanecen temporalmente restringidas.</p>
          <ChangePasswordForm compact onComplete={() => void logout()} />
          <button className="primary-button" onClick={() => void logout()}><LogOut size={16} /> Cerrar sesión</button>
        </div>
      </main>
    );
  }

  const initials = user.displayName.split(/\s+/).slice(0, 2).map((part) => part[0]?.toUpperCase()).join('');
  const viewContent = (() => {
    switch (view) {
      case 'customers': return <CustomersPage />;
      case 'contracts': return <ContractsPage />;
      case 'projects': return <ProjectsPage />;
      case 'warehouses': return <WarehousesPage />;
      case 'users': return <UsersPage />;
      case 'audit': return <AuditPage />;
      case 'password': return <ChangePasswordForm onComplete={() => void logout()} />;
      default: return <DashboardPage onNavigate={setView} />;
    }
  })();

  const orderedItems = navItems.filter((item, index, all) => all.findIndex((candidate) => candidate.section === item.section) === index);

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand sidebar-brand"><span className="brand-mark"><Boxes size={23} strokeWidth={2.1} /></span><span>ArchiCore</span><span className="brand-beta">ERP</span></div>
        <div className="workspace-chip"><span className="workspace-icon"><Archive size={17} /></span><span><strong>Multisertec LTDA</strong><small>Espacio de trabajo</small></span><ChevronDown size={14} /></div>
        <nav className="side-navigation" aria-label="Navegación principal">
          {orderedItems.map((group) => (
            <div className="nav-group" key={group.section}>
              <p className="nav-section-label">{group.section}</p>
              {navItems.filter((item) => item.section === group.section).map(({ id, label, icon: Icon }) => (
                <button key={id} className={`nav-item ${view === id ? 'nav-item-active' : ''}`} onClick={() => setView(id)}>
                  <Icon size={18} strokeWidth={1.8} /><span>{label}</span>{view === id && <span className="nav-active-dot" />}
                </button>
              ))}
            </div>
          ))}
        </nav>
        <div className="sidebar-bottom">
          <div className="side-help-card"><span className="help-symbol"><ShieldCheck size={18} /></span><div><strong>Sesión protegida</strong><small>Acceso por roles</small></div><Activity size={15} /></div>
          <button className={`nav-item nav-settings ${view === 'users' ? 'nav-item-active' : ''}`} onClick={() => setView('users')}><Settings2 size={18} /><span>Administrar usuarios</span></button>
          <div className="sidebar-user"><span className="user-avatar">{initials || 'AC'}</span><span className="user-details"><strong>{user.displayName}</strong><small>{user.role}</small></span><button className="icon-button logout-button" title="Cerrar sesión" aria-label="Cerrar sesión" onClick={() => void logout()}><LogOut size={17} /></button></div>
        </div>
      </aside>
      <main className="main-area">
        <header className="topbar"><div className="breadcrumb"><span>ArchiCore</span><span className="breadcrumb-slash">/</span><strong>{pageNames[view]}</strong></div><div className="topbar-actions"><span className="topbar-date">{new Date().toLocaleDateString('es-CO', { weekday: 'short', day: 'numeric', month: 'short' })}</span><button className={`icon-button notification-button ${view === 'audit' ? 'nav-item-active' : ''}`} title="Abrir auditoría" aria-label="Abrir auditoría" onClick={() => setView('audit')}><Bell size={18} /><span /></button><span className="topbar-divider" /><div className="topbar-user"><span className="user-avatar small-avatar">{initials || 'AC'}</span><span><strong>{user.displayName}</strong><small>{user.role}</small></span></div></div></header>
        <div className="page-content">{viewContent}</div>
        <footer className="app-footer"><span>ArchiCore ERP <span className="footer-separator">·</span> Núcleo operativo</span><span><span className="live-dot" /> API conectada <span className="footer-separator">·</span> v0.1.0</span></footer>
      </main>
    </div>
  );
}
