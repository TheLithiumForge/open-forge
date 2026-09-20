---
open-forge:
  description: Route and Find work showed that accepted future consumers should be checked before a capability becomes command-local
  tags: [Memory, Observation, AgentLearning, Contextual, Candidate, CLI, Architecture, Locality, Shared]
---

# Shared CLI Consumers Should Be Considered Before Local Implementation

## Observation

Route and Find now contain classes and functions whose mechanics may also be
needed by References, Context, Index, and later CLI commands. Some of these
capabilities may need to move into neutral Framework or command-family scopes.
That promotion should happen only after the consumers and identical meaning are
proved, but the consumer horizon should be considered before another command
builds a local substitute.

## Evidence

Find initially kept generic YAML event parsing in its command scope while Route
already had separate Markdown frontmatter extraction. The later top-down review
found the duplicated document boundary and established the shared
`Framework/Documents/Markdown` and `Framework/Documents/Yaml` foundations before
Find acceptance. The correction required an additional Gray, Red, Green, Blue,
and Purple sequence even though both command slices had focused passing evidence.

The current [CLI Framework layer](../../crystallized/documents/cli/layers/framework.md)
already names source, routing, Markdown, YAML, and generated-navigation consumers.
The active [Plan](../../working/cli-development/plan.md#approach) requires neutral
mechanical foundations before dependent commands and keeps semantic policy local
until multiple consumers prove identical meaning.

## Interpretation

The occurrence supports an explicit shared-capability audit during command
Preflight. The audit should list accepted future consumers, identify neutral
mechanics, and decide whether an existing shared boundary is sufficient before
feature-local code is frozen. It must not promote command-specific status,
findings, rendering, or policy merely because names or algorithms look similar.

The strongest alternative is to keep every capability local until a second
command is implemented. That minimizes speculative abstraction, but it can make
the second command pay for migration and correction after the first local API and
tests have hardened. The current candidate approach keeps the same two-consumer
proof for semantic promotion while checking the known consumer horizon earlier.

## Scope And Uncertainty

This evidence applies to the replacement CLI's Route and Find foundations and to
the accepted References, Context, and Index consumers. It does not establish
that every Route or Find helper should move, or that similar implementation alone
proves shared meaning. The next command Preflight should classify concrete
callables before any promotion decision is accepted.

## Follow-Up And Promotion Signals

- Audit Route and Find capability consumers before References and Index freeze
  their local architecture.
- Record which candidates have identical inputs, outputs, invariants, and failure
  meaning across real consumers.
- Keep command-specific interpretation and presentation local.
- Promote a reusable planning or implementation rule only if another command
  avoids duplicated foundations or a later migration through this audit.

## Related Sources

- [Program Architecture And Delegation](../../../directives/program-architecture.md)
- [Source Locality](../../../directives/source-locality.md)
- [Replacement CLI Architecture](../../crystallized/documents/cli/architecture.md)
- [Architectural Context Delegation Gap](2026-08-21_architectural-context-delegation-gap.md)
