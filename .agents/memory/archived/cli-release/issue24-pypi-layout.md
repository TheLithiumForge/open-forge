---
open-forge:
  description: Issue 24 three-kind PyPI package layout and local verification receipt
  tags: [Memory, Archived, Contextual, Historical, CLI, Delivery]
---

# PyPI Layout
The PyPI layout is a generated package graph that mirrors npm's platform, entry and alias packages.

The maintainer replaced the original single-project wheel design on 2026-11-12. The [distribution contract](../../crystallized/documents/cli/distribution.md#alias-and-pypi) defines the replacement. Work continues from `7726be221` in `feature/package-aliases`.

Status: implementation complete with local focused verification. CI qualification and publication are pending.

This record came from the active replacement checkpoint. It is archived because the local implementation is complete. The [earlier receipt](issue24-package-aliases.md) keeps the first design and its prior full-suite failures as history. It does not describe the replacement package graph.

## Preflight

The change affects local package artifacts and future publication. Git and regeneration recover local outputs. Publication stays outside this task. A partial upload can leave earlier projects published. The existing default-off switch and trusted publisher setup stay required.

The implementation reuses the npm name and target model, selected version, deterministic ZIP writer, staging and release checks. Standard Python metadata, markers, distribution lookup and console scripts provide the installed package graph. Node's native file snapshots provide reviewed metadata and launcher baselines. No new dependency or exceptional machinery is selected.

The evidence tiers are:

- Unit - generated names, metadata, environment markers, wheel tags, contents and version mapping, with reviewed snapshots.
- Integration - per-host packing, universal-wheel agreement, release collection and publication preparation.
- PackageEndToEnd - a clean venv resolves the local alias to the entry package and exactly one platform package, then runs the version command.
- Managed regression - build and focused payload and Status tests for the revised shipped Skill. Full and native suites stay in CI under the updated lane rules.

## Evidence

The twelve focused PyPI tests pass with ten reviewed native Node file snapshots. These cover six native wheels, the two universal wheels, all markers and version mapping. The first formatting check rejected JSON snapshot formatting. The same reviewed bytes now use the repository's `.txt` snapshot extension, and delivery formatting passes. Normal runs write received files below `artifacts/command-output-snapshots/<run>/PyPI/<group>`. Baseline updates require Node's snapshot-update flag.

Both local Python installation names passed with all six platform wheels available. The unscoped alias resolved three Open Forge distributions. The prefixed entry name resolved two. Both selected only Windows x64 and returned `0.9.0-beta.18`. The tracked `scripts/delivery/pypi/__tests__/smoke-local.ts` reproduces this using an existing host executable. Its five other targets use inert selection fixtures.

The source executable is the previously downloaded published beta18 Windows x64 binary. Its SHA-256 is `f8510a954aa8407dc0c6e862d0145fe9e40ca9c92dba28b2b7364c91d075b888`. No new native build runs under the lane rules.

The initial managed build passed with zero warnings. Focused Integration had five expected snapshot failures from the revised Skill. Review confirmed only the new install notes and the associated 419-character, 104-token and startup-share changes. The 57 Imprint baselines were updated serially, and all eleven selected tests passed in update mode. Their groups are Status `HealthyWithExtension` (seven files), `InstalledWorkspace` (42), `RecoveryBundlePresent` (seven) and payload `CustomGuidanceMatchesReviewedTemplates` (one). Diagnostic templates stayed unchanged. The encoded default-layout fixture changes only the same Skill entry. Its prior bytes matched the previous commit exactly.

Docusaurus typecheck and build passed. Its build reported the existing nonfatal Node localStorage experimental warning. The fresh delivery run passed 180 tests with one declared Windows exclusion. It covers refreshed-hash tampering for all three wheel kinds and the platform, entry and alias OIDC upload directories. The final managed build passed with zero warnings and errors. Normal focused Unit passed 78 cases in 2.435 seconds. Normal focused Integration passed eleven cases in 24.438 seconds. Both executed their complete selected populations with zero skips and snapshot-update mode off.

The seven package-layout checks and thirteen Markdown checks passed with zero skips. Delivery TypeScript, lint and formatting passed. The final collection tests check typed validation failures without matching diagnostic text. The fresh full delivery run after that assertion change again passed 180 tests with one declared Windows exclusion. No new runtime flake was seen in this revision.

The runtime and tools are Windows x64, managed .NET Release, Node 26.12.1, npm 11.20.0, .NET SDK 10.0.101 and Python 3.13.14. The working root is `D:/Repositories/open-forge-worktrees/package-aliases`. The executable inputs for the managed gate have these SHA-256 identities:

| Artifact | SHA-256 |
| --- | --- |
| Development Framework assembly | `1213b41060aea51d53831f824642f2f58bb58954dc597f814346553e6628449b` |
| Unit test assembly | `d479979a0ee0793e06bcd28dee04f2e110837a3dcaafb303835b0c60ba9929cb` |
| Integration test assembly | `0ed335ed142318daaa798caed07d16c1f3c177cc2d4c08af13e1279e8aec8c9f` |

The local smoke's universal wheel hashes are:

| Wheel | SHA-256 |
| --- | --- |
| Alias | `48905620bb7db77c09120f6b8cc764c16ac9d6aa248ac5f50e17acd4ba79dc08` |
| Entry | `70f99a4a1697129839192f829d5aa27c81695a82626a6072feb121c7f5755dae` |

## Reproduce

From the worktree root in PowerShell, run the local gates:

```powershell
npm run build -- --no-restore
dotnet artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests.dll --filter-namespace 'OpenForge.Cli.Core.UnitTests.Framework.Distribution*' --minimum-expected-tests 78 --parallel none --fail-warns on --fail-skips on --no-ansi --progress off
dotnet artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll --filter-class OpenForge.Cli.IntegrationTests.Commands.Status.StatusBeforeOutputSnapshotTests --filter-class OpenForge.Cli.IntegrationTests.Framework.Distribution.WorkspaceLayoutPayloadRenderingIntegrationTests --minimum-expected-tests 11 --parallel none --fail-warns on --fail-skips on --no-ansi --progress off
npm run test:delivery
npm run check:delivery
npm run test:package-layout
npm run check:docs
```

The focused PyPI unit command is:

```powershell
node --test --test-concurrency=1 scripts/delivery/pypi/__tests__/version.test.ts scripts/delivery/pypi/__tests__/environment-markers.test.ts scripts/delivery/pypi/__tests__/wheel.test.ts
```

The smoke uses the existing host fixture and performs no registry download:

```powershell
npm exec --offline --call "node scripts/delivery/pypi/__tests__/smoke-local.ts artifacts/alias-smoke-source/package/bin/open-forge.exe"
```

It makes all eight local wheels available to pip through `--no-index --find-links`. It installs only one entry requirement per clean venv. It does not use `--no-deps` or install the platform wheel directly. The receipt is `artifacts/alias-smoke-layout/alias-smoke.json`. The unscoped graph contains `open-forge`, `thelithiumforge-open-forge` and `thelithiumforge-open-forge-win-x64`, each at `0.9.0b18`. The prefixed graph contains the latter two. Both console commands return npm version `0.9.0-beta.18`.

In `src/docusaurus`, run `npm run typecheck` and `npm run build` for the site checks. Root and site dependencies are unchanged from the preceding implementation's successful `npm ci` setup.

## Limits

No package-layout deviation from the maintainer's replacement is recorded. The host smoke reuses the published executable because the updated lane rules leave new native builds and full suites to CI. Five inert non-host fixtures check dependency selection. They do not qualify those native targets. No full or native suite ran for this revision, and no prior managed pass is presented as native evidence.

All eight PyPI projects need trusted publisher setup before enabling `PUBLISH_PYPI`. The OIDC steps publish platform projects, then the entry project and alias. The switch still defaults off. Public smoke remains behind `--aliases` and requires both registries to contain the selected version. No publication, push or pull request is part of this task.
