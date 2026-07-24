---
open-forge:
  description: Current map of the repository locations that own or represent important project state
  tags: [Workspace, Repository, CurrentTruth, Evergreen]
---

# Sources Of Truth

Route map for this repository's important owners and representations. Each destination retains its own authority.

- [current product purpose, promise, principles, success criteria, and non-goals.](../memory/crystallized/documents/vision.md) - #CurrentTruth #Evergreen #Vision #Product #Document
- [current user-facing explanation of what Open Forge is and how to use it.](../../README.md) - #Evergreen #Documentation #Product
- [current framework system boundary, execution path, ownership, truth model, and packaging.](../memory/crystallized/documents/architecture.md) - #CurrentTruth #Evergreen #Architecture #Framework #Document
- [current CLI behavior truth; prefer it over inferring intent from the implementation.](../../docs/cli.md) - #CurrentTruth #Evergreen #Documentation #CLI
- [maintainer and AI governance for this repository; descriptors govern payload files and must not become hidden runtime context for installed users.](../../docs/framework/) - #Governance
- [the installable payload users receive; runtime truth for installed workspaces, including this one.](../../src/open-forge/) - #CurrentTruth #Payload
- [the MVP CLI implementation; `src/cli/cli.unit.test.ts`, `src/cli/cli.closure.test.ts`, and `src/cli/extensions.integration.closure.test.ts` cover its pure and command-boundary behavior, with shared infrastructure in `tests/support/` and `tests/run-tests.ts`.](../../src/cli/cli.ts) - #CLI #Implementation
- [bundled first-party extensions, each with `extension.json` plus `payload/`; `workflow-essentials` is the first one.](../../src/extensions/) - #Extension
- [the reproducible evaluation instrument: agnostic scenarios, reusable primitives, exact meta-scenarios, external isolated runs, orchestration support, and durable result publications.](../../benchmarks/) - #Benchmark #Evaluation
- [build script](../../build.ts) - #Build
- [build output; never edit it by hand.](../../dist/) - #Build #Generated
- [development documentation](../../docs/dev.md) - #Documentation #Development
- [extension authoring documentation](../../docs/extensions.md) - #Documentation #Extension
- [durable benchmark publications; conforming current runs use `<date-time>/<run-name>/` with `summary.md` plus `raw/`, while explicitly incomplete, flat, and `pre-harness/` reports remain historical context.](../../benchmarks/results/) - #Benchmark #Result #Evidence
- [accepted evaluation syntheses derived from benchmark evidence.](../memory/crystallized/documents/evaluations/) - #Memory #Evaluation

Fast development validation: `bun run index`, `bun run test`, and `bun run build`. Use `bun run test:closure` for affected OS/process boundaries and `bun run test:ci` for the complete unit-plus-closure suite before relevant closeout; raw `bun test` discovers both tiers.
