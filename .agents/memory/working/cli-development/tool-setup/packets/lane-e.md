---
open-forge:
  description: Implement Task 90 lane E upstream recipes
  tags: [Memory, Working, CLI, Tool, Task, Contextual, Active]
---

# Upstream recipes

Lane E is the upstream recipe investigation that prepares real catalogue entries and Windows journeys for the integrated tool implementation.
The coordinator supplies the exact execution base in the dispatch prompt.

Status: Prepared for Phase 2. Work starts only through the assigned Worker Watch run.

## Read first

Read AGENTS.md and .agents/loader.md in your worktree and load applicable Open Forge scopes before changing files. Independently read the complete current .agents/directives/csharp/_csharp.md, design.md and style.md, plus CLI, software, Open Forge CLI, testing and work-planning scopes. Read the current CLI architecture, selected Task 90 and these exact spec files:

- .agents/memory/crystallized/documents/cli/technical-designs/tool-setup.md
- .agents/memory/crystallized/documents/cli/technical-designs/tool-setup-lanes.md
- .agents/memory/crystallized/documents/cli/contracts/tool/_tool.md, interface.md and behavior.md, then your leaf contracts
- .agents/memory/crystallized/documents/cli/contracts/shared/tool-records/interface.md and behavior.md
- .agents/memory/crystallized/documents/maintenance/tools.md
- D:/Repositories/open-forge-worktrees/of-lane-rules.md
- D:/Repositories/open-forge-worktrees/of-tool-setup-decisions-2.md

The maintainer's C1 through C7 decisions in the design override the older round-2 file. CLI selectors are bare ID, ID@major.minor or ID@exact stable SemVer. No CLI minor.x, dist-tags or full npm range expressions. Bare Install restores exact lock versions. A named bare ID selects latest support. Use --yes, no --automatic or --allow-download. No Open Forge per-tool Skill or managed-block guidance.

Codebase Memory uninstall is account-wide, including the binary and all agent entries. It needs live interactive confirmation naming that effect. With --yes, no terminal or a declined confirmation, skip it, remove only Open Forge project additions and print its native command. Project Remove never restores account-wide native settings. Keep them, say so and print native manual change commands. No cross-project setting registry. These intentional kept effects do not alone make project removal incomplete.

## Shared limits

You are not alone in the codebase. Keep other lanes' work and never revert their edits. Work only in your assigned worktree and writable paths below. The master worktree and maintainer checkout are read-only to you. No push, PR, GitHub mutation or local integration merge. Use local Git objects for old code. Public upstream research and dependency restore are authorized for the assigned task. Don't inspect credentials or unrelated private stores.

The user chooses gpt-6.1-sol with xhigh effort, overriding the older Worker Watch roster. Don't spawn more agents. Start your response with your agent name, model and effort from the invocation.

Never change real persistent user PATH, registries or real shell profiles. Root PATH capabilities must be testable through owned test paths or a fake host. No real tool installation in this first implementation pass. E handles real journeys later in owned temporary homes and Git repos under D:.

Models under src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Models/, schemas, actual .agents/open-forge.json and .agents/open-forge.lock.json, parent Task 90, shared contracts, all other packets and central composition are protected. A required shared change is a separate proposed contract(tool): ... increment. Report CONTRACT_REQUEST with exact changes and affected consumers before implementing against it. The mastermind integrates the contract first. Do not create private substitutes or copy shared contracts into your lane.

Tool operations add no state, lease, journal or recovery bundle under .open-forge-state/. Repository facts and additive receipts go in the committed lock. Native previous settings and machine facts stay in per-user state. Tool commands do not run concurrently by design. Keep expected-state checks, physical containment, unchanged unrelated data, atomic replacement and Git-aware sibling backups. No automatic rollback.

Use the existing layer graph: Framework mechanisms and data, Operations command behavior, Rendering presentation, Root privileged process/network/platform hosts and wiring. Match physical paths and namespaces. Use the BCL and existing packages, source-generated concrete JSON and existing command result conventions. A new dependency, project or exceptional platform mechanism needs an upward request.

