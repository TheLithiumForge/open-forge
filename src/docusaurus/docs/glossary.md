---
title: Glossary
description: Plain definitions and examples of Open Forge terms.
---

# Glossary

Look up a term used in a page, a workspace file, or a CLI result. Each definition includes an example.

## Routing and files

### Loader

`.agents/loader.md` tells the agent how to select and load context. `AGENTS.md` asks the agent to read it before starting a task. It defines the core terms, rules, tags, and root routes.

Example: The loader exposes Directives and Memory.

### Entrypoint

The Markdown index that makes a folder routable. Its usual name is `_{folder-name}.md`. `index.md`, `_index.md`, `references.md`, and `_references.md` are compatibility names.

Example: `directives/frontend/_frontend.md` lists frontend rules.

### Entry

One generated navigation line with a link, description, tags, and any declared file patterns. It describes a destination rather than containing its contents.

Example: `- [C# rules](csharp.md) - #Directive #LoadNow`

### Route

A navigable path exposed through entrypoints and `Entries`.

Example: `directives/frontend/components` reaches a component rule through the frontend scope.

### Root route

A route exposed directly by the loader. The standard root routes are Directives, Guidance, Maps, Memory, Patterns, Skills, and Templates.

Example: `directives` is a root route when the loader lists it.

### Include

To list a document that already exists in the project in the loader, where it lives, so agents use it without moving it or adding frontmatter.

Example: `open-forge route include CONTRIBUTING.md --description "Contribution rules" --tag LoadNow`.

### Scope

A part of a route that narrows where the following content applies.

Example: `frontend` in `directives/frontend/components` limits the rule to frontend work.

### Slug

The concrete folder name in a route path.

Example: `frontend` is the folder spelling in that route.

### Frontmatter

YAML metadata between `---` lines at the start of a Markdown file.

Example: `description: C# rules` and `tags: [Directive, LoadNow]`

### Description

Short text that helps a reader decide whether to open a file.

Example: `Run tests before finishing a code change`

### Responsibility

An optional sentence stating what a file defines, so an editor can decide what belongs there. It creates no authority or loading behavior.

Example: `Define the current CSV export limits`

### `applyTo`

Optional frontmatter patterns that limit a source to work on matching files. They filter loading and applicability, without granting or restricting edit permission.

Example: `applyTo: ["**/*.cs"]` limits a rule to C# work.

### Working file

A file the task investigates, creates, changes, deletes, renames, or reviews, identified by its workspace-relative path. Planned files count too. Reading a context source alone does not make it a working file.

Example: `src/Order.cs` counts when the task plans to create it.

### Axiom

A required rule under `Axioms` in the loader or a recognized loaded entrypoint. Selected descendants inherit it.

Example: A scoped entrypoint requires changes to follow the project's API conventions.

### Overwrite companion

A user-owned `{name}.overwrite.md` file read immediately after its base. It shares the base's route, scope, and loading behavior and is not indexed or selected separately. Where both answer the same question differently, the overwrite wins for that content only.

Example: `_memory.overwrite.md` adds a local recording rule to `_memory.md`.

### Managed route

A route whose declared manager may install, update, or remove identified files. Management creates no runtime authority.

Example: The CLI updates an installed category entrypoint it manages.

### Source reference

A source ID, an exact `.agents` Markdown path, or `./<path>` for an included file outside `.agents/`, that selects content in commands. Extension and Library IDs identify different things.

Example: `skills/open-forge-cli` selects `.agents/skills/open-forge-cli/SKILL.md`.

## Framework parts

### ACE

Adaptive Context Engineering combines explicit goals and expected results with context that agents load as the task needs it.

Example: An agent opens relevant architecture and local rules for a change while unrelated history stays closed.

### Framework

The complete Open Forge operating model installed in a workspace. It has two parts, Core and Memory.

Example: A setup combines the loader, Core routes, and Memory states.

### Core

Base routing and loading, workspace orientation, and reusable agent-facing roles.

Example: Directives require behavior and Patterns show shapes.

### Memory

Self-growing Markdown state for active work, coordination, accepted knowledge, candidates, and history. People and agents deliberately add useful records and routed scopes without loading the whole store.

Example: A project adds a payments scope as useful payments knowledge grows.

### Extension

An optional package of routes, capabilities, integrations, and support files. Record categories and workflow recipes come from Extensions or from you.

Example: Planning adds Decisions and a planning recipe.

### Harness

