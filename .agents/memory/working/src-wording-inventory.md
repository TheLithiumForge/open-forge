---
open-forge:
  description: Source prose inventory and explicit review coverage for the lossless wording proposal
  tags: [Memory, Working, Evidence, Wording, Contextual]
---

# Source wording inventory

Baseline: `f5f91a92`. See the [proposal](src-wording-proposal.md) for the eight
candidate replacements, inheritance premises and protected repetition.

This inventory distinguishes a semantic prose read from a source-surface scan.
It is not a claim that every implementation string or historical transcript was
revalidated against every runtime path. This is the original proposal inventory;
the subsequently authorized replacements are tracked in the proposal’s
implementation-verification section.

## Markdown coverage

All 71 Markdown files are accounted for: 14 Core, 55 Extension and 2 CLI evidence
files. Core and Extension bodies were read for role, scope, inheritance,
optional versus required behavior, source authority and independent entrypoints.
“Retain” means no replacement is proposed in this pass, not a timeless guarantee
that the file cannot improve. Archived contract transcripts remain unchanged.

| Source                                                                                                                                                                                                                                                      | Treatment                                                                      |
| ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------ |
| [src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md](../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md)               | Preserved historical examples; scope notice inspected, no runtime revalidation |
| [src/cli/tests/README.md](../../../src/cli/tests/README.md)                                                                                                                                                                                                 | Read; ambiguous “this project” noted separately                                |
| [src/extensions/collaboration/content/.agents/guidance/adaptive-collaboration.md](../../../src/extensions/collaboration/content/.agents/guidance/adaptive-collaboration.md)                                                                                 | Retain; semantic prose read                                                    |
| [src/extensions/collaboration/content/.agents/templates/collaboration/_collaboration.md](../../../src/extensions/collaboration/content/.agents/templates/collaboration/_collaboration.md)                                                                   | W07                                                                            |
| [src/extensions/collaboration/content/.agents/templates/collaboration/brainstorming.md](../../../src/extensions/collaboration/content/.agents/templates/collaboration/brainstorming.md)                                                                     | Retain; semantic prose read                                                    |
| [src/extensions/collaboration/README.md](../../../src/extensions/collaboration/README.md)                                                                                                                                                                   | Retain; semantic prose read                                                    |
| [src/extensions/development/content/.agents/skills/use-workflow/references/development/_development.md](../../../src/extensions/development/content/.agents/skills/use-workflow/references/development/_development.md)                                     | Retain; semantic prose read                                                    |
| [src/extensions/development/content/.agents/skills/use-workflow/references/development/debugging.md](../../../src/extensions/development/content/.agents/skills/use-workflow/references/development/debugging.md)                                           | Retain; semantic prose read                                                    |
| [src/extensions/development/content/.agents/skills/use-workflow/references/development/development.md](../../../src/extensions/development/content/.agents/skills/use-workflow/references/development/development.md)                                       | Retain; semantic prose read                                                    |
| [src/extensions/development/content/.agents/skills/use-workflow/references/development/review.md](../../../src/extensions/development/content/.agents/skills/use-workflow/references/development/review.md)                                                 | Retain; semantic prose read                                                    |
| [src/extensions/development/README.md](../../../src/extensions/development/README.md)                                                                                                                                                                       | Retain; semantic prose read                                                    |
| [src/extensions/development-toolkit/README.md](../../../src/extensions/development-toolkit/README.md)                                                                                                                                                       | Retain; semantic prose read                                                    |
| [src/extensions/orchestration/content/.agents/memory/emerging/observations/_observations.md](../../../src/extensions/orchestration/content/.agents/memory/emerging/observations/_observations.md)                                                           | Retain; semantic prose read                                                    |
| [src/extensions/orchestration/content/.agents/memory/working/handoffs/_handoffs.md](../../../src/extensions/orchestration/content/.agents/memory/working/handoffs/_handoffs.md)                                                                             | Retain; semantic prose read                                                    |
| [src/extensions/orchestration/content/.agents/skills/use-workflow/references/orchestration/_orchestration.md](../../../src/extensions/orchestration/content/.agents/skills/use-workflow/references/orchestration/_orchestration.md)                         | Retain; semantic prose read                                                    |
| [src/extensions/orchestration/content/.agents/skills/use-workflow/references/orchestration/managed-delivery.md](../../../src/extensions/orchestration/content/.agents/skills/use-workflow/references/orchestration/managed-delivery.md)                     | Retain; semantic prose read                                                    |
| [src/extensions/orchestration/content/.agents/templates/orchestration/_orchestration.md](../../../src/extensions/orchestration/content/.agents/templates/orchestration/_orchestration.md)                                                                   | Retain; semantic prose read                                                    |
| [src/extensions/orchestration/content/.agents/templates/orchestration/handoff.md](../../../src/extensions/orchestration/content/.agents/templates/orchestration/handoff.md)                                                                                 | Retain; semantic prose read                                                    |
| [src/extensions/orchestration/content/.agents/templates/orchestration/observation.md](../../../src/extensions/orchestration/content/.agents/templates/orchestration/observation.md)                                                                         | Retain; semantic prose read                                                    |
| [src/extensions/orchestration/README.md](../../../src/extensions/orchestration/README.md)                                                                                                                                                                   | Retain; semantic prose read                                                    |
| [src/extensions/planning/content/.agents/memory/crystallized/decisions/_decisions.md](../../../src/extensions/planning/content/.agents/memory/crystallized/decisions/_decisions.md)                                                                         | Retain; semantic prose read                                                    |
| [src/extensions/planning/content/.agents/memory/emerging/analysis/_analysis.md](../../../src/extensions/planning/content/.agents/memory/emerging/analysis/_analysis.md)                                                                                     | Retain; semantic prose read                                                    |
| [src/extensions/planning/content/.agents/memory/emerging/ideas/_ideas.md](../../../src/extensions/planning/content/.agents/memory/emerging/ideas/_ideas.md)                                                                                                 | Retain; semantic prose read                                                    |
| [src/extensions/planning/content/.agents/memory/working/checkpoints/_checkpoints.md](../../../src/extensions/planning/content/.agents/memory/working/checkpoints/_checkpoints.md)                                                                           | Retain; semantic prose read                                                    |
| [src/extensions/planning/content/.agents/patterns/work-records.md](../../../src/extensions/planning/content/.agents/patterns/work-records.md)                                                                                                               | W06                                                                            |
| [src/extensions/planning/content/.agents/skills/use-workflow/references/planning/_planning.md](../../../src/extensions/planning/content/.agents/skills/use-workflow/references/planning/_planning.md)                                                       | Retain; semantic prose read                                                    |
| [src/extensions/planning/content/.agents/skills/use-workflow/references/planning/planning.md](../../../src/extensions/planning/content/.agents/skills/use-workflow/references/planning/planning.md)                                                         | Retain; semantic prose read                                                    |
| [src/extensions/planning/content/.agents/templates/planning/_planning.md](../../../src/extensions/planning/content/.agents/templates/planning/_planning.md)                                                                                                 | Retain; semantic prose read                                                    |
| [src/extensions/planning/content/.agents/templates/planning/analysis.md](../../../src/extensions/planning/content/.agents/templates/planning/analysis.md)                                                                                                   | Retain; semantic prose read                                                    |
| [src/extensions/planning/content/.agents/templates/planning/backlog.md](../../../src/extensions/planning/content/.agents/templates/planning/backlog.md)                                                                                                     | Retain; semantic prose read                                                    |
| [src/extensions/planning/content/.agents/templates/planning/checkpoint.md](../../../src/extensions/planning/content/.agents/templates/planning/checkpoint.md)                                                                                               | Retain; semantic prose read                                                    |
| [src/extensions/planning/content/.agents/templates/planning/decision.md](../../../src/extensions/planning/content/.agents/templates/planning/decision.md)                                                                                                   | Retain; semantic prose read                                                    |
| [src/extensions/planning/content/.agents/templates/planning/idea.md](../../../src/extensions/planning/content/.agents/templates/planning/idea.md)                                                                                                           | Retain; semantic prose read                                                    |
| [src/extensions/planning/content/.agents/templates/planning/plan.md](../../../src/extensions/planning/content/.agents/templates/planning/plan.md)                                                                                                           | Retain; semantic prose read                                                    |
| [src/extensions/planning/content/.agents/templates/planning/run-record.md](../../../src/extensions/planning/content/.agents/templates/planning/run-record.md)                                                                                               | Retain; semantic prose read                                                    |
| [src/extensions/planning/content/.agents/templates/planning/scenario-collection.md](../../../src/extensions/planning/content/.agents/templates/planning/scenario-collection.md)                                                                             | Retain; semantic prose read                                                    |
| [src/extensions/planning/content/.agents/templates/planning/scenario.md](../../../src/extensions/planning/content/.agents/templates/planning/scenario.md)                                                                                                   | Retain; semantic prose read                                                    |
| [src/extensions/planning/content/.agents/templates/planning/task.md](../../../src/extensions/planning/content/.agents/templates/planning/task.md)                                                                                                           | Retain; semantic prose read                                                    |
| [src/extensions/planning/content/.agents/templates/planning/user-flow.md](../../../src/extensions/planning/content/.agents/templates/planning/user-flow.md)                                                                                                 | Retain; semantic prose read                                                    |
| [src/extensions/planning/README.md](../../../src/extensions/planning/README.md)                                                                                                                                                                             | Retain; semantic prose read                                                    |
| [src/extensions/project-documents/content/.agents/memory/crystallized/documents/_documents.md](../../../src/extensions/project-documents/content/.agents/memory/crystallized/documents/_documents.md)                                                       | W05                                                                            |
| [src/extensions/project-documents/content/.agents/skills/use-workflow/references/project-documents/_project-documents.md](../../../src/extensions/project-documents/content/.agents/skills/use-workflow/references/project-documents/_project-documents.md) | Retain; semantic prose read                                                    |
| [src/extensions/project-documents/content/.agents/skills/use-workflow/references/project-documents/architecture.md](../../../src/extensions/project-documents/content/.agents/skills/use-workflow/references/project-documents/architecture.md)             | Retain; semantic prose read                                                    |
| [src/extensions/project-documents/content/.agents/skills/use-workflow/references/project-documents/vision.md](../../../src/extensions/project-documents/content/.agents/skills/use-workflow/references/project-documents/vision.md)                         | W04                                                                            |
| [src/extensions/project-documents/content/.agents/templates/documents/_documents.md](../../../src/extensions/project-documents/content/.agents/templates/documents/_documents.md)                                                                           | Retain; semantic prose read                                                    |
| [src/extensions/project-documents/content/.agents/templates/documents/architecture.md](../../../src/extensions/project-documents/content/.agents/templates/documents/architecture.md)                                                                       | Retain; semantic prose read                                                    |
| [src/extensions/project-documents/content/.agents/templates/documents/document.md](../../../src/extensions/project-documents/content/.agents/templates/documents/document.md)                                                                               | Retain; semantic prose read                                                    |
| [src/extensions/project-documents/content/.agents/templates/documents/maintenance-contract.md](../../../src/extensions/project-documents/content/.agents/templates/documents/maintenance-contract.md)                                                       | Retain; semantic prose read                                                    |
| [src/extensions/project-documents/content/.agents/templates/documents/principles.md](../../../src/extensions/project-documents/content/.agents/templates/documents/principles.md)                                                                           | Retain; semantic prose read                                                    |
| [src/extensions/project-documents/content/.agents/templates/documents/vision.md](../../../src/extensions/project-documents/content/.agents/templates/documents/vision.md)                                                                                   | Retain; semantic prose read                                                    |
| [src/extensions/project-documents/README.md](../../../src/extensions/project-documents/README.md)                                                                                                                                                           | Retain; semantic prose read                                                    |
| [src/extensions/README.md](../../../src/extensions/README.md)                                                                                                                                                                                               | Retain; semantic prose read                                                    |
| [src/extensions/workflows/content/.agents/skills/use-workflow/references/_references.md](../../../src/extensions/workflows/content/.agents/skills/use-workflow/references/_references.md)                                                                   | Retain; semantic prose read                                                    |
| [src/extensions/workflows/content/.agents/skills/use-workflow/SKILL.md](../../../src/extensions/workflows/content/.agents/skills/use-workflow/SKILL.md)                                                                                                     | W08                                                                            |
| [src/extensions/workflows/content/.agents/templates/workflows/_workflows.md](../../../src/extensions/workflows/content/.agents/templates/workflows/_workflows.md)                                                                                           | Retain; semantic prose read                                                    |
| [src/extensions/workflows/content/.agents/templates/workflows/workflow.md](../../../src/extensions/workflows/content/.agents/templates/workflows/workflow.md)                                                                                               | Retain; semantic prose read                                                    |
| [src/extensions/workflows/README.md](../../../src/extensions/workflows/README.md)                                                                                                                                                                           | Retain; semantic prose read                                                    |
| [src/open-forge/.agents/directives/_directives.md](../../../src/open-forge/.agents/directives/_directives.md)                                                                                                                                               | W02                                                                            |
| [src/open-forge/.agents/guidance/_guidance.md](../../../src/open-forge/.agents/guidance/_guidance.md)                                                                                                                                                       | Retain; semantic prose read                                                    |
| [src/open-forge/.agents/loader.md](../../../src/open-forge/.agents/loader.md)                                                                                                                                                                               | Retain; semantic prose read                                                    |
| [src/open-forge/.agents/maps/_maps.md](../../../src/open-forge/.agents/maps/_maps.md)                                                                                                                                                                       | Retain; semantic prose read                                                    |
| [src/open-forge/.agents/memory/_memory.md](../../../src/open-forge/.agents/memory/_memory.md)                                                                                                                                                               | Retain; integration and closeout inventory are distinct duties                 |
| [src/open-forge/.agents/memory/archived/_archived.md](../../../src/open-forge/.agents/memory/archived/_archived.md)                                                                                                                                         | Retain; semantic prose read                                                    |
| [src/open-forge/.agents/memory/crystallized/_crystallized.md](../../../src/open-forge/.agents/memory/crystallized/_crystallized.md)                                                                                                                         | Retain; semantic prose read                                                    |
| [src/open-forge/.agents/memory/emerging/_emerging.md](../../../src/open-forge/.agents/memory/emerging/_emerging.md)                                                                                                                                         | Retain; semantic prose read                                                    |
| [src/open-forge/.agents/memory/working/_working.md](../../../src/open-forge/.agents/memory/working/_working.md)                                                                                                                                             | Retain; semantic prose read                                                    |
| [src/open-forge/.agents/patterns/_patterns.md](../../../src/open-forge/.agents/patterns/_patterns.md)                                                                                                                                                       | W03                                                                            |
| [src/open-forge/.agents/skills/_skills.md](../../../src/open-forge/.agents/skills/_skills.md)                                                                                                                                                               | Retain; semantic prose read                                                    |
| [src/open-forge/.agents/templates/_templates.md](../../../src/open-forge/.agents/templates/_templates.md)                                                                                                                                                   | W01                                                                            |
| [src/open-forge/AGENTS.md](../../../src/open-forge/AGENTS.md)                                                                                                                                                                                               | Retain; semantic prose read                                                    |
| [src/open-forge/CLAUDE.md](../../../src/open-forge/CLAUDE.md)                                                                                                                                                                                               | Retain; semantic prose read                                                    |

