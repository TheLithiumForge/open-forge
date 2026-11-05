# ADR 003: Distribute one HTML file

This record explains the single HTML distribution and the distinction between delivery and source organization.

## Context

A player should be able to save the game, open it later and still have the complete board, artwork and controls. A directory full of scripts, images and fonts creates several opportunities for a copied game to become incomplete. Moving just the HTML can lose a stylesheet; a remote font or sprite can stop loading away from the network. The original distribution discussion therefore focused on what a player actually receives.

The chosen artifact is `dist/index.html`. It contains the script, styles and artwork needed for play and can open directly from disk. The source remains a set of focused TypeScript, TSX and CSS files. Single-file delivery was never an argument for putting the whole implementation into one authored module. [Source layout](source-layout.md) documents the source arrangement.

## Build decision

Vite combines React transformation with `vite-plugin-singlefile`. The main configuration sets the target to `es2023` and disables module preload. A preload helper would have no useful chunks to fetch in the final inline page. The source comment records that reason, and `build-output.e2e.test.ts` inspects the HTML for the unwanted fetch and preload code.

The source `index.html` is an entry document containing a root element and a module reference to `src/main.tsx`. It is not itself the final self-contained game. Copying it before a build would preserve a development entry path rather than deliver the playable artifact. This distinction matters when reporting that an offline test passed.

## What the choice buys

An offline copy is straightforward to archive and pass between machines. Inline SVG faces need no image directory. The system font needs no font request. There is no runtime account or service configuration to carry alongside the file. These choices reinforce the artifact boundary; see [ADR 004](adr-004-no-backend.md), [ADR 007](adr-007-svg-faces.md) and [ADR 027](adr-027-offline-first.md).

Browser tests cover the built artifact, including opening from a file URL with network access blocked. This is more informative than observing that a development server happened to have everything cached. The browser test command builds current source before running, so its distribution evidence refers to the current implementation.

## Tradeoffs

A single HTML file gives up separate long-lived asset caches. An update replaces the complete artifact rather than just a changed script. For a small game with generated artwork and no remote resources, that cost was accepted in exchange for reliable copying and local opening. The bundle is still a real bundle, so package size and build output remain relevant; [ADR 040](adr-040-file-size-aim.md) discusses the compact-artifact aim.

Saved preferences are a separate concern. Storage behavior for file URLs varies between browsers. The project uses a local HTTP origin when checking persistence, while direct disk opening checks that the game plays. A usable offline artifact does not promise that every browser treats its file storage like a stable web origin.

The diagnostic face sheet and session harness also have single-file builds, but they are separate outputs under `.art` and `.session`. They are not extra files a player needs, and they are not embedded debugging routes inside the shipped page. [ADR 051](adr-051-test-artifacts.md) explains why that separation is useful.

## The distribution configuration

The same configuration used by the build keeps development watcher exclusions separate from the offline output settings.

```ts
import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';
import { viteSingleFile } from 'vite-plugin-singlefile';

export default defineConfig({
  plugins: [react(), viteSingleFile()],
  // One inline file has no chunks to preload, so leave out the preload helper and its fetch call.
  build: { target: 'es2023', modulePreload: false },
  server: {
    watch: {
      ignored: ['**/.browser-work/**', '**/.playwright-browsers/**', '**/.npm-cache/**'],
    },
  },
});
```
