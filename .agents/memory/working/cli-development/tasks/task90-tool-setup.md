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

Done: The core is committed. The native reader and C's full Root candidate are checked. The CI whitespace correction passes both formatting commands. The maintainer approved I1 and the minimum Open Forge version 0.9.0-beta.19.
Now: Task 90 “Tool setup” (phase 3/3): milestone 3/7. E completes final docs and real Windows journeys. F diagnoses the observed Codebase Memory preflight failure.
Next: Integrate the checked reader and Root, close the native journey gap, then run the one payload pass and fresh adherence review.
Blocker: Codebase Memory planning reports an unavailable recipe value before writes. Its exact cause is under investigation. CodeGraph's real journey completes.

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

## Integration queue

The final planning interfaces are committed as `fd2b1e4e12cf25d70d205db1e4158dc22b6bc8a9`.
The active continuations are:

| Lane | Run              | Scope                                                       |
| ---- | ---------------- | ----------------------------------------------------------- |
| A    | `r_021ff77a91c8` | npm metadata, entry paths, PATH backups and Corepack probes |
| C    | `r_18df73167656` | Settings-only grant publication and final interface use     |
| D    | `r_ebf7110b001f` | Removal scope, version floor, grants and pending backups    |
| E    | `r_da8ba8d01e58` | Read-only review of B against the native catalogue          |
| F    | `r_81083d102942` | Independent Root home and client-path provider              |

B's frozen correction candidate is from `r_208391bf8dcf`.
Its 138-test receipt and 67-test correction receipt are recorded in its packet.
Commit continuation follows the focused catalogue review.
The final fresh adherence review is separate and follows integration and journeys.

E's review returned E-B1 and E-B2 against the unchanged 39-file B checkpoint.
A name-specific probe match can falsely show directory coverage.
A later negation can defeat a prepended block while byte verification succeeds.
Both source paths were checked and routed together to B in `r_96829394863e`.
E commits its separate review report in `r_875162c17667`.
The rest of the native recipe, telemetry, receipt and backup checks found no further gap.

## Checked returns

C's 81-file implementation is committed as `9f4b0a4b192bc3875b38dcc2d0dfd82be99f6158` in `r_d8f059cc68f6`.
Its final 69 focused tests, clean build and both .NET checks pass.
The settings-only grant writer was checked against its expected-state helper and owned fixtures.
It changes no tool lock, home or runtime state and returns pending backup handles.
The six synthetic baselines are unchanged.

D's `r_ebf7110b001f` return passes 153 focused tests, a clean build and both .NET checks.
The account-effect classification, prerelease minimum check, grant sequence and PATH backup handling were checked.
Commit continuation is `r_ba759816d60a`.
Root supplies the actual running version and only the declared native environment values.
Nine synthetic snapshots were reviewed. Final payload snapshots are still pending.

F's provider passes 49 focused cases for all 61 declared bindings.
Its execution-time home resolution and independent physical client bindings were checked.
Continuation `r_3d69f04b2c96` checks the latest shared interfaces at `fd2b1e4e1`.
E checks one reported OpenClaw syntax limit against CBM's actual tagged reader in `r_673c4611e7b4`.
That question doesn't authorize a new parser or dependency.

The separate review evidence commit is `240c78f3449147fb9993f3fecc8c45a879583241`.
Its staged main-worktree merge passes a clean build and twelve focused documentation tests.
Both required .NET checks pass before the merge commit.

## Records integration

The review evidence merge is `c8fb87aa2b5f7e5e6d465c1a35d7c02719058b69`.
C's implementation merge is staged above it.
The combined managed build passes with zero warnings and errors.
All twelve touched classes pass, with 62 Unit cases and 50 Integration cases.
The exact reproduction selections are the current C packet classes with counts 8, 10, 8, 15, 5, 4, 4, 6 and 2 for Unit, then 19, 22 and 9 for Integration.
The invocation keeps snapshot updates disabled and uses the existing minimum-count, warning and skip gates.
Logs are under `artifacts/tool-setup/merge-records/`.
Both .NET checks pass before its merge commit.