## Extension identity and dependencies

All seven manifest descriptions were compared with the corresponding README,
payload roles and direct dependency list. Keep IDs, versions and dependencies
unchanged. A package dependency ensures installation, not automatic loading.

| Package                                                                           | Direct dependencies                      | Result                          |
| --------------------------------------------------------------------------------- | ---------------------------------------- | ------------------------------- |
| [collaboration](../../../src/extensions/collaboration/extension.json)             | None                                     | No wording replacement proposed |
| [development](../../../src/extensions/development/extension.json)                 | workflows                                | No wording replacement proposed |
| [development-toolkit](../../../src/extensions/development-toolkit/extension.json) | development, planning, project-documents | No wording replacement proposed |
| [orchestration](../../../src/extensions/orchestration/extension.json)             | development, planning                    | No wording replacement proposed |
| [planning](../../../src/extensions/planning/extension.json)                       | workflows                                | No wording replacement proposed |
| [project-documents](../../../src/extensions/project-documents/extension.json)     | workflows                                | No wording replacement proposed |
| [workflows](../../../src/extensions/workflows/extension.json)                     | None                                     | No wording replacement proposed |

## CLI source surfaces

The 101 OutputText C# files contain 2,549 string-bearing lines (a textual count,
not a count of distinct messages or executed scenarios). The scan covered all
files below for language about inheritance, loading, authority, default selection
and Skills. Shared help, Index help and relevant command-definition descriptions
were read in context. Repeated command output does not inherit a previous
command's explanation. No factory, identifier, selection logic or expectation
is changed by this proposal.

