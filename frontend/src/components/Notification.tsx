import '../sass/components/Notification.sass';
import { Check, BanIcon, ChevronDown } from 'lucide-react';

 type Props = {
  isSuccessfull: boolean;
  closed: boolean;
  message: string;
  onClose: () => void;
};

export default function Notification({ isSuccessfull, closed, message, onClose }: Props) {
  if (closed) return null;

  const Icon = isSuccessfull ? Check : BanIcon;

  return (
    <div className={`notification notification--${isSuccessfull ? 'success' : 'error'}`}>
      <span className="notification-icon" aria-hidden="true"><Icon size={22} /></span>
      <div className="notification-content" role={isSuccessfull ? 'status' : 'alert'} aria-atomic="true">
        <strong>{isSuccessfull ? 'Success' : 'Oops! Something went wrong'}</strong>
        <p>{message}</p>
      </div>
      <button type="button" className="notification-close" onClick={onClose} aria-label="Close notification">
        <ChevronDown size={20} aria-hidden="true" />
      </button>
    </div>
  );
}
