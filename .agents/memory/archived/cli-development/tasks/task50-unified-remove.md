---
open-forge:
  description: Deliver one root remove command with explicit targets and persistent exclusions across content managers
  tags: [Memory, CLI, Task, Remove, Complete, Archived, Historical, Contextual]
---

# Task 50 — Unified Remove

## Outcome And Authority

The maintainer asked for a product assessment and specification, and explicitly
authorized implementation and parallel workers if a unified command makes sense.
The principal accepts the design below under that delegation. This task extends
the completed beta Core work; it does not activate older backlog tasks.

One root `remove` command makes sense because the user's intent is the same:
remove a selected thing from this workspace and keep it removed. Existing commands
split that intent by implementation ownership and do not consistently remember it.
Keep their proven lifecycle operations underneath the common interface. Do not
replace them with a universal mutation engine.

## Public Contract

```text
open-forge remove <target> [--kind <path|route|extension|library>]
  [--dry-run] [--automatic] [--allow-path <path>] [global flags]
```

- Exactly one target is required. `path` is the default and means an exact
  workspace-relative path. Accept portable slash-separated paths with an optional
  `./` prefix; reject rooted paths, traversal, wildcards and the workspace root.
- `route` accepts the existing source-reference grammar, including automatic IDs.
  `extension` and `library` accept their existing stable IDs. Never infer a package
  or registration from a bare path operand. Unknown kinds are invalid input.
- A path that denotes a routed source or category uses route-aware removal,
  including its overwrite companion, affected generated navigation and supported
  incoming authored links. A category directory selects its complete tree.
  An entrypoint file supplied with `--kind path` does not silently widen deletion
  to its directory: return an actionable blocked result naming the category
  directory or explicit `--kind route` selection. An ordinary file operand never
  authorizes removal of its sibling files merely because it is an entrypoint.
- Other regular files and ordinary directories can be removed, including binary
  support files and files installed by an extension. Ordinary file removal does
  not rewrite authored links; route-aware removal retains that existing behavior.
- Extension removal uninstalls one package, respects retained dependents and
  shared file owners, and leaves unrelated dependencies installed. Library removal
  detaches one local registration and its owned links; it never deletes library
  source content. A direct path removal does not follow links.
- Existing `route remove`, `extension remove`, and `library detach` remain
  supported. Their corresponding removal intent becomes persistent too. Their
  existing multi-package selection, where supported, remains available.
- Preview is effect-free, shows exclusions and filesystem/ownership effects, and
  requires no consent. Applying requires the existing confirmation policy;
  `--automatic` is the explicit noninteractive consent. `--allow-path` retains the
  existing permission meaning and cannot bypass protected paths or source guards.
- Root invocations report command `remove` through the existing schema-3 report,
  detail, severity, stream and exit conventions. Preserve concrete target-specific
  evidence and recovery details. No reflective dispatch or untyped result bag.

Examples:

```text
open-forge remove .agents/guidance/old-note.md --dry-run
open-forge remove .agents/templates --automatic
open-forge remove docs/obsolete.txt --automatic
open-forge remove planning --kind extension --automatic
open-forge remove shared-guidance --kind library --automatic
open-forge remove guidance/old-note --kind route --automatic
```

## Persistent Removal Rules

The authored `.agents/open-forge.json` remains the sole source of removal intent.
Keep schema version 1 with optional additive lists:

| Property | Meaning |
| --- | --- |
| `removedCategories` | Existing root-category exclusions under `.agents/` |
| `removedFiles` | Exact canonical workspace-relative file paths |
| `removedDirectories` | Canonical workspace-relative directory paths and all descendants, including future files |
| `removedExtensions` | Stable package IDs that must not be installed, including as dependencies |
| `removedLibraries` | Stable local library IDs that must not be attached or synchronized |

Lists are ordinal, deterministic, duplicate-free and validated. Paths use `/`,
have no trailing slash, and cannot denote the root, traversal or reserved control
state. Preserve unknown JSON properties when recording a removal. Existing
settings parsing permits comments; serialization follows its current documented
round-trip behavior. Manual deletion alone does not infer new exclusions.

