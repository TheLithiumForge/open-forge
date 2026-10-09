---
open-forge:
  description: Accepted current Interface for installing, configuring and verifying built-in Framework routes
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
unwritable lock grants no wider ownership. Ordinary Install keeps best-effort
publication. Configuration must verify registration before applying Gitignore.

The named installed content files, the generated ownership control file
`.agents/open-forge.lock.json`, and the managed host regions are separate
populations. The lock is a generated state-file effect, not an installed
content file; `AGENTS.md` and `CLAUDE.md` remain host regions rather than
installed content files. The reported directory population contains only
directories strictly below `.agents`; the `.agents` container itself is
excluded from that count even though creating it is a real first filesystem
effect when needed.

## Status And Authority

This is the accepted current Crystallized Interface Contract for the root
`install` command. The CLI is available as a public beta. [CLI Distribution](../../distribution.md)
records qualified platforms and published versions. [CLI Development](../../../../../working/cli-development/_cli-development.md)
records current implementation and release work. It owns the public purpose, syntax, flags, exact
Framework footprint, management-establishment states, observable effects,
results, errors, examples, non-goals, and caller-visible conformance boundary.

The sibling [Behavior Contract](behavior.md) defines technology-neutral request
resolution, lifecycle classification, planning, verification, recovery, and
conformance. The shared [Global CLI Flags Interface](../shared/global-flags/interface.md)
defines the six global flags once. Framework routing and maintenance sources
remain authoritative for the meaning of the files that this operation consumes.

The generated `.agents/open-forge.lock.json` is the only ownership state file.
The authored `.agents/open-forge.json` supplies settings. Install updates
Framework ownership after verified effects, preserves
other ownership sections, and records whole-file ownership separately from
region ownership. Its optional route-sharing pairs grant no deletion authority.
It stores no fingerprint baseline, workspace binding, plan,
history, or recovery evidence. Leftover records from earlier formats are ordinary
workspace files and are not read, migrated, or deleted. For ordinary Install, a
skipped lock write does not block target effects and its publication outcome is
`not-requested`. Explicit configuration verifies registration before Gitignore.
The existing state-file outcome points at the lock, with no additional field.

The [Shared Result Coordinates](../shared/result-coordinates/interface.md) define
the exact structured JSON result schema and numeric exit mapping. This Interface uses those shared definitions without
duplicating implementation mechanics. Gate 5 must prove source-generated
YamlDotNet and STJ serialization, fixed Markdig where used, real `System.IO`,
Native AOT, OS locking, isolated tests, and package journeys. The accepted
lifecycle direction does not claim that implementation or proof.

## Purpose And Boundary

`install` establishes management of the embedded Framework in one exact
workspace. It establishes a safely absent Framework state or completes the
bounded initial workspace adoption described below before verifying the
resulting state. It verifies an exact trusted managed state as a no-op. It does
not reconcile divergence in authored managed payload. Existing authored
managed divergence directs the caller to the root `update` operation.
Explicit configuration additionally selects built-in routes, restores eligible
missing defaults, and changes the frontmatter form through the bounded rules below.

The command has one stable root operation. Its request, current facts, intended
state, generated-navigation projection, complete plan, preflight, status model,
verification, and recovery remain the same for safe absence, bounded initial
adoption, initial force, exact managed no-op, and dry-run. Ordinary install
does not require a new flag or `--force` for the bounded adoption below.
`--force` widens only the existing eligible initial-occupant boundary. It
does not authorize adoption and does not turn `install` into managed update
or generic replacement.

`install` may establish lifecycle facts only after the complete selected plan
has applied and verified. A dry run, incomplete result, blocked result, failed
result, or cancelled result does not publish lifecycle state.

## Syntax

The complete public command form is:

