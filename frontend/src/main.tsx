import React from 'react';
import { createRoot } from 'react-dom/client';
import App from './App';
import './sass/app.sass';
import { NotificationProvider } from './contexts/NotificationContext';

createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <NotificationProvider>
      <App />
    </NotificationProvider>
  </React.StrictMode>,
);