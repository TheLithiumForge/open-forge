---
title: Glossary
description: Plain-language definitions of the terms used across Open Forge.
---

# Glossary

## Routing and files

| Term                    | Meaning                                                                                                                                                                                                                                                                                 |
| ----------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Loader**              | `.agents/loader.md`, the file `AGENTS.md` tells the agent to read before starting a task. It defines the core terms, the rules for selecting and loading context, the defined tags, and the root routes.                                                                                |
| **Entrypoint**          | The Markdown file that makes a folder routable, named `_{folder-name}.md`. `index.md`, `_index.md`, `references.md`, and `_references.md` are accepted compatibility names.                                                                                                             |
| **Entry**               | One generated navigation line with a link, description, tags, and, when declared, file patterns. It describes a destination, not its contents.                                                                                                                                          |
| **Route**               | A navigable path exposed through entrypoints and `Entries`.                                                                                                                                                                                                                             |
| **Root route**          | A route the loader exposes directly. The standard root routes are Directives, Guidance, Maps, Memory, Patterns, Skills, and Templates.                                                                                                                                                  |
| **Scope**               | A part of a route that narrows where the following content applies.                                                                                                                                                                                                                     |
| **Slug**                | The concrete folder name in a route path.                                                                                                                                                                                                                                               |
| **Description**         | Short text that helps a reader decide whether to open a file.                                                                                                                                                                                                                           |
| **Responsibility**      | An optional sentence stating what a file defines, so an editor can decide what belongs there. It creates no authority or loading behavior.                                                                                                                                              |
| **`applyTo`**           | Optional frontmatter patterns that narrow a file to tasks that work on matching files. It filters loading and never grants or restricts permission to edit.                                                                                                                             |
| **Working file**        | A file the task investigates, creates, changes, deletes, renames, or reviews, named by its workspace-relative path. It can be a planned file that does not exist yet.                                                                                                                   |
| **Axiom**               | A required rule under `Axioms` in the loader or a recognized loaded entrypoint, inherited by selected descendants.                                                                                                                                                                      |
| **Overwrite companion** | A user-owned `{name}.overwrite.md` file that loads immediately after `{name}.md`. It shares the base file's route, scope, and loading behavior, and isn't indexed or selected separately. Where the two answer the same question differently, the overwrite wins only for that content. |
| **Managed route**       | A route whose declared manager, such as the CLI, may install, update, or remove identified files. Management doesn't create runtime authority.                                                                                                                                          |

## Framework parts

| Term          | Meaning                                                                                                                                                                              |
| ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Framework** | The complete Open Forge operating model installed in a workspace. It has two parts, Core and Memory.                                                                                 |
| **Core**      | Base routing and loading, workspace orientation, and reusable agent-facing roles.                                                                                                    |
| **Memory**    | Self-growing Markdown state for active work, coordination, accepted knowledge, candidates, and history. It grows through routed records without loading the whole store.             |
| **Extension** | An optional package of routes, capabilities, integrations, and support files. Decisions, Checkpoints, and workflow recipes come from Extensions, not from the base.                  |
| **Harness**   | The tool that runs an agent and gives it files and commands, such as Claude Code, Codex, or GitHub Copilot. Open Forge works with any harness that reads `AGENTS.md` or `CLAUDE.md`. |
| **CLI**       | The optional `open-forge` command that finds context, keeps navigation correct, and installs or updates files. The Framework works without it.                                       |

## Content roles

The first six are the Core categories that ship with the base. Workflows come from an Extension.

| Term          | Meaning                                                                                                                                                                                        |
| ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Directive** | Required behavior for a selected scope.                                                                                                                                                        |
| **Guidance**  | Advice for a recurring choice or situation. It can be adapted when the context justifies it.                                                                                                   |
| **Pattern**   | A reusable default shape for code, files, APIs, documents, or other work.                                                                                                                      |
| **Skill**     | A specialized capability exposed through a native `SKILL.md` package.                                                                                                                          |
| **Template**  | Copy-ready source used to start an independently owned artifact. Later changes to the Template don't update copies.                                                                            |
| **Map**       | A coarse link to an important local or external source that says when to use it. It points to the source and doesn't replace it.                                                               |
| **Workflow**  | A repeatable Markdown recipe for reaching a defined goal, with a Goal, Steps, and Completion. It's selected through the `use-workflow` Skill, which comes from the Workflow Support Extension. |

## Memory states

The base ships these four states. A **record** is one saved item in the applicable Memory state.

| Term             | Meaning                                                                            |
| ---------------- | ---------------------------------------------------------------------------------- |
| **Working**      | Temporary state needed to continue or resume active work. It's expected to expire. |
| **Emerging**     | Useful material that isn't accepted yet.                                           |
| **Crystallized** | Accepted knowledge that should remain current.                                     |
| **Archived**     | Useful history that no longer controls current work.                               |

## Memory categories from Extensions

These categories aren't part of the base. Each one arrives with an Extension as an entrypoint under a Memory state. Its entrypoint and records are on demand.

| Term            | Meaning                                                                            | From                                   |
| --------------- | ---------------------------------------------------------------------------------- | -------------------------------------- |
| **Checkpoint**  | Current state, current step, and next steps for one active workstream.             | Planning, in Working                   |
| **Handoff**     | A sealed snapshot of an actual transfer or explicitly planned resumption boundary. | Observations and Handoffs, in Working  |
| **Idea**        | A possibility, experiment, question, or option worth exploring.                    | Planning, in Emerging                  |
| **Analysis**    | Structured reasoning or comparison that remains unsettled.                         | Planning, in Emerging                  |
| **Observation** | A concrete occurrence or pattern noticed in evidence that may matter later.        | Observations and Handoffs, in Emerging |
| **Decision**    | A record of what was chosen and why.                                               | Planning, in Crystallized              |
| **Document**    | A coherent current explanation of an accepted subject.                             | Project Documents, in Crystallized     |

## Loading

| Term                      | Meaning                                                                                                                                                                     |
| ------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Entrypoint at startup** | Read before the task begins. Linked items open when selected, tagged to load, or explicitly required by a loaded rule.                                                      |
| **Entrypoint on demand**  | The entrypoint isn't read until a task opens it, such as the Templates or Archived Memory entrypoint.                                                                       |
| **On demand**             | Not read until a task selects it.                                                                                                                                           |
| **Refresh point**         | A moment when `KeepInMind` content, such as the Emerging Memory entrypoint, is read again: task start or resume, after context restoration, and before handoff or closeout. |

## Tags

Defined tags control loading or classify content, but they don't grant authority. Any other tag is a search and routing signal.

| Tag                              | Meaning                                                                                                                              |
| -------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------ |
| `#LoadNow`                       | Read the linked file, in listed order, when a loaded parent exposes it. If it's an entrypoint, its child loading rules apply.        |
| `#KeepInMind`                    | Read the tagged context when its parent loads, then refresh it at each refresh point while its scope remains active.                 |
| `#Contextual`                    | Useful context, not authority by itself. Treat it as unaccepted unless applicable authority establishes acceptance within its scope. |
| `#CurrentTruth`                  | Accepted current state within its stated scope.                                                                                      |
| `#Evergreen`                     | Material that must stay aligned with accepted current state. It creates no authority or loading behavior.                            |
| `#Core`, `#Memory`, `#Extension` | Which part of the Framework, or which optional package, the content belongs to, as defined above.                                    |
| `#Empty`                         | Marks the `none` placeholder in an entrypoint with no entries.                                                                       |
