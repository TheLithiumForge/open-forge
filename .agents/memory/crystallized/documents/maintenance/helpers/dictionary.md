---
open-forge:
  description: Plain-language dictionary for consistent Open Forge terms
  responsibility: Explain what Open Forge terms mean, when to use them, and which similar terms to distinguish
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Helper, Internal, Dictionary, Terminology]
---

# Open Forge Dictionary

## Purpose

Use this repository-only dictionary when writing or reviewing Open Forge text. It assumes the reader understands basic software concepts such as files, folders, commands, and source code. It does not assume prior knowledge of Open Forge.

The dictionary helps maintainers choose consistent words. It does not replace the Loader or the linked Framework documents. Use those sources when exact behavior matters. Workspaces that install Open Forge do not receive this dictionary.

## Routing And Files

| Term                  | Meaning                                                                             | Use                                                                                        |
| --------------------- | ----------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------ |
| `entrypoint`          | Markdown file that makes its folder routable.                                       | Use when the file's routing behavior matters. Do not call every routed file an entrypoint. |
| `sibling file`        | Another file in the same folder as an entrypoint.                                   | Use this by default for the same-folder relationship.                                      |
| `direct sibling file` | Sibling file listed directly under its entrypoint's `Entries`.                      | Use only when direct indexing or loading matters. Otherwise use `sibling file`.            |
| `child entrypoint`    | Entrypoint in a direct child folder.                                                | Use when the direct parent-child relationship matters.                                     |
| `descendant`          | File or folder below another folder at any depth.                                   | Use when the exact depth or relationship does not matter.                                  |
| `ancestor entrypoint` | Entrypoint above the current route that establishes its path or inherited `Axioms`. | Use when explaining inherited rules or route selection.                                    |
| `route`               | Navigable path exposed through entrypoints and `Entries`.                           | Use for Open Forge navigation, not for every filesystem path.                              |
| `entry`               | One generated route line under `Entries`.                                           | An entry is navigation metadata, not the file it points to.                                |
| `scope`               | Part of a route that narrows where the following content applies.                   | Use for applicability, not ownership or authority.                                         |
| `root route`          | Route exposed directly by the Loader.                                               | A familiar folder name or tag does not create another root route.                          |
| `slug`                | Concrete folder name in a route path.                                               | Use when the path spelling matters. Use `scope` when its narrowing effect matters.         |
| `managed route`       | Route whose declared manager may install, update, or remove identified files.       | Management does not create runtime authority.                                              |
| `overwrite companion` | User-owned `{name}.overwrite.md` file loaded after `{name}.md`.                     | It shares the base file's route and is not indexed separately.                             |

Relative to the Directives entrypoint, `public-facing-writing.md` is a sibling file, `open-forge/_open-forge.md` is a child entrypoint, and `open-forge/framework/deliberate-framework-change.md` is a descendant. Avoid `non-entrypoint` when the exact relationship is known.

The [routing model](../../framework/routing/model.md), [scope rules](../../framework/routing/scope.md), and [overwrite rules](../../framework/routing/overwrites.md) define the complete behavior.

## People, Authority, And Ownership

| Term                                       | Meaning                                                                                                   | Use                                                                                |
| ------------------------------------------ | --------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------- |
| `user`                                     | Person or group that directs, adopts, uses, owns, installs, customizes, or consumes an environment.       | Name the user only when that relationship matters.                                 |
| `agent`                                    | AI system that investigates, coordinates, writes, executes, verifies, or preserves context.               | Orchestrators and subagents are agents.                                            |
| `maintainer`                               | Person or agent changing Open Forge source, distribution, contracts, tooling, tests, or release surfaces. | Use for work on Open Forge itself.                                                 |
| `contributor`                              | Person or agent changing a shared project when product responsibility is not the point.                   | Use when contribution matters more than decision authority.                        |
| `decision-maker`                           | Person responsible for an important choice.                                                               | Use only when that responsibility matters.                                         |
| `responsible person` or `responsible role` | A person's accountability for work or a decision.                                                         | Do not confuse responsibility with semantic authority.                             |
| `delegated authority`                      | Permission to decide or act within a stated boundary.                                                     | State who delegated it and what it covers when that affects the work.              |
| `authoritative source`                     | Source whose answer controls a specific question.                                                         | Use when authority or conflict resolution matters. Otherwise prefer a direct verb. |
| `authoritative document`                   | Current document that defines an accepted subject.                                                        | Use when the source type helps the reader.                                         |
| `authoritative route`                      | Routed content that defines applicable Framework or workspace meaning.                                    | Do not use for a route that only points elsewhere.                                 |
| `authoritative system`                     | External system that contains current code, issue state, product data, or another live subject.           | Use when the external system owns the current answer.                              |
| `ownership`                                | Possession or managed lifecycle.                                                                          | Do not use ownership as a synonym for authority.                                   |
| `operator`                                 | Role explicitly defined by a technical interface.                                                         | Do not use as a generic name for the user or an agent.                             |
| `owner`                                    | Person, package, or system that possesses or manages something.                                           | Otherwise name the authoritative source or responsible role.                       |
| `canonical source`                         | Exact source bytes or preferred authoring form.                                                           | Do not use as a vague synonym for `authoritative source`.                          |

