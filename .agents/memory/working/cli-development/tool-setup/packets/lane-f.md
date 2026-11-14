---
open-forge:
  description: Resolve the shared tool planning contracts before Root integration
  tags: [Memory, Working, CLI, Tool, Task, Contextual, Active]
---

# Integration preflight

Integration preflight is the review that prepares the shared planning inputs needed by the tool implementation lanes.
For example, a channel plan gives D the exact invocation and paths without copying A's manager rules.

Status: Proposal only. Root decides and commits shared contracts before dependent implementation.

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

Your initial writable files are this lane-f.md packet and evidence/shared-contracts.md plus evidence/shared-contracts.patch under .agents/memory/working/cli-development/tool-setup/.
The dispatcher provides this packet as the only expected untracked file at the exact base.
Read the other worktrees and their reports only. Do not change shared models, schemas, other lane files or Root composition in this first run.
The master worktree is D:/Repositories/open-forge-worktrees/tool-setup. Each peer is tool-setup-a through tool-setup-e beside it.

This is the integrator lane F, with an initial bounded contract proposal.
Later Root composition and focused integration testing can transfer to the same agent after the core branches merge.
Do not start that implementation yet.

## Inputs

Read these inspected source locations:

- Framework/Tools/Models/Services/ToolServiceContracts.cs, ToolChannelModels.cs, ToolConfigModels.cs, ToolSetupModels.cs and ToolBackupModels.cs.
- Framework/Tools/Models/Catalogue/ToolSetupSpec.cs and Channels/ToolReleaseAsset.cs.
- The completed lane-D packet at the peer tool-setup-d path, especially its proposed contract increments and first-install observation gap.
- The lane-B packet at the peer tool-setup-b path, especially template/native parser and dropped-receipt publication questions.
- The research report at the peer tool-setup-e path, evidence/upstream-recipes.md. E is adding tagged-source telemetry and bundled-entry facts. Treat current source facts as evidence, not a change you can make.
- src/cli/root/OpenForge.Cli/Composition/CliCompositionRoot.cs, CliStandaloneComposer.cs, CliLibraryComposer.cs and Models/CliCompositionInputs.cs.
- src/cli/tests/support/OpenForge.Cli.TestSupport/Isolation/TestDataHome.cs.

Framework paths above start at src/cli/framework/OpenForge.Cli.Framework/.
Read current production consumers before proposing a change.

## Outcome

Return one minimal coherent proposed patch against the frozen base, with a short report.
Do not implement any command, host, parser or filesystem behavior.
Do not create a new dependency, plugin framework, registry, journal or workspace state.

The proposal must close these actual needs:

1. A channel plan exposes the planned invocation, commands, runtime, launchers and destination paths. It has the Open Forge home input. Install executes that same plan. D doesn't reconstruct package-manager or runtime paths.
2. B has a read-only removal plan with observations and conflicts. Removal revalidates that plan. Config reapply can retire dropped recipes. Define complete remaining receipt publication for one-stage apply/remove results, including untouched receipts.
3. One bounded token grammar and a cohesive resolved recipe context cover the bin, workspace, native user home, client profile roots and slash-form mainRoot that E's actual recipes require. Unknown tokens fail. Null environment values remove an overlay key. Template expansion is not a new config-edit kind.
4. First-install preflight reads CodeGraph's documented stored telemetry file or its documented default before the CLI exists. Never install the tool merely to discover the previous value. Preserve the native off command for writes. Do not probe or persist telemetry identity or queues.
5. Native setup has declared possible outputs for backup/report planning. Use native --plan or dry-run when an installed invocation exists and otherwise a description plus declared possible destinations. Don't reimplement CBM client discovery or limit native behavior to two clients. Recommend the smallest data shape needed for exact backup coverage, profile/legacy paths and conditional .codegraph ignore coverage.
6. Add only the platform fields needed for the bundled Node Linux kernel and system-library requirements E proved. Keep them separate from host wrapper Node requirements. Declare bundled runtime relative entry paths instead of path heuristics.
7. Root can inject homes, process environment and PATH hosts for owned Windows journeys. No real registry or persistent PATH writes during tests. No new production test switch. The confirmed CBM account-uninstall consent path is tested with an injected host; the real journey uses --yes project-only cleanup.
8. The first shipping Open Forge version is asked of the maintainer. Do not invent or bump it in this proposal.

