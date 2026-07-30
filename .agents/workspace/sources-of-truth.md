---
open-forge:
  description: Current map of the repository's important authoritative sources and representations
  tags: [Workspace, Repository, CurrentTruth, Evergreen]
---

# Sources Of Truth

Route map for this repository's important authoritative sources and representations. Each destination retains its own authority.

- [current product purpose, promise, scope, success criteria, and non-goals.](../memory/crystallized/documents/vision.md) - #CurrentTruth #Evergreen #Vision #Product #Document
- [current foundational principles that define Open Forge's identity and guide unfamiliar product, Framework, and tooling choices.](../memory/crystallized/documents/principles.md) - #CurrentTruth #Evergreen #Principle #Foundation #Identity #Product #Document
- [current user-facing explanation of what Open Forge is and how to use it.](../../README.md) - #Evergreen #Documentation #Product
- [current Open Forge system map, component boundaries, context flow, authority, evolution, scaling, and tool boundary.](../memory/crystallized/documents/architecture.md) - #CurrentTruth #Evergreen #Architecture #Framework #Document
- [current internal architecture of the shipped Open Forge Framework, including Core and Memory.](../memory/crystallized/documents/framework/architecture.md) - #CurrentTruth #Evergreen #Architecture #Framework #Core #Memory #Document
- [current CLI MVP role, command surface, deterministic state, safety model, verification boundary, proven properties, and liabilities.](../memory/crystallized/documents/cli/architecture.md) - #CurrentTruth #Evergreen #Architecture #CLI #MVP #Document
- [current Extensions MVP package semantics, composition, runtime boundary, ownership lifecycle, safety properties, and liabilities.](../memory/crystallized/documents/extensions/architecture.md) - #CurrentTruth #Evergreen #Architecture #Extension #MVP #Document
- [current user contract for CLI commands, options, safety behavior, outputs, and limitations.](../../docs/cli.md) - #CurrentTruth #Evergreen #Documentation #CLI
- [current maintainer contracts for reviewed source and repository surfaces.](../memory/crystallized/documents/maintenance/_maintenance.md) - #CurrentTruth #Evergreen #Maintenance #Governance #Document
- [the installable payload users receive; runtime truth for installed workspaces, including this one.](../../src/open-forge/) - #CurrentTruth #Payload
- [exact current deterministic CLI behavior.](../../src/cli/cli.ts) - #CLI #Implementation
- [current first-party extension catalogue.](../../src/extensions/README.md) - #CurrentTruth #Extension #Catalogue
- [bundled first-party extension source packages and manifests.](../../src/extensions/) - #Extension #Implementation
- [the reproducible evaluation instrument: agnostic scenarios, reusable primitives, exact meta-scenarios, external isolated runs, orchestration support, and durable result publications.](../../benchmarks/) - #Benchmark #Evaluation
- [build script](../../build.ts) - #Build
- [build output; never edit it by hand.](../../dist/) - #Build #Generated
- [current repository workflow for changing, verifying, building, packaging, and releasing Open Forge.](../../docs/development.md) - #Documentation #Development
- [current user and author contract for choosing, installing, creating, updating, and removing Extensions.](../../docs/extensions.md) - #CurrentTruth #Evergreen #Documentation #Extension
- [durable benchmark publications; conforming current runs use `<date-time>/<run-name>/` with `summary.md` plus `raw/`, while explicitly incomplete, flat, and `pre-harness/` reports remain historical context.](../../benchmarks/results/) - #Benchmark #Result #Evidence
- [accepted evaluation syntheses derived from benchmark evidence.](../memory/crystallized/documents/evaluations/) - #Memory #Evaluation

Fast development validation: `bun run index`, `bun run test`, and `bun run build`. Use `bun run test:closure` for affected OS/process boundaries and `bun run test:ci` for the complete unit-plus-closure suite before relevant closeout; raw `bun test` discovers both tiers.
