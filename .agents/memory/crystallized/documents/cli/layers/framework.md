---
open-forge:
  description: The Framework layer - reusable facts about the workspace and the explicit effect boundaries that change it
  responsibility: Define the workspace, filesystem identity, document parsing, source and route facts, and the lifecycle, mutation, and recovery boundaries
  tags: [Memory, Crystallized, Document, CurrentTruth, Evergreen, CLI, Architecture, Framework, Sources, Routing, Documents, Lifecycle]
---

# Framework Layer

Framework answers **"what is true about this workspace, and what may change
it?"** It holds reusable facts and explicit effect boundaries derived from the
accepted Framework contracts, and it depends on no command.

Framework has no dependency on Commands, Shell, or Presentation. It returns
typed workspace facts and effect receipts, not command reports, detail policy,
format settings, prompts, user-facing wording, or process streams. A command
that needs to publish one of those facts projects it into its own result model
before Presentation consumes it.

The layer has three bands, and they are not interchangeable:

- **Documents** turn bytes into a typed model. Markdown, frontmatter, YAML.
  Nothing below them; they are the base of the whole tree.
- **Sources and Routing** turn files into identity and a graph. Canonical source
  identity, recognized entrypoint form, layers and overwrites, the Loader root,
  the route chain, loading behaviour, and generated navigation.
- **State** is everything that can change or be changed: lifecycle records,
  mutation, recovery, permissions, Libraries, and Extensions.

**Routing here means the workspace's document route graph, not command
dispatch.** Selecting which command to run is Shell's job. This is the single
most important vocabulary distinction in the tree, because both senses of the
word are natural in a CLI and only one of them is a Framework capability.

The [land Architecture](../architecture.md) records how this layer sits against
the others, including the dependency cycles that currently exist between these
capabilities.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Framework Capability Model

The complete command set demonstrates several shared capabilities before their
first consumer is implemented. Architecture establishes neutral mechanical
foundations at their nearest shared scope. Other shared contracts may be
established early, while semantic behavior is implemented only when a consuming
Task proves identical meaning.

## Workspace

Workspace selection resolves explicit and inferred subjects, records the exact
selection method, normalizes identity once, and never invents a fallback fact.
Terminal modes bypass workspace selection. Workspace-free commands preserve null
workspace in concrete results.

## Filesystem And Resolved Path Identity

Filesystem code uses real `System.IO` and typed outcomes. Path strings,
normalized lexical paths, resolved physical paths, and observed link targets are
separate facts. `physical identity` names this resolved-path and observed-alias
fact under the stable-workspace boundary. It does not mean an inode, file ID, or
handle-bound object identity.

`PhysicalPathResolver` walks one existing directory component at a time from a
proven root. For each directory component needed to reach a final leaf it
inspects without enumerating descendants, classifies ordinary, missing,
inaccessible, dangling, or reparse/link state, resolves one permitted link
target, immediately proves containment, records the identity used for cycle and
observable-link alias detection, and continues only from the proven contained
result. The final logical leaf is handed to the no-follow observation below
before ordinary file identity resolution.

A path that leaves the root and later re-enters is blocked at the first external
transition. Final-target containment is insufficient. Paths are resolved before
access, expected state is revalidated immediately before effects, and ordinary
managed BCL file operations provide atomic replacement. These checks reject
static escapes and detected persistent changes; they do not claim adversarial
handle-bound identity across a transient namespace swap.

Every effect that addresses a logical file leaf first obtains a neutral typed
no-follow observation of that leaf before ordinary physical resolution. The same
observation is repeated during initial preflight, under-lease revalidation, and
immediately before the effect. A present link, reparse point, or special final
leaf blocks ordinary `Create`, `Replace`, `Delete`, and `ReplaceGeneratedRegion`.
The guard does not resolve or follow that final component. Stable contained
directory-link ancestry remains governed by this ordinary path contract; this
Architecture does not broaden rejection of that ancestry. Library source and
destination rules may require the stricter real-directory boundary defined by
the [Workspace Libraries Technical Design](../technical-designs/workspace-libraries.md).

The no-follow guard is neutral and does not consult Library records. Therefore a
Route Update, Index, Route Move, or Route Remove operation cannot write through
or delete a Library projection, even when the projection has no readable or
matching Library record.

