---
open-forge:
  description: Implement Task 90 lane A channels and hosts
  tags: [Memory, Working, CLI, Tool, Task, Contextual, Active]
---

# Channels and hosts

Lane A is the implementation that detects published channels and installs one active tool with verified bytes and direct launchers.
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

You alone edit Framework/Tools/Shared/{Supply,PackageManagers,Runtimes,Launchers,Path,Versions}/** below src/cli/framework/OpenForge.Cli.Framework/, and src/cli/root/OpenForge.Cli/Composition/Shared/Tools/**. You own matching paths in Unit and Integration test projects and this lane-a.md packet. C owns all record codecs. B owns config edits and backups. The integrator owns root command composition, central registration, shared models and schemas.

Implement IToolVersionService, IToolChannelService, IToolProcessHost, IToolDownloadHost and IToolPathHost from Framework/Tools/Models/Services/ToolServiceContracts.cs. Keep host implementations in Root and channel policy in Framework. Expose a focused construction entry that Root composition can use. Root supplies resolved home paths. No new public environment switch unless already accepted or returned as a contract request.

## Steps

Follow this ordered implementation plan:

1. Check Git base and applicable context. Read all Models/Catalogue, Channels, Launchers, Records and Services files. Read existing root process/network capabilities before making another owner. Reuse only the exact lane-A copy sources in tool-setup-lanes.md, through git show, with their tests. Never cherry-pick.
2. Implement minor/exact spec parsing and profile matching at Framework/Tools/Shared/Versions/. CLI minor.x is invalid. Catalogue/settings minor.x is valid. Reject prereleases and versions below the profile floor. Verify all cases through focused pure tests.
3. Implement bounded command detection and npm-family capability facts under PackageManagers/. Probe npm, pnpm, yarn and bun separately, excluding Yarn Modern globals. Honor per-client minimums, the entry's RequiredClient, runtime requirements, project-lockfile preference, multiple-client choices, --yes fallback and global-bin readiness. Prove choices through injected process results, not a real account install.
4. Implement published npm global installation, existing-install reuse and GitHub resolution/download/extraction under Supply/. Do not silently make a second install or switch channels after a failure. Honor registry/proxy trust and avoid leaking credentials. Exact versions are shown and recorded. Direct assets need catalogue-pinned SHA-256 or verified attestation. Use manager verification honestly, without claiming dependency closure.
5. Implement private Node under Runtimes/. Use official portable Node archives with SHA-256 pinned from signed SHASUMS256. Skip archive links in preparation before the unchanged link-rejecting extractor. Run the real node executable with npm-cli.js at its platform path. Install packages with that Node into tools/<id>/<version> so native addons match it. Share versioned runtimes. Several versions can coexist. Never proxy Node or put it in bin.
6. Implement absolute-path .cmd and executable sh launchers plus direct native binaries under Launchers/. Update rewrites them. Open Forge is absent at run time. Verify Windows argument quoting and ordinary Unix permissions in owned files. Record the cmd /c MCP constraint as native journey evidence, without rewriting upstream integration.
7. Implement PATH observation and the Root platform host. Interactive Windows writes use Environment.SetEnvironmentVariable(..., User), never setx or system PATH. Unix offers detected startup files and a custom file, writing self-identifying marked blocks only to chosen files. Show exact manual lines and restart instructions. Removal needs live confirmation and no launcher can still use a deleted runtime. Tests never execute real persistent user PATH or profile edits. Inject a platform host or exercise only redirected files.
8. Build and run only the tests you created or touched. Check no real user paths changed. Run both .NET checks. Return the construction entry, files, exact test evidence and READY_FOR_REVIEW. The coordinator handles the commit continuation.

## Acceptance

Cover stable spec selection, supported existing reuse, no duplicate install, npm-family requirements and preference, Node absence, pinned private runtime installation, archive escape/link rejection, SHA mismatch, bounded timeout/cancellation/output, dry-run latest-only lookup disclosure, launchers and chosen PATH files. Verify using local fixtures and owned temporary homes. GitHub production access is not required by tests. CodeGraph's version probe must carry CODEGRAPH_NO_DOWNLOAD=1 exactly as declared by E's later entry. Never alter normal tool fallback behavior.

Dependency direction and no real PATH writes matter independently of test counts. Do not add package-manager implementation policy to command rendering or record codecs.

## Progress

Done: Packet prepared from the accepted design.
Now: Awaiting the assigned Worker Watch run.
Next: Verify the execution base and complete the ordered steps.
Blocker: None identified before implementation.

## Evidence

No implementation or focused tests have run for this lane yet.

## Divergences

None recorded before implementation.
