---
open-forge:
  description: Add one native Shell question-and-answer transport without a prompt framework
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Foundation, Shell, Interaction]
---

# Add Native Interactive Session

## Task State

- State: Complete at integrated commit `e782090` after focused review and the
  combined managed/Native AOT acceptance gate.
- Parent: [Next-Wave Shared Foundations](_shared-foundations.md).
- Consumers: Extension Create, root Install, and the Route Inspect interaction
  correction.

## Outcome

Shell provides one BCL-only prompt-capable session over supplied input, prompt
output, and capability facts. It owns asynchronous question/answer, end-of-input,
and cancellation mechanics. The later protected root integration constructs it
from standard input, stderr, and redirected-stream facts; JSON, automatic, and
redirected flows never prompt, and stdout remains pure result output.

## Architecture And Ownership

- Expected production: `Shell/Interaction/**` only. Root composition remains a
  protected sequential integration surface.
- Expected tests: mirrored Shell Unit paths using real `StringReader` and
  `StringWriter`; no mocks or PTY framework.
- Protected: root composition, command-local questions/policy, generic operation
  delegates, unrelated requests, serialization, and command behavior.
- Root composition injects the session only into prompt-capable operation
  factories. Binders place only a command-local explicit interaction-policy
  Boolean in complete immutable requests. `CliInvocation`, generic binding and
  operation contracts, unrelated factories, and unrelated requests remain
  unchanged and carry no interaction or stream object.
- The root derives prompt capability only when both standard input and the stderr
  prompt stream are terminal-capable, using ordinary .NET redirection checks.

## Evidence

Cover prompt-capable/unavailable, answer, end of input, cancellation, exact
prompt output, supplied-stream ownership, and no ambient `Console` use. The first
prompt-capable command integrations own root construction, proof that unrelated
commands retain their invocation path, directly injected host interaction, and
redirected published-process noninteraction.

The neutral transport was reviewed and integrated at `e782090`. The combined
post-foundation gate passes Release with `0` warnings and `0` errors, managed
Unit `1284/1284`, Integration `500/500`, and EndToEnd `125/125`, Native AOT
Integration `500/500` and EndToEnd `125/125`, with zero skips.

Decisive focused evidence is transport Unit `8/8`, with an independent review
pass.

## Stop Conditions

Stop before adding an interactive package, dependency injection, a workflow
engine, generic retry/default policy, ambient console access in Core, terminal or
PTY framework, native probe, P/Invoke, or any feature addition/removal not
accepted by the maintainer.
