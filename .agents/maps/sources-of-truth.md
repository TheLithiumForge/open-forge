---
open-forge:
  description: Current map of the repository's important authoritative sources and representations
  tags: [Map, Repository, CurrentTruth, Evergreen]
---

# Sources Of Truth

Route map for this repository's important authoritative sources and representations. Each destination retains its own authority.

- [current product purpose, promise, scope, success criteria, and non-goals.](../memory/crystallized/documents/vision.md) - #CurrentTruth #Evergreen #Vision #Product #Document
- [current foundational principles that define Open Forge's identity and guide unfamiliar product, Framework, and tooling choices.](../memory/crystallized/documents/principles.md) - #CurrentTruth #Evergreen #Principle #Foundation #Identity #Product #Document
- [current user-facing explanation of what Open Forge is and how to use it.](../../README.md) - #Evergreen #Documentation #Product
- [current Open Forge system map, component boundaries, context flow, authority, evolution, scaling, and tool boundary.](../memory/crystallized/documents/architecture.md) - #CurrentTruth #Evergreen #Architecture #Framework #Document
- [current internal architecture of the shipped Open Forge Framework, including Core and Memory.](../memory/crystallized/documents/framework/architecture.md) - #CurrentTruth #Evergreen #Architecture #Framework #Core #Memory #Document
- [current cross-cutting structure, project graph, dependency direction, and stable implementation invariants for the non-shipping C# replacement CLI.](../memory/crystallized/documents/cli/architecture.md) - #CurrentTruth #Evergreen #Architecture #CLI #Replacement #NativeAOT #Document
- [current replacement CLI command-contract roles, topology, authority boundaries, and links to command-local Interface, Behavior, and optional Technical Design contracts.](../memory/crystallized/documents/cli/command-contract-set.md) - #CurrentTruth #Evergreen #CLI #Replacement #Contract #Document
- [current detailed replacement CLI command contracts under the `contracts/` route, with `index-candidate/` and `references-candidate/` staging scopes pending final compatibility-name validation.](../memory/crystallized/documents/cli/contracts/_contracts.md) - #CurrentTruth #Evergreen #CLI #Replacement #Contract #Document
- [current caller-visible shared CLI result envelope, source-location, semantic-status, process-exit, primary-stream, and compatibility coordinates.](../memory/crystallized/documents/cli/contracts/shared/result-coordinates/_result-coordinates.md) - #CurrentTruth #Evergreen #CLI #Replacement #Contract #Result #Document
- [current cross-command operation conventions for the non-shipping replacement CLI.](../memory/crystallized/documents/cli/shared-operation-contract.md) - #CurrentTruth #Evergreen #CLI #Replacement #Contract #Document
- [current exact realization designs for shared replacement CLI capabilities.](../memory/crystallized/documents/cli/technical-designs/_technical-designs.md) - #CurrentTruth #Evergreen #CLI #Replacement #TechnicalDesign #Document
- [current replacement CLI package graph, x64 and ARM64 targets, staging, packing, checksums, proof, and publication boundary.](../memory/crystallized/documents/cli/distribution.md) - #CurrentTruth #Evergreen #CLI #Replacement #Distribution #Document
- [current rationale for replacement CLI dependency roles, central exact-version ownership, and Native AOT constraints.](../memory/crystallized/decisions/cli-dependency-policy.md) - #CurrentTruth #Evergreen #CLI #Replacement #Dependency #Decision
- [historical frozen CLI MVP role, command surface, deterministic state, safety model, verification boundary, proven properties, and liabilities.](../memory/crystallized/documents/cli/mvp-architecture.md) - #CurrentTruth #Evergreen #Architecture #CLI #MVP #Legacy #Document
- [active top-down work graph, Task state, and evidence for the greenfield replacement CLI.](../memory/working/cli-development/_cli-development.md) - #Contextual #CLI #Architecture #Plan #Task #Development
- [maintainer-authored emerging findings retained until explicit archival or pruning direction.](../memory/emerging/authors-findings/_authors-findings.md) - #Contextual #Candidate #AuthorsFindings #Author
- [historical CLI-v2 designs, decisions, governance, plans, and implementation records kept as raw input.](../memory/archived/cli-v2/_cli-v2.md) - #Contextual #Historical #CLI #CLI-v2
- [current Extension package meaning, composition, and runtime boundary.](../memory/crystallized/documents/extensions/architecture.md) - #CurrentTruth #Evergreen #Architecture #Extension #Document
- [current public contract for the non-shipping replacement CLI commands, options, safety behavior, outputs, and limitations.](../../docs/cli.md) - #CurrentTruth #Evergreen #Documentation #CLI #Replacement
- [current maintainer contracts for reviewed source and repository surfaces.](../memory/crystallized/documents/maintenance/_maintenance.md) - #CurrentTruth #Evergreen #Maintenance #Governance #Document
- [the installable payload users receive; runtime truth for installed workspaces, including this one.](../../src/open-forge/) - #CurrentTruth #Payload
- [current C# CLI implementation, projects and required resources.](../../src/cli/) - #CLI #Implementation
- [current repository build, delivery, package and agent tooling.](../../scripts/) - #Tooling #Build
- [current first-party extension catalogue.](../../src/extensions/README.md) - #CurrentTruth #Extension #Catalogue
- [bundled first-party extension source packages and manifests.](../../src/extensions/) - #Extension #Implementation
- [shared local and CI commands](../../package.json) - #Build
- [build output; never edit it by hand.](../../artifacts/) - #Build #Generated
- [current repository workflow for changing, verifying, building, packaging, and releasing Open Forge.](../../docs/development.md) - #Documentation #Development
- [current public documentation site, with getting started, concepts, and a file-by-file reference for every first-party Extension; it also publishes the CLI, Extension, and development guides.](../../src/docusaurus/) - #Evergreen #Documentation #Site
- [current user and author contract for choosing, installing, creating, updating, and removing Extensions.](../../docs/extensions.md) - #CurrentTruth #Evergreen #Documentation #Extension

The retired MVP remains available in Git history at `c4428a90`. All 28 native
CLI commands are implemented. The current Task records distinguish accepted
local evidence from matching-host execution and publication still unperformed.
