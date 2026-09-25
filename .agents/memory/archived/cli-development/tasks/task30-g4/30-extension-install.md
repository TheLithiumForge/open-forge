---
open-forge:
  description: Extension install output catalogue, package selection, permission and confirmation flows
  tags: [Memory, CLI, Task, Plan, G4, Extension, Contextual, Archived, Historical]
---

# 30 — extension install

> Read [00 — G4 conventions](00-conventions.md) first. Depends on
> [03](03-rendering-system.md) and [04](04-interaction-system.md).

## Goal

`extension install` says which packages it installed, including the ones
they required, which files it created, which Entries sections it updated,
and which grant it saved. Outside `.agents` it asks for permission or names
the flag. A package with no content is a warning, never a silent success.

## Depends on / Blocks

- Depends on: 03, 04, 29. Lane E.
- Blocks: 31.

## Shape

Change report.

## Situations

`single-package`, `with-dependencies`, `select-from-source-prompt`,
`no-selection-non-interactive` (invalid), `permission-required-non-interactive`
(blocked), `permission-prompt-always`, `permission-prompt-once`,
`allow-path-flag`, `existing-file-without-force` (blocked), `with-force`,
`already-installed` (no-op), `changed-since-install` (blocked, points at
update), `no-content-directory` (warnings), `dry-run`, `source-unreadable`,
`lock-held`, `write-failed-partial`, `cancelled`.

## Statuses and headlines

| Status                  | When                                                                                                  | Headline                                                                                       | Exit | Stream |
| ----------------------- | ----------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | one package                                                                                           | `Installed the <id> Extension.`                                                                |    0 | stdout |
| completed               | with dependencies                                                                                     | `Installed the <id> Extension and <N> packages it requires: <ids>.`                            |    0 | stdout |
| completed               | several selected                                                                                      | `Installed <N> Extensions: <ids>.`                                                             |    0 | stdout |
| completed               | already installed and equal                                                                           | `The <id> Extension is already installed and matches the package. Nothing to do.`              |    0 | stdout |
| completed (dry run)     | planned                                                                                               | `Would install the <id> Extension.` (variants as above)                                        |    0 | stdout |
| completed-with-warnings | nothing to install (no content directory), lifecycle observation, retained recovery                   | `Nothing was installed from <source>: the package has no content directory.` / headline + rows |    2 | stdout |
| incomplete              | source, Framework, record, permissions or recovery unreadable                                         | `The <id> Extension could not be installed: <limitation>. Nothing was changed.`                |    3 | stdout |
| invalid-input           | bad ID, invalid package, no selection possible, prompt ended                                          | `Cannot install: <problem>.`                                                                   |    4 | stderr |
| blocked                 | permission required or declined, existing file, changed since install, conflict, lock, target changed | `Cannot install <id>: <reason>.`                                                               |    5 | stderr |
| failed                  | after effects                                                                                         | `Extension install stopped after <n> of <m> changes.`                                          |    1 | stderr |
| cancelled               | prompt cancelled, Ctrl+C                                                                              | `Extension install was cancelled. Nothing was changed.`                                        |  130 | stderr |

## Text by level

`minimal`, with dependencies:

```text
Installed the orchestration Extension and 2 packages it requires: planning, project-documents.
  Created 9 files under .agents/workflows, .agents/patterns and .agents/templates
  Updated the Entries section of 3 files
  Saved a grant for tools/review to .agents/open-forge.json
```

`minimal`, permission required outside a terminal (stderr):

```text
Cannot install team-tools: it writes outside .agents and no grant allows that.
  tools/review   (directory: everything under it)
Next: open-forge extension install team-tools --allow-path tools/review
  Or add "tools/review" to allowInstallPaths in .agents/open-forge.json.
```

`minimal`, no content (exit 2):

```text
Nothing was installed from ./mypkg: the package has no content directory.
  Package files belong under ./mypkg/content/.agents/.
```

`minimal`, existing file without `--force` (stderr):

```text
Cannot install my-tools: 1 file already exists where the package would write.
  .agents/workflows/review.md
Next: open-forge extension install my-tools --force --dry-run  (preview replacing it)
```

`standard` adds `Workspace:`, the source, every created file as a row with
its package, every Entries section as a row, the grant scope, and the
dependency order.

