export type LifecycleActionSpec = {
  fromStatus: string;
  endpoint: string;
  label: string;
  successStatus: string;
  confirmationMessage?: string;
};

type LifecycleActionsProps = {
  status: string;
  actions: readonly LifecycleActionSpec[];
  busy?: boolean;
  onAction: (action: LifecycleActionSpec) => void;
};

export function LifecycleActions({ status, actions, busy = false, onAction }: LifecycleActionsProps) {
  const available = actions.filter((action) => action.fromStatus === status);

  if (available.length === 0) {
    return <span className="lifecycle-empty">Sin acciones</span>;
  }

  return (
    <div className="lifecycle-actions" aria-label={`Acciones para el estado ${status}`}>
      {available.map((action) => (
        <button
          key={`${action.fromStatus}-${action.endpoint}`}
          type="button"
          className="lifecycle-action-button"
          disabled={busy}
          onClick={() => {
            if (action.confirmationMessage && !window.confirm(action.confirmationMessage)) return;
            onAction(action);
          }}
        >
          {busy ? 'Guardando…' : action.label}
        </button>
      ))}
    </div>
  );
}
