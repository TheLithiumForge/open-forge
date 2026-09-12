---
open-forge:
  description: Why focused Extension packages replace Toolkit payload ownership and Experience Design leaves the catalogue
  tags: [Memory, Decision, CurrentTruth, Extension, Catalogue]
---

# Focused Extension Packages

## Accepted Choice

The user explicitly accepted the focused package split and removal of Experience Design during Task 28. The [first-party catalogue](../../../../../src/extensions/README.md) defines the resulting package inventory and contents.

Project Documents, Memory Starters, Planning, Development, and Orchestration are independently selectable packages. Development Toolkit remains a convenience bundle of the first four. Orchestration depends only on Planning and Development. Review stays with Development to avoid an additional package boundary for one related Workflow.

Each retained payload file keeps its installed path and content. Each Template subtree has one package responsible for its entrypoint and files. Shared content is supplied through dependencies. The bundle owns no payload files.

## Rationale

Project documentation, Memory starters, planning, development, and coordinated delivery serve distinct selection needs. A workspace can choose what helps without installing unrelated content. Keeping the Toolkit bundle preserves a simple way to select the common set.

These are distribution boundaries. The [Extension architecture](../../documents/extensions/architecture.md) continues to define composition; installed routes and native consumers define meaning. No new Framework category, mandatory record collection, fixed agent hierarchy, or APM dependency follows from this split.

The Experience Design Skill and its three references are removed from the shipped catalogue and the matching repository copies. Its general review reminders did not have demonstrated added value sufficient to justify continued first-party distribution. The [assessment](../../../emerging/analysis/framework-review/experience-design-value.md) records that reasoning and its limits. This is not a measured finding that every agent already covers those concerns, or a rejection of prose Skills. A future focused capability can be proposed when actual work establishes a useful gap.

## Delivery Boundary

The source reorganization is part of Task 28's review branch. It does not authorize merging or release. The [catalogue synchronization task](../../../working/cli-development/tasks/extension-catalogue-synchronization.md) assigns embedded catalogue refresh and executable qualification to the separate CLI implementer.

Moving a file's package source does not itself transfer managed ownership in an existing installation. Old Toolkit receipts, workspace edits, removed defaults, and retired Skill files need explicit transition checks. Matching bytes alone do not authorize adopting an existing file.

## Prior Choice

The [Toolkit consolidation](development-toolkit.md) records the earlier catalogue decision. This choice supersedes its single-package distribution and Skill-retention outcomes while preserving optional complete-file packaging and independent installed interpretation.
