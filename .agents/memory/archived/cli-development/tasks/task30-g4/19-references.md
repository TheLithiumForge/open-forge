---
open-forge:
  description: References output catalogue
  tags: [Memory, CLI, Task, Plan, G4, References, Contextual, Archived, Historical]
---

# 19 — references

> Read [00 — G4 conventions](00-conventions.md) first. Depends on
> [03](03-rendering-system.md).

## Goal

`references` shows the authored links into and out of one source, with
locations, and says plainly when there are none and that Entries links are
not counted (C15). The 21-line inspection trace is gone from every level
below `full`.

## Depends on / Blocks

- Depends on: 03. Lane C, after 18.
- Blocks: 40.

## Shape

Data.

## Situations

`links-both`, `no-authored-links`, `out-only`, `in-only-with-include`,
`broken-outgoing`, `external-outgoing`, `unknown-source` (invalid),
`invalid-direction`, `include-with-out-only` (invalid), `ambiguous-source`
(blocked), `unreadable-source` (incomplete).

## Statuses and text

| Status                  | When                                                                           | Headline                                                                                               | Exit | Stream |
| ----------------------- | ------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------ | ---: | ------ |
| completed               | links exist                                                                    | `<path>` then rows                                                                                     |    0 | stdout |
| completed               | none                                                                           | `<path> has no authored links in or out. Entries links are not counted.` (direction-specific variants) |    0 | stdout |
| completed-with-warnings | a broken, malformed or unsupported outgoing link                               | `<path>` then rows with their state, then warning rows                                                 |    2 | stdout |
| incomplete              | a scanned file could not be read, or an Entries section could not be separated | rows plus warning rows; `standard` adds `The scan is incomplete.`                                      |    3 | stdout |
| invalid-input           | unknown source, bad direction, filters with out-only                           | `Cannot list references: <problem>.`                                                                   |    4 | stderr |
| blocked                 | ambiguous or unsafe source, selector or target                                 | `Cannot list references: <reason>.`                                                                    |    5 | stderr |
| failed                  | unexpected error                                                               | `References stopped because of an unexpected error: <reason>.`                                         |    1 | stderr |
| cancelled               | Ctrl+C                                                                         | `References was cancelled.`                                                                            |  130 | stderr |

## Text by level

`minimal`, links:

```text
.agents/memory/_memory.md
  in   .agents/loader.md:104:3
  in   .agents/maps/_maps.md:12:3
  out  :52:3   archived/_archived.md
  out  :53:3   crystallized/_crystallized.md
  out  :60:1   https://example.org/guide   not checked
  out  :61:3   ../old.md   missing
```

`in` rows name where the link is written. `out` rows give the location inside
the source and the destination as written, followed by a state word only when
the link is not fine: `missing`, `heading not found`, `not checked`
(external), `not followed` (unsupported kind).

`minimal`, none:

```text
.agents/patterns/_patterns.md has no authored links in or out. Entries links are not counted.
```

`standard` adds `Workspace:`, the resolved path after each `out` destination
(`-> .agents/memory/archived/_archived.md`), the counts sentence (`2 in, 4
out`), and the filters used for the incoming scan.

`full` adds the sources that were scanned for incoming links and the layer
of each occurrence.

## Findings catalogue