```text
open-forge install [--configure] [--preset <essentials|full-core|custom>]
  [--frontmatter <root|scoped>]
  [--route <id>=<add|remove|git-ignore>...] [--force] [--automatic] [--dry-run] [global flags]
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

Explicit setup selection also plans the authored `.agents/open-forge.json`
exclusions and the bounded Install-owned `.gitignore` section. These are
configuration effects, not embedded installed-content ownership.

Every fresh Install also plans the frontmatter setting. Configure's bounded
conversion additionally selects owned eligible Framework and Extension delivered
targets, including scoped copies, without expanding ordinary Install's footprint.

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

The embedded payload footprint remains closed. During safe initial
establishment, Install may form a separate set of user-adoption targets for
missing compatible required metadata and route entrypoints in the selected
standard route subtree. These targets do not become embedded payload targets
or Framework-owned whole files. The following section defines this separate
population.

## Initial Workspace Adoption

Ordinary root Install automatically adds missing compatible required metadata
and route entrypoints in the selected standard route subtree when they are
needed for routability. This is part of initial establishment and requires no
new flag or `--force`. An exact managed state with no effects remains
`TrustedExact`. When its authored managed base verifies and scoped workspace
adoption or the generated navigation required by that adoption has effects,
Install completes those effects without `--force` and sets schema-3
`data.classification` to `managed-adoption`; this does not reconcile authored
managed payload divergence. Existing embedded payload collisions keep the
current occupancy and explicit force rules.

Managed-base admission verifies each selected non-user payload target's
authored source fingerprint and required managed-block contents through the
existing readers. Planned user-owned targets are excluded only from this base
admission and remain subject to exact intended-byte verification after apply.
A generated `Entries` fingerprint may differ only for a safely projected
adoption change whose authored fingerprint still matches. Preserve bounded
generated-span checks before writes. The occupancy exception covers only
verified managed targets and planned user-owned targets; it does not broaden
initial-establishment `--force`.

Install reuses a unique recognized entrypoint. It creates a missing canonical
`_{folder-name}.md` entrypoint only when the selected standard route needs it,
or adds a missing `## Entries` section when that section is required for the
selected route. It honors `removedFiles` and removed defaults and does not restore an
intentionally absent route or destination. Ordinary catalogue observation may
read other safe sources under its existing selection rules, but Install does not
adopt or normalize unrelated content. It never follows an outside junction
target. Binary files, overwrite companions, and content outside selected routes
remain unchanged.

Metadata completion adds only missing compatible required fields. Existing
valid fields stay byte-for-byte unchanged. Optional or absent metadata that is
already allowed remains absent. Completion preserves original bodies, unknown
YAML fields, encoding, newline style, and user content. It does not normalize
the file or reorder existing metadata.

A native `SKILL.md` keeps its native format and semantics. Install completes a
missing required native `name` or `description` when its value is derivable.
It does not add an `open-forge:` wrapper or an `Entries` section to a Skill.
Existing complete
native fields, including optional `license`, remain untouched. A missing
native `name` comes from the containing Skill directory name. A missing
description uses an existing usable description, then an existing usable
title, then the first top-level H1 heading, then the workspace-relative path.
Routed resource catalogues below `references` follow the accepted Task 47
selection boundary.

For other required route descriptions, Install uses an existing usable
description, then an existing usable title, then the first top-level H1 heading,
then the workspace-relative path. A required tag may be added only when the
new completion actually needs ordinary classification. In that case
`Workspace` is a search tag. Install does not invent loading, behavior,
authority, or state tags, and it does not add optional fields just because a
candidate lacks them.

Install observes Framework, Extension, and Library ownership before forming
adoption candidates. An absent ownership file is known empty. An invalid or
unreadable existing lock blocks every Install effect because route-sharing policy
is unavailable. Recognized data in an unknown schema keeps the forgiving reader
behavior; schema version alone does not block. Ordinary best-effort ownership
publication remains a separate write boundary. Unavailable claims cannot license
adoption when a competing claim cannot be ruled out. A known competing claim, payload collision, ambiguous entrypoint,
malformed metadata, conflicting input, or unsafe physical boundary blocks the
affected plan with a precise finding before any write. Ownership never grants
ownership or expands the candidate set.