If managed BCL evidence cannot satisfy an accepted required guarantee,
implementation stops at Architecture rather than adding P/Invoke or silently
weakening it. A theoretical guarantee outside the accepted threat model does not
justify exceptional machinery. Typed reads distinguish complete, missing,
invalid encoding or syntax, access denied, and I/O failure while retaining
bounded direct causes without leaking sensitive content.

## Documents

_What turns bytes into a typed model?_

Markdown capabilities use one fixed CommonMark pipeline only when a real
consumer requires body parsing. One neutral frontmatter parser owns delimiter
and body boundaries. One neutral YAML syntax parser owns the source-preserving
node shape, scalar spans, aliases, and unsupported-mapping facts. Semantic
metadata uses one CLI-root generated YAML context and small models. Command
interpretation, findings, and status remain local.

Documents depend on nothing else in Framework. Every other capability that needs
the content of a file reaches it through this one, and **no command parses a
document format itself**. A command that needs a structure the parser does not
expose asks for it here; it does not grow a second parser beside the first.

## Sources

_What is a source, and which layers make it up?_

Source references use one shared grammar and typed identity model. Commands
retain attempted identity separately from resolved identity.

The source catalogue exposes immutable facts only: canonical source identity,
recognized entrypoint form, overwrites, and the physical identity behind each
logical layer. A Framework scope is authored meaning, not a mechanically
identifiable path segment; a consumer reports it only from explicit contract
evidence. Commands translate shared facts into local meaning.

An entry file is not a routed source. `AGENTS.md` and the Loader carry no route
metadata, and a consumer must not require it of them.

## Routing

_How do sources form a graph, and what does that graph expose?_

**Routing here is the workspace's document route graph. It is not command
dispatch.**

The route graph exposes immutable facts only: Loader root, route chain, loading
behaviour, and safe topology. Routing depends on Sources and is depended on by
generated navigation; it holds no command policy and performs no effects.

Generated navigation is a projection of routed sources, never an independent
authority. One neutral formation combines retained observed catalogue evidence
with intended logical membership, topology, Loader, alias, and collision facts
without command policy or effects. Exact callables, formation rules, missing-
Loader behavior, collision handling, projection, and region mechanics live in
the [Generated Navigation Technical
Design](../technical-designs/generated-navigation.md).

## Workspace Libraries

Workspace Libraries project recursively discovered eligible files from one
workspace-contained real source root. No specially named child is required.
Each record keeps a source root, a destination root (`.` for the workspace
root), and source-relative leaf paths. Central typed mapping derives final
consumer destinations and exact raw relative file-link targets. Destination
parents are real ordinary directories; only individual leaves are symlinks.

All selected and registered source trees remain protected from actual mutation
targets. A destination root may be their ancestor, including the workspace root.
Source and destination ancestry remain no-follow ordinary-directory boundaries.
Incomplete inventory blocks Sync; Detach uses recorded mappings without source
availability. Library permissions cover exact external files or explicitly
approved destination folders and future descendants, shared by all owners.
Revocation still gates removal and recovery. Permission never grants ownership
or overrides protected paths, source trees, ancestry or collisions.

The consumer keeps its Loader and route chain. Only mapped `.agents` leaves
can affect existing generated navigation under the Index contract; external
Markdown remains opaque. Relative-link effects never follow or mutate source
bytes. Prepare one recovery bundle, verify permission publication before links,
and publish the Library record last. Ordinary file effects reject link leaves.

The [Workspace Libraries Technical Design](../technical-designs/workspace-libraries.md)
defines strict record shape, complete inventory, mapping, grant integration,
recovery and capability gates. No copy fallback, Git operation, native interop,
per-file remapping, glob or write-through mutation is introduced.

## Consumer Workspace Permissions

`Framework/Settings/` owns the sole authored-settings parser, safe observation,
shared allow-list evaluation, explicit grant editing and neutral permission
receipts. `Commands/Shared/Permissions/` owns the common prompt choices and wire
projection. Library scope proposals remain in the Library command capability.
Requirements are ordinary destination strings; there is no per-owner grant
record or compatibility reader. Commands retain their lease, recovery and
content application responsibilities.

