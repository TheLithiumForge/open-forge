---
open-forge:
  description: Promote one neutral routed authored-metadata fact for Open Forge Markdown and native Skills
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Framework, Sources, Metadata, ReadOnly, Index]
---

# Establish The Routed Authored-Metadata Foundation

## Task State

- State: Complete and squash-integrated at
  `5924698a3d79156b403348743f31453c03fe48a4`. The accepted feature candidate
  was `8a2932151518f6355d630be9ba9a40236a56102a`; the integrated tree preserves
  its exact production and evidence changes.
- Trigger: the Generated Navigation foundation proved a second consumer of the
  native `SKILL.md` metadata semantics currently trapped in Route-local parsing.
  Neutral source classification remains owned by the accepted
  `SourceDocumentForm` and `SourceFormClassifier`.
- Parent: [Implement Index](index-command.md), as a prerequisite correction for
  [Generated Navigation](index-generated-navigation-foundation.md).
- Responsible role: bounded routed-metadata Task Mastermind.
- Profile: Assured shared foundation because Route and Generated Navigation must
  consume the same source meaning without duplicate parsers or classifications.

## Outcome

Promote one neutral Framework fact and parser that accepts an already parsed
Markdown document plus its accepted `SourceDocumentForm`, distinguishes Open
Forge Markdown/entrypoint metadata from native `SKILL.md` metadata, and returns
one immutable metadata state, description, and authored-tag projection. Native
Skills retain their required `name` and `description` and carry zero authored
Open Forge tags. Callers retain the authoritative `SourceDocumentForm` and use
`SourceDocumentForm.Skill` when their policy needs the accepted native
classification or synthetic `#Skill` generated-entry tag. Loader metadata
remains not applicable.

Migrate Route's metadata reader to that neutral fact without changing any Route
request, result, finding, rendering, help, exit, traversal, compatibility-entry,
or overwrite behavior. Preserve the existing Open Forge metadata parser surface
for Context and other current consumers unless a direct move is required by the
accepted design.

## Dependency And Consumer Map

Inputs are the accepted `MarkdownDocumentFacts`, `SourceDocumentForm`, YAML
syntax facts, and Open Forge metadata grammar. The neutral parser may compose the
accepted `SourceOpenForgeMetadataParser`; it must not depend on Commands, Shell,
root composition, JSON models, process writers, or Generated Navigation.

Immediate consumers are Route's command-local metadata adapter and the paused
Generated Navigation foundation. Later Index, Doctor, Repair, and Cleanup may
consume the same fact only where their authored-metadata meaning is identical.
Find's occurrence-preserving frontmatter reader has different location and
partial-metadata semantics and is not migrated by this Task.

## Ownership

Owned production paths:

- new or directly refined files under
  `src/cli/core/OpenForge.Cli.Core/Framework/Sources/Metadata/**` and
  `Framework/Sources/Models/Metadata/**`;
- the Route-local adapter
  `Commands/Route/Shared/Source/RouteMetadataParser.cs`;
- only if required to consume the promoted fact without duplication,
  `Commands/Route/Shared/Models/Source/RouteSourceMetadata.cs` and
  `Commands/Route/Shared/Source/RouteSourceProjector.cs`.

Owned evidence paths are focused mirrored Unit tests for Framework Sources
metadata and Route metadata plus the smallest real-filesystem Integration
regression needed to prove routed native Skills and Open Forge documents retain
their accepted meaning.

Protected paths are Generated Navigation's uncommitted feature lane, all public
command contracts and presentations, Context, Find, Extension, Lifecycle,
Mutation, Recovery, Git, Shell serialization and root composition, project and
package files, configuration, and generated Markdown. Do not migrate
`FindFrontmatterReader` or remove its occurrence/location semantics.

## Frozen Semantics

| Authoritative `SourceDocumentForm`           | Metadata interpretation           | Complete requirement                                                             | Authored tags       |
| -------------------------------------------- | --------------------------------- | -------------------------------------------------------------------------------- | ------------------- |
| `Skill`                                      | native Skill                      | non-blank `name` and `description`                                               | exactly zero        |
| `Markdown` and all accepted entrypoint forms | Open Forge                        | complete accepted `open-forge.description` and non-empty valid `open-forge.tags` | exact accepted tags |
| `Loader`                                     | not applicable                    | none                                                                             | exactly zero        |
| `OverwriteCompanion`                         | invalid logical-base parser input | fail fast; caller must parse the logical base instead                            | none                |

