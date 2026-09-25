---
open-forge:
  description: Accepted non-shipping Interface for establishing and verifying the managed root Framework lifecycle
  responsibility: Define install's exact syntax, management-establishment boundary, initial force rule, results, and read/write surface
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Install, Framework, Interface, Lifecycle, Safety, Recovery, CurrentTruth]
---

# Install Interface Contract

## Ownership Receipt Boundary

The generated `.agents/open-forge.lock.json` distinguishes whole-file paths
from regions. Root `AGENTS.md` and `CLAUDE.md` hosts carry `open-forge` region
receipts for their existing managed blocks; the host files are not whole-file
ownership. Generated Entries use `entries` region receipts. Publication follows
verified operation effects, retains unaffected verified ownership, and does not
convert a region-only edit into ownership of its authored host. A missing or
unwritable lock does not authorize wider ownership or block the operation.

The named installed content files, the generated ownership control file
`.agents/open-forge.lock.json`, and the managed host regions are separate
populations. The lock is a generated state-file effect, not an installed
content file; `AGENTS.md` and `CLAUDE.md` remain host regions rather than
installed content files. The reported directory population contains only
directories strictly below `.agents`; the `.agents` container itself is
excluded from that count even though creating it is a real first filesystem
effect when needed.

## Status And Authority

This is the accepted current Crystallized Interface Contract for the non-shipping
root `install` command. It owns the public purpose, syntax, flags, exact
Framework footprint, management-establishment states, observable effects,
results, errors, examples, non-goals, and caller-visible conformance boundary.
The new CLI does not ship yet.

The sibling [Behavior Contract](behavior.md) defines technology-neutral request
resolution, lifecycle classification, planning, verification, recovery, and
conformance. The shared [Global CLI Flags Interface](../shared/global-flags/interface.md)
defines the six global flags once. Framework routing and maintenance sources
remain authoritative for the meaning of the files that this operation consumes.

The generated `.agents/open-forge.lock.json` is the only state-file input and
output. Install updates Framework ownership after verified effects, preserves
other ownership sections, and records whole-file ownership separately from
region ownership. It stores no fingerprint baseline, workspace binding, plan,
history, or recovery evidence. Leftover records from earlier formats are ordinary
workspace files and are not read, migrated, or deleted. A skipped lock write does
not block target effects; its public publication outcome is `not-requested`.
The existing state-file outcome points at the lock, with no additional field.

The [Shared Result Coordinates](../shared/result-coordinates/interface.md) define
the exact structured JSON result schema and numeric exit mapping. This Interface uses those shared definitions without
duplicating implementation mechanics. Gate 5 must prove source-generated
YamlDotNet and STJ serialization, fixed Markdig where used, real `System.IO`,
Native AOT, OS locking, isolated tests, and package journeys. The accepted
lifecycle direction does not claim that implementation or proof.

## Purpose And Boundary

`install` establishes management of the embedded Framework in one exact
workspace. It creates a safely absent recognized Framework state or verifies an
exact trusted managed state as a no-op. It does not reconcile managed
divergence. Existing managed divergence directs the caller to the root
`update` operation.

The command has one stable root operation. Its request, current facts, intended
state, generated-navigation projection, complete plan, preflight, status model,
verification, and recovery remain the same for normal mode, initial force, and
dry-run. `--force` widens only the eligible initial-occupant boundary; it never
turns `install` into managed update, adoption, or generic replacement.

`install` may establish lifecycle facts only after the complete selected plan
has applied and verified. A dry run, incomplete result, blocked result, failed
result, or cancelled result does not publish lifecycle state.

## Syntax

The complete public command form is:

```text
open-forge install [--force] [--automatic] [--dry-run] [global flags]
```

`install` is a direct root command. It has no operands, child operations,
`framework` group, root `init`, replacement or reinstall alias, `--prune`,
`--yes`, or generic plan or apply mode.

The shared [Global CLI Flags Interface](../shared/global-flags/interface.md)
defines:

```text
--workspace <path>
--format <text|json>
--detail <minimal|standard|full|debug>
--detail debug
--help
--version
```

Those flags retain their shared grammar, defaults, repetition, composition,
terminal behavior, and errors. `--help` and `--version` stop before workspace
selection and install work. Command-specific input remains invalid with a
terminal mode.

## Exact Workspace And Source

