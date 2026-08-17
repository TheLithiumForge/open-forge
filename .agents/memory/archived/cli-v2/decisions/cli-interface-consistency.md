---
open-forge:
  description: Historical CLI-v2 source: The replacement CLI groups commands by semantic locality, names every operation explicitly, and keeps flags orthogonal, globally consistent, and predictable
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Interface Consistency

## Context

The replacement CLI should feel intuitive, predictable, helpful, and technically ordinary. The principal failure mode to avoid is a command whose meaning changes according to flags, argument shape, terminal state, or hidden context, as seen in overloaded interfaces where one verb may switch, restore, rename, create, or discard.

Open Forge deliberately treats agents as the primary callers, so predictable grammar also reduces discovery calls, retries, output parsing, and accidental authorization. People need the same predictability during lifecycle operations with larger preservation consequences.

## Decision

Each invocation resolves to one explicit operation, subject, target, and
lifecycle intent. Grouping follows semantic locality rather than one universal
word order, and ordinary command paths remain shallow. Flags configure one
selected operation; they do not hide unrelated operations or change meaning
through terminal state, argument spelling, filesystem coincidence, or another
implicit signal.

Global and repeated flags have one definition and meaning. Workspace and path
selection remain explicit, purpose-specific, and free of ancestor or
filesystem guessing. Required subjects stay visible, while a guided selector
may complete deliberately omitted subjects without weakening deterministic
automation.

Selection, confirmation, replacement, deletion, source trust, executable
configuration, Git policy, and other consequential authority remain distinct.
Human and structured presentations consume the same operation and typed
result; neither creates another implementation path.

The [CLI Interface](../../documents/cli/interface.md) owns exact command paths,
arguments, flag spellings, aliases, applicability, defaults, and public error
behavior. The [request construction](../../documents/cli/contracts/request-construction.md),
[source review](../../documents/cli/contracts/source-review.md), and focused
lifecycle contracts own the complete shared semantics. This Decision preserves
why consistency and separation were selected rather than copying those APIs.

## Rationale

Explicit operation selection makes authorization, help, completion, handlers, tests, plans, and error contracts line up around the same intent. Stable flags keep learned behavior reusable instead of forcing callers to rediscover local meanings.

Exact workspace selection keeps the mutation and inspection target visible. Upward discovery would be convenient in nested directories, but could silently redirect an invocation to a parent workspace; callers instead provide `--workspace` when the current directory is not the intended root.

Semantic grouping keeps related behavior together without making sentence-like word order more important than discovery. It also lets source locality follow the public domain family while each leaf remains independently testable.

Restricting aliases to readable abbreviations prevents mnemonic convenience from becoming another hidden vocabulary. Avoiding inferred argument types keeps scripts and agents stable when paths, ids, or future source forms overlap.

One underlying operation contract prevents a wizard, agent process, continuous integration, and redirected output from developing different behavior. Interaction may differ while request, planning, application, verification, and results remain shared. The guided-selection rule makes the no-subject form consistently discoverable for people while keeping explicit arguments deterministic for agents and scripts.

## Alternatives And Tradeoffs

- Operation-selecting flags can shorten a surface but require exclusive combinations and may make completion or help less discoverable than command words.
- Uniform action-first commands read naturally one invocation at a time but scatter related domain behavior across root-level actions.
- Uniform subject-first commands preserve domain locality but manufacture subject families for complete standalone jobs.
- Flat compound commands keep one parser level but enlarge root help and duplicate family prefixes.
- Allowing command-local flag meanings would make each definition convenient while increasing CLI-wide surprise and collision risk.
- Assigning any available letter would create more aliases but make them harder to predict than the long names they abbreviate.
- Inferring ids, paths, or source kinds from token syntax would shorten some invocations while making edge cases and future extension sources ambiguous.
- Requiring every human to provide complete non-interactive arguments would keep invocation uniform but make installation and other consequential setup less helpful.
- A transparent `plan` prefix would make preview visibly separate, but it would add a meta-command exception and complicate help, completion, and nested parsing. `--dry-run` keeps the selected mutation intact.
- Requiring a creation kind would make role selection explicit, but it would duplicate meaning already established by routed placement and prevent generic routed Markdown creation.

The explicit surface may contain more verbs. Completion and focused help absorb that cost while preserving reliable meaning.

## Consequences

- The command tree is part of the operation contract rather than a collection
  of context-dependent aliases.
- Learned flag and argument behavior transfers across commands.
- Guided use can remain helpful without making people and automation invoke
  different operations.
- Safety-sensitive authority stays reviewable because one shortcut cannot
  silently inherit another decision.
- Exact public behavior changes in the Interface and focused contracts, while
  this record changes only when the underlying design rationale changes.

## Authoritative Sources

- [Open Forge CLI Interface](../../documents/cli/interface.md)
- [CLI command-surface decision](cli-command-surface.md)
- [CLI interface consistency directive](../../../../directives/open-forge/cli/cli-interface-consistency.md)
- [Predictable command-surface pattern](../../../../patterns/open-forge/cli/commands/predictable-command-surface.md)
- [Guided operation pattern](../../../../patterns/open-forge/cli/commands/guided-operation.md)
- [Planned CLI mutation pattern](../../../../patterns/open-forge/cli/filesystem/planned-mutation.md)
- [Reviewed source boundary Pattern](../../../../patterns/open-forge/cli/commands/reviewed-source-boundary.md)
- [External source review contract](../../documents/cli/contracts/source-review.md)
- [Template instantiation boundary Pattern](../../../../patterns/open-forge/cli/commands/template-backed-creation.md)

## Decision Relationships

- [Agent-first CLI product contract](cli-agent-first-product-contract.md)
- [CLI result and display boundary](cli-result-display-boundary.md)
- [CLI core job model](cli-core-job-model.md)
- [CLI command framework](cli-command-framework.md)
- [CLI doctor and repair contract](cli-doctor-repair-contract.md)
- [CLI source locality](cli-source-locality.md)
