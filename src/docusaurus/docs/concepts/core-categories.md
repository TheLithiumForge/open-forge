---
title: Core categories
description: The six content roles in Core, the question each one answers, and how to tell them apart.
---

# Core categories

Core gives content six roles. Each one answers one question, and choosing the right role is most of the work of placing something well.

| Category                  | Question it answers                                                         | Loads at startup?           |
| ------------------------- | --------------------------------------------------------------------------- | --------------------------- |
| [Directives](#directives) | What behavior is required in this scope?                                    | Yes, within selected scopes |
| [Guidance](#guidance)     | What approach is recommended, and when does it fit?                         | Entrypoint only             |
| [Patterns](#patterns)     | What reusable shape makes related work easy to create and inspect?          | Entrypoint only             |
| [Skills](#skills)         | Which specialized capability would help with this work?                     | Entrypoint only             |
| [Templates](#templates)   | What starting content can be copied, adapted, and maintained independently? | No                          |
| [Maps](#maps)             | Where is a useful local or external source, and when should it be used?     | Entrypoint only             |

"Entrypoint only" means the agent sees the list of entries at startup, so it knows what exists, and opens individual files when the task calls for them.

## Directives

**Required behavior.** A Directive is a rule the agent must follow within its scope.

- Every Directive file directly in a Directives folder carries `#LoadNow`, because it's mandatory there.
- Each Directive file has one non-empty `## Instructions` section.
- Files in the root `directives/` folder apply to the whole workspace. A scoped folder like `directives/frontend/` loads only when selected, and its Directives then add to the root ones. A narrower Directive never cancels a broader one.

Use a Directive for a correction you keep repeating. If it's advice that depends on the situation, it's Guidance instead.

## Guidance

**Recommended approaches.** Guidance explains a recurring situation, the recommended approach, why it works, and its tradeoffs. Unlike a Directive, it can be adapted when another approach fits better, and the agent explains the difference when it matters.

## Patterns

**Reusable shapes.** A Pattern defines a concrete, inspectable shape: how an API response looks, how a test file is organized, where a component's files live. Each Pattern covers one shape.

Patterns do more than they seem to. When work follows the local pattern, that's a quiet sign it's probably right. When it doesn't, the drift stands out in review.

A Pattern is the default shape in its scope. A justified departure is allowed, and the agent explains material departures before other work depends on them.

## Skills

**Specialized capabilities.** A Skill is a native capability package entered through a `SKILL.md` file. The `SKILL.md` defines how to use the Skill and its resources, and your agent runtime controls how Skills are discovered, activated, and executed.

Open Forge routes to Skills so agents can find them. It doesn't replace your harness's own Skill mechanism. Repeatable step-by-step methods live behind one Skill, `use-workflow`, supplied by the [Workflow Support](../extensions/workflows.md) Extension.

## Templates

**Copy-ready starting files.** Copy a Template, adapt it, then maintain the result independently. Later changes to the Template never update copies made from it.

Templates stay on demand. Extensions supply most of them: see [Core Templates](../extensions/core-templates.md) for one starter per category.

## Maps

**Pointers to important sources.** A Map links to local or external sources and says what they contain and when to use them: the architecture document, the API reference, a sibling repository. Maps stay broad. They point to the source, they don't replace it.

## Telling them apart

- **Required or recommended?** Required is a Directive. Recommended is Guidance.
- **A shape or a procedure?** A shape is a Pattern. A step-by-step procedure is a workflow recipe behind the `use-workflow` Skill.
- **A rule or a starting point?** A rule you follow is a Directive or Pattern. A starting file you copy is a Template.
- **Content or a pointer?** Content lives in its own file. A pointer to where content lives is a Map.
- **Something to remember?** That's [Memory](memory.md), not a Core category. Writing an instruction into Memory doesn't make it a Directive.

Next: [Memory](memory.md).