| Code                                    | Severity | Family                | Message                                                                                                                                | Next                                |
| --------------------------------------- | -------- | --------------------- | -------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------- |
| references.invalid-input                | error    | invalid-input         |                                                                                                                                        |                                     |
| references.invalid-source               | error    | unknown-source        | `No source has the ID <ref>.` / `<path> is not under .agents.`                                                                         | `open-forge route list --depth=all` |
| references.invalid-direction            | error    | local                 | `--direction must be in, out, or both.`                                                                                                | none                                |
| references.invalid-filter               | error    | local                 | `--include and --exclude apply to incoming links. Use --direction in or both.` / `<value> is not a source ID or a path under .agents.` | none                                |
| references.workspace-unavailable        | error    | workspace-unavailable |                                                                                                                                        |                                     |
| references.workspace-unsafe             | error    | workspace-unsafe      |                                                                                                                                        |                                     |
| references.source-ambiguous             | error    | source-ambiguous      |                                                                                                                                        |                                     |
| references.source-unsafe                | error    | source-unsafe         |                                                                                                                                        |                                     |
| references.selector-ambiguous           | error    | selector-ambiguous    |                                                                                                                                        |                                     |
| references.selector-unsafe              | error    | selector-unsafe       |                                                                                                                                        |                                     |
| references.identity-collision           | warning  | identity-collision    |                                                                                                                                        |                                     |
| references.physical-alias               | error    | local                 | `<path> and <path> resolve to the same physical file.` / if one or more paths are unavailable, the message says so without inventing a path | `open-forge doctor`                 |
| references.identity-unavailable         | warning  | local                 | `The identity of <path> could not be determined.` / if the path is unavailable, the message says so                                     | `open-forge doctor`                 |
| references.candidate-unsafe             | warning  | local                 | `<path> could not be scanned safely for incoming links.`                                                                               | `open-forge doctor`                 |
| references.layer-unresolved             | warning  | local                 | `<name>.overwrite.md has no base file and was not scanned.`                                                                            | `open-forge doctor`                 |
| references.inspection-unavailable       | warning  | local                 | `<path> could not be read and was not scanned.`                                                                                        | `open-forge doctor`                 |
| references.invalid-encoding             | warning  | local                 | `The link at <path>:l:c has an encoding that cannot be resolved.`                                                                      | fix by hand                         |
| references.generated-region-unavailable | warning  | local                 | `The Entries section of <path> could not be identified, so its links could not be told apart from authored links.`                     | `open-forge doctor`                 |
| references.destination-malformed        | warning  | local                 | row state `not a resolvable link`                                                                                                      | fix by hand                         |
| references.destination-unsupported      | warning  | local                 | row state `not followed`                                                                                                               | none                                |
| references.target-missing               | warning  | local                 | row state `missing`                                                                                                                    | `open-forge doctor`                 |
| references.fragment-missing             | warning  | local                 | row state `heading not found`                                                                                                          | `open-forge doctor`                 |
| references.target-unsafe                | error    | local                 | `The link at <path>:l:c points outside the workspace.`                                                                                 | fix by hand                         |
| references.target-ambiguous             | error    | local                 | `The link at <path>:l:c could point to more than one file.`                                                                            | fix by hand                         |
| references.target-unreadable            | warning  | local                 | row state `target could not be read`                                                                                                   | none                                |
| references.operation-failed             | error    | operation-failed      |                                                                                                                                        |                                     |
| references.interrupted                  | error    | interrupted           |                                                                                                                                        |                                     |

Row-state findings are rendered inline on the `out` row at `minimal` and as
finding rows only at `full`, so the same fact is not printed twice.

## Counts

`incoming`, `outgoing`, `sourcesScanned`.

## Next rules

Broken links -> `open-forge doctor`; invalid source -> the route list;
otherwise none.

## JSON data by level

| Level    | `data`                                                                                                                               |
| -------- | ------------------------------------------------------------------------------------------------------------------------------------ |
| minimal  | `{ source { id, path }, direction, incoming: [ { path, location } ], outgoing: [ { location, destination, resolvedPath, state } ] }` |
| standard | + `coverage { incoming, outgoing }`, `filters { include, exclude }`                                                                  |
| full     | + `scanned: [ { id, path, layer } ]`, per occurrence `layer`                                                                         |

## References

- `src/cli/core/OpenForge.Cli.Core/Commands/References/Shared/Rendering/*` — replaced by `Presentation/References/`.
- References interface Human output, Structured output, Compact JSON Output (ledger only).

## Preconditions

- [ ] 03 merged.

## Steps

1. [ ] Write `ReferencesReportSelector` and `ReferencesDataTextRenderer`.
2. [ ] Delete the old renderers and `ReferencesTextEscaping`.
3. [ ] Regenerate snapshots and review.
4. [ ] Three suites green.

## Acceptance

- [ ] No `Inspected:` line below `full`.
- [ ] The zero case names authored links and says Entries links are not counted.
- [ ] Every `out` row shows its destination as written.

## Changes ledger

> References' native presentation was already merged. This lane supplied the
> missing evidence: the capture, not the command.

