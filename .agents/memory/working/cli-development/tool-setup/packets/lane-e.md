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

Done: Preserved the complete 33-path candidate, B/C v3 inputs and all prior evidence. Applied only the checked 12-file D source patch. E-J1, E-J2, E-J3, E-J4, I-R1 and I-R2 are revalidated. Both real Windows journeys complete. Real CBM project-only Remove returns completed and exit 0 without native uninstall or its dry run. Confirmed account removal passes with injected native effects. CBM `0.12.1` now joins CodeGraph `1.6.2` in tested metadata.
Now: READY_FOR_REVIEW, uncommitted. The actual beta.19 graph and manual host build. Final focused results are 132 passed, two pending C Imprint mismatches and zero skips across 134 cases. Both mandatory .NET checks return 0. Markdown, local links, LF and site checks pass. The E-only set has 35 paths and the protected input record has 72 paths.
Next: The coordinator reviews the two C account-choice captures, then handles the 43 help captures, other payload work and later commit continuation. Use the final E-only and protected input manifests under `artifacts/task90-lane-e-remove-final/`. No further native recheck is required by the current findings.
Blocker: The two protected C `AccountChoice` baselines need coordinator review for the intended deferred-plan rendering. Their semantic assertions pass before Imprint. No native journey or product defect stays open in this return. No baseline is accepted by E.

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

### Native reader review

The [native reader review](../evidence/native-reader-review.md) checks only F's three new reader files, changed caller and two test files against tagged CBM and the accepted plan.
All six hashes match before and after review. The content checkpoint is `c72327e8f458cb28cc1608090da61fc1a41f8ae724d16294f220215aaf3528c3`.
No NR1 finding is raised.
F's 176-case receipt, clean build and both .NET checks were consumed without rerunning them.
The source review ran no product edits, tests, build, package or native execution, account actions or commit.
Only the assigned review report and this packet change.
Prettier, six local links, LF, whitespace, packet-section and two-file checks pass. Six external source links stay marked unchecked by References.

### Reader review commit

The coordinator authorized a separate local commit of the accepted review report and this packet.
The exact prior E build receipt stays unchanged at `artifacts/task90-lane-e-review-docs/final-receipt.json`.
It records SDK `10.0.101`, managed Release `net10.0` on Windows x64, zero warnings and errors and duration `00:04:39.43`.
The build log still matches SHA-256 `2e4c1792b39e2b7d2376de25a9cb0df5fdee6da02a2d95a70fa909c8940ba482`.
Only Markdown changed since that build's source base `fd2b1e4e12cf25d70d205db1e4158dc22b6bc8a9`.
Product source and build configuration are unchanged, so no new build ran.

Fresh pre-commit checks are:

| Check                             | Result                                                  | Artifact under `artifacts/task90-lane-e-native-reader-review/` |
| --------------------------------- | ------------------------------------------------------- | -------------------------------------------------------------- |
| `npm run check:dotnet:whitespace` | Passed, exit 0                                          | `whitespace.log`                                               |
| `npm run check:dotnet:style`      | Passed, exit 0                                          | `style.log`                                                    |
| Focused Markdown formatting       | Passed for both files                                   | `markdown.log`                                                 |
| Production References             | Six complete local links, six external links unchecked  | `report-references.json`, `packet-references.json`             |
| LF, whitespace and Markdown AST   | Passed, only Progress and Evidence change in the packet | `document-checks.json`                                         |
| Exact file limit and commit list  | Only the two assigned documents                         | `final-receipt.json`                                           |

The exact document and link commands are:

```powershell
node node_modules/prettier/bin/prettier.cjs --check .agents/memory/working/cli-development/tool-setup/evidence/native-reader-review.md .agents/memory/working/cli-development/tool-setup/packets/lane-e.md
open-forge references .agents/memory/working/cli-development/tool-setup/evidence/native-reader-review.md --direction out --format json
open-forge references .agents/memory/working/cli-development/tool-setup/packets/lane-e.md --direction out --format json
git diff --check
```

The final local receipt records the full commit SHA, parent, two-file list, source hashes and clean status.
No tests, native tools, catalogue, Root, grammar, shared-file or snapshot edits, GitHub actions, account actions, push or merge ran.

### Final execution capsule

