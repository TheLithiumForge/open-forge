---
open-forge:
  description: Approved beta Template and package changes, verification results, and the remaining manual review
  tags: [Memory, Task, Contextual, Template, Extension, Beta, Archived, Historical]
---

# Beta Templates And Extension Packages

## Outcome And Acceptance

The maintainer approved implementation on 2026-09-23 after reviewing the proposed templates, package split, names, and descriptions. The [current catalogue](../../../../src/extensions/README.md) defines the resulting selection. The [package decision](../../crystallized/decisions/extensions/focused-extension-packages.md) records the accepted boundaries. Qualitative review belongs to the maintainer; no claim of measured improvement in agent behavior is made.

The initial template implementation was prepared on `codex/beta-core-templates`, based on `1de63190`, with no CLI changes. The subsequent accepted Core review below extends that work into framework wording and CLI behavior. The original checkout's Directive clarification is preserved there and incorporated in this branch's installed and dogfood rules.

## Accepted Core Review Follow-Up

On 2026-09-23 the maintainer approved R1–R7 in one pass with separate commits, then authorized GPT-6 Luna workers through Worker Watch. The accepted boundaries are:

- R1: author no-local-rule entrypoints with `## Axioms` and the inherited marker. Keep missing and empty local sections valid input, preserve ancestor rules, and remove Doctor's false invalid classification.
- R2: retain `removedCategories` and add exact `removedFiles` exclusions in `.agents/open-forge.json`. Framework install and update must not recreate or rewrite excluded destinations. The lists record intent; they do not delete files or infer removal from absence. Remove an exclusion deliberately before requesting restoration.
- R3: distinguish editing/replacing one source or using its overwrite from adding an independent binding instruction. Directive leaves supply Instructions, not Axioms.
- R4: allow justified Pattern adaptations and ordinary scoped Memory organization within granted authority. Required shapes and changes to the top-level Memory states keep their authority boundaries.
- R5–R7: remove inherited repetition from template catalogues and Core, keep distinct category behavior, and use concrete wording for refreshing work context and identifying sources.

The principal owns shared Core meaning and integration. Three isolated workers own Axioms validation, removal settings, and template cleanup. They start from the same baseline plus the staged initial beta changes, leave those staged changes untouched, and return only their additional changes. No concurrent writer shares a worktree. No worker may commit or publish.

The change affects local authored files and managed Framework install/update behavior. Git and existing mutation recovery provide recovery; hostile same-user processes remain outside the supported boundary. The implementation reuses the existing JSON reader, path model, settings observations, payload selection, mutation planning, and isolated test workspaces. No new dependency, custom parser, or platform workaround is planned. Unit evidence covers classification and settings; integration evidence covers real installation/update and preservation. The new settings surface triggers aggregate managed and Windows x64 Native AOT verification after integration. Evidence below from the earlier template-only snapshot does not prove the new changes.

## Delivered

- Core Templates supplies one starter for each Core category plus Memory. The Skill starter contains a native SKILL.md file inside a routed wrapper; it does not install a new capability.
- Flows and Scenarios supplies User Flow, Scenario, Scenario Collection, and Run Record independently of Planning.
- Observations and Handoffs supplies its two Memory categories and starters without development dependencies. The Memory category paths are unchanged.
- Task Coordination keeps `orchestration` as its ID and now depends on Observations and Handoffs. Development Toolkit adds `scenarios` directly. Core Templates stays outside the Toolkit.
- All ten package descriptions match the approved plain-language wording. Package source revisions are `0.4.0`.
- All 27 packaged starter leaves match their repository copies. Fifteen older starters now expose their instructions as removable prompts. Task gains a Result section, and Run Record supports non-software work.
- The Template copy rule preserves Template classification when the new artifact is itself a Template. Other copied artifacts discard source-only classification.
- Current documents, package guidance, links to moved starters, and affected generated Entries are aligned. Existing independently maintained records are not rewritten into new templates.

These changes implement only the accepted beta scope. They do not settle the broader open Local Planning review or introduce its proposed status system, permanent IDs, execution budgets, or archival procedure into the packages.

