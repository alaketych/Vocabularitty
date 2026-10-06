import * as api from '../api/dictionaryApi';
import { Spinner, Pagination } from '../components/_index';
import { useEffect, useState, useRef } from 'react';
import { Link, useParams, useSearchParams } from 'react-router-dom';
import { BookOpen, Trash2, Pencil } from 'lucide-react';
import { Word } from '../components/Dictionary/_index';
import Modal from '../modals/Modal';
import { WordSummary, DictionarySummary } from '../models/_index';
import usePagination from '../hooks/usePagination';
import { errorMessage } from '../utils/errorMessage';
import { PAGE_SIZE, START_PAGE } from '../constants/index';
import { useNotification } from '../contexts/NotificationContext';

export default function Dictionary() {
  const { showNotification } = useNotification();

  const { id } = useParams();
  const { pageNumber, changePage } = usePagination();

  const [dictionaries, setDictionaries] = useState<DictionarySummary[]>([]);
  const [count, setCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [revision, setRevision] = useState(0);
  const [saving, setSaving] = useState(false);

  const deleteDialogRef = useRef<HTMLDialogElement>(null);
  const [selectedWord, setSelectedWord] = useState<WordSummary | null>(null);

  const dictionary = dictionaries.find(item => item.id === id);
  const words = dictionary?.words ?? [];

  const [newWord, setNewWord] = useState({
    word: '',
    transcription: '',
    translation: '',
  });

  const [newDictionaryTitle, setNewDicitonaryTitle] = useState('');
  const [isEditingTitle, setIsEditingTitle] = useState(false);

  useEffect(() => {
    if (dictionary) {
      setNewDicitonaryTitle(dictionary.dictionary_name);
    }
  }, [dictionary]);

  useEffect(() => {
    document.title =
      `${newDictionaryTitle || 'Dictionary not found'} · Vocabularity`;
  }, [newDictionaryTitle]);

  useEffect(() => {
    if (selectedWord) {
      deleteDialogRef.current?.showModal();
    }
  }, [selectedWord]);

  const createWord = () => {
    if (!dictionary || !newWord.word.trim() || !newWord.translation.trim()) {
      return;
    }

    const word: WordSummary = {
      id: crypto.randomUUID(),
      original_word: newWord.word.trim(),
      original_transcriptioned_word:
        newWord.transcription.trim() || null,
      translated_word: newWord.translation.trim(),
      createdAt: new Date(),
      updatedAt: new Date(),
    };

    onCreate(dictionary.id, word);

    setNewWord({
      word: '',
      transcription: '',
      translation: '',
    });
  };

  const startEditingTitle = () => {
    if (!dictionary) return;

    setIsEditingTitle(true);
  };

  const saveTitle = () => {
    if (!dictionary || !newDictionaryTitle.trim()) return;

    const title = newDictionaryTitle.trim();

    setNewDicitonaryTitle(title);
    onEdit(dictionary.id, title);
    setIsEditingTitle(false);
  };

  useEffect(() => {
    const controller = new AbortController();

    setLoading(true);
    setError(null);

    async function load() {
      try {
        if (!id) throw new Error('Missing dictionary ID');

        const [dictionary, words] = await Promise.all([
          api.getDictionary(id, controller.signal),
          api.getWords(id, pageNumber, PAGE_SIZE, controller.signal),
        ]);

        if (controller.signal.aborted) return;

        setDictionaries([
          {
            ...dictionary,
            words: words.data,
          },
        ]);

        setCount(words.data.length);
      } catch (error) {
        if (!controller.signal.aborted) {
          setError(errorMessage(error));
        }
      } finally {
        if (!controller.signal.aborted) {
          setLoading(false);
        }
      }
    }

    void load();

    return () => controller.abort();
  }, [id, pageNumber, revision]);

  async function mutate(
    action: () => Promise<{
      data: api.OperationResponse | undefined;
    }>,
    deleting = false
  ) {
    setSaving(true);
    setError(null);

    try {
      const response = await action();

      if (response.data?.isSuccessfull === false) {
        showNotification(false, response.data.message);
        return;
      }

      showNotification(
        true,
        response.data?.message || 'Deleted successfully.'
      );

      if (
        deleting &&
        count === START_PAGE &&
        pageNumber > START_PAGE
      ) {
        changePage(pageNumber - START_PAGE);
      } else {
        setRevision(value => value + START_PAGE);
      }
    } catch (error) {
      showNotification(false, errorMessage(error));
    } finally {
      setSaving(false);
    }
  }

  const onCreate = (id: string, word: WordSummary) =>
    void mutate(() => api.createWord(id, word));

  const onEdit = (id: string, title: string) => {
    if (dictionary) {
      void mutate(() =>
        api.updateDictionary(
          id,
          title,
          dictionary.language_id
        )
      );
    }
  };

  const onDelete = (id: string, wordId: string) =>
    void mutate(
      () => api.deleteWord(id, wordId),
      true
    );

  if (loading) {
    return (
      <div className="page-loading">
        <Spinner label="Loading page…" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="page" role="alert">
        <p>{error}</p>

        <button
          type="button"
          className="modal-cancel"
          onClick={() =>
            setRevision(value => value + 1)
          }
        >
          Retry
        </button>

        <Pagination
          pageNumber={pageNumber}
          hasNext={!error && count === PAGE_SIZE}
          disabled={loading || saving}
          onChange={changePage}
        />
      </div>
    );
  }

  return (
    <div className="page" aria-busy={saving}>
      <Link to="/dictionary">
        ← Back to library
      </Link>

      <section className="page-heading">
        <div className="dictionary-title-row">
          <span
            className="dictionary-preview-icon"
            aria-hidden="true"
          >
            {dictionary?.icon ?? <BookOpen size={24} />}
          </span>

          {isEditingTitle ? (
            <div className="dictionary-title-editor">
              <input
                className="dictionary-title-input"
                aria-label="Dictionary title"
                value={newDictionaryTitle}
                onChange={e =>
                  setNewDicitonaryTitle(e.target.value)
                }
              />

              <button
                type="button"
                onClick={saveTitle}
                className="primary-button dictionary-title-save"
              >
                Save
              </button>
            </div>
          ) : (
            <>
              <h1>
                {newDictionaryTitle || 'Dictionary not found'}
              </h1>

              {dictionary && (
                <button
                  type="button"
                  onClick={startEditingTitle}
                  className="dictionary-title-edit"
                  aria-label="Edit dictionary title"
                >
                  <Pencil size={18} />
                </button>
              )}
            </>
          )}
        </div>

        {!dictionary && (
          <p>
            This dictionary is unavailable.
          </p>
        )}
      </section>

      {dictionary && (
        <form
          className="dictionary-word-form"
          onSubmit={event => {
            event.preventDefault();
            createWord();
          }}
        >
          <div className="word-table-scroll">
            <table
              className="word-table"
              aria-label="Dictionary words"
            >
              <thead>
                <tr>
                  <th scope="col">Word</th>
                  <th scope="col">Transcription</th>
                  <th scope="col">Translation</th>
                </tr>
              </thead>

              <tbody>
                {words.map(word => (
                  <Word
                    key={word.id}
                    word={word}
                    variant="row"
                    onDelete={() =>
                      setSelectedWord(word)
                    }
                  />
                ))}

                <tr className="word-input-row">
                  <td>
                    <input
                      aria-label="Word"
                      required
                      value={newWord.word}
                      onChange={e =>
                        setNewWord({
                          ...newWord,
                          word: e.target.value,
                        })
                      }
                      placeholder="Enter a word"
                    />
                  </td>

                  <td>
                    <input
                      aria-label="Transcription"
                      value={newWord.transcription}
                      onChange={e =>
                        setNewWord({
                          ...newWord,
                          transcription: e.target.value,
                        })
                      }
                      placeholder="Optional"
                    />
                  </td>

                  <td>
                    <input
                      aria-label="Translation"
                      required
                      value={newWord.translation}
                      onChange={e =>
                        setNewWord({
                          ...newWord,
                          translation: e.target.value,
                        })
                      }
                      placeholder="Enter translation"
                    />
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <div className="word-form-actions">
            <button
              type="submit"
              className="primary-button"
              aria-label="Add word"
              disabled={
                !newWord.word.trim() ||
                !newWord.translation.trim()
              }
            >
              + Add word
            </button>
          </div>
        </form>
      )}

      <Modal
        dialogRef={deleteDialogRef}
        title="Delete word ?"
        description={`Remove “${selectedWord?.original_word ?? ''}” from this dictionary?`}
        icon={<Trash2 size={25} />}
        onClose={() => setSelectedWord(null)}
        footer={
          <>
            <button
              type="button"
              className="modal-cancel"
              autoFocus
              onClick={() =>
                deleteDialogRef.current?.close()
              }
            >
              Cancel
            </button>

            <button
              type="button"
              className="primary-button"
              onClick={() => {
                if (!dictionary || !selectedWord) return;

                deleteDialogRef.current?.close();

                onDelete(
                  dictionary.id,
                  selectedWord.id
                );

                setSelectedWord(null);
              }}
            >
              Delete word
            </button>
          </>
        }
      />

      <Pagination
        pageNumber={pageNumber}
        hasNext={!error && count === PAGE_SIZE}
        disabled={loading || saving}
        onChange={changePage}
      />
    </div>
  );
}