`full` adds the unchanged Entries sections, the permission evaluation, the
Framework fingerprint, recovery and verification facts in words.

## Prompts

Per [04](04-interaction-system.md): multi-select when the source has several
packages and no ID was given; Permission for paths outside `.agents`; Confirm
for existing files (`Replace the 1 existing file listed above? [y/N]`); plan
review; `Apply these changes? [y/N]`.

## Findings catalogue

| Code                                           | Severity | Family                       | Message                                                                                        | Next                                                  |
| ---------------------------------------------- | -------- | ---------------------------- | ---------------------------------------------------------------------------------------------- | ----------------------------------------------------- |
| extension-install.invalid-input                | error    | invalid-input                |                                                                                                |                                                       |
| extension-install.confirmation-required        | error    | confirmation-required        |                                                                                                |                                                       |
| extension-install.selection-required           | error    | selection-required           |                                                                                                | `open-forge extension list`                           |
| extension-install.interaction-ended            | error    | interaction-ended            | (status `cancelled`; record the classification change)                                         |                                                       |
| extension-install.source-unavailable           | warning  | local                        | `The source <path> could not be read.`                                                         | none                                                  |
| extension-install.source-invalid               | error    | local                        | `<path> is not a valid package or package folder: <reason>.`                                   | fix by hand                                           |
| extension-install.package-content-missing      | warning  | local                        | `The package has no content directory. Package files belong under <path>/content/.agents/.`    | none                                                  |
| extension-install.framework-unavailable        | warning  | framework-unavailable        |                                                                                                |                                                       |
| extension-install.framework-unsafe             | error    | framework-unsafe             |                                                                                                |                                                       |
| extension-install.lifecycle-unavailable        | warning  | lifecycle-unavailable        |                                                                                                |                                                       |
| extension-install.lifecycle-blocked            | error    | lifecycle-blocked            |                                                                                                |                                                       |
| extension-install.lifecycle-observation        | info     | ownership-observation        |                                                                                                |                                                       |
| extension-install.managed-divergence           | error    | managed-divergence           |                                                                                                | `open-forge extension update <id>`                    |
| extension-install.initial-force-required       | error    | target-occupied              | row per existing file                                                                          | `open-forge extension install <id> --force --dry-run` |
| extension-install.ownership-conflict           | error    | ownership-conflict           |                                                                                                |                                                       |
| extension-install.permission-required          | error    | permission-required          |                                                                                                |                                                       |
| extension-install.permission-declined          | error    | permission-declined          |                                                                                                |                                                       |
| extension-install.permissions-invalid          | error    | permissions-invalid          |                                                                                                |                                                       |
| extension-install.permissions-unavailable      | warning  | permissions-unavailable      |                                                                                                |                                                       |
| extension-install.permissions-changed          | error    | permissions-changed          |                                                                                                |                                                       |
| extension-install.permission-write-failed      | error    | permission-write-failed      |                                                                                                |                                                       |
| extension-install.target-unsafe                | error    | target-unsafe                | includes reserved paths: `<path> is reserved for Open Forge's own files.`                      |                                                       |
| extension-install.projection-unavailable       | warning  | projection-unavailable       |                                                                                                |                                                       |
| extension-install.generated-region-unsafe      | error    | generated-region-unsafe      |                                                                                                |                                                       |
| extension-install.workspace-lock-unavailable   | error    | workspace-lock-unavailable   |                                                                                                |                                                       |
| extension-install.target-changed               | error    | target-changed               |                                                                                                |                                                       |
| extension-install.recovery-conflict            | error    | recovery-conflict            |                                                                                                |                                                       |
| extension-install.recovery-unavailable         | warning  | recovery-unavailable         |                                                                                                |                                                       |
| extension-install.recovery-artifact-retained   | warning  | recovery-artifact-retained   |                                                                                                |                                                       |
| extension-install.write-failed                 | error    | write-failed                 |                                                                                                |                                                       |
| extension-install.topology-verification-failed | error    | local                        | `The Entries sections did not match the installed files after writing. Recovery data: <path>.` | `open-forge doctor`                                   |
| extension-install.lifecycle-publication-failed | error    | lifecycle-publication-failed |                                                                                                |                                                       |
| extension-install.verification-failed          | error    | verification-failed          |                                                                                                |                                                       |
| extension-install.recovery-failed              | error    | recovery-failed              |                                                                                                |                                                       |
| extension-install.operation-failed             | error    | operation-failed             |                                                                                                |                                                       |
| extension-install.interrupted                  | error    | interrupted                  |                                                                                                |                                                       |

