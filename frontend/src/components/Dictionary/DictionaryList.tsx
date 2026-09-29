import { Plus } from 'lucide-react';
import EmptyLibrary from '../EmptyLibrary';
import type { DictionarySummary } from '../../models/_index';
import DictionaryPreview from './DictionaryPreview';

type Props = {
    dictionaries: readonly DictionarySummary[];
    onAdd: () => void;
    onDelete: (dictionary: DictionarySummary) => void;
};

export default function DictionaryList({ dictionaries, onAdd, onDelete }: Props) {
    if (dictionaries.length === 0) {
        return <EmptyLibrary onAdd={onAdd} />;
    }

    return (
        <ul className="dictionary-list">
            {dictionaries.map(dictionary => (
                <li key={dictionary.id}>
                    <DictionaryPreview id={dictionary.id} title={dictionary.dictionary_name} icon={dictionary.icon} words={dictionary.words} onDelete={() => onDelete(dictionary)} />
                </li>
            ))}
            <li>
                <button type="button" className="dictionary-preview dictionary-create-card"
                    onClick={onAdd} aria-label="Add dictionary" aria-haspopup="dialog">
                    <Plus size={40} strokeWidth={1.5} aria-hidden="true" />
                </button>
            </li>
        </ul>
    );
}
