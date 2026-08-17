---
open-forge:
  description: Contextual history of the maintainer-accepted Queue 31 revision for route move and remove
  responsibility: Preserve the review risks and dissent while pointing current meaning to the accepted route contracts
  tags: [Memory, Archived, CLI, Release, Review, History, Settled, Contextual, Route, Mutation, Safety, Historical]
---

# Route Move and Remove Review History

**Packet:** Queue 31, Packet 4 of 6; `settled`

**Classification:** `#Contextual` review history. This file is not an Interface
Contract, Behavior Contract, Decision, Architecture, or implementation record.
The accepted meaning is in the current [route move Interface](../../../crystallized/documents/cli/contracts/route/move/interface.md),
[route move Behavior](../../../crystallized/documents/cli/contracts/route/move/behavior.md), [route remove
Interface](../../../crystallized/documents/cli/contracts/route/remove/interface.md), and [route remove
Behavior](../../../crystallized/documents/cli/contracts/route/remove/behavior.md) contracts.

## Status, authority, and purpose

The maintainer accepted the Queue 31 revision and its integration into the
current CLI contracts. The [Decision Agenda](../../../working/cli-release/decision-agenda.md), the
operation-local contracts, and the linked shared Framework and CLI contracts are
the current authorities. This record preserves why the review mattered, the
risks considered, and useful dissent without creating a second authority.

Gate 2 remains open and non-shipping. Queue 31's contracts are retained
implementation targets, but no implementation, Architecture selection, library
choice, package work, or release claim follows from this history. Queue 32
cleanup is now the current review; Queue 33 remains waiting behind it.

## Accepted maintainer revision

The accepted revision keeps the shallow grouped command surface:

```text
open-forge route move <source-reference> <destination-target> [--dry-run] [--skip-git-check] [global flags]
open-forge route remove <source-reference> [--dry-run] [--skip-git-check] [global flags]
```

Each command accepts exactly one eligible ordinary unmanaged logical leaf or one
eligible ordinary unmanaged category. A category is selected through one
recognized entrypoint source reference and includes its complete physically
contained folder tree: the root entrypoint, overwrite companion when present,
descendant entrypoints and leaves, routed or unrouted Markdown, native or binary
resources, ordinary support files, and every other regular contained file and
directory. Every item must pass containment,
identity, ownership, lifecycle, collision, and recovery classification. One
incomplete or unsafe item blocks the category operation as a whole.

The accepted contracts also establish these boundaries:

- Category processing may enumerate many logical sources and resources, but it
  forms one complete plan and one recovery/verification boundary. It is never a
  series of independently committed leaf commands and never a generic batch or
  apply surface.
- Positive unmanaged proof requires one complete trusted lifecycle-ownership
  inventory from the Framework baseline and every applicable Extension receipt
  or manager claim. The inventory must claim none of the selected logical
  sources or resources. Missing, malformed, conflicting, stale, or incomplete
  inventory blocks. A missing receipt, path, tag, generated entry, matching
  bytes, or familiar route does not prove unmanaged status.
- A leaf destination is one exact ordinary routed file target under an existing
  valid route. A category destination is one exact destination entrypoint path
  under an existing valid parent route; it establishes the category root and
  preserves descendant relative layout. Self-moves, destinations inside the
  source, aliases, collisions, overwrite conflicts, unsafe containment, and
  implicit parent initialization are rejected.
- Move performs one complete physically contained supported-workspace-Markdown
  reference pass inside and outside `.agents`. It rewrites every exact
  supported resolvable local authored reference whose existing destination would
  no longer resolve to the same intended target after the move, including
  outside-to-moved, moved-to-outside, and needed internal references. It
  preserves labels, fragments, valid encoding, and unrelated bytes. Internal
  links that remain valid need no rewrite, and external URLs are unchanged.
- Remove performs the same complete pass. Each incoming exact supported
  Markdown link from outside the removed subject is detached to its visible
  label as plain authored text, with surrounding prose preserved. Every
  detachment appears in dry-run and final human and JSON results. References
  originating inside the removed subject disappear with it. Unsupported or
  ambiguous forms, prose-losing transformations, or incomplete coverage block
  with no writes; a supported broken link is never silently left behind.
- Both operations project the affected parent navigation (old and new for move,
  old for remove) and the Loader where applicable. They include affected-path Git
  rules, expected-state revalidation,
  backup readiness where Git cannot recover, all-effects verification, and
  identity-guarded reverse recovery. They create no hidden index subprocess,
  receipt, tombstone, journal, saved plan, or history.
- The command path and exact source and destination subjects supply consent in
  human, JSON, and non-interactive use. `--dry-run` is the only preview. There
  is no `--force`, `--automatic`, `--yes`, `--apply`, root move/remove, or batch
  operand. `--skip-git-check` bypasses only affected-path cleanliness.