The final dispatch transfers catalogue tested metadata, matching journey evidence, docs, help wording, CommandHelpIntegrationTests and both CLI Skill copies to E.
The frozen input hashes, safe native environment, exact manual commands and results are in the [Windows journey report](../evidence/windows-journeys.md).
The complete input manifest is `artifacts/task90-lane-e-final/input-manifest.json`.
The reviewed patch and all 34 C files matched their supplied hashes. Copy targets matched the core base. None of the 51 supplied files was staged.
Transferred help wording and this packet are the only writable overlaps with those inputs. Other input files stay byte-identical.

The realistic effects are tool installation, owned native client configuration and project additions inside task-owned D: locations.
Git and sibling backups keep project edits reviewable. Native account settings stay in the owned account and are kept on project Remove.
No real persistent PATH, registry or profile change is permitted.
The existing production graph and injected homes, clients, process, download and PATH capabilities cover the journey.
The manual host is test support, not a production flag or replacement service implementation.
Normal automated tests stay offline. Manual native execution is recorded separately.
Focused catalogue, command help and new synthetic journey evidence are the selected managed tests. Full suites and Native AOT stay in CI.
No snapshots or payload fixtures will be regenerated by E.

Root and Docusaurus `npm ci` pass. The baseline prospective build passes with zero warnings and errors in `00:04:09.19`.
Its invocation uses `-p:OpenForgeCliVersion=0.9.0-beta.19` and the local NuGet audit override only on the command.
Actual executable and composed-host `--version` output is `0.9.0-beta.19`.
Composed List returns schema 3 with `data.tools`. Initial Inspect reports the missing CodeGraph state.
CodeGraph `TestedVersions` now contains `1.6.2`. CBM stays empty because E-J1 blocks its real journey.

### Final checked candidate

The final managed Release build passes with zero warnings and errors in `00:03:27.24`.
It uses `-p:OpenForgeCliVersion=0.9.0-beta.19` and the local audit override only on the invocation.
The resulting executable reports `0.9.0-beta.19`.
The final log is `artifacts/task90-lane-e-final/final-build.log`.
The site build passes with `npm run build` in `src/docusaurus`.

The focused results are:

| Class                               | Cases | Result                           | Artifact under `artifacts/task90-lane-e-final/` |
| ----------------------------------- | ----- | -------------------------------- | ----------------------------------------------- |
| `CodeGraphCatalogueTests`           | 13    | Passed, exit 0                   | `test-codegraph.log`                            |
| `ToolJourneyTransitionTests`        | 2     | Passed, exit 0                   | `test-transition-final.log`                     |
| `ToolDocumentationIntegrationTests` | 3     | Passed, exit 0                   | `test-docs.log`                                 |
| `ToolHelpIntegrationTests`          | 8     | Six pass, two fail, exit 2. E-J3 | `test-help-final.log`                           |

Both final-source mandatory gates pass:

| Command                           | Exit | Artifact under `artifacts/task90-lane-e-final/`     |
| --------------------------------- | ---- | --------------------------------------------------- |
| `npm run check:dotnet:whitespace` | 0    | `whitespace-final.log`, `whitespace-final.exit.txt` |
| `npm run check:dotnet:style`      | 0    | `style-final.log`, `style-final.exit.txt`           |

The Windows report gives the exact test commands, native vectors, physical writes and completed MCP request.
CodeGraph's initial project content returns byte-for-byte after Remove.
Its telemetry stays off and its CLI and index stay available.
The owned MCP process exits 0, and its native daemon shuts down idle with zero clients.
No CBM native command runs because E-J1 blocks before download.
The confirmed account path still needs injected process and PATH coverage after the corrected CBM journey.
The frozen C account-choice cases are consumed as reviewed evidence, without rerunning them or editing their snapshots.

The final input and E slice records are:

- `artifacts/task90-lane-e-final/input-manifest.json` - all 51 supplied input hashes and preparation checks.
- `artifacts/task90-lane-e-final/frozen-input-check.json` - 46 unchanged protected files and five assigned wording or packet overlaps.
- `artifacts/task90-lane-e-final/e-files.txt` and `e-file-manifest.json` - the 31-file E slice, including one deleted unused outline.
- `artifacts/task90-lane-e-final/final-receipt.json` - final source hashes, checks and exact blocked requirements.

Only Progress and Evidence change in this packet against its frozen `3a6469a8e1e7119281b842640f069c6d0456ad44` input.
The Windows report is the new routed Markdown file for the coordinator's final Index.
No shared ABI change is requested.
E-J1, E-J2 and E-J3 are implementation findings for coordinated correction.
The full CBM journey and its `TestedVersions` update are incomplete.
Payload-driven help, Skill, catalogue read and the two known WorkspaceShape captures wait for the coordinator's single final pass.
No snapshot is regenerated.
Nothing is staged or committed.