D's implementation is committed as `41ef63a7fc8987836e74315ea3450fe8bdd7273c`.
Checking its leaf connections found D-R5.
The three writer bindings take constructed operations, which would resolve host facts during Root composition.
Correction run `r_37c1c9c2353a` changes those inputs to execution-time factories and checks construction, help, invalid input and once-only execution.
The planning and effect implementation stays unchanged.

E's native-source check confirms F-H1 and F-H2.
CBM supports JSON5 syntax beyond the current BCL reader and treats a zero-length file as missing.
Its include precedence already matches the provider.
E investigates an ordinary compatible parser in `r_70a2ecb5dfbd` before a separate dependency choice.
The mastermind can make the smallest reversible choice under the user's direction and records it as a maintainer question.
No custom parser, include resolver or dependency is added by the current provider slice.

## Provider integration

F's checked intermediate provider is committed as `3afe9b3ed29cba34aca4fbe1452ae36e073d358e` in `r_fc1ae7358727`.
Its latest ABI pass has 26 affected tests, a clean build and both .NET checks. The earlier 49-case receipt still applies.
The staged combined merge passes a clean build, all 49 provider cases and twelve documentation cases.
Both .NET checks pass again on the combined source before its merge commit.
The original proposal patch keeps its checked SHA-256 and required unified-diff context spaces. Git whitespace checking covers the other files.

The two package readers checked by E don't match native validation or duplicate handling.
Under the user's delegated choice, I1 uses a private read-only C# port of CBM's tagged grammar and selected-path lookup.
The technical design records its exact scope, tests, replacement condition and maintainer question.
E's source and licence plan is from `r_0a42fc165fa0`; its report commit continuation is `r_4a4930c295f5`.
F implements after the dated contract and its base provider merge.
Root keeps the native licence notice before shipping derived code.

The new [central integration packet](../tool-setup/packets/lane-c-root.md) transfers command registration to C on the checked core base.
F keeps the private provider and parser files, while A keeps process, network and PATH hosts.
This lets central wiring and the native-reader correction proceed in separate files.

## Final core returns

A's `r_021ff77a91c8` return has 191 passing focused tests, a clean build and both .NET checks.
Corepack read overlays, bounded metadata and PATH backup handles were checked.
Commit continuation is `r_e916c0ac6394`.
B's `r_96829394863e` closes E-B1 and E-B2 with 69 affected tests and clean gates.
Its owned Git preview checks candidate coverage and conflicts without changing an unterminated negation.
Commit continuation `r_73f2e23f0951` also replaces repeated ignore filenames in those tests with the mechanism's constant.

D-R5 passes 38 binding cases, including 24 factory-lifecycle cases.
D-R6 adds pure catalogue validation so unknown named IDs and channels are rejected before the factory resolves homes.
The combined correction and commit continuation is `r_576e6838e9e3`.

## Core merge

All core implementation heads are staged in one user-authorized no-fast-forward merge:

- A `38cc6a791a0bd5b8d31565e5e78232ef4d97ddfc`, with 58 files and 191 passing cases.
- B `46834001af0470d5cff565e772339856e335fa52`, with 39 files and its accepted focused receipts.
- D `9044c04c2b758e9414a1992b743923d4180331f3`, above `41ef63a7fc8987836e74315ea3450fe8bdd7273c`, with 45 final binding cases.
- E `973d223de7c59eb01e29d1f124106d28acff943f`, with the native-reader source and licence plan.

The branches have separate file assignments. This combines their checked commits without replaying or squashing them.
The full upstream MIT text is added to the existing root licence for F's derived reader.
Its words stay unchanged. Line endings and trailing blank-line spaces follow repository formatting.

The first combined build found 101 compiler errors from the new `Tools.Shared.Path` namespace hiding `System.IO.Path` in B and C.
The integrator renames that one internal capability folder to `Paths` and changes its eight namespace references.
No shared model or behavior changes. The same frozen API still connects the services.
The first log is kept as `artifacts/tool-setup/merge-core/build-namespace-collision.log`.
The corrected combined build and all touched A, B and D test classes run before acceptance.
The selected matrix is written under `artifacts/tool-setup/merge-core/` with each class's command outcome.
Final payload regeneration is still reserved for the end.

## Core corrections

