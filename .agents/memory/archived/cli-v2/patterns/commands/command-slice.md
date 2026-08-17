---
open-forge:
  description: Historical CLI-v2 source: Keep one CLI command definition, named handler, focused support, and direct tests in one inspectable behavioral slice
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Command Slice

## Boundary

The [CLI Interface](../../../../memory/crystallized/documents/cli/interface.md)
owns exact commands, arguments, flags, aliases, and public results. The
[Predictable Command Surface](predictable-command-surface.md) owns public
grammar and global policy registration. [Guided Operation](guided-operation.md)
owns request completion, the [Result And Display Boundary](result-display-boundary.md)
owns completion, and the [Operation Prerequisites
contract](../../../../memory/crystallized/documents/cli/contracts/operation-prerequisites.md)
owns prerequisite semantics. This Pattern owns one leaf's colocated module,
handler, registration, local support, and direct-test shape.

## Leaf Module

A small command begins as one folder with one same-named module and one
adjacent test:

```text
src/cli/commands/
  status/
    status.ts
    status.test.ts
```

The same-named module exports named command metadata, an independently
callable named handler, and the function that registers the command. The
example is schematic until production source owns the exact types and
Commander integration:

```ts
export const StatusCommand = {
  name: "status",
  description: "Show workspace and Framework orientation.",
} as const;

export async function handleStatus(input: StatusInput, dependencies: StatusDependencies): Promise<StatusResult> {
  return inspectStatus(input, dependencies);
}

export function registerStatusCommand(parent: CommandParent, dependencies: StatusDependencies, complete: CompleteCommand<StatusResult>): void {
  parent
    .command(StatusCommand.name)
    .description(StatusCommand.description)
    .action(async (options) => {
      const result = await handleStatus(toStatusInput(options), dependencies);
      await complete(result, toPresentationSelection(options));
    });
}
```

Command-local syntax and descriptions live in named readonly metadata beside
the command metadata. The registration function consumes those definitions,
maps inferred parser values into the focused handler input, awaits the handler,
and hands the result to the supplied completion function. It does not
redeclare global policy or perform presentation itself.

## Family Composition

A group module owns family registration and local help. It creates the group
from its supplied parent and directly calls its child registration functions:

```ts
export function registerRouteCommand(parent: CommandParent, dependencies: RouteDependencies, complete: CompleteCommand<RouteResult>): void {
  const route = parent.command(RouteCommand.name).description(RouteCommand.description);

  registerRouteListCommand(route, dependencies, complete);
  registerRouteInspectCommand(route, dependencies, complete);
  registerRouteInitCommand(route, dependencies, complete);
  registerRouteRebuildCommand(route, dependencies, complete);
}
```

The group contains no domain handler. Its only behavior is visible composition
of directly imported children.

## Local Support And Promotion

Keep parsing, presentation, fixtures, request resolution, and focused support
inside the command folder. Split only when a real responsibility warrants it.
Use an adjacent test only for a command expected to remain a one-test subject.
When a command has or is expected to need multiple test files, create
`__tests__/` from its first test and collect its local evidence and test-only
support there instead of leaving a long alternating list of production and
test files:

```text
status/
  status.ts
  render-status.ts
  __tests__/
    status.test.ts
    status-workspace.integration.test.ts
    render-status.test.ts
```

Name tests by the production behavior they prove. Keep direct and integration
depth in the filename suffix rather than adding tier subdirectories. A
whole-executable end-to-end journey remains at CLI scope because the command
folder does not own the complete process boundary.

Pass focused dependencies directly. The handler calls its prerequisite
inspectors and application behavior through visible typed branches. Do not add
a service locator, reflective requirement registry, sibling command import, or
speculative layer.

When explicit input may be completed through defaults or a wizard, keep the
resolver local and apply the Guided Operation Pattern. Test organization and
process-boundary evidence follow the [CLI Tiered Test
Slice](../bun/tiered-test-slice.md). Shared support moves through the [Nearest Shared
Scope](../../../software/source-locality/nearest-shared-scope.md) Pattern only after demonstrated
reuse.

## Review Checks

- The command folder has one obvious same-named entry module.
- One-test commands may use adjacency; commands expected to need multiple local test files start with `__tests__/` and do not add tier subdirectories.
- Named metadata, handler, and registration function are intentional exports.
- The handler is directly callable without spawning the CLI.
- Registration receives its parent, dependencies, and completion function explicitly.
- A group visibly composes children and contains no domain operation.
- Command-specific support and tests remain local.
- Splits name real responsibilities.
- No sibling-private import, service locator, or string-keyed requirement registry appears.
- Exact public grammar and semantic contracts remain at their linked authoritative sources.
