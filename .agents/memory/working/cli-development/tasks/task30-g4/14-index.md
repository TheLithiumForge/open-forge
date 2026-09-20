---
open-forge:
  description: Index output catalogue
  tags: [Memory, Working, CLI, Task, Plan, G4, Index, Contextual, Active]
---

# 14 — index

> Read [00 — G4 conventions](00-conventions.md) first. Depends on
> [03](03-rendering-system.md), which migrates `index` as its golden slice.
> This task completes the catalogue and the situations.

## Goal

`index` says which Entries sections it rewrote, with entry counts, and names
the leaf file that blocks a rewrite. A dry run shows the plain diff at
`standard`. The counts always add up.

## Depends on / Blocks

- Depends on: 03 (golden slice), 05 (block boundary). Lane B, after 13.
- Blocks: 40.

## Shape

Change report.

## Situations

`all-current`, `one-stale`, `dry-run-one-stale`, `explicit-source`,
`folder-operand` (invalid), `unknown-source` (invalid), `blocked-malformed-leaf`,
`incomplete-unreadable-child`, `lock-held`, `write-failed-partial`, `cancelled`.

## Statuses and headlines

| Status                  | When                                                | Headline                                                                                       | Exit | Stream |
| ----------------------- | --------------------------------------------------- | ---------------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | nothing stale                                       | `Entries sections are current in all <M> files. Nothing to do.`                                |    0 | stdout |
| completed               | rewritten                                           | `Updated the Entries section in <N> of <M> files.`                                             |    0 | stdout |
| completed (dry run)     | changes planned                                     | `Would update the Entries section in <N> of <M> files.`                                        |    0 | stdout |
| completed-with-warnings | recovery bundle retained                            | headline + family row                                                                          |    2 | stdout |
| incomplete              | a routed file, its metadata, or recovery unreadable | `The Entries sections could not be rebuilt completely. Nothing was changed.` + limitation rows |    3 | stdout |
| invalid-input           | bad flag, folder operand, unknown source            | `Cannot index <operand>: <reason>.` / family                                                   |    4 | stderr |
| blocked                 | malformed leaf, unsafe target, lock, changed        | `Cannot rebuild the Entries section of <leaf's parent>.` + the leaf row                        |    5 | stderr |
| failed                  | after effects                                       | `Index stopped after <n> of <m> files.`                                                        |    1 | stderr |
| cancelled               | Ctrl+C                                              | `Index was cancelled. Nothing was changed.` or `... Stopped after <n> of <m> files.`           |  130 | stderr |

## Text by level

`minimal`, rewritten:

```text
Updated the Entries section in 2 of 21 files.
  .agents/memory/emerging/ideas/_ideas.md   0 -> 1 entries
  .agents/skills/_skills.md                 3 -> 4 entries
```

`minimal`, dry run:

```text
Would update the Entries section in 1 of 20 files.
  .agents/loader.md   8 -> 7 entries
No files were changed.
```

`minimal`, blocked (stderr):

```text
Cannot rebuild the Entries section of .agents/skills/_skills.md.
  Error  .agents/skills/pdf/SKILL.md:1:1  Frontmatter is invalid
         The frontmatter block is not closed.
  Nothing was written. The other 19 sections are current.
Next: open-forge doctor
```

`minimal`, folder operand (stderr):

```text
Cannot index .agents/memory/emerging/ideas/pricing: it is a folder, not a source.
Next: open-forge index memory/emerging/ideas/pricing
```

`standard` adds `Workspace:`, the unchanged files as one count line, and for
a dry run the diff of each changed section:

```text
--- .agents/loader.md  (Entries section)
- - [Reusable default shapes for code, files, APIs, documents, and other work](patterns/_patterns.md) - #LoadNow #Core #Pattern
```

Diff lines are authored content, written byte-exact through the authored span.
Blank list lines are not shown as `- ` lines.

`full` adds the selection (`loader roots` or the explicit sources), every
unchanged file as a row, and recovery facts in words.

## Findings catalogue

| Code                              | Severity | Family                      | Message                                                                                                     | Next                                                                             |
| --------------------------------- | -------- | --------------------------- | ----------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------- |
| index.invalid-input               | error    | invalid-input               |                                                                                                             |                                                                                  |
| index.invalid-source              | error    | local                       | `<operand> is a folder, not a source.` / `No source has the ID <operand>.` / `<path> is not under .agents.` | `open-forge index <id>` when derivable, else `open-forge route list --depth=all` |
| index.workspace-unavailable       | error    | workspace-unavailable       |                                                                                                             |                                                                                  |
| index.workspace-unsafe            | error    | workspace-unsafe            |                                                                                                             |                                                                                  |
| index.source-ambiguous            | error    | source-ambiguous            |                                                                                                             |                                                                                  |
| index.source-unsafe               | error    | source-unsafe               |                                                                                                             |                                                                                  |
| index.topology-ambiguous          | error    | local                       | `The routes under <source> could not be resolved to one structure: <reason>.`                               | `open-forge doctor`                                                              |
| index.target-unexposed            | error    | local                       | `<path> is routed but no parent lists it, so its entry has nowhere to go.`                                  | `open-forge doctor`                                                              |
| index.target-unsafe               | error    | target-unsafe               |                                                                                                             |                                                                                  |
| index.metadata-unsafe             | error    | metadata-unsafe             | the leaf row under the blocked headline                                                                     | edit the file                                                                    |
| index.generated-region-unsafe     | error    | generated-region-unsafe     |                                                                                                             |                                                                                  |
| index.workspace-lock-unavailable  | error    | workspace-lock-unavailable  |                                                                                                             |                                                                                  |
| index.target-changed              | error    | target-changed              |                                                                                                             |                                                                                  |
| index.recovery-conflict           | error    | recovery-conflict           |                                                                                                             |                                                                                  |
| index.discovery-incomplete        | warning  | local                       | `<path> could not be read, so the routes below it are unknown.`                                             | `open-forge doctor`                                                              |
| index.metadata-incomplete         | warning  | metadata-incomplete         |                                                                                                             |                                                                                  |
| index.projection-incomplete       | warning  | projection-unavailable      |                                                                                                             |                                                                                  |
| index.recovery-unavailable        | warning  | recovery-unavailable        |                                                                                                             |                                                                                  |
| index.recovery-artifact-retained  | warning  | recovery-artifact-retained  |                                                                                                             |                                                                                  |
| index.target-changed-during-apply | error    | target-changed-during-apply |                                                                                                             |                                                                                  |
| index.write-failed                | error    | write-failed                |                                                                                                             |                                                                                  |
| index.verification-failed         | error    | verification-failed         |                                                                                                             |                                                                                  |
| index.recovery-failed             | error    | recovery-failed             |                                                                                                             |                                                                                  |
| index.operation-failed            | error    | operation-failed            |                                                                                                             |                                                                                  |
| index.interrupted                 | error    | interrupted                 |                                                                                                             |                                                                                  |

