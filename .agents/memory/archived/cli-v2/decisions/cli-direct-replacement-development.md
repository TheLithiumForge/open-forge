---
open-forge:
  description: Historical CLI-v2 source: The replacement CLI owns the default entrypoint from the beginning while the frozen MVP remains an explicitly invoked development reference
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Direct Replacement Development

## Context

The accepted replacement architecture and interface already document the intended behavior in enough detail to implement and verify coherent slices directly. A temporary wrapper, compatibility router, and command-root ownership registry would add migration behavior that must later be removed without improving the final architecture.

The MVP remains useful as executable evidence for independently desirable behavior and safety scenarios. It does not need to remain behind the public replacement entrypoint to provide that evidence.

## Decision

The replacement CLI owns `src/cli/cli.ts`, the default build entrypoint, and the distributed `open-forge` binary from the beginning of implementation.

The complete MVP source and its tests live under `src/cli-mvp/`. Its explicit development identity is `open-forge-old`, exposed through `bun run cli:old`; it never claims the replacement's `open-forge` identity. The MVP is frozen except for minimal adaptations needed to keep it directly runnable and its existing tests valid after relocation. It is a development reference and does not participate in the replacement build or public command dispatch.

There is no old/new wrapper, compatibility parser, fallback, implementation selector, command-root registry, or staged ownership cutover. An invocation of the replacement always stays in the replacement. An unimplemented command returns the replacement CLI's ordinary invalid-invocation result.

Development ports one coherent command or shared capability slice at a time. Each port follows the accepted replacement architecture and public contract, then proves its own handlers, registration, integration boundaries, presentation, and relevant packaged behavior. MVP behavior is retained only when current documentation or explicit review establishes that it remains desirable. The replacement never accepts legacy syntax merely because the MVP did.

The MVP may be invoked explicitly through its source path when comparison or retained tests are useful. Once the replacement is complete and the retained MVP evidence has either been promoted or deliberately retired, remove the MVP source and its separate tests.

## Rationale

Starting from the final entrypoint and source structure keeps every new module permanent, directly testable, and free of temporary dispatch constraints. The accepted documents provide the specification, while the independently runnable MVP provides evidence without becoming compatibility architecture.

The simpler boundary also makes development state honest. Implemented replacement commands work through the final executable. Unimplemented commands are visibly unavailable rather than silently entering a different parser and behavior model.

## Consequences

- `src/cli/cli.ts` invokes only the replacement composition root.
- `src/cli-mvp/cli.ts` remains explicitly runnable during development and is not built into the distributed CLI.
- MVP tests remain separate and prove only the frozen MVP until useful scenarios are recreated against replacement production code.
- New tests assert the accepted replacement contract rather than backward compatibility.
- The build and package contain one command implementation and no migration router.
- Porting order is an implementation plan, not public dispatch state.
- A replacement release requires every operation promised by its published interface to be implemented and verified.

## Authoritative Sources

- [Open Forge CLI Architecture](../../documents/cli/architecture.md)
- [Open Forge CLI Interface](../../documents/cli/interface.md)
- [Open Forge CLI MVP Architecture](../../documents/cli/mvp-architecture.md)

## Historical Context

- [Rejected parallel-entrypoint migration](../../../archived/analysis/2026-07-31_cli-parallel-entrypoint-migration.md)

## Decision Relationships

- [CLI source locality](cli-source-locality.md)
- [CLI testing architecture](cli-testing-architecture.md)
- [CLI command framework](cli-command-framework.md)
