---
title: Development Toolkit
description: A dependency-only bundle that installs Project Documents, Planning, Flows and Scenarios, and Development together.
---

# Development Toolkit

Install the common set for software work in one step. This package has no files of its own. It exists only to pull in four others.

- **Package ID:** `development-toolkit`
- **Depends on:** [Development](development.md), [Planning](planning.md), [Project Documents](project-documents.md), [Flows and Scenarios](scenarios.md)
- **Loads at startup:** Only what its dependencies expose.

```sh
open-forge extension install development-toolkit --dry-run
```

## What you get

| Package                                   | Contribution                                                                                           |
| ----------------------------------------- | ------------------------------------------------------------------------------------------------------ |
| [Project Documents](project-documents.md) | Current documents, document starters, and the Vision and Architecture workflows                        |
| [Planning](planning.md)                   | Ideas, Analysis, Decisions, Checkpoints, the Work Records Pattern, starters, and the Planning workflow |
| [Flows and Scenarios](scenarios.md)       | User Flow, Scenario, Scenario Collection, and Run Record starters                                      |
| [Development](development.md)             | Development, Debugging, and Review workflows                                                           |
| [Workflow Support](workflows.md)          | The `use-workflow` Skill that all the workflows above go through, installed once                       |

## Separate choices

[Core Templates](core-templates.md), [Collaboration](collaboration.md), [Observations and Handoffs](observations-and-handoffs.md), and [Task Coordination](orchestration.md) aren't included. Add them individually if you want them.

Install individual packages instead when only part of the set is useful. A bundle selects capabilities. It doesn't make every convention mandatory.
