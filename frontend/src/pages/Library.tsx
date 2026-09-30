import { useSearchParams } from 'react-router-dom';
import axios from 'axios';
import * as api from '../api/dictionaryApi';
import { Spinner, Pagination } from '../components/_index';
import { BookOpen } from 'lucide-react';
import { useEffect, useRef, useState } from 'react';
import AddDictionaryModal from '../Modals/AddDictionaryModal';
import DeleteDictionaryModal from '../Modals/DeleteDictionaryModal';
import DictionaryList from '../components/Dictionary/DictionaryList';

import type { LanguageSummary, DictionarySummary } from '../models/_index';

type Props = { onNotification: (isSuccessfull: boolean, message: string) => void };

export default function Library({ onNotification }: Props) {
  const START_PAGE = 1;
  const PAGE_SIZE = 12;
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

  const dialogRef = useRef<HTMLDialogElement>(null);
  const deleteDialogRef = useRef<HTMLDialogElement>(null);
  const [selectedDictionary, setSelectedDictionary] = useState<DictionarySummary | null>(null);

  const openModal = () => dialogRef.current?.showModal();

  useEffect(() => { document.title = 'Library · Vocabularity'; }, []);
  useEffect(() => {
    if (selectedDictionary) deleteDialogRef.current?.showModal();
  }, [selectedDictionary]);

  useEffect(() => {
    const controller = new AbortController();
    setLoading(true);
    setError(null);
    async function load() {
      try {
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
      } catch (error) {
        if (!controller.signal.aborted) setError(errorMessage(error));
      } finally {
        if (!controller.signal.aborted) setLoading(false);
      }
    }
    void load();
    return () => controller.abort();
  }, [pageNumber, revision]);

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

  const onCreate = (dictionary: DictionarySummary) => void mutate(() => api.createDictionary(dictionary));
  const onDelete = (id: string) => void mutate(() => api.deleteDictionary(id), true);

  if (loading) return <div className="page-loading"><Spinner label="Loading page…" /></div>;
  if (error) return <div className="page" role="alert">
    <p>{error}</p>
    <button type="button" className="modal-cancel" onClick={() => setRevision(value => value + 1)}>Retry</button>
    <Pagination pageNumber={pageNumber} hasNext={!error && count === PAGE_SIZE}
        disabled={loading || saving} onChange={changePage} />
  </div>;

  return (
    <div className="page" aria-busy={saving}>
      <section className="page-heading">
        <span className="eyebrow"><BookOpen size={15} aria-hidden="true" /> YOUR WORDS, YOUR WORLD</span>
        <h1>Library<span>.</span></h1>
        <p>A place for every word you want to remember.</p>
      </section>
      <section aria-label="Your dictionaries">
        {count === 0 && pageNumber > 1 ? <p>No more dictionaries. Go back to the previous page.</p> :
          <DictionaryList dictionaries={dictionaries} onAdd={openModal} onDelete={setSelectedDictionary} />}
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
      <Pagination pageNumber={pageNumber} hasNext={!error && count === PAGE_SIZE}
        disabled={loading || saving} onChange={changePage} />
    </div>
  );
}