Record the whole selected directory, not only its current files. Record a root
category in `removedCategories` when applicable. For an exact file removal record
all removed physical companion files. Package removal records the selected package
ID, not every package file, so a file still required by another package survives.
Library removal records the registration ID, not its source path.

Framework install/update, extension install/update and library attach/sync honor
the same path policy. Force, prune, automatic mode, dependencies and generated
navigation do not override exclusions. Explicitly selecting an excluded package
or library returns an actionable blocked result, with no partial installation;
bulk update skips excluded content. A dependency excluded by ID blocks its parent.
An excluded missing ancestor needed by selected content blocks that content rather
than silently restoring the ancestor. Existing excluded files are left untouched.

To restore, remove the corresponding exclusion from the settings and run the
appropriate install, update, attach or sync command. Use Sync for individually
removed Library links whose registration remains. This is explicit user opt-in.
No new restore command, hidden clearing of lists, or automatic rollback is added.
Users may manually create files regardless of exclusions.

The public restoration journey exposed a required Update correction: after a
category directory is removed, clearing its exclusions must allow Update to
recreate ordinary missing parents for the selected Core payload in an already
installed Framework. The readable Framework registration establishes that
installed state; schema or release metadata alone does not gate it. Directory
creation is planned, visible in preview, revalidated under the existing lease,
and verified before ownership publication. Excluded or unsafe parents remain
blocked. An empty `.agents` directory without Framework registration does not
gain this restoration behavior. This correction is part of the approved
removal/restoration round trip.

## Safety And Realization

The supported boundary is ordinary local failures, malformed data, interruptions,
concurrent edits, observable links and cooperating Open Forge processes. This
command can delete uncommitted user data, so Git alone is not recovery evidence.

Reuse physical containment, no-follow observations, expected-state validation,
workspace leases, planned effects, atomic writes and verified recovery bundles.
Protect workspace root, `.agents` itself, Git metadata, settings, ownership lock
and recovery storage. Reject targets that contain a registered library source.
Do not infer deletion authority from package payloads or unknown ownership.
Unknown or malformed required settings/ownership blocks destructive effects.

For managed path removal, release matching file/region ownership after verified
effects while preserving unrelated package registrations and claims. An explicit
file selection may remove a multiply owned file; all managers must then honor its
path exclusion. A package selection keeps existing shared-owner retention rules.
Individual Library-owned links use the established link-specific primitives,
retain the Library registration, release just their source-relative mapping and
record the concrete destination in `removedFiles`. Never follow the link target.
Unknown links, changed link identities and unsupported link kinds are blocked.
Generic path removal rebuilds affected generated navigation from intended
remaining sources through the existing Framework projection primitives, including
native Skill sources. Ordinary authored links remain unchanged in this path case.

One removal has one lease and recovery boundary. Include prior settings and lock
bytes in recovery before effects. Persist and verify exclusions before deleting
content, then publish changed ownership after verified content/navigation effects.
A later failure retains truthful exclusions and recovery evidence; it does not
undo prior effects. Revalidate the complete observed plan under the lease. Repeat
removal of an already excluded, proven absent target is an effect-free no-op.

Keep semantic owners in Operations, reusable settings/path facts in Framework,
and typed execution/presentation composition in the root host. Shared mechanisms
remain primitives, not a new generic transaction framework. Use existing BCL and
pinned packages; exceptional machinery: none.

## Frozen Settings Foundation

Add initialized immutable `RemovedDirectories`, `RemovedExtensions`, and
`RemovedLibraries` properties to `WorkspaceSettingsDocument`, retaining its
existing constructor. Extend codec validation and observation equality.

Add `WorkspaceRemovalSelection` in `Settings/Models/Mutation` with initialized
immutable `Categories`, `Files`, `Directories`, `Extensions`, and `Libraries`.
`WorkspaceSettingsCodec.AddRemovals(ReadOnlyMemory<byte>, WorkspaceRemovalSelection)`
returns changed bytes or null. `WorkspaceSettingsChangePlanner.PlanRemovals(
WorkspaceSettingsRead, WorkspaceRemovalSelection)` returns a planned create or
replace, or null. It requires a safe observation and never performs effects.

