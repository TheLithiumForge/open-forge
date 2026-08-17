---
open-forge:
  description: Historical CLI-v2 source: Compose doctor and repair through visible typed domain calls instead of resolving behavior through diagnostic strings
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Explicit Diagnostic Composition

## Context

Doctor needs broad coverage without becoming a monolith. A registry keyed by
diagnostic or fixer strings would hide behavior behind runtime lookup and
weaken definition navigation, exhaustiveness, and repair safety. Executable
repair behavior also cannot enter serializable public findings.

## Decision

The diagnostic coordinator directly calls each accepted domain in a fixed
visible order. Domains return typed reports and create internal typed repair
proposals beside eligible findings. The repair coordinator directly calls the
corresponding domain planners. Public codes identify evidence only and never
resolve behavior.

The [Diagnosis And Repair
Contract](../../documents/cli/contracts/diagnosis-and-repair.md) owns exact domains,
finding vocabulary, completeness, ordering, proposal, conflict, application,
and rediagnosis semantics.

## Rationale

The accepted domain count is small and architecturally meaningful. One visible
import and call per domain is useful friction because adding coverage must also
address ordering, result shape, and repair authority. Direct composition keeps
go-to-definition and exhaustive TypeScript checks effective while preserving
serializable results.

## Rejected Alternatives

- A string-keyed registry hides call relationships and makes codes executable authority.
- Callable repairs inside findings mix private behavior with the public JSON contract.
- Looking up a repair later by finding code recreates the same hidden dispatch.
- A universal plugin surface would require an unaccepted trust, versioning, isolation, and authority model.

## Consequences

- Adding a domain or proposal variant creates intentional compile-time work.
- Domain tests remain focused while coordinator tests prove real composition.
- JSON contains no executable behavior.
- Some coordinator repetition is retained because the visible call graph is valuable.

## Related Sources

- [Diagnosis And Repair Contract](../../documents/cli/contracts/diagnosis-and-repair.md)
- [Diagnostic Domain Slice Pattern](../../../../patterns/open-forge/cli/diagnostics/diagnostic-domain-slice.md)
- [Planned Mutation Pattern](../../../../patterns/open-forge/cli/filesystem/planned-mutation.md)
