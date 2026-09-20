---
open-forge:
  description: Find output catalogue
  tags: [Memory, Working, CLI, Task, Plan, G4, Find, Contextual, Active]
---

# 18 — find

> Read [00 — G4 conventions](00-conventions.md) first. Depends on
> [03](03-rendering-system.md) and [17](17-context.md).

## Goal

`find` prints one row per match and nothing else. Zero matches is one
sentence that repeats the query. Descriptions appear at `standard`, match
evidence and the search details at `full`. No TSV promise (C10).

## Depends on / Blocks

- Depends on: 03, 17. Lane C, after 17.
- Blocks: 40.

## Shape

Data. At `minimal` there is no headline; rows are the answer.

## Situations

`bare-inventory`, `one-tag`, `two-tags-all`, `heading`, `no-matches`,
`with-content-headings`, `include-selector`, `ambiguous-selector` (blocked),
`unreadable-source` (incomplete), `section-missing` (warnings), `invalid-selector`,
`invalid-require`.

## Statuses and text

| Status                  | When                                   | Text                                                                                                                               | Exit | Stream |
| ----------------------- | -------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | matches                                | rows                                                                                                                               |    0 | stdout |
| completed               | no matches                             | `No sources match <query>.` (`No sources match --tag Zzz.`; bare inventory empty: `No Markdown sources were found under .agents.`) |    0 | stdout |
| completed-with-warnings | requested section absent, ID collision | warning rows, blank line, match rows                                                                                               |    2 | stdout |
| incomplete              | a source could not be inspected        | warning rows, blank line, the safe rows; `standard` headline says `The search is incomplete.`                                      |    3 | stdout |
| invalid-input           | bad selector, part, require value      | `Cannot search: <problem>.`                                                                                                        |    4 | stderr |
| blocked                 | ambiguous or unsafe selector           | `Cannot search: <reason>.`                                                                                                         |    5 | stderr |
| failed                  | unexpected error                       | `Find stopped because of an unexpected error: <reason>.`                                                                           |    1 | stderr |
| cancelled               | Ctrl+C                                 | `Find was cancelled.`                                                                                                              |  130 | stderr |

## Text by level

`minimal`:

```text
memory                          .agents/memory/_memory.md
memory/archived                 .agents/memory/archived/_archived.md
memory/crystallized             .agents/memory/crystallized/_crystallized.md
```

Two columns, aligned with spaces, id then path. IDs may contain spaces, so
nothing promises that whitespace splits the row; JSON is the machine form.

`standard`:

```text
12 sources match --tag Memory.
memory                          .agents/memory/_memory.md                        Self-growing Markdown memory for active work, coordination, accepted knowledge, candidates, and history
memory/archived                 .agents/memory/archived/_archived.md             Useful history that no longer controls current work
```

`full` adds under each row the match evidence (`  matched tag Memory in
frontmatter`) and, after the rows, the search details: filters, require,
regions searched, source set, `21 of 21 sources inspected`.

`--content` parts print after the rows through the shared
`ContentPartsTextRenderer` from [17](17-context.md), with the same delimiter
form, at every level.

## Findings catalogue

| Code                         | Severity | Family                | Message                                                                              | Next                                |
| ---------------------------- | -------- | --------------------- | ------------------------------------------------------------------------------------ | ----------------------------------- |
| find.invalid-input           | error    | invalid-input         | examples: `--require must be all or any.`, `--within <value> is not a known region.` |                                     |
| find.invalid-selector        | error    | local                 | `--include <value> is not a source ID or a path under .agents.`                      | `open-forge route list --depth=all` |
| find.workspace-unavailable   | error    | workspace-unavailable |                                                                                      |                                     |
| find.workspace-unsafe        | error    | workspace-unsafe      |                                                                                      |                                     |
| find.selector-ambiguous      | error    | selector-ambiguous    |                                                                                      |                                     |
| find.selector-unsafe         | error    | selector-unsafe       |                                                                                      |                                     |
| find.identity-collision      | warning  | identity-collision    |                                                                                      |                                     |
| find.candidate-unsafe        | warning  | local                 | `<path> could not be checked safely and was skipped.`                                | `open-forge doctor`                 |
| find.layer-unresolved        | warning  | local                 | `<name>.overwrite.md has no base file and was skipped.`                              | `open-forge doctor`                 |
| find.inspection-unavailable  | warning  | local                 | `<path> could not be read and was skipped.`                                          | `open-forge doctor`                 |
| find.invalid-encoding        | warning  | local                 | `<path> is not valid UTF-8 and was skipped.`                                         | fix the file                        |
| find.frontmatter-unavailable | warning  | local                 | `The frontmatter of <path> could not be read, so its tags were not matched.`         | `open-forge doctor`                 |
| find.section-ambiguous       | warning  | local                 | `<path> has more than one section named <name>. None was returned.`                  | fix by hand                         |
| find.projection-missing      | warning  | local                 | `<path> has no section named <name>.`                                                | none                                |
| find.projection-unavailable  | warning  | local                 | `The <part> of <path> could not be produced: <reason>.`                              | `open-forge doctor`                 |
| find.operation-failed        | error    | operation-failed      |                                                                                      |                                     |
| find.interrupted             | error    | interrupted           |                                                                                      |                                     |

## Counts

`matches`, `sourcesInspected`, `sourcesCandidates`.

## Next rules

