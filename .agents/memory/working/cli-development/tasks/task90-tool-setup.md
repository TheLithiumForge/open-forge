---
open-forge:
  description: Implement issue 23 tool setup through the round-2 spec and supervised lanes
  tags: [Memory, Working, CLI, Task, Contextual, Active]
---

# Tool setup

Tool setup is Task 90, which implements published tool installation and additive configuration through supervised lanes.
For example, bare Install restores the exact lock versions while tools run through their direct commands.

Status: Task 90 “Tool setup” (phase 3/3): milestone 3/7.

## Outcome

Implement the [Tool setup design](../../../crystallized/documents/cli/technical-designs/tool-setup.md), updated by the maintainer's [round-2 decisions](https://github.com/TheLithiumForge/open-forge/issues/23#issuecomment-6868316792).
The detailed 14 November 2026 source is `of-tool-setup-decisions-2.md`.
It overrides the earlier respec.
The maintainer accepted Phase 1 and authorized Phases 2 and 3 on 14 November 2026.
Report at each phase boundary and stop for the overseer's CI results at PUSH REQUEST.

## Milestones

Complete these milestones in order:

1. Fold round 2 into the spec, schemas, shared types and outlines, verify and commit.
2. Freeze strict lane packets and create one worktree per worker.
3. Start the Worker Watch lanes and report Phase 2.
4. Integrate A through D with local no-fast-forward merges and focused checks.
5. Complete E's catalogue entries, Windows journeys and user docs.
6. Run one fresh adherence review and close every gap.
7. Request hosted CI through the overseer and resolve every reported failure.

## Execution

Phase 1 is direct work by the mastermind.
Phase 2 uses `gpt-6.1-sol` with `xhigh` effort through Worker Watch in PowerShell.
The user's model choice overrides the older workspace roster.
Each worker has its own worktree and strict file assignment.
The [lane packets](../tool-setup/packets/_packets.md) define their scope and focused checks.
Workers return a checked candidate before the coordinator sends their commit continuation.
The mastermind integrates shared composition and final payload snapshots.
The user authorizes local lane merges, overriding the lane file's earlier no-merge rule.
No worker pushes, opens a PR or changes GitHub.

## Evidence

Phase 1 changes documents, schemas and compile-only types. Git keeps the previous tracked files.
The normal BCL and existing dependencies cover these data shapes. No exceptional mechanism is chosen.
Use a managed build, touched tests, formatting, Markdown links, the site build and both .NET checks.
Full suites and Native AOT run in CI. A local NU1903 override stays on the invocation.
Implementation handles developer files and native account effects.
Tests use owned temporary homes. Windows journeys use temporary repositories under D:.
Never modify real shell profiles or persistent user PATH during those journeys.
Tools add no workspace state, lease or recovery dependency under `.open-forge-state/`.

## Checkpoint

Done: Recorded C1 through C7 and committed the frozen packets in 2e517348e. Created five isolated worktrees and verified all five initial workers alive after dispatch.
Now: Task 90 “Tool setup” (phase 3/3): milestone 3/7. D has returned its independent selection and consent code. F prepares the shared planning proposal from B, D and E's evidence.
Next: Close the shared planning contracts, resume dependent implementation and integrate completed branches before E's entries, journeys and docs.
Blocker: Native templates, first-install stored-state observation, concrete channel plans and config removal plans need the shared contract increment. Independent work continues. The shipping Open Forge version is asked of the maintainer.

## Phase 1 checks

The final local checks on 14 November 2026 passed:

- Release solution build with zero warnings and errors. The NU1903 override is invocation-only.
- `ToolSetupDocumentationTests`, 12 passed, zero failed and zero skipped. This includes Markdown links and fresh schema derivation.
- `npm run check:dotnet:whitespace` and `npm run check:dotnet:style`.
- Prettier on changed Markdown and JSON, plus the Docusaurus production build.
- Route Index, followed by a dry run showing all 14 selected catalogues current.
- LF and Git whitespace checks, all 26 copy-source paths and matching CLI Skill reference copies.

