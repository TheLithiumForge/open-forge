---
title: Development Toolkit
description: A dependency-only bundle that installs Project Documents, Planning, Flows and Scenarios, and Development together.
---

# Development Toolkit

Install the common set for software work in one step. This package has no files of its own. It depends on four packages, and three of those depend on [Workflow Support](workflows.md), so one install brings in five.

- **[Package ID](../glossary.md#package-id):** `development-toolkit`
- **Depends on:** [Development](development.md), [Planning](planning.md), [Project Documents](project-documents.md), [Flows and Scenarios](scenarios.md)
- **Startup visibility:** This package has no entries of its own. Required packages add the Memory categories and Work Records Pattern from Planning, the Documents category from Project Documents, and the `use-workflow` entry from Workflow Support. The files behind those entries stay on demand.

These counts describe this package's own entries when their parent routes expose them in `Entries` and are read. Required packages add their own entries. Linked files stay on demand unless selected or required.

## Install

Package files follow your installation choices. Essentials leaves out Templates, so Extension starters are skipped. [Restore the Templates category](index.md#if-your-setup-leaves-out-templates) if you want those starters.

From your installed Open Forge workspace, preview the package and its required packages:

```sh
open-forge extension install development-toolkit --dry-run
```

Review the plan. Then apply it:

```sh
open-forge extension install development-toolkit
```

The CLI installs required packages from the same [catalogue](../glossary.md#catalogue). See the [Extension guide](/guides/extensions#install-an-extension) for scripts using `--automatic`, updates, removal, or a local source.

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

Next: [Install this package](#install), or [update and remove Extensions](/guides/extensions#update-and-remove).
