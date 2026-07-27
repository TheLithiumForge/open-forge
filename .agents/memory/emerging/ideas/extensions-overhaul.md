---
open-forge:
  description: Define the future Extensions package, source, trust, compatibility, composition, ownership, lifecycle, catalogue, and CLI boundaries
  tags: [Memory, Idea, Contextual, Candidate, Extension, Architecture, Product, Refactor]
---

# Extensions Overhaul

Open Forge Extensions are a dogfooded MVP whose long-term architecture remains intentionally open. The [Extensions MVP Architecture](../../crystallized/documents/extensions/architecture.md) owns current behavior, accepted runtime boundaries, safety properties, and known liabilities. This file owns candidate future design until it is coherent and accepted enough to replace the MVP view.

## Accepted Constraints

The overhaul begins from the stable boundaries already owned by the current architecture:

- Extensions remain optional and explicitly selected
- Installed human-readable files retain complete runtime meaning
- Packages contribute whole files through ordinary Framework routes
- Package metadata and ownership state do not become agent authority
- Manual plain-file installation remains possible
- Composition, mutation, update, and removal remain previewable and reviewable
- Local content, shared ownership, dependency integrity, containment, and route reachability remain protected

## Design Questions

### Package Boundary

- What makes a set of files one extension?
- When is a narrow package, mixed package, or convenience pack appropriate?
- How is one canonical payload owner maintained?
- Which metadata is required for managed and unmanaged distribution?

### Source And Trust

- Which source types are supported?
- Does remote distribution belong in Open Forge?
- How are provenance, integrity, authorship, and review presented?
- Which content may enter baseline-loading routes?
- How are untrusted instructions inspected before installation?

### Identity And Compatibility

- What identity remains stable across source movement?
- What does a version guarantee?
- How are Framework and CLI compatibility expressed?
- How are agent-runtime capability requirements represented without binding Extensions to one provider?
- How are deprecation and replacement communicated?

### Composition

- Which required, optional, and conflicting relationships earn first-class semantics?
- Is capability satisfaction justified, or are exact package relationships sufficient?
- How can external skills or packages satisfy a requirement safely?
- How does dependency closure remain visible and reviewable?

### Ownership And Scope

- Where does lifecycle ownership live for one workspace, nested scopes, submodules, and multi-repository sources of truth?
- How does Open Forge coexist with other package managers?
- What happens when a user modifies a managed file?
- How do scoped installation, update, and removal work?
- How do authored, managed, and generated boundaries compose?

### Lifecycle

- How do first installation, update, migration, restoration, removal, and orphan handling differ?
- Which changes require explicit user direction?
- How is interrupted application recovered?

### Catalogue Governance

- Which admission and review criteria apply?
- What evidence is required for first-party inclusion?
- Which stability labels and support promises are useful?
- Who owns maintenance and deprecation?
- How does discovery remain cheap without adding mandatory context?

## Candidate Component Boundary

The future implementation may separate:

```text
Extension source providers
  -> package and manifest model
    -> dependency and compatibility resolver
      -> assembled installation plan
        -> CLI safety and transaction application
          -> ordinary installed Framework files
```

The Extensions domain would own package identity, source metadata, dependencies, compatibility, and desired composition. The CLI would own filesystem inspection, target planning, safety checks, presentation, application, rollback, and Git integration. The Framework would own runtime meaning after installation.

This boundary remains a candidate until the design questions show whether each component has a stable independent responsibility.

## Migration Direction

Before generalizing the MVP:

1. Review every first-party package for broad reusable value and correct granularity
2. Classify current manifest and lifecycle behavior as invariant, MVP compatibility, or replacement candidate
3. Extract Extensions domain behavior from the CLI implementation
4. Define source, identity, scope, compatibility, ownership, and lifecycle contracts
5. Establish a common installation plan shared with the CLI architecture
6. Preserve safety and integration tests around desired invariants
7. Add migrations only where real versioned state requires them
8. Avoid remote distribution until trust and provenance have an accepted design
9. Keep installed files manually understandable throughout the transition

## Promotion Conditions

A replacement current Extensions architecture is justified when:

- Package and source boundaries are accepted
- Trust and compatibility semantics are explicit
- Scope and ownership work across the intended workspace shapes
- Lifecycle intents and required user decisions are distinct
- The Extensions and CLI responsibilities compose through one plan contract
- Catalogue governance is credible enough for the distribution being proposed