The operation selects one exact workspace:

- Without `--workspace`, it uses the process current working directory.
- With `--workspace <path>`, it uses exactly that path, resolving a relative
  value from the process current working directory.
- It normalizes the selected path for reporting but never substitutes another
  root discovered from Git, markers, a nested `.agents`, or nearby files.
- A missing, unavailable, non-directory, physically aliased, or unsafe selected
  workspace is blocked rather than discovered around.

The source is only the current Framework payload embedded in the running CLI.
Install does not download, fetch, search for, or restore a payload from a
network, package source, or another workspace.

The CLI distribution embeds Framework and first-party Extension assets with
deterministic inventory and hash proof. That proof identifies distributed source
assets; it is not evidence of a selected workspace's current installation or of
a proven runtime implementation.

The root command consumes the neutral Framework distribution reader placed by
the [CLI Architecture](../../architecture.md). The [Embedded Payload Technical
Design](../../technical-designs/embedded-payload.md) defines how the Core project
embeds the canonical `src/open-forge/` tree through ordinary .NET
`EmbeddedResource` items and how runtime uses exact-prefix BCL manifest-resource
access. Install never reads the development checkout.

## Recognized Framework Footprint

The recognized footprint is closed. It contains only:

1. The embedded current Framework payload's named installed content
   destinations below `.agents`, including authored files and affected
   generated `Entries` regions.
2. The generated ownership control file `.agents/open-forge.lock.json` as a
   separate state-file population.
3. The exact canonical `AGENTS.md` managed block.
4. The exact supported Claude `CLAUDE.md` managed bridge block.
5. The transparent Framework lifecycle facts needed to establish or compare
   management for those targets and regions.

Generated `Entries` are derived navigation. Their expected bodies come from the
intended authored topology and metadata in the selected workspace, not from
generated interiors embedded in the payload. The current [Index Interface](../index-candidate/interface.md)
and [Index Behavior](../index-candidate/behavior.md) own the generated-region
projection and heading-boundary rules that install consumes in its one plan.

Install never expands this footprint from filename resemblance, tags, route
names, byte equality, globs, arbitrary provider files, ownership-lock claims,
an Extension-owned path, an overwrite companion, a retired-only target, or an
operand. Bytes outside valid root/provider blocks remain workspace content.

This closed footprint is the base subset selected by root Install, not the
complete set of targets that may already exist in one trusted Framework lifecycle
section. Scoped managed targets and generated regions previously added by
Framework-aware Route Init remain outside Install's selected effects and must be
preserved exactly. Their presence alone is not divergence and does not prevent an
otherwise exact root no-op.

## Operands

No operands are accepted. A directory, source reference, provider name, glob,
route, or path intended to narrow the Framework is invalid. There is no partial
footprint mode.

## Flags

| Flag                | Role                          | Value                          | Omission                                                         | Repetition and composition                                                                           |
| ------------------- | ----------------------------- | ------------------------------ | ---------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| `--force`           | Initial replacement authority | Boolean                        | Selects ordinary management establishment or exact managed no-op | Repeats idempotently. It does not imply update, prune, adoption, or ownership.                       |
| `--automatic`       | Guided-input policy           | Boolean                        | Human input may use the minimal inspection and confirmation flow | Repeats idempotently. It suppresses interaction and selects only deterministic safe defaults.        |
| `--dry-run`         | Preview write policy          | Boolean                        | Permits application after the same preflight                     | Repeats idempotently. It writes nothing and uses the same request, facts, plan, and status as apply. |
| Shared global flags | Workspace and presentation    | Defined by the shared contract | Shared defaults                                                  | Shared repetition and terminal rules apply.                                                          |

### `--force`

Normal `install` may create only safely absent current targets or verify an
exact managed state. An exact current destination occupied before management is
established is an eligible initial occupant only when complete facts establish
that it has no trusted lifecycle owner or competing manager, no route or source
collision, no ambiguous marker or containment boundary, and no unsafe recovery
condition. A manually authored or untracked occupant with a competing ownership
claim is not eligible.

`install --force` may replace only that exact recognized current occupant and
then establish management from the newly written current source after complete
verification. It records the verified result; it does not adopt the occupant's
old bytes as lifecycle history.

Force does not:

- reconcile an already managed changed, missing, retired, or source-divergent
  state;
