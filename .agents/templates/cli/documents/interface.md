---
open-forge:
  description: Start a complete public command surface with explicit inputs, composition, outputs, results, errors, and scenarios
  tags: [Template, CLI, Command, Contract, Interface]
---

# `{command-path}` Interface Contract

{
Template selection:

- Need: One command-local document that completely defines a CLI command's public surface and observable results.
- Primary question: What may a caller enter, what does each input state mean, and what may the caller observe?

Copy this file to `contracts/{command-path}/interface.md`. Replace metadata,
including removing the `Template` tag, so it describes the independent file's
scope, state, and authority. This is a copy-ready starter, not a current command
instance. Copying it creates an independent file; later Template changes do not
update that file. Replace the title, links, link depth, and placeholders. Remove
sections that cannot apply, but preserve the responsibilities stated by this
starter. Repeat rows and subsections for every applicable operand, flag, finite
value, result, and independently testable fact. Do not add permanent requirement
IDs or temporary-state placeholders. Remove this braced source guidance.
}

## Status And Authority

{State lifecycle, authority, implementation availability, deferred details, and
the related shared contracts that keep the same meaning for this command.}

## Purpose

{State the complete public operation, the questions or outcomes it supports,
and the deterministic promise a caller may rely on.}

## Syntax

```text
open-forge {command-path} {operands} {flags}
```

{State whether the command has aliases or child operations. List applicable
global flags by linking to their shared contract.}

## Operands

| Operand     | State                                | Accepted value | Omission                         | Repetition and order                            |
| ----------- | ------------------------------------ | -------------- | -------------------------------- | ----------------------------------------------- |
| `{operand}` | {required, optional, or conditional} | {grammar}      | {exact default or invalid state} | {cardinality, ordering, and duplicate behavior} |

## Flags

| Flag       | Role                                             | Value                      | Omission        | Repetition and composition                             |
| ---------- | ------------------------------------------------ | -------------------------- | --------------- | ------------------------------------------------------ |
| `--{flag}` | {global, selection, projection, or write policy} | {finite values or grammar} | {exact default} | {repeatability, ordering, conflicts, and dependencies} |

### `{flag-or-value}`

{Define the incremental behavior, accepted and invalid boundaries, and public
result for one input or cohesive input dimension.}

## Human Output

{Define stable stream use, ordering, empty and no-op output, and representative
compact and expanded blocks. Compact output is token-friendly but retains
identity, hierarchy or order, status, completeness, safety, and required next
actions. Expanded is the default and adds explanation, evidence, provenance, and
locations. Keep diagnostics under `--verbose`, not human view selection.}

## Structured Output

{Define the complete public information returned, its relationship to both human
views, the no-op effect of `--view` under JSON, and every exact schema detail
intentionally deferred.}

## Semantic Results

| Result     | Meaning                | Process completion status                                |
| ---------- | ---------------------- | -------------------------------------------------------- |
| `{result}` | {observable condition} | {accepted mapping or explicitly deferred numeric status} |

## Errors

{Define the public boundaries for invalid, blocked, failed, interrupted,
attention, and incomplete states. State a useful correction when one exists.
Keep empty and no-op outcomes with non-error results.}

## Scenarios

### `{Omission Or Finite Value}`

```text
open-forge {complete invocation}
```

{Show the representative result or output. Add combination scenarios only for
non-additive composition, dependency, precedence, conflict, or order behavior.}

## Non-Goals

{State excluded operations or interpretations that would otherwise remain
plausible.}

## Verification

{List applicable `Unit`, `Integration`, `EndToEnd`, and `PackageEndToEnd` evidence
for syntax, every input state and finite value, composition, human and structured
output, semantic results, errors, and process-level behavior. Cite exact
Interface facts or sections.}

## Related Sources

- {Behavior Contract in the destination}
- {Shared contracts and accepted product direction in the destination}
