---
open-forge:
  description: Prove native CI and reproducible artifact collection for all six accepted platforms
  tags: [Memory, Working, CLI, Task, Distribution, NativeAOT, CI, Contextual]
---

# Task 13: Native CI and Reproducible Artifacts

## Scripts README And Help Follow-up

The maintainer requests discoverable script documentation and useful command
help, followed by local squash integration. Direct implementation and review,
no helpers. Isolated branch codex/delivery-help starts at 54c28134b. Scope is
README prose, help metadata/rendering and conventional -h support. Existing
build/test/package/publication behavior remains authoritative. No new
dependencies or native build changes. Focused evidence: bootstrap and every
command help through the real entry point, static checks, readable examples
and local Markdown links. No .NET or native qualification is triggered by
this presentation-only change. Complete: the scripts index and delivery README,
command-specific prerequisites, positional usage, outputs, examples and -h
support are implemented. Direct review and all 47 delivery regressions pass,
including every command’s help in a fixture without node_modules. TypeScript,
lint, formatting, diff checks and README local-link checks pass. Reproduce with
`npm run check:delivery` and `npm run test:delivery` using Node 24.19.0/npm
11.17.0. Representative pack, release:collect and version help was inspected
against the actual stage implementations. This closeout accompanies authorized
local squash integration into develop. No remote effects occurred.

## Unified Delivery CLI And Visible Stages

The maintainer requested clearer arguments and pipeline stages, packing despite
failing local Mac tests, and a small unified CLI for setup/build/test/pack/version.
Selected design: dependency-free Node/TypeScript `forge` entry point plus one command
catalog, short npm aliases, existing focused task modules, dist --plan and
named stage diagnostics. The private root npm package registers the `forge` bin,
so `npx forge <command> [options]` works from the repository root without the
extra argument separator. npm owns local command installation and invocation.
No bootstrap compilation or global link is needed. Offline fixture execution
proves the registered command works before node_modules exists and passes its
options through. All 41 delivery regressions pass after this addition.
The six suites mean execution modes on the current host, not six platforms.

Worktree: /tmp/open-forge-delivery-stages; branch codex/delivery-stages;
original base 8770d24a7, rebased onto 5bb4aefa without conflicts. Verified
implementation commit 72101ed5 became 9e6855e79dfc28fd0809725312bd63ca2f39b6b8
(tree b73f2f463150c649852022c0324df5b2a7ed89eb). Delivery code, tests, package
configuration, workflows and delivery prose are byte-identical across that
rebase. Concurrent C# view work is preserved; the remaining closeout changes
only task state. Standard direct implementation and self-review, no helpers.
The original phase 3/3 implementation and local verification passed. The
maintainer accepted the direction and extended this horizon with target-selective
publication. The maintainer confirmed exact wrapper dependencies for the
selected version. Extension phase 3/3, milestone 3/3: implementation, verification and direct
review passed. This closeout accompanies the authorized local squash. Selection is
frozen at pack time and recorded through wrapper/native package metadata and
release.json. The default Actions graph stays all six. Existing wrapper
versions with different dependencies fail preflight before any uploads.
Evidence covers owned target parsing, argument propagation, real tarballs and
subset collection without Linux, plus a simulated registry mismatch. No .NET
source, dependency, compiler or native build input changes in this extension. No push, remote publication
or global install is selected.
Pack --skip-tests and dist --skip-tests bypass qualification/installation tests,
preserve source/artifact/license checks, and mark outputs untested. Native
publication and complete release collection reject those outputs. Test
compilation remains part of build:native. CI keeps all qualification gates and
uses separate build/test/pack steps with one shared RID and retained stage logs.

Verification scope: owned CLI routing/bootstrap, argument routing, stage failure
and stop behavior, untested packaging/integrity/publication rejection, existing
package layout/installation checks, and real Linux npm installation with a prior
native binary as an explicit packaging fixture. No fresh .NET or Mac qualification
is claimed for TypeScript orchestration changes. Earlier gate evidence remains
bound to its original candidate below.

Final extension result: 47 delivery regressions, seven package layout cases, the installed-command
smoke check, TypeScript/lint/formatting and actionlint passed. The refactored npm
packer and installed-native helper passed against the prior 0.0.0 Linux binary
as an explicit fixture. Unified setup succeeded with no node_modules: 95 cached
npm packages installed and .NET restored offline. The unified version command
and its .NET lifecycle hook passed in a disposable fixture. The product version,
global installation and registry state are unchanged. No blocking review findings.
The x64-only wrapper built and previewed through real offline npx with exactly
three dependencies and the license. A Linux-only wrapper/native installation
passed using the prior 0.0.0 native binary as a packaging fixture. Source review
confirms npm’s multi-field view returns the version/dependency object checked
by retry preflight. No registry was contacted. The test boundary remains owned
selection, packaging, installation and orchestration, not upstream npm behavior.

Reproduce with Node 24.19.0/npm 11.17.0 and the configured .NET SDK:

```sh
npx forge setup --offline
npm run check:delivery
npm run test:delivery
npm run test:package-layout
npm run test:package-manager
npx forge dist --skip-tests --no-restore --plan
```

For a real local untested package, run forge build:native and then forge
pack --skip-tests, or forge dist --skip-tests. Native tests still compile in
the build. New-schema host package manifests require tested: true and recorded targets;
historical intermediate/package sets lacking that field must be regenerated.
Logs and the optional review receipt live under artifacts/delivery-stage-review
in the worktree. This tracked capsule owns the durable outcome and limits.

## Retryable Publication And Local Integration

The maintainer authorized publication retries, simpler version/.NET orchestration,
a final review and local squash integration into develop. This supersedes the
older no-merge constraint below. No push, actual npm publication, hosted Actions
run or authentication migration is authorized. Keep action version tags and
NPM_TOKEN. Standard direct implementation and self-review; no helpers.

Phase 3/3 complete: implementation, review and local verification passed. This
closeout accompanies the authorized local squash into develop. The
shared native, wrapper and release publisher preflights every exact npm version,
skips existing versions with a warning, and rejects non-E404 lookup failures.
Dry runs stay offline. Generated npm manifests replace seven tracked templates;
standard npm version owns the bump and a focused hook updates one .NET property.
Managed builds own normal restore; native publication reuses it. Owned tests
cover response parsing, skip/order/failure behavior and generated version state.

Develop advanced independently through b94f8359 to c7494f04. The trial was rebased onto c7494f04 while
preserving its C# and documentation changes. Candidate b1d2d149566a73eba4f0947c1fcb4e43f3cfd5b7,
tree 4974c8f2387e6c4bd6eb255b15448147871f82cb, owns code verification. Earlier
native failures belong to the original trial base; develop includes their fixes.
A redundant build against the old base was stopped after its managed build passed.

Review found no blocking delivery issue. Deliberate limits: npm existence checks
establish availability, not remote byte identity; skips do not retag; concurrent
publication can still race, and uploads remain non-atomic. Dry runs do not query
the registry. Keep NPM_TOKEN and external action version tags as requested.

The rebased normal build passed with zero warnings/errors. npm run verify passed:
33 delivery tests, seven platform layout cases, TypeScript, lint, formatting and
.NET whitespace/analyzer diagnostics. Wrapper packaging and its clean-source,
offline publication preview passed with LICENSE validation. A disposable version
command smoke check evaluated both .NET version properties as 0.1.0-beta.1.
The full Linux dist gate passed: Unit 3,232; managed and native Integration
1,745 each; public, native-public and managed-public-on-native 123 each. All
7,091 executions passed with zero failures or skips. Build output has zero
warnings/errors. The real npm native installation/invocation journey passed,
portable and npm packages were created with LICENSE, and both local publication
previews passed. No extra rebuild or retest is inferred from the final prose-only
closeout commit or squash: b1d2d149 owns the executed source evidence. Rebuilding
is required before publishing artifacts under a different source commit.

Reproduce from the selected source with Node 24.19.0/npm 11.17.0, .NET 10.0.111
and the Linux native toolchain:

```sh
npm run setup
npm run build
npm run verify
npm run dist -- --no-restore
npm run publish:native -- --tag preview --dry-run
npm run dist:wrapper
npm run publish:wrapper -- --tag preview --dry-run
```

Optional raw logs and integrated-receipt.json are under
artifacts/local-delivery-trial in the trial worktree; this tracked capsule owns
the durable acceptance summary. Native execution on other hosts and registry
publication are not claimed.

## Shared Local And Actions Trial

Maintainer preference: reference external GitHub Actions by version tags rather
than commit SHAs. The two trial workflows now use their existing documented
versions directly; local reusable-workflow references stay relative.

The maintainer accepted the two-workflow design in the existing
`codex/local-delivery-trial` worktree, with no merge or push. Standard direct
implementation, no helpers or independent review; one grouped correction pass.
Complete: phase 3/3, milestone 3/3. Shared commands, local behavior and workflow
wiring are verified, and documentation and the evidence receipt are aligned.
No hosted run, npm publication, global installation or C# change is authorized.

Accepted shape: build.yml runs setup/verify once and one six-platform matrix
runs setup/dist. Each host builds, tests and packages in one job. Release.yml
selects an exact successful build or invokes build.yml, downloads finished
packages and calls shared collection/publication scripts. The four platform
packaging workflows and intermediate transfers are removed. Package validation
and upload behavior are shared with individual local publishers. Complete
release validation precedes publication and the wrapper remains last.

Evidence is limited to owned behavior: TypeScript/static workflow validation,
existing delivery/layout checks, complete package selection/order and negative
source/version/tarball cases, and an offline publication preview. The shared
verify command also runs the existing .NET diagnostic gate. No new native build
is needed for YAML orchestration and archive-reader extraction; the earlier
native trial and its three source-owned integration failures remain historical.

Result: two workflow files replace six; one six-host matrix replaces the
separate build/test/package jobs. Full npm run verify passed (25 delivery tests,
seven layout cases, TypeScript/lint/formatting and .NET diagnostics). After the
final collector input/output separation guard, focused delivery tests and
static checks passed again. The collector now emits hashed publication inputs;
release selection validates all seven packages, source/version, current bytes
and licenses before invoking the same publisher as local single-package commands.
The complete dry-run fixture and actual wrapper packaging/preview passed.
The Linux-only inert launcher fixture remains independently selectable; dist's
real installed native-package journey supplies the default host-level smoke check.