Prefer direct verbs in normal prose:

- `The Architecture document defines the current structure.`
- `The Decision records why the choice was made.`
- `GitHub contains the current issue state.`

Use `authoritative source` when authority itself is the point: `When two files disagree, follow the authoritative source for that question.`

The [typed authority terminology Decision](../../../decisions/framework/authoritative-source-terminology.md) explains the full distinction.

## Core And Its Primitives

| Term           | Meaning                                                                                     | Use                                                                                         |
| -------------- | ------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------- |
| `Framework`    | Complete Open Forge operating model installed in a workspace.                               | Use for the whole system, not one route or primitive.                                       |
| `Core`         | Base routing and loading mechanics, workspace orientation, and reusable agent-facing roles. | Name the exact primitive when its behavior matters.                                         |
| `primitive`    | Reusable Core role that gives selected content a specific purpose.                          | The current primitives are Directives, Guidance, Patterns, Skills, Templates, and Map.      |
| `Axiom`        | Required inherited rule in the Loader or a loaded entrypoint.                               | Only the Loader and recognized entrypoints define active `Axioms`.                          |
| `Instructions` | Required rules in a direct Directive file.                                                  | Use the exact heading when the Directive file contract requires it.                         |
| `Directive`    | Required behavior for a selected scope.                                                     | Use for binding rules, not adaptable advice.                                                |
| `Guidance`     | Advice for a recurring choice or situation.                                                 | It can be adapted when the context justifies it.                                            |
| `Pattern`      | Reusable default shape for code, files, APIs, documents, or other work.                     | Use for inspectable structure, not a procedure.                                             |
| `Skill`        | Specialized capability exposed through a native `SKILL.md` package.                         | Use when a capability performs the work.                                                    |
| `Template`     | Copy-ready source used to start an independently owned artifact.                            | Say `copy and adapt` in ordinary instructions when the lifecycle distinction is not needed. |
| `Map route`    | Coarse link to an important local or external destination.                                  | It points to the source; it does not replace it.                                            |

The [Core primitive model](../../framework/primitives/model.md) defines the complete roles and boundaries.

## Workflow Recipes

| Term       | Meaning                                                 | Use                                                                                                      |
| ---------- | ------------------------------------------------------- | -------------------------------------------------------------------------------------------------------- |
| `Workflow` | Repeatable Markdown recipe for reaching a defined goal. | Use for a repeatable procedure selected through a Skill, not a specialized capability or reusable shape. |

## Memory

