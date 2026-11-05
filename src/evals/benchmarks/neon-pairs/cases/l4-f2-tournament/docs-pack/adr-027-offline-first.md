# ADR 027: Make offline play the normal delivery path

This record explains offline operation beyond merely caching a previously visited web page.

## Context

A short browser game is useful in places where a network is intermittent or unavailable. Requiring a successful launch request, a remote font or an account check would make the game depend on infrastructure unrelated to matching cards. The distribution discussion therefore treated offline operation as a normal way to play, rather than a degraded mode after a website had loaded once.

The decision is a complete HTML artifact that can open directly from disk. Its script, CSS and generated SVG artwork travel together. The game uses a system font and has no runtime service, external asset or analytics request. Dependency installation can use a network; playing the delivered file does not.

## Why this is more than a cache

A service worker could have made a hosted application available after installation, but it would add lifecycle, cache invalidation and origin requirements. This game does not need that machinery to deliver one bounded page. A saved file can be opened without first visiting an origin or arranging an installation event.

The single-file plugin supplies the packaging, while the source remains modular. Disabling the module preload helper avoids runtime code whose purpose is fetching chunks that the final page does not have. [ADR 003](adr-003-single-file.md) and [ADR 005](adr-005-vite.md) explain those concrete build choices.

## Local state fits the model

Turn rules and logical time run locally. New Game chooses a seed in the browser and deterministic generation expands it. A board code needs no lookup service. Snapshot export produces text that can be retained manually, and replay is a local developer tool. These mechanisms keep reproducibility available when no host is reachable.

Preferences use the browser's local store when it is available. Direct file URLs have browser-dependent storage behavior, so persistence tests use a local HTTP origin. Offline play and stable-origin persistence are related but distinct claims. A failed store does not stop the in-memory game. [Private browsing notes](private-browsing-notes.md) and [Storage failure policy](storage-failure-policy.md) describe those limits.

## Alternatives

A hosted static page would be simple to deploy but would leave first launch dependent on a reachable origin. A progressive-web-app shell could add installation and caching features, but those features were not needed for the specified delivery path. A server-backed account could recover progress across devices, at the cost of turning a local game into a service.

The chosen model gives up automatic synchronization and central recovery. An unexported game is in memory; reloading starts a fresh deal. This is visible product behavior rather than a hidden fallback. The explicit snapshot route makes preserving progress a deliberate operation.

## Evidence

The browser delivery journey opens the built HTML from a file URL while network access is blocked. The build-output test also inspects the final HTML for unwanted fetch and preload code. Source checks guard against request APIs and external runtime assumptions. Each check covers a different route by which a supposedly offline page could still depend on a host.

A development-server success alone would not demonstrate this property, because that server can silently provide missing assets. Nor would an already warm browser cache prove the delivered artifact complete. Testing the actual built file is the useful boundary. [Offline delivery](offline-delivery.md), [ADR 038](adr-038-no-analytics.md) and [ADR 054](adr-054-acceptance-journeys.md) cover the supporting design and acceptance evidence.

## Inspecting the built artifact

This focused output check complements opening the file in a browser with networking blocked. It looks at the distribution rather than only authored imports.

```ts
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { test } from 'node:test';
import { GAME_URL } from './game-fixture.ts';

// Vite's module preload polyfill tests link relList support and fetches chunks. An inline page has
// no chunks, so neither belongs in it.
const NETWORK_CALL = 'fetch(';
const PRELOAD_POLYFILL = 'relList';

test('the built page carries no fetch call or module preload code', async () => {
  const html = await readFile(new URL(GAME_URL), 'utf8');
  assert.equal(html.includes(NETWORK_CALL), false, 'T10 the built page has no fetch call');
  assert.equal(html.includes(PRELOAD_POLYFILL), false, 'T10 the built page has no preload code');
});
```
