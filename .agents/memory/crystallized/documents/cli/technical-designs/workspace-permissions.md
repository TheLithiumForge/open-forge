---
open-forge:
  description: Define the strict permission codec and lease-bound ordinary-file publication design
  tags: [Memory, Crystallized, Document, CurrentTruth, Evergreen, CLI, TechnicalDesign, Permission, Filesystem]
---

# Workspace Permissions Technical Design

This realizes the [Workspace Permissions contracts](../contracts/shared/workspace-permissions/_workspace-permissions.md)
under the [CLI Architecture](../architecture.md). Permission is a consumer-owned
admission fact, distinct from Extension ownership and Library link ownership.

## Capability And Consumers

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

## Receipt And Recovery Integration

Use the existing `PlannedFileChange`, `FileChangeReceipt`, workspace lease and
recovery preparation. The permission capability returns a proposed change;
only the command application owner writes it. Include it in complete expected
state and bundle preparation before other effects. Existing receipt states
map directly to the contract's permission outcome, including unknown completion.

Create uses reversible prior-absence recovery and Replace preserves exact prior
bytes. There is no new recovery schema, repair proposal, independent write
transaction, DI container, runtime registry or compatibility reader. A later
content failure retains the observed permission outcome and existing bundle.
The standard BCL and current strict JSON tools are sufficient for this local
cooperating-developer boundary.

The canonical writer preserves unrelated valid entries semantically. Compare
exact observed permission bytes or absence under the lease before writing;
never merge a concurrent document change into an already approved plan.

## Implemented Foundation

The codec uses strict UTF-8 validation, JsonDocument for exact schema admission
and Utf8JsonWriter for canonical bytes. It needs no reflective serializer or
runtime dispatch. Consumer observation rejects linked parents/leaves and retains
an ordinary-file snapshot before decoding. Change planning requires explicit
approval and a safe complete or missing observation.

The portable grammar is owned by Framework Filesystem Shared Paths and consumed
by Extensions, lifecycle validation and permissions. Permission membership uses
that parser's exact portable key rather than a different Unicode case comparer.
The shared grant helper keeps Library IDs bound to their observed source roots;
Task 25 owns any explicit rebinding flow before Library mutation consumes it.
