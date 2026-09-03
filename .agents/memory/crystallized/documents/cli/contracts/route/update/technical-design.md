---
open-forge:
  description: Accepted bounded lexical recognizer for Route Update attached-empty responsibility input
  responsibility: Define the exact implementation exception that preserves the Route Update public responsibility grammar
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Route, Update, TechnicalDesign, Parser, EdgeCase, CurrentTruth]
---

# route update Technical Design

## Status And Authority

This Technical Design records the one accepted Route Update parser exception.
The [Interface Contract](interface.md) remains authoritative for public syntax
and exact-empty responsibility removal. The [Behavior Contract](behavior.md)
remains authoritative for technology-neutral request resolution and operation
behavior. This design adds no spelling, result, diagnostic, or domain fact.

The generic parser and typed-request boundaries remain in the [CLI
Architecture](../../../architecture.md). The active [edge-case
ledger](../../../../../../working/cli-development/edge-cases.md) records this
exception as `CLI-EDGE-016`, separate from Route List `CLI-EDGE-005`.

## CLI-EDGE-016 Recognizer

Pinned `System.CommandLine` erases the distinction between a bare zero-token
`--responsibility` option and the two accepted attached-empty forms from its
typed parse result. `CliBindingParse` therefore retains the immutable
`ParseResult` and original argument sequence together at the binding boundary.

Only the Route Update binding may consult that original sequence. It may do so
only after typed parsing proves exactly one selected `--responsibility` option
with zero value tokens. The bounded lexical recognizer then accepts exactly:

```text
--responsibility=
--responsibility:
```

It rejects the bare `--responsibility` form. The ordinary valued forms,
including `--responsibility ""`, retain their Interface-contract meaning and do
not enter this attached-empty recognizer.

Inspection stops at the first `--`. The recognizer does not inspect, parse,
count, select, or diagnose tokens after that terminator. It does not parse a
value, aggregate occurrences, select a command, replace parser diagnostics, or
reinterpret any other option spelling.

Original arguments do not enter `CliInvocation`, the Route Update request, or
domain behavior. All other parser facts remain exclusively parser-owned.

## Conformance Evidence

Focused parser and process evidence must prove both accepted attached-empty
forms, rejection of the bare form, unchanged behavior for ordinary valued forms
including the explicit empty string, the exactly-one-occurrence and zero-token
precondition, the `--` stop boundary, and absence of raw arguments from the
request and domain. Route List evidence separately preserves its exact
equals-only depth grammar under `CLI-EDGE-005`.

## Related Current Sources

- [route update Interface Contract](interface.md)
- [route update Behavior Contract](behavior.md)
- [CLI Architecture](../../../architecture.md)
- [Shared CLI Operation Contract](../../../shared-operation-contract.md)