- adopt an unowned or another-manager-owned path;
- bypass route, source, physical-identity, containment, ownership, marker,
  expected-state, bundle, verification, or recovery checks;
- repair malformed Entries headings or managed workspace markers;
- delete retired content; or
- replace bytes outside the exact current Framework footprint.

When an existing managed state diverges, both `install` and `install --force`
return `blocked`, make no write, and provide one useful `Next:` action for
`open-forge update`. Force is not an update shortcut.

### `--automatic`

`--automatic` suppresses the human inspection and confirmation flow. It selects
only the documented deterministic safe effects for the explicit `install`
operation. It never supplies initial force authority, replaces divergence,
restores missing managed content, deletes retired content, adopts content,
takes ownership, or bypasses a safety boundary.

For a safely absent workspace, automatic mode may establish the ordinary
installation. For an exact managed state, it may verify the no-op. For an
eligible initial occupant, it does not select `--force`; explicit force remains
required. Repetition is idempotent.

### `--dry-run`

Dry-run resolves the same exact workspace, source, lifecycle facts, intended
state, generated projection, complete plan, and preflight as application. It
shows every selected effect and bounded diff, but writes no payload file,
managed block, generated region, lifecycle fact, recovery bundle, temporary
artifact, or other persistent state. It cannot claim application, verification,
lifecycle publication, or bundle-handling success.

### Human Confirmation

After the complete plan and preflight succeed, a prompt-capable human apply that
would write asks exactly once for confirmation before acquiring the workspace
lease or beginning any effect. Confirmation continues with the already formed
plan. Refusal, end of input, or caller cancellation returns `cancelled` and
writes nothing. The exact decorative prompt sentence is not contract meaning.

Dry-run, verified no-op, `--automatic`, JSON, and any request without terminal-
capable stdin and stderr never prompt. A non-prompt-capable human apply that would
write is `invalid-input` unless `--automatic` is explicit; its single next action is to
rerun the same command with `--automatic`. Automatic adds no force or safety
authority.

