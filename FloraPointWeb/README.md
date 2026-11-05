# FloraPointWeb

Aplikacja internetowa FloraPoint: mapa występowania roślin, statystyki, zgłaszanie fałszywych zdjęć i panel administratora. Konsumuje [`FloraPointAPI`](../FloraPointAPI).

## Stack

- [Vite](https://vite.dev/) + [React](https://react.dev/) + TypeScript
- [Tailwind CSS](https://tailwindcss.com/) — stylowanie
- [TanStack Query](https://tanstack.com/query) — komunikacja z API
- [React Router](https://reactrouter.com/) — routing
- [React-Leaflet](https://react-leaflet.js.org/) + OpenStreetMap — mapa
- [Recharts](https://recharts.org/) — statystyki/wykresy
- [Firebase Auth](https://firebase.google.com/docs/auth) — logowanie
- [Vitest](https://vitest.dev/) + [React Testing Library](https://testing-library.com/react) — testy

## Wymagania

- Node.js 20.19+ lub 22.12+ (sprawdź wersję: `node -v`)
- Działające lokalnie [`FloraPointAPI`](../FloraPointAPI) (patrz jego README/CLAUDE.md)

## Setup

```bash
cd FloraPointWeb
npm install
cp .env.example .env.local   # i wypełnij wartości (patrz komentarze w pliku)
npm run dev
```

## Skrypty

| Komenda                | Co robi                                                      |
| ---------------------- | ------------------------------------------------------------ |
| `npm run dev`          | serwer deweloperski z hot-reload                             |
| `npm run build`        | sprawdzenie typów (`tsc -b`) + build produkcyjny             |
| `npm run preview`      | podgląd zbudowanej wersji                                    |
| `npm run lint`         | lint ([oxlint](https://oxc.rs/docs/guide/usage/linter.html)) |
| `npm run format`       | formatowanie (Prettier)                                      |
| `npm run format:check` | sprawdzenie formatowania bez zmian (uruchamia to CI)         |
| `npm run test`         | testy jednostkowe (Vitest, jednorazowo)                      |
| `npm run test:watch`   | testy w trybie watch                                         |

## CI

`.github/workflows/web-ci.yml` uruchamia na każdy push/PR dotykający tego folderu: `lint`, `format:check`, `test`, `build`.
