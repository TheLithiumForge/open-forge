---
open-forge:
  description: Route the accepted non-shipping Extension list, inspect, create, install, update, and remove contracts
  responsibility: Provide Extension group help and route child contract sets without defining lifecycle behavior
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Extension, Lifecycle, CurrentTruth]
---

# extension Command Group

## Status And Authority

This is the accepted current Crystallized routing-only entrypoint for the
`open-forge extension` command family. The new CLI does not ship yet. The group
has several actual operations and therefore is a real operation family, not a
ceremonial namespace.

The group performs no package, catalogue, wizard, lifecycle, or mutation work.
Its bare form shows help for `list`, `inspect`, `create`, `install`, `update`,
and `remove`. The child contract sets define their own complete Interface and
Behavior authority. No Extension Technical Design files exist. The accepted CLI
Architecture defines the shared implementation boundary. Gate 5 must prove
source-generated YamlDotNet and STJ serialization, fixed Markdig where used,
real `System.IO`, Native AOT, OS locking, isolated tests, and package journeys.
These contracts do not claim that implementation or proof.

All Extension child commands use the shared native report: `--format
<text|json>`, `--detail <minimal|standard|full|debug>`, and repeatable
`--detail-filter <error|warning|info|all>`. Their result names are `completed`,
`completed-with-warnings`, `incomplete`, `invalid-input`, `blocked`, `failed`,
and `cancelled`; JSON is the schema-3 envelope. All child contracts use this
report and do not define alternate legacy presentations or envelope forms.

## Shared Lifecycle Boundary

Extension install and update use one selected source universe, dependency
closure, route projection and ownership plan. Framework, Extension and Library
receipts share `.agents/open-forge.lock.json`. Remove uses current ownership and
route facts without requiring package source bytes. Missing or unusable
ownership yields information and no inferred deletion. List and inspect are
read-only. Create writes only a catalogue scaffold.

A mutation preserves unrelated owners and publishes ownership after its effects
verify. Equal intended ownership causes no rewrite; a changed lock is written
wholesale with deterministic UTF-8. The existing public state-file outcome names
this single lock. Old records are unrelated content and are never migrated or
consulted. Current/intended comparison is computed within the invocation, with
no stored baseline. Shared allow-list admission, physical containment, reserved
paths and cross-owner checks apply before effects.

Prior bytes are captured in the verified external recovery bundle before
reversible effects. Update retains that bundle and supplies review advice under
its command contract. Standalone Extension Create remains a create-only
operation without a workspace lease or recovery bundle. Authored settings stay
separate from machine-owned receipts.

The CLI distribution embeds Framework and first-party Extension assets with
deterministic inventory and hash proof. That proof identifies distributed source
assets; it is not evidence of a selected workspace's current installation or of
a proven runtime implementation.

Installed files retain the meaning of their destination routes. Package metadata
and lifecycle ownership are evidence and management facts; they do not create
Framework runtime meaning or authority.

## Package Layout

The replacement CLI reads, creates, inventories and embeds `content/` only.
Each file below it retains its workspace-relative destination. First-party folders, embedded asset keys, scaffolds, help and examples use
this same layout. The user explicitly excludes legacy handling: update our own packages and
use `content/` throughout the replacement CLI. Do not add old-folder detection,
diagnostics, aliases or migration. A package without `content/` is reported by
Extension Install as `completed-with-warnings`: `Nothing was installed from
<extension-source>: the package has no content directory.` The report also says
`Package files belong under <extension-source>/content/.agents/.` and does not
write package files.

The rename does not rename JSON members such as `payload` or `payloadTargets`,
internal payload concepts, or stable effect tags. Those describe contributed
data, rather than a directory. Path values in them use `content/` when they
identify source files. Manifest and lifecycle schemas stay unchanged. Installed
ownership identifies destinations and does not depend on the package folder.
The retired MVP implementation remains in Git history and is not
a second supported source format for the replacement CLI.

## Consumer Permissions

Extension Install, Update and Remove consume the
[Workspace Permissions Interface](../shared/workspace-permissions/interface.md)
and [Behavior](../shared/workspace-permissions/behavior.md). Each command owns
its required grants and preserves its existing ownership and force boundaries.

## Child Contract Sets

- [`list/`](list/_list.md) reports separate Installed and Available sections.
- [`inspect/`](inspect/_inspect.md) reports package-specific installed,
  available, and package facts.
- [`create/`](create/_create.md) creates a local catalogue scaffold.
- [`install/`](install/_install.md) establishes managed ownership for selected
  absent packages or verifies an exact managed no-op.
- [`update/`](update/_update.md) reconciles trusted managed package identities.
- [`remove/`](remove/_remove.md) releases selected ownership and performs only
  bounded safe cleanup.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [Route the accepted local Extension catalogue-scaffold creation contracts](create/_create.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Extension #Create #Catalogue #Mutation #CurrentTruth
- [Route the accepted read-only Extension inspect contracts for one stable package identity](inspect/_inspect.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Extension #Inspect #Interface #Behavior #ReadOnly #CurrentTruth
- [Route the accepted Extension management-establishment and exact-no-op install contracts](install/_install.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Extension #Install #Lifecycle #Ownership #CurrentTruth
- [Route the accepted read-only Extension list contracts for Installed and Available package facts](list/_list.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Extension #List #Interface #Behavior #ReadOnly #CurrentTruth
- [Route the accepted Extension ownership-release and bounded removal contracts](remove/_remove.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Extension #Remove #Ownership #CurrentTruth
- [Route the accepted Extension trusted-managed update contracts for force replacement, prune, and automatic safe reconciliation](update/_update.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Extension #Update #Lifecycle #Ownership #CurrentTruth