For application with one or more existing-target effects (`Replace` or
`ReplaceGeneratedRegion`),
orchestration uses only
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.Create)` and its application-owned
`OpenForge/recovery/v1` subtree, with no temporary-directory, repository, `HOME`,
or custom-platform fallback. An operation containing only `Create` effects or
semantic or byte no-ops does not resolve recovery storage and creates no bundle.
Otherwise it prepares exactly one immutable ZIP recovery bundle
outside the workspace for the complete operation. Its deterministic external
directory key and final name use the normalized physical workspace path and
operation ID. The
source-generated schema-v1 `manifest.json` and streamed ordinal payload
entries identify the operation and normalized physical workspace, and record exact prior bytes,
lengths, hashes, ordered relative targets, change kinds, and intended final
absence or length/hash. A draft is CreateNew-written under its exact name,
closed and reopened for semantic manifest, exact ordered entry, length, hash,
and payload-byte verification, moved within the same directory to its
deterministic final name, and verified again. Only the valid final ZIP forms the
opaque `RecoveryBundlePreparation`; the draft remains `Incomplete`. Every
planned existing-target effect must match the preparation; Create and no-op effects have
none. All preparation is complete before the first effect. Unavailable storage
is `incomplete` before effects; a collision or failed final verification is
`blocked` before effects.

Directory creation is a separate effect from file Create/Replace. If the fully
preflighted plan starts without `.agents`, that exact path is the first ordinary
visible planned and reported directory-create effect. Install first acquires the
external workspace lease, then immediately revalidates the missing target and
exact contained physical parent, calls ordinary `Directory.CreateDirectory`, and
verifies the resulting contained ordinary directory. Every later missing
directory is applied parent-first through the same shared capability. A verified
created directory remains and is reported as residual state if a later effect
fails or is interrupted. Directories have no recovery entry and are never rolled
back, compensated for, or removed by Install.

Workspace mutation uses the persistent reusable zero-byte external lock under
`LocalApplicationData/OpenForge/locks/v1`, named with a display-only friendly
workspace prefix and the authoritative full SHA-256 key of the normalized
physical workspace path. The operation holds one read/write `FileShare.None`
handle and never writes metadata, truncates, or deletes the lock file. File
existence is not lock ownership. An active handle blocks mutation; lock behavior
is concurrency safety, not lifecycle authority or recovery history. The
application-owned lock and recovery subtrees are separate.

After final verification, whole-command success deletes only the positively
recognized bundle it created. `Deleted`/`Removed` permits normal completion.
`Failed`/positively observed `Retained` keeps target effects successful and
produces `completed-with-warnings`, the exact residual path, and
cleanup guidance. `Failed`/`Unknown` produces `failed` and reports an exact expected path only when the deletion result
provides one. Before post-verification deletion begins, handled application,
verification, or cancellation outcomes stop new effects and report the actual
residual draft or final path; a valid final remains when preparation completed.
A closed final ZIP may remain after abrupt process termination, without an
executable crash or power-loss guarantee. No target is automatically restored,
no current target state is derived from recovery provenance, and no journal,
progress receipt, history, or replayable plan is saved. Cleanup owns exact named
final and draft deletion under its separate lease-bound contract.

## Management States

Install distinguishes these finite states without inferring ownership from a
path, tag, route, matching bytes, or matching fingerprint:

| Current facts                                                                                                                              | Normal `install`                                                  | `install --force`                                                      | Result                                                         |
| ------------------------------------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------- | ---------------------------------------------------------------------- | -------------------------------------------------------------- |
| Safe absence: no selected Framework ownership, no occupied exact current targets, no managed root/provider block, and no recovery residual | Establish the current footprint and management after verification | Same plan; force adds no authority                                     | `completed` after verified apply or complete pre-effect dry-run |
| Trusted managed state is semantically exact                                                                                                | Verified no-op; do not rewrite format-only bytes                  | Same no-op                                                             | `completed`                                                     |
| Selected owned state differs from the running payload or is missing                                                                        | Do not reconcile; direct the caller to `update`                   | Same; force does not change the operation                              | `blocked`, no writes                                           |
| Exact current destination is an eligible initial occupant                                                                                  | Preserve it                                                       | Replace the exact occupant and establish management after verification | Normal `blocked`; eligible force `completed`                    |
| User-owned, Extension-owned, unknown, colliding, or unsafe content intersects the footprint                                                | Preserve and stop                                                 | Preserve and stop                                                      | `blocked`, no writes                                           |
| Required target or source coverage is safely unavailable                                                                                   | Do not guess                                                      | Do not broaden the footprint                                           | `incomplete`, no writes                                        |
| Required target identity, markers, or containment is malformed or ambiguous                                                                | Do not write                                                      | Do not repair or bypass                                                | `blocked`, no writes                                           |

Safe non-Framework content, user routes, Memory, overwrite companions, and
content outside the recognized footprint are preserved and do not create a
status condition by themselves.

## Ownership And Currentness

Ownership comes only from the generated lock. A missing, malformed or unreadable
lock supplies no ownership claims and never blocks because of its own state.
Existing destination occupants and actual source, marker, containment, lease,
and recovery conflicts retain their ordinary protection. Matching bytes do not
establish ownership or authorize force over an Extension-owned destination.

For selected owned targets, currentness compares current disk content with the
running payload in the same invocation. Source metadata recorded by an older
release does not gate that comparison. Root Install preserves unselected scoped
receipts and files without checking their content against a stored baseline.
An absent unselected scoped file does not block root Install or get recreated.

The operation uses the existing `open-forge-markdown-v1` comparison policy for
supported Markdown. It preserves authored significant text and whitespace and
normalizes line endings. Generated Entries are compared with the intended
projection separately from authored content. Unsupported kinds retain their
existing exact-byte fallback. No comparison fingerprint or policy is persisted.
Exact bytes are captured afresh for planning, revalidation, verification and
recovery. Equal semantic content and generated projection produce the existing
verified no-op for selected managed state.

A planned lock write is one ordinary verified state-file effect after target
verification. Its prior bytes receive the same recovery protection as other
planned replacements. Identical receipts write nothing, and an unavailable lock
publication plans no effect and reports `not-requested`.

## Generated Navigation And Ownership

Install forms one hypothetical post-install workspace from current authored
content plus permitted payload and bounded-block effects. It then projects every
affected generated region from that topology and metadata, preserving user-added
routes and intentionally absent defaults. It changes only the valid generated
body beneath the unique top-level `## Entries` heading and preserves the
heading and outside bytes. Retired generated guards inside that body are removed
when it is rewritten. A missing or duplicate Entries section blocks the plan;
force does not repair it.

