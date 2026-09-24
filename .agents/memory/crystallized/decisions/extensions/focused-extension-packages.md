---
open-forge:
  description: Why focused Extension packages replace Toolkit payload ownership and Experience Design leaves the catalogue
  tags: [Memory, Decision, CurrentTruth, Extension, Catalogue]
---

# Focused Extension Packages

## Accepted Choice

The user explicitly accepted the focused package split and removal of Experience Design during Task 28. The [first-party catalogue](../../../../../src/extensions/README.md) defines the resulting package inventory and contents.

The current catalogue has independently selectable Core Templates, Collaboration, Project Documents, Planning, Flows and Scenarios, Observations and Handoffs, Development, Task Coordination, and Workflow Support packages. Development Toolkit selects Project Documents, Planning, Flows and Scenarios, and Development. Task Coordination keeps the ID `orchestration` and depends on Development, Observations and Handoffs, and Planning. Review stays with Development to avoid another package boundary for one related workflow.

The maintainer accepted this beta refinement on 2026-09-23 after reviewing the package boundaries and descriptions. Core Templates supplies one starter per Core category plus Memory, with a native Skill file inside its routed starter. It remains optional and outside the Toolkit. Scenario starters leave Planning so they can serve both experience design and verification independently. Observations and Handoffs leave Task Coordination so they can be used without development methods. Package names and descriptions say what is supplied or what work they help with.

Memory category paths remain stable. Scenario starters move to `templates/scenarios/`; Observation and Handoff starters move to `templates/observations-and-handoffs/`. Existing user records and independently maintained Template copies retain their content and locations. The original Task 28 split kept all retained payload paths and wording; the later beta refinement changes the starter paths and makes their source instructions visible. Each Template subtree has one package responsible for its entrypoint and files. Shared content is supplied through dependencies. The bundle owns no payload files.

## Rationale

Project documentation, Memory starters, planning, development, and coordinated delivery serve distinct selection needs. A workspace can choose what helps without installing unrelated content. Keeping the Toolkit bundle preserves a simple way to select the common set.

These are distribution boundaries. The [Extension architecture](../../documents/extensions/architecture.md) continues to define composition; installed routes and native consumers define meaning. No new Framework category, mandatory record collection, fixed agent hierarchy, or APM dependency follows from this split.

The Experience Design Skill and its three references are removed from the shipped catalogue and the matching repository copies. Its general review reminders did not have demonstrated added value sufficient to justify continued first-party distribution. The [assessment](../../../emerging/analysis/framework-review/experience-design-value.md) records that reasoning and its limits. This is not a measured finding that every agent already covers those concerns, or a rejection of prose Skills. A future focused capability can be proposed when actual work establishes a useful gap.

## Delivery Boundary

The source reorganization is part of Task 28's review branch. It does not authorize merging or release. The [catalogue synchronization task](../../../archived/cli-development/tasks/extension-catalogue-synchronization.md) assigns embedded catalogue refresh and executable qualification to the separate CLI implementer.

Moving a file's package source does not itself transfer managed ownership in an existing installation. Old Toolkit receipts, workspace edits, removed defaults, and retired Skill files need explicit transition checks. Matching bytes alone do not authorize adopting an existing file.

## Prior Choice

The [Toolkit consolidation](development-toolkit.md) records the earlier catalogue decision. This choice supersedes its single-package distribution and Skill-retention outcomes while preserving optional complete-file packaging and independent installed interpretation.