Install forms prospective source bytes and route facts after these planned
completions, then derives affected navigation through the existing Index
projection. This exception does not relax the shared Index parser, selector,
or projector. Extension adoption is not part of this change. Any later
Extension behavior requires a separate bounded decision over its affected
projection closure.

## Operands

No operands are accepted. A directory, source reference, provider name, glob,
route, or path supplied as an operand is invalid. Setup selection uses the
named flags below, not arbitrary destination paths.

## Flags

| Flag                | Role                          | Value                               | Omission                                                         | Repetition and composition                                                                                       |
| ------------------- | ----------------------------- | ----------------------------------- | ---------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- |
| `--force`           | Initial replacement authority | Boolean                             | Selects ordinary management establishment or exact managed no-op | Repeats idempotently. It does not imply update, prune, adoption, or ownership.                                   |
| `--automatic`       | Guided-input policy           | Boolean                             | Human input may use the minimal inspection and confirmation flow | Repeats idempotently. It suppresses interaction and selects only deterministic safe defaults.                    |
| `--dry-run`         | Preview write policy          | Boolean                             | Permits application after the same preflight                     | Repeats idempotently. It writes nothing and uses the same request, facts, plan, and status as apply.             |
| `--configure`       | Explicit setup change         | Boolean                             | Ordinary Install verification                                    | Repeats idempotently. Selects additive route configuration and bounded frontmatter conversion without general Update or deletion authority. |
| `--preset`          | Built-in setup selection      | `essentials`, `full-core`, `custom` | Interactive first setup or deterministic unattended defaults     | Repeated identical values are idempotent. Conflicting values are invalid. Existing setup requires `--configure`. |
| `--frontmatter`     | Metadata output form          | `root`, `scoped`                    | Workspace preference, with root as the fresh unattended default | Repeated identical values are idempotent. Conflicting values are invalid. An installed workspace requires `--configure`. |
| `--route`           | Custom row override           | `<id>=<add\|remove\|git-ignore>`    | Retain the Custom base row                                       | Repeat for different rows. Requires Custom. Conflicting actions for one row are invalid.                         |
| Shared global flags | Workspace and presentation    | Defined by the shared contract      | Shared defaults                                                  | Shared repetition and terminal rules apply.                                                                      |

### Setup Selection

`--configure` changes built-in route choices or the frontmatter form and restores
eligible missing defaults. Route configuration remains additive. The narrow
conversion rule below is its only payload-replacement exception. It grants no
general Update or route deletion authority. An explicit `--preset` or
`--frontmatter` on an installed workspace requires `--configure`.

`install --configure --frontmatter <form>` is valid without a preset and keeps
route choices unchanged.

The presets are Essentials, Full Core and Custom. Essentials includes
Directives, Patterns, Skills, Emerging and Crystallized Memory. It installs
the complete ordinary Working route and Git-ignores its contents while keeping
its entrypoint eligible for Git. Guidance,
Maps, Templates and Archived Memory are omitted. Full Core selects every
built-in category and Memory state, with no preset Git-ignore entries. Neither
preset installs optional Extensions.

Custom exposes Directives, Guidance, Maps, Patterns, Skills, Templates, and
the four built-in Memory states. Each row is Add (`+`), Add + Git-ignore (`~`),
or Remove (`-`).
Remove omits supplied defaults and releases only their Framework management.
Existing files, authored notes and overwrite companions are retained and remain
routable. The wizard states this for a focused Remove row in an installed
workspace. Add + Git-ignore installs the
ordinary route, records its sharing policy in the lock, then adds a contents
pattern and entrypoint exception to its section of `.gitignore`. Index omits
private contents from generated Entries. Context, Find and References still
read local content. Files already tracked by Git stay tracked.

