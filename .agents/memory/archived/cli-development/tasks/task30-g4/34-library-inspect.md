---
open-forge:
  description: Library inspect output catalogue
  tags: [Memory, CLI, Task, Plan, G4, Library, Contextual, Archived, Historical]
---

# 34 — library inspect

> Read [00 — G4 conventions](00-conventions.md) first. Depends on
> [03](03-rendering-system.md).

## Goal

`library inspect` compares the source folder with the registered links and
says in one sentence whether they match, then names every file that was
added, removed, missing or changed.

## Depends on / Blocks

- Depends on: 03, 33. Lane F.
- Blocks: 35.

## Shape

Report.

## Situations

`current`, `added-source-files`, `retired-source-files`, `missing-links`,
`changed-links`, `empty-source`, `source-unreadable` (incomplete),
`record-invalid` (incomplete),
`unknown-id` (invalid), `no-ownership-record` (info), `invalid-id`, `blocked-mapping`.

## Statuses and headlines

| Status                  | When                                             | Headline                                                                                         | Exit | Stream |
| ----------------------- | ------------------------------------------------ | ------------------------------------------------------------------------------------------------ | ---: | ------ |
| completed               | inventory equals the links                       | `<id> is current: <N> files from <source> are linked under <destination>.`                       |    0 | stdout |
| completed               | empty source and no links                        | `<id> is current. The source folder <source> has no eligible files and no links are registered.` |    0 | stdout |
| completed               | no ownership record                              | `No ownership record exists, so <id> cannot be inspected.`                                       |    0 | stdout |
| completed-with-warnings | additions, retirements, missing or changed links | `<id> needs a sync: <K> files differ between <source> and <destination>.`                        |    2 | stdout |
| incomplete              | source or record could not be scanned completely | `<id> could not be inspected completely: <limitation>.`                                          |    3 | stdout |
| invalid-input           | bad or unknown ID                                | `Cannot inspect <ref>: <problem>.`                                                               |    4 | stderr |
| blocked                 | unsafe mapping, containment or link              | `Cannot inspect <id>: <reason>.`                                                                 |    5 | stderr |
| failed                  | unexpected error                                 | `Library inspect stopped because of an unexpected error: <reason>.`                              |    1 | stderr |
| cancelled               | Ctrl+C                                           | `Library inspect was cancelled.`                                                                 |  130 | stderr |

## Text by level

`minimal`, attention:

```text
team-knowledge needs a sync: 3 files differ between shared/team and docs.
  Warning  docs/new-a.md      not linked yet; new in the source folder
  Warning  docs/old.md        linked, but its source file is gone
  Warning  docs/review.md     missing
Next: open-forge library sync team-knowledge --dry-run
```

`standard` adds `Workspace:` and every file with its relation (`current`,
`new in the source folder`, `source file gone`, `missing`, `changed`).

`full` adds expected and observed link targets and the inventory count.

## Findings catalogue

| Code                                    | Severity | Family                | Message                                                                              | Next                                     |
| --------------------------------------- | -------- | --------------------- | ------------------------------------------------------------------------------------ | ---------------------------------------- |
| library-inspect.invalid-id              | error    | local                 | `<value> is not a valid Library ID. Use lowercase letters, digits and hyphens.`      | `open-forge library list`                |
| library-inspect.unknown-id              | error    | unknown-id            |                                                                                      | `open-forge library list`                |
| library-inspect.record-invalid          | warning  | local                 | `The Library section of .agents/open-forge.lock.json is invalid: <reason>.`          | `open-forge doctor`                      |
| library-inspect.record-unavailable      | warning  | lifecycle-unavailable |                                                                                      |                                          |
| library-inspect.record-blocked          | error    | lifecycle-blocked     |                                                                                      |                                          |
| library-inspect.ownership-observation   | info     | ownership-observation |                                                                                      |                                          |
| library-inspect.source-root-invalid     | error    | local                 | `The source folder <path> is not a folder inside the workspace.`                     | none                                     |
| library-inspect.source-root-unavailable | warning  | local                 | `The source folder <path> cannot be read, so the comparison could not finish.`       | none                                     |
| library-inspect.source-root-blocked     | error    | local                 | `The source folder <path> resolves to an unsafe location.`                           | none                                     |
| library-inspect.inventory-incomplete    | warning  | local                 | `Some files under <source> could not be listed, so the comparison could not finish.` | none                                     |
| library-inspect.path-added              | warning  | local                 | `<destination path>  not linked yet; new in the source folder`                       | `open-forge library sync <id> --dry-run` |
| library-inspect.path-retired            | warning  | local                 | `<destination path>  linked, but its source file is gone`                            | `open-forge library sync <id> --dry-run` |
| library-inspect.link-missing            | warning  | local                 | `<destination path>  missing`                                                        | `open-forge library sync <id> --dry-run` |
| library-inspect.link-changed            | warning  | local                 | `<destination path>  is no longer the link <id> created`                             | fix by hand                              |
| library-inspect.link-blocked            | error    | local                 | `<destination path>  could not be checked safely: <reason>`                          | `open-forge doctor`                      |
| library-inspect.operation-failed        | error    | operation-failed      |                                                                                      |                                          |
| library-inspect.interrupted             | error    | interrupted           |                                                                                      |                                          |

## Counts

`sourceFiles`, `linksCurrent`, `filesAdded`, `filesRetired`, `linksMissing`, `linksChanged`.

## JSON data by level

