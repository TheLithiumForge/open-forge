---
open-forge:
  description: Prepare consumer-approved Workspace Library projections beyond the .agents tree
  tags: [Memory, Working, CLI, Task, Workspace, Library, Destination, Symlink, Contextual, Queued]
---

# Task 25: Workspace Library Destination Projections

## Task State

- State: Queued after Task 24 “Extensions Evolution”. Functional draft
  preparation may overlap command work. Implementation requires completed
  Task 23, settled Task 24 decisions, and user review.
- Permanent mapping: Task 25 “Workspace Library Destination Projections” in the
  [project control ledger](../project-control.md).
- Sources: [Workspace Libraries idea](../../../emerging/ideas/workspace-libraries.md),
  [Extensions Evolution idea](../../../emerging/ideas/extensions-overhaul.md),
  and the accepted first-release design at the immutable contract tip recorded
  in [Task 23](workspace-libraries.md).
- Parent: [Complete The Replacement CLI](00-cli-development.md).
- Phase and milestone: not assigned.

## Current User Direction

On 2026-09-08 the user required a consumer allowlist and a CLI question when
an entry is missing, alongside the accepted Task 24 `content/` rename. The
[joint draft](extensions-destination-proposal.md) now includes remembered exact
file grants, prompt-free automation/dry-run, and explicit permission effects.
This supersedes its earlier manual-edit-only recommendation. Task 25 still
requires the accepted Task 23 baseline and frozen Task 24 shared boundaries.

## Pending Source Selection And Recovery Boundary

The consumer must be able to discover or nominate a never-approved external
file before the CLI can ask for permission. Inventorying only `.agents/` and
already-approved paths cannot do that. The root asked the user to choose
explicit external paths or all eligible files in the contained source root;
Task 24 proceeds independently while this Task 25 choice remains open.

Permission changes use the existing bundle as recoverable evidence but are not
automatic Library repairs. Exclude the permission control-file entry from
`LibraryResidualAttributionReader.IsAttributedEntry`; link repair must not
restore or revoke consumer grants. Add focused regression evidence at Task 25.

## Candidate Boundary

This task prepares a proposal for consumer-approved relative-symlink projections to
exact workspace-relative destinations outside `.agents/`. The first Task 23
release remains `.agents/**`-only and is not changed by this queue record.

Any later design must preserve contained sources, complete eligible inventory,
exact relative-link identity, collision refusal, source-preserving Sync and
Detach, recovery, and the separate Library record. Source content cannot widen
consumer permission. Library links never become Extension copies or Extension
lifecycle ownership.

Task 24 may prove a neutral destination-admission primitive covering canonical
relative paths, exact allowlist membership, containment, and no-follow identity.
Task 25 may reuse it only when the meaning is identical; permission and
ownership remain Library-local.

## Implementation Conditions

Activation requires completed Task 23 evidence, an explicit permission carrier
and allowlist grammar, protected-root and alias rules, proportionate real-filesystem
evidence, and maintainer acceptance. Read-only design may overlap Task 24 now,
but shared/public implementation and integration are serialized.

The final functional draft must reach the user for comments before any
implementation. Preparation changes no command, contract, record schema,
source, or accepted destination. Compatibility machinery, remote action, and
publication remain outside this task.

## Functional Draft

The [joint functional proposal](extensions-destination-proposal.md) presents
recommended package vocabulary, consumer permission, concrete copy and link
examples, lifecycle behavior, and the remaining user decision. It is contextual
and must reach the user before implementation.

## Ready Dependency Baseline

Task 24 is accepted and integrated at `2eedaf87`; all original CLI commands,
including the five Library commands, were already integrated before that
Extension enhancement. Shared permission observation, evaluation, publication
and result contracts now have full managed and linux-x64 Native AOT evidence.
The only current Task 25 product blocker is how external source files are
selected before offering missing grants. The pending explicit-path versus
whole-eligible-source choice remains unaccepted. No Library projection
implementation has begun.
