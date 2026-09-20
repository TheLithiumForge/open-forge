---
open-forge:
  description: Freeze the route-free Shell definitions, binding model, immutable messages, result contract, and serialization call surfaces
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Foundation, Shell, Contract, Architecture, Complete]
---

# Freeze The Core Shell Contracts

## Task State

- State: Complete.
- Implementer: Mastermind.
- Responsible role: Mastermind.
- Parent: [CLI Foundation](../_foundation.md).
- Plan step: F2 callable-contract portion.

## Expected Outcome

Core contains the complete route-free callable model needed by every command.
The contracts compile, express dependency direction, and make later behavior
Tasks closed without adding a placeholder command or domain behavior.

## Required Source Model

Create cohesive files below `core/OpenForge.Cli.Core/Shell/`:

### Definitions

- `CliSyntaxDefinition` and `CliOptionDefinition<T>` own names, descriptions,
  arity, defaults, and finite spellings.
- `CliPresentationFormat`, `CliView`, `CliVerbosity`, and `CliOutputTarget` are
  finite values with centralized definitions.
- `CliSemanticStatus` and `CliStatusPolicy` exhaustively map seven statuses to
  fixed exits and primary targets.
- `CliTerminalMode` and `CliTerminalPolicy` represent none, help, and version plus
  composition conflicts.
- `CliProcessIdentity` carries executable name and generated informational version.

Split files by these responsibilities. Do not recreate one broad definitions file.

### Composition

- `CliCommandBinding<TRequest, TResult>` stores one concrete command definition,
  binder, operation delegate, renderer set, optional diagnostic renderer, and
  invalid-result factory.
- `ICliCommandBinding` exposes exact `Command` identity and one non-generic invoke
  boundary while preserving the closed generic internals.
- `CliCommandTree` stores the root symbol, ordered groups and leaves, and exact
  symbol-identity lookup.
- `CliRendererSet<TResult>` contains cached non-capturing human and JSON delegates.
- Delegates accept cohesive typed inputs and caller cancellation. They do not
  capture writers or locate services.

### Invocation And Messages

- `CliInvocation` contains process identity, presentation, terminal mode,
  workspace request, and normalized global facts only.
- `CliPresentation` is one immutable value containing format, view, and verbosity.
- `CliOutputWriters` contains explicit stdout and stderr `TextWriter` instances.
- `CliOperationRequest<TRequest>`, `CliOperationResult<TResult>`,
  `CliPresentationRequest<TResult>`, `CliRenderedOutput`, `CliOutputReceipt`, and
  `CliProcessCompletion` form the stage messages.
- `ICliCommandResult` exposes shared command identity, semantic status, optional
  workspace, and optional next action. It is not serialized.

### Serialization

- Define one source-generated STJ options/context boundary with reflection
  disabled and no command result registered yet.
- Define minimal generated YAML metadata models and the one static context call
  surface required by accepted command contracts. Behavior arrives in its
  dedicated integration step.

## Invariants

- Core Shell references no concrete command namespace.
- No type contains route, Foundation probe, or fake-operation identity.
- Generic request/result types close inside one binding.
- Unknown finite values can be rejected at direct stage entry.
- Writers do not appear in operation, result, presentation, or renderer records.
- Context records contain one stage or lifecycle only.
- Constructors validate null delegates, writers, and inconsistent finite policy.

## Evidence

Add pure Unit evidence for construction invariants, exhaustive status policy,
unknown finite values, command-identity lookup, cached renderer selection, closed
generic invocation, null rejection, and interface-versus-wire separation.

## Allowed Changes

- Core Shell contract files, assembly friend declarations, source-generation
  metadata required to compile, and matching Unit tests.
- No parser, stage, host, filesystem, or command behavior.

## Stop Conditions

Stop if the non-generic binding boundary requires an untyped result, reflective
generic invocation, runtime service lookup, or serialization through
`ICliCommandResult`. Return to Architecture rather than weakening the model.

## Completion

Complete when all later parser, pipeline, host, and command Tasks can name their
exact input and output contracts without adding another shell abstraction.
