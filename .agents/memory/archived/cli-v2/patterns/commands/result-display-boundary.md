---
open-forge:
  description: Historical CLI-v2 source: Return one typed command result and select human or structured display only at the CLI boundary
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Result And Display Boundary

## Boundary

The accepted [CLI Interface](../../../../memory/crystallized/documents/cli/interface.md#results-and-presentation)
owns the exact public envelope, operation identifiers, statuses, messages,
workspace reference, unexpected-failure result, structured presentation, and
status-to-exit mapping. Focused operation contracts own their data. This
Pattern owns the reusable one-way source and execution arrangement that
implements those contracts.

## Shape

```text
named handler
  -> typed operation result
  -> selected display adapter
  -> rendered stdout and stderr
  -> named process completion
```

A handler returns its focused typed result. It does not call `console`, write a
stream, select presentation, or set process completion state. It imports the
shared result contract rather than redeclaring the public envelope.

Operation-specific data remains focused. Status uses its [Status Result
Contract](../../../../memory/crystallized/documents/cli/contracts/status-results.md),
and mutation results compose the projection owned by the [Mutation Execution
contract](../../../../memory/crystallized/documents/cli/contracts/mutation-execution.md)
only after a complete plan exists. Unrelated operation concerns never grow the
shared envelope.

## Display Adapters

Select one adapter at the CLI boundary. The internal adapter shape may remain
generic:

```ts
export interface DisplayAdapter<Result> {
  render(result: Result): DisplayOutput;
}

export type DisplayOutput = { readonly stdout: string; readonly stderr: "" } | { readonly stdout: ""; readonly stderr: string };
```

The union permits both streams to be empty but never permits both to be
non-empty. A renderer requests stdout alone, stderr alone, or no ordinary
output for one result.

An operation or family owns its useful terminal presentation. The structured
adapter serializes the same typed result. Neither adapter reruns application
behavior, changes semantic status, or manufactures another result shape.

The outer executable boundary alone writes final streams and maps the result
through the imported process-completion contract. Unexpected exceptions cross
the linked Interface safety boundary before display; adapters never serialize
raw caught errors.

## Placement

Shared result, presentation-selection, structured-display, and process
completion support lives at CLI scope. Command-local terminal renderers and
focused data remain beside their operation. Apply the [Named Values](../../typescript/named-values.md)
Pattern to protocol-significant values and move a definition only to the
nearest scope with demonstrated consumers.

## Review Checks

- The handler is directly callable without terminal or process state.
- Human and structured adapters consume the same typed result.
- Exactly one adapter is selected for one invocation.
- The completion contains at most one non-empty final stream.
- Structured output contains one document and no incidental logs.
- Rendering never performs or repeats application behavior.
- Operation-specific concerns remain outside the shared envelope.
- Public declarations and status-to-exit values come from their authoritative contract rather than this Pattern.
- The executable boundary alone writes streams and applies process completion.
- Callback and error-event failures from one write are contained as one failed
  completion, and temporary stream listeners are cleaned after that lifecycle.