## Verification

Run npm ci in your worktree. For the later site pass also run npm ci in src/docusaurus. Keep LF. Do not run Doctor in this repository workspace.

Use a managed solution build, then only tests you touch or create. Never run complete Unit, Integration, EndToEnd or native suites, unrelated test classes or build:native. CI is the full gate. Before a commit run npm run check:dotnet:whitespace and npm run check:dotnet:style.

A local build can use:
dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
Never commit that local NU1903 override. Never change PATH to make builds pass.

Use independently selectable tests with the right tier. For each touched class run the corresponding managed test project with --filter-class <qualified-class>, --minimum-expected-tests <actual-count>, --fail-warns on, --fail-skips on, --no-ansi and --progress off. Name the exact command, counts, artifact and result in your receipt. New tests use synthetic entries and owned temporary files, homes and processes. Never use a real per-user store. Avoid production test switches that alter semantics.

No literal assertions on user-facing text, help or rendered output. Use reviewed Imprint snapshots. Semantic assertions use the production owner for finding codes, environment names, paths and schema fields. Do not copy shipped payload strings into fixtures. Regeneration of payload-driven snapshots and fixtures is reserved for the integrator once, after all implementation and E's docs. Report any such pending snapshot impact.

## Return and commit

Update only your own packet's Progress and Evidence sections with the observed steps and exact checks. Do not edit parent state or common catalogues. Report new routed files so the integrator can Index once.

First finish the implementation and focused checks, then return READY_FOR_REVIEW with the changed files, test evidence, contract requests and remaining gaps. Leave the changes uncommitted for the coordinator's grouped review and commit continuation. Do not wait inside a running shell.

On the coordinator's continuation, apply the accepted grouped fixes if any, repeat only affected checks, make a local conventional commit with no attribution and report its full SHA. This packet includes that final commit-and-report assignment. Do not push or merge. No fabricated timestamps.

If a blocker affects only part of the assignment, finish independent work and report the exact blocked requirement. Never silently omit a requirement or call an unimplemented path complete.

## File assignment

This first run is research and a bounded recipe report only. You alone edit .agents/memory/working/cli-development/tool-setup/packets/lane-e.md and .agents/memory/working/cli-development/tool-setup/evidence/upstream-recipes.md. Read current shared models and old drafts through git show, but do not implement entries, change docs or install tools yet.

