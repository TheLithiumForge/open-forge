---
open-forge:
  description: Add one lease-bound ordinary-BCL directory-create mutation effect
  tags: [Memory, Working, CLI, Task, Foundation, Mutation, Directory, Contextual]
---

# Add The Shared Directory-Create Effect

## Task State

- State: Ready after contract-freeze integration.
- Parent: [Next-Wave Shared Foundations](_shared-foundations.md).
- Consumers: root Install and Route Init.

## Outcome

Mutation support creates one explicitly planned missing descendant below
`.agents` while holding the workspace lease, immediately revalidates its missing
state and exact physical parent, applies ordinary `Directory.CreateDirectory`,
and verifies the resulting contained ordinary directory. Directory effects
remain separate from file effects. The existing lock manager owns the single
missing-`.agents` bootstrap before lease acquisition.

## Architecture And Ownership

- Production: the smallest focused `Framework/Mutation/**` directory capability
  and nearest models; do not add a directory kind to `PlannedFileChangeKind`.
- Plans name every missing directory and order parents before children. Commands
  retain selection, ordering among other effects, findings, and result policy.
- When `.agents` itself is missing, the command plan reports it as the one lock-
  bootstrap support effect. `WorkspaceLockManager` creates and verifies it
  immediately before opening `.agents/open-forge.lock`; the shared directory
  applier receives only descendants after lease acquisition.
- `WorkspaceLockResult.BootstrapOutcome` is nullable for every result state.
  Preserve `Existing` after validating a pre-existing `.agents` and
  `Materialized` after observed absence, attempted BCL creation, and validation;
  preserve either across later failure or cancellation. `null` means no
  successful directory observation, while acquired requires a non-null outcome.
- A verified created directory remains as reported residual state after a later
  failure or interruption. There is no deletion, rollback, compensation, or
  recovery-bundle entry for directory creation.
- Install and Route Init reuse the identical native capability only where these
  mechanics have the same meaning.

## Evidence

Cover missing creation, exact physical-parent revalidation, target/parent race,
unsafe alias, non-directory parent or target, lease requirement, cancellation,
ordinary access and I/O failures, post-verification, parent-before-child chains,
retained residuals, and proof that file changes and recovery payloads remain
unchanged. Cover missing `.agents` bootstrap creation/verification/reporting,
cancellation before bootstrap, lock contention after bootstrap, and its retained
residual without passing `.agents` through the lease-bound applier. Cover the
nullable `WorkspaceLockResult` outcome matrix for acquired, failed, and cancelled
results, including outcome retention. Run directly affected managed and
published `linux-x64` Native AOT evidence.

## Stop Conditions

Stop before broadening file effects, deleting a directory, rollback,
compensation, recovery data, P/Invoke, a native package, or a creator-identity
guarantee against a hostile same-user process. Stop if Install and Route Init
require different descendant mechanics; keep divergent policy command-local.
Do not move or duplicate the workspace lock or move its support directory into
`LocalApplicationData`.
