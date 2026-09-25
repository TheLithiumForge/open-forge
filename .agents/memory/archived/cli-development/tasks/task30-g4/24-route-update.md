---
open-forge:
  description: Route update output catalogue
  tags: [Memory, CLI, Task, Plan, G4, Route, Contextual, Archived, Historical]
---

# 24 — route update

> Read [00 — G4 conventions](00-conventions.md) first. Depends on
> [03](03-rendering-system.md) and [04](04-interaction-system.md).

## Goal

`route update` shows each metadata field it changed as `old -> new`, says
when a Template body was not copied and why, and lists the parent Entries
section it refreshed.

## Depends on / Blocks

- Depends on: 03, 04, 23. Lane D.
- Blocks: 25.

## Shape

Change report.

## Situations

`description-changed`, `tags-replaced`, `responsibility-removed`,
`template-applied`, `template-body-protected` (warnings), `no-change`,
`dry-run`, `no-patch` (invalid), `unknown-source`, `ambiguous-id-prompt`,
`lock-held`, `write-failed-partial`, `cancelled`.

## Statuses and headlines

| Status                  | When                                          | Headline                                                        | Exit | Stream |
| ----------------------- | --------------------------------------------- | --------------------------------------------------------------- | ---: | ------ |
| completed               | fields changed                                | `Updated <id>`                                                  |    0 | stdout |
| completed               | already at the requested values               | `<id> already has these values. Nothing to do.`                 |    0 | stdout |
| completed (dry run)     | planned                                       | `Would update <id>`                                             |    0 | stdout |
| completed-with-warnings | Template body not copied (file has content)   | `Updated <id>, but the Template body was not copied.`           |    2 | stdout |
| completed-with-warnings | recovery bundle retained                      | + family row                                                    |    2 | stdout |
| incomplete              | source or Template unreadable                 | `<id> could not be updated: <limitation>. Nothing was changed.` |    3 | stdout |
| invalid-input           | no patch, bad value, bad Template, bad target | `Cannot update <id>: <problem>.`                                |    4 | stderr |
| blocked                 | unsafe frontmatter, ambiguity, lock           | `Cannot update <id>: <reason>.`                                 |    5 | stderr |
| failed                  | after effects                                 | `Route update stopped after <n> of <m> changes.`                |    1 | stderr |
| cancelled               | prompt cancelled, Ctrl+C                      | `Route update was cancelled. Nothing was changed.`              |  130 | stderr |

## Text by level

`minimal`:

```text
Updated memory/emerging/ideas/pricing/tiers
  description: "Pricing tier options" -> "Pricing tiers and their tradeoffs"
  tags: #Idea -> #Idea #Pricing
  Entry updated in .agents/memory/emerging/ideas/pricing/_pricing.md
```

`minimal`, responsibility removed: `  responsibility: "Define the tiers" -> (removed)`.

`minimal`, Template protected:

```text
Updated memory/emerging/ideas/pricing/tiers, but the Template body was not copied.
  description: "Pricing tier options" -> "Pricing tiers and their tradeoffs"
  The file already has content, which was kept. The Template templates/memory/idea was not copied.
```

`standard` adds `Workspace:`, the path, the Template path when used, and
`<path>  frontmatter rewritten` as the effect row.

`full` adds the before and after hashes and the full frontmatter before and
after.

## Prompts

Select when the ID matches several files (see 04).

## Findings catalogue

