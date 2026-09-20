---
open-forge:
  description: Route list output catalogue
  tags: [Memory, Working, CLI, Task, Plan, G4, Route, Contextual, Active]
---

# 20 — route list

> Read [00 — G4 conventions](00-conventions.md) first. Depends on
> [03](03-rendering-system.md).

## Goal

`route list` prints one row per route with its ID and description, indented
to show the hierarchy, and one trailer line when the requested depth stopped
the listing. Paths, tags and provenance appear at higher levels. The
workspace path is never printed with doubled backslashes.

## Depends on / Blocks

- Depends on: 03. Lane D, first.
- Blocks: 21.

## Shape

Data. No headline at `minimal`; rows are the answer.

## Situations

`roots-depth-1`, `subtree`, `depth-all`, `depth-0`, `empty-subtree`,
`unknown-source` (invalid), `invalid-depth`, `ambiguous-source` (blocked),
`metadata-missing` (warnings), `unreadable-entrypoint` (incomplete),
`loader-malformed` (blocked).

## Statuses and text

| Status                  | When                                                          | Text                                                                                   | Exit | Stream |
| ----------------------- | ------------------------------------------------------------- | -------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | rows                                                          | rows, then the depth trailer when depth stopped the listing                            |    0 | stdout |
| completed               | no routes under the subject                                   | `No routes under <id>.` / `The Loader exposes no routes.`                              |    0 | stdout |
| completed-with-warnings | metadata missing or malformed on a listed route, ID collision | warning rows, blank line, rows                                                         |    2 | stdout |
| incomplete              | an entrypoint could not be read                               | warning rows, blank line, the confirmed rows; `standard`: `The listing is incomplete.` |    3 | stdout |
| invalid-input           | unknown source, bad depth, a second operand                   | `Cannot list routes: <problem>.`                                                       |    4 | stderr |
| blocked                 | ambiguous source or route, malformed Loader, unsafe path      | `Cannot list routes: <reason>.`                                                        |    5 | stderr |
| failed                  | unexpected error                                              | `Route list stopped because of an unexpected error: <reason>.`                         |    1 | stderr |
| cancelled               | Ctrl+C                                                        | `Route list was cancelled.`                                                            |  130 | stderr |

## Text by level

`minimal`:

```text
directives                       Required instructions loaded through selected routes
guidance                         Advice for recurring choices, tradeoffs, and work situations
  guidance/adaptive-collaboration  Explore ideas, match the depth to the decision, integrate accepted outcomes, and offer useful independent review
maps                             Concise maps to important local and external sources and when to use them
memory                           Self-growing Markdown memory for active work, coordination, accepted knowledge, candidates, and history
  memory/archived                Useful history that no longer controls current work
  memory/crystallized            Accepted knowledge that should remain current
  memory/emerging                Useful material that is not accepted yet
  memory/working                 Temporary memory that helps agents continue or resume active work
patterns                         Reusable default shapes for code, files, APIs, documents, and other work
skills                           Specialized capabilities provided through native SKILL.md packages
templates                        Copy-ready files for starting independently maintained workspace content
workflows                        Repeatable Markdown recipes for reaching a defined goal
13 routes to depth 1. Deeper routes: open-forge route list --depth=all
```

Child rows use the full ID so any row can be pasted into `route inspect`.
Descriptions are authored content and are never shortened. The trailer
appears only when at least one listed entrypoint has children beyond the
requested depth; it never states a count of hidden routes.

`standard` adds `Workspace:`, then under each row the path and the tags:

```text
guidance                         Advice for recurring choices, tradeoffs, and work situations
                                 .agents/guidance/_guidance.md   #LoadNow #Core #Guidance
```

`full` adds per row: parent, depth, kind (`entrypoint` or `file`), direct
child count, how the row was selected, and whether an overwrite file exists.

## Findings catalogue

