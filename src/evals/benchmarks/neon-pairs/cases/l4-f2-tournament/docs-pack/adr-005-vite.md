# ADR 005: Use Vite for development and distribution

This record explains Vite’s role in transforming the source and producing the offline artifact.

## Context

The project needs a quick local browser loop, React and TypeScript transformation, CSS imports and a dependable production HTML file. Implementing a custom bundler pipeline would put effort into asset handling rather than game behavior. Vite supplies the build and development shell while the single-file plugin supplies the final packaging.

The main configuration imports `react` from `@vitejs/plugin-react`, `defineConfig` from `vite` and `viteSingleFile` from `vite-plugin-singlefile`. These are build-time facilities. The player does not run Vite, and the built game does not contact the development server.

## Decision

Use the main Vite configuration for both the development entry and production game build. `npm run dev` binds to `127.0.0.1`. `npm run build` first runs the compiler projects and then invokes `vite build`. The target is `es2023`. These details belong together because a successful transformation alone does not establish that the source satisfies its TypeScript checks.

The main build also sets `modulePreload: false`. In a distribution with inline code, the preload helper supplies unnecessary runtime machinery. The output check looks for both a fetch call and preload-related code in the built page. [ADR 003](adr-003-single-file.md) discusses the artifact contract; this record concerns the tool that makes it concrete.

## Separate entries for separate evidence

Artwork and session lifecycle tests need pages that a player does not need. `vite.art.config.ts` uses the `art` directory as its root and writes `.art`. `vite.session.config.ts` uses `art/session` and writes `.session`. Both retain React and single-file transformation. The production entry remains independent of those diagnostic builds.

That design avoids a query string or hash that secretly changes the shipped game into a harness. It also means the browser tests can compare a real card rendered in the game with the same card rendered in a face sheet. The outputs differ in purpose while sharing the actual component. [ADR 051](adr-051-test-artifacts.md) covers how those pages support review.

## Alternatives

A handwritten sequence of compiler and concatenation steps would have needed special treatment for JSX, CSS imports and inlining. A larger full-stack framework would have introduced routing and server assumptions beyond the game's scope. Using only a development server without a production build would have left the most important delivery behavior untested.

Vite was chosen as a narrow tool rather than as a place to host game logic. Deck generation does not depend on a Vite environment variable. The browser rules can run in Vitest or through Node replay without the development server. A future build-tool change would have to preserve the distribution and command behavior; the current game model is not tied to Vite APIs.

## Practical costs

The source has more than one build configuration, so a change to shared output assumptions needs attention across the game and diagnostic entries. Package pins and the lockfile make the chosen combination reviewable. Watcher exclusions for `.browser-work`, `.playwright-browsers` and `.npm-cache` prevent local verification work from becoming noisy development input.

Browser verification uses the current build rather than an earlier artifact left in `dist`. The script uses an AND chain, so a failed production build stops the browser command. That detail is small but central to the credibility of the test result. See [Build pipeline](build-pipeline.md), [ADR 018](adr-018-playwright.md) and [ADR 029](adr-029-pinned-versions.md).

## Diagnostic build example

The artwork entry is built with the same transformation family while retaining a distinct root and output directory.

```ts
import { fileURLToPath } from 'node:url';
import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';
import { viteSingleFile } from 'vite-plugin-singlefile';

export default defineConfig({
  root: fileURLToPath(new URL('./art', import.meta.url)),
  plugins: [react(), viteSingleFile()],
  build: { outDir: '../.art', emptyOutDir: true, target: 'es2023' },
});
```
