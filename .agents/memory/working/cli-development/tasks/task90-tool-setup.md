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
Now: Task 90 “Tool setup” (phase 3/3): milestone 3/7. Shared planning and native observation contracts are committed. Five implementation lanes are running. F returned the Root map. The catalogue slice is integrated. Core command integration is pending.
Next: Review the complete implementation candidates, commit and integrate their branches, then complete E's journeys and docs.
Blocker: No implementation blocker after this contract increment. The first shipping Open Forge version is asked of the maintainer.

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

A's first pass returned 93 passing focused cases, a green Release build and both .NET checks.
Its independent version, manager policy, transfer, archive, runtime, launcher and PATH components are uncommitted.
The channel service still needs the shared command-resolution, existing-install, runtime-receipt and removal-inventory shapes.

B's first pass returned 61 passing focused cases, a green Release build and both .NET checks.
Its independent config editors and backups are uncommitted.
Dropped recipes, templates, native values and deferred backup completion still need the shared contract.
Automatic approval review rejected cleanup of failed-test temporary directories as "blocked by policy".
Those directories are kept. Final fixtures clean up successfully and no flake was reported.

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

## Shared planning checks

F returned revised proposal run `r_3cccfc39ce08`.
The reviewed patch SHA-256 is `04154710409995db3754eb9380b39f5187d1b7631e49f04c6fac3a674072068f`.
The coordinator applied it and added A's resolved invocation, existing-install, runtime receipt and fresh removal inventory shapes.
The Release solution build passed with zero warnings and errors.
All twelve focused `ToolSetupDocumentationTests` passed.
Both required .NET checks, Prettier, Markdown links, LF and Git whitespace checks passed.
This increment changes shared types and defining documents. It adds no command behavior.

## Continued runs

The shared planning contract is committed as `09bcb9ebdb89abed343d4c228283a8ee5aeba7c9`.
The coordinator fast-forwarded idle A, B, D, E and F worktrees to that base, keeping their changes.
C completed its first pass, then received the shared contract while idle.
The continued runs use managed session `s_b74431d77d3a`:

| Lane | Run              | Scope                                                    |
| ---- | ---------------- | -------------------------------------------------------- |
| A    | `r_000591b628d8` | Complete channels and pinned private Node                |
| B    | `r_fac7d075731b` | Complete config, native setup plans and deferred backups |
| D    | `r_cf548ae21d0d` | Complete writer commands and fix RD1                     |
| E    | `r_fceca7f98984` | Typed catalogue entries before real journeys             |
| F    | `r_810ecdfafa97` | Read-only Root integration map                           |

The first shipping version is still asked of the maintainer.
E uses `0.9.0-beta.19` provisionally in entry metadata and doesn't bump release files.

## Records review

C's first pass returned forty-two focused tests, a Release build with zero warnings and errors, both .NET checks and six reviewed new Imprint baselines.
Its own packet records the classes and receive artifact. Final payload fixtures haven't been regenerated.
Review queues Open Forge home alignment, missing-home reads, complete native-setting identity and unavailable runtime/PATH status.
F's Root map also requires execution-time read bindings and deferred workspace observations.
C's requested inherited native override fact is added to the shared observation shape.
Root passes a narrow native environment map in recipe context. Reports carry only the deciding variable and effective on/off value.
This keeps raw environment values and native identity out of reports and records.

The additional shared types built with zero warnings and errors.
The first focused invocation used an incorrect class name and the minimum-count gate rejected zero selected tests.
The corrected full class name ran all twelve documentation tests successfully.
Both required .NET checks and Prettier passed. The override stays on the local build invocation.

## Current dispatch

The home and native override contract is committed as `6d007254d5ca2937f4874ba5a93161f585814d8c`.
C continued in run `r_bd49f34db450` after its idle worktree advanced to that commit.
Its grouped review covers contract adaptation, native inventory parsing, home alignment, deferred construction and read findings.
F returned `PREPARATION_READY` for run `r_810ecdfafa97`.
Its Root map records constructor and composition prerequisites. F's implementation starts after the checked lanes merge.
A, B, D and E are still running against `09bcb9ebd` and receive the optional observation addition when idle.

