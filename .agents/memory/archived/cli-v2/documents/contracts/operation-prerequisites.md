---
open-forge:
  description: Historical CLI-v2 source: Direct typed prerequisites, result behavior, and exact capability gates for every replacement CLI operation
  responsibility: Define what each command must prove before useful inspection or mutation without turning Framework presence into a universal permission switch
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Operation Prerequisite Contract

## Scope

The [CLI interface](../interface.md) owns public commands, arguments, flags, and
results. This contract owns the evidence each selected operation needs before
it can complete or mutate. The focused domain contracts retain their detailed
route, filesystem, recovery, Framework, Extension, and shell semantics.

The [request construction contract](request-construction.md) keeps semantic
intent separate from these operation-specific prerequisites. A request
resolver may inspect read-only prerequisite facts needed to present valid
choices; the handler and mutation preflight still evaluate and revalidate the
exact evidence required to execute the completed request.

Production TypeScript owns exact declarations, discriminants, diagnostic
codes, and import paths. The labels below describe semantic evidence; they are
not a runtime string registry.

## Guarantees

### Governing Rule

Framework presence is orientation evidence, not universal permission.

Each named handler directly calls the focused typed inspection or capability
function its operation needs, then exhaustively handles that result. Do not
declare prerequisites through strings such as:

```ts
// Rejected: indirect string-coupled behavior.
requires: ["framework-installed", "route-inventory"];
```

There is no generic capability bag, command middleware chain, service locator,
or reflection-based dispatcher. Shared inspection is reused through direct
imports at the nearest common source scope.

### Evidence Vocabulary

The matrix uses this compact documentation vocabulary:

| Evidence                            | Meaning                                                                                                                                                                                                                                                                                                                                                                                                                               |
| ----------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Workspace directory                 | The exact selected root exists as an ordinary directory                                                                                                                                                                                                                                                                                                                                                                               |
| Rooted route inventory              | The exact `.agents/loader.md` is safely readable as the inventory root; unrelated invalid descendants remain findings rather than erasing safe nodes                                                                                                                                                                                                                                                                                  |
| Installed Extension host            | The canonical workspace entry and loader are recognizable, and every required destination route host exists                                                                                                                                                                                                                                                                                                                           |
| Open Forge content path             | Every explicit positional `<path>` begins with `.agents` or `./.agents`, normalizes to one canonical path, remains contained beneath the selected workspace, and identifies an operation-eligible source or destination                                                                                                                                                                                                               |
| Workspace-contained Template source | A purpose-specific `./`-prefixed Template source resolves from the selected workspace, remains contained beneath it, and rejects absolute paths, parent traversal, links, aliases, and unsupported node kinds                                                                                                                                                                                                                         |
| Exact external Extension catalogue  | One explicitly supplied filesystem path resolves to one ordinary readable source directory; relative values resolve from the process current working directory; a root manifest makes it a direct package, otherwise only immediate manifested children are catalogued; the path may be inside or outside the workspace, is never persisted, and receives identity, link, manifest, duplicate-id, and collision validation before use |
| Detached rebuild source             | Every explicit workspace-relative `.agents/...` rebuild source has a locally available canonical entrypoint owner or is itself one                                                                                                                                                                                                                                                                                                    |
| Embedded Framework payload          | The running CLI contains its one coherent offline Framework payload                                                                                                                                                                                                                                                                                                                                                                   |
| Managed lifecycle state             | `.agents/open-forge.json` is absent when no lifecycle evidence is needed or valid and internally consistent for every target treated as managed                                                                                                                                                                                                                                                                                       |
| Persisted Framework exclusion       | One exact routed identity recorded in `framework.excluded`; broader coverage does not make an unstored descendant an equivalent identity                                                                                                                                                                                                                                                                                              |
| Extension source closure            | Every selected id and missing exact-id dependency resolves within the one selected embedded or external catalogue, every installed reused dependency remains valid, and the complete acyclic source set is available, exact-id consistent, and collision-free                                                                                                                                                                         |
| Reviewed external source closure    | Every selected external payload file has source-visible inspection evidence; no blocked finding exists, every review finding has dedicated authority for the exact inspected fingerprints, and no later source change occurred                                                                                                                                                                                                        |
| Extension lifecycle state           | Selected installed ids have valid flat ownership, dependency, `open-forge` or `external` source, and advisory checksum evidence                                                                                                                                                                                                                                                                                                       |
| Mutation readiness                  | Every planned effect can satisfy containment, identity, verification, Git readiness or explicit bypass, and applicable Gitless backup preflight                                                                                                                                                                                                                                                                                       |
| Shell target                        | One supported shell and its exact generated asset, default or custom activation profile when required, and link identity are resolved safely                                                                                                                                                                                                                                                                                          |

These are not booleans flattened out of domain evidence. Each focused result
retains the facts and alternatives needed for useful messages and direct
typed branching.