## Core Review Results

All seven accepted recommendations are implemented. New no-local-rule entrypoints retain the explicit inherited Axioms marker; existing missing or empty local sections remain valid, while the Loader still requires its binding rules. Doctor and Route Inspect agree on this distinction.

Framework Install and Update honor both removal lists before selecting whole-file and generated-region effects. Exact file exclusions also cover root managed blocks. Malformed settings stop mutation, and an excluded missing ancestor needed by another selected route produces a blocker. The lists do not delete content or change Extension lifecycle selection.

The Loader and category documents now distinguish source customization from additive binding instructions, explain justified Pattern adaptation, and allow ordinary scoped Memory organization within granted authority. Template catalogues keep only their own local requirements. Core acceptance, capture, and autonomy rules have fewer repeated statements, and loading/context prose uses concrete terms. The independent prose review found one dictionary definition that still described only updates; it now matches the Loader's install/update/remove wording.

Default startup measurement changes from approximately 5,676 to 5,696 tokens (about 0.4%). The explicit inheritance and customization rules are retained for clarity; this work does not claim an overall token reduction or measured improvement in agent adherence.

## Initial Template Verification

Evidence was produced from this worktree on Windows x64 with .NET SDK `10.0.101`, Node `26.3.1`, and Python `3.13.14`. Dependencies restored offline. Managed builds completed with zero warnings and zero errors.

```powershell
node scripts/delivery/cli.ts restore --offline
dotnet build src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj -c Release --no-restore -v quiet
& ./artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.exe --parallel collections --no-ansi --progress off --minimum-expected-tests 1 --filter-class '*EmbeddedFrameworkPayloadReaderIntegrationTests' --filter-class '*EmbeddedExtensionCatalogueIntegrationTests' --filter-class '*RouteCreateTemplateIntegrationTests' --filter-class '*RouteUpdateTemplateIntegrationTests'
& ./artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.exe --parallel collections --no-ansi --progress off --minimum-expected-tests 1 --filter-class 'OpenForge.Cli.IntegrationTests.Commands.Install.InstallOperationIntegrationTests'
```

The first selection executed 14 tests and the second executed 8: all 22 passed, with no failures or skips. Embedded-resource tests compare exact authored package and Framework assets with the compiled resources. The fresh `OpenForge.Cli.Framework.dll` SHA-256 was `14a410daa824c4670600d514e8966778923a12f2067e15e1661670b03de1a66c`.

The managed development CLI under `artifacts/publish/open-forge-dev/Release/` exercised each of the ten package selections in a separate disposable workspace. Every call set `OPENFORGE_DATA_HOME` to an isolated test directory. For each selection:

1. Run `install --automatic`.
2. Run `extension install <package-id> --automatic` using the freshly embedded catalogue.
3. Check every expected installed package path against that selection's exact dependency closure and check that unrelated package paths are absent.
4. Run `doctor`: zero findings for all ten fresh installations.
5. Run `index --dry-run`: zero changed files. For selections containing Workflow Support, explicitly check `.agents/skills/use-workflow/references/_references.md` too.

All calls supplied the explicit disposable `--workspace`, `--format json`, and `--detail full`. The static package audit found no ownership collisions, no hidden HTML instructions in packaged Templates, and no missing targets among 50 runtime Markdown links inside their assembled dependency closures. Links in changed documents also resolve.

An independently created Memory file used the Core Memory starter through `route create --template`, with destination-specific tags. Its exact bytes survived `extension update core-templates` and `extension remove core-templates`. A native Skill extracted from the fenced starter indexed successfully with native metadata and no Template wrapper; Doctor reported zero errors and warnings, plus one informational retained recovery bundle from the preceding removal. Native harness activation was not tested.

## Core Review Verification

The final Windows x64 gate passed after integrating all three worker slices and the principal's Core changes:

```powershell
npm run build:native -- --no-restore
npm run test:built
```

Managed builds and Native AOT publication completed with zero warnings and zero errors. The six final execution modes reported:

