---
open-forge:
  description: Route inspect output catalogue and ambiguity prompt
  tags: [Memory, Working, CLI, Task, Plan, G4, Route, Contextual, Active]
---

# 21 — route inspect

> Read [00 — G4 conventions](00-conventions.md) first. Depends on
> [03](03-rendering-system.md) and [04](04-interaction-system.md).

## Goal

`route inspect` keeps its three question blocks and drops everything that
only restates the selection. Counts read as English. The middle dot and the
`1 files` plural are gone.

## Depends on / Blocks

- Depends on: 03, 04, 20. Lane D.
- Blocks: 22.

## Shape

Report.

## Situations

`entrypoint`, `routed-file`, `load-now-child`, `keep-in-mind`, `overwrite-pair`,
`compatibility-entrypoint`, `not-routed-file`, `id-not-unique-exact-path`
(warnings), `ambiguous-id-prompt`, `unknown-source` (invalid), `loader-subject`
(invalid), `unreadable-source` (incomplete), `orphan-overwrite` (blocked).

## Statuses and headlines

| Status                  | When                                                    | Headline                                                                                  | Exit | Stream |
| ----------------------- | ------------------------------------------------------- | ----------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | resolved                                                | `<id>  <path>`                                                                            |    0 | stdout |
| completed-with-warnings | resolved by exact path or choice while the ID is shared | headline + warning row `The ID <id> also matches <other>. Use the exact path to be sure.` |    2 | stdout |
| incomplete              | a fact could not be measured or a layer read            | headline, blocks with the missing fact stated, warning rows                               |    3 | stdout |
| invalid-input           | unknown source, the Loader, several operands            | `Cannot inspect <ref>: <problem>.`                                                        |    4 | stderr |
| blocked                 | ambiguous route or overwrite, unsafe path               | `Cannot inspect <ref>: <reason>.`                                                         |    5 | stderr |
| failed                  | unexpected error                                        | `Route inspect stopped because of an unexpected error: <reason>.`                         |    1 | stderr |
| cancelled               | prompt cancelled, Ctrl+C                                | `Route inspect was cancelled.`                                                            |  130 | stderr |

## Text by level

`minimal`:

```text
memory  .agents/memory/_memory.md

Where this source belongs
  Route chain: memory
  Parent: none (Loader root)
  Direct children: 4 entrypoints
  Descendants: 11 entrypoints

When it is read
  At task start or resume: yes
  Read automatically when the Loader is read
  May be read again later: no

Context size
  This file: 3.36 KiB, about 861 tokens
  Selecting this route adds: nothing (already in startup context)
  Read automatically below it through #LoadNow: 6 files, 7.03 KiB, about 1801 tokens
```

Lines that do not apply are omitted: `Direct children` when a file has none,
`Read automatically below it` when nothing is tagged. Unusual facts appear as
extra lines in the first block: `Entrypoint name: index.md (compatibility
name; the canonical name is _memory.md)`, `Overwrite file: .agents/memory/_memory.overwrite.md`.

`standard` adds `Workspace:`, the Axioms block (`Inherited rules from:
loader`, `Local rules: yes`), and the tags line.

`full` adds the reason for the status, the selected-closure measurements
(`Selected context: 11 files, 14.41 KiB, about 3689 tokens`, `Already in
startup context: ...`), how the source was selected, and the physical layer
paths.

## Prompts

When an ID matches several files in a terminal: Select among the paths (see
[04](04-interaction-system.md)). The result then carries the warning row above.

## Findings catalogue