- test: `ReferencesBeforeOutputSnapshotTests` captured through the retired two-view helper; it now uses `MatchDetails`, capturing `minimal`, `standard`, `full` and `debug` in text and JSON for all 11 situations.
- file layout: the 44 `.compact`/`.expanded` files under `Commands/References/__snapshots__/` are replaced by **106** captures under `src/cli/tests/integration/snapshots/ReferencesBeforeOutputSnapshotTests/`. The retired tree holds no files.
- evidence: the worker reviewed every situation at every level against this file's `Text by level` and `JSON data by level` rows, and recorded the mapping per level in its report — including that `debug` repeats `full`'s primary output and adds diagnostics only.
- evidence, overseer: re-run on the merge commit, not taken from the worker's report. Unit 3,410 passed, 0 failed, 0 skipped; integration 2,244 total, 2,227 passed, 0 failed, 17 skipped; `PublishedReferencesProcessTests` 3 passed, 0 failed; `npm run check:dotnet` exactly the five documented errors.

- catalogue: split the source-catalogue identity issues into
  `references.identity-collision` (shared family), `references.physical-alias`
  (error, local wording), and `references.identity-unavailable` (warning,
  local wording). Added integration fixtures for all three; the physical-alias
  fixture is skipped on hosts that cannot create file symbolic links.
- consequence wording: `references.candidate-unsafe` now says that the source's
  links were not included in the incoming scan; its crystallized contract was
  updated. The invalid-encoding and row-state candidates were left unchanged.

## Divergences observed

1. **`unreadable-source` does not print the sentence this file promises.** The
   `Text by level` row says `standard` adds `The scan is incomplete.`; the
   regenerated `standard` capture omits it. Native output was left unchanged and
   no capture was regenerated over the wording. **Maintainer decision:** whether
   the sentence should be emitted, or the row removed.

2. **The identity-collision wording mismatch is settled.** Before this split,
   the command emitted the title `Two sources share an identity` with the
   generic message `Cannot list references: The source catalogue retained an
   unresolved boundary.` The shared `identity-collision` family says `The ID
   <id> matches more than one file. Use the exact path.` The narrowed
   `references.identity-collision` finding now uses that shared sentence and
   exposes one candidate line per retained path at `full`; the new physical
   alias and identity-unavailable findings own their local messages. The
   internal catalogue cause remains available to debug diagnostics.

3. **The worker's one integration failure was not caused by this change, and it
   does not reproduce here.** It reported `2,252 total, 1 failed` from
   `LibraryOutputArtifacts.Dispose` during `CancelledBetweenRealApplicationStages`
   and called it unrelated. Verified on two independent grounds: its diff touches
   nothing outside `Commands/References` and its own snapshot tree, and that
   teardown is load-sensitive by construction — unguarded `File.Delete`,
   `Directory.Delete(recursive: false)` which throws on a non-empty directory,
   and `Assert` calls, with no `try`/`catch`. On the merge commit the full
   integration suite is 0 failed and
   `LibraryAttachBeforeOutputSnapshotTests` passes 12/12. **Belongs to
   [40](40-verification.md):** a `Dispose` that asserts and deletes unguarded
   will keep producing phantom failures on a loaded machine.

4. **The packet named the wrong helper.** It said the capture used
   `Renderers.Match`; this checkout uses `ReadCommandOutputCapture.MatchAsync`.
   The worker converted the helper actually present and said so. Overseer's
   packet defect, not a worker error.

5. **The retired tree's directories could not be removed recursively.** The
   sandbox safety layer refused a recursive directory removal, so the worker
   deleted all 44 tracked files individually; the tree holds no files. Empty
   directories are not tracked by git and disappear on checkout.

6. **Closeout was blocked, as expected for a worktree.** Task-record editing and
   Git staging were both refused and no workaround was attempted. The overseer
   wrote this ledger and committed.

7. **The invalid-encoding candidate was not changed.** The same finding code is
   emitted for invalid source bytes, where scanning stops, and for an
   unresolved link or target encoding, where the outgoing row remains and only
   resolution fails. A single consequence clause would not be truthful for
   both situations without a new code or result fact; both are outside this
   wording-only slice.

## Rollback

Restore the bridge registration for references.
