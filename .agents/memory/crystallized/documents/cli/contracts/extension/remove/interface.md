---
open-forge:
  description: Accepted Interface for releasing selected Extension ownership with safe final-owner removal and same-request prune
  responsibility: Define remove's exact syntax, managed-ID authority, Keep-as-unmanaged/Delete choice, effects, results, and errors
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Extension, Remove, Interface, Ownership, Prune, Safety, CurrentTruth]
---

# extension remove Interface Contract

## Status And Authority

This is the accepted current Crystallized Interface Contract for
`open-forge extension remove`. It owns exact syntax, managed-ID selection,
source-independent behavior, dependency and ownership boundaries,
Keep-as-unmanaged/Delete choice, same-request `--prune`, automatic and wizard
behavior, effects, output, statuses, errors, examples, non-goals, and public
conformance. The new CLI does not ship yet.

The sibling [Behavior Contract](behavior.md) defines technology-neutral
resolution and mutation. The [Extension group entrypoint](../_extension.md),
[Global CLI Flags](../../shared/global-flags/interface.md), and current Index
contracts own their shared boundaries. No Technical Design exists.

## Purpose And Boundary

`remove` releases explicit managed Extension package ownership and performs only
bounded cleanup proven safe from the trusted `extensions` section of
`.agents/open-forge.lifecycle.json`, schema v1. It is separate from Framework
lifecycle because its package identities and owned paths are isolated in that
section.

Remove does not require current package source bytes. It never removes the
package source. It never removes Framework, Core, Memory, unknown, unowned,
another-manager-owned, or route-unsafe content.

## Syntax

```text
open-forge extension remove [<stable-id>...] [--prune] [--automatic] [--dry-run] [--skip-git-check] [global flags]
```

Explicit managed stable-ID operands are required for direct, JSON, and other
non-interactive application. Argumentless human `remove` may open a finite
wizard to select managed IDs. There is no `--source`, `--all`, `--force`, package
path, semver selector, `--yes`, or Framework remove/uninstall command.

Shared flags are `--workspace <path>`, `--json`, `--view=compact|expanded`,
`--verbose`, `--help`, and `--version`; their full grammar and terminal behavior
remain in [Global CLI Flags](../../shared/global-flags/interface.md).

## Required Workspace Facts And Source Independence

The target is the exact current workspace or exact `--workspace` value. Remove
does not discover another root or package source. It reads trusted Extension
ownership, dependency, route, generated-navigation, and cross-section facts from
the workspace lifecycle state and current files.

Remove may proceed without a healthy current Framework only when those complete
trusted Extension facts can still be established, including route-host and
cross-section preservation facts. Otherwise it is `incomplete` or `blocked` and
performs no managed mutation.

Before a workspace effect, the implementation must hold the actual OS lock for
the visible `.agents/open-forge.lock` path defined by the accepted CLI
Architecture. File existence is not lock ownership. A crash releases the OS
lock; an unlocked file is reusable and may be manually removed only when no
process is active. The lock is concurrency safety, not lifecycle authority or
history, and another process holding it blocks mutation.

Missing package source bytes do not erase readable lifecycle ownership facts. A
missing or untrusted lifecycle document or section is not treated as an empty
installed set and cannot prove a prior remove. The lifecycle document stores no
plan, runtime history, journal, recovery evidence, or session.

## Selection And Flags

| Input              | Role                                                          | Rule                                                                                                     |
| ------------------ | ------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------- |
| `<stable-id>...`   | Select explicit managed package IDs                           | Repeatable subjects. Duplicate IDs are invalid.                                                          |
| `--prune`          | Same-request deletion authority for changed final-owner paths | Boolean and idempotent. It selects Delete before planning; it cannot be added after ownership release.   |
| `--automatic`      | Interaction policy                                            | Boolean and idempotent. It never selects IDs, Delete, or ownership authority.                            |
| `--dry-run`        | Preview policy                                                | Boolean and idempotent. It writes nothing and uses the same plan and preflight as apply.                 |
| `--skip-git-check` | Affected-path cleanliness exception                           | Boolean and idempotent. It bypasses only Git cleanliness and uses accepted backup recovery where needed. |