After the namespace correction, the combined build passes with zero warnings and errors.
Both required .NET checks also pass on that source.
The first focused run stops in `ToolProcessHostTests.BoundsOutput` because its owned temporary directory is still held during cleanup.
The failed log is kept. A investigates the actual process cleanup in `r_be4c67dc773b` and returns an uncommitted checked correction for this merge.
The accepted earlier implementation commit stays in the merge history.

The first selection used A's earlier nineteen-class matrix instead of its final twenty-class receipt.
The missing metadata class is added from `abi/test-summary.json` and all seventeen cases pass.
The completed core selections now have 557 passing cases across forty classes, with zero failures or skips in those completed results.
The process-host class is still unresolved. The full selected minimum is 562 cases across forty-one classes.
Completed class receipts are kept while the failed class awaits its correction.

C's preflight is accepted from `r_f54be8a0f8f2`.
It needs no additional catalogue input field.
C-ROOT-ISOLATION extends C's integration assignment to the normal typed host entry and existing in-process capture and Status/Doctor fixture adapters.
Those adapters forward owned Tool inputs before any composed workspace observation.
C-ROOT-ENTRY changes D's factory input to the bound request and cancellation token and returns an asynchronous leaf operation.
D closes that connector in `r_e886f3ce5a0c` with typed factory failures and cancellation.
C updates the closed preflight choices in `r_6d01abc62e89` and waits for the checked core base before implementation.
F's independent native-reader correction continues in `r_a0614d63c6b0`.

## Final integration fixes

D's C-ROOT-ENTRY return passes seventy binding cases and both required checks.
Its six-file uncommitted patch is checked against `9044c04c2` and applied to the pending merge.
The five source hashes match D's manifest.
The earlier forty-five-case report is kept under a separate filename and isn't counted as current evidence for that changed class.

A's A-R2 return passes eight process-host cases, including root and child reaping for output, timeout and cancellation.
Source review finds a Windows exit-observation race consistent with the held directory. The original log doesn't identify its holding PID.
The correction uses the BCL's bounded process-handle wait, drains pipes after termination and joins cleanup tasks.
Its host/test and receipt patches are checked and applied to the pending merge.
The old failed process-host log and temporary directories are kept.
No shared model, constructor, global test-support or payload change is needed.
The main worktree rebuilds once for both corrections, then reruns the affected classes and both formatting gates.

F's native-reader correction is committed as `ffef620bca8ae506c611fc3f244e63a72ceae1ec`.
E's focused check finds no material conformance gap against the tagged native functions.
The six-file checkpoint is `c72327e8f458cb28cc1608090da61fc1a41f8ae724d16294f220215aaf3528c3`.
The 176-case receipt and both .NET checks pass in F's worktree.
The provider API stays unchanged. Its merge follows the core gate.
The fresh whole-feature review, native journeys, final docs and payload captures are still pending.

## Core acceptance

The corrected combined Release build passes with zero warnings and errors.
The final focused receipts contain 590 passing cases across 41 classes, with zero failures or skips.
The changed binding class contributes seventy cases and the changed process-host class contributes eight.
Their earlier receipts stay separate from the counted current results.
Both required .NET checks pass on the final source.
The final gate receipt is `artifacts/tool-setup/merge-core/gates-final.json`.
C receives this checked core as the base for central command composition.
Milestone four completes after that command graph is integrated.

## Native reader integration

Core integration is committed as `885b17db3e56ceb6839ba23609e85a4380c75018`.
C starts central command composition from that exact base in `r_30c5d61f744c`.
The independent native reader and its source review merge without conflicts from F `ffef620bca8ae506c611fc3f244e63a72ceae1ec` and E `3a6469a8e1e7119281b842640f069c6d0456ad44`.
That next merge stays separate from the core CI checkpoint.

Its fresh Release build passes with zero warnings and errors.
Its two touched classes pass 176 cases and the touched Tool documentation class passes twelve.
The first reader selection omitted its `Shared.OpenClaw` namespace and the minimum-count policy rejected zero tests.
That receipt is kept separately. The corrected selection supplies the counted result.
Markdown formatting removes one pre-existing extra indentation in F's packet command.
Both required .NET checks pass with exit zero on the final reader source.
Receipts live under `artifacts/tool-setup/merge-native-reader/`.
No payload regeneration, real tool journey or final whole-feature review has run yet.
The core CI checkpoint requests `feature/tool-setup` at `885b17db3e56ceb6839ba23609e85a4380c75018`.
The later reader merge is checked but uncommitted at that request.
Phase three continues after the overseer returns CI results.

