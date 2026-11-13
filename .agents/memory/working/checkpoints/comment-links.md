---
open-forge:
  description: Issue 21 comment-link implementation and focused verification state
  tags: [Memory, Working, Checkpoint, CLI, Contextual]
---

# Comment Links
Comment Links is the issue 21 feature that connects code and docs through path comments.

For example, `FindUniverseResolver.cs` points to the Find source-universe contract, and that contract points back to the class. The [source-reference contract](../../crystallized/documents/cli/contracts/shared/source-references/interface.md#comment-links) defines the grammar and resolution rules.

Status: implementation complete, CI pending.

## Changes

The feature recognizes `@open-forge` and `@rune` across common comment delimiters. Plain targets start at the repository root. `./` and `../` start next to the containing file. Markdown HTML comments and ordinary Markdown links share fragment checks.

References includes comment occurrences in both directions and accepts exact code paths. Context checks comments during requested link expansion. Doctor reports broken targets, fragments and legacy IDs with path suggestions. Repair changes only the target word and keeps the surrounding bytes.

Suggestions rank Git file moves, Git heading changes, matching old section content and unique heading slugs. Doctor keeps suggested fragments and labels weak slug matches. Repair keeps that order and requires selection for weak matches.

Contracts were committed before implementation. Public updates cover `docs/cli.md`, the Docusaurus pages, References help and both CLI Skill copies. The 66 audited code-link occurrences across 15 historical docs are fixed. The three `RouteSourceStructureReader.cs` citations are included in that count. The completed fragment checkpoint is archived under CLI Development Tasks.

## Execution

The shared Sources layer handles recognition, resolution and candidate facts. The host reads Git history through the existing bounded process runner. Command code maps the shared results. Comment discovery gives code files no navigation or metadata authority.

The feature uses the current Markdig parser, BCL file reads and shared resolver. It changes no dependency or runtime pin. Existing containment, expected-byte, protection and recovery checks apply to repairs. Same-user hostile namespace changes stay outside the existing safety model.

## Verification

The standard build reports NU1903 for the existing `Microsoft.DotNet.ILCompiler` 10.0.1 pin. Local verification keeps that audit warning visible through a command-only `WarningsNotAsErrors=NU1903` override. Project settings and pins are unchanged. The final build uses frozen sources and disables incremental compilation:

```powershell
dotnet build OpenForge.Cli.slnx --configuration Release --no-restore --no-incremental -p:OpenForgeSkipDevelopmentPublish=true -p:WarningsNotAsErrors=NU1903
```

Local results:

| Check | Result |
| --- | --- |
| Managed solution build | Pass, 3 NU1903 warnings |
| Unit `Feature=references` | 122 passed |
| Unit `Feature=context` | 53 passed |
| Unit `Feature=repair` | 122 passed |
| Unit `Feature=doctor-command` | 154 passed |
| Integration `Feature=comment-links` | 13 passed |
| Integration `CodeLinkFragmentSnapshotTests` | 2 passed |
| Integration `CommandHelpIntegrationTests` | 36 passed |
| Integration `StatusBeforeOutputSnapshotTests` | 5 passed |
| Integration `RepairProtectionIntegrationTests` | 21 passed |
| Integration `RepositoryMarkdownTests` | 13 passed |
| Docusaurus build | Pass |
| `npm ci`, repository and site | Pass |

All focused test selections have zero skips. Full Unit, Integration, EndToEnd and Native AOT suites have not run locally. CI is the full-suite gate under the lane instruction.

The test commands run the built assemblies with a feature or exact class filter, `--minimum-expected-tests`, `--fail-warns on` and `--fail-skips on`. Logs are kept locally under `artifacts/comment-links-*.log`. No flakes were seen. Earlier failures found scanner, finding-inventory, protection and candidate-output defects. Minimum-count checks also caught stale incremental test output. The final clean build and focused results replace those failed attempts.

## Snapshots

Reviewed and accepted output:

| Group | Files | Reason |
| --- | --- | --- |
| `CommentLinkOutputSnapshotTests` | 30 new | Legacy paths, comment rows and a weak heading suggestion with its fragment |
| `CodeLinkFragmentSnapshotTests` | 8 updated | Ordinary code files use the manual fragment action without false protection |
| `CommandHelpIntegrationTests`, References | 1 updated | Comment grammar and exact code operands |
| `StatusBeforeOutputSnapshotTests` | 56 updated | Skill payload byte, character, token and startup-share totals |

Received output stays under `artifacts/command-output-snapshots/<run>/<Class>/<Group>/`. The accepted snapshots verify in ordinary runs.

## First CI Fix

The overseer opened PR #27 at `9e6b9203b5082299c259d05e57f43af9f3999c98`. Run `43401811949` reports five Integration failures in Linux job `131810132481`. Linux x64 was the only failed platform job when this follow-up started. The maintainer selected the finding catalogue and guidance snapshot failures for this fix.

The Doctor, References and Context Findings catalogue tables now list all three comment-link finding codes. The production definitions added only `reference.legacy-anchor`, `references.legacy-anchor` and `context.legacy-anchor`. The invariant test passes without a new catalogue exception.

The reviewed Linux received snapshot adds the Comment links section and the References comment-link sentence. One shared `custom-guidance.content.txt` expectation feeds the Windows, Linux and macOS adapters. Its update passes the Windows payload-rendering test and the existing platform-comparer checks. No runtime, shipped payload or test implementation changed.

Follow-up checks:

| Check | Result |
| --- | --- |
| Integration `CliReportInvariantsTests` | 9 passed |
| Integration `CustomGuidanceMatchesReviewedTemplates` | 1 passed |
| Integration `RepositoryMarkdownTests` | 13 passed |
| Unit `PlatformSnapshotComparerTests` | 7 passed |
| `npm run check:dotnet:whitespace` | Pass |
| `npm run check:dotnet:style` | Pass |
| Index preview for the three contracts | Entries current |

All focused selections have zero skips. Both formatting commands report workspace-loading warnings and exit successfully. No full local suite or native suite ran. The snapshot update accepts one shared expectation for all three supported platform adapters. Local logs use the `artifacts/pr27-` prefix.

The other Linux failures concerned included-file Doctor output, reference read counts and custom-guidance path checks. They were outside the first two selected fixes. That follow-up made no push.

## Second CI Fix

The next maintainer assignment selects all three remaining failures from run `43401811949`. The fix reuses the current read session, layout mapping and snapshot comparer. Git provides recovery. No dependency, runtime pin, wire shape or new platform adapter is selected. Full suites and Native AOT stay with CI.

Comment discovery now reuses retained diagnosis text, including `AGENTS.md`. Parsed source documents keep their existing cache and identity. The reported five-versus-seven assertion counts file acquisitions. It does not count parses. One extra probe is necessary because the fixture contains an unseen ownership-marker file, which discovery must check for annotations. The corrected expectation is six reads, `acquiredBefore + 2`: one target read and one unseen-file probe. The parser callback stays at zero. Both retained-catalogue and parsed-document identity assertions pass. A second Integration case checks host-document reuse directly.

The comment example entered the shipped CLI Skill through `references/common.md`. `WorkspaceLayoutPayloadGuidance.ExampleCommentTargetPath` now maps its context path with the selected layout. The reviewed custom-root example uses `docs/context/guidance/style.md#naming`. The existing default-byte and authored-guidance preservation checks pass.

Diagnostics were downloaded with `gh run download` into `artifacts/pr27-round2/` for Linux x64, macOS x64 and macOS arm64. Their ten included-file Doctor views agree. Fresh Windows output also matches all thirty Unix receipts through the existing `PlatformSnapshotComparer`. Seven shared Doctor expectations are updated for the correct missing-target message and manual edit advice. Three other views already matched. One shared guidance expectation is updated for the mapped example. Separate platform cases are unnecessary because the existing comparer validates every received view.

Final local results:

| Check | Result |
| --- | --- |
| Managed solution build | Pass, existing NU1903 command-only override |
| Integration `LocalReferenceInventoryWorkTests` | 2 passed |
| Full Integration `*References*` selection | 105 passed |
| Full Integration `*Doctor*` selection | 188 passed |
| Integration `WorkspaceLayoutPayloadRenderingIntegrationTests` | 6 passed |
| Integration `IncludedFileReportingIntegrationTests` | 7 passed |
| Integration `CliReportInvariantsTests` | 9 passed |
| Integration `RepositoryMarkdownTests` | 13 passed |
| Unit `PlatformSnapshotComparerTests` | 7 passed |
| `npm run check:dotnet:whitespace` | Pass |
| `npm run check:dotnet:style` | Pass |
| Source-reference contract Index preview | Entries current |

The focused runs total 330 Integration tests and seven Unit tests, with zero skips. Both formatting commands exit successfully with workspace-loading warnings. Eight reviewed shared snapshots are accepted. No flakes were seen. Unix execution was unavailable, so Unix evidence comes from the downloaded CI receipts and comparison through the existing adapter. The follow-up makes no push.

## Handoff

The branch is `feature/comment-links`, stacked on `fix/code-link-fragments` at `c41e95b1b`. Changes stay in this worktree. The original lane made no push, merge or PR. The overseer later opened PR #27 and ran CI. The existing NuGet audit failure needs maintainer follow-up if it blocks CI.

## Spec Review

The maintainer selected review findings A1 through A6 for correction after `a299037d1`.
The changes stay in comment recognition, suggestion evidence, Repair and their contracts.
The CLI edits local authored files through the existing expected-byte, containment,
verification and recovery pipeline. Git and recovery bundles provide the existing
recovery limits. The supported same-user threat model stays unchanged.

The fixes reuse the shared destination encoder, parsed Markdown headings, source
locations, Git process runner and Repair pipeline. No dependency or runtime change
is needed. Exceptional machinery: none. The existing product marker scanner gains
file-syntax selection. Git target diffs give changed line ranges to the shared
heading matcher.

The local evidence is the touched pure scanner and finite-mapping tests, the new
code-string exclusion, encoded comment repair and heading-diff Integration cases,
and reviewed Repair and guidance snapshots. Only touched or created tests run
locally under the updated lane rule. CI covers complete managed and native suites.

The review corrections are complete:

| Finding | Correction | Regression evidence |
| --- | --- | --- |
| A1 | Decode the selected comment path once, then use the shared path encoder | `TargetWithSpaces` checks raw and encoded selections, spaces and a literal percent sequence, preview, application, byte preservation and fresh diagnosis |
| A2 | Select comment delimiters by file syntax and allow trailing comments | `TrailingCommentSyntax` covers VB, TeX and C# decrement followed by a comment. Updated `CommentStyles`, `BlockLines` and `Mentions` keep the existing scanner cases |
| A3 | Admit native Markdown images only from Markdown files | `CodeStringImage` keeps one real comment reference and excludes the JavaScript string image from the Doctor view and References |
| A4 | Read target diff hunks and pair unique changed parsed headings before section-content evidence | `HeadingDiff` covers changed and empty bodies. `MoveAndHeading` and `SectionSuggestions` keep Git, section-content and slug evidence distinct |
| A5 | Correct References source-universe and Repair exact-relink contracts | Grammar examples cover code sources, target-kind fragments and encoded repository-root comment replacements. Updated docs, site, help and both CLI Skill copies agree |
| A6 | Keep Git rename, Git heading change, section-content and weak unique-slug kinds through Repair | Mapper assertions cover all four kinds. `FiniteValuesMapExactly` covers their machine names. `WeakSlugRepairChoice` snapshots the weak choice label |

The local managed gates use only touched or created tests:

| Check | Result |
| --- | --- |
| Solution build | Pass, 3 existing NU1903 warnings with the command-only override above |
| Unit selected methods | 16 passed, zero skips |
| Integration selected comment-link methods and guidance snapshot | 11 passed, zero skips |
| Integration Repair help case | 1 passed, zero skips |
| `npm run check:dotnet:whitespace` | Pass, workspace-loading warning |
| `npm run check:dotnet:style` | Pass, workspace-loading warning |
| Scoped contract Index preview | Entries current in all three affected catalogues |
| Changed local Markdown links and Skill parity | Four links resolve. Both copies match |

Accepted snapshots are ten new `WeakSlugRepairChoice` views, the shared custom-guidance
view and the Repair help page. The Repair views label the unique slug weak at every
choice view and in full JSON reasons. They show no selected effect or recovery
bundle. The guidance and help changes describe file-syntax comments and encoded
repository-root relinks. Existing platform comparison handles these shared baselines.

No flakes occurred. The first focused run exposed a suppressed cross-document
section-content suggestion. The correction keeps that suggestion after the new
Git heading-diff candidate. Missing or changed snapshot baselines were reviewed
before acceptance. No full local suite, native gate, Unix run or push was used.
There are no deviations or open maintainer questions. The maintainer controls the
later rebase onto PR 25 and the CI gate.

## EndToEnd CI Fix

The maintainer selected the default-layout fixture and F08 proven-rename failures
from run `43420481673`. The two named EndToEnd methods are the local test scope.
The same local authored-file safety and recovery limits apply. The fixes use the
existing install capture, rename facts and Doctor classification. No dependency,
runtime or new exceptional machinery is needed. CI runs full managed and native
suites. The next CI run on `223c977f7` is `43426798966`.

The default-layout capture changes only the CLI Skill's `common.md` and
`discovery.md` bytes. All other twenty captured files match. The fixture keeps its
format and original output and ownership expectations. Issue 29 still covers its
later replacement.

Doctor skipped a retained proven rename when the shared comment-link reader also
offered Git evidence. The fix gives the retained proof precedence and keeps the
original exact-repair expectation. The completed Linux logs also report both
ambiguous-successor cases because their old literal reason arrays omit the new
Git and section evidence. Their choice and effect requirements stay unchanged.

The two named methods pass on the fresh Windows managed publication: four cases,
zero skips. The new ambiguity snapshot keeps both candidate paths and all reasons.
It comes from the previous Linux CLI receipt, with Git and section-content labels
updated to the distinct evidence kinds from `223c977f7`. Only the fixture's known
Git HEAD is replaced. The ambiguous cases haven't run locally because the
maintainer limited local EndToEnd execution to the two named methods.

Run `43426798966` also reports four Status snapshot failures. The Skill prose grew
by 280 characters and bytes, which adds 70 estimated routed tokens. The accepted
54 Status views change only those totals and the corresponding startup-share
percentages. The share uses current startup bytes divided by routed bytes. All
other structured fields stay equal. The four affected Status Integration cases
pass locally with zero skips.

The downloaded diagnostics from all six platforms match all 960 Status views
through the existing platform comparer. The Windows x64 receipts gave the reviewed
shared baselines. No comparer or platform-specific expectation was added.

The prior run is completed with a cancelled conclusion. Its retained failures on
Linux and Windows are the same fixture, proven-rename and ambiguous-reason cases.
The new run completed with failure. All six platform jobs report only the same
four Status failures, including macOS Intel. These stop the run before its
EndToEnd stage. Documentation run `43426798812` passed. Both completed Build runs
were read again through `gh run view`, including their failed logs.

The solution build includes a fresh managed development publication and uses the
existing command-only NU1903 override. EndToEnd source compilation also passes.
Both formatting checks pass. `git diff --check` and LF checks pass. No flakes
occurred. No full local suite, native build, Unix execution or push was used.
The fixes restore the existing exact-repair contract. There is no product scope
deviation. The ambiguous-successor snapshot is compiled and reviewed against the
retained Linux receipt and current evidence labels. Its execution stays with CI
because the maintainer selected only the two named EndToEnd methods locally.
The maintainer controls the push and next CI run.

## PR 25 Rebase

The maintainer requested a rebase onto `1f7b1f906`. The clean predecessor was
`f89b9b36f`. The local backup branch
`backup/comment-links-pre-pr25-rebase-20261113` keeps that predecessor. All eight
comment-link commits were replayed. The replayed head before regenerated
expectations is `b1a5e769e`.

This combines accepted behavior in a local developer CLI. Git and regeneration
provide recovery. The supported failure and same-user threat limits stay
unchanged. Existing typed projections, Imprint and the current fixture format
provide the required capabilities. There is no dependency, runtime, architecture
or mutation-safety change. Exceptional machinery: none. Full managed and native
gates stay with CI under the lane rule.

References help keeps comment syntax and the manual fragment correction. The
archived fragment checkpoint keeps PR 25's corrections and issue 21's completed
historical-link work. The automatic merges keep checked code links outside
selected Context bodies, physical overwrite locations, References
`subject.location` and manual actions in both commands. Both CLI Skill copies
match. The merged contracts, user docs and site describe both branches' behavior.

The two fragment test methods now also cover comment links. Four Context cases
check valid ranges and symbols, invalid fragments and an overwrite source. One
References case checks both markers, both path bases, source locations and manual
actions. Semantic assertions pass before snapshot acceptance.

Conflicting generated expectations were kept temporarily from the new base, then
replaced with fresh received output from the complete rebased tree. The accepted
111 views are:

| Suite | Views | Review |
| --- | --- | --- |
| `CodeLinkFragmentContextTests` | 36 new | Valid comment targets stay complete without code bodies. Invalid targets name the authored location and manual action |
| `CodeLinkFragmentSnapshotTests` | 10 new | Both comment findings keep `subject.location`, the exact destination and manual actions |
| `CommentLinkOutputSnapshotTests` | 8 updated | The legacy finding now includes its existing authored coordinates in the subject and text heading |
| `StatusBeforeOutputSnapshotTests` | 56 updated | Only routed totals and startup percentages change. The two added Skill sections add 2265 characters and bytes. Token totals use the existing rounding rule |
| References help | 1 updated | Comment grammar and PR 25's manual-correction guidance are both present |

The default-layout fixture was regenerated through a fresh install of the rebased
managed publication. Only the two CLI Skill reference byte entries change. The
other twenty installed files match exactly. The fixture format, text output,
JSON output, paths and ownership expectations stay unchanged. Issue 29 still
covers its replacement. The custom-root guidance snapshot merges both changes
and passes without another update. No generated number or byte sequence was
merged by hand.

The Windows managed gates pass with zero failures and skips:

| Gate | Result |
| --- | --- |
| Solution build with fresh development publication | Pass, four existing NU1903 warnings with the command-only override |
| EndToEnd fixture compilation | Pass, two existing NU1903 warnings with the same override |
| Unit comment scanner | 17 passed |
| Integration affected projections, comments, inventory, Status and payload rendering | 50 passed |
| Integration Context and References help | 2 passed |
| Integration report catalogue and repository Markdown | 2 passed |
| EndToEnd F08 proven and ambiguous renames, References outgoing and Context fragment journeys | 6 passed |
| EndToEnd default-layout compatibility | 2 passed |
| `npm run check:dotnet:whitespace` | Pass, existing workspace-loading warning |
| `npm run check:dotnet:style` | Pass, existing workspace-loading warning |
| Docusaurus build | Pass |
| Scoped Context, References and archived-checkpoint Index previews | Entries current in all three parents |

Both ambiguous-successor cases now run locally under the rebase selection and
match their reviewed evidence snapshot. The earlier local execution limit no
longer leaves those cases unexecuted. The initial verification mismatches were
new or stale snapshots. There were no timeout failures or flakes. No full local
suite, native build or Unix execution was used. There is no product deviation or
open maintainer question. The maintainer controls the force-push and CI gate.

Reproduce the managed build and focused tests from this worktree:

```powershell
dotnet build OpenForge.Cli.slnx --configuration Release --no-restore -p:WarningsNotAsErrors=NU1903
dotnet build src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj --configuration Release --no-restore -p:OpenForgeSkipDevelopmentPublish=true -p:BuildProjectReferences=false -p:WarningsNotAsErrors=NU1903
dotnet artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests.dll --filter-class '*SourceCommentLinkReaderTests' --minimum-expected-tests 16 --fail-warns on --fail-skips on --no-ansi --progress off
$integrationMethods = @(
    '*CodeLinkFragmentContextTests.CodeFragments',
    '*CodeLinkFragmentSnapshotTests.CodeFragments',
    '*CommentLinkIntegrationTests.*',
    '*CommentLinkOutputSnapshotTests.*',
    '*ContextCommentLinkTests.HtmlCommentExpansion',
    '*ReferencesBeforeOutputSnapshotTests.Direction',
    '*ContextOperationTests.FollowsLinksBreadthFirstWithoutFetchingOrRepeatingSources',
    '*ContextOperationTests.MissingFragmentIsIncompleteAndUnresolved',
    '*ContextOperationFindingTests.SameCodeLinkFindingsRetainAuthoredOrder',
    '*ContextLinkExpansionSeedTests.ZeroReasonSeedStillSuppliesTraversalLinks',
    '*LocalReferenceInventoryWorkTests.*',
    '*StatusBeforeOutputSnapshotTests.HealthyWithExtension',
    '*StatusBeforeOutputSnapshotTests.RecoveryBundlePresent',
    '*StatusBeforeOutputSnapshotTests.InstalledWorkspace',
    '*WorkspaceLayoutPayloadRenderingIntegrationTests.*'
)
dotnet artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll --filter-method $integrationMethods --minimum-expected-tests 50 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll --filter-display-name '*contract(command: "context")' '*contract(command: "references")' --minimum-expected-tests 2 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll --filter-method '*CliReportInvariantsTests.NativeJsonSnapshotsSatisfyEnvelopeMembershipScalarAndCatalogueRules' '*RepositoryMarkdownTests.RepositoryMarkdownGateHasNoDiagnostics' --minimum-expected-tests 2 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet artifacts/bin/OpenForge.Cli.EndToEndTests/release/OpenForge.Cli.EndToEndTests.dll --filter-method '*F08RepairRenamedReferenceJourneyTests.ProvenRename' '*F08RepairRenamedReferenceJourneyTests.AmbiguousSuccessors' '*PublishedReferencesProcessTests.PublishedReferencesOutgoingPreservesDirectFacts' '*PublishedContextProcessTests.PublishedContextFragmentFailureIsIncompleteAndReadOnly' --minimum-expected-tests 6 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet artifacts/bin/OpenForge.Cli.EndToEndTests/release/OpenForge.Cli.EndToEndTests.dll --filter-method '*PublishedMainRootDefaultCompatibilityProcessTests.ExactDefaultCompatibility' --minimum-expected-tests 2 --fail-warns on --fail-skips on --no-ansi --progress off
npm run check:dotnet:whitespace
npm run check:dotnet:style
```
