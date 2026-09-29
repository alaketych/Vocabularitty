import '../sass/components/Spinner.sass';

function Spinner({ size = 'medium', label = 'Loading…', className = '' }) {
  return (
    <div className={`spinner spinner--${size} ${className}`} role="status" aria-live="polite">
      <svg className="spinner-icon" viewBox="0 0 64 64" aria-hidden="true">
        <circle className="spinner-track" cx="32" cy="32" r="27" />
        <circle className="spinner-ring" cx="32" cy="32" r="27" />
        <path className="spinner-mark" d="m22 23 10 20 10-20" />
      </svg>
      <span className="spinner-label">{label}</span>
    </div>
  );
}

export default Spinner;