### Keep-as-unmanaged and Delete

For changed final-owner content, the operation has two bounded choices:

- `Keep-as-unmanaged` releases selected ownership and preserves the current
  changed file as visible unmanaged content.
- `Delete` deletes the changed final-owner file only when all identity,
  containment, route, Git, backup, and verification checks pass. `Delete` is
  selected by explicit `--prune` in the same request as remove.

The human wizard shows this choice before planning. Noninteractive normal mode
and automatic mode choose `Keep-as-unmanaged` unless the same request includes
explicit `--prune`. Automatic mode never chooses Delete by itself.

For an unchanged eligible final-owner path, ordinary remove authority may delete
the file after all complete safety and recovery checks. Shared files remain while
another owner remains. Ownership release is visible in the plan and result.

## Wizard, Direct, And Automatic Behavior

Argumentless human remove may ask for managed IDs, retained dependents, shared
owners, final-owner files, route-host effects, changed content, and the
Keep-as-unmanaged/Delete choice. Wizard answers and explicit IDs/`--prune`
populate one typed request. Recommendations do not create deletion authority.

JSON and other non-interactive modes never prompt. Missing IDs are `invalid`.
Without `--prune`, changed final-owner content uses Keep-as-unmanaged. With
`--prune`, the same request explicitly selects Delete for eligible changed
final-owner content. `--automatic` preserves that rule and adds no authority.

## Ownership, Dependencies, And Removal Effects

Removal reads trusted Extension lifecycle facts and fresh current semantic and
exact-byte facts. It validates:

- selected IDs and complete owner sets;
- retained dependents, dependency edges, and orphan outcomes;
- final-owner versus shared paths;
- current semantic fingerprint against persisted semantic baseline;
- route-host and generated-navigation reachability;
- Framework, user, unknown, and other-manager ownership; and
- exact cross-section, containment, Git, backup, verification, and recovery
  boundaries.

A retained dependent blocks removal of its dependency. A dependency that becomes
an orphan remains recorded and installed; there is no automatic orphan prune. A
route host cannot be removed while retained routed descendants depend on it.

Remove may:

1. release selected ownership for every eligible recorded package path;
2. delete an unchanged eligible final-owner file;
3. retain shared files while another owner remains;
4. preserve a changed final-owner file as unmanaged under Keep-as-unmanaged; and
5. delete a changed eligible final-owner file only under same-request `--prune`.

Unknown, unowned, shared-unsafe, Extension-external, Framework-owned,
route-unsafe, ambiguous, or untrusted content is never deleted. Once ownership
is released, a later prune cannot act on the resulting unmanaged file.

Generated `Entries` are derived navigation, not package-owned authored bytes.
Remove projects affected parents from intended authored topology and metadata
through current Index rules in the same plan. It preserves valid markers and
outside bytes and blocks malformed boundaries. The package source remains
unchanged.

## Lifecycle Trust And Semantic Identity

The lifecycle document has isolated `framework` and `extensions` sections.
Remove publishes the Extension ownership release only after complete verification
and preserves unrelated sections and envelope bytes and meaning. Unsupported or
ambiguous schema facts are `incomplete` or `blocked` under the existing safety
rules; they never grant force, prune, or remove authority.

For supported parseable kinds, removal and prune classification compare the
current `open-forge-markdown-v1` conservative parser/AST semantic fingerprint
with the persisted semantic baseline
fingerprint. Preserve Unicode, semantic text, headings, tags, links,
destinations, marker meaning, code blocks, and significant whitespace. Normalize
only line endings and parser-proven formatting trivia. Unsupported, binary, and
unparseable kinds use exact-byte identity and fail closed. Fresh exact bytes are
still needed for diff, revalidation, deletion, verification, and recovery. No
formatter executes or produces persisted formatter state.