The tool that runs an agent and gives it files and commands. Open Forge works with a harness that reads `AGENTS.md` or `CLAUDE.md`.

Example: Claude Code, Codex, or GitHub Copilot

### CLI

The optional `open-forge` command that finds context, maintains navigation, and installs or updates files. The Framework works without it.

Example: `open-forge index --dry-run` previews missing metadata, navigation files and Entries changes.

## Content roles

The first six roles are Core categories. Workflows are recipes supplied by Extensions or by you.

### Directive

Required behavior for a selected scope.

Example: Run the project's tests before calling a code change done.

### Guidance

Advice for a recurring choice or situation. It can be adapted when the context justifies it.

Example: Advice on when to split a module

### Pattern

A reusable default shape for code, files, APIs, documents, or other work.

Example: An API response with `data` and `error` fields

### Skill

A specialized capability exposed through a native `SKILL.md` package. The agent runtime controls its use.

Example: `open-forge-cli/SKILL.md` teaches use of the CLI.

### Template

Copy-ready source used to start an independently maintained artifact. Later Template updates do not update copies.

Example: Copy `task.md`, replace its prompts, and maintain the new Task.

### Map

A coarse link to an important local or external source that says when to use it. It points to the source and does not replace it.

Example: A Map points to the architecture document for component boundaries.

### Workflow

A repeatable Markdown recipe for reaching a defined goal, with `Goal`, `Steps`, and `Completion`. The `use-workflow` Skill from Workflow Support selects installed recipes.

Example: The debugging recipe moves from reproducing a symptom to checking its cause.

### Instructions

Required rules in a selected Directive file.

Example: `## Instructions` contains the obligation to run the project's tests.

## Memory states

These are independent places to keep material, not required stages. A record can start in whichever state fits.

### Record

One saved item in the applicable Memory state.

Example: `memory/crystallized/export-limit.md` records an accepted limit.

### Working

Temporary state needed to continue or resume active work. It is expected to expire.

Example: A note names the current step and next check.

### Emerging

Useful material that is not accepted yet.

Example: An investigation records a possible cause and its uncertainty.

### Crystallized

Accepted knowledge that should remain current within its scope.

Example: An accepted CSV export limit goes straight to Crystallized.

### Archived

Useful history that no longer controls current work. Validation and accepted direction are needed before restoring it to a current destination.

Example: A replaced design is retained with a link to its replacement.

## Memory categories from Extensions

These categories are optional. Each arrives as an on-demand entrypoint under a Memory state. Records can also live directly under a state.

### Checkpoint

Current state, current step, and next steps for one active workstream. Planning supplies this category in Working.

Example: A live checkpoint names the check needed to resume export work.

### Handoff

A sealed snapshot of an actual transfer or explicitly planned resumption boundary. Observations and Handoffs supplies it in Working.

Example: A fixed snapshot gives the next agent the transfer state and next action.

### Idea

A possibility, experiment, question, or option worth exploring. Planning supplies it in Emerging.

Example: Try streaming exports to reduce memory use.

### Analysis

Structured reasoning or comparison that remains unsettled. Planning supplies it in Emerging.

Example: Compare two export designs and record assumptions.

### Observation

A concrete occurrence or pattern noticed in evidence that may matter later. Observations and Handoffs supplies it in Emerging.

Example: One export run exceeded the expected memory use.

### Decision

A record of what was chosen and why. Planning supplies it in Crystallized. The current source defines what is true now.

Example: A Decision explains why amounts use whole cents and links to the current architecture.

### Document

A coherent current explanation of an accepted subject. Project Documents supplies it in Crystallized.

Example: The architecture document defines the current component boundaries.

## Work and experiences

### Task

A record keeping an outcome, plan, and current state for work together.

Example: An export Task records the result, steps, and next action.

### Plan

The sequence and checks for reaching an outcome.

Example: A separate Plan links to the Task defining the outcome.

### Backlog

A view used to select and prioritize work.

Example: A Backlog links to Tasks instead of copying their current state.

### Scenario

One starting situation, its actions, and the expected result.

Example: A cancelled install should leave workspace files unchanged.

### Run Record

The observed result of one actual attempt.

Example: A record notes the cancellation, checked files, and unrun checks.

### User Flow

An ordered journey connecting scenarios through each step's resulting state.

Example: Install, add a rule, ask the agent to make a change, then check its result.

### Scenario Collection

A group of related scenarios without a required order.