`--preset` accepts `essentials`, `full-core`, or `custom`. Custom starts from
the current concrete selections in an existing workspace and Essentials in
a fresh workspace. Repeatable `--route` overrides those rows and requires
`--preset custom`. Accepted IDs are `directives`, `guidance`, `maps`,
`patterns`, `skills`, `templates`, `memory/working`, `memory/emerging`,
`memory/crystallized`, and `memory/archived`. Accepted actions are `add`,
`remove`, and `git-ignore`. Unknown IDs/actions and conflicting duplicate
values are invalid input. Identical repeated values are idempotent.

A first prompt-capable interactive apply offers the three presets, with
Essentials first. Explicit `--configure` offers selection even on an installed
workspace. Custom then shows one marked list of the ten rows with their current
marks, under the question `Choose what Open Forge sets up` and the legend
`+ add   ~ add, keep contents out of Git   - leave out`. Each row shows its ID
and a short summary. Rows supplied by `--route` are locked. The shared marked
list primitive defines the keys and line mode. Selection completes before the
one immutable plan and final application confirmation. Cancellation writes
nothing.

```text
Choose what Open Forge sets up
+ add   ~ add, keep contents out of Git   - leave out

> [+] directives           Rules agents must follow
  [-] guidance             Advice for recurring choices
  [-] maps                 Pointers to important sources
  [+] patterns             Reusable shapes for code and documents
  [+] skills               Packaged agent capabilities
  [-] templates            Copy-ready starter files
  [~] memory/working       Notes for active work
  [+] memory/emerging      Findings not accepted yet
  [+] memory/crystallized  Accepted knowledge
  [-] memory/archived      Completed and historical records

up/down move   + ~ - set   space next   enter done   esc cancel
```
Ordinary repeated Install remains quiet when its exact state is already current.

After the preset choice, first interactive Install asks
`How should Open Forge write file metadata?` with root preselected. The choices are:

| Choice | Explanation |
| ------ | ----------- |
| `Root keys` | `description: and tags: at the top level` |
| `Scoped under open-forge:` | `open-forge: holds description: and tags:` |

An explicit `--frontmatter` skips the question. Every fresh Install writes the
resolved `frontmatter` key. A fresh unattended Install uses the flag, then an
explicit preference already in settings, otherwise root. Ordinary repeated
Install and Update never ask and never write the key.

Dry-run, automatic, JSON and redirected requests never ask setup questions.
Without explicit setup input, ordinary unattended first Install retains Full
Core and existing omissions. Noninteractive `--configure` requires an explicit
`--preset` or `--frontmatter`. Noninteractive Custom applies its deterministic
base plus supplied
row overrides. These requests use the same plan as an interactive equivalent.

Only exact selected root/state exclusions and required ancestor exclusions
may change. Preserve narrower exclusions, unknown settings members, unrelated
Git-ignore text, and unselected ownership. Do not persist a second preset
identity. A fresh checkout may restore ignored packaged scaffolding through
explicit configuration. It cannot recover unshared private records.

### Configure Conversion

As a narrow exception to Configure's additive-only rule, Configure compares
each owned eligible delivered target with the payload rendered in the selected
form and in the other form. A match with the selected form needs no effect. A
match with the other form becomes one whole-file replacement in the selected
form with projected `Entries`.

Targets matching neither form, including edited files and Extension files whose
source is unavailable, are kept unchanged and reported. Excluded, user-authored,
Library, and overwrite files are never touched. Retention under this rule does
not block the form change. The settings write and conversions form one reviewed
plan with ordinary recovery and the existing safety and permission checks.

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
- select or widen bounded initial adoption, or adopt a path claimed by another
  manager;
- bypass route, source, physical-identity, containment, ownership, marker,
  expected-state, bundle, verification, or recovery checks;
- repair malformed Entries headings or managed workspace markers;
- delete retired content; or
- replace bytes outside the exact current Framework footprint.