## Effects wording

Created files: at `minimal` counted by directory (`Created 3 files under
.agents/workflows`), at `standard` rows `<path>  created (<package>)`;
replaced existing files always rows `<path>  replaced (your previous file is
in the recovery bundle)`; `Updated the Entries section of <path>` (`minimal`
counts them when more than three); `Saved a grant for <path> to
.agents/open-forge.json`; lock `updated` at `standard`. Dry run: `Would
create`, `Would update`, `Would save`. Partial: `created`, `not started`,
`final state unknown`.

## Counts

`packagesInstalled`, `filesCreated`, `filesReplaced`, `sectionsUpdated`,
`grantsSaved`.

## JSON data by level

| Level    | `data`                                                                                                                                                                                        |
| -------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ mode, force, automatic, source { kind, path }, packages: [ { id, version, selected: bool, requiredBy: [...] } ], permissions { decision, required: [...], missing: [...], saved: bool } }` |
| standard | + per effect `owner`, `sections: [ path ]`, `selection { method } `                                                                                                                           |
| full     | + `entriesUnchanged: [ path ]`, `frameworkFingerprint`, `verification`, `recovery` details                                                                                                    |

## References

- `src/cli/core/OpenForge.Cli.Core/Presentation/Legacy/Extension/Install/Shared/Rendering/*` — deleted, replaced by `Presentation/Extension/Install/`. (Corrected during [41](41-documentation-propagation.md): this line previously named `Commands/Extension/Install/Shared/Rendering/*`.)
- `ExtensionInstallSelectionResolver.cs:90-160`, `ExtensionInstallTargetInspector.cs:205-250`, `ExtensionPermissionOperation.cs:25-90` — prompt sites per 04.
- Extension install interface Output, Human Confirmation, permission sections (ledger only).
- `docs/cli.md` still lists `extension remove --prune`; unrelated to this file but note in the ledger if seen.

## Preconditions

- [ ] 03, 04, 29 merged.

## Steps

1. [ ] Write `ExtensionInstallReportSelector` and `ExtensionInstallDataTextRenderer`.
2. [ ] Wire the four prompts per 04.
3. [ ] Make an empty content directory a warning result.
4. [ ] Delete the old renderers.
5. [ ] Regenerate snapshots and review.
6. [ ] Three suites green.

## Acceptance

- [ ] Single package `minimal` is three lines.
- [ ] The permission refusal names the paths and the flag.
- [ ] No `Generated navigation observed for installation.` line at any level.

## Changes ledger

- file layout/type: the legacy Extension Install bridge — the human and path renderers, the JSON projection and the command-local snapshots — is deleted; native `Presentation/Extension/Install/` owns the selector, data model, text renderer, JSON context, wording and help. `CliExtensionComposer` closes the binding over it. **With this lane the Extension family is complete**: Create, Inspect, Install, List, Remove and Update are all native.
- status or exit: legacy status-prefixed output is replaced by the catalogue headlines, with the specified streams and exits.
- message: the completed-with-no-content case now reads `Nothing was installed from <extension-source>: the package has no content directory.`, with the required content-path row.
- message: the legacy generated-navigation summaries are replaced by created and replaced file rows, Entries rows, grant rows, dependency details and the catalogue counts.
- JSON member: the legacy selection, source, framework, footprint, navigation and lifecycle shape is replaced by level-gated native data carrying versions, `requiredBy`, permissions, sections, fingerprint, verification and recovery.
- snapshots: the 72 legacy captures are replaced by **180** reviewed captures under `src/cli/tests/integration/snapshots/ExtensionInstallBeforeOutputSnapshotTests/`, covering 18 situations at four detail levels plus diagnostics — the largest situation set in the packet. The worker deleted its own retired tree.
- test: legacy callers are migrated to native root findings, effects, counts and Install data. `PublishedExtensionInstallProcessTests` was migrated by the worker and passes 3/3 against the installed binary.
- test fixture: an unconditional absent-lock deletion is now guarded by `File.Exists`, for isolated Windows test data.
- evidence, overseer: re-run on the merge commit, not taken from the worker's report. Unit 3,255 passed, 0 failed, 0 skipped; integration 2,208 total, 2,191 passed, 0 failed, 17 skipped; `PublishedExtensionInstallProcessTests` 3 passed; `PublishedSchema3ProcessTests` 28 passed and `PublishedShellBoundaryProcessTests` 26 passed, both checked because this lane reshaped `data`; `npm run check:dotnet` exactly the five documented errors.

- message: `write-failed` finding messages now use the shared filesystem-access cause vocabulary instead of exposing the exception type and HRESULT; the lock-held user-facing projection was already generic and remained unchanged.
- snapshots: regenerated the seven `PartialWriteFailure/write-failed-partial` native captures across text and JSON detail levels; no `PackageInstallation_lock-held` capture changed.

- catalogue: added `extension-install.confirmation-required` with the shared
  `confirmation-required` family; the emitted finding is `Invalid` and exits 4.

## Divergences observed

- The runtime emits `extension-install.confirmation-required` when final
  confirmation is unavailable, but the findings catalogue had no row for it.
  The shared-family row now records that existing output; no source or capture
  changed.

1. **The `--all` wording conflict, shared with [31](31-extension-update.md).**
   The catalogue asks for `--all cannot be combined with package IDs.`; the code
   emits `Explicit Extension IDs and --all cannot be combined.` The string lives
   in five call sites across both commands — `ExtensionInstallBinding.cs:159`,
   `ExtensionInstallOperationFactory.cs:91`,
   `ExtensionInstallSelectionResolver.cs:111`,
   `ExtensionUpdateBinding.cs:173` and `ExtensionUpdateOperation.cs:49`. Nothing
   was changed. **Maintainer decision, and one decision settles both commands.**

2. **This file's catalogue points at a renderer path that no longer exists.** It
   documents the old renderer location; the files actually deleted were under
   `Presentation/Legacy/Extension/Install/`. The same stale-path defect as
   [25](25-route-move.md) divergence 8 and [22](22-route-init.md) divergence 3.
   **Durable record to change:** this file's paths.

3. **The no-content step needed no operation change.** The operation already
   emitted `PackageContentMissing`; only the native presentation changed. Worth
   recording because the plan implied command-layer work that was not required.

4. **Three merge conflicts in shared Extension fixtures, resolved by the
   overseer.** `Commands/Extension/Shared/Interaction/ExtensionInteractionTestFactory.cs`
   conflicted in both the integration and unit suites — the usual shape, where
   each lane had removed its own legacy import and kept the other's. Resolved by
   keeping both native sets and dropping every legacy one; neither file now
   contains a `Presentation.Legacy.Extension` import.

   The third was **semantic, not an import list**:
   `Commands/Extension/Shared/Permissions/ExtensionPermissionLifecycleIntegrationTests.cs`
   is a theory over `install`, `update` and `remove`, and this lane and
   [31](31-extension-update.md) had rewritten the same assertion block
   differently — this lane branching on `command == "install"` for the
   already-installed no-op, the other asserting `Grant: … decision approved …`
   unconditionally. Neither side was wholly right: install had gone native in
   this merge and reports the no-op case, while update and remove keep the grant
   wording that [31](31-extension-update.md) verified. Resolved as a per-command
   branch preserving both, and confirmed by the integration suite passing.

5. **Closeout was blocked, as expected for a worktree.** Both the task-record
   patch and `git add` were refused, with the errors quoted in the worker's
   report. The overseer wrote this ledger and committed.

6. **This ledger was written late.** The lane merged and its gates passed, but
   the ledger was not written at merge time and the omission was only caught by
   a sweep of every subtask's fill state. **Process note:** scan
   `Changes ledger` emptiness across all subtasks before declaring a batch
   finished — a merged lane with an unfilled ledger is an incomplete subtask by
   this packet's own definition, and [40](40-verification.md)'s acceptance
   requires every one of them to be non-empty.

- The affected lock-held situation retains its raw lock cause only in existing internal evidence; no catalogue template change was needed. The newly corrected user-facing situation was `PartialWriteFailure`.

## Rollback

Restore the bridge registration for extension install.
