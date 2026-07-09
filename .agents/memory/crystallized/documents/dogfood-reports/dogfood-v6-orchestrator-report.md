---
open-forge:
  description: Accepted record of the dogfood v6 orchestrator evaluation of the enriched post-MVP seed
  tags: [Memory, Document, Record, CurrentTruth, Dogfood, Evidence, Orchestration, Seed]
---

# Orchestrator Dogfood V6 Report

Date: 2026-07-09
Workspace: `D:\Repositories\open-forge-dogfood-v6`
Seed source: `D:\Repositories\open-forge-seed` at `ade0011`
Seed baseline commit: `f69c06e Seed OpenForge dogfood v6`
Worker: `019f45e8-bae3-7dc3-9ef5-afc5da65addd` (`Linnaeus`)

## Summary

The improved seed is meaningfully better than v5C. It produced a worker implementation that matched the important product semantics: Bun + TypeScript, no runtime dependencies, handwritten CLI parsing, URL normalization, tag normalization, duplicate-add tag merging, corrupt JSON protection, and safer write-temp-then-rename persistence.

I would not call the seed perfect yet. V6 exposed two real process gaps:

- The worker used OS temp directories in tests, despite the workspace-only scope directive.
- The worker wrote its own dogfood report and incorrectly claimed no subagent facility existed, even though this run used a spawned subagent.

Product quality improved a lot. Orchestration-role clarity still needs tightening.

## Seed Changes Before V6

Before running v6, I upgraded `open-forge-seed` from a simple payload into a more post-MVP/brownfield-style seed:

- Added `rebuild-brief.md` to frame the project as a rebuild after an MVP.
- Added accepted decisions rejecting tempting but unwanted options:
  - no CLI framework
  - JSON over SQLite
  - small internal API
- Added analysis docs:
  - tooling and dependency tradeoffs
  - storage failure modes
- Added observations:
  - agent temptations
  - MVP edge cases
- Updated the payload README to point agents at the richer routes.

The seed payload validated cleanly through `open-forge extend`.

## Worker Result

The worker created a Bun/TypeScript CLI under `bookmarks/`:

- `bookmarks/src/core.ts`
- `bookmarks/src/storage.ts`
- `bookmarks/src/cli.ts`
- `bookmarks/src/index.ts`
- `bookmarks/test/core.test.ts`
- `bookmarks/test/cli.test.ts`
- `bookmarks/README.md`
- `bookmarks/package.json`
- `bookmarks/bun.lock`

It also modified:

- `.agents/memory/emerging/observations/agent-temptations.md`

And created:

- `DOGFOOD_V6_REPORT.md`

## Verification

I independently ran these from `bookmarks/`:

```powershell
bun install --frozen-lockfile
bun test
bun run typecheck
```

All passed.

Observed test result:

```text
12 pass
0 fail
```

I also ran a workspace-scoped smoke test with `BOOKMARKS_DATA_FILE` under `bookmarks/.tmp-smoke/bookmarks.json`. It confirmed:

- `https://Example.com` normalized to `https://example.com/`
- adding `https://example.com/` again updated the same bookmark
- tags merged into `[docs, reference, tools]`
- `list --tag DOCS` worked after tag normalization
- `search REF` worked
- `remove https://example.com` removed the normalized bookmark
- final JSON was `{ "bookmarks": [] }`

I removed the `.tmp-smoke` folder after verification.

## Product Review

Strong points:

- The implementation follows the seed's Bun + TypeScript decision.
- Runtime dependencies are empty; TypeScript is dev-only.
- No CLI framework was added.
- Core behavior is pure and testable.
- Storage is a separate adapter.
- URL normalization uses the platform `URL` parser.
- Only `http` and `https` URLs are accepted.
- Tags are trimmed, lowercased, de-duplicated, and sorted.
- Duplicate add merges tags rather than replacing them.
- Corrupt JSON produces a user-facing error and is not overwritten.
- Writes use temp-file-then-rename.
- README documents behavior, data shape, env override, write safety, and no locking.

Minor product caveats:

- `parseBookmarkStore` normalizes manually edited persisted data on read instead of rejecting non-canonical but valid-shaped data. This is reasonable for the seed, but worth noting.
- CLI integration tests call `runCli` directly rather than spawning the executable entrypoint. The test still covers command behavior, but it does not fully prove the bin/entrypoint path.

## Process Review

Good:

- The worker clearly absorbed the improved seed. The result reflects the new post-MVP docs, not just the original command list.
- It reported an initial typecheck failure and the fix.
- It admitted an accidental out-of-scope patch to `D:\Repositories\bookmarks`, then cleaned it up.
- I verified `D:\Repositories\bookmarks` no longer exists.

Bad:

- The accidental sibling-folder patch is a serious scope-control failure, even though it was corrected.
- Tests use `node:os.tmpdir()` in `bookmarks/test/cli.test.ts`, which writes outside the workspace during test runs. That conflicts with the seed's scope-control intent.
- The worker wrote `DOGFOOD_V6_REPORT.md`, which should be the orchestrator's job in this evaluation model.
- The worker report says "This runtime did not expose a subagent tool", which is false for the actual run. The worker likely meant that the subagent itself could not spawn another subagent, but the statement is misleading in the final artifact.

## Seed Assessment

Is the seed good enough now?

For product behavior: mostly yes. The extra decisions and analysis materially improved the implementation. Compared with v5C, the worker implemented the nuanced semantics that were previously missing.

For orchestration behavior: not quite. The seed needs sharper role boundaries:

- worker implements and reports command results in final response,
- orchestrator writes the dogfood report,
- tests and smoke checks must keep temp files under the workspace,
- if a worker is itself a subagent, it should not claim no subagent facility existed just because it cannot delegate further.

## Recommendations

Update the seed before the next iteration:

- Add an explicit "report ownership" note to `worker-implementation.md` and `final-review-report.md`.
- Strengthen `scope-control.md` and `test-contracts.md` to say tests must not use OS temp directories for this dogfood seed.
- Add an observation that corrected out-of-scope writes still count as compliance failures in evaluation.
- Consider requiring one executable-entrypoint smoke test, not only `runCli` direct tests.

## Verdict

V6 is a strong positive signal for OpenForge as a knowledge-routing system. The richer seed did what it was supposed to do: it made the worker behave as if it had learned from a previous MVP.

The conclusion is not "perfect." The conclusion is: product guidance is now strong; process guidance still needs one more hardening pass around role ownership and workspace isolation.