Invalid selector -> `open-forge route list --depth=all`; incomplete ->
`open-forge doctor`; otherwise none.

## JSON data by level

| Level    | `data`                                                                                                                      |
| -------- | --------------------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ matches: [ { id, path, description, parts: [ ... ] when requested } ] }`                                                 |
| standard | + per match `evidence: [ { kind: "tag" \| "heading", value, region, layer } ]`, `query { tags, headings, require, within }` |
| full     | + `sourceSet { mode, include: [...], exclude: [...], inspected, candidates }`                                               |

## References

- `src/cli/core/OpenForge.Cli.Core/Commands/Find/Shared/Rendering/*` — replaced by `Presentation/Find/`.
- Find interface Human Result View, Structured Result, Compact JSON Output (ledger only).

## Preconditions

- [ ] 03 and 17 merged.

## Steps

1. [ ] Write `FindReportSelector` and `FindDataTextRenderer` using `CliTable`
       and the shared content renderer.
2. [ ] Delete the old renderers and `FindTextEscaping`.
3. [ ] Regenerate snapshots and review.
4. [ ] Three suites green.

## Acceptance

- [ ] `minimal` with matches contains only rows.
- [ ] Zero matches is one sentence naming the query.
- [ ] No `result=` header line exists anywhere.

## Changes ledger

- file layout/type: the legacy Find renderers and bridge are deleted; native `Presentation/Find/` owns the selector, text renderer, data models, JSON context, wording and help. `Presentation/Legacy/Find/` holds no files.
- text: the legacy envelope and TSV output are replaced by aligned native rows carrying the catalogue headlines, descriptions, evidence, search details, warnings and Next actions.
- JSON member: the legacy `result` / `universe` / `presentation` / `projections` graph is replaced by typed `matches`; `standard` adds evidence and query, `full` adds `sourceSet` and the requested parts.
- file layout: **TSV output is retired.** The obsolete `__snapshots__/Query` fixtures are deleted and no `result=` header remains anywhere under `src/cli`. This is the largest behavioural retirement in the batch.
- snapshots: the 48 legacy captures under `Commands/Find/__snapshots__/` are replaced by **117** captures at the four native detail levels, text and JSON, across the 12 catalogue situations, under `src/cli/tests/integration/snapshots/FindBeforeOutputSnapshotTests/` — `bare-inventory`, `one-tag`, `two-tags-all`, `heading`, `no-matches`, `with-content-headings`, `include-selector`, `ambiguous-selector`, `unreadable-source`, `section-missing`, `invalid-selector`, `invalid-require`. The worker recorded, per situation, which catalogue rule requires what each level shows.
- test: four legacy rendering suites are deleted — `FindHumanRenderingTests` (732 lines), `FindJsonRenderingTests` (605), `FindDiagnosticsAndHelpTests` (299) and `FindHumanViewTests` (45) — and replaced by `FindPresentationTests` (163) plus the reviewed captures. That is the 99-test unit reduction: per-assertion legacy tests traded for catalogue-reviewed evidence, not lost coverage.
- test: `PublishedFindProcessTests` was migrated by the worker and passes 5/5 against the installed binary.
- layering: the worker's own architecture check found two `Presentation` imports of `Commands.Find`; both were replaced with local mappings. The imports that remain are `Commands.Find.Models.*` from `Presentation/Find/`, which `LayerBoundaryTests` permits for the owning command.
- evidence, overseer: re-run on the merge commit, not taken from the worker's report. Unit 3,267 passed, 0 failed, 0 skipped; integration 2,218 total, 2,201 passed, 0 failed, 17 skipped; `FindBeforeOutputSnapshotTests` 1 passed; `PublishedFindProcessTests` 5 passed; `npm run check:dotnet` exactly the five documented errors.

## Divergences observed

1. **The capture is one `[Fact]` looping over scenarios, not a `[Theory]` with a
   case per situation.** All 12 situations are captured and reviewed, and the
   files on disk confirm it, so coverage is intact. The cost is granularity: a
   failure in the first scenario aborts the rest, and the suite reports
   `total: 1` for the whole command, which makes a silently dropped situation
   harder to notice in a summary. Every other converted command in this packet
   uses a per-case identity. **Consider normalising in
   [40](40-verification.md)**; not corrected here, because the captures are
   correct and rewriting the harness is outside this lane.

2. **A build workaround leaked a directory into the worktree, and the sandbox
   refused to remove it.** The worker generated `artifacts-task30/` and reported
   `Rejected(... pwsh.exe ... rejected: blocked by policy)` on two cleanup
   attempts. It was still untracked at closeout and would have entered the merge
   through `git add -A`; the overseer removed it before committing. Same class as
   [36](36-library-sync.md) divergence 4. The worker reported it rather than
   concealing it, which is the behaviour the packet asks for.

3. **`FindTextEscaping` did not exist.** The plan names it for deletion; there
   was nothing to delete. A named symbol that does not exist is a divergence.
   **Durable record to change:** this file's Steps.

4. **The prescribed build could not read the user NuGet configuration.** Same
   root cause as [24](24-route-update.md) divergence 3; isolated temporary build
   artifacts were used successfully. No product divergence.

5. **No product wording divergence remains.** Unlike most lanes in this batch,
   the worker reports the catalogue and the code agree on every message it
   touched, including the frozen `--require must be all or any.` text and the
   frozen invalid-selector error.

## Rollback

Restore the bridge registration for find.
