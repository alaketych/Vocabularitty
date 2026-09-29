# Vocabularity frontend

React + TypeScript, React Router, Vite, and Lucide SVG icons.

## Run

Use Node.js 24 LTS. From this folder:

```powershell
npm install
npm start
```

Open http://127.0.0.1:3000.

```powershell
npm run build
npm run preview
```

The build checks TypeScript and generates `dist/`. Production hosting must serve
`index.html` for client routes such as `/library` and `/settings`.

## Layout

### Reusable modal

`Modal` accepts a title, optional description, icon, content (`children`), footer,
and an `onClose` callback. Use a dialog ref to open or close it. Escape, the close
button, and clicking the backdrop dismiss it. Each instance has unique accessible labels.

```tsx
const dialogRef = useRef<HTMLDialogElement>(null);

return <>
  <button onClick={() => dialogRef.current?.showModal()}>Open</button>
  <Modal dialogRef={dialogRef} title="Settings">
    <p>Your page content goes here.</p>
  </Modal>
</>;
```

Import `useRef` from `react` and `Modal` from `src/components/Modal` using the
relative path from your page.

- `src/components/Sidebar.tsx`: hamburger control, branding, Library and Settings navigation.
- `public/vocabularity.svg`: custom editable V logo and favicon.
- `src/sass/app.sass`: #ff8000 sidebar, 15vw desktop width, responsive breakpoints,
  and 260ms hover transitions in both directions.
- `src/pages/Library.tsx`: empty library screen.
- `src/pages/Settings.tsx`: locally saved reduced-motion preference.

Library and Settings use [Lucide SVG icons](https://lucide.dev), distributed under
the ISC license included with the installed package. Fonts use Google Fonts with
system-font fallbacks. Keyboard focus, a skip link, and system reduced-motion preferences are supported.

This is the frontend shell. Authentication and backend dictionary integration are
not connected yet; no sample dictionaries are presented as real data.

The previous placeholder react-scripts package was replaced by Vite, and the React
entry point now uses createRoot.

Verified: TypeScript and production build; browser navigation between Library and
Settings; hamburger collapse/expand; desktop sidebar width of 216px at a 1440px
viewport (15%); orange background and 260ms transitions.

## Styles

The entry point is src/sass/app.sass. Application rules are in _application.scss,
SVG illustration tokens are in _illustration.sass, and all hex colors are defined
in _colors.sass. Use named palette variables when adding styles. The legacy
_normalize.sass is retained but not loaded, preserving the existing appearance.

## Run frontend with backend

Start the API from backend with:
`dotnet run --project src/Vocabularity.Api --configuration Release`
Then run `npm start` from frontend and open http://127.0.0.1:3000/dictionary.
The development frontend sends requests to /api; Vite proxies them to
http://localhost:5080. Restart Vite after changing its proxy configuration.
Production uses VITE_BACKEND_API (or /api with a hosting reverse proxy).
Dictionary reads also load the language flags and nested word lists.
Demo reads are anonymous; writes still require an authenticated Bearer token.
