---
open-forge:
  description: Implement root and Extension creation, installation, update, and removal lifecycle commands
  tags: [Memory, Working, CLI, Task, Lifecycle, Extension, Install, Update, Contextual]
---

# Lifecycle Commands

## Task State

- State: Planned.
- Parent: [Complete The Replacement CLI](../00-cli-development.md).
- Prerequisites: Extension discovery, `index`, and Mutation Foundation.

## Shared Boundary

Lifecycle commands consume accepted package identities, catalogue facts,
workspace state, lifecycle schema, locks, Git, recovery, source review, and
generated-navigation primitives. Each command retains its selection, plan,
effects, findings, and result.

No command recognizes legacy lifecycle files. Package wrappers do not implement
lifecycle behavior. `extension create` has no workspace subject and uses exact
destination identity and isolated Git/recovery instead of the workspace lock.

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
- [Implement root Framework installation into a selected workspace](install.md) - #Memory #Working #CLI #Task #Install #Framework #Lifecycle #Contextual
- [Implement root Framework update from accepted lifecycle identity](update.md) - #Memory #Working #CLI #Task #Update #Framework #Lifecycle #Contextual
- [Implement Extension installation from exact reviewed package identity](extension-install.md) - #Memory #Working #CLI #Task #Extension #Install #Lifecycle #Contextual
- [Implement Extension update with source review, ownership, and recovery integrity](extension-update.md) - #Memory #Working #CLI #Task #Extension #Update #Lifecycle #Contextual
- [Implement Extension removal with preserved user content and recovery integrity](extension-remove.md) - #Memory #Working #CLI #Task #Extension #Remove #Lifecycle #Contextual

<!-- open-forge:generated-index:end -->