| Code                                   | Severity | Family                | Message                                                                          | Next                                         |
| -------------------------------------- | -------- | --------------------- | -------------------------------------------------------------------------------- | -------------------------------------------- |
| route-inspect.invalid-source-reference | error    | invalid-input         | `<operand> is not a source ID or a path under .agents.`                          | `open-forge route list --depth=all`          |
| route-inspect.unknown-source           | error    | unknown-source        |                                                                                  |                                              |
| route-inspect.missing-source           | error    | unknown-source        |                                                                                  |                                              |
| route-inspect.missing-source-file      | error    | local                 | `<path> does not exist.`                                                         | `open-forge route list --depth=all`          |
| route-inspect.multiple-sources         | error    | local                 | `Route inspect takes one source.`                                                | none                                         |
| route-inspect.loader-subject           | error    | local                 | `The Loader is not a route. Inspect one of its routes instead.`                  | `open-forge route list`                      |
| route-inspect.invalid-workspace        | error    | workspace-unavailable |                                                                                  |                                              |
| route-inspect.workspace-unavailable    | error    | workspace-unavailable |                                                                                  |                                              |
| route-inspect.unsafe-workspace         | error    | workspace-unsafe      |                                                                                  |                                              |
| route-inspect.ambiguous-source         | error    | source-ambiguous      | (the prompt resolves it in a terminal)                                           |                                              |
| route-inspect.ambiguous-route          | error    | route-ambiguous       |                                                                                  |                                              |
| route-inspect.ambiguous-overwrite      | error    | local                 | `<name>.overwrite.md could belong to more than one base file.`                   | fix by hand                                  |
| route-inspect.unsafe-source            | error    | source-unsafe         |                                                                                  |                                              |
| route-inspect.unsupported-source       | error    | local                 | `<path> is not a Markdown source Open Forge routes.`                             | none                                         |
| route-inspect.orphan-overwrite         | error    | local                 | `<name>.overwrite.md has no <name>.md beside it.`                                | fix by hand                                  |
| route-inspect.unreadable-source        | warning  | inspection-incomplete |                                                                                  |                                              |
| route-inspect.incomplete-route         | warning  | local                 | `The route chain above <path> could not be established completely: <reason>.`    | `open-forge doctor`                          |
| route-inspect.unavailable-fact         | warning  | local                 | `<fact> could not be measured: <reason>.` (rendered in place of the block line)  | `open-forge doctor`                          |
| route-inspect.automatic-id-not-unique  | warning  | local                 | `The ID <id> also matches <other path>. Use the exact path to be sure.`          | none                                         |
| route-inspect.not-routed               | info     | local                 | `<path> is not reachable from any route, so agents never load it automatically.` | `open-forge index` when its parent is routed |
| route-inspect.detached-source          | info     | local                 | `<path> belongs to a route that no Loader entry reaches.`                        | none                                         |
| route-inspect.compatibility-entrypoint | info     | local                 | `<path> uses the compatibility name <name>; the canonical name is _<folder>.md.` | none                                         |
| route-inspect.valid-overwrite          | info     | local                 | `<path> is read together with <base path>.`                                      | none                                         |
| route-inspect.operation-failed         | error    | operation-failed      |                                                                                  |                                              |
| route-inspect.interrupted              | error    | interrupted           |                                                                                  |                                              |

## Counts

`ownBytes`, `ownTokens`, `addedFiles`, `addedBytes`, `addedTokens`,
`loadNowFiles`, `loadNowBytes`, `loadNowTokens`, `directChildren`, `descendants`.

## JSON data by level