| Mode | Passed | Platform exclusions |
| --- | ---: | ---: |
| `unit` | 3,348 | 0 |
| `integration` | 2,354 | 17 |
| `public` | 246 | 0 |
| `native-integration` | 2,354 | 17 |
| `native-public` | 246 | 0 |
| `public-native` | 246 | 0 |

The integration exclusions require operating-system behavior unavailable on Windows. The managed/native modes reuse the same test populations; their sum is not a count of distinct cases. The gate checked source and artifact identity before and after execution.

Earlier aggregate runs exposed stale expectations for the approved catalogue, context-size measurements, the Toolkit dependency set, and the old Guidance payload hash. Only reviewed differences were accepted: 116 snapshot files in five cases, the explicit dependency expectation, the Planning/Collaboration journey's paths and versions, and the Guidance byte hash. The Planning journey now retrieves Plan and Task and keeps an exact inventory that excludes the separated scenario files. Focused reruns passed before the final aggregate gate.

The final gate source was `17ec3edb6b1b04a9f3891aa41550a7a1ba535e28` plus the one-line Guidance expectation committed with this record; its uncommitted execution-source fingerprint was `c48a1bef09a30140353efabbcb8916b2ea2aced223d0e96676f756fb5b1e12ea`. The final native CLI SHA-256 was `44284754cb6655f92622b10b148f3d66edc60f5852a0deb80f19e8fe8f5feb17`. No execution source changed after that gate.

A separate black-box run of the combined managed CLI verified missing category/file exclusions, existing-file byte preservation, generated-region preservation, force/prune, reinstall, explicit opt-in restoration, and malformed settings rejection with no workspace mutation. Each disposable workspace used an isolated `OPENFORGE_DATA_HOME`. Completed recovery bundles were previewed and cleaned through the CLI before reinstalling.

The combined CLI also ran 46 fresh package checks: all ten package selections installed their exact dependency closure, reported zero Doctor findings, and needed no Index changes. Static checks confirmed all 27 starter copies and all 50 runtime Markdown links. Qualitative template quality and native harness activation remain outside these automated checks.

## Existing Installation Migration

The [public migration instructions](../../../../docs/extensions.md#moving-from-the-earlier-package-layout) distinguish a fresh install from an existing ownership transition.

A disposable fixture installed the former Toolkit and Orchestration from the baseline catalogue and created an independent Handoff. The checks established:

- Direct update did not establish the new ownership automatically.
- Removing a category with a retained user descendant was blocked, preserving the record.
- Holding the unmanaged Handoff under the existing Working parent allowed old-owner removal without deleting the record.
- Retained recovery bundles blocked reinstallation until reviewed cleanup.
- Updating and retaining shared Workflow Support during the partial transition led to a reproducible install block reporting its catalogue changed after planning. This partial path is not qualified; no CLI correction was attempted in this content task.
- Removing the complete old dependency set, cleaning up the recognized completed recovery bundle, and installing the desired new Toolkit and Task Coordination completed. The Handoff moved back with exactly its original bytes. Final Doctor: no problems.

The complete old set was `development-toolkit`, `orchestration`, `planning`, `development`, `project-documents`, and `workflows`. Removal was previewed before apply; no force or prune option was used. This verifies the described fixture, not automatic migration of arbitrary edited workspaces.

## Limits And Next Action

The repository-wide Doctor is not clean. It reports existing navigation problems, including ambiguous `beta-follow-ups` routes, and managed content divergence. The tracked repository ownership record still describes the older packages; moving its starter files adds three missing-owned-template findings, and the new package versions differ from recorded installations. That record was not hand-edited or treated as permission to remove the repository's user records. The repository's source copies and navigation are updated, but managed migration of this populated dogfood installation remains separate from this content change.

The managed and Windows x64 Native AOT gates passed as recorded above. Other platforms, native harness activation, and qualitative agent trials were not run. The maintainer requested manual template review. Review the Core Templates catalogue, specialized starters, package descriptions, and migration guidance before release. Raw local execution reports are disposable; this record retains the commands, results, and limitations needed to assess the change.