### Grouped correction run

The complete grouped dispatch is read. The current C# rules, Open Forge scopes and defining Tool records are reloaded.
The execution capsule still uses owned native Windows fixtures, the production graph and only the assigned injectable hosts.
The new private temporary directory's ACL is required by native CBM. No shared ancestor is edited and the native guard stays enabled.
Real native execution stays separate from the injected confirmed-account proof.
No production option, storage field, dependency or shared model changes.

The prior E-only source hashes all match before preservation under `artifacts/task90-lane-e-grouped/prior-candidate/`.
The complete prior evidence directory is copied to `prior-evidence/`.
B patch hash is `27b3c7a09eb374d8e61f95b852cef21ff6315d0dafada8a727371c18b2c4cabc`.
`git apply --check` passes and all seven B output hashes match.
The C v2 manifest hash is `7a10709564a7c5108dd579b78e77b5f222a397cc7662e5a29d0de8fa66a37da6`.
Exactly four v1-to-v2 rows differ. Each target matches its v1 bytes before replacement.
The original 31 E files stay unchanged after those inputs.
All B/C inputs are protected and excluded from E staging.
No C production Root file changes. F-P1 stays with C and doesn't enter this graph.

The grouped initial build uses actual prospective version `0.9.0-beta.19`, passes with zero warnings and errors and takes `00:02:35.57`.
The manual host rebuild passes.
The current ten-case ToolHelp class passes, including missing-ID execution without any host or filesystem effect.
The CommandHelp class compiles with all exact prose/order assertions removed. Its 43 final Imprint captures stay pending.
Both CLI Skill Tool references are byte-identical and now use fixed settings paths, named selector scope and native setup after exact restore.

Real CBM installation passes in `D:/of-task90-e-final/journey-8c5e12856e/codebase-memory`.
Only this new private task directory gets an ACL change. Native default discovery finds Codex.
Its version, index and search work directly and over MCP. Archived memory is excluded. Native watcher defaults are read back unchanged.
Real Update reuses verified `0.12.1` with zero downloads.
Real `--yes` Remove is attempted and stops before effects with E-J4.
A separate injected native-preview diagnostic also blocks on absent optional client bindings.
The project stays byte-identical and the native binary and client entries stay present.

The confirmed account path succeeds through the production Root operation in a separate inert fixture.
It checks exact uninstall arguments and environment, live account confirmation, native-first order, project cleanup and an unchanged index sentinel.
No native uninstaller or real persistent PATH/registry operation runs.
The Windows report gives the source trace, exact artifact names and the remaining requirement.
CBM tested metadata stays empty because the real Remove hasn't succeeded.

The final solution build uses the same command given in the Windows report and actual `OpenForgeCliVersion=0.9.0-beta.19`.
It returns 0 with zero warnings and errors in `00:02:03.61`.
The actual Root `--version` output is `0.9.0-beta.19`. Release files are unchanged.
The final manual launcher build returns 0.
The final-source injected account proof runs in `D:/of-task90-e-final/proof-ab15bebde5` and returns 0.
Its receipt labels the version probe and native effects as injected.
All three recorded MCP PIDs are absent. Owned CBM daemon probes report no running daemon.
Native daemon status uses exit 1 for that state. Stop returns 0 with nothing to stop.

The focused final tests all use `--filter-class`, `--minimum-expected-tests`, `--fail-warns on`, `--fail-skips on`, `--no-ansi` and `--progress off`.
Their exact commands are in the Windows report. The final artifacts under `artifacts/task90-lane-e-grouped/` are:

| Class                               | Minimum | Passed | Failed | Skipped | Exit | Artifact                       |
| ----------------------------------- | ------- | ------ | ------ | ------- | ---- | ------------------------------ |
| `ToolHelpIntegrationTests`          | 10      | 10     | 0      | 0       | 0    | `test-help-final.log`          |
| `ToolDocumentationIntegrationTests` | 3       | 3      | 0      | 0       | 0    | `test-documentation-final.log` |
| `ToolJourneyTransitionTests`        | 2       | 2      | 0      | 0       | 0    | `test-transition-final.log`    |
| `CodeGraphCatalogueTests`           | 13      | 13     | 0      | 0       | 0    | `test-codegraph-final.log`     |

No CBM catalogue source or its 12-case class changes in this continuation.
B's 66-case receipt and C's accepted evidence are consumed without rerunning peer classes.
The CommandHelp class compiles. Its 43 payload captures aren't run or accepted.