Git availability and relevant cleanliness are mutation-readiness evidence, not
Framework presence. Gitless operation remains available through an explicit
warning and backup decision. `--skip-git-check` bypasses the cleanliness gate
without granting any other authority.

### Universal State Rules

- A syntactically invalid or unknown command request returns `invalid`.
- A valid request whose required external or workspace state is unavailable
  returns `blocked` with the missing evidence and useful next action.
- A read-only operation that completes with safe but incomplete evidence
  returns `attention` and marks its boundary; it does not invent completeness.
- A mutation completes preflight for every selected effect before writing. One
  unavailable prerequisite or unauthorized collision blocks the entire plan.
- Read-only operations report residual backups, temporary files, or lifecycle
  disagreement without changing them.
- A mutation whose relevant Git paths are dirty blocks unless the explicit
  Git-check bypass is present and every resulting backup requirement preflights.
- Completion mutation uses the exact external target and monotonic recovery
  scope defined by the [Completion lifecycle contract](completion-lifecycle.md).
  It does not acquire or depend on a selected workspace.

### Command Matrix

| Command              | Required evidence                                                                                                                                                                                                                                                                                   | Missing or degraded evidence                                                                                                                                                                                                                                                                                                 |
| -------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `status`             | Workspace directory                                                                                                                                                                                                                                                                                 | Missing or non-directory root blocks; every Framework presence state is otherwise reportable                                                                                                                                                                                                                                 |
| `context`            | Workspace directory, rooted route inventory, and every explicitly selected route                                                                                                                                                                                                                    | Missing loader blocks; unknown selected route is invalid; unrelated malformed nodes produce explicit incomplete or attention evidence                                                                                                                                                                                        |
| `find`               | Workspace directory and rooted route inventory; every explicit route bound must resolve                                                                                                                                                                                                             | Missing loader blocks; unknown bound is invalid; safe matches remain visible beside unrelated findings                                                                                                                                                                                                                       |
| `doctor`             | Workspace directory                                                                                                                                                                                                                                                                                 | Always runs read-only across uninstalled, installed, incomplete, and conflicting presence; findings determine attention                                                                                                                                                                                                      |
| `repair`             | Workspace directory; mutation readiness only for the exact safe-repair plan                                                                                                                                                                                                                         | Uninstalled state is inspected but never implicitly installed; ambiguous lifecycle, backup, or residual evidence remains manual rather than guessed                                                                                                                                                                          |
| `create`             | Workspace directory, rooted route inventory, exact routed parent, absent destination, optional routed Template identity or workspace-contained Template source, and mutation readiness                                                                                                              | Missing parent, occupied destination, unavailable Template, or incomplete effect preflight blocks without creating topology implicitly                                                                                                                                                                                       |
| `install`            | Workspace directory, embedded Framework payload, valid managed lifecycle state, every `--restore` value resolved as an exact persisted Framework exclusion, resolved restore, keep-removed, delete, and keep decisions, explicit replacement authority when needed, and mutation readiness          | No loader or installation is required beforehand; the target is always the complete payload minus retained exclusions, an unknown or non-excluded `--restore` value is invalid, unowned existing targets remain preserved, unresolved lifecycle choices block, and one blocked effect stops the complete plan                |
| `route list`         | Workspace directory, rooted route inventory, and optional selected route                                                                                                                                                                                                                            | Missing loader blocks; an unknown selected route is invalid; unrelated route findings do not erase safe list results                                                                                                                                                                                                         |
| `route inspect`      | Workspace directory, rooted route inventory, and every selected route                                                                                                                                                                                                                               | Missing loader blocks; unknown route is invalid; an ambiguous or unsafe selected identity blocks that selection                                                                                                                                                                                                              |
| `route init`         | Workspace directory, Open Forge content path, optional routed Template identity or workspace-contained Template source, and mutation readiness                                                                                                                                                      | Missing directories and entrypoints after `.agents` become planned effects; an unsafe or incompatible segment, occupied final authoring target, unavailable Template, or incomplete effect preflight blocks the complete chain                                                                                               |
| `route rebuild`      | Workspace directory and mutation readiness, plus either the fixed loader boundary when no path is supplied or one or more explicit Open Forge content paths and their rebuild sources                                                                                                               | Framework installation is not required for explicit paths; a missing loader blocks only the no-path form; a nonexistent source is invalid, a missing owner blocks an ordinary file, and an unavailable direct parent of a selected entrypoint is reported and omitted                                                        |
| `extension list`     | Workspace directory, selected embedded or external catalogue, and optional installed lifecycle state                                                                                                                                                                                                | Runs before installation and returns the union of catalogue and installed records; absent state means unmanaged, installed ids absent from the catalogue remain `installed-only`, and malformed lifecycle state is reported without hiding safe catalogue facts                                                              |
| `extension inspect`  | Workspace directory and one exact id known through the selected catalogue, installed lifecycle evidence, or both                                                                                                                                                                                    | Unknown id is invalid; catalogue-only and installed-only identities remain inspectable, while incomplete evidence blocks only facts that cannot be proven                                                                                                                                                                    |
| `extension add`      | Workspace directory, installed Extension host, one selected Extension source closure for every selected id, reviewed external source closure when applicable, explicit collision authority when needed, and mutation readiness                                                                      | It never bootstraps Core; missing subjects in non-interactive use, missing host routes, empty or invalid catalogue, duplicate or unknown id, installed-id conflict, blocked or unapproved source finding, unauthorized collision, or unsafe destination blocks the complete batch                                            |
| `extension update`   | Workspace directory, installed Extension host, selected Extension lifecycle state, one selected Extension source closure for every selected id, reviewed external source closure when applicable, resolved dropped-file decisions, explicit collision authority when needed, and mutation readiness | Missing subjects in non-interactive use, unknown or unmanaged id, installed id absent from the selected catalogue, source-classification mismatch, blocked or unapproved source finding, divergent unauthorized target, invalid workspace record, unresolved deletion decision, or incomplete host blocks the complete batch |
| `extension remove`   | Workspace directory, selected Extension lifecycle state, enough rooted route inventory to prove retained reachability, and mutation readiness                                                                                                                                                       | A healthy canonical entry is not required, but missing loader evidence, invalid ownership, modified target, or retained dependency blocks safe detachment                                                                                                                                                                    |
| `completion install` | One or more explicit shells, `--all`, or guided safely detected targets; exact generated assets and activation profiles; proven ownership state; required external metadata and identity capabilities; and mutation readiness for every target                                                      | Non-interactive omission is invalid; unavailable or ambiguous targets, malformed ownership, unapproved divergence, or insufficient identity and metadata capability block the complete selected plan and return `completion script` guidance                                                                                 |
| `completion remove`  | One or more explicit shells, `--all`, or guided discovered owned targets; exact generated assets and activation profiles; safe ownership inspection; required external metadata and identity capabilities; and mutation readiness when owned state exists                                           | Missing owned state is a successful no-op; malformed ownership, unapproved divergence, unsafe identity, or unavailable required metadata blocks the complete selected plan                                                                                                                                                   |
| `completion script`  | One exact supported shell                                                                                                                                                                                                                                                                           | Unknown shell is invalid; no workspace, profile, Git, or mutation capability is required                                                                                                                                                                                                                                     |