No command implementation, real tool installation or payload-snapshot regeneration ran in Phase 1.
The [recorded choices](../../../crystallized/documents/cli/technical-designs/tool-setup-lanes.md#recorded-choices) now contain the maintainer's C1 through C7 answers.
Codebase Memory account uninstall needs live interactive confirmation naming its effects.
With `--yes` or no terminal, remove only Open Forge project additions and print that command.
Project Remove keeps account-wide settings and prints their manual change commands.

## Runs

The managed Worker Watch session is `s_b74431d77d3a`.
All initial workers use `gpt-6.1-sol` with `xhigh` effort.
Each starts clean at `2e517348ef19e34afa6297c7efa7d0db5451b45c` on `feature/tool-setup-<lane>` in its own `tool-setup-<lane>` worktree.
The mastermind is Worker Watch agent `a_8cf5f6b0ccf3`, run `r_6e9c8c3daf44`, with the same model and effort.

The first runs started on 14 November 2026:

| Lane | Agent            | Run              | Assignment                                       |
| ---- | ---------------- | ---------------- | ------------------------------------------------ |
| A    | `a_0ba060b7c8e2` | `r_211b3404ed3e` | Channels, private Node, launchers and Root hosts |
| B    | `a_3ede9fb31b5c` | `r_5d2fd5f2874c` | Additive config and backups                      |
| C    | `a_e8437ba232a1` | `r_f24ce4555513` | Records, reads and workspace findings            |
| D    | `a_c9edc5b737c5` | `r_d1cc8b785375` | Install, Update and Remove                       |
| E    | `a_b49c60a6b38b` | `r_7759f5ae2da2` | Upstream recipes and safe journey plan           |

Use the same agent for related follow-ups after its run ends.
Worker Watch has no live steering.
The packets require a checked candidate before the coordinator's commit continuation.
Implementation, entries, journeys and final adherence review aren't complete at this phase boundary.

F is agent `a_179fed07b6f8`, run `r_d46da867335f`, with the same model and effort.
It starts from the frozen base in `tool-setup-integrator` on `feature/tool-setup-integrator`.
Its only initial supplied file is its own `lane-f.md` packet.
It writes a shared-contract report and proposed patch before any Root implementation.
The coordinator reviews and commits actual shared model changes.

## Phase 3 returns

D's first run returned 47 passing Unit cases for selection and consent, a green Release build and both .NET checks.
Its nine changed files are uncommitted pending the shared channel and removal-plan contracts.
Install, Update and Remove leaves aren't implemented yet.
Review RD1 corrects the packet's extra channel pin.
Bare Install pins the exact lock version and follows the normal existing-supported-install-first channel choice.
Only a named `--channel` is an override.
D's next continuation must remove the implicit lock-channel override and update its focused selection evidence.

E's first run returned twelve checked asset pins, native file inventories and six contract requests.
Its build, twelve documentation cases and both .NET checks passed.
Run `r_fec5e0e39de0` resumes E for tagged-source telemetry reads before installation, bundled runtime entry facts and native plan output details.
No catalogue or real installation ran yet.

The real Windows Remove journey uses `--yes` project-only cleanup.
The native account-uninstall confirmation path uses the production operation with an injected host, since the native Windows command edits the registry.
Baseline real Update with no newer supported release is combined with synthetic version-transition evidence.
No older unsupported release is added for that test.

## Phase 2 base

The C1 through C7 contract update and five lane packets passed these local checks:

- Release solution build with zero warnings and errors.
- `ToolSetupDocumentationTests`, 12 passed, zero failed and zero skipped.
- Both required .NET formatting checks, Prettier and the Docusaurus production build.
- Markdown links, current packet catalogues, LF and matching CLI Skill copies.

The frozen interfaces now carry the entry and workspace for channel installation, the invocation for native config, file hash expectations and backup completion.
Native setting change arguments are named as manual instructions.
These are compile-only changes before the worker execution base is created.
