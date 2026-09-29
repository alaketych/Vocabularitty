import { useId, type ReactNode } from 'react';
import { BookOpen, Trash2 } from 'lucide-react';
import { Word } from './_index';
import type { WordSummary } from './Word';
import { Link } from 'react-router-dom';

type Props = {
  id: string;
  title: string;
  icon?: ReactNode;
  words?: readonly WordSummary[];
  onDelete: () => void;
};


export default function DictionaryPreview({ id, title, icon, words = [], onDelete }: Props) {
  const titleId = useId();
  const previewWords = words.slice(0, 3);

  const deleteDictionary = () => {
    onDelete();
  };

  return (
    <article className="dictionary-preview" aria-labelledby={titleId}>
      <div className="dictionary-preview-overlay">
        <button
          type="button"
          className="dictionary-preview-delete"
          onClick={deleteDictionary}
          aria-label={`Delete ${title}`}
        >
          <Trash2 size={24} aria-hidden="true" />
        </button>
      </div>
      <header className="dictionary-preview-heading">
        <span className="dictionary-preview-icon" aria-hidden="true">
          {icon ?? <BookOpen size={24} />}
        </span>
        <h2 id={titleId}><Link className="dictionary-preview-link" to={`/dictionary/${encodeURIComponent(id)}`}>{title}</Link></h2>
      </header>
      {previewWords.length > 0 ? (
        <ul className="dictionary-preview-words" aria-label="Word preview">
          {previewWords.map(word => <li key={word.id}><Word word={word} /></li>)}
        </ul>
      ) : <p className="dictionary-preview-empty">No words yet.</p>}
    </article>
  );
}
