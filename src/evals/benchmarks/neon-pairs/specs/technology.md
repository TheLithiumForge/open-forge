# Technology
Technology is the build and language specification for Neon Pairs.

Status: spec version 2.1.

The game runs offline from one HTML file. Browser controls and inline SVG provide the board, input and artwork.

## Stack

- T01 - Use React with TypeScript and DOM rendering. Use native buttons, a native select and CSS Grid. Use no component library.

- T02 - Pin these packages and keep the lockfile. The stable versions were checked on 31 October 2026:

| Package | Version | Job |
| --- | --- | --- |
| `react`, `react-dom` | `19.3.1` | Browser UI |
| `typescript` | `7.0.2` | Type checking |
| `@types/react`, `@types/react-dom` | `19.3.0` | React types |
| `vite` | `8.3.5` | Build |
| `@vitejs/plugin-react` | `6.1.2` | React build integration |
| `vite-plugin-singlefile` | `2.3.4` | One HTML distribution |
| `vitest` | `5.0.5` | Unit tests |
| `@playwright/test` | `1.64.1` | Browser tests |
| `@types/node` | `24.19.2` | Build and test types |

TypeScript 7 is a stable release, as the [release announcement](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0/) describes. The [React versions page](https://react.dev/versions) lists the stable React release family.

## Language

- T03 - Use Node.js 24.19 or later and npm. Keep browser and Node compiler projects separate. Use ESM, static imports and type-only imports for types.

Use Node `26.12.1`, npm `11.20.0` and Playwright `1.64.1` with Chromium `156.0.8078.4` as the named compatibility combination. Record the locked install, build, unit tests and browser journeys with those exact versions in the project README before calling the combination tested. Other supported versions need their own results.

- T04 - Enable TypeScript strictness, `noUncheckedIndexedAccess`, `exactOptionalPropertyTypes`, `verbatimModuleSyntax` and `erasableSyntaxOnly`. Keep type checking separate from bundling. Vite's [TypeScript support](https://vite.dev/guide/features.html#typescript) transpiles code without checking types.

- T05 - Model project data with exact types and validate unknown data where it enters the game. Use narrowing instead of `any`, suppressed diagnostics or unchecked casts. Derive state unions from their named values.

- T06 - Keep behavior and its code, styles and tests together. Aim for 100 lines per TypeScript file and keep authored TypeScript files at or below 200 lines. Split by behavior when a file grows. Use direct imports from the file that defines a behavior.

## Commands

- T07 - Provide these commands:

| Command | Result |
| --- | --- |
| `npm ci` | Install the locked dependencies |
| `npm run dev` | Serve the game locally |
| `npm run typecheck` | Check every compiler project |
| `npm test` | Run unit tests for the game rules |
| `npm run test:e2e` | Build from current source, then run browser checks in fresh contexts against that build |
| `npm run build` | Type check and emit `dist/index.html` |
| `npm run --silent replay -- PATH` | Read replay JSON with the output and exit behavior in C14 |

The browser command must fail if its build fails. It must never fall back to an earlier `dist`. Stop development servers and watchers you start before finishing the work.

## Distribution

- T08 - `dist/index.html` contains the script, styles and artwork needed for play. It opens directly from disk without another file or a network request. Use the system font. Keep browser zoom enabled.

- T09 - Serve the game from a local HTTP origin when checking saved preferences. Storage on a `file:` URL differs between browsers, as the [localStorage documentation](https://developer.mozilla.org/en-US/docs/Web/API/Window/localStorage) explains. If storage is unavailable, play continues for the current session.

- T10 - Keep dependencies in development and build tooling where possible. The game uses no service, account, external asset, analytics or API key. Dependency installation can use the network. Playing the distribution doesn't use it.

The [game specification](README.md) connects this stack to the other requirements.
