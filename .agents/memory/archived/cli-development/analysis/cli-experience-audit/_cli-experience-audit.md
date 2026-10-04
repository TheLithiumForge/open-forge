---
open-forge:
  description: "Historical record: Hands-on audit of the CLI experience layer covering interoperability, interaction, presentation, and per-command output design"
  tags: [Memory, Analysis, Contextual, CLI, Experience, Presentation, Interaction, Interoperability, Audit, Archived, Historical]
---

# CLI Experience Audit

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Archive Status

Archived on 2026-10-04 from `.agents/memory/emerging/analysis/cli-experience-audit/_cli-experience-audit.md` after the maintainer selected Memory cleanup. Superseded analytical baseline; preserve original bodies and current residue owners.

This record preserves historical evidence. The [current CLI development route](../../../../working/cli-development/_cli-development.md) and its open Tasks define current work. Local qualification in this record does not establish later integration or publication.

## Current orientation, 2026-10-04

[Task 30](../../tasks/task30-cli-experience-remediation.md)
and [Task 31](../../tasks/task31-implementation-duplication.md)
were closed and archived on 2026-09-28. Their records preserve the accepted
remediation outcomes. Remaining task state is defined by
[CLI Project Control](../../../../working/cli-development/project-control.md#active-task-ledger),
with unresolved candidates in the
[candidate queue](../../../../working/cli-development/tasks/potential/_potential.md).

The audit remains contextual provenance in Emerging pending a collection-wide
lifecycle decision. Its original reproductions and proposals describe the
audited version. Validate them against the current Task and implementation
before treating a reported defect as open. The earlier scope description below
does not reopen Tasks 30 or 31. The
[Memory currency audit](../../../memory-currency-audit.md) lists later
outcomes without changing the original observations.

## Historical audit scope

This scope holds the findings from driving the linked `open-forge` CLI as a user
across clean, dirty, pre-populated, relocated, and nested workspaces. Each
document states the exact reproduction, the observed output, and the proposed
result. The scope remains in Emerging while Task 30, Task 31, and the candidate
queue still contain open decisions or incomplete work derived from it.

## Evidence Status

- Lifecycle: **Emerging analysis**; this route remains contextual evidence for
  active Tasks while its actionable conclusions are still being checked.
- Authority: it does not define current product behavior, Task state, or
  implementation scope.
- Update rule: implementation discoveries belong in the active Task or a new
  Analysis record. Do not rewrite this scope to follow code drift.
- Retention: keep this scope in Emerging until all open conclusions have an
  owner and an explicit seal/archive or prune decision.

The audit ran against `0.0.0-dev.sha-62b0e23e` on `win-x64`, linked with
`npm run cli:link`. Test workspaces are outside the repository under
`<workspace>\open-forge-test\`.

The sequenced work these findings inform is not here. It lives in Working Memory as
[Task 30: CLI Experience Remediation](../../tasks/task30-cli-experience-remediation.md)
and [Task 31: Implementation Duplication Removal](../../tasks/task31-implementation-duplication.md),
which own execution order, phase state, decisions, and implementation reality.
This scope stays evidence; those records stay the work.

This scope covers **what is broken and how to fix it**. The separate
[CLI Design Retrospective](../../../../emerging/analysis/cli-design-retrospective/_cli-design-retrospective.md)
covers **how we got here** — separating the original Framework design from the
accepted contracts from what agents built. Findings here are actionable
regardless of that analysis; several of them would become unnecessary if a
Framework-level decision recorded there is changed.

## Historical Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.
- This contextual analysis may inform active Tasks but does not define current
  CLI behavior or execution state.

## Entries

- [Historical record: Proposed output for every command across each semantic status, with a three-tier view model and the shared rules that generate it](command-output-design.md) - #Memory #Analysis #Contextual #CLI #Presentation #Design #ProgressiveDisclosure #Archived #Historical
- [Historical record: Assessment of the C# design and style directives against the shipped CLI, the rules that already exist and are violated, the asymmetry that permits unmigrated duplication, and ranked refactoring targets](csharp-directives-and-structure.md) - #Memory #Analysis #Contextual #CLI #CSharp #Directives #Refactoring #Duplication #Archived #Historical
- [Historical record: Why doctor warnings read as info and info reads as debug — four different kinds of statement are emitted through one finding channel, measured by code](finding-model.md) - #Memory #Analysis #Contextual #CLI #Doctor #Findings #Severity #Presentation #Archived #Historical
- [Historical record: Every place YAML or Markdown is parsed by hand instead of by the libraries already depended on, with the measured defects each one produces](hand-rolled-parsing.md) - #Memory #Analysis #Contextual #CLI #Parsing #Markdown #Yaml #Correctness #Archived #Historical
- [Historical record: Measured survey of duplicated implementation across the CLI, covering identical method bodies, the four incompatible text escapers, duplicated constants, and the finding that type placement is already sound](implementation-duplication.md) - #Memory #Analysis #Contextual #CLI #Duplication #Structure #Refactoring #Escaping #Archived #Historical
- [Historical record: The wizard does not exist, confirmation is asked before the plan is shown, and error paths point at help instead of answers](interaction-layer.md) - #Memory #Analysis #Contextual #CLI #Interaction #Wizard #Help #Errors #Archived #Historical
- [Historical record: Why a standard SKILL.md package blocks the workspace, traced to the exact source rule, and why every diagnostic command reports a different cause](interoperability-and-diagnosis.md) - #Memory #Analysis #Contextual #CLI #Interoperability #Diagnosis #Skill #Routing #Archived #Historical
- [Historical record: Measured comparison of a three-band and a four-layer grouping for the CLI, what the tree already satisfies, and why cross-layer purity matters more than the intra-layer cycles](layer-adherence.md) - #Memory #Analysis #Contextual #CLI #Architecture #Layers #Dependencies #Testing #Archived #Historical
- [Historical record: The layer model the architecture document is missing, the directive clauses added for sharing and naming, and a considered argument for what should be done first](layers-and-sequencing.md) - #Memory #Analysis #Contextual #CLI #Architecture #Layers #Sequencing #Refactoring #Archived #Historical
- [Historical record: The exact-bytes baseline over generated Entries that makes extensions uninstallable, the missing authoring templates, and the naming and layering decisions for projection, granularity, and thresholds](lifecycle-baselines-and-architecture.md) - #Memory #Analysis #Contextual #CLI #Lifecycle #Fingerprint #Architecture #Templates #Naming #Archived #Historical
- [Historical record: Why models over-tag LoadNow and under-scope context, and what would make progressive disclosure a default habit rather than an axiom](loading-and-scope-discipline.md) - #Memory #Analysis #Contextual #Framework #Loading #Scope #ProgressiveDisclosure #Context #Archived #Historical
- [Historical record: Where full rendered-command snapshots belong, why the integration layer is the right home, the AOT constraint that decides the tooling, and what the snapshot mechanism actually needs](model-level-snapshot-testing.md) - #Memory #Analysis #Contextual #CLI #Testing #Snapshot #Presentation #Pipeline #Archived #Historical
- [Historical record: First-pass field audit of the presentation layer and the end-to-end suite, with measured output sizes and eight functional bugs](presentation-field-audit.md) - #Memory #Analysis #Contextual #CLI #Presentation #Testing #Audit #Archived #Historical
- [Historical record: Running the CLI against the Open Forge repository itself, plus the configuration file sprawl, the ungrantable permission model, path grammar drift, and where the engineering effort actually went](repository-dogfood-and-configuration.md) - #Memory #Analysis #Contextual #CLI #Dogfood #Configuration #Permissions #Naming #Archived #Historical
- [Historical record: A method for classifying every finding code into a severity, the division of labour between index and doctor, and the extension source and reference resolution findings that support it](severity-and-command-division.md) - #Memory #Analysis #Contextual #CLI #Doctor #Index #Severity #Taxonomy #Archived #Historical
- [Historical record: The mandatory Axioms section, the HTML comment markers the Markdig parser makes unnecessary, the CLI documentation carried in the loader, and a simplified v1 lifecycle record](structural-requirements-and-markers.md) - #Memory #Analysis #Contextual #CLI #Structure #Markers #Lifecycle #Loader #Archived #Historical
- [Historical record: Whether the shipped memory categories and scoping model match how the workspace is actually used, why implementers skip the framework after planning, and how much should ship by default](taxonomy-and-adoption.md) - #Memory #Analysis #Contextual #Framework #Taxonomy #Memory #Scope #Adoption #Extension #Archived #Historical
- [Historical record: Whether integration and end-to-end are still distinct layers, what the in-process entry point already covers, and the one class of behaviour only in-process testing can reach](test-layer-consolidation.md) - #Memory #Analysis #Contextual #CLI #Testing #Architecture #Layers #Archived #Historical
- [Historical record: Assessment of the three existing test layers and a proposed transcript-driven scenarios layer with a self-policing command and status coverage matrix](test-strategy-and-scenarios.md) - #Memory #Analysis #Contextual #CLI #Testing #Scenarios #Coverage #Archived #Historical
- [Historical record: Whether the render layer is the pure model-driven View it was meant to be, and a rebuilt test taxonomy based on where this codebase actually seams rather than on unit-integration-e2e convention](view-layer-and-test-architecture.md) - #Memory #Analysis #Contextual #CLI #Architecture #Presentation #Testing #Layers #Archived #Historical
