type StatusBadgeProps = { status: string };

export function StatusBadge({ status }: StatusBadgeProps) {
  const normalized = status.toLowerCase();
  const kind =
    ['active', 'operational', 'available', 'completed', 'counted', 'inspected', 'activo', 'activa', 'habilitado', 'procesado'].includes(normalized) ? 'positive'
      : ['suspended', 'maintenance', 'prospect', 'draft', 'planned', 'pendiente'].includes(normalized) ? 'neutral'
        : ['inactive', 'closed', 'archived', 'cancelled', 'finished', 'inactivo', 'inactiva', 'deshabilitado'].includes(normalized) ? 'muted'
          : 'neutral';

  return <span className={`status-badge status-${kind}`}>{status}</span>;
}
