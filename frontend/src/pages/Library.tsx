import { BookOpen } from 'lucide-react';
import { useEffect, useRef, useState } from 'react';
import AddDictionaryModal from '../Modals/AddDictionaryModal';
import DeleteDictionaryModal from '../Modals/DeleteDictionaryModal';
import DictionaryList from '../components/Dictionary/DictionaryList';

import type { LanguageSummary, DictionarySummary } from '../models/_index';

type Props = {
  languages: readonly LanguageSummary[];
  dictionaries: readonly DictionarySummary[]; 
  onCreate: (dictionary: DictionarySummary) => void; 
  onDelete: (id: string) => void 
};

export default function Library({ languages, dictionaries, onCreate, onDelete }: Props) {
  const dialogRef = useRef<HTMLDialogElement>(null);
  const deleteDialogRef = useRef<HTMLDialogElement>(null);
  const [selectedDictionary, setSelectedDictionary] = useState<DictionarySummary | null>(null);

  const openModal = () => dialogRef.current?.showModal();

  useEffect(() => { document.title = 'Library · Vocabularity'; }, []);
  useEffect(() => {
    if (selectedDictionary) deleteDialogRef.current?.showModal();
  }, [selectedDictionary]);

  return (
    <div className="page">
      <section className="page-heading">
        <span className="eyebrow"><BookOpen size={15} aria-hidden="true" /> YOUR WORDS, YOUR WORLD</span>
        <h1>Library<span>.</span></h1>
        <p>A place for every word you want to remember.</p>
      </section>
      <section aria-label="Your dictionaries">
        <DictionaryList dictionaries={dictionaries}
          onAdd={openModal} onDelete={setSelectedDictionary} />
      </section>
      <AddDictionaryModal 
        dialogRef={dialogRef} 
        languages={languages} 
        onCreate={(title, language) => {
        onCreate({
          id: crypto.randomUUID(),
          dictionary_name: title,
          language_id: language.id,
          icon: language.icon,
          words: [],
        });
      }} />
      <DeleteDictionaryModal 
        dialogRef={deleteDialogRef}
        dictionaryTitle={selectedDictionary?.dictionary_name ?? ''}
        onClose={() => setSelectedDictionary(null)}
        onConfirm={() => {
          if (!selectedDictionary) return;
          onDelete(selectedDictionary.id);
          setSelectedDictionary(null);
        }} />
    </div>
  );
}
