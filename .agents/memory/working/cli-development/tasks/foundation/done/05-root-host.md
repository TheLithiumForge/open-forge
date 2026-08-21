---
open-forge:
  description: Implement the thin executable host and explicit command-free composition root
  tags: [Memory, Working, CLI, Task, Foundation, Host, Composition, Process, Contextual, Complete]
---

# Implement The Root Host

## Task State

- State: Complete.
- Implementer: Mastermind.
- Parent: [CLI Foundation](../_foundation.md).
- Plan step: F3.

## Expected Outcome

`OpenForge.Cli` is a retained thin executable that composes Core explicitly,
handles process boundaries once, and runs without a command or probe.

## Required Root Types

- `Program.cs` delegates immediately to `CliHost.RunAsync` and returns its exit.
- `CliHost` receives arguments, environment/current-directory facts, explicit
  stdout/stderr writers, and cancellation. It invokes Core once and contains no
  command branch.
- `CliCompositionRoot` constructs process identity, definitions, parser,
  route-free command tree, pipeline capabilities, and Core entry boundary through
  direct constructors.
- `CliProcessEnvironment` is one immutable root-owned value. Core receives only
  the facts needed by invocation resolution.
- Generated version source has one deterministic MSBuild owner and is available
  to help/version and process evidence.

## Route-Free Behavior

- `--version` returns generated version through the terminal path and performs no
  workspace or operation work.
- `--help` uses standard symbol help plus bounded product identity and Discovery
  text. It does not advertise a command as executable.
- Bare root shows root help with success under the accepted group/root policy.
- Unknown input produces parser diagnostics and the fixed invalid exit without a
  domain result.
- Core never calls `Environment.Exit` or ambient `Console`.

## Evidence

Unit evidence proves composition has zero leaf bindings and no service container.
Integration evidence invokes `CliHost` with explicit writers and verifies version,
help, bare root, unknown input, cancellation hookup, stream isolation, and no
workspace access. End-to-end source is added by the Test/AOT Task against the
actual built or published process.

## Protected Boundaries

- No command symbol, route-specific Discovery row, fake operation, Foundation
  request/result, filesystem probe, command result envelope, or package wrapper.

## Stop Conditions

Stop if host behavior requires a fake command to exercise the pipeline, if Core
must depend on root, or if a process global must be cached in Core.

## Completion

Complete when the executable host is useful for real help/version/parser behavior,
stays command-free, and exposes the exact composition seam later command Tasks use.
