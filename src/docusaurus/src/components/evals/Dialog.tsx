import { useEffect, useId, useRef, type ReactNode } from "react";

/**
 * A modal on the native dialog element: Escape and the close button close it, focus goes back to the control that opened
 * it, and narrow screens show it full screen. The content mounts only while it is open.
 */
export function Dialog({ open, onClose, title, subtitle, children }: {
  readonly open: boolean;
  readonly onClose: () => void;
  readonly title: ReactNode;
  readonly subtitle?: ReactNode;
  readonly children: ReactNode;
}) {
  const ref = useRef<HTMLDialogElement>(null);
  const opener = useRef<HTMLElement | null>(null);
  const titleId = useId();
  useEffect(() => {
    const dialog = ref.current;
    if (dialog === null) return;
    if (open && !dialog.open) {
      opener.current = document.activeElement instanceof HTMLElement ? document.activeElement : null;
      dialog.showModal();
    } else if (!open && dialog.open) dialog.close();
  }, [open]);
  function closed() {
    onClose();
    opener.current?.focus();
  }
  // A click on the dialog element itself lands on the backdrop, since the body fills the dialog.
  return <dialog ref={ref} className="eval-dialog" aria-labelledby={titleId} onClose={closed} onClick={(event) => { if (event.target === ref.current) ref.current.close(); }}>
    {open ? <div className="eval-dialog-body">
      <header className="eval-dialog-header">
        <div>
          <h2 id={titleId}>{title}</h2>
          {subtitle === undefined ? null : <p className="eval-muted">{subtitle}</p>}
        </div>
        <button type="button" className="eval-dialog-close" aria-label="Close" onClick={() => ref.current?.close()}>
          <svg viewBox="0 0 16 16" width="16" height="16" aria-hidden="true"><path d="M3 3l10 10M13 3L3 13" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" /></svg>
        </button>
      </header>
      {children}
    </div> : null}
  </dialog>;
}
