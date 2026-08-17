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

## Shared Lifecycle Boundary

Extension install and update use one exact source universe, complete dependency
closure, route projection, ownership plan, and the only new-CLI lifecycle
document, `.agents/open-forge.lifecycle.json`, schema v1. Remove uses trusted
workspace ownership and route facts and does not require package source bytes.
List and inspect are read-only. Create writes only a catalogue scaffold and
treats `--path` as a catalogue destination, not a source or workspace.

The lifecycle document has a common envelope and isolated `framework` and
`extensions` sections. An Extension operation changes only `extensions` and
preserves the unrelated section and envelope bytes and meaning. It stores no
plan, runtime history, journal, recovery evidence, or session. Files outside
this exact document are ordinary workspace content, not lifecycle input.

The CLI distribution embeds Framework and first-party Extension assets with
deterministic inventory and hash proof. That proof identifies distributed source
assets; it is not evidence of a selected workspace's current installation or of
a proven runtime implementation.

Installed files retain the meaning of their destination routes. Package metadata
and lifecycle ownership are evidence and management facts; they do not create
Framework runtime meaning or authority.

## Child Contract Sets

- [`list/`](list/_list.md) reports separate Installed and Available sections.
- [`inspect/`](inspect/_inspect.md) reports package-specific installed,
  available, and three-way facts.
- [`create/`](create/_create.md) creates a local catalogue scaffold.
- [`install/`](install/_install.md) establishes managed ownership for selected
  absent packages or verifies an exact managed no-op.
- [`update/`](update/_update.md) reconciles trusted managed package identities.
- [`remove/`](remove/_remove.md) releases selected ownership and performs only
  bounded safe cleanup.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

<!-- open-forge:generated-index:start -->
- [Route the accepted local Extension catalogue-scaffold creation contracts](create/_create.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Extension #Create #Catalogue #Mutation #CurrentTruth
- [Route the accepted read-only Extension inspect contracts for one stable package identity](inspect/_inspect.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Extension #Inspect #Interface #Behavior #ReadOnly #CurrentTruth
- [Route the accepted Extension management-establishment and exact-no-op install contracts](install/_install.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Extension #Install #Lifecycle #Ownership #CurrentTruth
- [Route the accepted read-only Extension list contracts for Installed and Available package facts](list/_list.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Extension #List #Interface #Behavior #ReadOnly #CurrentTruth
- [Route the accepted Extension ownership-release and bounded removal contracts](remove/_remove.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Extension #Remove #Ownership #Prune #CurrentTruth
- [Route the accepted Extension trusted-managed update contracts for force replacement, prune, and automatic safe reconciliation](update/_update.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Extension #Update #Lifecycle #Ownership #CurrentTruth
<!-- open-forge:generated-index:end -->