The [Workspace Permissions contracts](../contracts/shared/workspace-permissions/_workspace-permissions.md)
and [Technical Design](../technical-designs/workspace-permissions.md) define the
settings, always/once/cancel, explicit flag, result and recovery behavior.

## Embedded Framework Distribution

`Framework/Distribution/` owns one neutral embedded Framework payload reader and
immutable asset and inventory facts. The Framework project embeds the complete
canonical `src/open-forge/` tree under one fixed resource prefix. Runtime reads
that payload through ordinary BCL resource APIs and never reads repository source
paths.

The payload has canonical asset identity, exact bytes, deterministic ordinal
inventory, per-asset hashes, and one aggregate inventory fingerprint. Root
Install and Update consume the complete inventory. Framework-aware Route Init
consumes route entrypoint assets and topology from the same canonical payload.
`Framework/Extensions/Embedded/` derives the first-party catalogue from package
files embedded by the Framework project. Package manifests define IDs and dependencies;
the reader validates dependency closure and hashes embedded payload bytes.
No separately maintained compressed source snapshot or hash inventory is used.

Exact resource, hashing, parity, and isolated-binary mechanics live in the
[Embedded Payload Technical Design](../technical-designs/embedded-payload.md).

## Lifecycle, Mutation, And Recovery

Read-only commands create no locks, lifecycle files, caches, indexes, recovery
bundles, or drafts.

Mutation commands follow this cross-cutting stage order:

```text
resolve and inspect, including no-follow final-leaf facts
  -> form a command-local immutable plan
  -> validate policy and collisions
  -> acquire the real workspace lock when applicable
  -> revalidate every planned fact under the lease
  -> prepare and verify one external recovery bundle for every reversible non-no-op effect
  -> apply bounded filesystem changes with an immediate no-follow check per effect
  -> verify resulting identity and bytes
  -> write accepted lifecycle or Library record state last
  -> remove the command-owned recovery bundle only after whole-command success
  -> form one concrete result
```

The lock provides exclusion only among cooperating Open Forge processes. It is
external to the workspace and distinct from lifecycle and recovery. Existence is
not ownership, activity, lifecycle authority, or recovery history.

Every complete plan has one immutable verified final external bundle before the
first effect whenever it contains a non-no-op effect that the operation must be
able to reverse. This includes relative file-link creates and deletes and the
prior-missing Library record Create; semantic or byte no-ops have none. A
multi-file operation is not presented as one filesystem transaction. Shared
support never automatically restores, rolls back, or compensates for target
effects and never classifies current target state from recovery provenance.
Handled failure, interruption, and post-verification cleanup retain truthful
residual state for explicit Repair.

Shared mutation support provides facts and mechanical capabilities. Commands
retain their plan, effect ordering, findings, lifecycle publication, recovery
mapping, and result. Cleanup retains its narrow monotonic command-contract
exception. Status and Doctor observe recovery facts without acquiring the lease
or inferring activity.

Directory creation remains a separate Create-only effect rather than a file-
change kind. A verified directory may remain as residual state and has no
recovery payload. The [Directory Creation Technical
Design](../technical-designs/directory-creation.md) defines its exact mechanics.

Workspace state follows [Workspace State Files](../../../decisions/framework/workspace-state-files.md):
authored settings and one generated ownership lock. Framework, Extension and
Library receipts share the lock; no stored target baselines or old record
readers remain. Distribution owns current payload alignment and comparison;
Filesystem owns contained target reads. The [Ownership And Source Alignment
Technical Design](../technical-designs/lifecycle-provenance.md) defines the
boundary between stored receipts and operation-time evidence.

The [Mutation And Recovery Technical
Design](../technical-designs/mutation-and-recovery.md) defines application-data
stores, persistent lock identity, ZIP and manifest realization, expected-state
checks, same-directory atomic file mechanics, receipts, bounded validation, and
guarded deletion, no-follow recovery comparison, and relative-file-link
application. The [Workspace Libraries Technical Design](../technical-designs/workspace-libraries.md)
defines Library registrations and inventory. The [Shared CLI Operation
Contract](../shared-operation-contract.md) and command contracts define observable
operation and cleanup policy.