`WorkspaceRemovals` under `Settings/Shared/Planning` owns
`IsPathRemoved(string, WorkspaceSettingsDocument)`, `IsExtensionRemoved(string,
WorkspaceSettingsDocument)` and `IsLibraryRemoved(string, WorkspaceSettingsDocument)`.
All consumers use these methods; FrameworkPayloadSelection delegates path policy.

`WorkspaceOwnershipStore.PlanContentPathRelease(WorkspaceOwnershipRead,
ImmutableArray<string>, ImmutableArray<LibraryPathRelease> libraryPaths = default)` is the shared, effect-free release for exact removed
content paths. It drops matching Framework/Extension paths and generated regions,
preserves package registrations and, by default, Library records, and returns the existing
`OwnershipWritePlanResult`. Unknown ownership returns `Skipped`; destructive
callers must treat that as a blocking boundary. No matching claim is `Unchanged`
without creating or normalizing the lock. A caller that has planned verified owned
link deletion supplies `LibraryPathRelease(LibraryId, SourceRelativePath)` values
under `Ownership/Models/Mutation`. The caller uses `LibraryPathIdentity` to match
destinations; the ownership store only releases identified recorded paths. This
keeps destination interpretation in Libraries and avoids a dependency cycle from
Ownership back into Libraries. The remaining registration is preserved.

## Root Composition Boundary

Use `CliRootRemoveComposer` in the host for explicit typed execution cases.
Operations owns target classification; the host never infers ownership or reads
files itself. One System.CommandLine binding parses the root request, then the host
invokes exactly one concrete operation and its existing `CliReportPipeline`.
Keep RouteRemoveResult, ExtensionRemoveResult and LibraryDetachResult concrete.
Wrap their `CliReportRendering` selectors to set report command `remove`; retain
target-specific data, findings, effects and completion. Do not fabricate or reparse
arguments for existing command bindings. Root path removal has its own concrete
request/result and report, with invalid selection represented in that report.
No Presentation owner imports another command's models or behavior. Host
composition may connect the already-typed Operations and Presentation surfaces.

Ordinary path recovery adds the finite `Workspace` producer with wire value
`workspace`, paired only with the existing `Remove` operation. Extend the existing
attribution codec and admissible-tuple checks; use the current bundle schema and
guarded file/link target forms. Existing manager operations retain their original
recovery attribution when invoked through the root command.

## Execution And Evidence

Selected recipe: Managed Worktree Project Development. Use isolated worktrees,
one persistent worker per capability, at most four active writers, and direct
integration into this task's branch. User-authorized local commits are incremental;
workers return uncommitted patches for principal integration. No remote effects.

Phases: 1/4 specification and foundation; 2/4 domain implementation; 3/4 root
composition and documentation; 4/4 integrated verification and correction.

Independent lanes: settings foundation; route persistence and managed routes;
ordinary path removal; extension lifecycle; library lifecycle; root composition;
contract and usage documentation. Freeze any newly discovered shared seam before
dependent writers use it. Protected surfaces are other lane paths, unrelated
workspace content, toolchain/dependency configuration and the original checkout.

Unit evidence covers canonical exclusions, preservation, deterministic merging
and selection. Real filesystem integration covers dry run, removal, settings-first
ordering, recovery, changed observations, edited/shared managed content, directory
future files, no-follow boundaries, dependencies and repeat no-op. Public journeys
prove remove then install/update/sync and explicit restoration. Use focused managed
tests during development. One complete managed and supported Windows Native AOT
gate is required after integration because this changes public composition,
settings serialization and destructive mutation. One independent review targets
data loss, silent restoration, layering and contract clarity; one grouped repair
budget. Do not regenerate unrelated snapshots.

## Completion

Task 50 is complete on `codex/unified-remove`. The qualified implementation is
`1a1b4343d25c3a2119f03c6d0c2bfd1d6799e833`. The later closeout commit changes
only task status and this evidence record. No push, merge into the original
checkout, or worktree deletion occurred; the original checkout's existing edit
was preserved.