Both mandatory formatter commands finish on final source:

| Command                           | Exit | Artifact                                                              |
| --------------------------------- | ---- | --------------------------------------------------------------------- |
| `npm run check:dotnet:whitespace` | 0    | `whitespace-final-recorded.log`, `whitespace-final-recorded.exit.txt` |
| `npm run check:dotnet:style`      | 0    | `style-final-recorded.log`, `style-final-recorded.exit.txt`           |

The nine-file Prettier check, production Markdown link checks, CLI Skill byte parity and Docusaurus build pass.
The site build prints Node's localStorage experimental warning and generates static files.
UTF-8, LF, no BOM and `git diff --check` pass for the E slice.
Only Progress and Evidence change in this packet against the frozen input.
The old branch is kept and the index is empty.
The grouped input record has 58 paths, including 53 unchanged protected paths and five assigned overlaps.
The E manifest has 33 paths, including one deletion and the two new injected-proof helper files.
All 31 original candidate files and their prior evidence are kept in the grouped artifact archive.
The final source and artifact hashes are in `artifacts/task90-lane-e-grouped/final-receipt.json`.
The current defect list contains E-J4 only. No shared ABI change is proposed.
No payload baseline is regenerated or accepted, and nothing is staged or committed.

### C v3 input continuation

The coordinator accepts the E-J1, E-J2, E-J3, I-R1 and I-R2 closures and assigns E-J4 to D in `r_9bcb4010c5bb`.
This continuation copies frozen checked inputs and verifies their integration. It adds no architecture choice or mechanism.
Existing BCL file copies and SHA-256 checks keep the update reversible through the exact before bytes.
The requested managed Integration classes are the focused evidence. Native execution and full suites aren't part of this continuation.

All 33 E path hashes match before work.
Their complete bytes and deletion record are copied to `artifacts/task90-lane-e-c-v3/prior-candidate/`.
Both earlier final and grouped artifact directories are copied to `prior-evidence/` in that same directory.
The previous receipt SHA-256 is `54453150647e2f94c9016f4a9eace8ddb7fc4426cadbfa1e3007c5376f5763c7`.

The C v3 manifest SHA-256 is `56bfd6530bead94bc852d5b46f28c437923f4760733172a9cb0de1fb34ca703f`.
It has 36 paths. Exactly five differ from the applied 34-path v2 manifest.
All 31 other v3 rows equal v2 and aren't copied.
The three previously listed targets first match their v2 hashes.
The previously unlisted tracked Framework reader matches core `885b17db3e56ceb6839ba23609e85a4380c75018`, SHA-256 `6c044a0198d532994486faa9e555773051fc37f14b818a85e6ead5112740b85f`.
The new Integration reader test target is absent before copying.
Every source and copied target matches its v3 hash.
The exact five-path record is `artifacts/task90-lane-e-c-v3/input-corrections.json`.
No Unit file or assigned wording overlap is copied.
All 33 E hashes still match immediately after copying.

The copied paths are the Framework reader, Root `ToolCompositionFactory`, `ToolWorkspaceCompositionIntegrationTests`, the new Integration `ToolWorkspaceObservationReaderTests` and C's command-composition receipt.
All five stay protected and outside E staging.
The combined input set now has 60 paths, including 55 protected paths and five assigned packet or wording overlaps.
Only this packet's Progress and Evidence change in E's existing 33-path set.
The other 32 E paths, catalogue tested metadata, help wording and public docs stay byte-identical to the grouped return.
Earlier docs formatting, local links, Skill parity and site receipts are reused.
No site rebuild, native Tool call, account preview, writer fix or payload acceptance runs.

The current managed Release build passes with zero warnings and errors in `00:03:12.89`.
Its command is:

```powershell
dotnet build OpenForge.Cli.slnx --configuration Release -p:OpenForgeCliVersion=0.9.0-beta.19 -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
```

The build exit is 0 in `build.exit.txt`, with matching `build.log` under `artifacts/task90-lane-e-c-v3/`.
The rebuilt Root `--version` returns exit 0 and `0.9.0-beta.19` in `version.txt` and `version.exit.txt`.
The toolchain facts are in `dotnet-info.txt`.
No release file changes.

Only these three managed Integration classes run:

