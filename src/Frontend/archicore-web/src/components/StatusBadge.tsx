type StatusBadgeProps = { status: string };

export function StatusBadge({ status }: StatusBadgeProps) {
  const normalized = status.toLowerCase();
  const kind =
    ['active', 'operational', 'available', 'completed'].includes(normalized) ? 'positive'
      : ['suspended', 'maintenance', 'prospect', 'draft', 'planned'].includes(normalized) ? 'neutral'
        : ['inactive', 'closed', 'archived', 'cancelled', 'finished'].includes(normalized) ? 'muted'
          : 'neutral';

  return <span className={`status-badge status-${kind}`}>{status}</span>;
}