Outside Configure's bounded conversion, when an existing managed state diverges,
both `install` and `install --force`
return `blocked`, make no write, and provide one useful `Next:` action for
`open-forge update`. Force is not an update shortcut.

### `--automatic`

`--automatic` suppresses the human inspection and confirmation flow. It selects
only the documented deterministic safe effects for the explicit `install`
operation, including eligible bounded initial metadata and route completion. It
never supplies initial force authority, replaces divergence, restores missing
managed content without explicit configuration, deletes retired content, changes adoption eligibility or
widens its candidate set, takes ownership, or bypasses a safety boundary.

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

Setup selection happens before planning as defined above. Final confirmation
still occurs exactly once for a writing plan. Dry-run, ordinary verified no-op,
`--automatic`, JSON, and any request without terminal-capable stdin and stderr
never prompt. A non-prompt-capable human apply that would
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

| Current facts                                                                                                                                         | Normal `install`                                                  | `install --force`                                                      | Result                                                          |
| ----------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------- | ---------------------------------------------------------------------- | --------------------------------------------------------------- |
| Safe absence: no selected Framework ownership, no occupied exact current targets, no managed root/provider block, and no current recovery obstruction | Establish the current footprint and management after verification | Same plan; force adds no authority                                     | `completed` after verified apply or complete pre-effect dry-run |
| Trusted managed state is semantically exact                                                                                                           | Verified no-op; do not rewrite format-only bytes                  | Same no-op                                                             | `completed`                                                     |
| Selected owned state differs from the running payload or is missing                                                                                   | Do not reconcile; direct the caller to `update`                   | Same; force does not change the operation                              | `blocked`, no writes                                            |
| Exact current destination is an eligible initial occupant                                                                                             | Preserve it                                                       | Replace the exact occupant and establish management after verification | Normal `blocked`; eligible force `completed`                    |
| Non-adoptable user-owned, Extension-owned, unknown, colliding, or unsafe content intersects the embedded payload footprint                            | Preserve and stop                                                 | Preserve and stop                                                      | `blocked`, no writes                                            |
| Safe initial adoption targets require compatible metadata or route completion                                                                         | Complete the required fields and routes, then verify              | Same bounded plan                                                      | `completed` after verified apply or complete pre-effect dry-run |
| Required target or source coverage is safely unavailable                                                                                              | Do not guess                                                      | Do not broaden the footprint                                           | `incomplete`, no writes                                         |
| Required target identity, markers, or containment is malformed or ambiguous                                                                           | Do not write                                                      | Do not repair or bypass                                                | `blocked`, no writes                                            |

Safe non-Framework content, user routes, Memory, overwrite companions, and
content outside the recognized footprint are preserved and do not create a
status condition by themselves.

## Ownership And Currentness

Ownership comes only from the generated lock. A missing lock supplies empty
legacy sharing facts and no ownership claims. An invalid or unreadable existing
lock blocks Install before effects because route-sharing policy is unavailable;
`--force` cannot bypass this read prerequisite.
Existing destination occupants and actual source, marker, containment, lease,
and recovery conflicts retain their ordinary protection. Matching bytes do not
establish ownership or authorize force over an Extension-owned destination.
For initial adoption, missing or unusable ownership facts never act as proof
that a candidate has no competing manager.

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
content, permitted payload and bounded-block effects, and eligible prospective
metadata and route completions. It then projects every affected generated
region from that topology and metadata, preserving user-added routes and
intentionally absent defaults. It changes only the valid generated body beneath
the unique top-level `## Entries` heading and preserves the heading and outside
bytes. Retired generated guards inside that body are removed when it is
rewritten. The final prospective document must have one unique safe Entries
boundary. Authorized adoption may append a missing section first; duplicate or
ambiguous boundaries still block. This does not relax the shared Index parser,
selector, or projector.

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
| completed               | `--configure` on an installed workspace              | `Changed the Open Forge setup in <workspace>.`                                                  |    0 | stdout |
| completed               | already installed and current                        | `Open Forge is already installed and current. Nothing to do.`                                   |    0 | stdout |
| completed (dry run)     | install plan                                         | `Would install the Open Forge Framework into <workspace>.` (+ `, replacing <N> existing files`) |    0 | stdout |
| completed (dry run)     | `--configure` plan on an installed workspace         | `Would change the Open Forge setup in <workspace>.`                                             |    0 | stdout |
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
  AGENTS.md  Open Forge section added, your content was kept
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

