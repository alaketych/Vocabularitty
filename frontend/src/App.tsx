import { lazy, Suspense, useEffect, useState } from 'react';
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { Sidebar, Footer, Spinner, Notification } from './components/_index';
import DictionaryPages from './pages/DictionaryPages';
const Home = lazy(() => import('./pages/Home'));
const Settings = lazy(() => import('./pages/Settings'));

export default function App() {
  const [notification, setNotification] = useState({
    isSuccessfull: false,
    closed: true,
    message: '',
  });
  const [collapsed, setCollapsed] = useState(false);
  const [reducedMotion, setReducedMotion] = useState(() => {
    try { return localStorage.getItem('vocabularity.reducedMotion') === 'true'; }
    catch { return false; }
  });

  const showNotification = (
    isSuccessfull: boolean,
    message: string
  ) => {
    setNotification({
      isSuccessfull,
      closed: false,
      message,
    });
  };

  const closeNotification = () => {
    setNotification(state => ({
      ...state,
      closed: true
    }))
  };

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
                    <DictionaryPages key="words" 
                      detail
                      onNotification={showNotification} />
                  }
                />

                <Route
                  path="/dictionary"
                  element={
                    <DictionaryPages key="library"
                      onNotification={showNotification} />
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
            isSuccessfull={notification.isSuccessfull}
            closed={notification.closed}
            message={notification.message}
            onClose={closeNotification}
          />
        </div>
      </div>
    </BrowserRouter>
  );
}