## CI correction

The overseer reports run `43573635294` on PR 32 for `885b17db3`.
Only `check:dotnet:whitespace` failed, in the compact native-environment loops of Tool Inspect and Status presentation.
The checks job stopped platform builds, so no full-suite result is available.
The integrator expands both branches into braced blocks without changing behavior.
Both required formatter commands run on this corrected source before integration.
Every resumed lane records both command exit codes before each commit and repeats them after source corrections.

The corrected Release build and both formatter commands pass with exit zero.
The twelve Tool documentation cases also pass.
Receipts are under `artifacts/tool-setup/ci-43573635294/`.
Repository-wide `format:check` reports seventy files.
One is this feature's A packet table alignment, which the integrator formats.
The other sixty-nine paths are unchanged from `75f6677ad` and stay outside this task.
The changed Markdown check passes. The full formatter result isn't reported as green.

The maintainer approves I1 and the next beta's minimum `0.9.0-beta.19` in the task conversation.
The design and lane plan record both answers.
Generic allow-listed native config changes can turn off declared behavior with disclosure. Telemetry is the first case.

## Root acceptance

C returns 34 files and 113 passing focused cases from `r_30c5d61f744c`.
The integrator checks the lazy graph, typed request factories, host injection, optional consent, workspace appenders and synthetic snapshots.
The candidate and the bounded synthetic minimum-version correction in `r_0ec339952fb4` are accepted.
That correction keeps the real running version and uses one fixture-owned `0.1.0` minimum.
Its seven interaction cases pass under normal beta.18, prospective beta.19 and restored beta.18 builds.
All three builds and both final formatter commands pass.
Only the fixture, its two reviewed synthetic baselines and the receipt change.
The final 34-file frozen manifest is `artifacts/tool-setup/c-root-reviewed-inputs-v2/manifest.json`, SHA-256 `7a10709564a7c5108dd579b78e77b5f222a397cc7662e5a29d0de8fa66a37da6`.
Two workspace-journey cases still await `references/tools.md` in the final payload inventory.
They aren't counted as passing evidence.

E starts journeys and final docs in `r_222c03e86918` on the exact core base with frozen reviewed C and reader inputs.
The input patch SHA-256 is `d17391264d0044123887ea0a11b8d1f1a7a868180599171e13fc1a4584230707`.
The C input manifest SHA-256 is `422e0d7db326a8f07f13d0a91232c2dafa51a8c82e6ea09c7020c48e47bbe5a9`.
E keeps those other lanes' files as protected build inputs and stages only E's assigned files after review.
The coordinator compares the tested inputs with the final merged tree.

## Payload preparation

F returns the [single final pass plan](../tool-setup/evidence/payload-final-pass.md) from `r_de752f710493`.
It identifies four payload inventory corrections, the 43-page help class after E's additions, the affected snapshots and the 23-file default-layout fixture.
The plan distinguishes normal build embedding from baseline mutation.
It also gives Index, link, site and focused verification commands.
No product files, fixtures, snapshots or Git state changed during that preparation.
The coordinator copies the checked report and waits for E's final wording before executing the pass.

## Journey checkpoint

E's real CodeGraph Install, Status, Update and Remove reports are completed.
Its direct command and MCP captures are produced in the owned Windows fixture.
The native uninstall runs before Open Forge cleanup. The CLI and index stay installed.
The original exclusion stays. Telemetry stays off and the result prints the native command to change it.
The final E report will separate native writes from Open Forge additions and removals.

Codebase Memory Install is blocked before writes by `tool-install.native-plan-blocked`.
The cause names an unknown or unavailable recipe token without the token itself.
F investigates the exact input and responsible source in `r_8b1b64214346` while E finishes independent help and docs.
No protected implementation changes are authorized by that diagnostic assignment.