Actionlint 1.7.12 passed with a temporary runner-label allowlist for the existing
windows-11-vs2026-arm runner, documented by GitHub after that linter's catalog
was released. The checksum-verified linter and its config are local verification
tools, not repository dependencies or workflow changes. No hosted execution,
registry upload, native rebuild, C# changes, commit, merge or push occurred.
The main worktree remains unchanged. The canonical receipt is
`artifacts/local-delivery-trial/actions-receipt.json`; earlier native
qualification failures remain unresolved and are not reclassified as passing.

## Local Delivery Trial

Testing follow-up: the maintainer reaffirmed that tests cover only behavior
Open Forge owns and the minimal external integration needed to establish that
installation and invocation work. The trial's option, publication-selection,
license and cleanup checks meet that boundary. The existing npm smoke test now
invokes the installed command directly instead of asserting npm's symlink
implementation. This test-only correction passed the focused package smoke
test (1/1, no failures or skips), TypeScript, lint, formatting and diff checks.
No native rebuild was needed. The receipt is
`artifacts/local-delivery-trial/testing-ownership-receipt.json`. The
earlier full-trial receipt remains historical evidence for its recorded source.

The maintainer accepted a worktree trial of host-detected distribution,
independent native and wrapper npm publication, and bounded generated-output
cleanup. Baseline: `03fa6b9a5`. Branch: `codex/local-delivery-trial`.
State: Trial concluded, phase 3/3, milestone 4/4; native qualification failed.
M1 is the accepted design and isolated
worktree; M2 implements commands and regression evidence; M3 qualifies the real
Linux distribution and offline publication previews; M4 documents the trial.
No merge, registry publication, hosted run or global installation is authorized.

Standard profile, primary implementation owner, independent review budget zero,
council budget zero, correction budget one grouped pass. This is developer
tooling; realistic risks are deleting unrelated local files, packaging stale
outputs, and publishing the wrong package. New remote commands are tested with
offline plans only. Existing C# source, Framework payload and Extension source
are protected. No C# semantic or dependency change is accepted.

The accepted command surface is setup, restore, build, test, dist, dist:wrapper,
publish:native, publish:wrapper and clean, alongside existing focused CI and
developer commands. Build/test restore once by default; explicit --no-restore
and offline restore remain supported. Native distribution detects its host,
builds once, consumes exact tested artifacts and packages their existing bytes.
Wrapper preparation requires only Node/npm and source/version/license data.
Wrapper dependencies remain synchronized exact versions of all six platforms.
Individual publication consumes a verified tarball and an explicit tag. Dry runs
never invoke npm publication or read credentials. The complete CI release still
owns six-target completeness and publishes the wrapper last.

Known output paths are replaced before regeneration. Cleanup preserves source,
dependency caches, offline feeds, local npm link staging and unrelated artifacts.
Every deletion validates its owned path and rejects symlinked ancestors before
mutating. Compiler intermediates remain incremental between ordinary builds.
Source identity includes LICENSE. Failure invalidates current success markers.

Evidence: real-filesystem cleanup and publication-selection regressions;
existing delivery and package-layout suites; typecheck, lint and formatting;
wrapper-only packaging; the full matching-host managed/native distribution
journey and repeated pack/test cleanup checks. No macOS/Windows execution or
registry behavior is claimed. Tests use isolated fixtures and cached dependencies.

Result: command implementation, 25 delivery regressions, all seven package
layout cases and static checks pass. Offline setup installed 95 locked npm
packages and restored .NET from existing caches. Managed and all Linux native
binaries built without warnings or errors. Wrapper-only packaging and its
offline publication preview passed; repeating packaging removed a stale output
probe and produced an identical tarball, including LICENSE.

The real distribution stopped at integration qualification: Unit passed
3231/3231; managed and native Integration each passed 1721/1724. All three public
CLI modes passed 111/111. The same three existing source inconsistencies fail in
both Integration modes: two embedded Extension comparisons retain the old
24-asset catalogue while the authored toolkit now has only two files, and the
coalesced-update fixture replaces a phrase absent from the current Framework.
C# code, Framework and Extension source are unchanged from the trial baseline.
The trial does not change those protected inputs or weaken qualification.
Pack and the native publication preview correctly reject the failed manifest.
Repeated native pack/test success evidence remains unavailable until those
source inconsistencies are corrected. The isolated npm installation journey
provides separate package evidence without promoting qualification.

The evidence receipt is `artifacts/local-delivery-trial/receipt.json`, alongside
the full build log, suite reports, package journey and publication previews.
Changes remain uncommitted in `/tmp/open-forge-local-delivery`; the main
worktree is unchanged. No upload, hosted execution, merge or global installation
occurred. Sandbox subprocess restrictions required local execution escalation;
offline NuGet needed the existing global cache explicitly selected with the
temporary CLI home. These environment corrections needed no source changes.

## Accepted Script Structure Implementation

State: Complete. Phase 3/3, milestone 5/5. Completion grace: 2.
The maintainer accepted the preceding scripts recommendation and required a
feature branch followed by a local squash into develop. Baseline: 5a565594.
Branch: codex/delivery-script-structure. No external-core adoption, package-manager
migration, hosted action, remote publication or C# behavior change is included.

Standard structural profile: freeze existing observable commands, version rules,
artifact identity, package graph and all test assertions before Blue changes.
Phase 1 owns M1 source/test inventory and behavior freeze. Phase 2 owns M2
structure/configuration implementation and M3 focused typecheck, lint, formatting,
tooling tests, launcher and real package evidence. Phase 3 owns M4 one fresh
review and M5 documentation, exact staging and local squash integration.

Accepted architecture: one root strict Node no-emit configuration checks scripts
and tests; one focused emitting configuration ships only the npm launcher.
Keep independently runnable test tiers through stable commands and paths. Group
release and npm tooling beneath delivery; keep agent-tooling separate. Meaningful
tasks have direct entry points using capability-specific shared modules at their
nearest common scope. Shared modules never import task entry points. Preserve
root-relative execution from any npm invocation directory. No generic registry,
custom process framework or one-line wrapper for ordinary tools. Use semver's
public API for validation if its existing contract is preserved.

One Astra/high implementation owner owns scripts, root package/configuration and
workflow consumers. Root owns the Task, ledger, narrow local directive clarification
and development documentation. Protected: all C# source/tests, Framework and
Extension payloads, unrelated prose, existing artifacts and other-chat changes.
Dependency installation already authorized for workspace tooling remains limited
to directly needed dependencies. Do not update the global CLI during this change.

Evidence: baseline tests and source are frozen at 5a565594. Reuse existing 32
tooling assertions and installed-native package journey; add only evidence for
an actual uncovered changed entry-point boundary. Compare launcher output and
public package contents. Existing native bytes are frozen inputs; no new C# or
foreign-host runtime qualification is claimed. Source tracking must cover new
paths and removed-path deltas. Review budget: one independent structure/package
review S13-R1; correction budget: one grouped cycle S13-C1; council: zero.

Done: Implementation committed at 0ad9f11d; 32 tooling tests and native package
journey passed. Fresh review S13-R1 found no material source issues.
Now: Phase 3/3, milestone 5/5; accepted local squash completed.
Next: No remaining work in this bounded script refactoring.
Blocker: None.

### Implementation Review And Execution Correction

S13-R1 reviewed immutable baseline 5a565594 through candidate 0ad9f11d, tree
34975852dcedb58d0e4073b613dd6f9c15e99087, including preflight 509bb8b5.
No material source or documentation findings. The review covered argument/error
order, root resolution, shared dependencies, source tracking, compiler output,
package/release composition, version synchronization and preserved assertions.
The 68 recorded source hashes matched the candidate. The review was read-only.

The original supplemental comparison harness used Python in ignored artifacts.
Root rejected that execution format under the maintainer's no-Python instruction
and requested a TypeScript rerun plus removal of the two exact generated Python
files. The production candidate and 32 Node test results were unaffected. The
TypeScript harness exited zero and superseded the initial comparison receipt.
Both exact Python harnesses were removed, including the temporary external copy.
This consumes S13-C1 for an execution correction, not a source behavior fix.
The delegation packet should carry explicit language constraints even when its
source scope already says TypeScript; do not infer a relative model-quality
conclusion from this single packet omission.

### Accepted Local Integration

Feature 868253517b38ecbbe4b014034a4ddab1ae73fea7 was squash-integrated
onto develop baseline 5a565594f3eeb6c6c450057266d12b160fbf3e7a in the
commit containing this completion record. The staged squash first matched the
feature tree exactly. Only this Task, the ledger and checkpoint then received
completion updates. All qualified script/configuration bytes are unchanged.
No remote operations, global-link changes or publication occurred.

### Accepted Result And Evidence

The implementation candidate is 0ad9f11d5962b3b6a1e07fea2143972bc61676e3,
tree 34975852dcedb58d0e4073b613dd6f9c15e99087. Source and configuration
hashes remain unchanged after review. The root acceptance receipt is
`artifacts/task13-structure/acceptance.json`; it links the detailed command,
toolchain, source-hash, test, differential and package receipts.

- Removed ten redundant compiler configurations. One strict root Node
  configuration checks tooling and tests; one narrow configuration emits the
  launcher. Existing package commands still select each test boundary.
- Replaced the delivery dispatcher with direct restore/build/test/native-build,
  built-test, pack and version-bump entry points. Shared process, version,
  package identity, source and test execution capabilities are directly imported.
- Moved npm and release tooling beneath delivery and updated actual consumers.
  Agent tooling remains separate. All seven package templates are byte-identical
  to the baseline. No runtime dependency or external core was added.
- Version validation calls the existing semver API; only its development type
  declarations were added. Strict checking, linting, formatting and all 32
  existing tooling tests pass with zero failures or skips. Existing assertions
  remain unchanged apart from the relocated package-template path.
- The Node/TypeScript comparison proves 42 matching entry-point outcomes and
  22 matching version-validation outcomes. Its process sentinel captures our
  invocation arguments, cwd, output and exit handling without building C#.
- The emitted launcher is byte-identical. The second emitted module only loses
  the unused SHA-pattern export, now defined beside shared source identity.
  Exactly two JavaScript files are emitted; public package paths and whitelist
  are unchanged. The real installed Linux native-package journey passes and
  preserves the native binary hash.

Protected C#, Framework and Extension sources are unchanged. Foreign-host
runtime execution, new C# qualification, hosted workflows and publication are
not claimed. No production repair followed the no-finding review. External
core extraction and wider previously deferred refactoring remain deferred.

## Script Tooling And Architecture Follow-up