- A repeated move using its consumed old source is a non-mutating exact
  `source-not-found`/`invalid` result. A repeated remove is a verified no-op only
  when complete trusted ownership, topology, recovery, and reference evidence
  independently proves exact intended absence and no orphan companion,
  residual reference, stale generated region, or recovery artifact remains.

The current [route move Interface](../../../crystallized/documents/cli/contracts/route/move/interface.md) and
[route remove Interface](../../../crystallized/documents/cli/contracts/route/remove/interface.md) own the public
syntax, finite results, output, examples, and errors. Their sibling Behavior
contracts own deterministic planning, effects, verification, recovery, and
conformance. The [Index Behavior Contract](../../../crystallized/documents/cli/contracts/index/behavior.md)
supplies generated projection only; it does not perform route mutation.

## Review history and preserved risks

The first candidate considered a narrower ordinary-leaf boundary and a stop when
an incoming reference from outside `.agents` was found. The maintainer revised
both points. Categories are now accepted as complete physical subjects, and the
current contracts define a complete workspace-Markdown pass with move rewrites
and remove detachments. The earlier narrower boundary and stop are superseded
history, not current constraints.

The review retained these risks for later evidence and contract conformance:

- A category can have a large physical blast radius. Complete inventory,
  positive lifecycle proof, exact destination collision checks, and one plan are
  therefore required rather than treating the category as a convenient list of
  leaf operations.
- A complete reference catalogue is a safety boundary, not an optimization.
  Unsupported or ambiguous forms must remain visible as a no-write boundary,
  and remove must not sacrifice surrounding prose while detaching a link.
- Lifecycle ownership is not inferable from route placement, matching bytes, or
  receipt absence. The Framework baseline and all applicable Extension claims
  must be trusted before any selected item is treated as unmanaged.
- Generated navigation changes with topology but is not authored reference
  authority. The route operations own the complete mutation; Index supplies the
  projection and bounded generated-region rules.
- Consumed-source repeats cannot prove provenance without hidden state. Remove's
  verified absence is intentionally stricter than a missing-path observation.
- Reverse recovery must preserve an unexpected concurrent edit. Backups,
  expected-state revalidation, and identity-guarded recovery remain observable
  safety obligations while their mechanics stay deferred to Architecture.

## Disposition and program consequence

Queue 31 is settled because the maintainer accepted the revised leaf/category
boundary and the complete reference, ownership, plan, consent, repeat, and
recovery rules. The current contract pack was authored in the same integration
change. Queue 32 may now be reviewed; this record does not accept Queue 32's
cleanup candidate or advance Queue 33.

The accepted result adds two retained command contracts to the full delivery
target. It does not close Gate 2, open Gate 3, choose implementation technology,
or create a partial publication target.

## Sources consulted

- [Decision Agenda](../../../working/cli-release/decision-agenda.md), especially D015, D021, D025, D026,
  D041–D046, D048, and the repeated-write rule.
- [route move Interface](../../../crystallized/documents/cli/contracts/route/move/interface.md) and [Behavior](../../../crystallized/documents/cli/contracts/route/move/behavior.md).
- [route remove Interface](../../../crystallized/documents/cli/contracts/route/remove/interface.md) and [Behavior](../../../crystallized/documents/cli/contracts/route/remove/behavior.md).
- [Route group](../../../crystallized/documents/cli/contracts/route/_route.md), existing route-write contracts,
  and the [CLI contract overview](../../../crystallized/documents/cli/command-contract-set.md).
- [CLI Source References](../../../crystallized/documents/cli/contracts/shared/source-references/interface.md)
  and [Behavior](../../../crystallized/documents/cli/contracts/shared/source-references/behavior.md).
- [Index Interface](../../../crystallized/documents/cli/contracts/index/interface.md) and [Behavior](../../../crystallized/documents/cli/contracts/index/behavior.md).
- [References Interface](../../../crystallized/documents/cli/contracts/references/interface.md) and [Behavior](../../../crystallized/documents/cli/contracts/references/behavior.md).
- [Doctor Interface](../../../crystallized/documents/cli/contracts/doctor/interface.md) and [Behavior](../../../crystallized/documents/cli/contracts/doctor/behavior.md).
- [Shared CLI Operation Contract](../../../crystallized/documents/cli/shared-operation-contract.md).
- [Routing Model](../../../crystallized/documents/framework/routing/model.md),
  [Routing Paths And Identity](../../../crystallized/documents/framework/routing/paths.md),
  [Overwrite Customization](../../../crystallized/documents/framework/routing/overwrites.md),
  and [Routed Markdown Representation](../../../crystallized/documents/framework/markdown/routes.md).
- [Queue 30 historical analysis](gate-2-closeout.md), retained as contextual
  evidence only.