Extension ownership, Framework ownership, user ownership, and external-manager
claims remain distinct. Matching semantic fingerprints do not adopt an unowned
file. Framework ownership publication preserves unrelated Extension and Library
entries in the shared lock. An unavailable publication is skipped under the
ownership contract. Install never changes Extension package sources or payload
paths claimed by another manager.

## Human Output

The command uses the shared native report. The default detail is `minimal`; `standard`, `full` and `debug` add the catalogue-defined facts. `--detail-filter <error|warning|info|all>` is repeatable and changes only the rendered detail. Use `--format text` for this text report. Primary result text for `completed`, `completed-with-warnings` and `incomplete` is on stdout; primary errors for `invalid-input`, `blocked`, `failed` and `cancelled` are on stderr. There is no `Status:` line.

### Statuses and headlines

| Status                  | When                                                 | Headline                                                                                        | Exit | Stream |
| ----------------------- | ---------------------------------------------------- | ----------------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | fresh install                                        | `Installed the Open Forge Framework into <workspace>.`                                          |    0 | stdout |
| completed               | force replaced existing files                        | `Installed the Open Forge Framework into <workspace>, replacing <N> existing files.`            |    0 | stdout |
| completed               | already installed and current                        | `Open Forge is already installed and current. Nothing to do.`                                   |    0 | stdout |
| completed (dry run)     | any plan                                             | `Would install the Open Forge Framework into <workspace>.` (+ `, replacing <N> existing files`) |    0 | stdout |
| completed-with-warnings | recovery bundle retained after success               | headline as completed + family `recovery-artifact-retained` row                                 |    2 | stdout |
| incomplete              | bundled Framework, lock or recovery store unreadable | `Install could not start: <limitation>. Nothing was changed.`                                   |    3 | stdout |
| invalid-input           | bad input; confirmation unavailable                  | family `invalid-input` / `confirmation-required`                                                |    4 | stderr |
| blocked                 | occupied paths without force                         | `Cannot install: <N> files already exist where the Framework would write.`                      |    5 | stderr |
| blocked                 | changed Framework files (managed divergence)         | `Cannot install: <N> Framework files have changed since they were installed.`                   |    5 | stderr |
| blocked                 | other boundary                                       | `Cannot install: <reason>.`                                                                     |    5 | stderr |
| failed                  | write or verification failed after effects           | `Install stopped after <n> of <m> changes.`                                                     |    1 | stderr |
| cancelled               | no at the prompt, Ctrl+C, end of input               | `Install was cancelled. Nothing was changed.`                                                   |  130 | stderr |

### Text by level

`minimal`, fresh:

```text
Installed the Open Forge Framework into D:/work/myrepo.
Workspace: D:/work/myrepo
  Created <N> files and <N> directories under .agents (listed in .agents/open-forge.lock.json).
  Created AGENTS.md and CLAUDE.md with an Open Forge section.
```

`minimal`, existing `AGENTS.md`:

```text
Installed the Open Forge Framework into D:/work/myrepo.
Workspace: D:/work/myrepo
  AGENTS.md  Open Forge section added; your content was kept
  CLAUDE.md  created with an Open Forge section
  Created <N> files and <N> directories under .agents (listed in .agents/open-forge.lock.json).
```

`minimal`, dry run:

```text
Would install the Open Forge Framework into D:/work/myrepo.
Workspace: D:/work/myrepo
  .agents/open-forge.lock.json  would be created
  Would create <N> files and <N> directories under .agents, plus AGENTS.md and CLAUDE.md.
  Nothing that already exists would be changed.
No files were changed.
```

`minimal`, occupied, `--force`:

```text
Installed the Open Forge Framework into D:/work/myrepo, replacing 2 existing files.
Workspace: D:/work/myrepo
  .agents/loader.md      replaced (your previous file is in the recovery bundle)
  .agents/maps/_maps.md  replaced (your previous file is in the recovery bundle)
  Created <N> files and <N> directories under .agents (listed in .agents/open-forge.lock.json).
  Created AGENTS.md and CLAUDE.md with an Open Forge section.
```

`minimal`, occupied without `--force` (stderr):

```text
Cannot install: 2 files already exist where the Framework would write.
  .agents/loader.md
  .agents/maps/_maps.md
Next: open-forge install --force --dry-run  (preview replacing them)
```

