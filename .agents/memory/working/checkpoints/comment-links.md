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
