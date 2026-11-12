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

## Handoff

The branch is `feature/comment-links`, stacked on `fix/code-link-fragments` at `c41e95b1b`. Changes stay in this worktree. No push, merge or PR was made. The overseer publishes the branch and runs CI. The existing NuGet audit failure needs maintainer follow-up if it blocks CI.
