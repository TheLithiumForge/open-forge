# Open Forge Extensions

Choose optional content by the work it helps you do. Core supplies routing, scope, authority, and generic Memory. Extensions add useful methods, record conventions, and starting files.

## Choose By Need

| Package | Description | Direct dependencies |
| --- | --- | --- |
| [Core Templates](core-templates/README.md) | Create directives, guidance, patterns, skills, templates, maps, and memory records from adaptable starting files | None |
| [Collaboration](collaboration/README.md) | Explore ideas, compare alternatives, and clarify decisions through discussion and a brainstorming template | None |
| [Project Documents](project-documents/README.md) | Write and maintain project documentation, including vision, architecture, principles, and maintenance requirements | Workflow Support |
| [Planning](planning/README.md) | Record ideas, investigate questions, preserve decisions, and organize tasks, plans, backlogs, and checkpoints | Workflow Support |
| [Flows and Scenarios](scenarios/README.md) | Describe user journeys and expected outcomes, organize scenarios, and record what happened when they were tried | None |
| [Observations and Handoffs](observations-and-handoffs/README.md) | Save useful observations and prepare a clear snapshot for another person, agent, or session to resume work | None |
| [Development](development/README.md) | Implement changes, investigate defects, and review results using the project's existing tools and conventions | Workflow Support |
| [Task Coordination](orchestration/README.md) | Coordinate related tasks, assign responsibilities, manage dependencies, and verify combined results | Development, Observations and Handoffs, Planning |
| [Development Toolkit](development-toolkit/README.md) | Install Project Documents, Planning, Flows and Scenarios, and Development together | Development, Planning, Project Documents, Flows and Scenarios |
| [Workflow Support](workflows/README.md) | Find and follow installed workflows, or create your own using the supplied template | None |

Task Coordination keeps the stable ID `orchestration`. Workflow Support keeps `workflows`. All packages remain optional. Installing a package does not make its methods or record categories mandatory.

## How They Fit

Dependency arrows point from a package to what it needs:

```text
core-templates            -> no dependencies
collaboration             -> no dependencies
scenarios                 -> no dependencies
observations-and-handoffs -> no dependencies
project-documents         -> workflows
planning                  -> workflows
development               -> workflows
orchestration             -> development, observations-and-handoffs, planning
development-toolkit       -> development, planning, project-documents, scenarios
workflows                 -> no dependencies
```

Core Templates is a useful first choice when you want to add your own workspace content. Choose more specialized starters when their questions fit better.

Task Coordination also installs the methods it calls and the records it uses, without pulling in Project Documents or Flows and Scenarios. Development Toolkit supplies the common document, planning, scenario, and development set. Core Templates, Collaboration, Observations and Handoffs, and Task Coordination remain separate selections.

## Packaging Does Not Change Meaning

A Decision records an accepted choice wherever it is used. An Observation can come from solo work. A Scenario can help plan an experience before anyone tests it. Package selection does not narrow these roles.

Each package supplies complete files and owns its own Template subtree: `core/`, `collaboration/`, `documents/`, `planning/`, `scenarios/`, `observations-and-handoffs/`, or `workflows/`. Task Coordination supplies its workflow scope. Workflow Support supplies the shared selector and catalogue. The Toolkit contains dependencies only.

Users may customize or remove content. Review dependencies, references, and user-created descendants before removing a category. Template copies belong to their destinations; removing a package does not authorize deleting them.

## Review And Use

See [installation and customization](../../docs/extensions.md), including the [package split migration](../../docs/extensions.md#moving-from-the-earlier-package-layout).

The source uses `content/` exactly as supplied. Package revisions are `0.4.0`; these are source metadata, not a published compatibility guarantee. The former `memory-starters` package remains retired. Existing managed ownership requires a reviewed migration; moving a source file does not transfer ownership in an installed workspace.