E records E-J1 for the unavailable fresh-client bindings, E-J2 for CodeGraph's commented native ignore form and E-J3 for required parser arity blocking Inspect and Remove help.
B receives the grouped config corrections in `r_ea945188d402` on a new branch from the core base.
The design clarifies deferred client-specific outputs before native inventory and the required post-placement refresh before native writes.
E keeps both help-binding files and receives the arity corrections in its next grouped follow-up.
The coordinator stops newly assigned C `r_640877561762` and D `r_87ee4e95fe5c` before source edits because those paths had already transferred to E.
Both processes stop. C's 34-file manifest is unchanged and D's six existing async-entry files still match the committed core.
This keeps the final help corrections with one file assignment.

F's [exact token diagnosis](../tool-setup/evidence/cbm-journey-blocker.md) confirms E-J1 without adding another finding.
`${client:continue-config}` is Windows output declaration 209 after 208 resolved declarations.
The before/after file manifests match and `usedNetwork` is false.
The provider correctly leaves absent Continue, TRAE, Roo and Cody configuration paths unavailable.
B's repair must keep that native path behavior and the post-placement inventory guard.

F also confirms F-P1, a published-process isolation gap, without opening a real store.
The planned `PublishedInstallProcessTests.LegacyFilesAreNeitherReadNorDeleted` case reaches workspace Status and Doctor.
Their current combined record read reaches the real known-folder Tool store before checking declarations.
The other four planned published classes don't reach that reader.
The affected class stays unrun until the correction and final payload inventories are ready.
C receives F-P1 in `r_8c12583d9180`.
Its bounded correction checks project declarations before graph and home construction and leaves all declared-tool and Tool command observations unchanged.
The design and Status/Doctor contracts record the no-tools eligibility rule.
No new option, environment flag or production test mode is authorized.

## Grouped correction returns

B's `r_ea945188d402` return closes E-J1 and E-J2 in nine uncommitted files.
The integrator checks complete token validation, conditional deferral and post-discovery requirements, plus Git-backed recognition of comments around the reviewed ignore forms.
Its 27 expansion, 20 setup and 19 ignore cases pass, with a clean build and both final-source formatter exits zero.
Constructors and frozen models are unchanged.
The source-only seven-file patch SHA-256 is `27b3c7a09eb374d8e61f95b852cef21ff6315d0dafada8a727371c18b2c4cabc`.
The checked nine-file candidate is frozen under `artifacts/tool-setup/b-journey-reviewed-inputs/`.

E's first final return has 31 uncommitted assigned files, a clean build, site, links and format gates, 24 passing focused cases and the two E-J3 help failures.
The integrator finds I-R1, the required literal-help assertion migration is incomplete, and I-R2, two CLI Skill statements need version-selection and fixed settings-path scope corrections.
E receives those grouped fixes, the checked B patch, C's four v2 input corrections and both E-J3 arity edits in `r_585cef10e61b`.
Its full CBM journey and confirmed-account injected evidence are still required before catalogue acceptance.
No shipped payload baseline has been accepted.

C's F-P1 production correction is accepted from `r_8c12583d9180`.
The integrator requires its eleven file-backed eligibility cases to move from Unit to Integration.
`r_2025b6b17cff` makes that exact move and updates the namespace and Evidence traits.
All eleven Integration cases pass, with a clean build and both formatter exits zero.
The unchanged eighteen actual-Root cases keep their prior passing receipt.
The earlier Unit result is historical evidence, not the accepted tier.
The complete 36-file C candidate is frozen under `artifacts/tool-setup/c-root-reviewed-inputs-v3/`.
Its production bytes, synthetic snapshots and cancelled Inspector binding stay unchanged by the move.

E's grouped run closes E-J1, E-J2, E-J3, I-R1 and I-R2 in its recorded rechecks.
CBM installs in an owned private fixture after the upstream activation check rejects the shared temporary parent's ACL.
The safety check stays intact. Its MCP transcript is produced.
E-J4 then blocks real `--yes` project-only Remove.
The planner calls optional account uninstall dry run before choosing whether to skip it.
That upstream path opens HKCU even in dry run, so the safety host refuses it before execution.
An injected preview also exposes unchosen optional client-binding validation.
D receives E-J4 in `r_9bcb4010c5bb` with the exact writer-only file assignment.
It must keep optional account uninstall descriptive until live choice, preserving concrete validation and native-first order only for chosen effects.
The design and Remove contract record that rule. E's help binding files stay protected.

