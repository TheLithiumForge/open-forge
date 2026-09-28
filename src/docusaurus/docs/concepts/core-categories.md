---
title: Core categories
description: The six content roles in Core, the question each one answers, and how to tell them apart.
---

# Core categories

Core gives content six roles, called categories. Each category answers one question. To place something, pick the category whose question it answers.

| Category                  | Question it answers                                                         | At startup                                      |
| ------------------------- | --------------------------------------------------------------------------- | ----------------------------------------------- |
| [Directives](#directives) | What behavior is required in this scope?                                    | Entrypoint at startup, plus its Directive files |
| [Guidance](#guidance)     | What approach is recommended, and when does it fit?                         | Entrypoint at startup                           |
| [Patterns](#patterns)     | What reusable shape makes related work easy to create and inspect?          | Entrypoint at startup                           |
| [Skills](#skills)         | Which specialized capability would help with this work?                     | Entrypoint at startup                           |
| [Templates](#templates)   | What starting content can be copied, adapted, and maintained independently? | Entrypoint on demand                            |
| [Maps](#maps)             | Where is a useful local or external source, and when should it be used?     | Entrypoint at startup                           |

"Entrypoint at startup" means the agent reads the category's entrypoint: its purpose, its rules, and one line per item. It doesn't read the items. It opens one when the task calls for it. "Entrypoint on demand" means the entrypoint itself waits until a task opens it. Directive files are the exception: each file directly in `directives/` must carry `#LoadNow`, so it loads with its entrypoint. [Loading and tags](loading-and-tags.md#what-loads-at-startup-in-a-fresh-install) lists every startup file.

In a fresh install, every category entrypoint lists no items except Skills, which lists `open-forge-cli`. You or an [Extension](../extensions/index.md) add the rest.

## Directives

**Required behavior.** A Directive is a rule the agent must follow within its scope.

- Every Directive file directly in a Directives folder must carry `#LoadNow`, because it's mandatory there. Without the tag, it stays on demand.
- Each Directive file keeps its instructions under a non-empty `## Instructions` heading.
- Files in the root `directives/` folder apply to the whole workspace. A scoped folder like `directives/frontend/` loads only when selected, and its Directives then add to the root ones. A narrower Directive never cancels a broader one.

Use a Directive when behavior is mandatory in a scope, such as a correction you keep repeating. If the right approach depends on the situation, use Guidance instead.

## Guidance

**Recommended approaches.** Guidance explains a recurring situation, the recommended approach, why it works, and its tradeoffs. Unlike a Directive, it can be adapted when another approach fits better, and the agent explains the difference when it matters.

## Patterns

**Reusable shapes.** A Pattern defines a concrete, inspectable shape: how an API response looks, how a test file is organized, where a component's files live. Each Pattern covers one shape.

Shared shapes make related work consistent and easier to inspect. In review, work that departs from the local Pattern stands out.

A Pattern is the default shape in its scope. A justified departure is allowed, and the agent explains material departures before other work depends on them. When a Directive makes a shape mandatory, the agent follows the Directive.

## Skills

**Specialized capabilities.** A Skill is a native capability package entered through a `SKILL.md` file. The `SKILL.md` defines how to use the Skill and its resources. Your agent runtime controls how Skills are installed, activated, invoked, and executed.

Open Forge routes to Skills so agents can find them. It doesn't replace your harness's own Skill mechanism. The base ships one Skill, `open-forge-cli`, which teaches an agent when and how to use the [CLI](../cli/index.md). At startup the agent sees only its one-line entry. Repeatable step-by-step methods live behind another Skill, `use-workflow`, supplied by the [Workflow Support](../extensions/workflows.md) Extension.

## Templates

**Copy-ready starting files.** Copy a Template, adapt it, then maintain the result independently. Later changes to the Template never update copies made from it.

The Templates entrypoint stays on demand, and a fresh install ships no Templates. [Core Templates](../extensions/core-templates.md) adds one starter per Core category, plus one for Memory. Other Extensions add Templates for their own records, and you can write your own.

## Maps

**Pointers to important sources.** A Map links to local or external sources and says what they contain and when to use them: the architecture document, the API reference, a sibling repository. Maps stay broad. They point to the source and don't replace it.

## Telling them apart

- **Required or recommended?** Required is a Directive. Recommended is Guidance.
- **A shape or a procedure?** A shape is a Pattern. A step-by-step procedure is a workflow recipe behind the `use-workflow` Skill from Workflow Support.
- **Ongoing or one-time?** A Directive or Pattern keeps guiding related work. A Template is starting content you copy once.
- **Content or a pointer?** Content lives in its own file. A pointer to where content lives is a Map.
- **Something to remember?** That's [Memory](memory.md), not a Core category. Writing an instruction into Memory doesn't make it a Directive.

Next: [Memory](memory.md).