`minimal`, confirmation unavailable (stderr):

```text
Install needs confirmation, and this session cannot ask.
Next: open-forge install --automatic  (or --dry-run to see the plan first)
```

For partial application, the headline is `Install stopped after <n> of <m> changes.`.
The report identifies failed or unstarted effects, actual creations, and retained
recovery data. These effect counts are separate from the installed content-file
population described above.

`Workspace:` is shown at minimal detail too. `standard` additionally lists each
created content file, host-file effect, and the lock row
`  .agents/open-forge.lock.json  created; records the files above`, with child
directories summarized as a count.

`full` adds the directories as rows, the source asset path per file, the
bundled Framework fingerprint, and the recovery and verification facts in
words.

### Prompts

In a terminal without `--automatic`: plan review at `minimal` on stderr, then
`Apply these changes? [y/N]`. When existing files would be replaced under
`--force`, the question reads `Replace the 2 existing files listed above?
[y/N]`. See [04](../../../../../archived/cli-development/tasks/task30-g4/04-interaction-system.md).

### Representative transcripts by status

### Transcript — completed

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#install-completed).

### Transcript — completed-with-warnings

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#install-completed-with-warnings). [Matching reviewed capture](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Install/__snapshots__/InstallBeforeOutputSnapshotTests/ChangedFrameworkFile/changed-framework-file.minimal.txt).

### Transcript — incomplete

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#install-incomplete). [Matching reviewed capture](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Install/__snapshots__/InstallBeforeOutputSnapshotTests/RecoveryStoreUnavailable/recovery-store-unavailable.minimal.txt).

### Transcript — invalid-input

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#install-invalid-input). [Matching reviewed capture](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Install/__snapshots__/InstallBeforeOutputSnapshotTests/InvalidInput/invalid-input.standard.txt).

### Transcript — blocked

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#install-blocked). [Matching reviewed capture](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Install/__snapshots__/InstallBeforeOutputSnapshotTests/OccupiedGeneratedRegion/occupied-without-force.minimal.txt).

### Transcript — failed

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#install-failed). [Matching reviewed capture](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/__snapshots__/ExtensionInstallBeforeOutputSnapshotTests/PartialWriteFailure/write-failed-partial.minimal.txt).

### Transcript — cancelled

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#install-cancelled). [Matching reviewed capture](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/__snapshots__/ExtensionInstallBeforeOutputSnapshotTests/PackageInstallation_cancelled/cancelled.minimal.txt).

## Structured Output

`--format json` writes one schema-3 envelope to stdout for every report status. It contains the command, status, workspace when applicable, detail, filter, command data, findings, effects, counts, limitations, recovery facts and next action as applicable. It is the same typed result as the text report; no ordinary text is mixed into the JSON document. If parsing fails before binding, the raw parser diagnostic remains text on stderr and no report envelope exists.

### JSON data by level