E's `r_92a413f6ccfa` incorporates exactly five accepted C v3 inputs and checks the combined graph.
Its eleven reader, eighteen Root workspace and ten help Integration cases all pass.
The prospective beta.19 build and both final formatter exits pass. Unchanged docs and site receipts are retained.
All 55 protected inputs match, and E's 33-path candidate is preserved under `artifacts/tool-setup/e-reviewed-before-remove/`.
E-J4 and CBM's final tested-version update stay open pending D's return and the real removal recheck.

D's `r_9bcb4010c5bb` returns the checked E-J4 correction in fourteen uncommitted files.
The integrator checks the descriptive request, original deferred declaration, live-choice preparation, prepared-state reporting and unchanged CodeGraph project path.
Its build and 71 focused cases pass. Both final-source formatter exits are zero.
Bindings, constructors, authored wording and frozen Framework models stay unchanged.
The twelve-file C# patch SHA-256 is `eeb35af0156d5b8e0615cdc1896baa41ba216ae0d02d706276c64f4f888dae2c`.
The full fourteen-file candidate is frozen under `artifacts/tool-setup/d-remove-reviewed-inputs/`, with manifest SHA-256 `a25a59f7b34d6376d3b85460f58253dc5daa81c8f077e9bc58012da1fc23edf7`.
E receives only the checked source inputs in `r_81bfd04a6acf` and reruns real project-only CBM Remove.
The live native account command and its dry run stay forbidden in the real fixture.
Final tested-version metadata still waits for that successful recheck.


## Finishing direction

The maintainer redirects Phase 3 on 2026-11-14.
CI runs every test. Local execution uses only the managed build and both .NET formatting checks before each commit.
The one local exception produces the payload snapshots and default-layout fixture after all lane changes are combined.
E finishes its active real Remove verification before receiving the commit continuation.
A, B, C, D and F receive the no-tests direction and commit their finished files.
The coordinator commits the pending integration, merges every lane with `--no-ff`, produces the payload baselines once and sends the integrated SHA for CI.
The fresh adherence review and simplification work don't start before this push checkpoint.
Phase 4 needs approval. Its first deliverable is a proposal with file paths, expected line savings and risks for each change.


## Integrated CI checkpoint

The coordinator combines all finished lanes on `feature/tool-setup` on 2026-11-14.
Every lane receives the maintainer's no-local-tests direction in its commit continuation.
Each continuation runs only a managed build and both .NET formatting checks. Every check exits 0.
E's previously active real Remove verification finishes before its commit continuation.
No test suite runs locally after the redirect. The coordinator runs only the authorized baseline producers.

The local lane commits and Worker Watch runs are:

| Lane | Commit | Commit run |
| ---- | ------ | ---------- |
| A | `bb77c511bba6e894cdc3ae6ee5e0ffe982110cda` | `r_a27309e8c97d` |
| B | `22b8f83a54a94a9c24aa7e03807b39b17d1ce0ee` | `r_e061a86431f2` |
| C | `b225c371b0de9a60b977bf49b01c7a1d98cdf2d5` | `r_b7d7591b6d03` |
| D | `1c904c7ffbd8eb024a06c8e7abc0f52e118e55f1` | `r_900c27b73893` |
| E | `3559870e7bca266bda2e57ff0f9aff386ac3166d` | `r_f0eb4d16144b` |
| F | `049e6c0dff5293273fced51f459477a2255cee67` | `r_3894e90e584c` |

The coordinator commits its pending 50 files in `97ae832e24819a4b16279c8369d01fafcdf72f17`.
That merge keeps F's native reader, E's reader review, the approved contract updates and the two CI whitespace fixes.
Both compact renderer branches now use braces. Their behavior stays the same.
The lane merge commits are `de2aa612e`, `870337a78`, `f26fbde6a`, `ee71474cb`, `941301b11` and `5c561fa5f`.
Each uses `--no-ff`.
A's packet conflict keeps its latest completion record and the formatted tables.
E's three conflicts keep E's checked final group wording and help renderer.
All other changes merge without conflict. Root composition comes from C.

