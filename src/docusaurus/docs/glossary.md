---
title: Glossary
description: Plain-language definitions of the terms used across Open Forge.
---

# Glossary

## Routing and files

| Term                    | Meaning                                                                                                                              |
| ----------------------- | ------------------------------------------------------------------------------------------------------------------------------------ |
| **Loader**              | `.agents/loader.md`. The first Open Forge file an agent reads. It defines the terms, workspace rules, tag meanings, and root routes. |
| **Entrypoint**          | The Markdown file that makes a folder routable, named `_{folder-name}.md`.                                                           |
| **Entry**               | One line under an entrypoint's `Entries`, linking to a child with its description and tags. Navigation, not content.                 |
| **Route**               | A navigable path exposed through entrypoints and `Entries`.                                                                          |
| **Root route**          | A route the loader exposes directly: Directives, Guidance, Maps, Memory, Patterns, Skills, Templates.                                |
| **Scope**               | A part of a route that narrows where the content after it applies.                                                                   |
| **Slug**                | The concrete folder name in a route path.                                                                                            |
| **Axiom**               | A required rule in the loader or an entrypoint, inherited by selected descendants.                                                   |
| **Overwrite companion** | A user-owned `{name}.overwrite.md` next to its base file. It loads right after the base and wins for the content they both answer.   |
| **Managed route**       | A route whose declared manager, such as the CLI, may install, update, or remove identified files. Management creates no authority.   |

## Framework parts

| Term          | Meaning                                                                                   |
| ------------- | ----------------------------------------------------------------------------------------- |
| **Framework** | The complete set of Open Forge files installed in a workspace.                            |
| **Core**      | Routing and loading, workspace orientation, and the reusable content roles.               |
| **Memory**    | Self-growing Markdown state for active work, candidates, accepted knowledge, and history. |
| **Extension** | An optional package of more files.                                                        |
| **CLI**       | The optional `open-forge` command that automates maintenance.                             |

## Content roles

| Term          | Meaning                                                                                           |
| ------------- | ------------------------------------------------------------------------------------------------- |
| **Directive** | Required behavior for a selected scope.                                                           |
| **Guidance**  | Advice for a recurring situation, adaptable when context justifies it.                            |
| **Pattern**   | A reusable default shape for code, files, APIs, or documents.                                     |
| **Skill**     | A specialized capability in a native `SKILL.md` package.                                          |
| **Template**  | A copy-ready starting file. Copies are maintained independently.                                  |
| **Map**       | A pointer to an important local or external source, and when to use it.                           |
| **Workflow**  | A repeatable recipe with a Goal, Steps, and Completion, reached through the `use-workflow` Skill. |

## Memory

| Term             | Meaning                                                                |
| ---------------- | ---------------------------------------------------------------------- |
| **Working**      | Temporary state to continue or resume active work. Expected to expire. |
| **Emerging**     | Useful material that isn't accepted yet.                               |
| **Crystallized** | Accepted knowledge that should stay current.                           |
| **Archived**     | Useful history that no longer governs current work.                    |
| **Checkpoint**   | Live state, current step, and next steps for one active workstream.    |
| **Handoff**      | A sealed snapshot for an actual transfer or planned resumption.        |
| **Idea**         | A possibility worth exploring later.                                   |
| **Analysis**     | Structured reasoning that's still unsettled.                           |
| **Observation**  | A concrete occurrence noticed in evidence that may matter later.       |
| **Decision**     | A record of what was chosen and why.                                   |
| **Document**     | A coherent current explanation of an accepted subject.                 |

## Tags

| Tag                              | Meaning                                                        |
| -------------------------------- | -------------------------------------------------------------- |
| `#LoadNow`                       | Read when its loaded parent exposes it.                        |
| `#KeepInMind`                    | Read when its parent loads, then refresh at defined points.    |
| `#Contextual`                    | Useful, but not accepted.                                      |
| `#CurrentTruth`                  | Accepted current state within its scope.                       |
| `#Evergreen`                     | Must be kept aligned with current state.                       |
| `#Core`, `#Memory`, `#Extension` | Which part of the Framework or package the content belongs to. |
| `#Empty`                         | Marks the `none` placeholder in an entrypoint with no entries. |