| Level    | `data`                                                                                                                                      |
| -------- | ------------------------------------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ mode, force, automatic, classification, footprint { files, directories, sections }, lockPath }`                                          |
| standard | same                                                                                                                                        |
| full     | + `source { inventoryFingerprint }`, per-effect `sourceAssetPath` in `effects`, `lifecycle { action, outcome }`, `verification` |

`effects` lists every planned effect at every level (receipts are complete in
JSON).

## Semantic Results

The status and exit mapping above are unchanged by detail or format. Root effects and recovery receipts retain their complete result facts at every detail level; command-owned data follows the catalogue's level rows.

### Effects wording

| Effect                               | `minimal`                                                                    | `standard` row                                                   |
| ------------------------------------ | ---------------------------------------------------------------------------- | ---------------------------------------------------------------- |
| create directory                     | counted                                                                      | counted (`<N> directories created`)                        |
| create file under `.agents`          | counted named installed content file; the lock file is named separately      | `<path>  created`                                                |
| create `AGENTS.md` or `CLAUDE.md`    | `Created AGENTS.md and CLAUDE.md with an Open Forge section.`                | `<file>  created with an Open Forge section`                     |
| append section to existing host file | `<file>   Open Forge section added; your content was kept`                   | same                                                             |
| replace existing file (`--force`)    | `<path>  replaced (your previous file is in the recovery bundle)`            | same                                                             |
| create lock                          | named in the applied count sentence; explicit state-file row in preview                                        | `.agents/open-forge.lock.json  created; records the files above` |
| planned (dry run)                    | `Would ...` forms of the above                                               | same                                                             |
| not started, unknown (partial)       | listed under the partial headline with `not started` / `final state unknown` | same                                                             |

### Counts and limitations

`filesCreated`, `directoriesCreated`, `sectionsAdded`, `filesReplaced`.
`filesCreated` counts named installed content files and excludes the generated
ownership control file and host regions. `directoriesCreated` counts only
directories strictly below `.agents`; the `.agents` container is still an
ordered filesystem effect but is excluded from that count. The lock file and
host regions remain separate populations.

### Next rules

Blocked occupied -> `open-forge install --force --dry-run`; managed divergence
-> `open-forge update`; confirmation unavailable -> `open-forge install
--automatic`; partial or retained recovery -> `open-forge doctor` or
`open-forge cleanup`; completed -> none (the old `open-forge context`
suggestion is not printed; help covers it).

## Errors And Boundaries

The findings catalogue below is the command's finite error and warning vocabulary. Findings keep their code, severity, family, subject and cause; detail filtering affects display only. A blocked, failed or cancelled result prevents further effects according to the catalogue.

### Findings catalogue

| Code                                 | Severity | Family                       | Message                                                                                 | Next                                   |
| ------------------------------------ | -------- | ---------------------------- | --------------------------------------------------------------------------------------- | -------------------------------------- |
| install.invalid-input                | error    | invalid-input                |                                                                                         |                                        |
| install.confirmation-required        | error    | confirmation-required        |                                                                                         | `open-forge install --automatic`       |
| install.workspace-unavailable        | error    | workspace-unavailable        |                                                                                         |                                        |
| install.workspace-unsafe             | error    | workspace-unsafe             | also `workspace-lock-unavailable` when the lock could not be acquired                   |                                        |
| install.managed-divergence           | error    | managed-divergence           | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Install/Shared/Wording/InstallWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`install.managed-divergence`).    | `open-forge update`                    |
| install.target-occupied              | error    | target-occupied              | row `<path>` under the blocked headline                                                 | `open-forge install --force --dry-run` |
| install.ownership-conflict           | error    | ownership-conflict           |                                                                                         |                                        |
| install.target-unsafe                | error    | target-unsafe                |                                                                                         |                                        |
| install.generated-region-unsafe      | error    | generated-region-unsafe      |                                                                                         |                                        |
| install.lifecycle-blocked            | error    | lifecycle-blocked            |                                                                                         |                                        |
| install.recovery-conflict            | error    | recovery-conflict            |                                                                                         |                                        |
| install.payload-unavailable          | warning  | payload-unavailable          |                                                                                         |                                        |
| install.payload-invalid              | error    | payload-invalid              |                                                                                         |                                        |
| install.lifecycle-unavailable        | warning  | lifecycle-unavailable        |                                                                                         |                                        |
| install.projection-unavailable       | warning  | projection-unavailable       |                                                                                         |                                        |
| install.recovery-unavailable         | warning  | recovery-unavailable         |                                                                                         |                                        |
| install.recovery-artifact-retained   | warning  | recovery-artifact-retained   |                                                                                         |                                        |
| install.write-failed                 | error    | write-failed                 |                                                                                         |                                        |
| install.verification-failed          | error    | verification-failed          |                                                                                         |                                        |
| install.lifecycle-publication-failed | error    | lifecycle-publication-failed |                                                                                         |                                        |
| install.recovery-failed              | error    | recovery-failed              |                                                                                         |                                        |
| install.operation-failed             | error    | operation-failed             |                                                                                         |                                        |
| install.interrupted                  | error    | cancelled | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Install/Shared/Wording/InstallWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`install.interrupted`). |                                        |

## Scenarios

### Catalogue situations

`fresh-directory`, `fresh-directory-dry-run`, `already-installed`,
`existing-agents-md`, `occupied-without-force`, `occupied-with-force`,
`changed-framework-file` (blocked, points at update), `confirmation-unavailable`,
`recovery-store-unavailable`, `write-failed-partial`, `cancelled`,
`invalid-input`. Each at all levels, text and JSON.

Each status has one representative native text transcript above. JSON uses the same status and command facts under the schema-3 envelope.
## Non-Goals And Architecture Boundary

Install does not:

- perform managed update, reinstallation, replacement of a trusted divergent
  state, restoration of a missing managed target, or retired-content deletion;
- create a Framework group, root `init`, update/reinstall/replace/restore/recover
  alias, uninstall/remove leaf, generic apply, saved plan, session, or journal;
- discover providers, routes, package sources, Extension paths, or arbitrary
  workspace files;
- adopt matching bytes, repair markers, replace overwrite companions, or change
  user content outside valid managed regions;
- execute a formatter or persist formatter state;
- mutate the `extensions` section, the package source, or the repository
  `.temp/` directory.

If a supported formatter configuration is detected, the accepted conservative
direction allows informational advice only. Detection does not select a
formatter, execute it, change files, grant authority, make a formatting guess,
or persist formatter state.

The [Ownership And Source Alignment Technical
Design](../../technical-designs/lifecycle-provenance.md) defines ownership serialization and current source alignment, and the [Mutation And Recovery Technical
Design](../../technical-designs/mutation-and-recovery.md) defines exact recovery
and temporary-artifact mechanics. The [Embedded Payload Technical
Design](../../technical-designs/embedded-payload.md) defines exact inventory and
hash realization. The [CLI Architecture](../../architecture.md) defines
filesystem identity, diagnostic, and cross-cutting implementation boundaries.
Gate 5 must prove those boundaries and the embedded deterministic inventory/hash
evidence. This Interface remains
technology-neutral and does not claim that proof.

## Public Conformance

Future evidence must cover:

- exact root syntax, no operands, shared flags, terminal modes, and idempotent
  Boolean repetition;
- exact CWD and `--workspace` selection without discovery;
- ordinary embedded-resource inventory/byte parity and published Native AOT
  access after the binary is moved away from the checkout;
- safe absence's four facts, exact managed no-op, eligible initial occupant,
  managed divergence directing to update, and `--automatic` not supplying force;
- forgiving ownership reads without inferred ownership, ignored leftover
  records, and skipped unavailable publication;
- nullable operation-time `sourceAssetPath` on selected effects, separate
  whole-file and region receipts, and unselected scoped preservation;
- semantic equality for format-only differences, exact-byte operation facts,
  parser-proven fingerprint boundaries, and fail-closed equivalence;
- intended-topology generated projection, bounded headings, outside-byte
  preservation, and one complete lifecycle plan;
- one verified immutable external schema-v1 ZIP bundle for the complete
  operation, exact prior-byte and provenance facts, expected-state
  revalidation, per-effect and whole-operation verification, all three
  post-verification deletion state/disposition facts, residual reporting, and fresh rerun
  behavior;
- dry-run parity with no payload, lifecycle, recovery bundle, or temporary
  effects;
- the exact confirmation matrix: one post-preflight/pre-lease prompt only for a
  prompt-capable human application that would write; no prompt for dry-run,
  no-op, automatic, JSON, or non-prompt-capable requests; no-write
  `cancelled` refusal, end-of-input, and cancellation; and direct
  `--automatic` rerun guidance for a non-prompt-capable human write request;
- parent-first directory effects kept separate from file effects, with a held
  workspace lease, immediate missing-target and physical-parent revalidation,
  ordinary BCL creation, post-verification, and retained residual reporting
  without rollback, compensation, removal, or recovery provenance;
- missing `.agents` as the first ordinary visible planned/reported lease-bound
  directory-create effect, with verification and retained residual behavior;
- seven statuses, including `Failed`/positively observed `Retained` recovery
  `completed-with-warnings` and `Failed`/`Unknown` recovery `failed`, ordinary precedence,
  human streams, one-result
  JSON, bounded diagnostics, and one next action;
- no formatter execution or persisted formatter state, and no runtime
  implementation or shipping claim;
- Gate 5 evidence for source-generated serialization, fixed Markdig where used,
  real `System.IO`, Native AOT, OS locking, isolated tests, and package journeys.








## Executable Wording References

Exact wording is owned by the linked typed factories. Selection, output coordinates and behavioral requirements remain in this contract and its existing semantic owners. The independent fixture preserves the original reviewed message forms.

CLI help syntax: [`install.help.syntax`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Install/InstallText.cs).

<!-- @OpenForgeTextRef install.help.syntax -->