| Code                                      | Severity | Family                      | Message                                                                                                                                             | Next                                |
| ----------------------------------------- | -------- | --------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------- |
| route-update.invalid-input                | error    | invalid-input               |                                                                                                                                                     |                                     |
| route-update.invalid-target               | error    | unknown-source              | `<ref> is not a routed source that can be updated.` when known but ineligible                                                                       | `open-forge route list --depth=all` |
| route-update.invalid-patch                | error    | local                       | `Nothing to update: pass --description, --responsibility, --tag or --template.` / `--tag <value> is repeated.` / `--description must not be blank.` | corrected command                   |
| route-update.invalid-template             | error    | local                       | `--template <ref> is not a routed Template.`                                                                                                        | `open-forge find --tag Template`    |
| route-update.workspace-unavailable        | error    | workspace-unavailable       |                                                                                                                                                     |                                     |
| route-update.workspace-unsafe             | error    | workspace-unsafe            |                                                                                                                                                     |                                     |
| route-update.target-unsafe                | error    | target-unsafe               |                                                                                                                                                     |                                     |
| route-update.route-ambiguous              | error    | route-ambiguous             |                                                                                                                                                     |                                     |
| route-update.identity-collision           | error    | identity-collision          | (the prompt resolves it in a terminal)                                                                                                              |                                     |
| route-update.frontmatter-unsafe           | error    | local                       | `The frontmatter of <path> cannot be rewritten safely: <reason>.`                                                                                   | fix by hand                         |
| route-update.metadata-preservation-unsafe | error    | local                       | `Other frontmatter keys in <path> could not be preserved, so nothing was changed.`                                                                  | fix by hand                         |
| route-update.template-unsafe              | error    | local                       | `The Template <ref> could not be verified safely.`                                                                                                  | none                                |
| route-update.generated-region-unsafe      | error    | generated-region-unsafe     |                                                                                                                                                     |                                     |
| route-update.workspace-lock-unavailable   | error    | workspace-lock-unavailable  |                                                                                                                                                     |                                     |
| route-update.target-changed               | error    | target-changed              |                                                                                                                                                     |                                     |
| route-update.recovery-conflict            | error    | recovery-conflict           |                                                                                                                                                     |                                     |
| route-update.template-body-protected      | warning  | local                       | `The file already has content, which was kept. The Template <ref> was not copied.`                                                                  | none                                |
| route-update.inspection-incomplete        | warning  | inspection-incomplete       |                                                                                                                                                     |                                     |
| route-update.projection-incomplete        | warning  | projection-unavailable      |                                                                                                                                                     |                                     |
| route-update.template-unavailable         | warning  | local                       | `The Template <ref> could not be read.`                                                                                                             | none                                |
| route-update.recovery-unavailable         | warning  | recovery-unavailable        |                                                                                                                                                     |                                     |
| route-update.recovery-artifact-retained   | warning  | recovery-artifact-retained  |                                                                                                                                                     |                                     |
| route-update.target-changed-during-apply  | error    | target-changed-during-apply |                                                                                                                                                     |                                     |
| route-update.write-failed                 | error    | write-failed                |                                                                                                                                                     |                                     |
| route-update.verification-failed          | error    | verification-failed         |                                                                                                                                                     |                                     |
| route-update.recovery-failed              | error    | recovery-failed             |                                                                                                                                                     |                                     |
| route-update.operation-failed             | error    | operation-failed            |                                                                                                                                                     |                                     |
| route-update.interrupted                  | error    | interrupted                 |                                                                                                                                                     |                                     |

## Effects wording

Field rows `<field>: <old> -> <new>` with strings quoted and tags as `#Tag`
words; `(removed)` and `(none)` for absent values. `Body copied from the
Template <ref>`. `Entry updated in <parent>`.

## Counts

`fieldsChanged`, `sectionsUpdated`.

## JSON data by level

| Level    | `data`                                                                                                                   |
| -------- | ------------------------------------------------------------------------------------------------------------------------ |
| minimal  | `{ mode, target { id, path }, changes: [ { field, before, after } ], template { id, path, applied } \| null, listedIn }` |
| standard | same                                                                                                                     |
| full     | + `frontmatterBefore`, `frontmatterAfter`, per effect `before`, `after`                                                  |

## References

- `src/cli/core/OpenForge.Cli.Core/Commands/Route/Update/Shared/Rendering/*` — replaced by `Presentation/Route/Update/`.
- `RouteUpdateSourceSelector`, `RouteUpdateTargetObserver` prompt sites — per 04.
- Route update interface and its Technical Design (attached-empty responsibility) — unchanged behavior; ledger for wording.

## Preconditions

- [ ] 03, 04, 23 merged.

## Steps

1. [ ] Write `RouteUpdateReportSelector` and `RouteUpdateDataTextRenderer`.
2. [ ] Wire the ambiguity prompt.
3. [ ] Delete the old renderers.
4. [ ] Regenerate snapshots and review.
5. [ ] Three suites green.

## Acceptance

- [ ] Every changed field shows old and new at `minimal`.
- [ ] The protected-body case exits 2 with the sentence above.

## Changes ledger

