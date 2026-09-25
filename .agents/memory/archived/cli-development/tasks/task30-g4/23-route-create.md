---
open-forge:
  description: Route create output catalogue
  tags: [Memory, CLI, Task, Plan, G4, Route, Contextual, Archived, Historical]
---

# 23 — route create

> Read [00 — G4 conventions](00-conventions.md) first. Depends on
> [03](03-rendering-system.md).

## Goal

`route create` says which file it created and where it was listed. Invalid
input names every missing value in one run and shows the corrected command.

## Depends on / Blocks

- Depends on: 03, 22. Lane D.
- Blocks: 24.

## Shape

Change report.

## Situations

`created`, `created-from-template`, `dry-run`, `already-matching` (no-op),
`missing-description`, `missing-tag`, `missing-both`, `invalid-target`,
`parent-missing`, `exists-with-different-content` (blocked),
`template-unknown`, `lock-held`, `write-failed-partial`, `cancelled`.

## Statuses and headlines

| Status                  | When                                                   | Headline                                                              | Exit | Stream |
| ----------------------- | ------------------------------------------------------ | --------------------------------------------------------------------- | ---: | ------ |
| completed               | created                                                | `Created <path>  (<id>)`                                              |    0 | stdout |
| completed               | identical file already present                         | `<path> already has the requested content. Nothing to do.`            |    0 | stdout |
| completed (dry run)     | planned                                                | `Would create <path>  (<id>)`                                         |    0 | stdout |
| completed-with-warnings | recovery bundle retained                               | + family row                                                          |    2 | stdout |
| incomplete              | template, parent or record unreadable                  | `The file could not be created: <limitation>. Nothing was changed.`   |    3 | stdout |
| invalid-input           | missing or invalid metadata, bad target                | `Cannot create the routed file: <problem>.` (all problems in one run) |    4 | stderr |
| blocked                 | exists with different content, unsafe, ambiguous, lock | `Cannot create <path>: <reason>.`                                     |    5 | stderr |
| failed                  | after effects                                          | `Route create stopped after <n> of <m> changes.`                      |    1 | stderr |
| cancelled               | Ctrl+C                                                 | `Route create was cancelled. Nothing was changed.`                    |  130 | stderr |

## Text by level

`minimal`, created:

```text
Created .agents/memory/emerging/ideas/pricing/tiers.md  (memory/emerging/ideas/pricing/tiers)
  Listed in .agents/memory/emerging/ideas/pricing/_pricing.md
```

`minimal`, from a Template:

```text
Created .agents/memory/emerging/ideas/pricing/tiers.md  (memory/emerging/ideas/pricing/tiers)
  Body copied from the Template templates/memory/idea
  Listed in .agents/memory/emerging/ideas/pricing/_pricing.md
```

`minimal`, missing values (stderr):

```text
Cannot create the routed file: --description is missing and at least one --tag is required.
Next: open-forge route create memory/emerging/ideas/pricing/plans --description "<one sentence>" --tag <Tag>
```

`standard` adds `Workspace:`, the metadata written (`description:`, `tags:`,
`responsibility:`), and the Template path.