`minimal`, Configure dry run that adds Guidance and Git-ignores Archived Memory:

```text
Would change the Open Forge setup in D:/work/myrepo.
Workspace: D:/work/myrepo
Frontmatter: root
  .agents/loader.md             Entries would be updated
  .agents/memory/_memory.md     Entries would be updated
  .agents/open-forge.json       settings would be updated
  .agents/open-forge.lock.json  ownership record would be updated
  .gitignore                    Open Forge Git-ignore rules would be updated
  Would create 2 files and 2 directories under .agents, and update 5 existing files.
No files were changed.
```

`minimal`, occupied, `--force`:

```text
Installed the Open Forge Framework into D:/work/myrepo, replacing 2 existing files.
Workspace: D:/work/myrepo
  .agents/loader.md      replaced
  .agents/maps/_maps.md  replaced
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

Each row names the actual change. The preview form uses `would be` or `would`.

| Change | Row label |
| ------ | --------- |
| New file or directory | `created` |
| New `AGENTS.md` or `CLAUDE.md` | `created with an Open Forge section` |
| Section added to an existing host file | `Open Forge section added, your content was kept` |
| Open Forge section in a host file refreshed | `Open Forge section updated` |
| Generated `Entries` in an existing entrypoint or the loader | `Entries updated` |
| Install-owned Git-ignore section added or changed | `Open Forge Git-ignore rules added` or `Open Forge Git-ignore rules updated` |
| Existing `.agents/open-forge.json` | `settings updated` |
| Existing ownership record | `ownership record updated` |
| Configure frontmatter conversion | `metadata moved to root keys` or `metadata moved under open-forge:` |
| Missing metadata completed in an existing file | `metadata completed, your content was kept` |
| Whole-file replacement | `replaced`, plus `(your previous file is in the recovery bundle)` only when the bundle was retained |

`replacing <N> existing files` in a headline counts only whole-file
replacements. One summary line counts created files and directories under
`.agents` and updated existing files. `Nothing that already exists would be
changed.` appears only when no existing file would change.

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

The plan shows `Frontmatter: root` or `Frontmatter: scoped`. When the form
changes, it shows the transition, such as `Frontmatter: scoped -> root`.

In a terminal without `--automatic`: plan review at `minimal` on stderr, then
`Apply these changes? [y/N]`. When the plan replaces whole existing files, the
question reads `Apply these changes, including replacing 2 existing files? [y/N]`
with the actual count. Entries, section, settings, ownership record, Git-ignore
and frontmatter conversion changes do not change the question.
Configuration retains its bounded authority without requiring `--force`;
initial payload replacement still requires it. See
[04](../../../../../archived/cli-development/tasks/task30-g4/04-interaction-system.md).

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

`data.frontmatter` contains `form` as `root` or `scoped` at every detail level
when the form is resolved. It also contains `previousForm` when the form changed,
and `kept` at every detail level when Configure kept files in their previous
form. It is separate from `data.configuration`.

| Level    | `data`                                                                                                                          |
| -------- | ------------------------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ mode, force, automatic, classification, footprint { files, directories, sections }, lockPath, [migrations], [frontmatter { form, [previousForm], [kept] }] }` |
| standard | same                                                                                                                            |
| full     | + `source { inventoryFingerprint }`, per-effect `sourceAssetPath` in `effects`, `lifecycle { action, outcome }`, `verification` |