State: Complete. Phase 2/2, milestone 3/3. Completion grace: 2.
The maintainer requested workspace TypeScript 7 and direct tool commands, an
explanation/locality assessment of scripts and compiler projects, and review of
their dot-lith-cli task-orchestrator branch as a possible reusable .NET core.
Baseline: local develop `f84fa53f`; preserve the separately integrated Framework
changes and all C# source/tests. Dependency installation and cloning the named
repository are explicitly authorized; push, publication and hosted actions remain
prohibited. No adoption of the external core or package-manager migration is
accepted merely by investigating it.

Direct profile. M1 inspect current consumers, tool compatibility and the external
core; M2 install the requested compiler and simplify direct package commands with
focused checks; M3 record findings, recommendations and local changes. Phase 1
owns M1; phase 2 owns M2/M3. Existing assertions and package behavior remain
frozen. Expected implementation paths: package.json/lock and directly affected
compiler invocation consumers; structural reorganization is a proposal until its
tradeoffs are explained. No C# or Framework payload changes.

One bounded Astra/high external-core assessment runs read-only alongside root's
local tooling work. Review budget: one focused tooling delta review only if the
compiler upgrade changes emission behavior. Correction budget: one grouped cycle.
No council or full CLI qualification is needed for command spelling alone;
changed launcher emission requires its real package boundary evidence.

### Tooling Outcome And Evidence

Code commit: `5cfdf9d88b436509d7421bc4cc9026c658651f6f` on local develop.
Workspace commands now call tsc and eslint directly. TypeScript 7.0.2 supplies
`tsc`; Microsoft's documented TypeScript 6 API alias remains available to
ESLint through the `typescript` dependency. This is the supported
[side-by-side installation](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0/),
not a custom compatibility layer. Installed ESLint is 10.9.1.
Launcher staging calls the shared `build:launcher` package command instead of
resolving a compiler's private JavaScript entry point. npm remains the package
manager. No structural migration or external-core adoption was implemented.

Qualification: root typecheck/lint, focused delivery typecheck/lint/format and
launcher compilation passed. All 32 tooling tests passed: 16 delivery/release,
8 agent tooling, 7 package layout and 1 package fixture. Both emitted launcher
files are byte-identical to the previously qualified TypeScript 6 output.
The actual installed-native package journey passed using the new launcher and
the unchanged, previously qualified Linux native binary. C# tests and foreign
hosts were not rerun; this evidence qualifies tooling, not new CLI behavior.

A fresh isolated lockfile installation passed using authorized dependency
downloads, with the installed compiler and linter executables checked directly.
The first offline attempt failed because an existing isexe tarball was absent
from the cache; it is not recorded as a successful offline installation.
Logs and exact hashes are in `artifacts/task13-ts7/qualification.json` and its
referenced logs. Existing tests were unchanged. Byte-identical emission did not
trigger the capsule's conditional additional code review.

### Scripts Assessment And Proposed Follow-up

Inventory: 41 TypeScript files, approximately 2801 lines including tests,
11 nested compiler configurations plus the root configuration. The directories
have useful responsibilities, but configuration and shared ownership can be
simplified. The following changes are proposals, not completed implementation.

| Current directory            | Responsibility                                                                                | Recommended treatment                                                                      |
| ---------------------------- | --------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------ |
| scripts/delivery             | Restore, build, native output, built tests, versions and artifact identity                    | Keep one meaningful entry point per task and topic-local shared capabilities.              |
| scripts/ci                   | Release selection, complete platform collection, checksums and archive inspection             | Group as release tooling within delivery; retain checks at actual release boundaries.      |
| scripts/package-managers/npm | Seven manifest templates, thin launcher, staging, packing, local linking and package journeys | Keep npm-specific code together; share delivery facts without importing task entry points. |
| scripts/agent-tooling        | Agent projections and Git-object review tooling                                               | Keep separate from product delivery.                                                       |

Use one root strict Node no-emit configuration for tooling and tests, plus one
small emitting configuration for the shipped npm launcher. Most current nested
projects only vary file selection within the same Node environment. Unit,
integration and public package journeys can still run separately using their
existing test entry points. When implementing consolidation, clarify the scoped
Node compiler-project rule explicitly; do not weaken C# test-tier projects.
The emitting configuration earns its place by excluding build/release helpers
and tests from published JavaScript.

Shared version, platform, source identity, artifact and process mechanisms belong
at their nearest common delivery scope. Currently delivery imports a SHA rule
from the npm model, while npm imports delivery version handling; release helpers
also consume both. These are opposing folder dependencies, not a demonstrated
runtime module cycle. Move shared facts toward their real owner, preserving the
thin launcher's dependency boundary and package evidence. Avoid a generic global
utils bucket, a command registry or a custom workflow engine.

Prefer direct commands for tsc, eslint and formatting. Meaningful operations such
as restore/build/test/pack can each have a small task entry point using focused
shared modules. Do not replace direct commands with one-line TypeScript wrappers.
The existing semver package can be called through its API instead of spawning its
CLI merely to validate a version. Keep existing boundary assertions while moving
code; remove tests only when their owned behavior is explicitly replaced.

Keep npm for now. There is one private development package, and npm scripts
already expose workspace binaries. The six platform package directories are
output templates, not six independently developed workspace packages. pnpm would
change dependency management without fixing the present responsibility split.

### External Core Assessment

The explicitly authorized clone selected branch `feature/task-orchestrator`,
commit `92ad9fd9a105816fd8a8336a120d719cd42e9e97`. Read-only review covered
TypeScript source and tests; no external code was installed or executed.

The core offers build/publish/run/pack/clean/rebuild argument handling, Node
process execution and workflow scheduling. Its publish options cover native AOT,
self-contained output, trimming and single-file output. No dedicated restore or
prebuilt-test operation was found. NuGet pack is distinct from our npm package
staging. Our artifact identity, test-result handling and release collection would
still be needed.

Recommendation: keep Open Forge delivery local now. A future small extraction
could provide pure .NET argument builders and an executable-plus-arguments Node
process helper. Keep project discovery, persisted configuration, UI metadata and
workflow scheduling above that reusable boundary.

Material static findings supporting this recommendation:

