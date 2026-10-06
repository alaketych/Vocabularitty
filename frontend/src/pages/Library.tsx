import { BookOpen } from 'lucide-react';
import { useEffect, useRef, useState } from 'react';
import * as api from '../api/dictionaryApi';
import usePagination from '../hooks/usePagination';
import { errorMessage } from '../utils/errorMessage';
import { PAGE_SIZE, START_PAGE } from '../constants/index';
import { Spinner, Pagination, DictionaryList } from '../components/_index';
import { AddDictionaryModal, DeleteDictionaryModal } from '../modals/_index';
import type { LanguageSummary, DictionarySummary } from '../models/_index';
import { useNotification } from '../contexts/NotificationContext';

export default function Library() {
  const { showNotification } = useNotification();
  const { pageNumber, changePage } = usePagination();

  const [dictionaries, setDictionaries] = useState<DictionarySummary[]>([]);
  const [languages, setLanguages] = useState<LanguageSummary[]>([]);
  const [count, setCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [revision, setRevision] = useState(0);
  const [saving, setSaving] = useState(false);

  const dialogRef = useRef<HTMLDialogElement>(null);
  const deleteDialogRef = useRef<HTMLDialogElement>(null);

  const [selectedDictionary, setSelectedDictionary] =
    useState<DictionarySummary | null>(null);

  const openModal = () => {
    dialogRef.current?.showModal();
  };

  useEffect(() => {
    document.title = 'Library · Vocabularity';
  }, []);

  useEffect(() => {
    if (selectedDictionary) {
      deleteDialogRef.current?.showModal();
    }
  }, [selectedDictionary]);

  useEffect(() => {
    const controller = new AbortController();

    setLoading(true);
    setError(null);

    async function load() {
      try {
        const result = await api.getDictionaries(
          pageNumber,
          PAGE_SIZE,
          controller.signal
        );

        const languageOptions: LanguageSummary[] = [];

        for (let languagePage = 1; ; languagePage++) {
          const languageResult = await api.getLanguages(
            languagePage,
            100,
            controller.signal
          );

          languageOptions.push(...languageResult.data);

          if (languageResult.data.length < languageResult.pageSize) {
            break;
          }
        }

        if (controller.signal.aborted) return;

        setLanguages(languageOptions);
        setDictionaries(result.data);
        setCount(result.data.length);
      } catch (error) {
        if (controller.signal.aborted) return;

        const message = errorMessage(error);

        setError(message);
        showNotification(false, message);
      } finally {
        if (!controller.signal.aborted) {
          setLoading(false);
        }
      }
    }

    void load();

    return () => controller.abort();
  }, [pageNumber, revision]);

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
        response.data?.message || 'Operation completed successfully.'
      );

      if (
        deleting &&
        count === START_PAGE &&
        pageNumber > START_PAGE
      ) {
        changePage(pageNumber - START_PAGE);
      } else {
        setRevision(value => value + 1);
      }
    } catch (error) {
      showNotification(false, errorMessage(error));
    } finally {
      setSaving(false);
    }
  }

  const onCreate = (dictionary: DictionarySummary) => {
    void mutate(() => api.createDictionary(dictionary));
  };

  const onDelete = (id: string) => {
    void mutate(() => api.deleteDictionary(id), true);
  };

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
          onClick={() => setRevision(value => value + 1)}
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
      <section className="page-heading">
        <span className="eyebrow">
          <BookOpen size={15} aria-hidden="true" />
          YOUR WORDS, YOUR WORLD
        </span>

        <h1>
          Library<span>.</span>
        </h1>

        <p>A place for every word you want to remember.</p>
      </section>

      <section aria-label="Your dictionaries">
        {count === 0 && pageNumber > START_PAGE ? (
          <p>
            No more dictionaries. Go back to the previous page.
          </p>
        ) : (
          <DictionaryList
            dictionaries={dictionaries}
            onAdd={openModal}
            onDelete={setSelectedDictionary}
          />
        )}
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
        }}
      />

      <DeleteDictionaryModal
        dialogRef={deleteDialogRef}
        dictionaryTitle={selectedDictionary?.dictionary_name ?? ''}
        onClose={() => setSelectedDictionary(null)}
        onConfirm={() => {
          if (!selectedDictionary) return;

          onDelete(selectedDictionary.id);
          setSelectedDictionary(null);
        }}
      />

      <Pagination
        pageNumber={pageNumber}
        hasNext={count === PAGE_SIZE}
        disabled={loading || saving}
        onChange={changePage}
      />
    </div>
  );
}