### Final Qualification

`npm run build:native -- --no-restore` completed the managed build and Windows
Native AOT CLI/test builds with zero warnings and errors. `npm run test:built`
then passed all six modes on those exact artifacts:

| Mode | Passed | Platform skips | Failed |
| --- | ---: | ---: | ---: |
| Unit | 3,424 | 0 | 0 |
| Managed Integration | 2,479 | 17 | 0 |
| Managed Public | 247 | 0 | 0 |
| Native Integration | 2,479 | 17 | 0 |
| Native Public | 247 | 0 | 0 |
| Managed Public against native CLI | 247 | 0 | 0 |

The 17 skips in each Integration mode are the expected Windows exclusions for
Unix permission/socket evidence. No skipped case is counted as a pass.
The delivery manifest records the qualified commit, `dirty: false` and
`tested: true` at `artifacts/delivery/win-x64/manifest.json`. Machine-readable
results and execution logs are in `artifacts/delivery/win-x64/reports`.
Build and gate logs are retained in `artifacts/remove-discovery/qualified-native-build.log`
and `artifacts/remove-discovery/qualified-six-mode-tests.log`.

The complete 32-step command journey passed on the final native CLI. It covers
ordinary-file preview/apply/repeat, Core category removal and restoration, actual
routed Extension-file removal and restoration, package removal/reinstall,
Library links and registrations, future descendants under excluded directories,
source preservation, and stale-claim reconciliation.
Receipt: `artifacts/remove-discovery/journeys-82e136eaf2d94229803e8d5ff909ec5e/receipt.json`.
The earlier repaired managed journey also passed all 32 steps at
`artifacts/remove-discovery/journeys-6034b9aee2ed458590be4348fc4c2612/receipt.json`.

Targeted C# whitespace validation, changed-link checks, finding-documentation
checks and `git diff --check` passed. All delegated work is complete.

### Review And Corrections

No material review finding remains open. Resolved findings cover:

- **RM-ROUTE-001**: nested Git metadata protection.
- **RM-ROOT-001–005**: reserved paths, truthful effect wording, finding identity,
  simple conditional structure, and clear plan arguments.
- **RM-UPDATE-001–003**: Core directory restoration and observed/unknown
  partial-application reporting. The dedicated partial-failure evidence uses
  typed receipts from a real plan; it does not claim a live injected failure.
- **RM-EXT-001**: excluded generated hosts remain byte-for-byte unchanged.
- **RM-PROSE-001–004**: settings-only Library removal, individual-link Sync,
  workspace-root protection, and conditional recovery retention.
- **RM-F11-001**: the public lifecycle test requires the settings-creation
  recovery entry rather than merely validating it if present.

The complete gate exposed and resolved two production mapping omissions:
the new Workspace recovery producer's shared wire name, and Route rendering's
Operations-layer dependency. Public and Unit evidence was aligned with the
accepted missing-versus-invalid required-state boundary, explicit managed-route
removal, exact settings-file fixture ownership, and recovery of previously
absent settings. Linked contract and scenario prose was reconciled with those
same decisions. A new published Root Remove journey verifies Status, Doctor,
and Cleanup against the retained Workspace recovery bundle.

Final independent reviews `r_91b11d510349`, `r_e3b4d0b2a757`,
`r_0ed6777b6fbf` and `r_1aa5fc24129c` cover these repairs and resolve the final
finding. Earlier domain reviews remain recorded in the implementation history.
Failed and interrupted intermediate gate logs remain diagnostic history, not
final qualification. One worker retained a failed-test temporary directory after
its cleanup command was rejected by execution policy; no user workspace was
removed or reset to work around that rejection.

### Local Integration History

The task branch preserves separate commits for the accepted specification,
shared ownership release, settings foundation, Route persistence, Library
lifecycle, root composition, Core restoration, Extension lifecycle, recovery
interoperability, contracts, and gate corrections. The final correction commits
are `d24d5f7a` (code and public evidence), `457284c8` (contracts and scenarios),
and `1a1b4343` (required recovery-entry evidence). Further beta backlog work and
manual template review remain separate tasks.