`effects` lists every planned effect at every level (receipts are complete in
JSON). Internally, the migration collection is empty on a no-op. Public
schema-3 `data.migrations` is optional and omitted when there are no migration
rows, preserving existing no-op JSON snapshots. When rows exist, the array is
present at every detail level.

When concrete setup selection is resolved, optional `data.configuration`
contains `configure`, `preset`, and ordered `routes` rows with `id` and `action`.
It is present at every detail level for those requests and omitted for ordinary
requests with no setup selection. This is a result of the current request, not
persisted workspace mode state. Configuration effects use the existing complete
effect receipts, verification and recovery coordinates.

Each migration row has exactly these fields: `path`, `actions`, `fields`,
`derivation`, and `outcome`. All keys are present, and the `actions`,
`fields`, and `derivation` arrays are non-null. `path` is a canonical
workspace-relative path.
`actions` contains the applicable values from `metadata-completed`,
`entrypoint-created`, `entries-section-added`, `navigation-updated`, and
`content-preserved`. The last action identifies the user-owned overwrite
companion that preserves an existing category entrypoint's authored content.
`fields` lists only the metadata fields actually affected, such as `name`,
`description`, or `open-forge.description`. `derivation` is an array of
`{ field, source }` rows. Its source values are `existing-description`,
`existing-title`, `heading`, `relative-path`, `directory-name`, and
`required-tag`. `outcome` is `planned` or `applied`. A row reports
`applied` only after every effect it represents has verified. Unapplied or
failed rows remain `planned`, while the underlying effect outcomes remain
authoritative.

Migration rows are informational. They do not form warning or error findings
and do not change status.

## Semantic Results

The status and exit mapping above are unchanged by detail or format. Root effects and recovery receipts retain their complete result facts at every detail level; command-owned data follows the catalogue's level rows.

### Effects wording

| Effect                               | `minimal`                                                                    | `standard` row                                                   |
| ------------------------------------ | ---------------------------------------------------------------------------- | ---------------------------------------------------------------- |
| create directory                     | counted                                                                      | counted (`<N> directories created`)                              |
| create file under `.agents`          | counted named installed content file; the lock file is named separately      | `<path>  created`                                                |
| create `AGENTS.md` or `CLAUDE.md`    | `Created AGENTS.md and CLAUDE.md with an Open Forge section.`                | `<file>  created with an Open Forge section`                     |
| append section to existing host file | `<file>   Open Forge section added; your content was kept`                   | same                                                             |
| replace existing file (`--force`)    | `<path>  replaced (your previous file is in the recovery bundle)`            | same                                                             |
| create lock                          | named in the applied count sentence; explicit state-file row in preview      | `.agents/open-forge.lock.json  created; records the files above` |
| planned (dry run)                    | `Would ...` forms of the above                                               | same                                                             |
| not started, unknown (partial)       | listed under the partial headline with `not started` / `final state unknown` | same                                                             |

### Migration wording

When text detail includes a migration row, a dry run reports it as
`Planned migration: <path> (<summary>).` A verified apply reports it as
`Migrated <path>: <summary>.` The summary names the affected fields or route
change. These informational lines do not change status or become findings.

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