| Level    | `data`                                                                                                                                                                                                                                         |
| -------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ id, path, kind, entrypointForm, overwritePath, belongs { routeChain, parent, directChildren { files, entrypoints }, descendants { files, entrypoints } }, read { atStart, automaticallyWhen, mayReadAgain }, size { own, adds, loadNow } }` |
| standard | + `axioms { inheritedFrom: [...], local }`, `tags`                                                                                                                                                                                             |
| full     | + `selected { closure, startupOverlap }`, `selection { kind, method, requested }`, `layers: [ { path, kind } ]`, `statusReason`                                                                                                                |

## References

- `src/cli/core/OpenForge.Cli.Core/Commands/Route/Inspect/Shared/Rendering/*` (19 files) — replaced by `Presentation/Route/Inspect/`.
- `RouteInspectTextEscaping` (8192 limit) — deleted.
- `RouteInspectInteractiveSourceSelector` — replaced per 04.
- Route inspect interface Human output section (ledger only).

## Preconditions

- [ ] 03, 04, 20 merged.

## Steps

1. [ ] Write `RouteInspectReportSelector` and `RouteInspectDataTextRenderer` for the three blocks.
2. [ ] Wire the ambiguity prompt per 04.
3. [ ] Delete the old renderers and selector.
4. [ ] Regenerate snapshots and review.
5. [ ] Three suites green.

## Acceptance

- [ ] No `1 files`, no middle dot, no `Selection:` line at `minimal`.
- [ ] The three block headings are unchanged.

## Changes ledger

> Recorded by the overseer from the worker's closeout and the committed diff
> (`b1fa1b56`), for the worktree reasons in [10](10-status.md).

- file layout/type: the legacy Route inspect renderers are replaced by native `Presentation/Route/Inspect/` with its presentation, data model, selector, text and JSON renderers, wording and help.
- message: the three native blocks replace the legacy trace, per this file's `Text by level`; the ambiguity prompt reports the selected identity with a warning row per [04](04-interaction-system.md); `load-now-child` and `keep-in-mind` explain automatic reading; `overwrite-pair` states the overwrite facts and physical layers; `compatibility-entrypoint` states compatibility and canonical identity; `not-routed-file` is informational at full; `id-not-unique-exact-path` warns with no next action; `unknown-source` and `loader-subject` are invalid on stderr; `unreadable-source` is incomplete with an unavailable-fact block; `orphan-overwrite` is blocked.
- snapshots: 127 reviewed captures under `src/cli/tests/integration/snapshots/RouteInspectBeforeOutputSnapshotTests/`, at the four native detail levels, with 52 JSON files parsed and checked for forbidden plural grammar, middle dots, and an empty `Selection:` line at minimal.
- evidence, worker: unit 3,524 passed, 0 failed, 0 skipped; integration 2,289 total, 2,272 passed, 0 failed, 17 skipped; `check:dotnet` exactly the five documented errors.
- evidence, overseer after merging: unit 3,443 passed, 0 failed, 0 skipped; integration 2,257 total, 2,240 passed, 0 failed, 17 skipped; `PublishedRouteInspectProcessTests` 3/3 with no repair needed.
- consequence review: `route-inspect.incomplete-route` and
  `route-inspect.unavailable-fact` already state the incomplete or unmeasured
  answer. `route-inspect.automatic-id-not-unique` is a selection warning whose
  exact-path action still yields the selected output; no wording changed.

## Divergences observed

Recovered by the overseer from the run transcript (the archived execution transcript)
after the worker's reply was truncated mid-list. The four bullets below are the
worker's own, expanded; the fifth is the overseer's.

1. **The next-action policy conflicts with this file's catalogue.** The worker
   reported three classes of disagreement and changed no frozen string: `--help`
   where the catalogue asks for route list; `open-forge doctor` where the
   catalogue says "fix by hand"; and a next action emitted where the catalogue
   says none. **Maintainer decision:** which side is authoritative per row. This
   is the same family as the open `10-status` wording questions and should be
   settled with them.
2. **This file names the legacy renderers under `Commands/...`, but the branch
   holds them under `Presentation/Legacy/...`.** A named symbol that no longer
   exists is a divergence: the `Steps` and `References` wording points at a path
   that had already moved before this subtask ran. **Durable record to change:**
   this file's paths.
3. **The worker could not write this file.** Both `apply_patch` attempts returned
   `patch rejected: writing outside of the project; rejected by user approval
   settings`, because it addressed the main checkout by absolute path rather than
   its own worktree. The overseer wrote the ledger and committed.
4. **The worker could not commit or stage.** `git status --short --branch` first
   failed with dubious ownership; `git add -- src/cli` then failed creating
   `.git/worktrees/<id>/index.lock` with `Permission denied`, and alternate
   staging failed because `.git/objects` was not writable. This is the known
   worktree constraint, not a fault in the change.
5. **End-to-end was never run for this command, and one caller was left broken.**
   The worker's report ends "End-to-end tests were not run per explicit
   instructions" -- correct per its packet, which reserves the suite for the
   overseer. The gate was not run on its behalf either, so
   `PublishedShellBoundaryProcessTests.OptionLikeOperandAfterTerminatorRemainsDomainInput`
   went on reading `data.selection.requestedReference`, which the native Route
   Inspect report does not publish, and failed from this subtask's merge until it
   was found during [15](15-repair.md)'s review. It now asserts
   `findings[0].code == route-inspect.unknown-source` and
   `findings[0].subject.id == --view`. The branch's recorded end-to-end figure of
   163 passed, 0 failed did not hold between those two merges.

6. **No Route Inspect wording or capture changed in this slice.** Each
   candidate already carries its consequence in the existing sentence or is a
   selection instruction; adding another clause would restate the answer or
   invent answer loss.

## Rollback

Restore the bridge registration for route inspect.
