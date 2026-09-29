import { Trash2 } from 'lucide-react';
import type { WordSummary } from '../../models/_index'

type Props = { word: WordSummary } & (
  | { variant?: 'preview'; onDelete?: never }
  | { variant: 'row'; onDelete?: () => void }
);

export default function Word({ word, variant = 'preview', onDelete }: Props) {
  const transcription = word.original_transcriptioned_word || <span aria-label="No transcription">—</span>;

  if (variant === 'row') {
    return (
      <tr className="word-data-row">
        <td>{word.original_word}</td>
        <td>{transcription}</td>
        <td className="word-translation-cell">
          {word.translated_word}
          {onDelete && (
            <button type="button" className="word-delete" onClick={onDelete}
              aria-label={`Delete ${word.original_word}`} aria-haspopup="dialog">
              <Trash2 size={18} aria-hidden="true" />
            </button>
          )}
        </td>
      </tr>
    );
  }

  return (
    <dl className="word">
      <div className="word-column">
        <dt>Word</dt>
        <dd className="word-original_word">{word.original_word}</dd>
      </div>
      <div className="word-column">
        <dt>Transcription</dt>
        <dd className="word-original_transcriptioned_word">
          {transcription}
        </dd>
      </div>
      <div className="word-column">
        <dt>Translation</dt>
        <dd className="word-translated_word">{word.translated_word}</dd>
      </div>
    </dl>
  );
}
