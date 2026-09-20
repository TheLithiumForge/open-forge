---
open-forge:
  description: Implement fail-closed operation, presentation, rendering, diagnostic, output, and completion stages
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Foundation, Pipeline, Output, Diagnostics, Complete]
---

# Implement The Shell Pipeline And Output

## Task State

- State: Complete.
- Implementer: Mastermind.
- Parent: [CLI Foundation](../_foundation.md).
- Plan step: F2 pipeline and output portion.

## Expected Outcome

Closed command bindings can execute through directly callable immutable stages
that validate before effects, invoke one operation and renderer at most once,
isolate diagnostics, write explicit streams, and return fixed process completion.

## Required Stages

- `CliOperationStage.InvokeAsync<TRequest, TResult>` validates request, operation,
  status contract, and cancellation input before invoking the operation once.
- `CliPresentationStage.Create<TResult>` validates concrete result shared facts and
  combines them with accepted presentation without rendering.
- `CliRenderingStage.Render<TResult>` validates format and renderer set before
  selecting exactly one cached primary renderer. It optionally invokes one
  diagnostic renderer only when verbose.
- `CliOutputStage.WriteAsync` validates both targets and writers before the first
  write, writes primary content once, then optional diagnostic content once to
  stderr, and returns a receipt.
- `CliCompletionStage.Complete` maps the already-formed result through exhaustive
  status policy and returns process completion.
- `CliCommandPipeline<TRequest, TResult>` is a thin coordinator over these public
  stages. It does not duplicate their validation or hide effects.

## Rendering And Diagnostic Rules

- Human and JSON delegates remain concrete and command-supplied.
- JSON primary output always targets stdout and remains one document.
- Human target follows exhaustive status policy.
- Diagnostic content is optional, bounded, pre-escaped, redacted, and stderr-only.
- Verbose mode changes no request, operation, result, primary content, status,
  next action, or exit.
- Renderer and writer exceptions do not trigger retries or operation re-execution.

## Evidence

Unit evidence enters every stage directly with malformed and valid messages. Prove
validation before delegate/writer side effects, operation-once, cancellation,
one-renderer selection, unknown finite rejection, writer selection, write counts,
diagnostic isolation, JSON integrity, status preservation, completion exits, and
no ambient console access.

Use counting delegates and in-memory explicit writers. Do not call these cases
process or filesystem evidence.

## Protected Boundaries

- No command renderer, command result, parser, workspace resolution, process
  termination, logging framework, retry, rollback, compensation, or service
  location.

## Stop Conditions

Stop if direct-stage validation cannot be complete without rerunning a prior
stage, if diagnostics can change primary behavior, or if the interface requires
reflection or per-call capturing delegate construction.

## Completion

Complete when the full typed stage chain and every direct entry point are proven
before the root host or first command consumes them.