| Term            | Meaning                                                                                 | Use                                                                                                                                                                     |
| --------------- | --------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Memory`        | Self-growing Markdown state for continuity, accepted knowledge, candidates, or history. | It can grow through routed records without loading the whole store.                                                                                                     |
| `self-growing`  | Able to expand through useful records and routed scopes as work produces knowledge.     | Retain this descriptor when defining Memory. Explain how people and agents deliberately preserve useful records and add routed scopes without a fixed structural limit. |
| `record`        | One saved item in the applicable Memory state.                                          | Name the specific state or role when it matters.                                                                                                                        |
| `Working`       | Temporary state needed to continue or resume active work.                               | It is expected to expire.                                                                                                                                               |
| `Checkpoint`    | Current state, current step, and next steps for one active workstream.                  | Update it as the work changes.                                                                                                                                          |
| `Handoff`       | Sealed snapshot of an actual transfer or explicitly planned resumption boundary.        | Create it when boundary state must outlive Checkpoint changes. Keep its snapshot unchanged while it serves as a Handoff.                                                |
| `Emerging`      | Useful material that is not accepted yet.                                               | Keep its evidence and uncertainty visible.                                                                                                                              |
| `Analysis`      | Structured reasoning or comparison that remains unsettled.                              | Do not present it as an accepted Decision.                                                                                                                              |
| `Idea`          | Possibility, experiment, question, or option worth exploring.                           | Keep open questions and promotion signals visible.                                                                                                                      |
| `Observation`   | Concrete occurrence or pattern noticed in evidence that may matter later.               | Record its evidence, scope, uncertainty, and matching later occurrences.                                                                                                |
| `Crystallized`  | Accepted knowledge that should remain current.                                          | It is not a quality score or required stage.                                                                                                                            |
| `Decision`      | Record of what was chosen and why.                                                      | It records rationale; the current source defines what is true now.                                                                                                      |
| `Document`      | Coherent current explanation of an accepted subject.                                    | Use when one stable question deserves a complete current view.                                                                                                          |
| `Archived`      | Useful history that no longer controls current work.                                    | Treat it as current only after validation and accepted direction restore it to an explicit current destination.                                                         |
| `#Contextual`   | Useful context that is not accepted current state.                                      | The tag does not make the content authoritative.                                                                                                                        |
| `#CurrentTruth` | Accepted current state within a stated scope.                                           | It is a status, not a universal authority shortcut.                                                                                                                     |
| `#Evergreen`    | Material that must stay aligned with accepted current state.                            | It creates an update duty, not authority or loading behavior.                                                                                                           |

The [Memory model](../../framework/memory/model.md) and [transition rules](../../framework/memory/transitions.md) define complete state and movement behavior.

## Lifecycle And Writing

| Term             | Meaning                                                                                          | Use                                                                              |
| ---------------- | ------------------------------------------------------------------------------------------------ | -------------------------------------------------------------------------------- |
| `capture`        | Save useful material before it is lost.                                                          | Do not save ordinary activity without future value.                              |
| `consolidate`    | Combine related material into one coherent source.                                               | Avoid competing copies of current meaning.                                       |
| `promote`        | Move material into an accepted state or authoritative destination.                               | Use when the authority change matters. Otherwise state the destination directly. |
| `archive`        | Keep useful history outside current authority.                                                   | Use when the history still has value.                                            |
| `prune`          | Remove Working or candidate material that is no longer useful.                                   | Retain useful history according to accepted retention preferences.               |
| `restore`        | Validate historical material and move it into an explicit current destination.                   | Restoration is a new transition, not an automatic return of old authority.       |
| `description`    | Short text that helps a reader decide whether to open a file.                                    | State the purpose, trigger, or useful result.                                    |
| `responsibility` | Optional sentence that helps an editor decide what belongs in a file by stating what it defines. | It does not create authority or loading behavior.                                |
| `standard`       | Default route or configuration provided by Open Forge.                                           | Use ordinary lowercase language when no defined Open Forge concept is meant.     |
| `canonical form` | Preferred authoring syntax when structure carries machine meaning.                               | Compatibility input does not become preferred output.                            |
| `contract`       | Stable behavior, promise, or boundary that can be satisfied or violated.                         | Do not use as a formal-sounding synonym for any rule or document.                |
| `defined term`   | Term with a specific Open Forge meaning.                                                         | Explain or link it at first use, then use it consistently.                       |
| `essence`        | Short statement of identity, mechanism, important distinctions, and boundary.                    | Keep only what changes the reader's understanding of the subject.                |
| `actor`          | Person, agent, system, or role performing an action.                                             | Name the actor only when it adds useful meaning.                                 |
| `link`           | Navigable relationship to related detail.                                                        | A link does not merge authority, scope, loading, responsibility, or lifecycle.   |

The [Writing Standard](../writing.md) defines how repository prose should read. This dictionary helps maintainers apply it and the linked Framework sources.
