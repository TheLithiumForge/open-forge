---
open-forge:
  description: Implement shared workspace selection, typed reads, physical path identity, and component-wise containment
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Foundation, Filesystem, Safety, Containment, Complete]
---

# Implement The Filesystem Safety Foundation

## Task State

- State: Complete.
- Implementer: Mastermind.
- Responsible role: Mastermind.
- Parent: [CLI Foundation](../_foundation.md).
- Plan step: F4.

## Expected Outcome

Core provides small cohesive managed-BCL capabilities for workspace selection,
typed reads, physical path resolution, containment, link cycles, and direct causes.
No command-specific finding or status policy appears in the foundation.

## Required Capability Model

### Workspace

- `CliWorkspaceRequest` represents explicit, current-directory, or absent subject.
- `CliWorkspace` stores normalized lexical root, resolved physical root, and exact
  `CliWorkspaceSelectionMethod`.
- `CliWorkspaceSelectionResult` distinguishes selected, missing, invalid,
  inaccessible, and unsafe without guessed fallback facts.
- `CliWorkspaceSelector` performs normalization and root proof once. Terminal and
  workspace-free paths never invoke it.

### Typed Reads

- `FileReadResult<T>` distinguishes complete, missing, invalid encoding or syntax,
  access denied, I/O failure, and cancellation.
- It carries bounded direct cause and canonical logical path where applicable.
- UTF-8 readers use strict decoding and caller cancellation. They do not collapse
  access and I/O causes into one Boolean.

### Physical Resolution

Use separate cohesive types under `Framework/Filesystem/PhysicalPaths/`:

- `PhysicalPathResolver` coordinates one resolution request and contains no
  command policy.
- `PathComponentWalker` yields each existing lexical component from a proven
  physical root without descendant enumeration.
- `LinkTargetReader` classifies ordinary, symbolic/reparse, dangling,
  inaccessible, and unsupported components through managed BCL APIs.
- `PhysicalContainment` compares normalized physical paths with platform-correct
  root boundaries and never uses string prefix alone.
- `PhysicalIdentityTracker` records resolved identities used for cycle and alias
  detection within one operation.
- `PhysicalPathResolution` is a closed result whose state-specific factories
  enforce required and forbidden fields.

## Component Algorithm

For each component from root to candidate:

1. Confirm the current physical parent is already contained.
2. Read the next component's attributes and immediate link target without
   enumerating its descendants.
3. For an ordinary component, append it and prove the resulting physical path is
   contained before any later access.
4. For a link/reparse component, resolve exactly one target relative to its
   physical parent when needed.
5. Immediately prove the target is contained. Return external at this step even
   if another link would later re-enter.
6. Record physical identity and return cycle/alias when the operation-local policy
   detects repetition.
7. Continue from the proven contained target.

Missing, dangling, inaccessible, cycle, external, and unsupported states are
deterministic typed outcomes. Exceptions are translated only at the exact BCL
boundary that understands them.

## Evidence Matrix

Integration evidence uses owned real OS resources and covers ordinary paths,
workspace-root link, ancestor link, contained file and directory links, external
links, external-then-reentering chains, dangling links, self and multi-link cycles,
aliases, missing components, denied access where the target OS can prove it, and
unchanged snapshots. Target Windows must execute symbolic-link cases or report one
explicit environment incapability; release runners may not skip required safety.

Unit evidence covers state factories, root-boundary comparison, relative-target
calculation, direct causes, and cancellation classification without pretending to
prove filesystem effects.

Published Native AOT Integration evidence executes the same path resolver.

## Protected Boundaries

- No fake filesystem interface, command finding/status, directory inventory,
  Markdown parser, P/Invoke, native shim, shell command, final-target-only helper,
  or exception-message wire contract.

## Stop Conditions

Stop immediately if the managed BCL cannot establish the accepted containment
guarantee on a release platform. Return the exact missing fact and evidence to the
Architecture decision point. Do not inspect an external ordinary path hoping that
a later component returns inside.

## Completion

Complete only when the component proof and leave-and-reenter regression pass in
managed and Native AOT execution and each class remains one cohesive capability.
