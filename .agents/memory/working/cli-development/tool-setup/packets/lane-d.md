---
open-forge:
  description: Implement Task 90 lane D install update and remove
  tags: [Memory, Working, CLI, Tool, Task, Contextual, Active]
---

# Install update and remove

Lane D is the implementation that plans tool writers, obtains consent and sequences native setup and additive project changes.
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

You alone edit Operations Commands/Tool/{Install,Update,Remove}/**, Commands/Tool/Shared/Writing/** and Commands/Tool/Models/Writing/** below src/cli/operations/OpenForge.Cli.Operations/. You own the matching leaf Rendering and OutputText files, tests and this lane-d.md packet.

A owns channels and privileged hosts. B owns config/backups/profile digest. C owns records and reads. You consume only frozen service interfaces and injected catalogue/home facts. No private copies of their mechanisms. Root Tool group composition, group registration, shared Models and schemas are protected. Each leaf binding accepts its parent Command and keeps dependencies typed. Report exact constructors and presentation APIs for integration.

## Steps

Follow this ordered implementation plan:

1. Check exact base, all service/model contracts and Install/Update/Remove leaf contracts. Inspect a current writer such as Commands/Library/Attach/LibraryAttachOperation.cs and its binder/planner/presentation, plus Shell's typed binding pipeline. Follow existing input, invalid-result, status, cancellation, detail and source-generated result conventions.
2. Implement Install named/bare selection and id@version. Named bare ID gets newest support. Bare Install restores the exact locked version and channel, otherwise latest within the declaration. Update uses the same named grammar and moves declared tools to newest support when bare. A supplies spec/range interpretation. --channel is valid only with a named spec. No --automatic, --allow-download or value-taking --version.
3. Preflight the entire selected batch in ID order before effects. Resolve versions, compatibility, channel/manager, native declared effects and additive drift, then form one concrete plan. Any blocked tool blocks all effects. Ask about manager choices unless --yes fallback applies. A single usable project-lockfile preference wins first. Never create a second install and disclose account-wide replacement.
4. Render a plan with commands, exact versions where known, paths, scopes, installer writes, additions, telemetry, verification, downloads, backups and PATH choices. --yes accepts that plan and downloads. Without terminal and without --yes return missing consent without effects. JSON never prompts. Dry-run uses native preview when available and otherwise a description. Network only resolves latest, with disclosure.
5. Apply install/update through A, verify detection, back up declared native outputs through B and invoke upstream setup unchanged via the process host. Then plan/apply B's additions one at a time, publishing verified lock and machine receipts through C before continuing. Stop on first effect failure and preserve completed/unstarted work. Don't roll back package-manager effects or invent interrupted receipts. Update rewrites launchers and retains older profiles.
6. Implement Remove with the latest C2/C7 decisions. CodeGraph's local native uninstall can precede project cleanup as declared. Codebase Memory uninstall is account-wide, including its binary and all agent entries. Only a live interactive confirmation that names those effects permits it, and never with --yes. With --yes, no terminal or a declined confirmation, skip that uninstall, print the native command and remove only Open Forge project additions. Intentional retention is not itself incomplete removal.
7. Never restore account-wide native settings on a project Remove. Report kept settings and B's manual native change commands. No cross-project registry. Never run the package-manager uninstall; print it. Ask separately before deleting Open Forge-placed shared tool bytes or unused runtimes, and offer PATH removal after the last tool. --yes/unattended paths never delete shared files or edit PATH. Repeated project cleanup is a no-op and conflicts retain their receipts.
8. Build and test the real operations with injected service fakes only at network/process/interactive boundaries and real owned temporary record/config files where crossing OS behavior. Cover named/bare and pinned restore, preflight-none, --yes, no TTY, dry run, exact channel/version plans, reapply, self-update, first failure, native-first confirmed ordering, CBM skipped uninstall and kept account-wide settings.
9. Run both .NET checks and return READY_FOR_REVIEW with all entry/binding/presentation signatures, exact focused evidence, payload snapshot impact and any contract needs. The coordinator handles the commit continuation.

## Acceptance

Every advertised path executes the accepted sequence and has focused evidence. No placeholder success or NotImplemented path. No Tool run/choose/configure/roles, no own MCP/Skill/guidance edits, no workspace Tool state or lease. Keep upstream behavior and its argument vectors. Own command results rather than parsing output text. Use Imprint snapshots for text and direct semantic assertions for effects and consent boundaries. Never exercise actual account installs or real PATH/profile changes in this pass.

## Progress

Done: Packet prepared from the accepted design.
Now: Awaiting the assigned Worker Watch run.
Next: Verify the execution base and complete the ordered steps.
Blocker: None identified before implementation.

## Evidence

No implementation or focused tests have run for this lane yet.

## Divergences

None recorded before implementation.
