import { useId, useState, type RefObject } from 'react';
import { BookOpen } from 'lucide-react';
import Modal from './Modal';
import type { LanguageSummary } from '../models/_index';

type Props = {
  dialogRef: RefObject<HTMLDialogElement | null>;
  languages: readonly LanguageSummary[];
  onCreate: (title: string, language: LanguageSummary) => void;
};

export default function AddDictionaryModal({ dialogRef, languages, onCreate }: Props) {
  const id = useId();
  const [title, setTitle] = useState('');
  const [languageId, setLanguageId] = useState('');

  return (
    <Modal
      dialogRef={dialogRef}
      title="Add dictionary"
      description="Create a space for the words you want to remember."
      icon={<BookOpen size={25} />}
      footer={
        <>
          <button type="button" className="modal-cancel" onClick={() => dialogRef.current?.close()}>
            Cancel
          </button>
          <button type="submit" form={`${id}-form`} className="primary-button"
            disabled={!title.trim() || !languages.some(language => language.id === languageId)}>
            Create dictionary
          </button>
        </>
      }
    >
      <form id={`${id}-form`} className="dictionary-form" onSubmit={event => {
        event.preventDefault();
        const language = languages.find(option => option.id === languageId);
        if (!title.trim() || !language) return;
        onCreate(title.trim(), language);
        dialogRef.current?.close();
        setTitle('');
        setLanguageId('');
      }}>
        <div className="form-field">
          <label htmlFor={`${id}-title`}>Title</label>
          <input
            id={`${id}-title`}
            name="dictionary_name"
            type="text"
            placeholder="e.g. Everyday words"
            value={title}
            onChange={event => setTitle(event.target.value)}
            required
          />
        </div>
        <div className="form-field">
          <label htmlFor={`${id}-language`}>Language</label>
          <select
            id={`${id}-language`}
            name="language_id"
            value={languages.some(language => language.id === languageId) ? languageId : ''}
            onChange={event => setLanguageId(event.target.value)}
            disabled={languages.length === 0}
            required
          >
            <option value="" disabled>{languages.length ? 'Choose a language' : 'No languages available'}</option>
            {languages.map(language => (
              <option key={language.id} value={language.id}>
                {language.icon ? `${language.icon} ` : ''}{language.name}
                {language.original_name !== language.name ? ` — ${language.original_name}` : ''}
              </option>
            ))}
          </select>
        </div>
      </form>
    </Modal>
  );
}