| Code                                 | Severity | Family                       | Message                                                                                                                                                                                                                                                                                                                              | Next                                   |
| ------------------------------------ | -------- | ---------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | -------------------------------------- |
| install.invalid-input                | error    | invalid-input                |                                                                                                                                                                                                                                                                                                                                      |                                        |
| install.confirmation-required        | error    | confirmation-required        |                                                                                                                                                                                                                                                                                                                                      | `open-forge install --automatic`       |
| install.workspace-unavailable        | error    | workspace-unavailable        |                                                                                                                                                                                                                                                                                                                                      |                                        |
| install.workspace-unsafe             | error    | workspace-unsafe             | also `workspace-lock-unavailable` when the lock could not be acquired                                                                                                                                                                                                                                                                |                                        |
| install.managed-divergence           | error    | managed-divergence           | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Install/Shared/Wording/InstallWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`install.managed-divergence`). | `open-forge update`                    |
| install.target-occupied              | error    | target-occupied              | row `<path>` under the blocked headline                                                                                                                                                                                                                                                                                              | `open-forge install --force --dry-run` |
| install.ownership-conflict           | error    | ownership-conflict           |                                                                                                                                                                                                                                                                                                                                      |                                        |
| install.target-unsafe                | error    | target-unsafe                |                                                                                                                                                                                                                                                                                                                                      |                                        |
| install.generated-region-unsafe      | error    | generated-region-unsafe      |                                                                                                                                                                                                                                                                                                                                      |                                        |
| install.lifecycle-blocked            | error    | lifecycle-blocked            |                                                                                                                                                                                                                                                                                                                                      |                                        |
| install.recovery-conflict            | error    | recovery-conflict            |                                                                                                                                                                                                                                                                                                                                      |                                        |
| install.payload-unavailable          | warning  | payload-unavailable          |                                                                                                                                                                                                                                                                                                                                      |                                        |
| install.payload-invalid              | error    | payload-invalid              |                                                                                                                                                                                                                                                                                                                                      |                                        |
| install.lifecycle-unavailable        | warning  | lifecycle-unavailable        |                                                                                                                                                                                                                                                                                                                                      |                                        |
| install.projection-unavailable       | warning  | projection-unavailable       |                                                                                                                                                                                                                                                                                                                                      |                                        |
| install.recovery-unavailable         | warning  | recovery-unavailable         |                                                                                                                                                                                                                                                                                                                                      |                                        |
| install.recovery-artifact-retained   | warning  | recovery-artifact-retained   |                                                                                                                                                                                                                                                                                                                                      |                                        |
| install.write-failed                 | error    | write-failed                 |                                                                                                                                                                                                                                                                                                                                      |                                        |
| install.verification-failed          | error    | verification-failed          |                                                                                                                                                                                                                                                                                                                                      |                                        |
| install.lifecycle-publication-failed | error    | lifecycle-publication-failed |                                                                                                                                                                                                                                                                                                                                      |                                        |
| install.recovery-failed              | error    | recovery-failed              |                                                                                                                                                                                                                                                                                                                                      |                                        |
| install.operation-failed             | error    | operation-failed             |                                                                                                                                                                                                                                                                                                                                      |                                        |
| install.interrupted                  | error    | cancelled                    | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Install/Shared/Wording/InstallWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`install.interrupted`).        |                                        |

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
  state beyond Configure's bounded form conversion, restoration without explicit setup selection, or retired-content deletion.
- create a Framework group, root `init`, update/reinstall/replace/restore/recover
  alias, uninstall/remove leaf, generic apply, saved plan, session, or journal;
- discover providers or arbitrary workspace files. Adoption route discovery is
  limited to the selected standard route subtree. Configure source observations
  are limited to owned eligible delivered targets under its conversion rule.
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
- safe initial metadata and route completion in an existing workspace,
  preserving native Skill semantics, valid metadata, unknown YAML, bodies,
  encoding, newlines, user routes, and ownership; preserve exact unowned
  category-entrypoint collisions in overwrite companions and unowned native
  Skills in place, while other payload collisions retain the force boundary;
  verified managed-base adoption
  must classify as `managed-adoption`, preserve owned navigation-host role, and
  leave genuine authored managed divergence blocked;
- the pinned Skill Creator brownfield and already-managed-root process
  acceptance, with exact commands and fixture hashes in the Behavior Contract
  and Task70 acceptance record;
- exact migration rows in schema 3, omitted public no-op migrations, planned dry-run
  outcomes, verified-only applied outcomes, and informational text that never
  changes status;
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
