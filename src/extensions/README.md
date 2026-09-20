# Open Forge Extensions

Choose optional content by the work it helps you do. Core supplies routing, scope, authority, and generic Memory. Extensions add useful methods, record conventions, and starting files.

## Choose By Need

| Need | Package | What it adds | Direct dependencies |
| --- | --- | --- | --- |
| Explore choices and clarify direction | [Collaboration](collaboration/README.md) | Adaptive Collaboration; Brainstorming starter | None |
| Maintain current project knowledge | [Project Documents](project-documents/README.md) | Documents; five document starters; Vision and Architecture | Workflow Support |
| Explore, decide, and organize work | [Planning](planning/README.md) | Ideas, Analysis, Decisions, Checkpoints; eleven starters; Work Records; Planning | Workflow Support |
| Implement, debug, or review | [Development](development/README.md) | Three focused methods | Workflow Support |
| Coordinate related tasks and learn from execution | [Orchestration](orchestration/README.md) | Managed Delivery; Observations, Handoffs; Observation and Handoff starters | Planning, Development |
| Start with project knowledge, planning, and development | [Development Toolkit](development-toolkit/README.md) | Selects the three packages together; no content of its own | Project Documents, Planning, Development |
| Author or select reusable methods | [Workflow Support](workflows/README.md) | One native selector Skill, its catalogue, and a Workflow starter | None |

Workflow Support keeps the stable ID `workflows`. All packages remain optional. Category definitions and recipe bodies are selected on demand rather than loaded merely because a package was installed.

## How They Fit

Dependency arrows below point from a package to what it needs:

```text
collaboration       -> no dependencies
project-documents   -> workflows
planning            -> workflows
development         -> workflows
orchestration       -> planning, development
development-toolkit -> project-documents, planning, development
workflows           -> no dependencies
```

Collaboration and Orchestration are not part of the Toolkit. Selecting Orchestration also supplies Planning, Development, and one copy of Workflow Support, but not Project Documents.

## Packaging Does Not Change Meaning

A Decision is still an accepted choice, whether used during planning, operations, or research. An Observation does not require an Overseer. A Document need not have been produced by a planning process.

Each package owns complete files at their installed paths. Shared template parents are avoided: Planning owns `templates/planning/`, Project Documents owns `templates/documents/`, and Orchestration owns `templates/orchestration/`. Collaboration owns `templates/collaboration/`. Workflow Support owns `templates/workflows/` and the shared selector. Generated Entries expose whichever files are present.

Users can customize or remove content. Review actual descendants and dependencies when removing a category; user-created records and Template copies are not disposable package files. Removed defaults are not permission to restore them later.

## Review And Use

See [installation and customization](../../docs/extensions.md).

The source uses `content/` exactly as supplied. Revision versions are `0.3.0`; they are source metadata, not a published compatibility guarantee. The former `memory-starters` package is retired, not a hidden dependency or a compatibility alias. Existing ownership requires a reviewed migration.
