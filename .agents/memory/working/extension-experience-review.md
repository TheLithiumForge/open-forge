---
open-forge:
  description: Task 49 extension dependency audit, collaboration extraction, and forgiving CLI scenario review
  tags: [Memory, Working, Task, Contextual, Active]
---

# Extension And Experience Review

## Outcome

Task 49 follows the maintainer's 2026-09-19 request in order: audit every Extension, extract Adaptive Collaboration, commit a WIP checkpoint, then import and assess the supplied flows and scenarios. New end-to-end tests require the maintainer's later confirmation.

The CLI experience target is helpful continuation whenever useful work remains possible. Missing optional frontmatter should produce a warning and usable indexing, not stop the operation. This task records observed failures without implementing CLI behavior changes.

## Extension Audit

Read all six original manifests, READMEs, and all 40 runtime files. Checked Markdown destinations from installed locations against Core plus the transitive declared dependency closure, and reviewed literal paths and named capabilities in prose.

| Package | Runtime files | Local links | Missing runtime targets |
| --- | ---: | ---: | ---: |
| Development | 4 | 4 | 0 |
| Development Toolkit | 0 | 0 | 0 |
| Orchestration | 7 | 9 | 0 |
| Planning | 15 | 12 | 0 |
| Project Documents | 10 | 9 | 0 |
| Workflow Support | 4 | 3 | 0 |

Planning's references to an optional workspace transfer record do not require Orchestration. Project Documents' optional rationale does not require Planning's Decision convention. Workflow Support's empty recipe catalogue is intentional until other packages contribute recipes. Placeholder source fields in Templates are authoring prompts, not missing runtime files.

No broken runtime reference required a corrective task. One related repository Maintenance link still named the retired Memory Starters package; corrected it to the actual Template packages.

## Collaboration Placement

The optional `collaboration` package owns Adaptive Collaboration at its existing installed path and a Brainstorming Template under `templates/collaboration/`. It has no dependencies and remains outside the Development Toolkit.

Exploration and execution planning answer different questions. A new brainstorming Memory category would duplicate Ideas and Analysis. Use existing Emerging Memory scopes; an accepted outcome moves into the source that defines it. Neither installing Collaboration nor filling out its Template requires Planning, a Task, or an approval meeting.

Existing installations require an explicit ownership-aware migration because their previous Core copy may remain managed or edited. The package README states this boundary; this task does not invent automatic ownership transfer.

## Plan And State

1. Extension audit: complete; no missing runtime target.
2. Collaboration extraction: complete; source, dogfood, and focused verification agree.
3. WIP commit: `7b4745f7` (`wip - audit extension references and extract collaboration`).
4. Flow/template import and review: four Planning starters imported; all 24 supplied flows assessed, plus two recommended additions. All 438 scenarios have individual dispositions: 360 retained, 39 improved, 38 deferred and one redundant case omitted. The [assessment](../crystallized/documents/cli/experience/assessment.md), [compact validation list](../crystallized/documents/cli/experience/flow-review.md), and [run record](cli-experience-run.md) separate desired outcomes from observed behavior. The manual run and bounded evidence audit are complete: 26 flow records, 175 CLI calls in the canonical run, plus nine supplemental calls. Live-writer contention and explicitly listed alternatives remain unrun. No new automated tests or CLI fixes were implemented.
5. New scenario tests: withheld until maintainer validation.

### Existing Coverage Follow-up

While the maintainer reviews the flows, the [coverage audit](cli-experience-coverage/_cli-experience-coverage.md) maps all 26 flows, 438 scenario identities and 112 existing C# E2E methods. It separates complete individual-case assertions, partial and adjacent evidence, incompatible expectations, missing coverage, and selected lower-tier Integration evidence. None of the complete reviewed flows is established by the existing tests. This is static source inspection, not a new passing run or acceptance of the proposed scenarios. No test or CLI changes were made for this follow-up; step 5 still awaits validation.

Coverage-document validation: 438 unique scenario entries, 26 flow sections, 112 methods across 35 classes, 163 declared executions, and 1,207 local links checked. Exact source locations were checked for 201 distinct audit references. A bounded writing review identified and resolved one omitted published-method reference and ambiguous propagation of shared-scenario ratings into individual flow steps. `git diff --check` passed; no CLI, test, delivery-script or CI files changed during this follow-up.

Work is isolated on `codex/extensions-experience-review`. The starting source revision is `e11dfaf6e`. Preserve unrelated work in the original checkout. No merge or publication is part of this request.

## Verification

Dependency-closure inspection covers local Markdown links and reviewed prose, not arbitrary future user-created files. Runtime installation and fresh embedded-resource evidence are recorded here when completed. Existing package-driven snapshots may change when Core loses one file; these are existing fixture maintenance, not the deferred new scenario suite.

### Extraction Receipt

- Restored cached dependencies with `node scripts/delivery/cli.ts restore --offline`; no remote fetch.
- Built `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj -c Release --no-restore`: zero warnings/errors.
- Ran the Integration executable with `--parallel none --no-ansi --progress off --minimum-expected-tests 1` and six `--filter-class` selections: `*InstallOperationIntegrationTests`, `*UpdateBeforeOutputSnapshotTests`, `*StatusBeforeOutputSnapshotTests`, `*InstallBeforeOutputSnapshotTests`, `*EmbeddedFrameworkPayloadReaderIntegrationTests`, `*EmbeddedExtensionCatalogueIntegrationTests`. Final verification: 75 executed, 75 passed, no failures/skips.
- Updated 246 existing Install/Status/Update output snapshots explicitly and serially, then verified without update mode. Reviewed differences cover the removed asset, counts, context size, and six maintained fixtures. No new test was added.
- The Update partial-write fixture uses the Guidance entrypoint because it sorts before the deliberately blocked loader. The Status changed-file case uses that entrypoint; its missing-file case uses `CLAUDE.md`, preserving a missing managed-file warning without breaking startup navigation.
- Fresh managed CLI in this worktree completed Core install, Collaboration install, Doctor, and guidance context retrieval with exit 0 and no findings, under an isolated workspace and `OPENFORGE_DATA_HOME`.
- All 39 runtime Markdown links resolve in seven package closures. All three Collaboration runtime files match their dogfood copies byte-for-byte.
- Focused prose review found ambiguous catalogue wording and stale token measurements; both are corrected. Existing historical token counts are explicitly not current.
- `git diff --check` passed. Full suite and Native AOT execution were not performed; the new scenario suite remains withheld.
- Raw outputs are disposable under `.temp/astra-open-forge-2026-09-19/package-check/`; this tracked receipt retains the result.

## Experience Scope

The supplied pack is larger than estimated: 24 connected flows and 438 cases. All cases receive individual dispositions. The external proposed command-contract rewrite is not adopted. Desired UX changes remain explicit alongside the existing interface baseline. A fresh runner exercises practical main paths and records unavailable terminal, lock, and fault fixtures honestly.

The repository-only older Journey and Scenario starters are replaced by the four portable Planning starters, with useful expected-before-actual and cost guidance retained. Existing independently authored records remain unchanged apart from links to the successor templates.

All 43 current runtime Markdown links resolve across seven package dependency closures. The four new Planning templates match dogfood copies byte-for-byte, and all 12 supplied fixture files retain their exact original bytes. The new corpus and changed navigation links resolve. A focused writing review passed after correcting role placement, inconsistent continuation expectations, and unrelated fixture prerequisites. Testing guidance now permits multiple named snapshots per test/suite and reserves direct assertions for semantic facts and state effects.