```powershell
dotnet artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Observation.ToolWorkspaceObservationReaderTests --minimum-expected-tests 11 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll --filter-class OpenForge.Cli.IntegrationTests.Commands.Tool.ToolWorkspaceCompositionIntegrationTests --minimum-expected-tests 18 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll --filter-class OpenForge.Cli.IntegrationTests.Commands.Tool.ToolHelpIntegrationTests --minimum-expected-tests 10 --fail-warns on --fail-skips on --no-ansi --progress off
```

All 39 cases pass. The current run artifacts are:

| Class                                      | Minimum | Passed | Failed | Skipped | Exit | Artifact                                        |
| ------------------------------------------ | ------- | ------ | ------ | ------- | ---- | ----------------------------------------------- |
| `ToolWorkspaceObservationReaderTests`      | 11      | 11     | 0      | 0       | 0    | `reader.log`, `reader.exit.txt`                 |
| `ToolWorkspaceCompositionIntegrationTests` | 18      | 18     | 0      | 0       | 0    | `workspace-root.log`, `workspace-root.exit.txt` |
| `ToolHelpIntegrationTests`                 | 10      | 10     | 0      | 0       | 0    | `help.log`, `help.exit.txt`                     |

Both required formatter checks use the final C# source:

| Command                           | Exit | Artifact                                |
| --------------------------------- | ---- | --------------------------------------- |
| `npm run check:dotnet:whitespace` | 0    | `whitespace.log`, `whitespace.exit.txt` |
| `npm run check:dotnet:style`      | 0    | `style.log`, `style.exit.txt`           |

The packet's Prettier check, UTF-8, LF, no BOM and `git diff --check` pass.
All earlier public docs, local-link and site evidence stays unchanged and isn't rerun.
The current source checkpoint, input hashes, E-only paths and reused receipt hashes are in `artifacts/task90-lane-e-c-v3/receipt.json`.
Its matching files are `source-manifest.json`, `input-manifest.json`, `e-file-manifest.json` and `document-checks.json`.
The index is empty and the previous E branch stays unchanged.
E-J4 is still the sole open journey finding. No D patch or catalogue metadata change enters this return.
All 43 help captures and other payload work stay pending with the coordinator.
Nothing is committed.

### Final Remove recheck

The coordinator accepts D's E-J4 correction. This continuation reuses the existing owned-home graph, native safety guard and production process, config, records and backup mechanisms.
The only new E production change is tested-version metadata after successful real removal.
The focused managed tests, actual owned-file deltas and injected account operation cover the changed limit.
There is no new dependency, ABI, public option, storage setting or platform mechanism.

All 33 E paths and all 60 current inputs match before work.
Their complete source bytes and every prior evidence directory are copied under `artifacts/task90-lane-e-remove-final/prior-candidate/` and `prior-evidence/`.
The prior checked receipt SHA-256 is `c663f4d4a8fb614205d8decfb2e7cbab2dfb6fd600f4a75facec09f7094cb1f4`.
D patch SHA-256 is `eeb35af0156d5b8e0615cdc1896baa41ba216ae0d02d706276c64f4f888dae2c`.
D source-manifest SHA-256 is `015bab6f4103ead84dc07242ae3aafb5120904dc8a0c8c277cd3c46bf62914c0`.
`git apply --check --include=src/cli/**` passes, then the same include applies only source.
All twelve C# output hashes match.
No D packet or report is copied. None of its source paths overlaps E's bindings or wording.
All earlier E and B/C bytes still match immediately after application.
The final combined input record covers 72 paths, including 67 protected paths and five assigned overlaps.
Only E's 35-path set is eligible for its later staging.

The initial prospective graph and manual host build, then real `tool remove codebase-memory --yes --format json` completes in the existing private fixture.
The native executable, client files, project, native settings and index match the earlier Update receipt before that call.
Only activity and rendezvous data differ after earlier native control calls.
The earlier successful Install, Status, direct, MCP and Update receipts stay valid and aren't repeated.

The only changed file contents are project `.cbmignore`, `.agents/open-forge.json` and `.agents/open-forge.lock.json`.
The original `.cbmignore` bytes return, and the Tool declaration and lock entry are removed.
All native account files, executable copies, settings and the index keep their hashes.
The 67-file non-log inventory has no new or missing files.
The two existing native cache logs predate Remove and follow the earlier log exclusion policy.
Nine real child calls run. One is native `--version` and eight are local Git backup queries.
No native uninstall or uninstall dry run executes.
The guarded process host stays unchanged.
The result reports Description, skipped native removal, completed config and records and exactly one manual native uninstall vector.
All 329 unchosen possible output templates stay conditional.
Raw command, result, before/after hashes, process logs, inventory and verification are in the final artifact directory.

