---
open-forge:
  description: Repository-only maintainer helper mapping Open Forge knowledge roles to the primary questions they answer
  responsibility: Make placement and document selection consistent without creating another user-facing role system
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Helper, Internal, KnowledgeRole]
---

# Knowledge Role Helper

## Purpose

This internal helper supports authoring, review, and ongoing maintenance when information appears to fit several knowledge roles. It does not add a required frontmatter question field or another Framework primitive.

Users should not need this table. Route descriptions, document contents, Templates, and future CLI help should make each selection understandable where it is encountered.

## Current Views And Memory

| File or concept                                                                             | Primary question                                                                                    |
| ------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------- |
| [Vision](../../vision.md)                                                                   | What should become true, why does it matter, and what would success mean?                           |
| [Principles](../../principles.md)                                                           | What must remain true when unfamiliar choices are made for this to remain the same subject?         |
| [Operating Context template](../../../../../templates/documents/operating-context.md)       | Which external reality, stakeholders, constraints, assumptions, and dependencies shape the subject? |
| [Strategy template](../../../../../templates/documents/strategy.md)                         | How is accepted focus expected to move the subject toward its Vision?                               |
| [Architecture](../../architecture.md)                                                       | How is the current subject structured, how do its parts relate, and which boundaries govern it?     |
| Architectural invariant                                                                     | What structural condition must remain true across the accepted architecture?                        |
| [Roadmap template](../../../../../templates/documents/roadmap.md)                           | Which accepted outcomes come next, in what order, and what may change that order?                   |
| [Maintenance Contract template](../../../../../templates/documents/maintenance-contract.md) | What must a source or repository surface preserve when it changes, and how is that verified?        |
| Current document                                                                            | What is accepted now, and how does this coherent concept work?                                      |
| [Project Status template](../../../../../templates/memory/project-status.md)                | Where does active work stand, what matters now, and how can it resume?                              |
| [Decision](../../../decisions/_decisions.md)                                                | What discrete choice was accepted, why, and what consequences followed?                             |
| Working Memory                                                                              | What temporary context is needed to continue or resume current work?                                |
| Working Checkpoint                                                                          | What current state, current step, and next steps must survive a pause, restoration, or transfer?    |
| Working Handoff                                                                             | What stable boundary state must a named recipient or resumption preserve while active work changes? |
| Emerging Analysis                                                                           | What question, evidence, assumptions, alternatives, and current conclusion remain unsettled?        |
| Emerging Idea                                                                               | What plausible possibility deserves exploration, and what would justify promotion?                  |
| Emerging Observation                                                                        | What concrete occurrence or pattern was noticed, with which evidence, scope, and uncertainty?       |
| Archived Memory                                                                             | What useful history no longer governs current work?                                                 |

`Foundation`, `identity`, and `essence` are not additional knowledge roles. A foundation is a load-bearing principle, identity is what the foundational set preserves, and essence is a concise summary that normally belongs in an authoritative source's description or opening.

## Core

The primitive questions below mirror the [Core primitive model](../../framework/primitives/model.md#roles).

| File or concept                                                            | Primary question                                                                                   |
| -------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------- |
| Core primitive                                                             | Which distinct reusable Core role should contain this material and define how its authority works? |
| Loader or entrypoint Axiom                                                 | What inherited Framework behavior is mandatory throughout this loaded route?                       |
| [Directive](../../../../../directives/_directives.md)                      | What behavior is mandatory in this scope?                                                          |
| [Guidance](../../../../../guidance/_guidance.md)                           | How should this recurring choice or scenario be approached?                                        |
| [Pattern](../../../../../patterns/_patterns.md)                            | What reusable inspectable shape is the established default?                                        |
| [Skill](../../../../../skills/_skills.md)                                  | What specialized capability can perform this work?                                                 |
| [Template](../../../../../templates/_templates.md)                         | What copy-ready source can start this artifact?                                                    |
| [Workflow](../../../../../workflows/_workflows.md)                         | How should this defined goal be pursued and completed?                                             |
| [Map `route`](../../../../../maps/_maps.md)                                | Where does relevant local or external truth live?                                                  |
| [Core primitive decision](../../../decisions/framework/core-primitives.md) | Why does Core use distinct reusable roles instead of one generic content bucket?                   |

## Selection Rule

Choose the role whose primary question matches the meaning being preserved.

If one statement answers several materially different questions, split it among the appropriate sources and connect them with relative links. If no row fits, first test whether an existing role can express the meaning clearly before proposing a new one.

Primary questions are diagnostic prompts for authors and reviewers. Runtime entries use natural, descriptive, suggestive descriptions instead of repeating a fixed phrase or requiring a separate question field.
