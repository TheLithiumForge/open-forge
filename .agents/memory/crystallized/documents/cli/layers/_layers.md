---
open-forge:
  description: The four architectural layers of the replacement CLI, each with the one question it answers and the record that defines its inner workings
  responsibility: Route to the per-layer architecture records and state the order a request passes through them
  tags: [Memory, Crystallized, Document, CurrentTruth, Evergreen, CLI, Architecture, Layers]
---

# CLI Layers

Read at two resolutions. **Three bands** are the order a request moves through,
and the shape to hold in your head:

```text
Shell            arguments -> a request
  Processing     Framework facts -> command meaning -> one result
Presentation     a result -> selected report -> text or JSON
```

**Four layers** are the dependency rule, because `Processing` contains two
things that must not reference each other in both directions. Framework holds
facts and knows no command; Operations holds command meaning and consumes those
facts. That one-way edge is 2,541 references in the tree with **zero** coming
back, and naming Framework separately is what makes it checkable.

The [land Architecture](../architecture.md) holds the invariants that cross all
of them; these records hold the inner workings of each.

| Layer                           | The question it answers                                                 |
| ------------------------------- | ----------------------------------------------------------------------- |
| [Shell](shell.md)               | What was asked, and how does the answer leave the process?              |
| [Framework](framework.md)       | What is true about this workspace, and what may change it?              |
| [Operations](operations.md)     | What does this command mean?                                            |
| [Presentation](presentation.md) | Of everything found, what does this reader need, and how is it written? |

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Boundary Invariants

Four rules, all currently true, all mechanically checkable from the `using`
graph:

| Rule                                                                                               | Why it matters                                      |
| -------------------------------------------------------------------------------------------------- | --------------------------------------------------- |
| Framework never references Commands, Shell, or Presentation                                        | Framework stays independently testable and reusable |
| Shell never references a concrete command                                                          | The pipeline stays generic over command payloads    |
| Presentation never imports any `Framework.*` namespace                                             | Rendering cannot reach around command-owned facts  |
| `Presentation/<Owner>/` may import only that owner's `Commands.<Owner>.Models.*`                  | Each selector consumes its own projected data     |
| No file under `Commands/` or `Framework/` references `Presentation/`                              | Data and behavior do not depend on rendering      |
| Commands reference Framework and Shell, never another command's private `Shared`                  | Command meaning stays local                         |

A shared owner extracted during deduplication goes at the narrowest scope that
covers **every** consumer, measured against this graph rather than the folder
tree. Placing one a layer too low inverts a dependency silently: a wire
vocabulary consumed by Shell cannot live under `Commands/`, however many
commands also use it.

Cycles _inside_ a layer do not break these rules and are ranked below them. They
still cost independent testability, and the six that exist are recorded in the
[land Architecture](../architecture.md#measured-direction-inside-framework).

## The Order A Request Passes Through

```text
arguments
  -> Shell          parse, select the binding, select the workspace, form a request
  -> Operations     select what this request needs
  -> Framework      Documents parse bytes; Sources and Routing build identity and
                    the graph; State reads lifecycle, permissions, and inventory
  -> Operations     plan, apply if the command writes, form one result
  -> Presentation   select what to show, then render text or JSON
  -> Shell          write the output, return one exit code
```

Shell appears at both ends because it _is_ the boundary. Framework appears once
but is entered repeatedly, because facts are read as an operation needs them.

Two things this order makes visible that a flat list of names does not:

- **Framework is never entered directly from Shell.** A request reaches facts
  only through a command operation. This is why Shell holds no Framework-domain
  behaviour.
- **Operations is entered twice, around Framework.** Selection precedes reading;
  planning and result formation follow it. Treating the command as one
  undifferentiated step is what lets reading, planning and rendering re-derive
  the same fact three times and disagree. Presentation then applies one
  report-selection stage before either renderer writes the result.

## Naming

Two words in this vocabulary are easy to misread, and both have bitten:

- **Routing** is the workspace's document route graph — the Loader, route
  chains, entrypoints, generated Entries. Choosing which command to run is
  **binding selection**, and it belongs to Shell. Never use "routing" for
  dispatch in this codebase.
- **Selection** appears in two layers and means the same kind of thing in both:
  narrowing. Operations selects _which sources_ a request needs; Presentation
  selects _which facts_ a reader sees through `CliSelection`, the
  `CliReportSelector`, and the shared trimmer. Qualify it when the layer is not
  obvious from context.

## Cross-Command Presentation Guarantees

`CliReportInvariantsTests` enforces these guarantees over all native snapshots
and synthetic reports:

1. Text line counts grow monotonically from `minimal` through `standard` to
   `full` for each situation.
2. `debug` stdout equals `full` stdout; diagnostics are stderr-only.
3. Errors precede warnings, which precede infos in text and JSON findings.
4. Text and JSON at the same level list the same finding and effect identities in
   the same order.
5. Human primary text has no `Status:` or `Selected by:` line, no bare
   internal omission label, and no retired planning heading.
6. Text contains no JSON escaping outside the escaper's own visible sequences.
7. Every text snapshot is valid UTF-8 with ASCII-only framing; non-ASCII content
   is confined to authored spans and descriptions.
8. Every listed finding has a path or an identifier.
9. There is at most one `Next:` line, and the current report invariant places it
   last; the Extension Install continuation is the recorded exception.
10. A JSON member omitted at a detail level is not emitted as an empty array.
11. Count values are numbers or null; null values have a limitation except for
    the accepted finite nullable-count allow-list.
12. Every JSON finding code is checked against its command catalogue; a missing
    row is recorded as a review gap.
13. No file under `Commands/` or `Framework/` references `Presentation/`.

These checks are guarantees of the current implementation and evidence gate.
The `--detail-filter` name, the shared renderer's repeated action and reason
lines, the Extension Install continuation after `Next:`, the nullable-count
allow-list, whether diagnostics belong in the human-primary vocabulary check,
the missing `extension-list.installed-source-missing` catalogue row, and the
explicit-workspace echo in minimal examples remain maintainer questions. Their
observed behavior is preserved until each question is decided.

## Entries

- [The Framework layer - reusable facts about the workspace and the explicit effect boundaries that change it](framework.md) - #Memory #Crystallized #Document #CurrentTruth #Evergreen #CLI #Architecture #Framework #Sources #Routing #Documents #Lifecycle
- [The Operations layer - where a command turns a request into one result, through selection, planning, application, and result formation](operations.md) - #Memory #Crystallized #Document #CurrentTruth #Evergreen #CLI #Architecture #Operations #Commands #Planning
- [The Presentation layer - the complete report, report-selection stage, and renderers that turn selected facts into text or JSON](presentation.md) - #Memory #Crystallized #Document #CurrentTruth #Evergreen #CLI #Architecture #Presentation #Selection #Rendering
- [The Shell layer - the process boundary that turns arguments into a request and a result into an exit code](shell.md) - #Memory #Crystallized #Document #CurrentTruth #Evergreen #CLI #Architecture #Shell #Parsing #Pipeline
