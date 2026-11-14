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

Done: Packet prepared from the accepted design.
Now: Awaiting the assigned Worker Watch run.
Next: Verify the execution base and complete the ordered steps.
Blocker: None identified before implementation.

## Evidence

No implementation or focused tests have run for this lane yet.

## Divergences

None recorded before implementation.