| OutputText area | Files | Review level                                                         |
| --------------- | ----- | -------------------------------------------------------------------- |
| Cleanup         | 3     | Surface inventory and topic scan; no exhaustive execution-path audit |
| Context         | 2     | Surface inventory and topic scan; no exhaustive execution-path audit |
| Doctor          | 4     | Surface inventory and topic scan; no exhaustive execution-path audit |
| Extension       | 21    | Surface inventory and topic scan; no exhaustive execution-path audit |
| Find            | 2     | Surface inventory and topic scan; no exhaustive execution-path audit |
| Index           | 3     | Surface inventory and topic scan; no exhaustive execution-path audit |
| Install         | 3     | Surface inventory and topic scan; no exhaustive execution-path audit |
| Library         | 18    | Surface inventory and topic scan; no exhaustive execution-path audit |
| References      | 3     | Surface inventory and topic scan; no exhaustive execution-path audit |
| Repair          | 4     | Surface inventory and topic scan; no exhaustive execution-path audit |
| Route           | 25    | Surface inventory and topic scan; no exhaustive execution-path audit |
| Shared          | 7     | Surface inventory and topic scan; no exhaustive execution-path audit |
| Status          | 3     | Surface inventory and topic scan; no exhaustive execution-path audit |
| Update          | 3     | Surface inventory and topic scan; no exhaustive execution-path audit |

Command definitions, raw diagnostic producers, rendering composition, source
comments and test fixtures are additional explanatory surfaces in `src/cli`.
This proposal does not claim a complete semantic audit of those files. A string
scan alone cannot prove a runtime sentence is true: command-specific producer
facts and complete rendered output are needed. Existing Task 39/37 scopes are
the place for that work if selected, rather than extending this inheritance
proposal into an unbounded code and snapshot rewrite.

## Repetition evidence

An exact-line comparison found a shared customization paragraph in five package
READMEs, identical preparation instructions in fifteen starters, and the inherited
Axioms marker in four recipe scope entrypoints. All three groups are retained:
independent entrypoints, copied artifacts and explicit empty local-Axiom state
are different from an inherited instruction needlessly repeated in a child.

Original proposal validation matched each quoted source excerpt, checked links
and formatting, and confirmed no source changes at that planning boundary.
The maintainer subsequently authorized implementation; see the proposal's
implementation-verification section for the applied change and executed checks.
