---
open-forge:
  description: Review the concrete Task 24 content rename, permission contracts, implementation seams and decisive evidence before M1 freeze
  tags: [Memory, Archived, Contextual, Historical, CLI, Extension, Permission, Contract, Design, Candidate]
---

# Extensions Evolution Contract Draft

## Boundary And Authority

This is the retained M1 preparation pack for [Task 24](extensions-evolution.md), based on
`a157d439a8f130ef80ad7a87b0d3fcc6b7440d4d`. It refines the user-accepted
`content/`, consumer allowlist and missing-grant question. The
[functional proposal](extensions-destination-proposal.md) supplies the examples.
The reviewed shared Interface/Behavior and Technical Design are now promoted
into their permanent CLI routes. Those sources own current meaning; this draft
retains review context and is not a second maintained authority. Task 25 owns Library projection behavior and external path selection.

## Package Directory

The replacement CLI reads, creates, inventories and embeds `content/` only.
Each file below it retains its workspace-relative destination. Change physical
first-party Extension folders, embedded asset keys and hashes, scaffold paths,
help, examples and focused evidence together. The user explicitly excludes legacy handling: update our own packages and
use `content/` throughout the replacement CLI. Do not add old-folder detection,
diagnostics, aliases or migration. A package without `content/` retains the
existing empty-package semantics.

The rename does not rename JSON members such as `payload` or `payloadTargets`,
internal payload concepts, or stable effect tags. Those describe contributed
data, rather than a directory. Path values in them use `content/` when they
identify source files. Manifest and lifecycle schemas stay unchanged. Installed
ownership identifies destinations and does not depend on the package folder.
The frozen legacy `src/cli-mvp/` implementation remains historical; its source
is excluded from this replacement-CLI change. Do not regenerate its behavior.

## Permission Document Interface

The consumer owns `.agents/open-forge.permissions.json`. Schema version 1 has
exactly `schemaVersion`, `extensions` and `libraries` at the root. Both arrays
are required. Each Extension entry contains exactly `id` and `paths`; each
Library entry contains exactly `id`, `sourceRoot` and `paths`. IDs retain their
existing command grammars. Library source roots retain the accepted contained
workspace-relative grammar. The document is independent of ownership records.

Read strict UTF-8 JSON, rejecting unknown or duplicate properties, unsupported
versions, missing properties, nulls and incorrect types. Reject duplicate
identities, duplicate portable paths within one identity, and invalid paths.
Accept entry and path order supplied by a human; canonical writes sort entries
by ordinal ID and paths by ordinal spelling, use two-space indentation and a
final LF. A write preserves unrelated valid grants semantically. The recovery
bundle preserves exact prior bytes, including formatting. No comments or
trailing commas are supported.

A grant names one canonical, portable, workspace-relative file outside
`.agents/`. No absolute, empty, parent, dot, backslash, wildcard, trailing slash,
non-NFC, device-name or trailing-dot/space alias is accepted. Portable identity
uses the existing invariant case-insensitive target key; divergent spellings of
one portable path are rejected within an entry. A directory grant is invalid.
The document cannot grant ownership, force, source selection, executable trust,
or permission to mutate a Library source. Extension and Library identities are
different subjects even when their IDs match. A Library grant binds sourceRoot.

Absence means no external grants. A malformed, linked, unsafe or unreadable
document never becomes an empty grant set and is never overwritten by approval.
Commands requiring no external destination do not depend on this document.
Read-only commands do not prompt or change it. A package's own permission file
is content and cannot supply consumer approval; its control-file destination is
reserved and cannot be installed.

## Destination Admission

All prior ownership, collision, Framework-anchor, generated-region, overwrite,
source-disjointness and physical containment checks remain necessary. An exact
grant only removes the blanket prohibition on external destinations. Existing
`.agents/` destinations require no grant and retain all their existing checks.
Reject the workspace root and `.agents` itself as file destinations.