`full` adds the new file's content verbatim under a `--- <path> (new file)
---` header and the before and after hashes of the parent's Entries section.

## Findings catalogue

| Code                                     | Severity | Family                      | Message                                                                                                                                                      | Next                                |
| ---------------------------------------- | -------- | --------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------ | ----------------------------------- |
| route-create.invalid-input               | error    | invalid-input               |                                                                                                                                                              |                                     |
| route-create.invalid-target              | error    | local                       | `<target> must be a Markdown file below an existing route.` / `<target> is an entrypoint, a folder or an overwrite file; route create makes ordinary files.` | `open-forge route create --help`    |
| route-create.invalid-metadata            | error    | local                       | `--description is missing.` / `at least one --tag is required.` / `--tag <value> is repeated.` / `--tag <value> is not a valid tag.` joined with `and`       | corrected command                   |
| route-create.optional-metadata          | warning  | local                       | `Would create <path> without optional description or tags.` / `Created <path> without optional description or tags.` / `<path> has no optional description or tags.` / `Optional metadata is missing for <path>.` | `open-forge route update <id>` |
| route-create.invalid-template            | error    | local                       | `--template <ref> is not a routed Template.`                                                                                                                 | `open-forge find --tag Template`    |
| route-create.parent-missing              | error    | local                       | `<folder> has no entrypoint, so the file cannot be listed. Create the route first.`                                                                          | `open-forge route init <parent id>` |
| route-create.workspace-unavailable       | error    | workspace-unavailable       |                                                                                                                                                              |                                     |
| route-create.workspace-unsafe            | error    | workspace-unsafe            |                                                                                                                                                              |                                     |
| route-create.target-unsafe               | error    | target-unsafe               |                                                                                                                                                              |                                     |
| route-create.target-content-differs      | error    | local                       | `<path> already exists with different content.`                                                                                                              | `open-forge route update <id>`      |
| route-create.route-ambiguous             | error    | route-ambiguous             |                                                                                                                                                              |                                     |
| route-create.identity-collision          | error    | identity-collision          | (blocking: the new ID would collide)                                                                                                                         | choose another name                 |
| route-create.metadata-unsafe             | error    | metadata-unsafe             |                                                                                                                                                              |                                     |
| route-create.template-unsafe             | error    | local                       | `The Template <ref> could not be verified safely.`                                                                                                           | none                                |
| route-create.generated-region-unsafe     | error    | generated-region-unsafe     |                                                                                                                                                              |                                     |
| route-create.workspace-lock-unavailable  | error    | workspace-lock-unavailable  |                                                                                                                                                              |                                     |
| route-create.target-changed              | error    | target-changed              |                                                                                                                                                              |                                     |
| route-create.recovery-conflict           | error    | recovery-conflict           |                                                                                                                                                              |                                     |
| route-create.inspection-incomplete       | warning  | inspection-incomplete       |                                                                                                                                                              |                                     |
| route-create.metadata-incomplete         | warning  | metadata-incomplete         |                                                                                                                                                              |                                     |
| route-create.projection-incomplete       | warning  | projection-unavailable      |                                                                                                                                                              |                                     |
| route-create.template-unavailable        | warning  | local                       | `The Template <ref> could not be read.`                                                                                                                      | none                                |
| route-create.recovery-unavailable        | warning  | recovery-unavailable        |                                                                                                                                                              |                                     |
| route-create.recovery-artifact-retained  | warning  | recovery-artifact-retained  |                                                                                                                                                              |                                     |
| route-create.target-changed-during-apply | error    | target-changed-during-apply |                                                                                                                                                              |                                     |
| route-create.write-failed                | error    | write-failed                |                                                                                                                                                              |                                     |
| route-create.verification-failed         | error    | verification-failed         |                                                                                                                                                              |                                     |
| route-create.recovery-failed             | error    | recovery-failed             |                                                                                                                                                              |                                     |
| route-create.operation-failed            | error    | operation-failed            |                                                                                                                                                              |                                     |
| route-create.interrupted                 | error    | interrupted                 |                                                                                                                                                              |                                     |

## Effects wording

`Created <path>` / `Would create <path>`, `Listed in <parent>` /
`Would list it in <parent>`, `Body copied from the Template <ref>`.

## Counts

`filesCreated`, `sectionsUpdated`.

## JSON data by level

| Level    | `data`                                                                   |
| -------- | ------------------------------------------------------------------------ |
| minimal  | `{ mode, target { id, path }, listedIn, template { id, path } \| null }` |
| standard | + `metadata { description, responsibility, tags }`                       |
| full     | + `content`, per section `before`, `after`, `verification`               |

## References

- `src/cli/core/OpenForge.Cli.Core/Commands/Route/Create/Shared/Rendering/*` — replaced by `Presentation/Route/Create/`.
- Binder: report all missing metadata in one pass (ledger: contract says one at a time today).
- Route create interface Errors, Human output (ledger only).

## Preconditions

- [ ] 03, 22 merged.

## Steps

1. [ ] Make the binder collect every metadata problem before returning invalid.
2. [ ] Write `RouteCreateReportSelector` and `RouteCreateDataTextRenderer`.
3. [ ] Delete the old renderers.
4. [ ] Regenerate snapshots and review.
5. [ ] Three suites green.

## Acceptance

- [ ] `minimal` created is two lines.
- [ ] Missing description and tag are reported together with a corrected command.
- [ ] No hashes and no `Unchanged:` roster below `full`.

## Changes ledger

> Recorded by the overseer from the worker's closeout and the committed diff
> (`87ac7282`), for the worktree reasons in [10](10-status.md).

- file layout/type: the legacy Route create renderers are replaced by native `Presentation/Route/Create/`.
- message: `Created <path>  (<id>)` with the `Listed in <entrypoint>` row; the dry run says `Would create ...`, `Would list it in ...` and ends with `No files were changed.`; an unchanged target says `<path> already has the requested content. Nothing to do.`; invalid input says `Cannot create the routed file: <problem>.` with the corrected command as `Next`, not `--help`.
- JSON member: `data` became `{ mode, target { id, path }, listedIn, template }`. The complete effect receipts stay on the envelope rather than inside the command data.
- snapshots: 136 LF captures under `src/cli/tests/integration/snapshots/RouteCreateBeforeOutputSnapshotTests/` covering fourteen situations at the four native detail levels. Ten snapshot theories were consolidated.
- evidence, worker: unit 3,523 passed, 0 failed, 0 skipped; integration 2,290 total, 2,273 passed, 0 failed, 17 skipped; `check:dotnet` exactly the five documented errors.
- evidence, overseer after merging: unit 3,443 passed, 0 failed, 0 skipped; integration 2,257 total, 2,240 passed, 0 failed, 17 skipped; `PublishedRouteCreateProcessTests` 3/3 after the overseer migrated it.
- test, overseer: `PublishedRouteCreateProcessTests` still read the legacy graph and wording. Its dry-run JSON now reads `data.listedIn` and takes the effect list from the envelope; the apply journey asserts the `Created ...` headline and `Listed in ...` row and the absence of any `Status:` line; the invalid journey asserts the `Cannot create the routed file:` family and its corrected `Next`; the converge journey asserts the `Nothing to do.` headline and that a read-only result prints no `No files changed.`

- message: Route Create's blocked lock headline now uses the shared workspace-lock sentence; all cause-bearing wording delegates to the shared cause vocabulary, and the raw lock cause is retained only as `cause` evidence at `full`/`debug`.
- snapshots: regenerated the eight `SafetyBoundary/lock-held` text and JSON captures; each changed only the headline/summary line and, at `full`/`debug`, added the raw cause under `cause`, from the raw IOException headline cause to the shared lock sentence. The two partial-write debug diagnostic captures were reviewed and unchanged.
- consequence wording: `route-create.template-unavailable` now says that the
  file was not created; its crystallized contract was updated. No capture exists
  for this candidate.

## Divergences observed

- The worker corrected three things it found while reviewing its own captures: physical section paths were being printed where logical paths are required; the dry run did not end with `No files were changed.`; and an obsolete detail-invariance assertion was replaced with catalogue-level JSON assertions.
- This catalogue still references the legacy renderer by name; [41](41-documentation-propagation.md) must update that reference.
- This worker received the packet before it assigned each worker its own published-process class, so `PublishedRouteCreateProcessTests` was left on the legacy graph and the overseer migrated it after merging.
- The worker could not write this file or commit; the overseer did both.

- The broad sweep found the supplied Route Create lock-held minimal/standard leaks plus their full/debug counterparts, and raw causes in four existing Route Create debug-diagnostic captures (two lock-held, two partial-write). Only the eight primary lock-held captures changed; their full/debug raw causes are labelled `cause`, and the diagnostic channel remains raw by design.
- The template-unavailable candidate has no capture in the corpus, so snapshot
  regeneration produced no Route Create capture diff.

## Rollback

Restore the bridge registration for route create.
