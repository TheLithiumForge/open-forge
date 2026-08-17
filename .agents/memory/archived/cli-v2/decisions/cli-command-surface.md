---
open-forge:
  description: Historical CLI-v2 source: The replacement CLI uses nineteen focused leaves, destination-first creation, dry-run preview, one direct whole-Framework install command, and minimal Extension and completion lifecycles
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Command Surface

## Context

The replacement needed one public vocabulary after its product role, core
jobs, interface principles, doctor boundary, and command framework were
accepted. Several individually reasonable grammars created avoidable
complexity: universal action-first commands scattered domains, universal
subject-first commands manufactured ceremonial groups, operation flags hid
leaf commands, and Framework or Extension catalogues exposed distinctions that
did not correspond to useful jobs.

The selected surface needed to remain memorable, explicit enough for agent
authorization, helpful during guided use, and ordinary enough for help and
completion to describe without special cases.

## Decision

The replacement CLI has nineteen public leaf commands:

```text
status
context
find
doctor
repair
create
install

route list
route inspect
route init
route rebuild

extension list
extension inspect
extension add
extension update
extension remove

completion install
completion remove
completion script
```

The [CLI Interface](../../documents/cli/interface.md) is authoritative for exact
arguments, flags, defaults, interaction states, public results, and operation
behavior. Its focused contracts own shared request, lifecycle, recovery,
formatting, source-review, completion, and mutation semantics.

The chosen inventory mixes complete direct jobs with one-level subject
families. `status`, `context`, `find`, `doctor`, `repair`, `create`, and
`install` remain direct because each names one complete job. Route, Extension,
and Completion remain families because each has several operations sharing one
domain, lifecycle, help surface, and vocabulary.

Preview stays attached to a selected mutation through `--dry-run`; there is no
`plan` meta-command. Generic creation remains destination-first, with explicit
optional Template selection rather than typed creation subcommands or implicit
Template matching. The Framework installs as one coherent payload through
direct `install`, while Extensions and Completion retain only their useful
distinct lifecycle leaves.

## Rationale

The inventory maps directly to the five accepted CLI jobs:

| Job                  | Principal commands                                      |
| -------------------- | ------------------------------------------------------- |
| Orient               | `status`, `route list`, `extension list`                |
| Retrieve and explain | `context`, `find`, `route inspect`, `extension inspect` |
| Create and maintain  | `create`, `route init`, `route rebuild`                 |
| Validate and repair  | `doctor`, `repair`                                      |
| Manage lifecycle     | `install`, Extension mutations, Completion mutations    |

This mapping makes capability gaps visible without turning supporting concerns such as planning, output, and verification into standalone product jobs.

Destination-first creation keeps the CLI broad without requiring a versioned
catalogue of every Framework and repository-local role. Explicit Template
selection still makes specialized canonical bodies cheap.

One direct Framework mutation matches the actual source model: the complete
Framework is reconciled against one embedded payload, while exclusions and
overwrite authority remain explicit. A family, item selection, or several
lifecycle verbs would create vocabulary without creating a distinct user job.

The smaller Extensions family is honest about the current design. It retains
operations agents and users need without manufacturing package-authoring or
ownership commands.

Explicit completion lifecycle is slightly larger than one script command, but it makes external shell-profile mutation visible, previewable, and safely removable.

## Alternatives And Tradeoffs

- Flat compound names such as `route-list` keep one parser level but scatter domain help and source locality.
- Operation flags such as `route --list` shorten the tree but create hidden modes and invalid combinations.
- Action-first paths such as `list routes` read naturally but separate one domain across root-level verbs.
- A root `help` command could host Framework topics, but it would mix CLI discovery with a private ontology. Routed sources and command help remain the authorities.
- `plan repair` makes preview sound like an operation but requires a transparent command wrapper around the entire mutation grammar.
- Automatic Template selection reduces typing but makes installation of a new Template capable of changing an existing deterministic command.
- `route create <route>` keeps a domain identity as input but cannot honestly initialize missing directory chains without converting that identity into hidden filesystem behavior.
- A Framework family suggests several useful operations or selectable packages,
  but the actual job is one whole-payload installation and reconciliation.
- Separate Framework lifecycle verbs can sound precise, but their effects
  overlap when there is only one embedded source and one ownership model.
- A generic `sync` is shorter but conceals source authority and preservation direction.

The accepted surface favors a few explicit arguments and flags over hidden inference. Completion absorbs the typing cost.

## Consequences

- Command definitions, handlers, direct tests, help, completion, and operation
  ids align around nineteen leaves.
- The replacement implements this surface directly; the frozen MVP neither
  translates nor dispatches replacement invocations.
- Preview, guided selection, confirmation, and lifecycle authority remain
  policies around one explicitly selected leaf rather than alternate commands.
- Creation remains generic and destination-first, Framework installation
  remains whole-payload, and optional package or shell behavior stays in its
  own lifecycle family.
- Exact command and flag behavior can evolve only through the authoritative
  Interface and its focused contracts, not by expanding this rationale record.

## Authoritative Sources

- [Open Forge CLI Interface](../../documents/cli/interface.md)
- [Open Forge CLI Architecture](../../documents/cli/architecture.md)
- [CLI interface consistency directive](../../../../directives/open-forge/cli/cli-interface-consistency.md)
- [Predictable command-surface Pattern](../../../../patterns/open-forge/cli/commands/predictable-command-surface.md)
- [Planned CLI mutation Pattern](../../../../patterns/open-forge/cli/filesystem/planned-mutation.md)
- [Template instantiation boundary Pattern](../../../../patterns/open-forge/cli/commands/template-backed-creation.md)

## Decision Relationships

- [CLI interface consistency](cli-interface-consistency.md)
- [CLI agent-first product contract](cli-agent-first-product-contract.md)
- [CLI core job model](cli-core-job-model.md)
- [CLI doctor and repair contract](cli-doctor-repair-contract.md)
- [CLI result and display boundary](cli-result-display-boundary.md)
- [CLI command framework](cli-command-framework.md)