Always protect Git metadata (`.git` at any path segment), the selected source
tree, `.agents/open-forge.lifecycle.json`, `.agents/open-forge.libraries.json`,
`.agents/open-forge.permissions.json`, `.agents/open-forge.lock`, the selected
recovery storage and operation temporary paths, and overwrite companions.
Protect Framework-owned files/regions and registered Library destinations from
Extension effects through their existing observed ownership policies. These
checks also use portable identity. Never interpret an allowlist entry as an
override of a collision or a protected path.

External files remain opaque content. A Markdown file under `.apm/agents/`
does not join Open Forge routes or generated navigation. Other managers' known
control files remain protected where the existing ownership/boundary model
recognizes them. Do not claim universal detection of unknown manager formats or
invent a runtime registry. Ordinary unowned external files can be proposed;
existing occupants retain the command's explicit force and preservation rules.

For Install, require a grant per package for every external target in the
complete selected dependency closure. For Update, require grants for intended
external targets and previously owned external targets in the selected update
plan, including retirement and preserved paths. For Remove, require grants for
every selected owned external path, including paths kept as unmanaged. This
uniform rule makes revocation stop the complete selected lifecycle mutation,
including ownership release, until explicit reapproval. It does not delete
files or revoke ownership by itself. Unrelated installed packages are excluded.

## Permission Question

Finish complete source, ownership, destination and structural preflight first.
Then collect the unique missing subject/path pairs. Show source identity, each
package ID, exact destination and copy effect, and state that approval will be
remembered in this workspace. Ask once for the complete displayed set with
`[y/N]`. Accept `y` or `yes`, trimmed and case-insensitive. Any other answer,
empty input or EOF declines; do not loop. Cancellation retains the command's
`interrupted` semantics. Neither decline nor cancellation writes any file.

Use the existing typed interactive session and stderr prompt stream. The
permission question is allowed only for a human apply request with prompting
enabled and without `--automatic`. Dry-run and JSON never ask this question.
This restriction does not change existing command-selection or force questions.
Approval does not imply force, prune, or approval for new targets discovered
later. Declining and missing permission in unattended execution produce
`blocked`; dry-run with missing permission also reports `blocked` and the
complete missing set, without effect. Explicit user cancellation is interrupted.

## Application And Recovery

Approval is an immutable decision attached to the reviewed plan, not a write.
Acquire the normal workspace lease and reobserve sources, permission bytes or
absence, lifecycle, targets and parent identities. Any relevant change stops
before effects; do not merge new concurrent grants or transfer approval into a
recomputed wider plan. A wholly new invocation may ask again.

Prepare and verify one existing recovery bundle covering all planned effects,
including prior permission-file bytes or its proven absence. Only then apply
the atomic permission-file create/replace and verify intended bytes. The grant
effect precedes content, directory and lifecycle effects. If it fails, do not
start content effects. If a later effect fails or is interrupted, keep the
approved grant and report its actual outcome separately. Publish Extension
lifecycle last under the existing contract. Do not roll back permission.

Use the current reversible ordinary-file entry shape for permission replacement
and creation; creation preserves prior absence. No new recovery schema or entry
kind is required. Exact permission restoration is manual, using retained
evidence where necessary. Task 24 adds no automatic Doctor/Repair proposal.
Task 25 must prevent generic Library residual recovery from applying the
permission control-file entry automatically. Cleanup may remove positively
recognized bundles only under its existing explicit command contract.

## Public Results And Findings

Install, Update and Remove each add a command-local `permissions` object before
`lifecycle` in their existing result graph and JSON representation. The shared
process envelope, status precedence and exit mapping remain unchanged. Human
presentation uses this same semantic graph.