## Effects wording

`<path>  <before> -> <after> entries` for a rewritten section; `unknown ->
<after>` when the section was missing or unreadable before. Dry run: same
row. Partial: `<path>  not started` / `<path>  final state unknown`.

## Counts

`filesChecked`, `filesUpdated`, `filesCurrent`. They satisfy
`checked = updated + current` in every result; a file whose outcome is
unknown counts as neither and is listed.

## Next rules

Invalid source -> the corrected command; blocked or incomplete ->
`open-forge doctor`; lock -> rerun; retained recovery -> `open-forge cleanup`;
completed -> none.

## JSON data by level

| Level    | `data`                                                                                                          |
| -------- | --------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ mode, changes: [ { path, before, after } ] }`                                                                |
| standard | + `diff` per change (lines with `+`/`-` prefixes), `unchanged: [ { path } ]`                                    |
| full     | + `selection { origin, scope, sources: [ { id, path } ] }`, `regions: [ every region with action and outcome ]` |

## References

- `src/cli/core/OpenForge.Cli.Core/Commands/Index/Shared/Rendering/*` — golden slice in 03.
- Index interface sections Human output, Structured output, Errors And Boundaries, Compact JSON Output (ledger only).
- Help `Notes` section wording about the Entries section (ledger only).

## Preconditions

- [ ] 03 merged. 05 merged.

## Steps

1. [ ] Complete `IndexReportSelector` against this catalogue (03 delivered the
       shape; this task delivers every situation).
2. [ ] Regenerate snapshots for every situation and review.
3. [ ] Add the counts invariant test (`checked = updated + current`).
4. [ ] Three suites green.

## Acceptance

- [ ] No-op `minimal` is one line.
- [ ] The blocked headline names the parent whose section cannot be rebuilt and the row names the leaf.
- [ ] Counts add up in every snapshot.

## Changes ledger

> Index's native presentation was already merged. This lane supplied the missing
> evidence: the capture, not the command.

- test: `IndexBeforeOutputSnapshotTests` matched six situations through the retired two-view capture; all six call sites now use `MatchDetails`, capturing `minimal`, `standard`, `full` and `debug` in text and JSON.
- test identity: the `GeneratedEntries` and `InvalidSelectionOrMetadata` theories shared one situation-only snapshot identity; each case now uses `testName: $"{nameof(Method)}_{situation}"`, so cases no longer see one another's files as missing.
- file layout: the 44 `.compact`/`.expanded` files under `Commands/Index/__snapshots__/` are replaced by **110** captures under `src/cli/tests/integration/snapshots/IndexBeforeOutputSnapshotTests/`, covering 11 situations at four levels. The retired tree holds no files.
- evidence: every situation now has four native detail levels in both streams, with debug diagnostics captured separately. The worker reviewed each level against this file's `Text by level` and `JSON data by level` rows and recorded the mapping per situation in its report.
- evidence, overseer: re-run on the merge commit, not taken from the worker's report. Unit 3,410 passed, 0 failed, 0 skipped; integration 2,244 total, 2,227 passed, 0 failed, 17 skipped; `PublishedIndexProcessTests` 3 passed, 0 failed; `npm run check:dotnet` exactly the five documented errors.

## Divergences observed

1. **This file's own ledger was wrong about the legacy renderer.** The prior
   entry recorded that Index had no legacy capture left to retire. The checkout
   still had six `Renderers.Match` call sites and the full 44-file legacy tree,
   so Index's per-level tables had never been evidenced. Both are now converted.
   A ledger that records work which was not done is the reason the four-level
   capture has to be checked per command rather than assumed; see
   [40](40-verification.md).

2. **`minimal` for a no-op is two lines, not one.** This file's acceptance says
   the `minimal` no-op output is one line; the `all-current` capture carries the
   headline plus the required `Workspace: <workspace>` echo. Both rules are in
   this file and they contradict each other. Native output was left unchanged
   and no capture was regenerated over it. **Maintainer decision:** whether the
   workspace echo is exempt from the one-line rule, or the acceptance line
   should read two.

3. **The sandbox cannot read the user's NuGet configuration.** The worker's
   first `dotnet build` failed reading the unauthorized user NuGet config; it
   completed the build through an isolated absolute-artifact path. Same cause as
   [24](24-route-update.md) divergence 3; no product divergence.

4. **Closeout was blocked, as expected for a worktree.** The managed worktree
   denied the task-record write and refused to create
   `.git/worktrees/<id>/index.lock`. The overseer wrote this ledger and
   committed.

## Rollback

Revert to the 03 golden-slice state.
