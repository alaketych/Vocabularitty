export default function EmptyLibrary({ onAdd }: { onAdd: () => void }) {

  return (
    <>
    <div
      className="empty-library"
      role="button"
      tabIndex={0}
      aria-label="Add dictionary"
      aria-haspopup="dialog"
      onClick={onAdd}
      onKeyDown={event => {
        if (event.key === 'Enter' || event.key === ' ') {
          event.preventDefault();
          onAdd();
        }
      }}
    >
      <div className="empty-library-art" aria-hidden="true">
        <svg className="book-illustration" viewBox="0 0 148 116" fill="none" aria-hidden="true">
          <ellipse cx="74" cy="102" rx="56" ry="7" fill="var(--soft-background-color)" />
          <rect x="32" y="18" width="27" height="79" rx="5" fill="var(--book-peach-color)" stroke="var(--book-peach-border-color)" transform="rotate(-13 32 18)" />
          <path d="m42 32 15-3M45 43l15-3M54 81l15-3" stroke="var(--book-peach-line-color)" strokeWidth="2" />
          <rect x="65" y="9" width="28" height="88" rx="5" fill="var(--primary-color)" />
          <path d="M73 22h12M73 29h12M73 80h12M73 87h12" stroke="var(--book-orange-line-color)" strokeWidth="2" />
          <rect x="99" y="30" width="22" height="67" rx="4" fill="var(--book-sage-color)" stroke="var(--book-sage-border-color)" />
          <path d="M105 41h10M105 47h10M105 84h10" stroke="var(--book-sage-line-color)" strokeWidth="2" />
          <path d="M18 52v10m-5-5h10M126 14v8m-4-4h8" stroke="var(--book-sparkle-color)" strokeWidth="1.5" strokeLinecap="round" />
        </svg>
      </div>
        <h2>Make room for new words.</h2>
        <p>Your dictionaries will live here.</p>
      </div>
    </>
  );
}