Some calls run before a missing tool's binary exists. Do not require executing that binary to form a first-install plan.
Preflight checks all available requirements before effects. Future native failure remains an effect failure with completed receipts, not an omniscience claim.
Keep existing native behavior and account scope.

## Return

List every changed model/interface signature, defining doc change and affected lane.
Explain ordering and complete-versus-delta receipt sets in a few sentences.
Give explicit shared implementation responsibility: A channels/hosts, B config/templates/backups, C codecs/observations, D writer sequencing and F later Root wiring.
Flag any actual product decision separately from ordinary DTO placement.
Do not mark implementation complete or claim a build for a patch you haven't applied.
Return PROPOSAL_READY, uncommitted, then wait for the coordinator's continuation.

## Progress

Done: The coordinator accepted F-H1 and F-H2 after all 176 cases, the clean managed build, both mandatory .NET checks and E's focused review with no material conformance gap or NR1 finding. The six frozen source hashes match the accepted checkpoint.
Now: Task 90 “Tool setup” (phase 3/3): milestone 3/7. Accepted F-H1/F-H2 correction, with the local commit continuation authorized. C handles central Root composition on the stable provider API.
Next: Return the full commit SHA and source manifest. Whole-feature adherence review follows complete integration and journeys.
Blocker: None for F-H1/F-H2. Root integrates its pending complete MIT notice before shipping.

## Evidence

The initial proposal base was `2e517348ef19e34afa6297c7efa7d0db5451b45c`.
The coordinator fast-forwarded this worktree to accepted contract `09bcb9ebdb89abed343d4c228283a8ee5aeba7c9`, keeping F's three files.
That commit was the first Root preparation base. The current focused review base is recorded below.
Initial `git status --short` showed only this packet, as expected.
Read AGENTS.md, the loader, complete current C# rules, the selected CLI, software, testing and work-planning scopes, CLI architecture, Task 90, exact Tool contracts and supplied lane rules.
Read the current Framework models, Root composition inputs, TestDataHome, peer consumers and B, D and E's reports.
Rechecked E's added bundled-entry, stored-telemetry, native-plan and inventory-supplement sections before finalizing.

CONTRACT_REQUEST F1 is `contract(tool): complete shared setup planning`.
The [report](../evidence/shared-contracts.md) lists every changed declaration, defining text change, affected lane and remaining decision.
The [patch](../evidence/shared-contracts.patch) proposes 22 files, with 419 additions and 29 deletions. It isn't applied.
Its revised full SHA-256 is `04154710409995db3754eb9380b39f5187d1b7631e49f04c6fac3a674072068f`.
It covers concrete channel execution, removal observations, complete receipt publication, bounded tokens, pre-install telemetry, native inventories, bundled requirements and injected Root hosts.
The maintainer question asks which Open Forge version first ships Tool setup. No version changed.

Earlier proposal checks:

- `npm ci` passed, with nine moderate and one high advisory. Tracked dependencies didn't change.
- `dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1` passed on the unchanged baseline, with zero warnings and errors. It didn't build the proposed patch.
- `git apply --check --whitespace=error-all .agents/memory/working/cli-development/tool-setup/evidence/shared-contracts.patch` passed against the frozen base without applying it.
- `npx --no-install prettier --check .agents/memory/working/cli-development/tool-setup/evidence/shared-contracts.md` passed.
- Checking this packet with Prettier reports only the supplied build command's indentation at line 51, outside Progress and Evidence. The original instruction text stays unchanged.
- A read-only Python check verified LF in all three allowed files and resolved nine local links, including new RD1 links and their heading fragments, against the existing or proposed files.
- The revision re-ran `node node_modules/prettier/bin/prettier.cjs --check --stdin-filepath <proposed-path>` through binary LF stdin for all seven proposed Markdown files. All seven passed. A first attempt through Windows text-mode stdin returned formatting differences because that mode translated LF. The binary-input check replaced it.
- `git diff --exit-code 2e517348ef19e34afa6297c7efa7d0db5451b45c` passed. Every tracked file, including shared models, schemas and composition, is unchanged.