E's real Remove continuation is `r_81bfd04a6acf`.
It closes E-J4. CBM project-only Remove exits 0, restores the original project additions and keeps all account installation files and settings.
Native uninstall and its dry run never execute in the real fixture.
The separate injected proof checks confirmed account removal and native-first order.
Both catalogue entries now record their completed Windows journeys, CodeGraph `1.6.2` and Codebase Memory `0.12.1`.
The [Windows journey report](../tool-setup/evidence/windows-journeys.md) gives native writes, direct command and MCP results, Update and Remove effects.

### Payload generation

The single generation window uses integrated HEAD `5c561fa5fabb3ad335f17abd4dca5deec1a8d9f0` plus five inventory corrections.
Its input diff SHA-256 is `44ad3a645127fe6603ba725d72822674f516b6c36c9950e7ae7a72efc59421ed`.
Four fixture lists add the shipped `references/tools.md` after `references/routes.md`.
The documentation inventory drops E's deleted step-0 help outline.
The combined managed solution build exits 0 with zero warnings and errors in 00:03:31.65.

The coordinator sets `OPENFORGE_SNAPSHOT_UPDATE=1` only for the selected producers and restores both snapshot environment variables afterwards.
The twelve serial selections execute 79 cases. Every selection exits 0, with 79 successes, zero failures and zero skips.
There is no verification rerun or ordinary semantic-test selection.
For the table below, I means `OpenForge.Cli.IntegrationTests` and U means `OpenForge.Cli.Core.UnitTests`.
Each assembly is `artifacts/bin/<assembly>/release/<assembly>.dll`.
Every command is `dotnet <assembly> <selection> --minimum-expected-tests <cases> --fail-warns on --fail-skips on --no-ansi --progress off`.

| Assembly | Selection | Cases |
| -------- | --------- | ----- |
| I | `--filter-class OpenForge.Cli.IntegrationTests.Shell.Presentation.Shared.Help.CommandHelpIntegrationTests` | 43 |
| I | `--filter-method OpenForge.Cli.IntegrationTests.Framework.Distribution.WorkspaceLayoutPayloadRenderingIntegrationTests.CustomGuidanceMatchesReviewedTemplates` | 1 |
| I | `--filter-class OpenForge.Cli.IntegrationTests.Commands.Install.InstallBeforeOutputSnapshotTests` | 10 |
| I | `--filter-class OpenForge.Cli.IntegrationTests.Commands.Install.InstallChangeOutputSnapshotTests` | 3 |
| I | `--filter-method OpenForge.Cli.IntegrationTests.Commands.Install.InstallOperationIntegrationTests.SafeAbsentAutomaticApplyIsExactAndIdempotent` | 1 |
| I | `--filter-class OpenForge.Cli.IntegrationTests.Commands.Install.InstallSetupRecapIntegrationTests --filter-not-method OpenForge.Cli.IntegrationTests.Commands.Install.InstallSetupRecapIntegrationTests.ResolvedEssentialsWizardRetryPreservesSelection` | 4 |
| I | `--filter-method OpenForge.Cli.IntegrationTests.Commands.Install.InstallOutsideRouteSetupIntegrationTests.RegistersAfterInstall` | 1 |
| I | `--filter-method OpenForge.Cli.IntegrationTests.Commands.Install.InstallOutsideRouteSetupIntegrationTests.FailedRegistration` | 1 |
| I | `--filter-class OpenForge.Cli.IntegrationTests.Commands.Status.StatusBeforeOutputSnapshotTests` | 5 |
| U | `--filter-class OpenForge.Cli.Core.UnitTests.Commands.Tool.Models.Read.ToolReadPresentationTests` | 2 |
| U | `--filter-class OpenForge.Cli.Core.UnitTests.Presentation.Tool.Shared.Writing.ToolWritePresentationTests` | 6 |
| I | `--filter-method OpenForge.Cli.IntegrationTests.Commands.Tool.ToolCompositionInteractionIntegrationTests.AccountChoice` | 2 |

