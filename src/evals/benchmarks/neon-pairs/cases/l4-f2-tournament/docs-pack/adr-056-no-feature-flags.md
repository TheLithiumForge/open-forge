# ADR 056: Keep one product behavior path

This record explains why the playable game has no feature-flag system and how test switches remain separate.

## Decision

The current game has one supported product behavior for each documented action. It does not choose between old and new turn rules through runtime feature flags. There is no flag client, remote configuration request or player-facing experimental-mode control in the application.

This follows the bounded local scope. A board code should identify the same deck, a snapshot should have the same meaning, and an activation should follow the stated rules. Hidden switches would multiply those interpretations and make a bug report harder to reconstruct unless every switch traveled with it.

## Why a flag system was unnecessary

Feature flags are useful when a deployed service needs staged rollout or different cohorts. Neon Pairs is distributed as a complete offline artifact. A person retains the file they have, and the project can build another revision. There is no runtime service coordinating a rollout across players.

A flag around a changed generator would be especially dangerous if the public code prefix stayed the same. The format version already provides the relevant boundary for generation changes. A flag around an import parser could likewise make the same JSON load on one copy and fail on another without an explicit format distinction. [Format versioning guide](format-versioning-guide.md) explains the existing mechanism.

## Branches that are not feature flags

The application naturally branches on state. A mismatch blocks activation; an imported game may be paused; a completed board shows a banner; reduced-motion preference changes effects. These branches express documented session and browser behavior. They are not alternate product experiments controlled by a rollout service.

Tests also have configuration switches. `NEON_PAIRS_NAMED_COMBINATION` asks the browser wrapper to verify exact runtime versions. `PLAYWRIGHT_CHROMIUM_EXECUTABLE` can select a browser executable. Those values are read in test infrastructure, not by the shipped game to change matching or formats. The source-conventions test limits environment reads to the runners that need them.

## Diagnostic builds are separate

The artwork sheet and session harness are separate entries with separate Vite configurations. They do not use a production flag to reveal hidden test controls inside the game. This keeps the distributed entry focused on documented controls and reduces the risk of shipping a diagnostic branch accidentally.

A hash that secretly switches the face sheet into a session harness would blur that boundary. The art test explicitly checks that such a hash does not change the page's role. [ADR 051](adr-051-test-artifacts.md) explains the evidence structure.

## Alternatives and costs

Keeping an old implementation behind a toggle can make rollback look convenient, but both branches then need coherent tests and format interpretation. A local development boolean is also easy to forget when building the final artifact. For this small game, normal source history supplies the record of retired approaches without keeping them executable in every copy.

The cost is that the game does not support selectively enabling unfinished features for different players. That capability is outside the current product and would require its own design if ever justified.

[ADR 016](adr-016-no-state-library.md), [ADR 027](adr-027-offline-first.md), [Source audit notes](source-audit-notes.md) and [ADR 052](adr-052-release-tags.md) explain the neighboring choices. The useful outcome is a reportable behavior path whose important variations are already named in game state, browser preferences and versioned data.

## Environment reads belong to runners

The source convention compares environment-read locations with the command and test runner needs. These are tooling controls, not product rollout flags.

```ts
test('source reads only the environment its command and test runners need', async () => {
  const reads = (await typescriptFiles()).flatMap((file) =>
    [...file.text.matchAll(ENVIRONMENT)].map((match) => `${file.path}:${match[1] ?? match[2]}`),
  );
  const runners = [
    'scripts/replay-cli.ts:INIT_CWD',
    'scripts/replay.test.ts:npm_execpath',
    'tests/browser-test.ts:EXECUTABLE_ENV',
    'tests/browser-test.ts:NAMED_COMBINATION_ENV',
    'tests/browser-test.ts:NPM_AGENT_ENV',
    'tests/browser-test.ts:TEMP',
    'tests/browser-test.ts:TMP',
  ];
  assert.deepEqual([...new Set(reads.map((read) => read.replaceAll('\\', '/')))].sort(), runners);
});
```