No test class changed and zero test cases ran.
The grouped corrections preserve C's native inventory parser, B's setup planning, templates and config, D's effect sequencing and backup completion and F's later Root wiring.
Native command metadata and stored readers are optional. Command reads require a verified installed invocation and authored bounds. CodeGraph V1 prefers its stored reader throughout.
Each profile has a native home policy. Root resolves it without changing normal child environment, and machine facts bind previous values to the resolved home.
RD1 pins only the lock version on bare Install and keeps normal channel choice. D's selector correction waits until the contract lands. `data.tools` carries tool facts ordered by ID.
No baseline build was repeated for the grouped corrections. The proposed patch hasn't been built or executed.
No real tool, native setup, account uninstall, PATH write, registry write or user profile edit ran.
No payload snapshot or fixture was regenerated.
No commit, push, merge or external mutation ran.
The earlier proposal pass left its two evidence files and this supplied packet untracked.
The new report needs the integrator's later Index pass. Common catalogues stay unchanged.
The accepted contract is now committed. Its proposal report and patch are kept unchanged as the reviewed artifacts.

The [Root integration map](../evidence/root-integration.md) records this continuation's read-only preparation.
It lists exact current A/B/C/D/E constructors, C's three binding factories and JSON rendering, current workspace Status/Doctor constructors, central Root files and focused Integration test targets.
It records the concrete user-home versus Open Forge-home mismatch and the missing execution-time read factory connection without adding a private substitute.
A's injected process constructor and resolution appeared during this pass. B's service now implements config and setup planning. D's planners and payloads are partial. E's two catalogue entry files appeared. C advanced to the accepted contract during preparation.
Peer files are mutable partial work. Recheck the final merged signatures before implementation.

Preparation checks:

- `npx --no-install prettier --check .agents/memory/working/cli-development/tool-setup/evidence/root-integration.md` passed.
- A local link check passed for five links across the report and packet, including both report source links.
- LF and allowed-file checks passed. Four expected untracked files are present. Tracked source is unchanged. The packet's original instructions and Divergences are byte-identical, and its only formatter difference is the supplied build indentation.
- `git diff --check` and `git diff --exit-code 09bcb9ebdb89abed343d4c228283a8ee5aeba7c9` passed. Shared models, central Root and the parent Task are unchanged.
- SHA-256 checks confirmed the earlier proposal report and patch are unchanged.

The initial Root report SHA-256 was `86905ae41a93569983e90e9e688dc8b23c9c4178cb601db29f162d93cfb5568f`. This continuation refreshes that report.

No baseline build or test was repeated in this preparation. No test code changed.
No peer file, real tool, persistent PATH, registry or shell profile was changed.
No payload snapshot or fixture was regenerated. That pass stays after E's final wording.
The new report needs the coordinator's later Index pass. F keeps the subsequent Root implementation assignment.

### Records review

The coordinator fast-forwarded F to `6d007254d5ca2937f4874ba5a93161f585814d8c` and preserved the earlier four files.
C's frozen working candidate has the same base and 77 changed paths.
Read its complete packet, changed-files inventory, nine-fix dispatch, source and test manifests, production consumers, accepted Tool/records/read/Status/Doctor contracts, current C# rules and focused tests.
Read E's clean committed catalogue at `1ec6ad921943b881f819f3a8eb66b4796eeabacb`, including native file-valued config bindings and directory roots.
A, B and D's earlier partial signatures aren't treated as finished reviews.

The new [records review](../evidence/records-review.md) records C-R1 through C-R4 with severity, source lines, consequence and smallest correction.
C-R1 covers omitted known ownership fields keeping stale values.
C-R2 covers unknown fields lost in PATH, region and route arrays.
C-R3 covers new authored report labels outside OutputText.
C-R4 covers null machine-array entries escaping validation.
All nine requested mechanisms are present. Complete preservation and the reported validation and placement gaps still need correction.
C's deferred binding and workspace factories close the earlier eager-home connection gap.
Its records now use the Open Forge home directly and support absent-home reads.
No new shared contract request or product decision is proposed.