- The [core manifest](https://github.com/TheLithiumForge/dot-lith-cli/blob/92ad9fd9a105816fd8a8336a120d719cd42e9e97/packages/core/package.json)
  points to a missing index.ts and lacks a consumable exports/build surface.
  Existing consumers reach into source through aliases or relative imports.
  Bun owns repository build/test commands; the inspected runtime uses Node APIs,
  so Bun is not established as a runtime requirement.
- Build and publish wrappers persist .lith-cli.json after successful execution,
  including when defaults were not requested. Publish also changes cwd to the
  project's directory while retaining a relative project path, which can make
  a nested path resolve twice. These are wrapper policy and path concerns,
  separate from useful argument-building functions.
- The [process helper](https://github.com/TheLithiumForge/dot-lith-cli/blob/92ad9fd9a105816fd8a8336a120d719cd42e9e97/packages/core/src/utils/system/exec-helper.ts)
  uses shell command strings and a two-minute default timeout that the inspected
  build/publish command schemas do not expose. Native AOT can exceed that limit.
- The [workflow scheduler](https://github.com/TheLithiumForge/dot-lith-cli/blob/92ad9fd9a105816fd8a8336a120d719cd42e9e97/packages/core/src/commands/extensions/workflows/run-workflow/run-workflow.ts)
  treats a running dependency as an available result and can finalize its
  dependent as skipped when another parallel node completes first. Skipped
  results count toward overall success. This is a static control-flow finding,
  not an executed reproduction; the inspected tests do not cover that case.

### Follow-up Observation And Closeout

One bounded Astra/high reader inspected the external core while root handled
workspace compatibility and validation. Root checked the consequential source
findings before accepting them. This supplied an independent assessment without
an implementation team or review council. There is no controlled comparison
with prior Luna/Sol performance, so no relative model-quality claim is supported.

Done: M1 assessment, M2 compiler/direct commands and M3 durable findings complete.
Now: Phase 2/2, milestone 3/3; local code committed and documentation recorded.
Next: Structural consolidation and any external extraction remain proposals.
Blocker: None. No hosted run, publication or external-core adoption occurred.

## Delivery Simplification Continuation

- State: Complete and locally integrated at `1b8475ae` from baseline
  `ae433e200123f59ebc35d4a88c98167edee8e474`, tree
  `c21012fd024d22c39996d5b96d16dd89bdea6e91`.
- Current phase: 3/3. Completed milestones: 6/6. Completion grace: 2.
- Owner: Astra Overseer completed; Astra/high author and fresh reviewer completed.
- Authority: The maintainer approved the complete shared-script and pipeline
  proposal on 2026-09-12. This continuation includes the directly required
  version/package changes formerly mapped to Tasks 7/22; their earlier
  completion receipts remain historical, not new execution evidence.

### Outcome And Execution Capsule

Deliver one simple package-script interface for local development and all six
native CI targets. Local build, test and pack commands create portable artifacts;
only the release pipeline publishes externally. Use the accepted delivery
simplification in the project ledger. Standard profile, with explicit frozen
CLI behavior and focused Red evidence for new version/artifact behavior. Keep
production changes and test-policy removals reviewable as coherent commits.

The project is a local developer tool. This change affects regenerable build
outputs and version-controlled tooling. Ordinary command failure, stale or
mismatched artifacts, incomplete platform sets and package-version drift are the
supported risks. Standard npm, Node, .NET and Actions mechanisms suffice;
exceptional machinery: none. No new runtime dependency or CLI behavior is needed.

Root package.json owns the version. Use native npm version handling without
commits/tags, then synchronize the exact resulting value to .NET and every
implemented shim. Optional SHA builds derive a prerelease version without
editing tracked versions. Builds stamp the version before compilation. Tests,
packaging and release consume that identity. Reject missing or mismatched
artifacts; preserve native bytes between stages. Build/test/pack operate for the
current native host by default. Full CI always covers the accepted six targets.

Use short reusable platform packaging workflows and one release coordinator.
Manual invocation accepts a ref/commit and destination github/npm/all; automatic
version-tag invocation uses the configured destination, default all. A tag must
match the committed version. Reuse a successful build for the exact selected
commit when supplied; otherwise call the same build workflow once. Never reuse
an unrelated or failed build, or rebuild inside test/package/publication jobs.
GitHub creation and registry publication stay pipeline-only. Publication must
wait for complete package preparation; npm platform packages precede the main
package. No claim of a cross-registry transaction is made.

Expected paths: package.json and its dependency lock, Directory.Build.props,
focused root scripts delivery/CI and package-manager TypeScript tooling/tests,
.github/workflows, and directly affected development/distribution prose.
Protected: CLI C# source and behavioral tests, src/open-forge,
src/extensions, unrelated Markdown/Task 28 work, other worktrees, and all
existing evidence. Report a required protected-path change before proceeding.
No Python or authored JS/MJS/CJS, no dependency downloads, remote Git/GitHub
operations, hosted runs or publication during this task. The accepted cleanup
extension below separately authorizes the current native global link.

Review budget: one fresh Astra/high whole-change review D13-R1 and one
user-scope-extension delta review D13-R2.
Correction budget: one grouped cycle D13-C1, rechecked at affected boundaries.
Council budget: zero. Consumed IDs: D13-R1, D13-C1 and D13-R2. D13-R2 reviewed cleanup candidate
`b4983d53`, tree `420a32a46`, including the earlier release corrections. D13-R1 was assigned to the fresh whole-change
review of candidate `a446897d`, tree `63980a39`, against baseline `ae433e20`.

### Accepted Repository Cleanup Extension

On 2026-09-12 the maintainer required coordination scripts under root
`scripts/`, C# implementation and required .NET resources only under
`src/cli/`, removal of unused scripts/package entries, a current managed-marker
check, and explicit global linking of the new CLI. This supersedes the prior
local-link prohibition only for the current native CLI and its known npm links.
Remote operations, downloads, hosted runs and publication remain prohibited.

Move delivery, CI, npm staging/launcher tooling and maintained agent tooling
into cohesive children of `scripts/`; update their real consumers and scoped
compiler projects. Preserve C# behavior/tests, Framework/Extension source and
all unrelated Markdown. Remove obsolete MVP build entrypoints and unused
package dependencies after inspecting their consumers. The old CLI can be
retired once the refreshed native link is verified; retain its history in Git.

M4 includes qualification through the final script paths and M6 includes the
verified global link. The fixed phase/milestone horizon remains 3 phases and
6 milestones. Review budget increases to two because the user added a new
placement/linking boundary after the first immutable review: D13-R2 is a focused
delta review of that boundary, not a repetition of D13-R1. D13-C1 groups the two
accepted release-selection fixes. Root owns live placement and documentation;
the implementation owner prepares C1 in isolated draft files while current
build artifacts remain frozen.

### Evidence And Fixed Horizon

1. Phase 1: M1 freeze the accepted behavior, script/artifact interfaces and
   focused failure evidence against the inspected baseline.
2. Phase 2: M2 implement shared commands/version/packaging; M3 implement the
   six-target workflows and pipeline-only release coordinator.
3. Phase 3: M4 pass tooling checks and the complete managed/Linux native and
   actual packed-install journeys through the new commands; M5 pass fresh
   review and grouped corrections; M6 finish documentation, exact local
   integration and the changes/findings/evidence record.

Use separate focused Node test projects for pure version/artifact checks and
real filesystem/package integration. Do not retain upstream parser/OS tests or
hand-maintained C# case inventories. Runner failures, missing/empty evidence,
required suite execution and relevant warnings/skips remain failures. Retain
normal test reports and compact SHA/version/RID/artifact hashes. The build/package
boundary triggers the complete managed plus supported Linux AOT gate once the
implementation is coherent. Other native hosts receive static review only,
as accepted by the maintainer; hosted execution and shipping remain unclaimed.

The seven-package graph and all current C# behavior/tests are frozen at the
baseline. Existing Task 27 receipts supply only the starting runtime baseline.
Current scripts must produce fresh ending evidence. Preserve the prior CI helper
behavior that protects owned outcomes, and explicitly account for obsolete
host probes and exact-count/hash test bookkeeping replaced by standard results.

### Observations

D13-O1: The local execution sandbox flattened Node process-isolated test
reports to file-level results and omitted child output. The identical two-test
probe outside the sandbox reported two named assertions. Keep the standard
Node runner; use approved execution permission for local child-process evidence
rather than adding a repository-wide isolation workaround. The five Red
assertions were independently visible against callable stubs before Green.

### Progress

Done: Final `b4983d53` qualification passed all 6997 C# executions, 32 tooling
tests, installed-native npm and portable extraction journeys; review passed.
The simplified global-link command exited zero and exposes the exact qualified
version/native hash.
Now: Complete at phase 3/3, milestone 6/6; exact local integration accepted.
Next: Task 28 continues independently. Broader refactoring stays deferred.
Blocker: None. Foreign-host execution and public release remain unclaimed.

### Accepted Local Integration

Feature `0101f25ca8bd0c6615d77dc3457d93dfc02b0c44` was squash-integrated
into current local develop at `1b8475aef267384df7bc07aef6fb5bf8bee29700`.
Both trees are exactly `5461e0021f7b198e253166a1eda9b15afa67e5cf`.
The integration has 122 changed paths; it preserves all C# core/root/tests and
Framework/Extension payload source against the initial baseline. The worktree
was clean after integration. The final completion record changes only Markdown.

The receipt is `artifacts/task13-simplification/integration.json`. The saved
metadata freeze accounts for all five final coordination/prose paths; the source
changes and replacement tests were frozen in earlier coherent commits. No remote
effect or publication occurred.

### Final Qualification And Remaining Work

The final code/tooling candidate is `b4983d53f154261371e38cdd0c0f915933013c0d`,
tree `420a32a467a1936237ee4346244a4eb66bd895d4`. Its SHA build version is
`0.0.0-dev.sha-b4983d53f154261371e38cdd0c0f915933013c0d`. Subsequent closeout
changes touch only prose/coordination, not executable or package inputs.

The canonical saved receipt is
`artifacts/task13-simplification/final-qualification/qualification.json`;
that directory preserves all six standard test reports. Root npm commands
passed offline restore, C# formatting/diagnostics, native build, prebuilt tests,
packing and global linking. Build summaries reported zero warnings/errors.

| Selection                        | Passed |
| -------------------------------- | -----: |
| Unit                             |   3228 |
| Managed Integration              |   1718 |
| Managed public E2E               |    111 |
| Native Integration               |   1718 |
| Native public E2E                |    111 |
| Managed public E2E on native CLI |    111 |
| Delivery/CI helpers              |     16 |
| Agent tooling                    |      8 |
| npm package layouts              |      7 |
| Installed npm fixture            |      1 |

The C# rows are repeated execution of shared suites under supported build modes,
not 6997 distinct behaviors. All rows have zero failures/skips. The real packed
native npm journey is separate from the installed fixture row. Native publication,
packed npm, extracted portable execution and the global command share SHA256
`bca2542b7dadc6ee32a68d7fd4a9681b7cbfff07c39c2d281d96ff6c1575ce5a`.
The final package directory is `artifacts/delivery/linux-x64/packages-nExnNk`.

Strict root/focused TS projects, root/delivery lint, delivery formatting, C#
formatting and warning diagnostics passed. Final YAML parsed through Prettier;
all 26 Bash blocks passed syntax and ShellCheck. `D13-R1`, grouped `D13-C1`
and focused `D13-R2` are closed. C# core/root/tests and Framework/Extension
payload source match the starting `ae433e20` baseline exactly. All removed
legacy files have Git identities; every moved and formerly untracked authored
file is included in the reviewed commits. No Python or authored JS/MJS/CJS was
introduced. `src/cli` has C# source/tests, required .NET resources/projects and
its preserved test README; TypeScript tooling is under root `scripts/`.

Remaining outside this local acceptance horizon: execute the five other native
hosts when authorized; enable publication credentials and perform a separately
authorized release; let Task 28's source owner correct the one README locator
recorded below. Broader C# refactoring remains deferred under Task 27's beta
closeout. No remote operation, hosted run, download or publication occurred.

### Current Cleanup And Qualification Checkpoint

The first full candidate `a446897d` passed all six selections: 3228 Unit,
1718 managed Integration, 111 managed public, 1718 native Integration, 111 native
public and 111 managed public on native (6997 executions, zero failure/skip).
Native publication had zero warnings/errors. The actual installed npm journey
and extracted portable executable passed with identical native SHA256
`065995dc64f53fe1281d4f9c32c725f5baaf926e654a755e5d40f7c48d497d5e`.
The full initial receipt is
`artifacts/task13-simplification/initial-qualification/qualification.json`.

D13-R1 found two material release-selection defects. Both are fixed in isolated
commit `737444e0`: an existing version tag must resolve to the selected commit,
and numeric prereleases use npm channel `next`. The correction has focused
matching/mismatching/annotated/missing-tag and ordinary request-failure evidence.
Its isolated patch and receipts remain under
`artifacts/task13-simplification/correction-draft/`.

Commit `c4428a90` migrated two maintained agent-tooling test files to Node while
preserving all 49 assertion sites and eight selections. The mapping and receipts
remain under `artifacts/task13-simplification/agent-test-draft/`.
The following placement moves and package cleanup preserve the C# implementation
and assertions. All relocated helper evidence passed: delivery16, agent tooling8,
package layout7 and installed fixture1. Strict root/delivery projects passed.
Final exact-path native qualification and D13-R2 subsequently passed as recorded above.

The first global-link invocation completed global package links but failed at
its unnecessary final repository dependency-link step because offline npm tried
to resolve an uncached development dependency. The global command nevertheless
matched the qualified native version and exact hash above. The root/staged
unlink coupling was removed from the helper, and the two previously known
repository-local symlinks were verified, recorded and removed. Final execution
of the simplified global link subsequently passed as recorded above. No dependency was downloaded.

The legacy CLI/build retirement covers exactly ten unchanged tracked TypeScript
files, recorded with blob identities in
`artifacts/task13-simplification/retired-scripts.txt`. Their last active snapshot
is `c4428a90`; every Markdown document is preserved. Current source and map
references now point to native C# and root scripts; historical MVP references
use the retained Git snapshot. Required .NET project/resource files stay in
`src/cli/`; there is no TypeScript implementation there.

The marker inspection personally read the complete current C# directive trio:
`_csharp.md` SHA256 `31045ebcb02d5bfeee8ba9f3112d307b1f72a2186cda618fbf7d22e7d1d90b53`,
`design.md` `76aa8fc7aaaa79d9535998f5557150f3754e3d80520a7a06864b61659373c1a9`,
`style.md` `c3fa9d31575e77fedb103ca397f0ccf10ab7236e6edbef1658c7fe36138457cb`.
`FrameworkContentIdentity` still recognizes the `open-forge:start` and
`open-forge:end` managed-host comments; Install/Update use that recognition and
existing Integration assertions prove the resulting blocks and preserved bytes.
No C# source or contract changed.

D13-O2: The real global-link journey exposed dependency resolution caused by a
repository-local npm link in a private tooling root. The existing offline package
fixture did not exercise that extra step. Keep globally installed package linking
independent of the repository development dependency graph.

D13-O3: Astra/high produced the coherent tooling implementation and a fresh review
that found one additional normal-use prerelease defect beyond the author's tag
finding. Two later bounded correction/test-migration packets returned saved
receipts and assertion mapping. Initial Green receipts existed only in tool
output, so final acceptance requires explicit saved logs. This is task-specific
observational evidence, not a controlled Astra/Sol/Luna quality comparison.

D13-R2 passed the focused delta at `b4983d53`, tree `420a32a46`. It confirmed
relocated consumers/compiler projects, preserved agent assertions, scoped dependency
removal and the simplified three-step global link. D13-R1-F1/F2 are closed.
Root also passed all 26 final workflow Bash blocks through syntax and ShellCheck,
plus full root lint. No hosted workflow was executed.

One nonblocking source-chat handoff remains: in
`src/extensions/orchestration/README.md`, replace the literal
`src/agent-tooling/review/inspect-git-objects.ts` with
`scripts/agent-tooling/review/inspect-git-objects.ts`. The actual OpenCode bridge
is already correct. Leave that protected Extension prose to its existing owner.

The two inspected obsolete generated files `dist/cli.mjs` and
`.temp/cli/embedded-assets.generated.ts` were removed after recording their
hashes in `artifacts/task13-simplification/retired-build-outputs.json`. Nearby
Markdown, transfer packets and other existing evidence remain preserved.

### Evidence Replacement Map

- Host-receipt probes moved to normal setup actions and current-host/RID
  validation. SDK-directory and upstream host assumptions are no longer tests.
- Manifest Integration evidence preserves source/version/RID, native byte and
  required closure checks; per-file mode and absolute-host inventories retired.
- Standard CTRF qualification plus runner flags preserve nonempty execution,
  failures, skips, warnings and suite errors. Exact global case counts, deferred
  case hashes and duplicate subject inventories were removed deliberately.
- Build/package producer fixtures moved to built-manifest qualification, the
  unchanged installed-native journey, and actual offline all-six archive/seven
  package collection. SHA/version/byte mismatches and missing targets still fail.
- Existing npm layout and installed-fixture assertions remain. The native journey
  adds a precompiled launcher input while preserving its assertions.
- New version evidence covers derivation, synchronized manifests, the real npm
  version hook and lockfile, and absence of a created commit or tag. Release
  selection covers the exact successful build and beta/stable channel mapping.

## Historical Completed Horizon

## Task State

- State: Complete and squash-integrated under the accepted local-only horizon.
  Tasks 21, 27 and Task 7 ARM64 expansion are complete through `7eeeb19d`.
  The accepted feature branch is `codex/cli-delivery`; integration is on `develop`. The maintainer approved Linux,
  macOS and Windows CI/artifacts on x64 and ARM64 on 2026-09-09.
- Permanent mapping: Task 13 “Native CI and Reproducible Artifacts” in
  the [project control ledger](../../project-control.md).
- Parent: [CLI Delivery](_delivery.md).
- Historical preparation profile: The opt-in
  [Supervised Luna Preparation Trial](../../../../../workflows/supervised-luna-preparation-trial.md).
- Exact preparation base: local `develop` commit
  `c8051478127ab9204ca31b1d86ef358dd982207f`, tree
  `ccc762115238c4317b3b35963be18edd38be2609`.
- Lane: `codex/native-ci-preparation` in
  `open-forge-worktree/native-ci-preparation`.
- Current phase and milestone: phase 3 of 3, milestone 6 of 6.
- Current-state suffix: M6 complete. Accepted feature `6d370632` is integrated
  into local `develop` at `3bf03e0e`, exact tree `33c98766`.
  No remote operation, foreign-host execution or publication is claimed.
- Preparation role: One experimental Luna/max Task Mastermind supervised at
  most three bounded read-only evidence workers, followed by one fresh Sol/xhigh
  whole-task review. That allocation is historical. Later semantic implementation
  and fresh review use Astra/high under current Overseer direction.

The project control ledger defines permanent identity, queue state, worktree
mapping, and integration state. This Task record defines the accepted horizon,
preparation result, evidence, trial measurements, and later revalidation.

## Accepted Local Completion Boundary

On 2026-09-10 the user made local Git authoritative, prohibited all remote Git
and GitHub operations, and accepted manually executed Linux commands plus static
inspection of the other platform jobs for local completion. This supersedes the
earlier hosted-execution prerequisite. The six-target implementation remains;
actual execution on the other five hosts is unproven and is not claimed.

The existing three-phase, six-milestone horizon retains M1 through M4. M5 now
means accepted local execution and static platform inspection; M6 means exact
local acceptance and squash integration. No hosted run, upload, publication or
global installation refresh is part of this horizon. Future release proof is
separate from local task completion.

The final workflow inspection passed all three six-target matrices and all
thirty Bash blocks through syntax and ShellCheck checks. Existing exact Linux
runtime and package receipts remain valid because their inputs are unchanged.
The fifty CI helper cases include collector fixtures; they do not represent six
real host bundles. Local Bun remains 1.3.0, not hosted pin 1.3.14, and no fresh
dependency installation or byte-for-byte repeated-build proof is claimed.
The static receipt is
`artifacts/task13-six-target-preparation/root/local-only-closeout/workflow-static.json`.
Temporary Python helpers were removed at the user's request; raw evidence,
manifests, Markdown and source tooling were preserved. No Python is tracked.

## Expected Outcome

CI proves native build and smoke, packed installation and invocation, checksums,
and bounded artifact collection for each accepted target. It records exact
source, SDK, dependency and artifact identities without publishing to a registry.
Byte-for-byte reproducibility requires an independent repeated-build comparison;
one checksum manifest proves identity and integrity only.

## Matrix And Current Authority

| Operating system | Accepted native targets    |
| ---------------- | -------------------------- |
| Linux, glibc     | `linux-x64`, `linux-arm64` |
| macOS            | `osx-x64`, `osx-arm64`     |
| Windows          | `win-x64`, `win-arm64`     |

Use a matching native host for each target. Revalidate current official runner,
SDK/toolchain, action and Native AOT support before freezing concrete jobs.
An emulated execution, cross-build, source property or existing workflow row is
not native proof. Missing matching-host evidence remains visibly incomplete.

Retain the accepted split into a build matrix, a test matrix, and a separate
manual/on-demand artifact publication workflow. The latter stages and packs
accepted artifacts, proves isolated installed-launcher invocation, writes
checksums and collects bounded artifacts; it does not publish npm packages or
create a product release. Routine workflow file names and runner choices within
this accepted graph are implementation decisions for the Overseer.

D1 owns native build/smoke, package installation/invocation, checksums and
bounded artifact receipts for all six targets. Task 7 owns the package graph,
staging/packing implementation and package-owned journeys. Full managed Unit,
Integration and public suites, plus Native AOT Integration/public evidence,
remain Architecture/A1 evidence consumed by D1 and Task 22. The test matrix may
schedule these required gates without collapsing their evidence ownership.

At activation, refreeze the exact candidate and package graph after Task 7,
map every target to its native host and commands, and preserve phase 1/3,
milestone 2/6 as completed historical preparation. Revalidation does not itself
complete M3. Additional RIDs, libc variants, support-floor matrices, signatures,
SBOM, provenance and OIDC remain outside this accepted expansion.

## Six-Target Execution Capsule

- Base: `7eeeb19d5dd3b30445f7b67a6c9ade1e02e5ee85`, tree
  `c222861a9a6026e686b9bf435a376092a1d7533d`, on `codex/cli-delivery` in the
  primary worktree. Task 7's exact reviewed executable inputs passed 6,421
  managed/Linux native cases and its actual Linux installed-package journey.
- Profile/applicability: bounded delivery tooling for a private developer CLI.
  It handles reproducible job inputs and disposable build/package artifacts;
  failures stop acceptance and can be rerun. Standard GitHub workflow reuse,
  Node/TypeScript, .NET, npm and tar supply the needed capabilities. Checksums
  prove identity and integrity, not hostile-runner attestation or independent
  byte reproducibility. Exceptional machinery is none.
- Accepted preparation: root accepts the two-workflow, same-run architecture in
  `artifacts/task13-six-target-preparation/architecture/packet.md`, with the
  refinements in `root-refinements.json` and formatting policy below. Refreeze
  exact workflow/helper contracts and evidence before Green. Historical Linux
  recipes are not current implementation instructions.
- Owner: root retains architecture, authority, commits, integration and acceptance.
  The continuous `six_platform_ci_design` Astra/high owner assumes the coherent
  implementation role after its read-only architecture packet. One fresh
  Astra/high reviewer inspects the final coherent boundary.
- Placement: modify `.github/workflows/cli.yml`; add
  `.github/workflows/cli-artifacts.yml`. New focused CI tooling lives under
  `src/cli/ci/`: `verify-test-results.ts`, `artifact-manifest.ts`, optional
  `host-receipt.ts`, `collect-packages.ts` when collection needs cohesive code,
  explicit `test-inventory.json`, and strict `tsconfig.json`. Root accepted
  `test-result-documents.ts` for pinned JSON-to-fact validation and
  `package-contents.ts` for ordinary tar member/content operations; these preserve
  the 200-line TypeScript boundary without creating a generic parser. Focused tests and
  their strict project live in `ci/__tests__/`, mirroring those responsibilities.
  A demonstrated split remains within its capability, requires a reported
  placement refinement and never creates a generic CI or process framework.
- Workflow relationship: ordinary PR/manual verification plus `workflow_call`
  exposes separate build and test matrices. The manual-only artifact workflow
  calls verification in the same run, then its package matrix consumes the exact
  tested native files, followed by one bounded complete-graph collector. No
  cross-run selection, API/token service, `workflow_run`, registry publication,
  release or automatic artifact-publication trigger is introduced.
- Matrix: Linux glibc x64/ARM64 on `ubuntu-24.04`/`ubuntu-24.04-arm`, macOS
  x64/ARM64 on `macos-15-intel`/`macos-15`, Windows x64/ARM64 on
  `windows-2025`/`windows-11-vs2026-arm`. Assert actual matching native host and
  selected tool architecture. The accepted packet records official action SHA
  pins; use exact SDK 10.0.111, Node 24.19.0 and Bun 1.3.14 with frozen Bun
  dependencies, repository NuGet configuration and captured resolved graphs.
- Tool setup: isolate the SDK beneath runner temporary storage so `global.json`
  cannot select an unrelated preinstalled SDK. Record exact source SHA/tree,
  tool versions, architecture, dependency identities and bounded artifact hashes.
  Missing native capabilities fail rather than installing an unaccepted fallback.
- Build/transfer: stamp every build/publish with the same full-SHA version;
  preserve complete managed Unit, Integration and public closures before native
  publications, and a separate managed-public-on-native closure afterward. Keep
  the canonical development/native publications at their build-owned paths.
  Transfer only these bounded roots and receipts through ordinary tar so Unix
  executable modes survive. Test jobs never rebuild the accepted artifacts.
- Evidence: preserve raw discovery, CTRF, command exits, logs and runtime hashes.
  Use the exact discovered count for the runner minimum and separately enforce
  exact executed counts. Current baseline is 2,860/2,882 Unit, 1,603/1,603
  Integration and 111/111 public; qualify the nine known deferred Unit theories
  by full case identity and multiplicity. Every command retains exactly three
  public journeys, with twenty-six Shell and one embedded-artifact case separate.
- Package proof: Linux x64 additionally runs the seven layout cases and original
  one-case fixture. Every native package row runs Task 7's existing argv-driven
  installed-launcher journey. Bind native/package/source hashes to successful
  build/test receipts, preserve the exact tested tarballs, and require all six
  RIDs and the synchronized seven-package graph. If main tarball hashes differ,
  compare their actual member contents before choosing the tested Linux main
  as canonical. No repacking or rebuild after acceptance.
- Gray/Red: freeze accepted inputs, callable shapes, file ownership and independent
  expected data first. New custom receipt/checksum code receives focused tests
  for positive acceptance and false-green rejection: missing/partial/inconsistent
  or skipped results, unexplained case changes, changed/missing artifacts, and
  incomplete/mismatched package graphs. A temporary compilable contract stub may
  establish qualified Red; absent imports or compiler/setup failures are not Red.
  Freeze tests before Green. Keep contract/evidence and implementation commits
  isolated; do not test upstream runner, npm or tar internals.
- Final ladder: focused strict TypeScript, scoped lint/format, workflow
  trigger/matrix/action/permission inspection, protected-path and exact input
  checks, one fresh holistic review, and local Linux build/transfer/test/package
  qualification. This build/package boundary triggers the complete managed and
  Linux Native AOT gate. Root may converge separately owned Task 22 documentation
  preparation before that final source freeze so one exact final candidate pays
  the combined gate once. Task-owned changes and acceptance remain distinct.
- Protected: all C# production/tests/projects, existing npm implementation and
  tests, root package/dependencies/configuration, Framework/Extension source,
  frozen MVP, global installation, unrelated dirty worktrees and Task 28. Any
  separately accepted Task 22 delta is root-owned and explicitly identified;
  the CI author never edits it or treats it as their implementation scope.
- Budgets: fresh whole-task review maximum one `T13-R1`, grouped correction and
  affected recheck maximum one `T13-C1`, council zero. `T13-R1` is consumed for
  the immutable 21-path candidate `20776f5d`, accepted without material findings;
  `T13-C1` is consumed for the observed reporter-selection correction and
  one affected recheck, accepted without further findings; no second
  whole-source review is selected.
- Phases/milestones: phase 1 completed M1/M2 preparation.
  Phase 2/3 owns M3 implementation and M4 reviewed local qualification. Phase
  3/3 owns M5 local execution plus static six-target acceptance and M6 exact
  local squash integration under the current completion boundary above.
  Remote execution, downloads, uploads and publication are outside this horizon.
- Local checkpoints: Gray `910c837c`, independent Red `fae17b29`, tier-config
  correction `a262c7dc`, and initial five-helper Green `0f5d698c` are committed.
  Root independently reproduced all 39 initial cases passing. Supplemental Gray
  `2bb4ddea` and Red `31bad86d` add host qualification, selected-root snapshots,
  and matching-host package exports. Root qualified four Unit and seven
  Integration named-stub failures after compilation, with no skips. All fifty
  assertions and the inventory are frozen before supplemental Green.
- Placement refinements: `host-receipt.ts` owns concrete host/tool/source facts;
  `package-export.ts` forms portable relative package exports on the producing
  host. `artifact-manifest.ts` owns enumeration of explicitly selected complete
  closures. No generic runner, parser or dispatcher is added. The two existing
  tier projects select all CI Unit and Integration evidence. Root corrected the
  initial mixed compiler project and the supplemental pure-host Unit cases
  before accepting those boundaries.
- Local tools: Node v24.19.0 and SDK 10.0.111 match the accepted pins; installed
  Bun is 1.3.0, while hosted CI requires 1.3.14. Existing dependencies permit
  local Node/.NET gates, but local evidence must not claim exact pinned Bun or
  a fresh frozen-lock installation. No download or global mutation is authorized.
- Hosted rollout: GitHub requires a manual workflow to exist on the default
  branch before `workflow_dispatch` can receive events. The new artifact
  workflow therefore requires an explicitly authorized default-branch rollout
  before its first manual run. See [GitHub's manual workflow documentation](https://docs.github.com/en/actions/how-tos/manage-workflow-runs/manually-run-a-workflow).
  Keep the accepted manual-only trigger; no remote effect is implied here.
- Green checkpoint: `20776f5d` completes the six remaining source deltas. Root
  verified all 21 path hashes, modes and saved bodies, eight focused execution
  receipts, and exact 21 Unit plus 29 Integration passing totals. The eleven
  frozen Red paths remain unchanged. Thirty shell blocks pass syntax and
  ShellCheck; strict production and both test projects, lint and formatting pass.
- Historical checkpoint before local-only acceptance: M4 complete, phase 3/3.
  T13-R1 accepted the complete CI boundary and proposed local transfer/runtime script. Its canonical reports are
  `artifacts/task13-six-target-preparation/review/T13-R1.md` and `.json`. Root
  verified the receipts and accepted the qualified local runtime chain, including
  the bounded T13-C1 correction. No agent or runtime lease remains active.
  The next dependency is explicit authorization to inspect the remote state and
  prepare the exact default-branch workflow registration and hosted run.

### Local Runtime And Reporter Correction

The frozen `c2eb60b3` candidate passed all 6,421 managed/Linux native executions
from its separate exact-source consumer fixture. These are 4,596 cases exercised
across the six accepted runtime selections, not 6,421 distinct cases. All raw
process exits, cases, source/artifact hashes and required formatting qualify.
Root verified 3,196 exact source files and preserved all 24 unrelated dirty paths.

The first package receipt check rejected Node's default spec output after the
seven layout and one fixture cases passed. A first continuation using direct
Node selected TAP but omitted the accepted harness's npm execution context;
six layout cases failed that prerequisite. Both attempts are preserved. The
successful continuation retained root npm scripts and selected TAP through
command-scoped `NODE_OPTIONS`. It passed the eight package cases, the actual
Linux installed-native journey, package export and bounded archive verification.
All 57 protected successful evidence files remained byte-identical.

Isolated correction `696c56b7` changes only the two workflow invocation lines.
No C#, tests, fixtures, projects, embedded resources, package implementation or
CI helper changed; the correction therefore requires only affected reporting
proof and recheck, not another full native build or repeated 6,421 executions.
The final hosted workflow still needs actual matching-host execution.

Canonical local evidence is `artifacts/task13-ci/local-c2eb60b3/canonical-summary.json`.
`execution-chain.json` binds the initial failure, rejected continuation, successful
continuation and source correction without relabelling the tested commit.
The same reviewer accepted T13-C1 with no further findings in
`artifacts/task13-six-target-preparation/review/T13-C1.md` and `.json`.
M4 is accepted. All six actual hosted runs and complete graph collection remain
M5; neither this source nor the Linux archive earns hosted completion.

### Formatting Baseline And Gate

Root verified the whole current solution: whitespace passes, and ordinary full
`dotnet format --verify-no-changes --severity warn --no-restore` passes with no
diagnostics. These are the required CI gates and preserve the existing workflow's
warning-severity policy. The supplementary whole-solution style/info pass reports
703 existing suggestions across 294 untouched files, mainly eager collection
expressions. They are recorded under
`artifacts/task13-six-target-preparation/format-baseline/`; no suppression or
production edit is introduced. A clean required pass does not imply a clean
informational baseline. Task-scoped C# authoring still follows the current
Directives and its applicable focused checks. This CI Task does not reopen the
accepted structural refactor to apply unrelated automated suggestions.

## Historical Preparation Boundary

The following preparation, inventory, feasibility, recipe and review sections
retain the earlier Linux-only D1 and x64-package decisions and receipts. Their
ARM-undecided wording and future execution instructions are historical, not
current authority. The six-target matrix above supersedes those scope limits.
No old command recipe is ready to run without the activation refreeze.

## Preparation Trial Boundary

The preparation lane was authorized to inspect the exact committed CI workflow,
repository-root build and package commands, accepted Task 7 package graph,
artifact paths, checksum requirements, and local evidence already present. It
could update only this Task record and directly required generated navigation.

The lane had to record:

- the current workflow matrix and its difference from accepted D1;
- the exact future native build, direct smoke, packed install and invocation,
  checksum, and bounded artifact steps;
- inputs or paths that remain unstable until command acceptance;
- protected package, product, support-floor, and publication meaning;
- the exact facts and commands that must be revalidated before CI mutation.

Protected surfaces include `.github/workflows/`, CLI production and tests,
package-manager source and manifests, root project and dependency files, public
documentation, release configuration, and generated runtime projections. This
preparation does not edit CI, execute remote jobs, publish, install globally,
or claim D1 acceptance.

## Preparation Result

The exact semantic preparation base is commit
`c8051478127ab9204ca31b1d86ef358dd982207f`, tree
`ccc762115238c4317b3b35963be18edd38be2609`. The activation candidate is commit
`3c904a23ed6735ca91bb3ca2810156b01a25e5b9`, tree
`f2a780b8cb1da6ac680b7d7d7586d9732cb20cf6`, with the semantic base as its
parent. The activation comparison changed only project-control and workflow
records, the Task 13 record, and workflow navigation records. Concretely, the
changed paths are `.agents/memory/working/cli-development/project-control.md`,
this Task record, `.agents/workflows/_workflows.md`, and the added
`.agents/workflows/supervised-luna-preparation-trial.md`. `.github/workflows/cli.yml`,
the root build controls, and the npm package sources remain byte-identical
across those two snapshots. This lane has not changed any protected
implementation or delivery surface.

The accepted preparation candidate is commit
`e76896965e1cce51b7295f899499c5a7d0cce252`, tree
`c204c17b4bdda87bf43a479879d6ebcf87472283`, with activation commit
`3c904a23ed6735ca91bb3ca2810156b01a25e5b9`, tree
`f2a780b8cb1da6ac680b7d7d7586d9732cb20cf6`, as its direct parent. Its direct
parent comparison changes only this Task record. The commit containing the
project-ledger convergence integrates that accepted semantic delta onto local
`develop` parent `de40d550c00e51f55fe7e8b5d39297c450f721e9`, tree
`e6040c4996eb8af5375979b54a9bafc198438a34`.

### Current CI Inventory

The current [CLI workflow](../../../../../../.github/workflows/cli.yml) runs for
relevant pull requests and manual dispatch. One `native` job expands to six
RIDs: `win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`, `osx-x64`, and
`osx-arm64`. It selects `ubuntu-latest` for `linux-x64`, installs the floating
`10.0.x` SDK line, restores the solution with `NuGet.Config`, verifies format,
builds Release, runs managed Unit and Integration evidence, publishes the root
CLI, runs managed EndToEnd evidence, publishes and executes native Integration
and EndToEnd projects, and uploads the native root executable for each RID.

That workflow currently differs from accepted D1 in four material ways:

- D1 owns one native `linux-x64` delivery lane. The other five matrix entries
  are inventory only and are outside the current support and D1 evidence
  boundary. Their presence does not establish ARM support or package release
  readiness.
- The workflow has no accepted package staging, `npm pack`, isolated offline
  install, or installed-launcher invocation step. Task 7's one package journey
  proves package reachability and executable placement, but deliberately does
  not invoke the CLI.
- The workflow uploads native executables but emits no checksum manifest for
  the native binary, packed packages, or a deterministic source archive.
- The workflow records neither the resolved SDK/toolchain and dependency graph
  nor one bounded artifact manifest tied to the candidate commit and
  informational version. Its `10.0.x` setup input must be reconciled with the
  repository's `global.json` (`10.0.100` with feature-band roll-forward).

The existing managed and native test publishes are separately recorded
Architecture/A1 evidence. They do not become D1 requirements merely because
the shared workflow schedules them, and no unsupported RID is a current
shipping claim.

The accepted future CI shape is minimal: a build matrix, a test matrix, and a
separate manual or on-demand publish workflow. The current combined `native`
job is only the recorded starting point. Its matrix contents, native runner
labels, package steps, and artifact evidence must be revalidated when M3 is
authorized; a current ARM entry cannot be carried forward as a support
decision.

### Accepted x64 Expansion And ARM Feasibility

The permanent Task 7 “npm Package Manager Release and Local Linking” record is
already reopened with a separate platform-expansion horizon. That current
record, rather than the earlier candidate recommendation preserved in the review
receipt below, defines phase 1 of 4 with 0 of 7 milestones complete; its
preparation is complete. It keeps the historical 8/8 closeout intact, consumes
and revalidates this Task's target evidence in M1 and M2, applies the coherent
x64 package expansion in M3 and M4, runs focused owned-package evidence in M5,
and closes through one review and integration record in M6 and M7. The package
evidence asserts only Open Forge package placement, launcher reachability,
argument/process forwarding, and completion; it does not test npm, Node, the
operating system, third-party libraries, or CLI command behavior.

The ARM candidates were assessed as feasibility only:

| Candidate     | Native AOT buildability                                                                                                                                                                            | Native runner and testability                                                                                                                                                          | npm release shape and incremental cost                                                                                                                                                               | Disposition                                                                                                        |
| ------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------ |
| `linux-arm64` | .NET Native AOT supports the RID. Cross-architecture x64-to-Arm64 publication needs the target linker, runtime objects, and compatible native dependencies; a cross-build is not sufficient proof. | GitHub lists `ubuntu-24.04-arm`, so a native Linux run is available in principle. The glibc floor, SDK toolchain, publish, and smoke still need a real runner receipt.                 | One thin package with `os: ["linux"]`, `cpu: ["arm64"]`, and `libc: "glibc"`; plus one model/staging row, native artifact, package tarball, checksum, install/reachability journey, and runner lane. | Technically feasible candidate; not simple or acceptance-ready, and not accepted.                                  |
| `osx-arm64`   | .NET Native AOT supports the RID. Cross-architecture publication can use the macOS/Xcode toolchain, but cross-OS publication remains unsupported.                                                  | GitHub lists `macos-latest`/current macOS Arm64 labels. Native execution is possible, but hosted Arm64 limitations and action compatibility require image/toolchain revalidation.      | One thin package with `os: ["darwin"]` and `cpu: ["arm64"]`, with the same additional artifact, checksum, and owned install/reachability evidence.                                                   | Technically feasible candidate; the native path is plausible but not yet simple or proven, and not accepted.       |
| `win-arm64`   | .NET Native AOT supports the RID. Publication requires the appropriate Visual Studio C++ Arm64 tools; cross-architecture publication must use a matching Windows target toolchain.                 | GitHub lists `windows-11-arm` and `windows-11-vs2026-arm`. Native execution is possible in principle, but the installed C++ toolchain and image identity require a spike-time receipt. | One thin package with `os: ["win32"]` and `cpu: ["arm64"]`, plus one model/staging row, native artifact, checksum, install/reachability journey, and runner lane.                                    | Technically feasible candidate; toolchain/image verification makes it non-baseline-simple, and it is not accepted. |

These are not three metadata-only additions. Together they would add three
platform package templates/manifests, three main-package optional dependency
edges, three runtime/staging mappings, three native publication and smoke
lanes, three package tarballs, and corresponding checksums and owned
installation evidence. The macOS x64 package is a separate accepted baseline
gap and must be solved even if every ARM candidate is declined. No candidate
may be released merely because its RID is present in `Directory.Build.props` or
the current workflow.

The feasibility assessment uses the current official sources for the later
spike: [Native AOT cross-compilation](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/cross-compile),
[Native AOT deployment and supported targets](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/),
the [.NET RID catalog](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog),
[GitHub-hosted runner labels and limitations](https://docs.github.com/en/actions/reference/runners/github-hosted-runners),
and [npm platform package fields](https://docs.npmjs.com/cli/v11/configuring-npm/package-json/).
They were read on 2026-09-03 and must be revalidated before an ARM scope
decision or any runner/package mutation. These sources establish feasibility
constraints, not repository acceptance or successful build evidence.

### Future Execution Capsule

The later implementation owner must revalidate every input below on the then
current clean candidate. These commands are a preparation recipe, not evidence
run by this task and not permission to mutate the protected workflow now.

| Boundary                         | Future command or change                                                                                                                                                                                                                                                                                                                                                            | Required result                                                                                                                                                                                                                                               |
| -------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Candidate and host identity      | `git status --short --branch`; `git rev-parse --verify HEAD`; `git rev-parse HEAD^{tree}`; `uname -m`; `ldd --version`; `dotnet --version`; `node --version`; `npm --version`                                                                                                                                                                                                       | Clean native Linux x64 runner, exact commit/tree, resolved toolchains, and glibc facts are recorded.                                                                                                                                                          |
| Audited restore and dependencies | `dotnet restore OpenForge.Cli.slnx --configfile NuGet.Config`; `dotnet package list --project OpenForge.Cli.slnx --include-transitive --format json --no-restore`                                                                                                                                                                                                                   | Restore uses the repository source and audit settings. The complete resolved graph is captured with no warning-bearing or partial result.                                                                                                                     |
| Native publication               | `gitSha="$(git rev-parse --verify HEAD)"`; `version="0.0.0-dev.sha-${gitSha}"`; `dotnet publish src/cli/root/OpenForge.Cli/OpenForge.Cli.csproj --configuration Release --runtime linux-x64 --self-contained true --no-restore --output artifacts/publish/linux-x64/open-forge -p:OpenForgeSkipDevelopmentPublish=true -p:OpenForgeCliInformationalVersion="${version}"`            | A fresh `artifacts/publish/linux-x64/open-forge/OpenForge.Cli` is produced from the exact clean candidate, with its version marker and informational version recorded.                                                                                        |
| Native smoke                     | `nativeArtifact="artifacts/publish/linux-x64/open-forge/OpenForge.Cli"`; `test -x "${nativeArtifact}"`; `"${nativeArtifact}" --version`                                                                                                                                                                                                                                             | The Open Forge executable starts natively and returns the expected candidate version. This is the owned smoke, not a test of the runtime or SDK.                                                                                                              |
| Accepted package staging         | `node src/cli/package-managers/npm/manage.ts stage artifacts linux-x64 artifacts/publish/linux-x64/open-forge/OpenForge.Cli local "${gitSha}"`                                                                                                                                                                                                                                      | Task 7's main package and Linux x64 optional package are staged under ignored artifacts with synchronized `0.0.0-dev.sha-<full lowercase Git SHA>` versions. The Windows package remains part of the accepted graph but is not staged for this Linux journey. |
| Fresh bounded output roots       | `runRoot="artifacts/task13/linux-x64/${gitSha}"`; `test ! -e "${runRoot}"`; `test ! -e artifacts/publish/linux-x64/open-forge`; `test ! -e artifacts/npm/stage/linux-x64`; `mkdir -p "${runRoot}/pack/main" "${runRoot}/pack/linux" "${runRoot}/delivery"`                                                                                                                          | The candidate refuses stale task-owned output roots instead of deleting them, then creates the exact pack and delivery directories required by later redirections. The publish and stage commands must create their own guarded output paths.                 |
| Packed install                   | `npm pack artifacts/npm/stage/linux-x64/open-forge --pack-destination "${runRoot}/pack/main" --offline --ignore-scripts --no-save --no-package-lock --no-audit --no-fund`; repeat for `open-forge-linux-x64` with `"${runRoot}/pack/linux"`                                                                                                                                         | Exactly the staged main and Linux platform packages are packed. Capture each path only after asserting that its destination contains exactly one `.tgz`.                                                                                                      |
| Tarball path capture             | `test "$(find "${runRoot}/pack/main" -maxdepth 1 -type f -name '*.tgz' \| wc -l)" -eq 1`; `test "$(find "${runRoot}/pack/linux" -maxdepth 1 -type f -name '*.tgz' \| wc -l)" -eq 1`; `mainTarball="$(find "${runRoot}/pack/main" -maxdepth 1 -type f -name '*.tgz' -print -quit)"`; `linuxTarball="$(find "${runRoot}/pack/linux" -maxdepth 1 -type f -name '*.tgz' -print -quit)"` | Fresh absolute-to-workspace tarball paths are captured and no stale or duplicate package is accepted.                                                                                                                                                         |
| Isolated invocation              | `installRoot="$(mktemp -d)"`; `npm install --offline --ignore-scripts --no-save --no-package-lock --no-audit --no-fund --prefix "${installRoot}" "${mainTarball}" "${linuxTarball}"`; `"${installRoot}/node_modules/.bin/open-forge" --version`                                                                                                                                     | Only the Open Forge package boundary is asserted: both packages install, the launcher resolves the installed Linux payload, and the installed command returns the expected version. Do not test npm internals or third-party behavior.                        |
| Source and checksum identity     | `git archive --format=tar --prefix="open-forge-${gitSha}/" "${gitSha}" > "${runRoot}/delivery/open-forge-${gitSha}.tar"`; `sha256sum "${nativeArtifact}" "${mainTarball}" "${linuxTarball}" "${runRoot}/delivery/open-forge-${gitSha}.tar" > "${runRoot}/delivery/SHA256SUMS"`; `sha256sum --check "${runRoot}/delivery/SHA256SUMS"`                                                | The manifest binds the exact native binary, packed artifacts, and source archive. The check proves traceable integrity and identity; it is not byte-for-byte reproducibility evidence.                                                                        |
| Bounded receipt                  | Record runner identity, SDK and dependency facts, commit/tree, informational and package versions, exact artifact paths and hashes, command statuses, selected/discovered/executed counts where a test runner is used, failures, skips, warnings, and limits                                                                                                                        | A reviewable receipt distinguishes D1-owned package/native proof from separately consumed A1 evidence. No publication or credentials are involved.                                                                                                            |

The `mainTarball`, `linuxTarball`, and `nativeArtifact` shell names in the
recipe are resolved from the fresh outputs of the immediately preceding steps.
The implementation must not reuse an older ignored artifact or use
`--no-build` against an artifact whose input identity is not recorded.

This Linux D1 journey intentionally does not stage the accepted `osx-x64` or
`win-x64` packages; those package journeys belong to the reopened Task 7
horizon. It also does not stage any ARM package.

### Change Matrix And Ownership

The future D1 mutation is limited to the accepted CI responsibilities and
their bounded evidence paths. The exact workflow file names are not frozen by
this preparation record; M3 must return a project change request if the
retained authority does not identify them. The three responsibilities are:

- build matrix: compile the accepted candidate and collect exact SDK, runtime,
  dependency, commit, and bounded build-artifact identity;
- test matrix: run only the accepted focused managed/native evidence selected
  by the final task authority, with no third-party or broad CLI-contract
  assertions; and
- manual/on-demand publish workflow: perform the Linux D1 native/package
  stage, pack, isolated install/reachability, archive, checksum, and bounded
  artifact upload steps only when explicitly dispatched.

These responsibilities must not be collapsed back into the current combined
`native` job, must not make publication automatic, and must not use the
existing ARM rows as implicit release support. Any matrix or workflow-path
change needs a fresh support-boundary decision.

The accepted `src/cli/package-managers/npm/` source, package manifests, root
`package.json`, `global.json`, `Directory.Build.props`, `Directory.Packages.props`,
and `NuGet.Config` are inputs to revalidate, not preparation mutation targets.
No production source, test project, package contract, public documentation,
release configuration, generated runtime projection, or project-control file
is needed for M3. A mismatch in those sources returns to the Overseer as a
project change request rather than receiving an improvised local substitute.

### Accepted Authority Split

The accepted x64 direction is aligned across the current Distribution, Delivery,
Task 7, Task 13, and Task 22 records. Task 7 owns the accepted x64 package graph,
platform-package expansion, and applicable-host package journeys. Task 13
retains only the later Linux D1 native build and smoke, checksums, and bounded
artifact collection after retained commands, Task 10, and accepted Task 21
remediation. Task 22 owns final acceptance and separately authorized atomic
publication of the complete x64 graph. ARM remains undecided and outside all
three task scopes.

### Stale Risks And Revalidation Triggers

- The preparation base predates Route Move integration and the later command,
  architecture-remediation, and release prerequisites. Every source fact and
  command path must be reread after those inputs settle; this record does not
  authorize starting M3 from the preparation commit.
- Task 7's package contract is accepted, but later package-source, launcher,
  manifest, or version changes invalidate staging and packed-install evidence.
  The installed journey must use a fresh native artifact and the current full
  candidate SHA.
- `ubuntu-latest` and `10.0.x` are labels rather than complete reproducibility
  identities. Revalidate native `x86_64`, glibc, resolved SDK, Node/npm, action
  versions, and package graph on the actual runner. Do not present an emulated
  or cross-compiled run as native execution proof.
- Ignored `artifacts/` output is disposable and can outlive its inputs. Start
  the D1 output directories fresh, record the producing commit/tree, and reject
  stale or warning-bearing binaries, tarballs, manifests, and receipts.
- The current package EndToEnd test uses an inert owned payload and checks
  installation reachability only. D1 must add one real native packed-launcher
  invocation while asserting only Open Forge's resulting behavior; it must not
  grow into npm, Node, or CLI command-contract coverage.
- Restore may contact NuGet and CI may upload bounded artifacts, but this
  preparation made no such external effects. Publication, registry contact,
  credentials, signatures, SBOM, provenance, OIDC, and support-floor expansion
  remain outside D1.

Before M3, revalidate the prerequisite ledger state, exact clean ancestry,
workflow and package-source identities, runner facts, restore/audit result, fresh
native version smoke, package graph and synchronized versions, offline packed
invocation, and checksum verification. Any changed input invalidates the
dependent downstream evidence from that earliest boundary.

## Whole-Task Review And Disposition

A fresh Sol/xhigh whole-task review inspected this record, the complete routed
Open Forge context, the activation parent/tree, local links, protected paths,
and the official feasibility sources. It found no blocking issue. One grouped
correction accepted all eight findings:

| Review ID | Finding                                                                                          | Disposition                                                                                                                                                                    |
| --------- | ------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| R1        | The Task record could not own queue state while saying `Queued` against an `ACTIVE` ledger row.  | Accepted and fixed: the record remains active for review; the Overseer must perform any ledger handback.                                                                       |
| R2        | M2 could not be complete while whole-task review and correction were pending.                    | Accepted and fixed: review is now dispositioned and the record is at phase 1/3, milestone 2/6.                                                                                 |
| R3        | The accepted x64 expansion needed an explicit authority and ownership handoff beyond Task 13.    | Accepted and recorded as the deferred project-change request naming Architecture, release, Delivery, Task 7, Task 13, and the ledger; the current authority split resolves it. |
| R4        | The future change matrix contradicted the accepted build/test/manual-publish workflow split.     | Accepted and fixed: the three responsibilities are now separate and exact future file paths remain an authorization boundary.                                                  |
| R5        | The recommended Task 7 reopening named phases but not six milestones.                            | Accepted and fixed: the recommendation now names M1 through M6 and remains non-authoritative until Task 7 is reopened.                                                         |
| R6        | The future recipe did not guard fresh pack/delivery directories or capture unique tarball paths. | Accepted and fixed: guarded output-root initialization and one-tarball path capture are now explicit.                                                                          |
| R7        | One checksum pass proved identity, not byte-for-byte reproducibility.                            | Accepted and fixed: the current outcome is traceable/checksummed collection; stronger reproducibility requires an independent repeat comparison.                               |
| R8        | Activation provenance omitted the Task record from its changed-record description.               | Accepted and fixed: Task, project-control, and workflow navigation records are named.                                                                                          |

No finding was rejected, duplicated, or treated as preference. The reviewer
verified that only the owned Task record is modified, protected-path diff is
empty, all local links resolve, and `git diff --check` passes.

Integration completed R1's ledger handback. It also converged R5's earlier
non-authoritative recommendation with the already accepted current Task 7
phase 1/4 and 0/7 horizon. The other review dispositions remain unchanged.

## Execution Horizon

1. Phase 1, preparation.
   - M1 inventories the current CI, package, artifact, checksum, and accepted
     authority boundary on the exact base.
   - M2 freezes a future execution capsule, stale-data risks, revalidation
     triggers, and the experimental workflow evidence. Task 13 then returns to
     the queue until its implementation prerequisite is satisfied.
2. Phase 2, implementation after Task 21, Task 27, Task 7 ARM64 expansion,
   and refreeze of the six-target source, package, runner and evidence boundary.
   - M3 applies the native CI and artifact collection change for all six targets.
   - M4 passes focused local/static validation and the available CI-equivalent
     journey without remote publication.
3. Phase 3, acceptance.
   - M5 passes matching native-runner, packed install/invocation, checksum, and
     bounded artifact evidence for every accepted target on the final candidate.
   - M6 records the final evidence, residual limits, integration identity, and
     trial comparison.

The phase and milestone counts do not regress. Preparation evidence is not
implementation or acceptance evidence, and every prepared fact is revalidated
before M3.

## Reproducibility And Artifacts

For each accepted target, record native host, SDK, RID, commit, informational
version, dependency graph, source
archive identity, binary/package hashes, package inventory, and smoke/packed
results. This preparation defines traceable and checksummed artifact collection;
it does not call one production run byte-for-byte reproducible. A later release
gate must independently repeat production from the same clean candidate and
compare the resulting identities before using that stronger claim. Upload only
bounded release-candidate artifacts. Run publishes sequentially when output is
shared.

## Stop Conditions

Stop on emulated evidence presented as native, warning-bearing publish, skipped
required behavior, runner-specific uncommitted patch, root-path C# assumption,
automatic publication, a reproducibility claim without an independent repeat
comparison, or any broader RID/support-floor claim without explicit acceptance.

## Historical Trial Measurements

Record elapsed preparation time, child count, handoffs, missing-context or model
fallbacks, accepted and rejected Sol/xhigh review findings, grouped correction
count, facts invalidated before M3, later revalidation cost, gate failures,
integration conflict, and any defect found after acceptance. The initial
read-only candidate comparison required a fallback evidence child because its
requested Luna runtime was unavailable; that child result is useful input but
does not count as successful Luna-only execution evidence.

This preparation now has three bounded read-only evidence-child packets: the
current CI/workflow inventory, the package/artifact/checksum authority
inventory, and the ARM feasibility inventory. The requested Luna/max route was
attempted once and rejected by the runtime, so the packets are fallback-model
evidence and none counts as Luna-only evidence. The ARM packet independently
confirmed that all three candidates are possible only subject to native
toolchain/runner receipts, while the accepted release boundary remains the
three x64 targets above. The whole-task review accepted R1–R8, one grouped
correction was applied, and no finding was rejected or deferred. The accepted
authority split is now recorded in the current delivery records.