After A through D integrate, the coordinator will resume this same agent with transferred ownership of Framework/Tools/Catalogue/**, matching catalogue/journey tests, the later docs/help/CLI Skill pass and real Windows journeys. No other lane edits catalogue entries. Do not touch C/D leaf text before that transfer.

## Steps

Follow this ordered research plan:

1. Check exact base and read the full entry-review checklist, C1 through C7 and catalogue/channel/setup/config/record models. Read Council 2 tools.md, node.md and reuse.md plus the relevant Council 1 findings. Use the exact old draft source paths in the lane handoff for facts only.
2. Recheck official CodeGraph 1.6.2 and Codebase Memory 0.12.1 publication, licence, npm/GitHub channels, target assets, runtime/OS/CPU/libc requirements and exact per-asset hashes. CodeGraph SHA256SUMS is sufficient. Record the attestation/tag mismatch as a note, not a blocker. The CodeGraph wrapper support floor is Node 20. Supply per-client minimums only when supported by evidence.
3. Record the exact native setup/uninstall/dry-run vectors and every documented project/account output for backup planning. CodeGraph uses local Claude/Codex setup with its hook and permissions. Codebase Memory uses its documented installer and default account/client discovery, with --skip-binary after verified placement and --dir for the Open Forge bin. Keep all native MCP names, instruction blocks, Skills, hooks, permissions and watcher defaults.
4. Record only Open Forge's additions: CodeGraph telemetry off, archived-memory exclude union, conditional .gitignore coverage and a marked .cbmignore archive block. Open Forge adds no MCP entry, Codex TOML section, usage Skill or managed guidance line. Read-only CodeGraph version detection sets CODEGRAPH_NO_DOWNLOAD=1. Check telemetry stored-state probing and report inherited overrides separately.
5. Map the facts to the frozen model fields. List exact unsupported shapes or token/template needs as CONTRACT_REQUEST, without inventing another model. Initial ranges/floors are 1.6.x >=1.6.2 and 0.12.x >=0.12.1. Minimum Open Forge build should match the first shipping build; flag the release value if it isn't assigned. Keep Rune outside the V1 catalogue.
6. Design the later Windows journey steps in owned temp Git repos under D: and owned temporary homes. Verify how each upstream command resolves user home, known folders, caches, registries and PATH before executing anything. Native CBM uninstall is account-wide and must not touch real user PATH or profiles. Describe how to prove direct CLI and MCP initialization/use, update and both confirmed native and --yes project-only remove. A missing safe seam must be reported, not bypassed.
7. Write a concise tracked report with source URLs, checked version/date, exact hashes, commands, file inventory, environment/known-folder requirements, journey plan and contract needs. No copied binaries or credentials. No real installation in this first run.
8. Because this run changes only research documents, run formatting and focused Markdown/link checks using the touched documentation test class if you extend its source list through a contract request. Run a build and both required .NET checks before the later commit continuation. Return READY_FOR_REVIEW with evidence and future implementation prerequisites.

## Acceptance

The report can be used to compile complete entries and run both Windows journeys after integration without guessing native effects or weakening C2/C7. Cite official source code or documentation for niche facts. The first run does not claim entry implementation or native journey success. Never run npm install -g, native setup/uninstall or telemetry commands against the maintainer's account. No new Open Forge per-tool Skills. The existing CLI Skill copies get normal command docs only in the later transferred pass.

## Progress

Done: The B review documents were committed locally as `240c78f3449147fb9993f3fecc8c45a879583241`. The coordinator accepted F-H1, F-H2 and the bounded C# port plan after two ordinary JSON5 readers failed required native behavior. Root has authored the dated contract choice. Both required .NET checks pass for this design evidence commit. The exact prior build receipt is kept.
Now: The two-document design evidence increment is checked for its authorized local commit on base `240c78f3449147fb9993f3fecc8c45a879583241`. Only the new report and this packet's Progress and Evidence change. Formatting, local links, LF, packet-section and exact two-file checks pass. Root keeps the implementation choice as a maintainer question under the user delegation.
Next: Return the local commit's full SHA, two-file list and checked native licence cache path and SHA-256. F handles the private reader and tests through Root's contract increment. Full user docs and real Windows journeys wait for Root wiring. Successful exact journey versions must populate `TestedVersions` later.
Blocker: None for this local design evidence increment. Product source, dependencies, root `LICENSE` and F files are unchanged. `MinimumOpenForgeVersion` stays provisionally `0.9.0-beta.19`. Product and package versions are unchanged.

## Evidence

The catalogue continuation uses the shared contract introduced at `09bcb9ebdb89abed343d4c228283a8ee5aeba7c9` and keeps the prior evidence, including supplementary run `r_fec5e0e39de0`. The reviewed commit continuation uses current base `6d007254d5ca2937f4874ba5a93161f585814d8c`.
This pass handles static recipe data used by a local developer CLI. An incorrect declaration could affect later account or project writes, so focused Unit tests cover the exact pins, native vectors, bounds, home policies, additions and output declarations. Git keeps this code reversible. No native effects run in these tests.
The BCL and protected shared models cover this data. Exceptional machinery: none. Shared setup/config/record behavior stays with A through D. A supplies private Node pins to the factory. This slice selects a managed solution build and only the new catalogue test classes. The full managed and Native AOT gates stay with CI.

The first research run started clean at `2e517348ef19e34afa6297c7efa7d0db5451b45c`. The first catalogue candidate used `09bcb9ebdb89abed343d4c228283a8ee5aeba7c9`. The coordinator fast-forwarded the idle worktree to `6d007254d5ca2937f4874ba5a93161f585814d8c`, with all 19 candidate files kept.
The [research report](../evidence/upstream-recipes.md) records official URLs, publication dates, twelve hashes, native vectors, native file inventories, additions, model gaps and later Windows journeys.
The report is one new routed file for the integrator's Index pass. Common catalogues and parent state are unchanged.

The first-run checks are:

- `npm ci` passed. It reported 9 moderate and 1 high advisory. No dependency files changed.
- `dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1` passed on the managed target with zero warnings and errors. Build artifacts are under `artifacts/bin/`.
- `npm run check:dotnet:whitespace` passed with exit 0.
- `npm run check:dotnet:style` passed with exit 0.
- The focused Integration command below passed 12 tests, with zero failures, skips and warnings. It covers the packet's existing documentation source set. The report's source-list addition is proposed to the integrator, so its links also have a separate production References check.
- A source-only checksum cross-check compared both downloaded checksum files, GitHub release asset digests and the report. All 12 selected assets matched. Receipt: `artifacts/task90-lane-e/upstream-checks.json`.
- Prettier, final production References reports and LF/Git whitespace results are included in the final check receipt under `artifacts/task90-lane-e/`.

The exact focused command is:

```powershell
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Documents.RepositoryDocumentation.ToolSetupDocumentationTests --minimum-expected-tests 12 --fail-warns on --fail-skips on --no-ansi --progress off --results-directory artifacts/test-results/task90-lane-e-final --report-xunit-trx --report-xunit-trx-filename lane-e-documentation.trx
```

The initial test invocation used unsupported `--report-trx` and exited before running tests. The corrected invocation above passed and produced `artifacts/test-results/task90-lane-e-final/lane-e-documentation.trx` with 12 passed, zero failed and zero skipped.
No test source, snapshot or fixture changed. No full suite or Native AOT run was selected.
No upstream binary was downloaded or installed, and no native setup, uninstall or telemetry command ran.
The earlier executable checks above weren't rerun for this Markdown-only continuation.
The report replaces the former confirmed-uninstall journey blocker with the maintainer's injected production-operation test choice.
No upstream binary changes, registry test keys, protected model edits, parent Task edits or other packet edits were made.

The continuation's focused source evidence is:

- CodeGraph v1.6.2 `docs/design/telemetry.md:112-150`, `src/telemetry/index.ts:211-255,402-426` and `src/bin/codegraph.ts:228-250,2792-2830` establish the stored file, default, field validation, status output and read-only behavior. The report separates native malformed-file fallback from the proposed Open Forge blocking rule and inherited overrides.
- Node v24.16.0 `doc/api/os.md:206-220` establishes USERPROFILE/current-profile resolution on Windows and HOME/effective-UID resolution on POSIX.
- CodeGraph v1.6.2 `scripts/build-bundle.sh:99-150` and `scripts/npm-shim.js:44-107` establish executable and JS paths, the two ordered runtime flags and host-PID forwarding. These are tagged packaging facts, not an archive-execution claim.
- CBM v0.12.1 `src/cli/cli.c:10980-11098,11298-11347` establishes the complete `agent.install.plan.v1` object, file-array overlap, hook object fields and the read-only early exit. The report records exact legacy and profile supplements with source links at each path rule.
- Supplementary run `r_fec5e0e39de0` reported 20 registry profiles. The catalogue pass rechecked the published CBM v0.12.1 `src/cli/agent_clients.c` initializer and counted 19 records. The array-bound enum token is excluded. `src/cli/cli.c:1597-1671,1752-1759` limits old Skill cleanup to empty directories, correcting the earlier broad wording.

No CodeGraph or CBM command, binary download, archive extraction, account file read, PATH edit or registry action ran in this continuation.

The continuation's document checks are:

- `node node_modules/prettier/bin/prettier.cjs --check .agents/memory/working/cli-development/tool-setup/packets/lane-e.md .agents/memory/working/cli-development/tool-setup/evidence/upstream-recipes.md` passes for both files.
- The focused Markdown AST check passes for 2 files, 1 fenced JSON object, 4 local links and 33 fetched tagged-source line ranges. It also passes LF, trailing-whitespace and packet-section checks. Packet edits stay within Progress and Evidence.
- `git diff --check` passes. HEAD is unchanged at the execution base. The fresh document receipt is `artifacts/task90-lane-e/continuation-document-checks.json`.

The catalogue candidate adds these primary entry points:

- `ToolCatalogueFactory.Create(IReadOnlyList<ToolPrivateNodeSpec> nodeRuntimes)` returns the shared `ToolCatalogue`. F supplies A's approved runtime list. The factory copies it and builds the ordinal frozen entry dictionary. No private Node pin is defined by E.
- `CodeGraphCatalogueEntry.Create()` returns the CodeGraph `1.6.x` profile from `1.6.2`, with the Node native home policy, two channels, six pins, typed bundled Node metadata, native local setup and stored telemetry metadata.
- `CodebaseMemoryCatalogueEntry.Create()` returns the Codebase Memory `0.12.x` profile from `0.12.1`, with HomeThenUserProfile, GitHub only, six pins, native plan plus default all-client discovery and separate uninstall declarations.

Both profiles use provisional first shipping build `0.9.0-beta.19`. Their `TestedVersions` arrays are empty until native journey evidence exists. The minimum reviewed patches and the twelve exact archive pins are complete.
The report's Catalogue factory and Client bindings sections give F the composition details. File-valued `<id>-config` bindings and directory-valued client roots must match the native overrides and reviewed path choices before setup.
CodeGraph has only telemetry off, archive union and conditional index-ignore additions. CBM has only its marked archive-ignore addition. Native outputs don't become Open Forge config changes.
Cline v0.12.1 withholds new context hooks and removes only recognized existing adapters. The entry declares its four platform-specific legacy hook paths. No upstream setup vector or watcher default changed.

The first catalogue candidate's managed checks were:

| Check                             | Result                                      | Artifact                                                                                                       |
| --------------------------------- | ------------------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| Release solution build            | Passed, zero warnings and errors            | `artifacts/bin/`, final build duration `00:04:49.51`                                                           |
| `ToolCatalogueTests`              | 3 passed, zero failed, skipped or warnings  | `artifacts/test-results/task90-lane-e-catalogue/ToolCatalogueTests/ToolCatalogueTests.trx`                     |
| `CodeGraphCatalogueTests`         | 13 passed, zero failed, skipped or warnings | `artifacts/test-results/task90-lane-e-catalogue/CodeGraphCatalogueTests/CodeGraphCatalogueTests.trx`           |
| `CodebaseMemoryCatalogueTests`    | 12 passed, zero failed, skipped or warnings | `artifacts/test-results/task90-lane-e-catalogue/CodebaseMemoryCatalogueTests/CodebaseMemoryCatalogueTests.trx` |
| `npm run check:dotnet:whitespace` | Passed, exit 0                              | Final receipt                                                                                                  |
| `npm run check:dotnet:style`      | Passed, exit 0                              | Final receipt                                                                                                  |

The exact final build command is:

```powershell
dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
```

The exact focused commands are:

```powershell
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.Catalogue.ToolCatalogueTests --minimum-expected-tests 3 --fail-warns on --fail-skips on --no-ansi --progress off --results-directory artifacts/test-results/task90-lane-e-catalogue/ToolCatalogueTests --report-xunit-trx --report-xunit-trx-filename ToolCatalogueTests.trx
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.Catalogue.CodeGraphCatalogueTests --minimum-expected-tests 13 --fail-warns on --fail-skips on --no-ansi --progress off --results-directory artifacts/test-results/task90-lane-e-catalogue/CodeGraphCatalogueTests --report-xunit-trx --report-xunit-trx-filename CodeGraphCatalogueTests.trx
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.Catalogue.CodebaseMemoryCatalogueTests --minimum-expected-tests 12 --fail-warns on --fail-skips on --no-ansi --progress off --results-directory artifacts/test-results/task90-lane-e-catalogue/CodebaseMemoryCatalogueTests --report-xunit-trx --report-xunit-trx-filename CodebaseMemoryCatalogueTests.trx
```

`npm ci` ran again successfully without a dependency-file change. Its advisory counts were unchanged.
The new tests are pure Unit checks of catalogue facts and synthetic runtime injection. They read no real per-user store and launch no upstream process. No rendered-text assertion or snapshot regeneration was added. No full suite, Native AOT gate, Doctor, user docs pass or real journey ran.
No flake was observed. The fresh final receipt, source hashes, local-link results and exact changed-file list are under `artifacts/task90-lane-e-catalogue/`.
One new routed Markdown file still needs the integrator's Index pass. Protected Models, schemas, composition, parent Task and other packets are unchanged. No shared contract commit, package version bump, push or GitHub mutation was made.

The coordinator accepted the staged catalogue candidate, then requested one grouped fix before its local commit.
`CodeGraphDefinitions.NoDownloadValue` now defines the enabled value for `CODEGRAPH_NO_DOWNLOAD`.
Both production environment declarations and both technical-value assertions use it.
The reviewed candidate comparison shows only those three C# files and the two assigned Markdown files changed since review. The other fourteen candidate files are byte-identical.
No protected type changed. The optional native environment facts added at current base were read before the fix.

The fresh commit-continuation checks are:

| Check                             | Result                                          | Artifact                                                                                          |
| --------------------------------- | ----------------------------------------------- | ------------------------------------------------------------------------------------------------- |
| Release solution build            | Passed, zero warnings and errors, `00:07:03.68` | `artifacts/task90-lane-e-commit/build.log`                                                        |
| `CodeGraphCatalogueTests`         | 13 passed, zero failed, skipped or warnings     | `artifacts/test-results/task90-lane-e-commit/CodeGraphCatalogueTests/CodeGraphCatalogueTests.trx` |
| `npm run check:dotnet:whitespace` | Passed, exit 0                                  | `artifacts/task90-lane-e-commit/dotnet-whitespace.log`                                            |
| `npm run check:dotnet:style`      | Passed, exit 0                                  | `artifacts/task90-lane-e-commit/dotnet-style.log`                                                 |

The build uses the exact managed build command recorded above.
The only fresh test selection is:

```powershell
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.Catalogue.CodeGraphCatalogueTests --minimum-expected-tests 13 --fail-warns on --fail-skips on --no-ansi --progress off --results-directory artifacts/test-results/task90-lane-e-commit/CodeGraphCatalogueTests --report-xunit-trx --report-xunit-trx-filename CodeGraphCatalogueTests.trx
```

The earlier 3 `ToolCatalogueTests` and 12 `CodebaseMemoryCatalogueTests` results stay green for their unchanged cases. They weren't rerun in this continuation.
The Markdown format, production References, LF, packet-section and Git whitespace receipts are under `artifacts/task90-lane-e-commit/`.
The local commit covers only the nineteen assigned files. Its canonical receipt records the final source hashes and commit identity.
`TestedVersions` stays empty in both entries. The later entry update must add each exact version with a successful native journey.
The first shipping metadata stays provisionally `0.9.0-beta.19`. No package version changed.
The next documentation continuation must also read `D:/Repositories/open-forge-worktrees/tool-setup/artifacts/tool-setup/dispatch/documentation-voice.md`.
No native journey, full user docs pass, payload snapshot update, full test suite, push, merge or GitHub action ran in this continuation.

The focused read-only B review uses E's base `fd2b1e4e12cf25d70d205db1e4158dc22b6bc8a9` and B's frozen uncommitted candidate on `b22e1ec393280935dea93a312f2a1aa337834ad5`.
The [config catalogue review](../evidence/config-catalogue-review.md) records E-B1 and E-B2 with severity, exact locations, source traces and the smallest corrections.
It also records the checked telemetry schema, inherited overrides, native vectors, home binding, C7 Remove, native output and client-file bindings, complete receipt sets, backups and fixed profile digest inputs.
The 138-test earlier receipt and 67 affected grouped checks were read from B's packet and logs. Their populations overlap. No test or build was rerun for this review.
B's 39-file source manifest SHA-256 is `3dfd5b2341268316ecd3a31f944ff6da078a1b5983e004f24abbe231434b56ed`, unchanged at both source checkpoints.
The new report defines its hash format and the four production subset hashes.
Only this packet's Progress and Evidence sections and the new report are writable in this pass. The report needs the integrator's later Index pass.
No product or peer file was changed, no native execution ran and no commit was made.
Both changed Markdown files passed Prettier. Production References found six complete local links and two external links marked unchecked. The two external Git manual pages were read separately for the findings.
LF, trailing whitespace, `git diff --check`, Markdown AST packet-section checks and the exact two-file change limit passed. B's final source checkpoint still matches the hash above.

### Review doc checks

The coordinator accepted this review and authorized one local two-document commit.
The catalogue implementation commit is `1ec6ad921943b881f819f3a8eb66b4796eeabacb`.
The current review-document base is `fd2b1e4e12cf25d70d205db1e4158dc22b6bc8a9`, which includes the integrated npm entry caller metadata.
This continuation changes no product or test source and performs no new review of B's routed corrections.

The checked local receipt uses SDK `10.0.101`, managed Release `net10.0` on Windows x64:

| Check                              | Result                                                 | Artifact                                                                               |
| ---------------------------------- | ------------------------------------------------------ | -------------------------------------------------------------------------------------- |
| Release solution build             | Passed, zero warnings and errors, `00:04:39.43`        | `artifacts/task90-lane-e-review-docs/build.log`                                        |
| `npm run check:dotnet:whitespace`  | Passed, exit 0                                         | `artifacts/task90-lane-e-review-docs/whitespace.log`                                   |
| `npm run check:dotnet:style`       | Passed, exit 0                                         | `artifacts/task90-lane-e-review-docs/style.log`                                        |
| Focused Markdown format            | Passed for both review documents                       | `artifacts/task90-lane-e-review-docs/markdown.log`                                     |
| Production References              | Six complete local links, two external links unchecked | `artifacts/task90-lane-e-review-docs/packet-references.json`, `report-references.json` |
| LF, whitespace and packet sections | Passed, exactly the two assigned files                 | `artifacts/task90-lane-e-review-docs/final-receipt.json`                               |

The exact build and .NET commands are:

```powershell
dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
npm run check:dotnet:whitespace
npm run check:dotnet:style
```

The focused Markdown and link commands are:

```powershell
node node_modules/prettier/bin/prettier.cjs --check .agents/memory/working/cli-development/tool-setup/packets/lane-e.md .agents/memory/working/cli-development/tool-setup/evidence/config-catalogue-review.md
open-forge references .agents/memory/working/cli-development/tool-setup/packets/lane-e.md --direction out --format json
open-forge references .agents/memory/working/cli-development/tool-setup/evidence/config-catalogue-review.md --direction out --format json
git diff --check
```

The existing Prettier Markdown parser checks that packet edits stay within Progress and Evidence.
Raw byte checks cover LF, final newlines and trailing whitespace in both files.
The final local receipt records source hashes, the exact two-file commit list and the commit identity after creation.
No tests were selected or run because no test source changed. No native tool, payload snapshot, push or merge ran.
The report's original frozen B checkpoint is historical review evidence. B handles the routed fixes separately.

### Native reader plan

The accepted review-document commit is `240c78f3449147fb9993f3fecc8c45a879583241` on parent `fd2b1e4e12cf25d70d205db1e4158dc22b6bc8a9`.
The next bounded investigation compared F's OpenClaw workspace resolver with the exact CBM v0.12.1 selected-path reader.
Root accepted the JSON5 syntax gap as F-H1 and zero-length fallback as F-H2.
The native reader doesn't follow include files. A root include prevents fallback only when the configured workspace is missing.

The ordinary-package investigation checked the unchanged central dependencies first, then `Json5` 1.0.4 and `Json5Core` 1.0.12.
The first loses complete syntax validation and raw values. Its two data paths also handle duplicates differently.
The second overwrites duplicate names before path lookup.
These conclusions come from tagged source, not executed package tests.

The [native config reader plan](../evidence/native-config-reader.md) maps the necessary native functions, the internal caller contract, lexical families, focused fixtures, the MIT notice and exact future footprint.
The proposed dated choice is a private same-format port with numeric validation and skip.
It adds no package, writer, normalizer, generic public parser, native bridge or interpreter.
Root records the exception and maintainer question before F implements it.
F handles six source and test files. Root keeps the native MIT notice in the existing root licence copied by current distribution paths.
This pass changes neither architecture nor those seven future files.

The cached native sources and licence match the v0.12.1 Git tree `a7158ac2f48ca646b4527341b76f930f2aeff87e`.
The report records their exact SHA-256 values and tagged source lines.
The first research return ran no build, .NET checks, tests, native command, account file read, PATH or registry action.
Prettier passes for both changed Markdown files.
Production References reports three complete local links from the report and three from the packet.
The report's twelve external source links stay marked unchecked by References. Their native source lines were read from the tagged, blob-matched cache.
LF, final newlines, trailing whitespace and `git diff --check` pass.
The packet comparison against HEAD passes for edits restricted to Progress and Evidence.
The exact two-file change check passes with the new report untracked and the packet modified.
The focused format and link commands are:

```powershell
node node_modules/prettier/bin/prettier.cjs --check .agents/memory/working/cli-development/tool-setup/evidence/native-config-reader.md .agents/memory/working/cli-development/tool-setup/packets/lane-e.md
open-forge references .agents/memory/working/cli-development/tool-setup/evidence/native-config-reader.md --direction out --format json
open-forge references .agents/memory/working/cli-development/tool-setup/packets/lane-e.md --direction out --format json
git diff --check
```

The first research return left both documents uncommitted.
The coordinator then accepted the plan, reported Root's authored dated contract choice and authorized a separate local design evidence commit of these two files.
The integrator's later Index pass handles the one new routed report.

### Reader commit checks

The commit continuation keeps `artifacts/task90-lane-e-review-docs/final-receipt.json` unchanged.
Its exact managed Release build records SDK `10.0.101`, zero warnings and errors and duration `00:04:39.43`.
The retained command is `dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1`.
The retained build log SHA-256 is `2e4c1792b39e2b7d2376de25a9cb0df5fdee6da02a2d95a70fa909c8940ba482`.
Its hash was rechecked against that receipt.
Git confirms only Markdown changed since the checked build's source base `fd2b1e4e12cf25d70d205db1e4158dc22b6bc8a9`.
The coordinator authorizes keeping this build without rerunning it.

Fresh pre-commit results are:

| Check                                | Result                                                     | Artifact                                                                                        |
| ------------------------------------ | ---------------------------------------------------------- | ----------------------------------------------------------------------------------------------- |
| `npm run check:dotnet:whitespace`    | Passed, exit 0                                             | `artifacts/task90-lane-e-native-reader-design/whitespace.log`                                   |
| `npm run check:dotnet:style`         | Passed, exit 0                                             | `artifacts/task90-lane-e-native-reader-design/style.log`                                        |
| Focused Prettier command above       | Passed for both files                                      | `artifacts/task90-lane-e-native-reader-design/markdown.log`                                     |
| Production References commands above | Six complete local links, twelve external links unchecked  | `artifacts/task90-lane-e-native-reader-design/report-references.json`, `packet-references.json` |
| LF, whitespace and Markdown AST      | Passed, packet changes restricted to Progress and Evidence | `artifacts/task90-lane-e-native-reader-design/document-checks.json`                             |
| Exact file limit                     | Passed, only the two assigned documents                    | Final local receipt                                                                             |

The native `LICENSE` cache was rechecked at commit continuation.
Its SHA-256 is `1f58f9911dc5e3bcb96de28bb28e7b6bb7eb323952d29569c5d7214a152146bb`.
Its Git blob `0b95352746a97a8cf2d1f3ba568bc7bb5e514d1f` matches the tagged native tree.
The final local receipt records the exact cache path, final source hashes, commit identity and two-file list.
No new build, tests, native tool, account action, product edit, dependency change, root licence edit, F source edit, push or merge ran.

## Divergences

None recorded before implementation.