Consumed C's final evidence rather than rerunning it:

- The 77 source hashes match its manifest and Git changed-file set.
- The final build log reports zero warnings and errors. Both final managed test assemblies match their recorded SHA-256 hashes.
- Ten focused class logs report 50 Unit and 22 Integration cases passed with zero failures and skips. C's packet records zero warnings and the exact minimum-count, warning and skip gates.
- The final whitespace and style logs contain verify-only commands without diagnostics. C's receipt records exit zero.
- Read all six reviewed synthetic Imprint baselines. Stored-off and normalized effective-on facts stay separate. No payload baseline was regenerated.

Report hashes at the records-review checkpoint:

- Root map - `fdac821dc6193c1a8529050b4ece1879b1faf006a9cd205e822c694604c7114c`.
- Records review - `c462acfd53e155c93b68681317aacbf1540b6d06cdc4879f7ecb97a78db873e9`.

Local verification covers Prettier on both reports, their local links, LF, packet section limits, unchanged proposal artifacts, exact allowed paths and unchanged tracked source at the review base.
`git diff --check` and `git diff --exit-code 6d007254d5ca2937f4874ba5a93161f585814d8c` pass.
The supplied packet's sole Prettier difference stays the original build-command indentation outside Progress and Evidence.
Five untracked F files are expected, including the two historical proposal artifacts.
No build, tests, native process, Doctor, install, persistent PATH, registry or shell profile write ran in this review.
No source, model, central Root, peer file or parent Task changed. No commit or integration ran.
Only this packet, the refreshed Root map and new records-review.md changed.
The new review report needs the coordinator's later Index pass.
F keeps the subsequent Root implementation assignment after the checked branches integrate.

### Native home roots

The coordinator authorized this independent slice at `b22e1ec393280935dea93a312f2a1aa337834ad5` before full core integration.
Read the current loader, applicable scopes, complete C# rules, E's committed catalogue and client-binding sections and tagged native path sources.
Refreshed the applicable KeepInMind records before return. Useful findings are kept in this receipt.
The earlier C findings stay routed to C. No peer candidate is changed or treated as integrated.

