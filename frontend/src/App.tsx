import axios from 'axios';
import { lazy, Suspense, useEffect, useState } from 'react';
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import type { DictionarySummary } from './components/Dictionary/DictionaryList';
import type { WordSummary } from './components/Dictionary/Word'
import { Sidebar, Footer, Spinner, Notification } from './components/_index';
import {
  getDictionaries,
  createDictionary,
  updateDictionary,
  deleteDictionary,
  createWord,
  deleteWord
} from './api/dictionaryApi';
const Home = lazy(() => import('./pages/Home'));
const Library = lazy(() => import('./pages/Library'));
const Settings = lazy(() => import('./pages/Settings'));
const Dictionary = lazy(() => import('./pages/Dictionary'));


export default function App() {
  const [isSuccessfull, setStatus] = useState(true);
  const [dictionaries, setDictionaries] = useState<DictionarySummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [apiError, setApiError] = useState<string | null>(null);

  const [collapsed, setCollapsed] = useState(false);
  const [reducedMotion, setReducedMotion] = useState(() => {
    try { return localStorage.getItem('vocabularity.reducedMotion') === 'true'; }
    catch { return false; }
  });

  const closeNotification = () => {};

  async function fetchDictionaries() {
    const response = await getDictionaries();
    setDictionaries(response.data);
  }

  async function handleCreateDictionary(dictionary: DictionarySummary) {
    await createDictionary(dictionary);
  }

async function handleEditDictionary(
  dictionaryId: string,
  dictionaryTitle: string
) {
  await updateDictionary(dictionaryId, dictionaryTitle);
  await fetchDictionaries();
}

  async function handleDeleteDictionary(dictionaryId: string) {
    await deleteDictionary(dictionaryId);
  }

  async function handleCreateWord(
    dictionaryId: string,
    word: WordSummary
  ) {
    createWord(dictionaryId, word);
  }

  async function handleDeleteWord(
    dictionaryId: string,
    wordId: string
  ) {
    await deleteWord(dictionaryId, wordId);
  }

  useEffect(() => {
    document.documentElement.dataset.reducedMotion = String(reducedMotion);
    try { localStorage.setItem('vocabularity.reducedMotion', String(reducedMotion)); }
    catch { /* The preference still works when browser storage is unavailable. */ }
  }, [reducedMotion]);

  useEffect(() => {
    function closeMenu(event: KeyboardEvent) {
      if (event.key === 'Escape') setCollapsed(true);
    }
    window.addEventListener('keydown', closeMenu);
    return () => window.removeEventListener('keydown', closeMenu);
  }, []);

  async function loadDictionaries() { 
    setLoading(true);
    setApiError(null);
    try { await fetchDictionaries(); }
    finally { setLoading(false); }
  }

  useEffect(() => { void loadDictionaries(); }, [])

  return (
    <BrowserRouter>
  <div className={`app-shell ${collapsed ? 'is-collapsed' : ''}`}>
    <a href="#main-content" className="skip-link">
      Skip to content
    </a>

    <Sidebar
      collapsed={collapsed}
      onToggle={() => setCollapsed(value => !value)}
    />

    <div className="main-content">
      <main
        id="main-content"
        className="page-content"
        tabIndex={-1}
      >
        {apiError && <div className="page" role="alert">
          <p>{apiError}</p>
          <button type="button" className="modal-cancel" onClick={() => void loadDictionaries()}>Retry</button>
        </div>}
        <Suspense
          fallback={
            <div className="page-loading">
              <Spinner size="large" label="Loading page…" />
            </div>
          }
        >
          <Routes>
            <Route
              path="/"
              element={<Home />}
            />

            <Route
              path="/dictionary/:id"
              element={
                loading ? <div className="page-loading"><Spinner label="Loading dictionaries…" /></div> : <Dictionary
                  dictionaries={dictionaries}
                  onCreate={handleCreateWord}
                  onEdit={handleEditDictionary}
                  onDelete={handleDeleteWord}
                />
              }
            />

            <Route
              path="/dictionary"
              element={
                loading ? <div className="page-loading"><Spinner label="Loading dictionaries…" /></div> : <Library
                  dictionaries={dictionaries}
                  onCreate={handleCreateDictionary}
                  onDelete={handleDeleteDictionary}
                />
              }
            />

            <Route
              path="/settings"
              element={
                <Settings
                  reducedMotion={reducedMotion}
                  onReducedMotionChange={setReducedMotion}
                />
              }
            />

            <Route
              path="*"
              element={<Navigate to="/" replace />}
            />
          </Routes>
        </Suspense>
      </main>

      <Footer />

      <Notification 
        isSuccessfull={true}
        closed={false}
        message='test exeption'
        onClose={closeNotification}
      />
    </div>
  </div>
</BrowserRouter>
  );
}