Missing frontmatter or required values is `missing`; invalid frontmatter, YAML,
shape, or invalid Open Forge tags is `malformed`. The accepted Open Forge parser
continues to reject aliases and unsupported mappings. Before replacing native
Skill parsing, characterize its current Route behavior for aliases, unmatched
members, and non-scalar required values; preserve that behavior exactly or stop
for a project decision rather than silently making the grammar stricter or
looser. The promoted native parser must remain deterministic and AOT-safe.

The Framework fact owns authored metadata only. `SourceDocumentForm` remains the
single neutral source-classification authority. Route continues to derive its
command-local kind from that form and owns compatibility-entry and overwrite
flags. Generated Navigation continues to own canonical generated-line policy,
including derivation of its synthetic classification tag from the form. No
neutral fact grants filesystem effect authority.

## C# And .NET Design Boundary

Use the scoped modern C# design rules explicitly: keep Framework free of Shell
serialization types; use the existing source-generated/YAML-syntax machinery
without reflection; place property-only facts in the nearest `Models/` scope;
prefer `required init` object initialization for genuine data shapes and a named
constructor or factory only where it establishes an invariant. Prefer one
cohesive fact parameter over repeatedly forwarding several members, keep normal
callable arity within the accepted one-to-three preference, use named arguments
for any remaining ambiguous constructor call, and do not add trusted-flow null
guards or postfix suppression. No renderer or string assembly belongs in this
foundation.

## Evidence Ladder

1. Unit evidence freezes every form/state transition, fail-fast
   `OverwriteCompanion` admission, native Skill name and
   description requirements, zero authored Skill tags, source-specific alias and
   unsupported-shape behavior, Open Forge tag grammar, and immutable
   deterministic facts. Native Skill characterization runs against the current
   Route parser before its semantics move.
2. Route regression evidence proves its existing metadata states, compatibility
   flags, overwrite flags, native classification, and public downstream behavior
   are unchanged.
3. Run affected Source, Markdown, YAML, Route, Context, and Find regressions,
   followed by the full managed Unit and Integration suites.
4. Run locked restore when needed, warning-free Release build, format
   verification, `git diff --check`, dependency/path/protected-surface audits,
   and the supported local `linux-x64` Native AOT Integration executable. If the
   exact locked assets cannot be materialized locally, acceptance is blocked;
   the AOT execution gate is not waived.
5. Obtain one independent correctness/architecture review before candidate
   acceptance. Generated Navigation must still rebase, consume the promoted
   fact, rerun its own evidence, and receive its own acceptance review.

## Review And Correction Budgets

- `RM-R1`: one independent correctness, dependency-direction, AOT, and
  behavior-preservation review.
- `RM-C1`: one grouped correction cycle for accepted material findings.

## Acceptance Record

The accepted capability adds one two-argument
`SourceAuthoredMetadataParser.Parse(MarkdownDocumentFacts, SourceDocumentForm)`
and one immutable fact containing only metadata state, description, and authored
tags. `SourceDocumentForm` remains the sole classifier. Route consumes the fact
without changing its compatibility/overwrite flags or form-derived kind; Find,
Context, Shell serialization, Generated Navigation, projects, packages,
configuration, and public contracts remain unchanged.

The final grammar preserves prior scalar aliases, rejects forward or non-scalar
aliases, duplicate keys, unmatched members, and non-scalar required values;
classifies empty or plain-null required values as missing; preserves quoted and
explicit-string null spellings; makes Loader not applicable; and fails fast for
overwrite companions and undefined forms. `RM-R1` found one raw-scalar null
compatibility defect. Grouped `RM-C1` corrected it, and the focused Sol/xhigh
recheck returned `ACCEPTED`.

Final evidence is focused Unit `89/89`, real-filesystem Route Integration `5/5`,
affected Unit `860/860`, affected Integration `237/237`, full managed Unit
`1012/1012`, full managed Integration `354/354`, and local locked `linux-x64`
Native AOT Integration `354/354`, all with zero failures/skips. Release is
warning-clean; format, diff, dependency, exact/protected-path, Framework
dependency, and doctor audits pass. Doctor reports only the unrelated existing
C# directive warning. No push or remote operation occurred.

## Stop Conditions

Stop on a new package or reflection requirement, Shell/YAML-context dependency
from Framework, a public behavior change, a second metadata parser or
classification model, loss of Find's location-aware semantics, weakened Open
Forge validation, Generated Navigation edits in this lane, filesystem effects,
or any required change outside the owned paths.