The final graph repeats confirmed account removal at `D:/of-task90-e-final/proof-a3f7b72b63` with injected processes and downloads.
Live plan and account choices precede preparation and native-first apply.
Exactly one injected uninstall runs with the unchanged native arguments and empty environment overlay.
Project cleanup restores its original bytes and the index sentinel stays unchanged.
The proof returns 0 and records real native uninstall and persistent PATH/registry edits as false.
Only after the real journey and injected proof pass does `CodebaseMemoryCatalogueEntry` add `CodebaseMemoryDefinitions.Version` to `TestedVersions`.
Its matching catalogue assertion uses that same production definition.
The user docs describe the actual completed Windows result.

The final solution build uses the same prospective command shown in the Windows report and returns 0 with zero warnings and errors in `00:02:31.12`.
The final manual host build returns 0. The actual Root version output is `0.9.0-beta.19`.
Release files and package versions are unchanged.

The exact eleven focused commands are in the Windows report and `test-commands.json`.
Every command uses its qualified class, actual minimum count, strict warning/skip policy, no ANSI and progress off.
The final managed population has these results:

| Class                                        | Cases | Passed | Failed | Skipped | Exit | Log                 |
| -------------------------------------------- | ----- | ------ | ------ | ------- | ---- | ------------------- |
| `ToolWriteOperationTests`                    | 36    | 36     | 0      | 0       | 0    | `d-operation.log`   |
| `ToolWriteSafetyTests`                       | 15    | 15     | 0      | 0       | 0    | `d-safety.log`      |
| `ToolWriteRemovePlanningTests`               | 9     | 9      | 0      | 0       | 0    | `d-planning.log`    |
| `ToolWriteDeferredRemovalTests`              | 11    | 11     | 0      | 0       | 0    | `d-application.log` |
| `ToolCompositionInteractionIntegrationTests` | 7     | 5      | 2      | 0       | 2    | `c-interaction.log` |
| `ToolWorkspaceCompositionIntegrationTests`   | 18    | 18     | 0      | 0       | 0    | `c-workspace.log`   |
| `ToolWorkspaceObservationReaderTests`        | 11    | 11     | 0      | 0       | 0    | `c-reader.log`      |
| `ToolHelpIntegrationTests`                   | 10    | 10     | 0      | 0       | 0    | `e-help.log`        |
| `ToolJourneyTransitionTests`                 | 2     | 2      | 0      | 0       | 0    | `e-transition.log`  |
| `CodebaseMemoryCatalogueTests`               | 12    | 12     | 0      | 0       | 0    | `e-catalogue.log`   |
| `ToolDocumentationIntegrationTests`          | 3     | 3      | 0      | 0       | 0    | `e-docs.log`        |

The first CBM command incorrectly used a minimum of 15. It ran all 12 cases successfully but returned policy exit 9.
The corrected minimum is 12 and its repeat returns 0.
The older 15-case subtotal included 12 CBM cases and three catalogue-factory cases.
No test assertion or source changes to reduce the population. The first log and exit receipt are kept separately.
The final population has 134 cases, 132 passes, two snapshot mismatches and zero skips.

Both C account-choice cases pass their semantic assertions before Imprint.
Their only differences are the unresolved conditional output template, absent pre-confirmation backup target and manual command purpose.
The received `approved.json` and `declined.json` captures are copied under `pending-c-account-choice/` for the coordinator.
Their protected expected files stay byte-identical.
No capture is accepted. The 43 shipped-help captures and other payload work stay pending too.

Both mandatory checks finish on final C# source:

| Command                           | Exit | Artifacts                               |
| --------------------------------- | ---- | --------------------------------------- |
| `npm run check:dotnet:whitespace` | 0    | `whitespace.log`, `whitespace.exit.txt` |
| `npm run check:dotnet:style`      | 0    | `style.log`, `style.exit.txt`           |

The nine-file Prettier check, production local-link and Skill parity tests, LF and `git diff --check` pass.
The Docusaurus build returns 0 and generates static files with the same Node localStorage experimental warning.
No unrelated class, full suite or native build runs.
Final input identity, E-only paths, source hashes, commands, exact exits and pending captures are in `artifacts/task90-lane-e-remove-final/final-receipt.json`.
Only Progress and Evidence change in this packet against its frozen input.
The old branch and all prior evidence stay intact. The index is empty and nothing is committed.

## Divergences

None recorded before implementation.
