---
open-forge:
  description: Hands-on audit of the CLI experience layer covering interoperability, interaction, presentation, and per-command output design
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Experience, Presentation, Interaction, Interoperability, Audit]
---

# CLI Experience Audit

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
[Task 30: CLI Experience Remediation](../../../working/cli-development/tasks/task30-cli-experience-remediation.md)
and [Task 31: Implementation Duplication Removal](../../../working/cli-development/tasks/task31-implementation-duplication.md),
which own execution order, phase state, decisions, and implementation reality.
This scope stays evidence; those records stay the work.

This scope covers **what is broken and how to fix it**. The separate
[CLI Design Retrospective](../cli-design-retrospective/_cli-design-retrospective.md)
covers **how we got here** — separating the original Framework design from the
accepted contracts from what agents built. Findings here are actionable
regardless of that analysis; several of them would become unnecessary if a
Framework-level decision recorded there is changed.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.
- This contextual analysis may inform active Tasks but does not define current
  CLI behavior or execution state.

## Entries

- [Proposed output for every command across each semantic status, with a three-tier view model and the shared rules that generate it](command-output-design.md) - #Memory #Analysis #Contextual #Candidate #CLI #Presentation #Design #ProgressiveDisclosure
- [Assessment of the C# design and style directives against the shipped CLI, the rules that already exist and are violated, the asymmetry that permits unmigrated duplication, and ranked refactoring targets](csharp-directives-and-structure.md) - #Memory #Analysis #Contextual #Candidate #CLI #CSharp #Directives #Refactoring #Duplication
- [Why doctor warnings read as info and info reads as debug — four different kinds of statement are emitted through one finding channel, measured by code](finding-model.md) - #Memory #Analysis #Contextual #Candidate #CLI #Doctor #Findings #Severity #Presentation
- [Every place YAML or Markdown is parsed by hand instead of by the libraries already depended on, with the measured defects each one produces](hand-rolled-parsing.md) - #Memory #Analysis #Contextual #Candidate #CLI #Parsing #Markdown #Yaml #Correctness
- [Measured survey of duplicated implementation across the CLI, covering identical method bodies, the four incompatible text escapers, duplicated constants, and the finding that type placement is already sound](implementation-duplication.md) - #Memory #Analysis #Contextual #Candidate #CLI #Duplication #Structure #Refactoring #Escaping
- [The wizard does not exist, confirmation is asked before the plan is shown, and error paths point at help instead of answers](interaction-layer.md) - #Memory #Analysis #Contextual #Candidate #CLI #Interaction #Wizard #Help #Errors
- [Why a standard SKILL.md package blocks the workspace, traced to the exact source rule, and why every diagnostic command reports a different cause](interoperability-and-diagnosis.md) - #Memory #Analysis #Contextual #Candidate #CLI #Interoperability #Diagnosis #Skill #Routing
- [Measured comparison of a three-band and a four-layer grouping for the CLI, what the tree already satisfies, and why cross-layer purity matters more than the intra-layer cycles](layer-adherence.md) - #Memory #Analysis #Contextual #Candidate #CLI #Architecture #Layers #Dependencies #Testing
- [The layer model the architecture document is missing, the directive clauses added for sharing and naming, and a considered argument for what should be done first](layers-and-sequencing.md) - #Memory #Analysis #Contextual #Candidate #CLI #Architecture #Layers #Sequencing #Refactoring
- [The exact-bytes baseline over generated Entries that makes extensions uninstallable, the missing authoring templates, and the naming and layering decisions for projection, granularity, and thresholds](lifecycle-baselines-and-architecture.md) - #Memory #Analysis #Contextual #Candidate #CLI #Lifecycle #Fingerprint #Architecture #Templates #Naming
- [Why models over-tag LoadNow and under-scope context, and what would make progressive disclosure a default habit rather than an axiom](loading-and-scope-discipline.md) - #Memory #Analysis #Contextual #Candidate #Framework #Loading #Scope #ProgressiveDisclosure #Context
- [Where full rendered-command snapshots belong, why the integration layer is the right home, the AOT constraint that decides the tooling, and what the snapshot mechanism actually needs](model-level-snapshot-testing.md) - #Memory #Analysis #Contextual #Candidate #CLI #Testing #Snapshot #Presentation #Pipeline
- [First-pass field audit of the presentation layer and the end-to-end suite, with measured output sizes and eight functional bugs](presentation-field-audit.md) - #Memory #Analysis #Contextual #Candidate #CLI #Presentation #Testing #Audit
- [Running the CLI against the Open Forge repository itself, plus the configuration file sprawl, the ungrantable permission model, path grammar drift, and where the engineering effort actually went](repository-dogfood-and-configuration.md) - #Memory #Analysis #Contextual #Candidate #CLI #Dogfood #Configuration #Permissions #Naming
- [A method for classifying every finding code into a severity, the division of labour between index and doctor, and the extension source and reference resolution findings that support it](severity-and-command-division.md) - #Memory #Analysis #Contextual #Candidate #CLI #Doctor #Index #Severity #Taxonomy
- [The mandatory Axioms section, the HTML comment markers the Markdig parser makes unnecessary, the CLI documentation carried in the loader, and a simplified v1 lifecycle record](structural-requirements-and-markers.md) - #Memory #Analysis #Contextual #Candidate #CLI #Structure #Markers #Lifecycle #Loader
- [Whether the shipped memory categories and scoping model match how the workspace is actually used, why implementers skip the framework after planning, and how much should ship by default](taxonomy-and-adoption.md) - #Memory #Analysis #Contextual #Candidate #Framework #Taxonomy #Memory #Scope #Adoption #Extension
- [Whether integration and end-to-end are still distinct layers, what the in-process entry point already covers, and the one class of behaviour only in-process testing can reach](test-layer-consolidation.md) - #Memory #Analysis #Contextual #Candidate #CLI #Testing #Architecture #Layers
- [Assessment of the three existing test layers and a proposed transcript-driven scenarios layer with a self-policing command and status coverage matrix](test-strategy-and-scenarios.md) - #Memory #Analysis #Contextual #Candidate #CLI #Testing #Scenarios #Coverage
- [Whether the render layer is the pure model-driven View it was meant to be, and a rebuilt test taxonomy based on where this codebase actually seams rather than on unit-integration-e2e convention](view-layer-and-test-architecture.md) - #Memory #Analysis #Contextual #Candidate #CLI #Architecture #Presentation #Testing #Layers
