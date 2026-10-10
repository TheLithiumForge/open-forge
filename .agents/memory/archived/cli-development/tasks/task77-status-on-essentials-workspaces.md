---
open-forge:
  description: "Historical record: Status stopped warning about the routes Essentials leaves out, released in beta11"
  tags: [Memory, Task, CLI, Status, Install, Contextual, Complete, Archived, Historical]
---

# Task 77: Status on Essentials workspaces

## Outcome

`status` on a freshly installed Essentials workspace reports `Open Forge is installed and current.` instead of four warnings.

The beta10 public smoke test found the defect, which beta9 already had. The maintainer asked for the fix and the next beta release on 2026-10-10.

Status: Task 77 “Status on Essentials workspaces” (phase 1/1): milestone 3/3 — complete and released in beta11 on 2026-10-10.

## Findings

- **Removed routes.** Status listed every payload entrypoint with an Entries region as an expected Entries target, including the Guidance, Maps, Templates and Archived Memory entrypoints that Essentials records as removed in `.agents/open-forge.json`. Each missing file became `Entries section is missing`.
- **Counts line.** At `standard`, Status printed the JSON counts as a raw line, such as `... 33.962701262597011467624232596 startup share ...`, repeating the lines above it. The contract's `standard` example has no counts line.
- **Negative additions.** A workspace that loads less than shipped printed `Added since shipped: -2 files, about -0.6k tokens`.

## Decisions

- The generated-navigation reader reads workspace settings and skips payload entrypoints that `FrameworkPayloadSelection.IncludesPath` excludes, the same selection Install and Update use. Unreadable settings keep the previous behavior. Doctor's reader uses only workspace sources and needs no change.
- Status shows the counts line at `full` only, like Find. The shared renderer keeps exact numeric counts, as its existing contract tests require.
- The difference line reads `Compared with shipped: 2 fewer files, about 0.6k fewer tokens`, uses `more` or `fewer` per part, drops a token part below 50 tokens, and is omitted when nothing visible differs.

## Evidence

- The Essentials and copied-Working health Integration tests now also require a completed Status with no warnings or errors.
- Status output snapshots are refreshed for the counts and difference changes.
- Manual check: a fresh Essentials workspace reports current, and deleting a selected entrypoint still reports findings.
- Gates at `badbaf461`. Managed Windows: Unit 4,539, Integration 2,980 with 17 declared platform skips, EndToEnd 330. Native AOT on Windows x64: all six `test:built` modes passed. Linux from a WSL clone: Unit 4,539, Integration 2,967 with 30 skips, EndToEnd 297 with 33 skips, every skip reason declared.

## Beta 11 release, 2026-10-10

- The fix branch was squashed into `develop` as `0ed21575d`, followed by the version change to `0.9.0-beta.11` in `1b7cd09ff`.
- [Build 38043268695](https://github.com/TheLithiumForge/open-forge/actions/runs/38043268695) qualified `1b7cd09ff` on all six platforms in parallel with the Release.
- Tag `v0.9.0-beta.11` at `1b7cd09ff` started [Release 38043270935](https://github.com/TheLithiumForge/open-forge/actions/runs/38043270935), which rebuilt and retested all six platforms and published all seven npm packages, with `latest` and `beta` pointing at `0.9.0-beta.11`, and the GitHub prerelease with six portable archives and `SHA256SUMS`.
- A public smoke test installed the exact npm version in a fresh directory: `--version` reported `0.9.0-beta.11`, and `status` on a fresh Essentials workspace reported `Open Forge is installed and current.` with `Compared with shipped: 2 fewer files, about 0.6k fewer tokens` at standard detail.

## Current state

Complete. Released in beta11.
