---
open-forge:
  description: Implement root and Extension creation, installation, update, and removal lifecycle commands
  tags: [Memory, Working, CLI, Task, Lifecycle, Extension, Install, Update, Contextual]
---

# Lifecycle Commands

## Task State

- State: Planned. No command behavior is Active or Ready.
- Parent: [Complete The Replacement CLI](../00-cli-development.md).
- Common prerequisites: Extension discovery and `index`; workspace mutations also
  consume Mutation Foundation.
- Intended-membership formation prerequisite: accepted feature
  `f82c2b168657baf2fac50c76e2ff2cc0ed3776d7`, squash-integrated at
  `cc35c853fd7b55d31e3fb2c9a454d9ab61c1884e`, exact tree
  `867be79ebee4ba60f2116a83857edadb8a5dbf0a`. It applies only to root Install
  and root Update in this group. Extension Create is independent of it.

## Shared Boundary

Lifecycle commands consume accepted package identities, catalogue facts,
workspace state, lifecycle schema, locks, external recovery-bundle support,
source review, and generated-navigation primitives. Each command retains its
selection, plan, effects, findings, and result.

No command recognizes legacy lifecycle files. Package wrappers do not implement
lifecycle behavior. `extension create` has no workspace subject and uses a
separate exact-destination, collision, and revalidation path with no workspace
lease, no Replace/Delete, and no recovery bundle.

Root Install and root Update later consume
`Build(SourceCatalogue observedCatalogue, IReadOnlyList<SourceLogicalSource> intendedSources)`
to project their complete post-operation Generated Navigation graph from real
observed evidence. This shared formation adds no prospective catalogue/source
framework, virtual filesystem, temporary checkout, or hidden Index. Extension
Create does not consume it. That Planned Task must not begin until the maintainer
accepts deterministic manifest defaults plus the host-owned boundaries for
interactive input and asynchronous request resolution. Do not infer those
product decisions from the completed shared prerequisite.

Every other lifecycle mutation that replaces or deletes an existing ordinary
target prepares one immutable, strictly verified external recovery bundle for
the complete operation before its first target effect. Creates and no-ops create
none; failures retain the bundle and report target state without restoration,
rollback, or compensation. The persistent workspace lock preserves its bytes
and is owned only through a `FileShare.None` handle.

## Child Tasks

- [ ] [Implement creation of one reviewable Extension package outside a workspace](extension-create.md) — Planned — Implementer: Not assigned
- [ ] [Implement root Framework installation into a selected workspace](install.md) — Planned — Implementer: Not assigned
- [ ] [Implement root Framework update from accepted lifecycle identity](update.md) — Planned — Implementer: Not assigned
- [ ] [Implement Extension installation from exact reviewed package identity](extension-install.md) — Planned — Implementer: Not assigned
- [ ] [Implement Extension update with source review, ownership, and recovery integrity](extension-update.md) — Planned — Implementer: Not assigned
- [ ] [Implement Extension removal with preserved user content and recovery integrity](extension-remove.md) — Planned — Implementer: Not assigned

## Entries

<!-- open-forge:generated-index:start -->
- [Implement creation of one reviewable Extension package outside a workspace](extension-create.md) - #Memory #Working #CLI #Task #Extension #Create #Lifecycle #Contextual
- [Implement Extension installation from exact reviewed package identity](extension-install.md) - #Memory #Working #CLI #Task #Extension #Install #Lifecycle #Contextual
- [Implement Extension removal with preserved user content and recovery integrity](extension-remove.md) - #Memory #Working #CLI #Task #Extension #Remove #Lifecycle #Contextual
- [Implement Extension update with source review, ownership, and recovery integrity](extension-update.md) - #Memory #Working #CLI #Task #Extension #Update #Lifecycle #Contextual
- [Implement root Framework installation into a selected workspace](install.md) - #Memory #Working #CLI #Task #Install #Framework #Lifecycle #Contextual
- [Implement root Framework update from accepted lifecycle identity](update.md) - #Memory #Working #CLI #Task #Update #Framework #Lifecycle #Contextual
<!-- open-forge:generated-index:end -->
