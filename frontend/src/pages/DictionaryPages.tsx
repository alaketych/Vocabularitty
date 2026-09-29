import axios from 'axios';
import { lazy, useEffect, useState } from 'react';
import { useParams, useSearchParams } from 'react-router-dom';
import * as api from '../api/dictionaryApi';
import { Spinner, Pagination } from '../components/_index';
import type { DictionarySummary, LanguageSummary } from '../models/_index'

const Library = lazy(() => import('./Library'));
const Dictionary = lazy(() => import('./Dictionary'));

type Props = {
  detail?: boolean;
  onNotification: (
    isSuccessfull: boolean,
    message: string
  ) => void;
};

export default function DictionaryPages({ detail = false, onNotification}: Props) {
  const START_PAGE = 1;
  const PAGE_SIZE = 12;
  const { id } = useParams();
  const [params, setParams] = useSearchParams();
  const requestedPage = Number(params.get('pageNumber') || START_PAGE);
  const pageNumber = Number.isSafeInteger(requestedPage) && requestedPage > 0 && requestedPage <= 178956970
    ? requestedPage : START_PAGE;
  const [dictionaries, setDictionaries] = useState<DictionarySummary[]>([]);
  const [languages, setLanguages] = useState<LanguageSummary[]>([]);
  const [count, setCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [revision, setRevision] = useState(0);
  const [saving, setSaving] = useState(false);

  function changePage(page: number) {
    setParams(previous => {
      const next = new URLSearchParams(previous);
      next.set('pageNumber', String(page));
      return next;
    });
  }

  function errorMessage(error: unknown) {
    if (!axios.isAxiosError(error)) {
      return 'Something went wrong. Please try again.';
    }

    return (
      error.response?.data?.message ??
      error.response?.data?.errorMessage ??
      error.response?.data?.ErrorMessage ??
      'Unable to load your data. Please try again.'
    );
  }

  useEffect(() => {
    const controller = new AbortController();
    setLoading(true);
    setError(null);
    async function load() {
      try {
        if (detail && id) {
          const [dictionary, words] = await Promise.all([
            api.getDictionary(id, controller.signal),
            api.getWords(id, pageNumber, PAGE_SIZE, controller.signal),
          ]);
          if (controller.signal.aborted) return;
          setDictionaries([{ ...dictionary, words: words.data }]);
          setCount(words.data.length);
        } else {
          const result = await api.getDictionaries(pageNumber, PAGE_SIZE, controller.signal);
          const languageOptions: LanguageSummary[] = [];
          for (let languagePage = 1; ; languagePage++) {
            const result = await api.getLanguages(languagePage, 100, controller.signal);
            languageOptions.push(...result.data);
            if (result.data.length < result.pageSize) break;
          }
          if (controller.signal.aborted) return;
          setLanguages(languageOptions);
          setDictionaries(result.data);
          setCount(result.data.length);
        }
      } catch (error) {
        if (!controller.signal.aborted) setError(errorMessage(error));
      } finally {
        if (!controller.signal.aborted) setLoading(false);
      }
    }
    void load();
    return () => controller.abort();
  }, [detail, id, pageNumber, revision]);

  async function mutate(action: () => Promise<{ data: api.OperationResponse | undefined }>, deleting = false) {
    setSaving(true);
    setError(null);
    try {
      const response = await action();
      if (response.data?.isSuccessfull === false) {
        onNotification(false, response.data.message);
        return;
      }
      onNotification(true, response.data?.message || 'Deleted successfully.');
      if (deleting && count === START_PAGE && pageNumber > START_PAGE) changePage(pageNumber - START_PAGE);
      else setRevision(value => value + START_PAGE);
    } catch (error) {
      onNotification(false, errorMessage(error));
    } finally {
      setSaving(false);
    }
  }

  return (
    <>
      {error && <div className="page" role="alert">
        <p>{error}</p>
        <button type="button" className="modal-cancel" onClick={() => setRevision(value => value + START_PAGE)}>Retry</button>
      </div>}
      {loading ? <div className="page-loading"><Spinner label="Loading page…" /></div> : !error && (
        <div aria-busy={saving}>
          {count === 0 && pageNumber > START_PAGE ? (
            <div className="page"><p>No more {detail ? 'words' : 'dictionaries'}. Go back to the previous page.</p></div>
          ) : detail ? (
            <Dictionary 
              dictionaries={dictionaries}
              onCreate={(id, word) => void mutate(() => api.createWord(id, word))}
              onEdit={(id, title) => void mutate(() => api.updateDictionary(id, title,
                dictionaries.find(dictionary => dictionary.id === id)!.language_id))}
              onDelete={(id, wordId) => void mutate(() => api.deleteWord(id, wordId), true)} />
          ) : (
            <Library 
              languages={languages}
              dictionaries={dictionaries}
              onCreate={dictionary => void mutate(() => api.createDictionary(dictionary))}
              onDelete={id => void mutate(() => api.deleteDictionary(id), true)} />
          )}
        </div>
      )}
      <Pagination 
        pageNumber={pageNumber} 
        hasNext={!error && count === PAGE_SIZE}
        disabled={loading || saving} 
        onChange={changePage} />
    </>
  );
}