| Member     | Meaning                                                                                                |
| ---------- | ------------------------------------------------------------------------------------------------------ |
| `path`     | Always `.agents/open-forge.permissions.json`.                                                          |
| `required` | Ordered objects with `id` and exact `path` for the selected operation.                                 |
| `missing`  | Ordered subset absent from the observed document, retained after approval.                             |
| `decision` | `not-evaluated`, `not-required`, `granted`, `required`, `approved`, or `declined`.                     |
| `action`   | `none`, `create`, or `replace`; the planned control-file action.                                       |
| `outcome`  | `not-requested`, `planned`, `not-started`, `verified`, `verification-failed`, or `completion-unknown`. |

Order pair arrays by ordinal ID then path. `granted` means all required grants
already exist; `approved` means this invocation received explicit Yes. A result
formed before permission determination uses `not-evaluated`, action `none`,
and outcome `not-requested`; retain any known requirements but do not claim an
unknown missing set is complete. `not-required` is reserved for complete
determination that no external grant is needed. Content effects do not include a
duplicate permission effect. Recovery protected paths do include the control
file when it has a planned change.

Map existing ordinary-file receipts without guessing: a NotStarted effect maps
to `not-started`; Applied with Verified maps to `verified`; Applied with Failed
verification, including unavailable verification, maps to `verification-failed`;
an Unknown effect maps to `completion-unknown`. Before application a planned
change has `planned`; when no change is requested use `not-requested`. Only a
verified receipt supports saying the grant was saved. Later content failure
does not alter an already observed permission outcome. Retained bundle location
continues to use the existing recovery object.

Command finding prefixes remain `extension-install`, `extension-update` and
`extension-remove`. Add `permission-required` and `permission-declined`
(`blocked`), `permissions-invalid` (`blocked`), `permissions-unavailable`
(`incomplete`), `permissions-changed` (`blocked`), and `permission-write-failed`
(`failed`). Malformed storage directs to inspect and correct the consumer file;
missing grants direct to rerun interactively or add the displayed exact entries.
No hint implies `--force` bypasses permission or that Repair restores grants.
Replace the obsolete Install `target-outside-agents` finding with existing
`target-unsafe` for invalid/reserved paths and the new permission finding for
otherwise eligible unapproved external paths.

## Realization And Owned Seams

Use `Framework/Permissions/Models` for immutable document, subjects, observation,
requirements and proposed change. Keep strict reading, writing and evaluation
under `Framework/Permissions/Shared/<Capability>`. This capability does not own
the workspace lease, prompting, command policy, rollback or an independent
writer. Commands consume its exact proposed ordinary-file change in their
existing application plan. Share only demonstrated neutral grammar with Library
paths; do not depend on an Extension command from Framework permissions.

Install normalizes complete content before permission planning, then composes
the permission change into `ExtensionInstallApplicationOperation` under its
existing lease. Update includes permission observations in its plan comparison
and complete effect set. Remove derives required grants solely from trusted
ownership; it must remain source-independent. All three preserve existing
Framework and Library boundaries, compare approved facts during lease-bound
revalidation, and keep their own result formation and presentation.

Extend ancestor inspection from `.agents` to the bounded workspace for external
targets. Create only declared missing ordinary parents. Extend no routing or
generated-region ownership to external Markdown. Move any materially changed
data-only helper record into its nearest Models scope in the same phase.

## Decisive Evidence

Unit: strict document grammar and duplicate/alias cases; independent canonical
serialization oracle; exact identity matching and missing-set order; complete
enum mappings plus an undefined value; no grant transfer between subject kinds.

Integration: real content packages and scaffold/embedded asset identity; one
prompt and remembered reuse; decline/EOF/cancel without writes; dry-run, JSON
and automatic prompt absence; dependency-specific grants; malformed and linked
control file; protected destinations; collision and linked ancestry; stale
permission/source facts; prior-byte and prior-absence recovery; permission write
failure prevents content; later content failure retains truthful grant effect;
source-independent Remove and grant revocation. Cover only Open Forge behavior.

Keep exactly three simple public journeys for each existing Extension command.
Renew existing journeys for `content/` and exercise remembered approval through
focused Integration. M8 freezes all changed source and artifacts before full
managed, formatting/static and supported linux-x64 Native AOT gates.