The read-only issue snapshot includes the body and all nine comments.
The latest comment is still the round-2 summary `issuecomment-6868316792` from 14 November 2026.
The review bundle and draft fresh-review packet are kept under `artifacts/tool-setup/` until dispatch.
No GitHub content was changed.

## Catalogue integration

E's reviewed catalogue slice is committed as `1ec6ad921943b881f819f3a8eb66b4796eeabacb` in continuation run `r_a9c85c87d42c`.
It contains nineteen assigned files, twelve pinned assets, unchanged native recipes and twenty-eight focused Unit cases.
The final symbolic no-download value update passed a fresh build, thirteen affected cases and both .NET checks. The other fifteen accepted cases were unchanged.
Both tested-version arrays stay empty until actual native journeys succeed.
The metadata minimum is still provisional `0.9.0-beta.19`. Release files weren't bumped.
The combined catalogue tree passed its Release build with zero warnings and errors, all twenty-eight new catalogue tests, twelve documentation tests and both required .NET checks. The coordinator closes the no-fast-forward merge after those checks.
The [upstream recipe report](../tool-setup/evidence/upstream-recipes.md) gives F the exact client bindings.

Review A-R1 queues Corepack read-probe controls from its official documentation.
Review D-R2 separates project uninstall with incidental account telemetry from destructive account removal.
D-R3 checks the callable parameter limit. D-R4 checks minimum Open Forge preflight enforcement.
These grouped reviews wait for the corresponding worker to return idle.

C returned the full grouped continuation with seventy-two focused tests, a clean build, both .NET checks and six reviewed synthetic Imprint baselines.
F reviews that frozen candidate in run `r_aeb323402330` before its commit continuation.
Final payload snapshots and native journeys are still pending.

## Core review fixes

The catalogue merge is `b22e1ec393280935dea93a312f2a1aa337834ad5`.
F's focused review confirmed all nine C mechanisms and found four corrections.
C-R1 clears omitted known ownership fields while keeping unknown data. C-R2 keeps unknown PATH, region and route fields by typed identity.
C-R3 moves new wording to OutputText. C-R4 rejects null machine-array elements at ingress.
C fixes the group in run `r_f7ac869adb14` after its idle worktree advanced to the catalogue merge.

F's independent Root provider starts in run `r_81083d102942` on that same base.
Its exclusive new files are root `Composition/Shared/ToolHomes/**` and matching Integration tests.
A keeps `Composition/Shared/Tools/**`. F resolves homes and declared client paths. It doesn't discover clients or wire the full graph yet.
This independent slice uses the committed catalogue while the core lanes finish. Full Root composition still follows their integration.

## Planning completion

A's full candidate returned 162 passing focused tests and three shared requests.
The combined contract adds an authored package-relative npm entry, bounded npm metadata reads without a client and pending PATH backup handles.
The catalogue's known `npm-shim.js` entry and its portability check move with that required field. E is idle during this one-time caller update.
A supplies twelve signed-checksum Node pins for 22.23.5 and 24.22.3. Its own packet records the GPG evidence.

D returned 105 passing focused tests and a request for explicit path-grant publication.
The existing Tool interface already keeps `--allow-path`. A separate `ToolRecordGrantRequest` and record-service call publish only settings using the existing codec.
It touches no tool lock, machine state or runtime. Dry-run doesn't call it. C implements it and D reports the separate explicit edit before tool planning.
This avoids adding unrelated tool records as a side effect of authoring a grant.

B's three grouped fixes returned 67 passing affected tests. C's four fixes returned 47 passing affected tests with unchanged snapshots.
Both reported clean builds and mandatory checks. B is ready for its commit continuation. C also needs the new grant method.
The combined shared types and catalogue caller build with zero warnings and errors.
Thirteen affected catalogue tests, twelve documentation tests, both .NET checks, Markdown formatting and Git whitespace checks pass.
No behavior implementation is added by this contract increment.