Example: Installation, cancellation, and update cases for workspace setup

## Packages and maintenance

### Catalogue

A set of available Extension packages, bundled with the CLI or stored in a local folder.

Example: The bundled catalogue contains `planning` and `workflows`.

### Package ID

The stable name used to select an Extension in commands.

Example: `orchestration` selects the package displayed as Task Coordination.

### Dependency

Another package that an Extension requires.

Example: Planning requires Workflow Support.

### Manifest

The `extension.json` file naming a package and declaring its version and dependencies.

Example: Planning lists `workflows` in `dependencies`.

### Ownership record

The CLI's record of files and generated sections it manages. It is for maintenance and gives content no authority.

Example: `.agents/open-forge.lock.json` records installed package files.

### Exclusion

A saved choice that keeps a route, path, package, or Library out of later installation and updates.

Example: `removedCategories: ["templates"]` keeps Templates omitted.

### Recovery bundle

A retained folder containing earlier file bytes saved for recovery. It is recovery evidence, not automatic rollback.

Example: An update reports the bundle holding the edited file it replaced.

### Authoritative source

The source whose answer controls one specific question. Authority is distinct from possession or managed ownership.

Example: The architecture document defines current component boundaries.

### Path grant

A workspace setting allowing Extension or Library files at specified destinations outside `.agents/`. It grants an agent no new tools.

Example: An `allowInstallPaths` entry admits a support-file destination.

### Library

A registered source root inside a workspace whose files appear at another location in that workspace through relative file links. Unlike copied package files, links still point to their source. See [Workspace libraries](/guides/cli#workspace-libraries).

Example: Team rules stay in their source folder while an attached workspace reads links to them.

## Setup choices

See [Installation](getting-started/installation.md#choose-the-installed-routes) for the complete route choices.

### Essentials

A setup preset that includes essential routes and leaves other built-in routes out.

Example: Templates is omitted until you enable it.

### Full Core

A setup preset selecting all built-in Framework routes.

Example: Templates and Archived are present but their entrypoints stay on demand.

### Custom

A setup preset where you choose each built-in route's inclusion and Git-sharing policy.

Example: Include Guidance and keep Working records out of Git.

## Loading

### Entrypoint at startup

Read before the task begins. Linked items open when selected, tagged to load, or explicitly required by a loaded rule.

Example: Full Core reads the Patterns entrypoint at startup, but its linked Patterns stay on demand.

### Entrypoint on demand

The entrypoint waits until a task selects it.

Example: Templates and Archived Memory entrypoints stay on demand.

### On demand

Not read until selected or required by a loaded rule.

Example: An optional workflow recipe opens for a task that needs it.

### Refresh point

`KeepInMind` context is read again at task start or resume, after context restoration, and before handoff or closeout while its scope stays active.

Example: The agent refreshes Emerging Memory before closing a task.

## Tags

Defined tags control loading or classify content, but grant no authority. Other tags are search and routing signals.

### `#LoadNow`

Read the linked file, in listed order, when a loaded parent exposes it and its file conditions apply. Entrypoints also apply their child loading rules.

Example: A testing Directive loads when its Directives parent is read.

### `#KeepInMind`

Read applicable context when its parent loads, then refresh it at each refresh point while its scope remains active.

Example: An active Checkpoint refreshes after its category has been selected.

### `#Contextual`

Useful context, not authority by itself. Treat it as unaccepted unless applicable authority establishes acceptance within its scope.

Example: An Emerging Analysis keeps an unaccepted conclusion visible.

### `#CurrentTruth`

Accepted current state within its stated scope.

Example: A Crystallized record states an accepted export limit.

### `#Evergreen`

Material that must stay aligned with accepted current state. It creates no authority or loading behavior.

Example: Update the affected architecture summary after an accepted design change.

### `#Core` tag

Classifies base routing, loading, workspace orientation, and reusable agent-facing roles.

Example: The Patterns entrypoint carries `Core`.

### `#Memory` tag

Classifies self-growing Markdown states and records.

Example: A saved export requirement carries `Memory`.

### `#Extension` tag

Classifies content supplied through an optional package. It does not identify which package manages the file.

Example: An installed planning Template carries `Extension`.

### `#Empty`

Marks the `none` placeholder in an entrypoint with no entries. It is not a loading tag.

Example: `- none - No entries - #Empty`

Next: [Routing](concepts/routing.md), or [choose an Extension](extensions/index.md#choose-by-need).