The new [native home receipt](../evidence/native-home-roots.md) gives exact constructors, immutable fields, all 61 binding keys, later C and D calls, source links and current limits.
The [Root map](../evidence/root-integration.md#home-provider) records the checked provider connection and keeps full registration pending.
The implementation adds ten production files below Root `Composition/Shared/ToolHomes/` and five files below the matching Integration test path.
Only those new directories, this packet, root-integration.md and native-home-roots.md change in this slice.
The provider snapshots the given environment and inputs at construction. Known-folder and filesystem reads start only when its resolved facts are requested.
Explicit injected homes, environment and all client bindings avoid real known folders and privileged hosts.
Node and CBM home policies stay separate. File-valued config bindings stay separate from directory roots.
Native discovery and B's normalized effective environment facts stay with their existing capabilities.

Final verification runs on Windows x64 with SDK `10.0.101`:

- `npm ci` passed without changing tracked dependency files.
- `dotnet build OpenForge.Cli.slnx --configuration Release --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1` exited zero with zero warnings and errors. Log: `artifacts/tool-home-roots/build-accepted.txt`.
- `dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Composition.Shared.ToolHomes.ToolHomeProviderTests --minimum-expected-tests 11 --fail-warns on --fail-skips on --no-ansi --progress off` passed 11 cases. Log: `artifacts/tool-home-roots/ToolHomeProviderTests-final.txt`.
- `dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Composition.Shared.ToolHomes.ToolClientRootTests --minimum-expected-tests 23 --fail-warns on --fail-skips on --no-ansi --progress off` passed 23 cases. Log: `artifacts/tool-home-roots/ToolClientRootTests-final.txt`.
- `dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Composition.Shared.ToolHomes.ToolClientConfigurationTests --minimum-expected-tests 15 --fail-warns on --fail-skips on --no-ansi --progress off` passed 15 cases. Log: `artifacts/tool-home-roots/ToolClientConfigurationTests-final.txt`.
- Every focused command exits zero with zero failures, warnings and skips. The executed total is 49. The platform-policy cases use owned host paths and don't claim native Linux or macOS execution.
- `npm run check:dotnet:whitespace` and `npm run check:dotnet:style` exit zero on the final source. Logs: `artifacts/tool-home-roots/whitespace-final.txt` and `style-final.txt`.
- Prettier, local Markdown links and heading fragments, LF, trailing whitespace, packet section limits and exact allowed-path checks pass. The packet's sole formatter difference stays the supplied build-command indentation outside Progress and Evidence.
- `git diff --check` and `git diff --exit-code b22e1ec393280935dea93a312f2a1aa337834ad5` pass. All tracked files stay unchanged. The two proposal artifacts and records-review.md stay byte-identical.

The artifact folder keeps the 18 changed slice paths in `changed-files.txt`, all 15 source hashes in `source-sha256.txt`, two executable hashes in `test-artifact-sha256.txt` and five tagged-source hashes in `research-sha256.txt`.
The test assembly is `artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll`, SHA-256 `a528e5750ecd256228a660667358b42b14c0eb038537c47eb6d982ec1c3725ae`.
The Root assembly is `artifacts/bin/OpenForge.Cli/release/OpenForge.Cli.dll`, SHA-256 `22987864801100935707d8e740e4a4f8ae37ba84cb86d3ed30fbe9ae8163efb8`.
Its copy beside the test assembly matches byte for byte.
The first build found five test declaration errors. The first focused selections passed 48 of 49 before the quoted Rovo scalar check was corrected. Final checks passed after that correction and the final containment change.

OpenClaw config supports JSON comments and trailing commas. Other JSON5 syntax returns an unavailable workspace binding and blocks when that binding is required.
The report-only CBM runtime binding names the native rendezvous parent. CBM chooses the identity-derived child. No UID, SID or daemon contents are read.
These are recorded limits, not completed full-graph behavior. No shared contract change, new dependency or shipping version is proposed.
No full suites, Native AOT, Doctor, native process, installation, product network request, real user-home store, PATH or registry write, profile edit or snapshot regeneration ran in this slice.
No central Root, A Tools folder, protected model or contract, peer file, other packet or parent Task changed.
No commit, push, merge or integration ran. The candidate is uncommitted for grouped review.
The new routed file is native-home-roots.md. The coordinator's later Index pass handles it.

Report hashes at the original 49-case checkpoint:

- Root map - `7088070846d05ee6564490f16cd2ee4cf77fa9cb94e7f0129b6dd403dda93324`.
- Native home receipt - `1715d1a4ce17b9d909805b0055ad5052a576f6e15624c54feb43b69ad46ce95d`.

### ABI continuation

The coordinator fast-forwarded F to `fd2b1e4e12cf25d70d205db1e4158dc22b6bc8a9`, tree `20c25b59229c7907411ba616e1d696e88e2638e5`, keeping the slice.
Read the loader and current applicable scopes, full C# sources, selected Task and current shared declarations. Refreshed KeepInMind records before return.
All 15 source files matched the original checked candidate before adaptation.
Added only `IToolDownloadHost.ReadNpmMetadataAsync(ToolNpmMetadataRequest, CancellationToken)` to the owned unused-host fake, with the same throwing behavior as its other members.
Removing that one line restores its prior SHA-256. All ten production files and four other owned test files stay byte-identical.
PATH add and removal signatures are unchanged. The fake constructs no result, so the required `PendingBackups` members need no invented value.
The committed npm entry-path declaration and record grant method require no provider caller adaptation.
E's OpenClaw native-parity answer is pending. This continuation changes no parser, dependency, include handling or binding.

Final managed Windows x64 checks use SDK `10.0.101`:

- `dotnet build OpenForge.Cli.slnx --configuration Release --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1` exits zero with zero warnings and errors. Log: `artifacts/tool-home-roots/abi/build.txt`.
- `dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Composition.Shared.ToolHomes.ToolHomeProviderTests --minimum-expected-tests 11 --fail-warns on --fail-skips on --no-ansi --progress off` passes all 11 cases. Log: `artifacts/tool-home-roots/abi/ToolHomeProviderTests.txt`.
- `dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Composition.Shared.ToolHomes.ToolClientConfigurationTests --minimum-expected-tests 15 --fail-warns on --fail-skips on --no-ansi --progress off` passes all 15 cases. Log: `artifacts/tool-home-roots/abi/ToolClientConfigurationTests.txt`.
- Both affected class commands exit zero with zero failures, warnings and skips. Only those 26 cases rerun. The other 23 binding cases keep their earlier result on b22e1ec, without a fresh execution claim.
- `npm run check:dotnet:whitespace` and `npm run check:dotnet:style` exit zero. Logs: `artifacts/tool-home-roots/abi/whitespace.txt` and `style.txt`.
- Prettier, LF, trailing whitespace, 12 local Markdown links and anchors, packet section limits, exact allowed paths and unchanged historical artifact hashes pass. The packet's sole formatter difference stays its supplied build indentation outside Progress and Evidence.
- `git diff --check` and `git diff --exit-code fd2b1e4e12cf25d70d205db1e4158dc22b6bc8a9` pass. No tracked file changed.

The fresh Root assembly is `artifacts/bin/OpenForge.Cli/release/OpenForge.Cli.dll`, SHA-256 `6af37fb1fa2dce32323e8188b6bd081ea8119196931a38f0f0b4a1333e532402`.
The fresh Integration assembly is `artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll`, SHA-256 `7a9ab3f3c0c60298bdd423c746c22876f88af473396ecc38eebf3a3cf16277ed`.
The Root copy beside the test assembly matches byte for byte.
The ABI artifact folder contains `changed-files.txt` for 18 slice paths, `source-sha256.txt` for all 15 source files, `adapted-files.txt` for the one changed fake, `shared-inputs-sha256.txt` for eight shared inputs, `configuration-sha256.txt` for seven unchanged inputs, `test-artifact-sha256.txt` for both binaries, `report-sha256.txt` for the three final Working files and `untracked-files.txt` for exactly 21 expected untracked files.
The three predecessor manifests keep the original 49-case candidate identities. Build and package inputs are unchanged, so the previous `npm ci` pass applies.
The final Root map SHA-256 is `421838371f4a8dc14e369e2799f0a8730d8189253dabfd4c9213f099cb3fdb45`.
The final native-home receipt SHA-256 is `b81387c2a1fff10a5a7ddcfd3fe7bd26552504115ad1ef17676c69bed4f9f269`.
The receipt and map contain the final ABI connections. No new routed file is added in this continuation.
The prior proposal artifacts and records review stay byte-identical.
Only the owned fake, this packet and the two assigned reports change in this continuation.
No central Root, A host, protected type, peer file, parent Task or other leaf changes.
No full suite, Native AOT, Doctor, tool process, installation, persistent PATH or registry write, profile edit or payload regeneration runs.
No commit, push, merge or integration runs. Return READY_FOR_COMMIT and wait for the coordinator's continuation after the native-parity answer.

### Intermediate commit

The coordinator accepted the ABI return and authorized one intermediate local commit with the existing F files and reports.
E confirmed F-H1, additional native JSON5 syntax, and F-H2, zero-length native config fallback.
Both corrections are unresolved before final acceptance. Include precedence is correct.
The coordinator selected a bounded read-only C# port after the checked published libraries failed native validation or duplicated behavior.
F keeps provider and parser implementation. No parser work starts before the dated contract increment lands.
E prepares exact tagged functions, licence and fixtures in `r_0a42fc165fa0`.
Only this packet and the two assigned report statuses change in this continuation.
All 15 source hashes, the earlier proposal artifacts and records review stay unchanged.
Reuse the accepted managed build, the affected 26 cases and prior 49-case evidence and both mandatory .NET checks. No executable check is rerun for these status-only changes.
Markdown formatting, 12 local links and anchors, LF, whitespace, exact staging paths and packet section limits pass before committing.
Git whitespace checks cover the 20 non-patch files. The unchanged historical unified diff keeps its 37 required blank context lines, each a single space. Its original SHA-256 is checked instead of changing the reviewed patch.
The commit includes all 21 expected existing F files. The full SHA and file list are returned after Git creates the commit.
This is an intermediate reviewable base, not full native syntax support or final acceptance.
No central Root, A host, protected type, peer file, parent Task, parser implementation, push, merge or payload regeneration is part of this increment.

### Native reader preflight

The current base is `f2200252bfff42c2b260df7375b92a468cf63263` with a clean initial worktree.
I1 accepts the smallest bounded native-format port after ordinary readers failed the required outcomes.
This read-only local developer-file adapter reuses the bounded eight MiB file read and physical paths. Git keeps prior source and owned fixtures keep test files separate from real stores.
The three private support files port only validation, fixed byte-key lookup and selected string decoding. Numbers are skipped without conversion. Strict UTF-8 validation and codepoint encoding use BCL support.
No package, writer, include expansion, interpreter, native bridge or public parser API is added.
The Integration project is required by its existing Root reference. One new pure-reader class and the changed owned-file class cover syntax and caller effects at that allowed tier.
Full suites and Native AOT stay for CI. No live native process or user store is used.
E's complete source, function, licence and fixture plan is `evidence/native-config-reader.md` in the read-only peer E tree. The current design's Native config reader section and I1 define this implementation.
Root keeps the upstream MIT notice in LICENSE before shipping. Each derived source identifies the tagged native path and that notice.
Only the six assigned source and test files and F's packet and two reports can change.

### Native reader checks

F-H1 and F-H2 are fixed in the uncommitted candidate on `f2200252bfff42c2b260df7375b92a468cf63263`, tree `4ac0207e0f6deb51620f3f8cc118aa770ee3048d`.
The three new private files port native document validation, fixed byte-key lookup and selected string decoding. The caller keeps existing home, environment, include and profile precedence.
Key matching and value decoding keep separate native escape branches. Unrelated values and numeric tokens are validated and skipped without conversion. Strict UTF-8 validation and codepoint encoding use BCL support.
The caller's eight MiB file bound and the native 64-depth grammar are covered through their production constants.
The syntax cursor stays in its one assigned file because lexical and recursive validation share native byte offsets and depth. The one source-mapped fixture oracle serves both focused classes from their nearest shared test scope.
The obsolete BCL syntax-limit claim is removed from the current receipt. Its unquoted missing-path case now has native missing coverage instead of an unavailable expectation.
All provider constructors and immutable fact fields stay unchanged. C keeps central Root composition on that API.

Final managed Windows x64 checks use SDK `10.0.101`:

- `dotnet build OpenForge.Cli.slnx --configuration Release --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1` exits zero with zero warnings and errors in `00:02:02.02`. Log: `artifacts/tool-home-roots/native-reader/build-corrected.txt`.
- `dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Composition.Shared.ToolHomes.Shared.OpenClaw.ToolOpenClawConfigurationReaderTests --minimum-expected-tests 151 --fail-warns on --fail-skips on --no-ansi --progress off` passes all 151 selected and executed cases. Log: `artifacts/tool-home-roots/native-reader/ToolOpenClawConfigurationReaderTests.txt`.
- `dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Composition.Shared.ToolHomes.ToolClientConfigurationTests --minimum-expected-tests 25 --fail-warns on --fail-skips on --no-ansi --progress off` passes all 25 selected and executed cases. Log: `artifacts/tool-home-roots/native-reader/ToolClientConfigurationTests.txt`.
- Both execution commands exit zero with zero failures, warnings and skips. The total is 176. Filtered `--list-tests --no-ansi --progress off` discovery confirms the same 151 and 25 counts in `reader-discovery.txt` and `configuration-discovery.txt` in the same artifact folder.
- `npm run check:dotnet:whitespace` and `npm run check:dotnet:style` exit zero on the final source. Logs: `artifacts/tool-home-roots/native-reader/whitespace.txt` and `style.txt`.
- The seven build, package and formatting inputs still match the earlier `npm ci` and restore hashes. Dependency setup isn't repeated.
- Prettier, local links and anchors, LF, whitespace, packet section limits and the exact nine-file assignment pass before return. The packet's supplied build indentation stays its sole formatter difference outside Progress and Evidence.

The first build finds five fixture annotation errors after a reflection-dependent string serializer call triggers formatter fixes. The fixture now uses `JsonEncodedText.Encode`, and all generated AOT attributes and reflection-dependent serializer calls are removed.
The corrected build and first focused executions pass. No test flake occurs.
The final Root assembly is `artifacts/bin/OpenForge.Cli/release/OpenForge.Cli.dll`, SHA-256 `8f38b24c22c696923d4cde8d9cf3a4e943a2db71ec589a23fd99fbce3beb1871`.
The final Integration assembly is `artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll`, SHA-256 `2610bbb73a637d3e2faa72d36909746634d9a798a78bbf383bf047b126b7918b`.
Its Root copy matches the built Root assembly byte for byte.
The native reader artifact folder keeps `changed-files.txt` for all nine candidate paths, `source-sha256.txt` for the six assigned source files, `configuration-sha256.txt` for seven unchanged inputs, `test-artifact-sha256.txt` for both binaries, `upstream-sha256.txt` for the two checked tagged sources and `report-sha256.txt` for the three final Working files.
The final native-home receipt SHA-256 is `3e791faab40d08e7c106614e00ef2b51ec4b1b841dcb5d5f0b0b17c58fb9ea4d`.
The final Root map SHA-256 is `3d91a7224880ce2b8469948284b65912e1bca8f3705724ff1d42df7f13980b1e`.
No new routed Markdown file is added.
Only the six assigned source and test files and the three assigned Working files change. Protected models and contracts, LICENSE, central Root, A hosts, peers, other packets and the parent Task stay unchanged.
Root appends the complete upstream MIT notice before shipping. Every derived production source identifies the tagged native source and that notice. This remains a Root integration task.
No native tool, real user store, process host, network request, persistent PATH or registry write, profile edit, package, writer, interpreter or native bridge is used by the provider or tests.
No full suite, Native AOT, Doctor, payload fixture or snapshot regeneration, commit, push, merge or integration runs.
Return READY_FOR_REVIEW, uncommitted. Full Root wiring proceeds separately through C.

### Reader commit

The coordinator accepts the correction and authorizes the conventional local commit of exactly nine assigned files.
Read E's complete focused report at `evidence/native-reader-review.md` in its read-only peer tree.
Its `r_73e82470b77f` check reports no material conformance gap and no NR1 finding.
The six frozen source hashes match before and after review and again before this commit.
The accepted checkpoint is `c72327e8f458cb28cc1608090da61fc1a41f8ae724d16294f220215aaf3528c3`.
The unchanged source manifest SHA-256 is `eab5fcc34c86b3b3b30cb74ec1c986276012a48d8304ed73e537721f8ed51376`.
The unchanged nine-path manifest SHA-256 is `75e8e353210ea8ccee62007ac46388e1284132c13b0e0763dcb3248d5f5d7ab0`.
Only this packet and the two assigned report statuses change in the continuation. The accepted build, 176 cases and both mandatory checks stand without unrelated reruns.
Final Markdown, links, LF, whitespace, frozen hashes, packet section limits and exact nine-file staging checks pass before the commit.
Root has retained the complete DeusData MIT notice in its pending LICENSE addition, with words unchanged and line endings and trailing blank spaces normalized. F doesn't edit LICENSE or central Root.
The full local SHA and unchanged source manifest are returned after Git creates the commit.
F-H1 and F-H2 are closed. The provider API stays stable for C.
No native execution, new test run, payload capture, push, merge or integration is part of this continuation.
E's focused conformance check isn't the later fresh whole-feature adherence review.

## Divergences

None recorded before the run.