## Output And Results

Human output leads with exact workspace, selected IDs, retained dependents,
shared/final owners, Keep-as-unmanaged or Delete choice, normal/automatic and
apply/dry-run mode, released ownership, deleted/retained/preserved paths,
generated projection, lifecycle publication, source unchanged, recovery facts,
status, and at most one next action. JSON emits one complete typed result from
the same result for every status.

| Result        | Meaning for `extension remove`                                                                                                                                                                                                               |
| ------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | Selected ownership release and all permitted effects or dry-run completed with complete coverage and no unresolved finite attention. A verified no-op requires trusted proof that the selected ID and selected ownership are already absent. |
| `attention`   | Complete safe removal released ownership but preserved changed final-owner content as unmanaged or retained another finite non-blocking fact.                                                                                                |
| `incomplete`  | Safe lifecycle, source-independent ownership, route, generated, parser, or recovery coverage is unavailable. No managed mutation occurs.                                                                                                     |
| `invalid`     | IDs, flags, repetition, missing semantic input, or terminal-mode input is invalid.                                                                                                                                                           |
| `blocked`     | Unsafe, ambiguous, untrusted, colliding, retained-dependent, route-unsafe, dirty, unauthorized, or recovery facts prevent one complete plan.                                                                                                 |
| `failed`      | Application, lifecycle publication, verification, or handled recovery fails unexpectedly.                                                                                                                                                    |
| `interrupted` | The caller interrupts before completion and no stronger recovery failure remains.                                                                                                                                                            |

Primary human complete/attention/incomplete results go to stdout. Primary human
invalid/blocked/failed/interrupted results go to stderr. Bounded diagnostics use
stderr. JSON emits one result on stdout for every status.

## Repeated Remove And Errors

A repeated remove is a verified no-op only when a valid trusted current lifecycle
document proves that the requested ID and all selected ownership are already
absent. Missing document or section, malformed, unsupported, or untrusted
lifecycle evidence is `incomplete` when coverage is safely unavailable or
`blocked` when ambiguity is unsafe. It is never presumed to prove that an earlier
remove succeeded.

Every error names `extension remove`, selected IDs/path/owner when known, the
cause, and at most one useful next action. A retained dependent blocks before
any ownership release. A changed final-owner path without `--prune` is retained,
not an error; an ineligible path under `--prune` is blocked, not deleted.

## Examples

Open the human selection and ownership-choice wizard:

```text
open-forge extension remove
```

Release one managed package and keep changed final-owner files unmanaged:

```text
open-forge extension remove development-toolkit --automatic
```

Select Delete for eligible changed final-owner paths in the same preview:

```text
open-forge extension remove development-toolkit --prune --dry-run --json
```

Remove does not require a package source:

```text
open-forge extension remove development-toolkit --workspace D:/work/example
```

## Non-Goals And Public Conformance

Remove does not fetch a source, remove a package source, install/update a
package, remove Framework/Core/Memory, delete unknown or unowned files, infer
ownership, prune orphans, choose Delete automatically, repair markers, run a
formatter, use a later invocation to add prune authority, or create a hidden
transaction journal.

Conformance must cover exact managed-ID and wizard/direct/automatic behavior,
source-unavailable facts, trusted/untrusted/absent states, retained dependents,
orphan retention, shared owners, semantic current-versus-baseline classification,
unchanged final-owner deletion, changed Keep/Delete and same-request prune,
ownership release, later-prune refusal, route/generated safety, package-source
preservation, complete plan, Git/backup/recovery, dry-run parity, no-op proof,
seven statuses/streams, JSON parity, and no Framework mutation. The shared CLI
Architecture defines the exact JSON result schema and exit mapping. Gate 5 must
prove source-generated serialization, fixed Markdig where used, real
`System.IO`, Native AOT, OS locking, isolated tests, and package journeys.