### Representative Behavior

Uninstalled ordinary workspace:

```text
status                 -> success: Framework uninstalled
install                -> allowed after complete-payload preflight
context                -> blocked: .agents/loader.md is absent
extension add          -> blocked: install the Framework host first
```

Incomplete workspace with a recognizable loader but no canonical root entry:

```text
route list             -> allowed with presence attention
route rebuild          -> allowed when the fixed loader closure preflights
context                -> returns safe rooted bodies with incomplete entry evidence
extension add          -> blocked: Extension host is incomplete
install                -> allowed to reconcile the missing root entry
```

Detached Extension payload selected as the workspace:

```text
open-forge --workspace ./src/extensions/development-toolkit/payload \
  route rebuild ./.agents/templates/memory/_memory.md --dry-run
  -> allowed without AGENTS.md or .agents/loader.md
  -> rebuilds only the locally available owner, subtree, and parent

open-forge --workspace ./src/extensions/development-toolkit/payload \
  route init ./.agents/guidance/team --dry-run
  -> plans the missing local route chain without inventing a loader
```

Dirty relevant Git state:

```text
status                 -> reports relevant workspace state
doctor                 -> diagnoses it without writing
route rebuild          -> blocked by default
install                -> blocked by default
same mutation --skip-git-check
                       -> allowed only after backup and complete preflight
```

Completion generation:

```text
open-forge completion script powershell
  -> works without a selected Framework workspace
```

## Boundaries

Prerequisites gate only the capability required by the selected operation.
Framework presence is never a universal permission switch, read-only commands
do not inherit mutation requirements, and a bypass flag can waive only the
exact policy named by that flag. Missing or unproven capability blocks rather
than selecting a weaker operation path.

## Verification

- Command handlers call focused inspectors directly and receive their typed
  results through explicit parameters or direct imports.
- Domain result discriminants and diagnostic codes come from enums or readonly
  const objects; handlers do not branch on repeated raw strings.
- Direct command tests cover every matrix row's available, degraded, and
  unavailable paths without spawning the executable.
- Integration tests prove filesystem, lifecycle state, recovery, and shell
  target boundaries with real temporary state.
- End-to-end tests prove representative cross-command journeys rather than
  repeating the complete matrix through subprocesses.

## Related Current Sources

- [Mutation execution](mutation-execution.md)
- [Request construction](request-construction.md)
- [Route inventory](route-inventory.md)
- [Filesystem effects](filesystem-effects.md)
- [Recovery](workspace-recovery.md)
- [Diagnosis and repair](diagnosis-and-repair.md)
