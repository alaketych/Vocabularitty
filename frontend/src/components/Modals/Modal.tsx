import { useId, type ReactNode, type RefObject } from 'react';
import { X } from 'lucide-react';

type ModalProps = {
  dialogRef: RefObject<HTMLDialogElement | null>;
  title: string;
  description?: string;
  icon?: ReactNode;
  children?: ReactNode;
  footer?: ReactNode;
  onClose?: () => void;
};

export default function Modal({ dialogRef, title, description, icon, children, footer, onClose }: ModalProps) {
  const id = useId();

  return (
    <dialog
      ref={dialogRef}
      className="modal"
      aria-labelledby={`${id}-title`}
      aria-describedby={description ? `${id}-description` : undefined}
      onClose={onClose}
      onKeyDown={event => {
        if (event.key === 'Escape') {
          event.preventDefault();
          event.stopPropagation();
          event.currentTarget.close();
        }
      }}
      onClick={event => {
        if (event.target === event.currentTarget) {
          const bounds = event.currentTarget.getBoundingClientRect();
          if (event.clientX < bounds.left || event.clientX > bounds.right ||
              event.clientY < bounds.top || event.clientY > bounds.bottom) {
            event.currentTarget.close();
          }
        }
      }}
    >
      <div className="modal-heading">
        {icon && <span className="modal-icon" aria-hidden="true">{icon}</span>}
        <button type="button" className="modal-close" aria-label={`Close ${title}`}
          onClick={() => dialogRef.current?.close()}>
          <X size={21} aria-hidden="true" />
        </button>
      </div>
      <h2 id={`${id}-title`}>{title}</h2>
      {description && <p id={`${id}-description`}>{description}</p>}
      {children}
      {footer && <div className="modal-actions">{footer}</div>}
    </dialog>
  );
}
