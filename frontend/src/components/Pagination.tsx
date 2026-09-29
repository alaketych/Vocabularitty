import { ChevronLeft, ChevronRight } from 'lucide-react';
import '../sass/components/Pagination.sass';

type Props = {
  pageNumber: number;
  hasNext: boolean;
  disabled?: boolean;
  onChange: (page: number) => void;
};

export default function Pagination({ pageNumber, hasNext, disabled, onChange }: Props) {
  return (
    <nav className="pagination" aria-label="Pagination">
      <button type="button" disabled={disabled || pageNumber === 1}
        onClick={() => onChange(pageNumber - 1)}>
        <ChevronLeft size={17} aria-hidden="true" /> Previous
      </button>
      <span role="status">Page {pageNumber} · 12</span>
      <button type="button" disabled={disabled || !hasNext}
        onClick={() => onChange(pageNumber + 1)}>
        Next <ChevronRight size={17} aria-hidden="true" />
      </button>
    </nav>
  );
}