| Code                                | Severity | Family                | Message                                                                                           | Next                                               |
| ----------------------------------- | -------- | --------------------- | ------------------------------------------------------------------------------------------------- | -------------------------------------------------- |
| route-list.invalid-source-reference | error    | invalid-input         | `<operand> is not a source ID or a path under .agents.`                                           | `open-forge route list --depth=all`                |
| route-list.unknown-source           | error    | unknown-source        |                                                                                                   |                                                    |
| route-list.invalid-depth            | error    | local                 | `--depth must be a whole number or all.`                                                          | none                                               |
| route-list.loader-subject           | error    | local                 | `The Loader is the root of every route. Run route list without an operand.`                       | `open-forge route list`                            |
| route-list.invalid-workspace        | error    | workspace-unavailable |                                                                                                   |                                                    |
| route-list.workspace-unavailable    | error    | workspace-unavailable |                                                                                                   |                                                    |
| route-list.ambiguous-source         | error    | source-ambiguous      |                                                                                                   |                                                    |
| route-list.route-ambiguous          | error    | route-ambiguous       |                                                                                                   |                                                    |
| route-list.unsafe-source            | error    | source-unsafe         |                                                                                                   |                                                    |
| route-list.physical-boundary        | error    | local                 | `<path> resolves outside the workspace and was not listed.`                                       | none                                               |
| route-list.loader-unavailable       | error    | local                 | `.agents/loader.md could not be read.`                                                            | `open-forge doctor`                                |
| route-list.loader-malformed         | error    | local                 | `.agents/loader.md has no usable Entries section.`                                                | `open-forge doctor`                                |
| route-list.unsupported-source       | error    | local                 | `<path> is not a routed Markdown source.`                                                         | none                                               |
| route-list.read-unavailable         | warning  | inspection-incomplete | `<path> could not be read, so the routes below it are not listed.`                                | `open-forge doctor`                                |
| route-list.metadata-missing         | warning  | local                 | row description shows `(no description)`; finding `<path> has no description in its frontmatter.` | `open-forge route update <id> --description "..."` |
| route-list.metadata-malformed       | warning  | local                 | `The frontmatter of <path> could not be read: <reason>.`                                          | fix by hand                                        |
| route-list.authored-form            | warning  | local                 | `<path> uses the compatibility entrypoint name <name>.`                                           | rename to `_<folder>.md`                           |
| route-list.identity-collision       | warning  | identity-collision    |                                                                                                   |                                                    |
| route-list.operation-failed         | error    | operation-failed      |                                                                                                   |                                                    |
| route-list.interrupted              | error    | interrupted           |                                                                                                   |                                                    |

## Counts

`routes`, `roots`, `depth`.

## Next rules

Unknown or invalid source -> `open-forge route list --depth=all`; unreadable
-> `open-forge doctor`; otherwise none.

## JSON data by level

| Level    | `data`                                                                                               |
| -------- | ---------------------------------------------------------------------------------------------------- |
| minimal  | `{ subject: { id, path } \| null, depth, rows: [ { id, path, description, tags, relativeDepth } ] }` |
| standard | same (path and tags are already present)                                                             |
| full     | + per row `parentId`, `absoluteDepth`, `kind`, `directChildren`, `selectedAs`, `hasOverwrite`        |

## References

- `src/cli/core/OpenForge.Cli.Core/Commands/Route/List/Shared/Rendering/*` — replaced by `Presentation/Route/List/`.
- `RouteListTextEscaping` — deleted (it doubled backslashes).
- Route list interface Human output and Structured output (ledger only).

## Preconditions

- [ ] 03 merged.

## Steps

1. [ ] Write `RouteListReportSelector` and `RouteListDataTextRenderer` using `CliTable`.
2. [ ] Delete the old renderers and escaper.
3. [ ] Regenerate snapshots and review.
4. [ ] Three suites green.

## Acceptance

- [ ] `minimal` for the stock roots is 14 lines.
- [ ] No `Coverage:`, `Selection:`, `Confirmed:` line at any level.
- [ ] Tags print as `#Tag` words, never as JSON arrays.

## Changes ledger

