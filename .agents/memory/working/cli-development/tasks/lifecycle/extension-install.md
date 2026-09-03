---
open-forge:
  description: Implement Extension installation from exact reviewed package identity
  tags: [Memory, Working, CLI, Task, Extension, Install, Lifecycle, Contextual]
---

# Task 14: Extension Install

## Task State

- State: Prepared with findings and queued after Task 12 “CLI Architecture
  Authority Remediation”. No implementation is active.
- Permanent mapping: Task 14 “Extension Install” in the
  [project control ledger](../../project-control.md).
- Prerequisites: Extension Inspect, root Install, and Mutation Foundation are
  complete. Route Move and Task 12 integration provide the accepted activation
  base; Route Remove and root Update are not prerequisites.
- Planned progress horizon: phase 0 of 5, milestone 0 of 8. The streamlined
  phases are Preflight, explicit Gray/Red, one coherent implementation and
  focused-verification pass, one fresh whole-task review with at most one
  grouped improvement pass, and acceptance.
- Parent: [Lifecycle Commands](_lifecycle.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/extension/install/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/extension/install/behavior.md).

## Read-Only Preflight

The Sol/xhigh Task Mastermind inspected clean local `develop` at commit
`d13336d2d4124469bc57ee210668f0719f5434ab`, exact tree
`09e5cfd33a5343bf0c2a7a1faf787066d05e0e0e`, without edits, builds, tests,
activation, or worktree creation. The current lifecycle schema, collision and
ownership rules, external lock and recovery model, generated-navigation
formation, seven statuses, and direct typed C# composition are already settled.
Task 14 does not reopen them.

Four maintainer-owned freezes remain before Gray:

1. Own the exact result and presentation locally. Recommended ordered JSON
   facts are `mode`, `force`, `automatic`, `selection`, `source`, `packages`,
   `framework`, `footprint`, `effects`, `generatedNavigation`, `lifecycle`,
   `recovery`, `verification`, and `findings`. Arrays remain present and
   non-null; unavailable early atomic facts may be nullable. Human sections,
   finite findings, `next`, help, diagnostics, and exit mapping remain exactly
   aligned with that command-local result.
2. Make omitted source deterministically embedded without prompting. Prompt
   only for unresolved multi-package selection or an eligible initial-force
   choice. Selection accepts exact package IDs or exact `all`; invalid answers
   retry locally; EOF is no-write `invalid`; cancellation is no-write
   `interrupted`; dependency closure is displayed but not optional. There is no
   generic apply confirmation, and `--automatic` grants neither selection nor
   force.
3. Apply and verify dependency-first target and generated effects, verify the
   intended target topology, publish and verify Extension lifecycle as the last
   workspace file effect, then reread targets, Extension lifecycle, and
   unchanged Framework meaning before success and recovery cleanup.
4. Restrict managed package payload targets to descendants of `.agents/` for
   this Task. Reject other targets before planning instead of expanding the
   accepted directory-creation capability to arbitrary workspace parents.

The proposed Gray surface remains command-local under
`Commands/Extension/Install/**` with direct construction and small typed stage
records. A neutral Extension lifecycle-currentness reader may be added for
truthful verification and later Status/Doctor consumption, but Task 14 must not
implement Status rows, Doctor findings, a contributor registry, or a generic
engine.

## Expected Outcome

`extension install` applies one or more explicitly selected, reviewed Extension
packages to a workspace, records isolated Extension lifecycle identity, preserves
Framework lifecycle, and produces one verified external recovery bundle covering
every existing target it replaces or deletes.

## Architecture

- Reuse shared Extension catalogue/manifest/payload facts and root lifecycle
  primitives.
- Keep selection modes, dependency order, source-review policy, package conflicts,
  `ExtensionInstallPlan`, findings, and result local.
- Plan every package and collision before the lock. Revalidate source and workspace
  expectations under the lock before effects.
- Lifecycle `extensions` entries remain isolated by exact package identity.

## Evidence

Cover exact and automatic selection, none/one/many packages, dependencies,
collisions across packages and Framework, source review, malformed payload,
unsupported compatibility, dry run, lock/source race, bundle preparation and
retention after partial failure/cancellation at each package, lifecycle
isolation, generated navigation, idempotence, process, packed packages, and AOT.

## Stop Conditions

Stop before package download outside accepted sources, runtime code execution,
silent conflict resolution, Framework lifecycle mutation, or applying an
unreviewed/changed package.

## Activation Boundary

Activate only after Route Move is integrated, Task 12 is complete and
integrated, the four freezes above are accepted, and a fresh clean implementation
base and directive fingerprints are recorded. Gray and Red remain explicit;
one Brilliant Implementer owns production and focused verification; one fresh
whole-task review and at most one grouped improvement pass close the Task.