| Level    | `data`                                                                                                                                                              |
| -------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ id, sourceFolder, destinationFolder, current: bool, files: [ { sourcePath, destinationPath, relation } ] }` (only non-current files at minimal; all at standard) |
| standard | all files                                                                                                                                                           |
| full     | + per file `expectedTarget`, `observedTarget`, `inventory { eligible, excluded }`                                                                                   |

## References

- `src/cli/core/OpenForge.Cli.Core/Commands/Library/Inspect/Shared/Rendering/*` — replaced by `Presentation/Library/Inspect/`.
- Library inspect interface Output (ledger only).

## Preconditions

- [ ] 03, 33 merged.

## Steps

1. [ ] Write `LibraryInspectReportSelector` and `LibraryInspectDataTextRenderer`.
2. [ ] Delete the old renderers.
3. [ ] Regenerate snapshots and review.
4. [ ] Three suites green.

## Acceptance

- [ ] Current `minimal` is one line.
- [ ] Every differing file is a row at `minimal`.

## Changes ledger

> Recorded by the overseer from the worker's closeout and the committed diff
> (`3f91f419`). The worker could not write this file: its sandbox refused
> `.agents` and it reported that git could not create the worktree `index.lock`.
> Neither reproduced outside the worker, and its work was intact.

- file layout/type: the Inspect bridge under `Presentation/Legacy/Library/Inspect/Shared/Rendering/` is removed — `LibraryInspectHumanRenderer`, `LibraryInspectPathsHumanRenderer`, `LibraryInspectPresentation` and `LibraryInspectLegacyPresentation` are deleted. Native `Presentation/Library/Inspect/` now owns `LibraryInspectPresentation`, `Models/LibraryInspectData`, the selector, the text renderer, a files converter, the source-generated JSON context, help sections and wording. `CliLibraryComposer` closes the Inspect binding over the native rendering.
- message: the legacy header and trace are replaced by one headline per status and the catalogue's file rows. `minimal` states the Library, its state and the file counts; `standard` adds `Workspace:` and every file with its relation; `full` adds expected and observed link targets and the inventory count.
- JSON data: the legacy result graph became the native `data` graph, with per-detail file and relation shapes and the accepted `Next` guidance per status.
- counts: the source view publishes `ExcludedCount` so the output can report files the inventory excluded. `LibraryInspectOperation` fills it from `inventoryRead.ExcludedPaths.Length`, and `LibraryInspectSourceView` carries it. This is the command projecting a fact its own output needs; no observation behaviour changed.
- test retirement: the executable `LibraryInspectOutputSnapshotTests` class was deleted, which is the whole of the unit count change from 3,523 to 3,512. Its original Imprint snapshot files are preserved, matching the retirement recorded in [33](33-library-list.md). Unit, integration and published-process callers were migrated from the legacy graph to the native data graph.
- snapshots: `LibraryInspectBeforeOutputSnapshotTests` captures every situation at the four native detail levels in text and JSON under `src/cli/tests/integration/snapshots/LibraryInspectBeforeOutputSnapshotTests/`.
- evidence, worker: build 0 warnings and 0 errors; unit 3,512 passed, 0 failed, 0 skipped; integration 2,300 total, 0 failed, 17 skipped; the focused snapshot suite 11/11; the whitespace check reported exactly the five documented pre-existing errors.
- evidence, overseer after merging into `feature/render-improvements`: `npm run build` succeeded; unit 3,512 passed, 0 failed, 0 skipped; integration 2,300 total, 2,283 passed, 0 failed, 17 skipped.

- status or exit: malformed or duplicate Library records now return `incomplete` / exit 3 with `library-inspect.record-invalid`; an ownership observation still returns the existing complete result.
- message: record causes pass through the shared plain-English cause vocabulary, while the existing invalid-record catalogue message remains unchanged.
- snapshots: no Inspect capture was regenerated because its native snapshot situations contain no record-invalid case; boundary tests cover the status mapping.

## Divergences observed

- **Needs a maintainer decision.** A Library whose destination is the workspace root renders `1 file from shared/team is linked under ..` — the destination `.` followed by the sentence's own full stop. The catalogue's examples all use a named destination folder such as `docs`, so it does not say what a root destination should read as. The frozen wording was preserved rather than edited; the snapshot records the current sentence.
- The catalogue's examples show compact finding lines, while the shared renderer from [03](03-rendering-system.md) emits a title line and a message line per finding. The shared renderer wins; the catalogue examples predate it.
- Fixtures that pass an explicit `--workspace` show the `Workspace:` line at `minimal`. That is the shared workspace-echo rule (C12) in [00](00-conventions.md#shared-presentation-rules), not an Inspect decision.
- A source that cannot be read produces two inventory-incomplete findings. Both were preserved rather than collapsed. This is the same family of repeated observation that [19](19-references.md) fixed at its cause in `ReferencesFindingFactory`; Inspect has not been given the equivalent, and a later pass should decide whether one is warranted here.

- test, overseer: `PublishedLibraryInspectProcessTests` still asserted the legacy graph and was migrated after the merge. File rows are selected from `standard`, so the healthy journey now asks for `--detail=full` and checks `expectedTarget` equals `observedTarget`; the retired per-file `sourceId` member has no native equivalent. The invalid-id journey asserts the native `Cannot inspect ...` headline and its `Next:` line, and reads the finding code from a `--detail=full` run, because codes reach text only from full detail while JSON carries them at every level.

- Inspect already documented the incomplete headline for an unreadable record; the status/finding mapping and situation inventory were brought into alignment without changing that output template.

## Rollback

Restore the bridge registration for library inspect.