- file layout/type: the legacy Route Update bridge under `Presentation/Legacy/Route/Update/` (9 files: human renderer and its `.Effects` partial, diagnostic renderer, JSON projection and renderer, help sections, legacy presentation) is deleted; native `Presentation/Route/Update/` owns the presentation, `Models/RouteUpdateData`, `Shared/Selection/RouteUpdateReportSelector`, `Shared/Rendering/` text renderer and JSON context, `Shared/Wording/` and `Shared/Help/`. `CliRouteComposer` closes the Route Update binding over it.
- message: the legacy status and body output is replaced by the catalogue headlines. Verified against `Statuses and headlines` at `minimal` for every situation: `Updated <id>`; `<id> already has these values. Nothing to do.`; `Would update <id>`; `Updated <id>, but the Template body was not copied.`; `Cannot update <id>: <problem>.` for no-patch, unknown source, ambiguity and lock; `Route update stopped after <n> of <m> changes.`; `Route update was cancelled. Nothing was changed.`
- message: field transitions, Template and body-copy effects, parent-entry effects and the dry-run `no files were changed` footer are rendered as rows beneath the headline rather than as legacy status lines.
- JSON member: the legacy projection is replaced by the schema v3 report envelope; `data` carries the catalogue shape, with hashes and frontmatter appearing only at `full` and `debug`.
- status or exit: status, exit disposition and stdout/stderr routing now come from the native report rather than legacy status lines.
- model: the presentation no longer reads Framework workspace types. `RouteUpdateResult` projects them into command-owned `WorkspacePath` and `WorkspaceExplicit` members, which is what `LayerBoundaryTests` requires.
- snapshots: the 52 command-local `.compact`/`.expanded` captures under `Commands/Route/Update/__snapshots__/` are replaced by **128** captures at the four native detail levels, text and JSON, under `src/cli/tests/integration/snapshots/RouteUpdateBeforeOutputSnapshotTests/`, across 13 situations.
- test: legacy JSON presence and schema contract tests are deleted; `RouteUpdatePresentationTests`, the application and interaction integration tests, and `CliResultHelpTests` are migrated to the native report. `PublishedRouteUpdateProcessTests` was migrated by the worker and passes 3/3 against the installed binary.
- evidence, overseer: re-run on the merge commit, not taken from the worker's report. Unit 3,410 passed, 0 failed, 0 skipped; integration 2,244 total, 2,227 passed, 0 failed, 17 skipped; `PublishedRouteUpdateProcessTests` 3 passed, 0 failed; `npm run check:dotnet` exactly the five documented errors.

- message: Route Update cause-bearing wording now delegates to the shared cause vocabulary; the recent cancellation receipt behavior remains unchanged.
- snapshots: regenerated the owned Route Update class; no primary capture changed, and its four raw debug-diagnostic captures remained unchanged.
- consequence wording: `route-update.template-unavailable` now says that the
  route was not updated; the selector delegates to the wording owner and its
  crystallized contract was updated. No capture exists for this candidate.

## Divergences observed

1. **The plan's prompt-wiring step was already done.** `Steps` asks to wire the
   ambiguity prompt per [04](04-interaction-system.md); the worker found the
   final prompt wiring already present, kept it and verified it rather than
   rebuilding it. No product change. A step that is already satisfied is not a
   defect, but the step wording is now stale.

2. **The capture form changed shape, not just level count.** The command-local
   helper wrote `.compact`/`.expanded` pairs; the native helper requires
   `minimal`, `standard`, `full` and `debug`. 52 captures were replaced by 128,
   so the per-level tables in this file are evidenced for the first time. The
   now-empty `Commands/Route/Update/__snapshots__/` tree is deleted.

3. **The sandbox cannot read the user's NuGet configuration.** The worker's
   first `dotnet build` failed on restore, because a `workspace-write` sandbox
   has no access to `%APPDATA%\NuGet\NuGet.Config` and `network: false` then
   denies the fallback to nuget.org. It worked around it by pointing `APPDATA`
   at a task-local directory and restoring from the warm global package cache.
   No product divergence, but it cost the run time, and it stopped a sibling
   lane outright. **Durable record:** worker packets should carry
   `$env:APPDATA = Join-Path (Get-Location) 'artifacts\nuget-appdata'` in their
   build section.

- The broad sweep found four raw exception/HRESULT matches in existing Route Update `*.debug.diagnostics.txt` files only. There was no minimal or standard leak, so the cancellation ruling and all captures remained unchanged.
- The template-unavailable candidate has no capture in the corpus, so snapshot
  regeneration produced no Route Update capture diff.

## Rollback

Restore the bridge registration for route update.