- file layout/type: the legacy Route List bridge and renderers are deleted, along with the legacy escaper; native `Presentation/Route/List/` owns the presentation. `Presentation/Legacy/Route/List/` holds no files.
- model: the presentation reached Framework-backed result state; it now reads a command-owned `Commands/Route/List/Models/Result/RouteListPresentationFacts`, which is what `LayerBoundaryTests` requires.
- text: legacy headers and path-first rows are replaced by aligned ID and description rows, with paths, tags, details and the depth trailer.
- text: legacy diagnostics are replaced by the catalogue finding wording, with native stdout/stderr routing.
- JSON member: the nested legacy coverage and findings graph is replaced by `subject`, `depth` and `rows`, with row details only at `full`.
- JSON member: the legacy `RouteListCompactJsonModels`, `RouteListJsonModels` and `RouteListJsonDepthConverter` are deleted; callers read native top-level `findings` and `data.depth`.
- text: finite-depth output lost its boundary child counts; `directChildren` and the trailer are retained.
- debug: the physical and workspace dump is replaced by bounded route-list diagnostics.
- snapshots: the 44 legacy captures under `Commands/Route/List/__snapshots__/` are replaced by **107** captures at the four native detail levels, text and JSON, across the 11 catalogue situations, under `src/cli/tests/integration/snapshots/RouteListBeforeOutputSnapshotTests/`.
- test: `PublishedRouteListProcessTests` was migrated by the worker and passes 3/3 against the installed binary. The worker also ran `PublishedShellBoundaryProcessTests` (26/26) unprompted, which is correct: that class reads Route List output through `data.rows`.
- evidence, overseer: re-run on the merge commit, not taken from the worker's report. Unit 3,261 passed, 0 failed, 0 skipped; integration 2,208 total, 2,191 passed, 0 failed, 17 skipped; `PublishedRouteListProcessTests` 3 passed; `PublishedShellBoundaryProcessTests` 26 passed; `npm run check:dotnet` exactly the five documented errors.
- strictness: metadata-missing and metadata-malformed findings now remain attention warnings, retain their routed rows, and use `(no description)` when no description is available; read-unavailable entrypoints remain incomplete.
- strictness: Loader-malformed selection findings now remain blocked, preserving the exit-5 boundary and the `Cannot list routes: <reason>.` headline.
- snapshots: regenerated the 20 `metadata-missing` and `loader-malformed` native captures; the `unreadable-entrypoint` captures were unchanged.
- evidence, worker: affected Route unit and integration classes passed; the full gates passed with unit 3,179/0/0 and integration 2,219 total/2,202 passed/0 failed/17 skipped. Whitespace retained the five documented pre-existing errors.

- message: Route List cause-bearing wording now delegates to the shared cause vocabulary; the three-level status ladder and its wording remain unchanged.
- snapshots: regenerated the owned Route List class; no capture changed because the broad sweep found no exception, HRESULT or JSON-parser text in Route List output.
- consequence review: `route-list.metadata-malformed` and `route-list.authored-form`
  were left unchanged. The malformed row remains the report, and its result
  does not carry a stable exact lost-field fact; the authored-form row is itself
  the listing report.

## Divergences observed

1. **Resolved: metadata warnings retain the listed route.** The prior
   incomplete/omitted-row divergence is fixed: missing or malformed metadata is
   completed-with-warnings at exit 2, and the row remains visible with
   `(no description)` when needed. Read-unavailable entrypoints remain
   incomplete at exit 3.

2. **Resolved: a malformed Loader is blocked.** Loader-malformed routing now
   returns blocked at exit 5 with the catalogue headline shape and
   `open-forge doctor` next action.
   Both of these join the pattern recorded in [26](26-route-remove.md)
   divergence 1, [36](36-library-sync.md) divergence 1 and the three-command
   `lock-held` finding: the error paths have drifted toward permissiveness
   relative to the catalogues, and are worth deciding as one group.

3. **The worker left its retired capture tree in place.** All 11 situations were
   converted and the 107 native captures written, but the 44 legacy files were
   not deleted; the overseer removed them before committing. Sixth lane with
   this omission.

4. **Merge conflict in a shared help test, resolved by the overseer.**
   `Shell/Presentation/Shared/Help/CliResultHelpTests.cs` conflicted between this
   lane and [25](25-route-move.md): each branch had removed its own legacy import
   and kept the other's, because each branched before the other merged. Resolved
   against what the merged body calls — `RouteListHelpSections` and
   `RouteMoveHelpSections`, both native — dropping both legacy imports. The
   legacy imports that remain in that file are Doctor, Route Remove and Update,
   which is exactly the set of commands still unconverted. **That correspondence
   is a useful invariant**: in a shared test file, the surviving legacy imports
   should always match the unconverted set.

5. **The prescribed build could not use the direct NuGet and artifact paths.**
   The worker used a task-scoped temporary `APPDATA` and `ArtifactsPath`
   successfully. Same root cause as [24](24-route-update.md) divergence 3, and
   nothing leaked into the merge.

6. **The shared compiler could not create DLL outputs in the sandbox.** The
   final build and gates succeeded with `-p:UseSharedCompilation=false` and the
   prescribed offline NuGet flags; no product files were affected.

- The broad owned-snapshot sweep found no raw exception, HRESULT or JSON-parser text in Route List captures. Its status ladder was not revisited.
- No Route List wording or capture changed in this slice; adding a consequence
  clause would either restate the listed row or invent which metadata was lost.

## Rollback

Restore the bridge registration for route list.
