import { AlertCircle, CheckCircle2, LoaderCircle } from 'lucide-react';

export function ErrorMessage({ children }: { children: string }) {
  return <div className="feedback feedback-error" role="alert"><AlertCircle size={17} />{children}</div>;
}

export function SuccessMessage({ children }: { children: string }) {
  return <div className="feedback feedback-success" role="status"><CheckCircle2 size={17} />{children}</div>;
}

export function LoadingState({ label = 'Cargando información…' }: { label?: string }) {
  return <div className="loading-state"><LoaderCircle className="spin" size={21} /><span>{label}</span></div>;
}