Each command also writes a CTRF result with `--report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup/finish/snapshots/<selection-name>`.
The selection names and full arguments are in `artifacts/tool-setup/finish/snapshot-receipts.json`.
The final generation changes 262 snapshot files. Seven are new Tool help pages.
The reviewed changes add Tool to root help, add the new Skill reference and its counts and update payload token totals.
The two account-choice captures keep conditional output templates before confirmation, omit a premature backup target and label the printed command `manual-native-uninstall`.
The semantic assertions in those two producers pass before the capture is written.
The other 35 existing help pages have no content changes.
Imprint's host line endings are converted to LF. Terminal newlines and all other snapshot bytes stay as generated.
The diff and grouped review are under `artifacts/tool-setup/finish/` as `payload-snapshot-review.diff` and `semantic-baseline-review.txt`.

The default-layout fixture is produced once with the built managed development executable in an owned temporary workspace and child home.
Its child environment redirects HOME, USERPROFILE, APPDATA, LOCALAPPDATA, XDG_DATA_HOME and OPENFORGE_DATA_HOME.
The two argument vectors are `install --automatic` and `install --automatic --format json`.
These are Framework Install calls. No Tool or package-manager command runs.
The capture records all workspace files except the runtime state directory and replaces the workspace path with `<workspace>` in output.
The fixture grows from 22 to 23 files. Its lock lists 19 Framework paths.
Decoded settings use schema v6. The lock uses schema v2.
The changed payload bytes are the CLI Skill and new Tool reference. No old payload file disappears.
First Install reports 19 files. The second Install stays completed with no effects or findings.
The reviewed fixture SHA-256 is `3bb99311f1bcc47521835dc64ba2ae3b1b8b939dfb653dd3491f5b429bbf10e6`.
The received fixture stays under `artifacts/payload-final-pass/`.

Index previews, then updates only the Tool setup Working subtree.
It creates the evidence entrypoint and includes the C Root and F packets.
The follow-up preview exits 0 with no effects or findings.
No repository Doctor, broad Repair, site build or unrelated check runs in this continuation.

### Final gates

The final managed build command is `dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false --disable-build-servers -m:1`.
It rebuilds the fixture resource and exits 0 with zero warnings and errors in 00:00:54.90.
The NuGet override stays on the invocation. No release version changes.
Both final checks, `npm run check:dotnet:whitespace` and `npm run check:dotnet:style`, exit 0.
The changed source, documents and generated baselines pass UTF-8, LF and BOM checks and `git diff --check`.
Full managed suites, all platform builds and Native AOT execution await hosted CI.
The fresh adherence review hasn't started. Phase 4 simplification hasn't started and needs approval.
After committing this checkpoint, the coordinator requests the overseer's push and stops for CI results.


## Adherence corrections

The maintainer authorizes A1-A5 from the fresh review on 2026-11-14.
Simplification stays unapproved and unstarted.
The corrections use the original A-D lanes on new branches from `350768fbb`.
They run no local tests or snapshot producers. Their commit gates are the managed build and both .NET formatting checks.

The shared contract adds structured permission observations to config plans, explicit machine/runtime record deletion selections and cancellation flags beside completed progress.
A per-invocation permission list carries explicit preview grants without saving them.
A separate resolved recipe on the observation supplies complete plan values without expanding inserted literals twice or exposing unrelated existing file contents.
The original recipe stays the apply input.
These fields have defaults so existing typed fakes still compile until their regression cases are updated.
Record omission keeps its existing upsert meaning. Only selected IDs or runtime versions request guarded deletion.

A handles channel cancellation. B handles destination permissions, config cancellation and resolved recipe facts.
C handles guarded record deletion and record cancellation.
D handles writer permission preflight, deletion publication, cancellation precedence and complete text/JSON/consent plans.
The coordinator restores the two corrupted UTF-8 examples for A5 and keeps shared models, this Task and integration.
Each lane has exclusive production files and its matching tests. B's config fixtures and D's real-graph fixtures get explicit grants where success requires them.
The fresh reviewer will recheck A1-A5 against the integrated correction without running tests.
Any hosted CI failures supplied before the push checkpoint join this correction batch.
