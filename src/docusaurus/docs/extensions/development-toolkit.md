---
title: Development Toolkit
description: A dependency-only bundle that installs Project Documents, Planning, Flows and Scenarios, and Development together.
---

# Development Toolkit

Install the common set for software work in one step. This package has no files of its own. It depends on four packages, and three of those depend on [Workflow Support](workflows.md), so one install brings in five.

- **Package ID:** `development-toolkit`
- **Depends on:** [Development](development.md), [Planning](planning.md), [Project Documents](project-documents.md), [Flows and Scenarios](scenarios.md)
- **Loads at startup:** Nothing of its own. Its packages add one-line entries to entrypoints that load at startup: Memory categories and the Work Records Pattern from Planning, the Documents category from Project Documents, and the `use-workflow` Skill from Workflow Support. The files behind those entries open on demand.

```sh
open-forge extension install development-toolkit --dry-run
```

## Why it exists

These four packages are the common set for software work, and the Toolkit makes them one install. While the Toolkit is installed, you can't remove one of its packages on its own, because the Toolkit depends on it. If you want only some of them, install those individually instead.

## What you get

| Package                                   | Contribution                                                                                                                 |
| ----------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------- |
| [Project Documents](project-documents.md) | The Documents Memory category, document starters, and the Vision and Architecture workflows                                  |
| [Planning](planning.md)                   | Ideas, Analysis, Decisions, and Checkpoints Memory categories, the Work Records Pattern, starters, and the Planning workflow |
| [Flows and Scenarios](scenarios.md)       | User Flow, Scenario, Scenario Collection, and Run Record starters                                                            |
| [Development](development.md)             | Development, Debugging, and Review workflows                                                                                 |
| [Workflow Support](workflows.md)          | The `use-workflow` Skill that selects and guides the workflows above, and a starter for writing your own                     |

## Separate choices

[Core Templates](core-templates.md), [Collaboration](collaboration.md), [Observations and Handoffs](observations-and-handoffs.md), and [Task Coordination](orchestration.md) aren't included. Add them individually if you want them.

Install individual packages instead when only part of the set is useful. A bundle selects capabilities. It doesn't make every convention mandatory.
