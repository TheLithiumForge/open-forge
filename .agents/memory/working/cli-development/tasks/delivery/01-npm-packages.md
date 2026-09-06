---
open-forge:
  description: Prepare and prove the thin npm package graph and explicit local-link workflow
  tags: [Memory, Working, CLI, Task, Delivery, Npm, Package, Contextual]
---

# Task 7: npm Package Manager Release and Local Linking

## Task State

- State: Historical 8/8 and platform-expansion 7/7 horizons remain complete and
  immutable. The provisional local-use horizon is active at phase 2 of 3,
  milestone 3 of 5. Live preflight, exact Loader/current-truth/future-idea
  prose, focused evidence, and both independent reviews are complete; candidate
  refreeze and commit are active.
- Parent: [CLI Delivery](_delivery.md).
- Profile: Assured local-use flow with one Task Mastermind, one coherent prose
  owner, one independent Writing Reviewer, one focused local-link safety
  reviewer, and at most one grouped correction.
- Historical review budget: maximum 1; `T7-R1` was consumed with
  `CHANGES_REQUIRED`.
- Historical correction budget: maximum 1; `T7-C1` was consumed by the grouped
  M7 repair.
- Follow-up review budget: maximum 1; `T7-XR1` was consumed with `PASS` and no
  material findings.
- Follow-up correction budget: maximum 1, unused.
- Provisional review budget: maximum 2; `T7-LR1` passed writing and `T7-LR2`
  passed package/local-link safety. Both are consumed.
- Provisional correction budget: maximum 1; `T7-LC1` is unused.
- Council budget: 0 across the current horizon.
- Current owner: Kepler Task Mastermind is refreezing and committing the
  candidate on branch `codex/provisional-local-use`. Ada Writer completed the
  bounded Loader, maintenance-contract, current-truth, and future-idea prose.
  Task 13 retains Linux D1 ownership.
- Provisional activation base:
  `0bc82357a4fc7bd54ecbca1d58501d721c04baa6`, tree
  `41b74d9cad9c5d16ed8a02bb60ba06a4b8df8903`.
- Restart baseline: `195ff13ecff6a598dd18ef22a0335c1dc75e6736`.
- Historical progress: 8 of 8 milestones complete in phase 5 of 5.
- Follow-up progress: 7 of 7 milestones complete in phase 4 of 4.
- Follow-up M3-M5 implementation and evidence: `496df91ba59bc999fb0fea0b4bfb3a6aec5f69e3`,
  tree `24e323ca3d37bda9ebea99568d62ba20d0caed0a`.
- Follow-up M6 fresh-review candidate: `aa7450281bc17b361b0a79facfa82bb346cf63ba`,
  tree `8f61dc066aa277dde76e12eed4915df8e4139a41`.
- Follow-up M7 accepted closeout is the commit containing this record.
- M3 package staging is committed on the restarted task branch as
  `00e09b24059e26e8dbec8cff129bc404876c9f0c`.
- M4 local linking and root migration are committed on the restarted task branch
  as `46cf3467661bcf06cbecafe64ad5f32000bf5de0`.
- M5 package evidence is committed on the restarted task branch as
  `edf493617c393269f6e9504eb9bc5d508c920783`.
- M6 canonical verification is recorded as
  `de6690f6367ed7864d22de9aadd9e678106dfa4b`.
- M7 review and grouped repair are committed as
  `d5bfddd156c4708780a0fe4a12cc2c6c1c30ea43`.
- M8 accepted closeout is `6ba0e060ed9aabc1954d70d0f1d2277bf5d8dff6`,
  tree `bccefb120f4e2d05a632cc80fc03b04ab2870e2d`; squash integration is the commit
  containing this record.

## Provisional Local-Use Horizon

The maintainer authorizes a provisional machine-local installation so the
replacement CLI can support documentation migration, brainstorming, and new
project authoring before the remaining command backlog is complete. This is
private developer tooling, not release or publication. It reuses the accepted
package-owned local-link workflow and does not externalize or package
`local/extensions` or other Extension content.

### Execution Capsule

- Outcome: align the source Loader, dogfood Loader, and Loader maintenance
  contract with the replacement CLI's actual integrated public commands; record
  the unaccepted centralized `.agents` interoperability idea; prove package and
  local-link safety; prepare and, only after exact release, execute one
  reversible local link from the verified integrated tree.
- Expected paths: `src/open-forge/.agents/loader.md`, `.agents/loader.md`, the
  Loader maintenance contract, Loading Reliability decision, this Task record,
  project control, checkpoint, plan, and the existing Extension-overhaul idea
  record.
- Protected paths: package source and root package files, C# source and tests,
  generated Loader Entries, `local/extensions`, Extension content, lockfiles,
  dependencies, release or CI surfaces, and every other Task's record and
  implementation.
- Accepted CLI examples are drawn from current executable and source evidence.
  They must not advertise the unintegrated root Update command, obsolete
  `load --bodies` or `chain` commands, or installation lifecycle as Loader
  guidance.
- Focused evidence: exact changed and untracked inventory, source/dogfood
  authored parity with unchanged generated Entries, obsolete-example and
  relative-path scans, Markdown/link and format checks, independent writing
  review, and independent link-safety review.
- Stop before publication, registry or remote contact, dependency installation,
  package-source or root-package mutation, authored JavaScript/MJS/CJS, C#
  behavior, compatibility machinery, or any local link before the Overseer
  verifies the exact integrated target and releases the pre-authorized effect.
- Phase 1 owns M1–M2 preflight and prose alignment. Phase 2 owns M3–M4 focused
  evidence, review, refreeze, and candidate commit. Phase 3 owns M5 integration,
  exact live-link release, isolated smoke, receipt, and closeout.

### Milestones

1. M1 — complete. Live Git, process, command-resolution, package-link, toolchain,
   and clean isolated-worktree state were inspected. No existing `open-forge`
   command or accepted package link would be displaced.
2. M2 — complete. Align the three Loader authorities and Loading Reliability
   consequence, then record the unaccepted centralized `.agents`
   interoperability idea.
3. M3 — complete. Run focused evidence and consume `T7-LR1` and `T7-LR2` on one
   frozen prose and safety boundary.
4. M4 — active. Apply at most one grouped correction, refreeze every path, and
   create one coherent non-amending candidate commit.
5. M5 — pending. After candidate integration and exact release, link only from
   the verified restored integrated tree, run an isolated no-user-project
   smoke, record rollback and receipts, and return integration readiness.

### M1 Live Preflight

- The activation base and isolated worktree were clean. No relevant owned
  Open Forge, .NET, npm, or Node process remained, no `open-forge` executable
  resolved from the active command search path, and neither the main nor
  current-host Open Forge npm package was globally linked or installed.
- Current host and accepted package mapping are Linux x64,
  `@thelithiumforge/open-forge-linux-x64`, and
  `@thelithiumforge/open-forge`. The accepted `npm run cli:link` sequence
  performs an offline no-restore Native AOT publish, stages the host and main
  packages, then links platform to main to repository root. Its inverse is
  `npm run cli:unlink`.
- A new isolated worktree does not carry restored .NET intermediates or the
  existing dependency installation. Restore assets copied from another
  worktree retain originating-worktree paths, so the interrupted copied-assets
  publish probe is non-evidence. No npm link command ran, and all probe-created
  artifacts were removed from the candidate worktree.
- The accepted precondition is candidate review and commit first, then
  integration into the clean already-restored main worktree. The Overseer will
  revalidate its exact commit, tree, package inputs, resolved targets, global
  state, and rollback before releasing `npm run cli:link` there. No artifact
  copying may substitute for that source-identity proof.

### M2-M3 Evidence And Review Receipt

- The frozen review target changed exactly nine tracked prose paths and no
  untracked paths. Its patch SHA-256 was
  `3a82f7f665a0666276deccc0d737ec0d78682afe5ef72278c15d906db2cab398`;
  its sorted path-manifest SHA-256 was
  `77c649a67bf3943dd69f9050b682f9e0723668ace36d242a09b2c037f833e467`.
- Source and dogfood authored Loader content matched, both generated Entries
  regions were unchanged, and the authored Loader contained 77 non-empty lines
  within its 35–80 budget. Obsolete command, workstation-path, Git commit-time,
  implementation-path, and generated-drift checks passed.
- All nine advertised root or leaf help surfaces executed successfully against
  the integrated replacement CLI. Targeted Markdown formatting passed; the
  dogfood Loader retains only its pre-existing generated-region formatting
  difference, while its reviewed authored content is identical to the formatted
  source Loader.
- `T7-LR1` passed the exact writing/current-truth boundary with no material
  finding. Its only residual was one pre-existing out-of-scope evidence link in
  Loading Reliability.
- The first `T7-LR2` allocation was interrupted after prolonged silence with no
  output, process, finding, or mutation. One reviewer retry consumed the same
  stable review ID and passed package/local-link safety with no material
  finding. A mid-sequence external npm failure can leave known partial links,
  so exact pre-link revalidation and immediate `npm run cli:unlink` rollback
  remain mandatory.
- No grouped correction was required; `T7-LC1` remains unused.

## Follow-up Preflight

- Durable Distribution accepts one main package plus optional `linux-x64`,
  `darwin/osx-x64`, and `win-x64` platform packages. ARM remains undecided.
- The current reusable npm source is 10 files and 587 lines, with no authored
  JS, MJS, or CJS. The current implementation may be reused and generalized as
  required by the accepted graph; a rewrite is not implied.
- Clean historical restart `6ba0e060` has the identical package subtree but is
  stale. Dirty `1541d1ef` and rejected MJS lane `dc7c769f` are discard-only and
  must not be reused.
- The Darwin x64 package is the missing package in the current implementation.

## Expected Outcome

The accepted public package graph is one main package plus optional Linux x64,
Darwin x64, and Windows x64 platform packages. The historical Task 7
implementation realized Linux and Windows x64; this follow-up adds the Darwin
x64 package and completes the accepted graph. The same package-owned tooling can
stage the current host packages and explicitly link or unlink them for local CLI
use. It does not publish, download, install from a registry, or add CLI domain
behavior. ARM remains undecided and outside this Task.

## Package Contract

- Main package: `@thelithiumforge/open-forge`.
- Platform packages: `@thelithiumforge/open-forge-linux-x64`,
  `@thelithiumforge/open-forge-darwin-x64`, and
  `@thelithiumforge/open-forge-win-x64`.
- The main package has exact synchronized `optionalDependencies` on all accepted
  platform packages. They are not peer dependencies.
- Linux metadata is `os: ["linux"]`, `cpu: ["x64"]`, and `libc: "glibc"`.
  Windows metadata is `os: ["win32"]` and `cpu: ["x64"]`.
- macOS metadata is `os: ["darwin"]` and `cpu: ["x64"]`.
- Tracked templates are private and use version `0.0.0`. Staged manifests are
  public and synchronize all package versions to either a stable
  `major.minor.patch` release or `0.0.0-dev.sha-<full lowercase Git SHA>`.
- The main package alone owns `open-forge -> ./bin/open-forge.js`. Each platform
  package contains only its manifest, license, and native executable.
- The launcher admits only the accepted host/platform pairs, resolves the
  matching optional package, and directly hands process arguments and state to
  the native executable. It has no download, postinstall, fallback, telemetry,
  compilation, or domain behavior.
- Authored source is TypeScript. Generated JavaScript exists only below ignored
  `/artifacts/`.

## Local-Use Contract

- `manage.ts` performs literal `stage`, `link`, and `unlink` dispatch only.
- `stage` compiles the launcher closure, copies the supplied native artifact and
  license, and emits exactly the main package plus the requested host package in
  caller-owned artifacts.
- `link` detects only the current accepted host, reads the full local Git SHA,
  performs the exact no-restore Release Native-AOT publish, stages the current
  host packages, then uses ordinary npm platform-to-main-to-root link semantics.
- `unlink` reverses only those known package links.
- Link commands use offline, ignore-scripts, no-save, no-lock, no-audit, and
  no-fund flags. They run only when directly requested; task evidence never
  executes link or unlink.
- The frozen root legacy package identity, version, bin, files, dependencies,
  build command, and `cli:old` compatibility stay intact.

## Restarted Execution Capsule

1. Phase 1 — mastermind preflight, contract, and evidence boundary.
   - M1 fixed the clean baseline, authority, ownership, and protected paths.
   - M2 froze the package, local-use, and single-journey evidence contracts.
2. Phase 2 — one brilliant implementer owns implementation and the sole journey.
   - M3 built the package model, templates, stage operation, and thin launcher.
   - M4 built direct link/unlink, root integration, and removed the rejected shim.
   - M5 added the sole package end-to-end journey and its focused evidence gate.
3. Phase 3 — targeted verification.
   - M6 was owned by the task mastermind and passed TypeScript, static, stage,
     pack, and offline-install evidence.
4. Phase 4 — one fresh review and one grouped repair.
   - M7 spent the single review on contract, architecture, evidence, and
     maintainability; the task mastermind grouped both accepted corrections into
     one repair pass.
5. Phase 5 — mastermind acceptance and handoff.
   - M8 recorded final evidence, commit/tree identity, and integration readiness.

The discarded predecessor used a 73-test internal-contract suite. That suite is
rejected history only and carries no current implementation or acceptance
authority.

## Platform Expansion Horizon

1. Phase 1 — consume and revalidate Task 13's Linux D1 evidence.
   - M1 freezes Linux, macOS, and Windows x64 RID/package mappings and records
     ARM as undecided; feasibility alone does not add ARM packages.
   - M2 freezes exact protected paths, owned package contracts, local-link
     journeys, and target-host evidence without reopening accepted CLI behavior.
2. Phase 2 — one Task Mastermind and one Brilliant Implementer own the coherent
   expansion.
   - M3 adds the macOS x64 optional package and generalizes only the package-owned
     model, staging, launcher, and local-link surfaces required by the frozen
     graph.
   - M4 makes each accepted host build its native CLI through the existing .NET
     build boundary, then stage and link from `src/cli/package-managers/npm/`.
3. Phase 3 — focused owned-package evidence.
   - M5 keeps evidence intentionally small: exact main/optional package graphs,
     accepted-host staging and packing, and one simple host-selected owned
     `PackageEndToEnd` journey. The journey may stage, pack, install offline in
     isolation, resolve the launcher, and make one harmless reachability
     invocation such as `--version`. Assert only Open Forge package placement,
     launcher reachability, argument/process forwarding, and completion. Never
     test npm, Node, the operating system, a third-party library, or CLI command
     behavior.
4. Phase 4 — one review and closeout.
   - M6 is one fresh whole-task review followed by at most one grouped
     correction.
   - M7 records exact evidence, target limitations, commit/tree identity, and
     integration readiness.

This horizon preserves the original accepted 8/8 history. It does not amend or
reinterpret those commits, execute publication, authorize global link/unlink in
evidence, or add a generic multi-package-manager framework.

The follow-up uses one Task Mastermind, one Brilliant Implementer, and one fresh
reviewer, with at most one grouped correction. Its journey never uses live
global link or unlink in evidence.

## Platform Expansion Activation

- Working branch: `codex/npm-package-expansion`.
- Activation base: `e321fd45067cf7e2105e6ed62334636afb5fa136`, tree
  `5e84659b5310b8cc1672764b8ae3ce253122cbb4`.
- Authority relationship: the activation base is a clean descendant of Task 14
  integration `20807781`; the Task 7 package source is unchanged from the
  accepted ten-file historical foundation.
- M1 mapping: `linux-x64` owns Linux x64 with glibc, `osx-x64` maps to the
  `darwin-x64` npm package for macOS x64, and `win-x64` owns Windows x64. ARM is
  undecided and excluded.
- Profile: the maintainer-selected simple follow-up with one Task Mastermind,
  one Brilliant Implementer, one fresh whole-task reviewer, and at most one
  grouped correction.
- Consequence and recovery: the Task changes non-shipping package source and a
  local developer command. Git and ignored isolated artifacts provide recovery;
  publication, registry contact, and live global link state stay outside local
  evidence.
- Standard capability: ordinary TypeScript, Node process and filesystem APIs,
  npm optional packages, and the existing .NET publish boundary are sufficient.
  Exceptional machinery is `none`.

### M2 Frozen Boundary

| Accepted behavior                                                                                                                                                                  | Decisive evidence                                                                                                                                                             |
| ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| The main template declares exact synchronized optional dependencies for Linux, macOS, and Windows x64.                                                                             | Parse the four tracked manifests and inspect the staged main manifest.                                                                                                        |
| The package model maps `linux-x64`, `osx-x64`, and `win-x64` to the accepted npm package identities, host facts, and native filenames.                                             | Strict TypeScript plus isolated staging and packing for all three runtimes.                                                                                                   |
| Direct local linking recognizes the current accepted host, publishes its existing .NET RID, stages its package pair, and uses the existing platform-to-main-to-root link sequence. | Static review and focused TypeScript checks only; local or global link/unlink is never executed as evidence.                                                                  |
| The launcher selects the matching optional package and forwards one harmless invocation to its payload.                                                                            | One Linux-host `PackageEndToEnd` journey stages, packs, installs offline, resolves the launcher, invokes `--version`, and asserts the owned forwarding result and completion. |

- Expected production paths:
  `src/cli/package-managers/npm/darwin-x64/package.json`, `package-model.ts`,
  `stage.ts`, `main/open-forge.ts`, `main/package.json`, `manage.ts`, and
  `local-link.ts`. The Linux and Windows templates remain package-owned
  integration neighbors and change only if the frozen synchronized graph
  requires it.
- Expected evidence path:
  `src/cli/package-managers/npm/package-manager.e2e.test.ts`; it remains the sole
  package-manager test and contains one host-selected journey. Internal helper
  contracts and npm, Node, operating-system, runtime, library, or other
  third-party behavior are explicitly excluded from assertions.
- Direct integration neighborhood: the repository `LICENSE`, ignored
  `/artifacts/`, the existing .NET root publish project, and the root TypeScript,
  lint, formatting, and package-script configuration are read-only inputs.
- Protected paths and meaning: all C# source and tests, `.github/**`, root
  `package.json` and lockfiles, Task 13 and Task 22 records, CLI architecture and
  distribution authority, unrelated package managers, `/scripts`, and
  `src/cli/root/development-link`. No tracked JavaScript, MJS, or CJS may be
  added. Generated JavaScript is allowed only in ignored artifacts.
- Focused acceptance: strict root TypeScript, targeted lint and formatting,
  parsed package manifests, three-runtime isolated stage and pack evidence, the
  sole `1/1` Linux-host `PackageEndToEnd` journey, source inventory, protected
  path, and diff checks. Zero selected or executed tests, warnings, skips,
  network access, or stale artifacts cannot prove a gate.
- Stop before a new dependency, package-manager framework, postinstall,
  download, fallback, telemetry, registry contact, publication, live link or
  unlink, remote mutation, C# or CLI behavior change, root package change, ARM
  expansion, another test, or third-party behavior assertion.

### M3-M5 Implementation And Evidence Receipt

- Candidate: `496df91ba59bc999fb0fea0b4bfb3a6aec5f69e3`, tree
  `24e323ca3d37bda9ebea99568d62ba20d0caed0a`.
- Changed boundary: the Darwin template plus the existing main template,
  package model, stage entry, manager entry, and sole package journey. The
  model-driven launcher and local-link implementation required no change.
- Toolchain: Node `24.19.0`, npm `11.17.0`, TypeScript `6.0.2`, ESLint
  `10.9.1`, Prettier `3.9.6`, `typescript-eslint` `8.68.0`, and `@types/node`
  `26.3.0` from the existing read-only dependency installation.
- Strict TypeScript, targeted ESLint, and targeted Prettier passed with no
  warnings. Four of four tracked manifests parsed with the exact package graph
  and metadata.
- Isolated staging passed for `linux-x64`, `osx-x64`, and `win-x64`; six of six
  package tarballs packed offline. The platform tarballs contained only the
  manifest, license, and exact native filename, while each main tarball
  contained the launcher, package model, manifest, and license.
- The sole `PackageEndToEnd` journey selected, discovered, executed, and passed
  `1/1/1/1`; failures, cancellations, skips, todos, and warnings were zero. It
  staged and packed the Linux pair, installed both tarballs offline in
  isolation, resolved the installed main launcher, forwarded `--version` and
  an owned environment marker to the inert owned payload, preserved its exact
  output and empty stderr, and completed successfully.
- The exact six-path inventory, eleven-file npm source inventory, sole-test
  count, no-authored-JavaScript scan, protected-path comparison, diff check, and
  clean dependency-residue check passed. Generated JavaScript and package
  artifacts remained ignored.
- Dependency reuse during final evidence used one explicit temporary read-only
  `node_modules` symlink because ordinary TypeScript resolution does not consume
  `NODE_PATH`. The exact symlink was removed before the source commit and final
  inventory; no ignore or source accommodation was added.

### M6 Review And M7 Closeout

- Fresh whole-task review `T7-XR1` inspected candidate `aa745028`, tree
  `8f61dc066aa277dde76e12eed4915df8e4139a41`, against frozen baseline
  `beefac6a`, tree `374b72a9c1bf2e52dbf93faa9b050257cf25cebf`.
  It passed behavior and package contracts, production architecture and
  structure, and test/evidence quality with no material findings.
- The review independently confirmed the exact six-path change, synchronized
  three-package optional graph, correct RID and host mappings, model-driven
  launcher and local link, TypeScript-only source, sole owned package journey,
  clean protected paths, and the staged and packed artifacts.
- No grouped correction was needed, so the follow-up correction budget remains
  unused and the reviewed executable candidate is unchanged.
- Accepted limits: only Linux executed the installed-launcher journey. Darwin
  and Windows received staging and packing evidence. Live link or unlink, native
  publication for those hosts, registry contact, publication, and remote effects
  were intentionally not executed.

### Follow-up Flow Evaluation

- One Task Mastermind, one Brilliant Implementer, and one fresh Reviewer were
  used. The implementation had one ownership handoff back to the Task
  Mastermind, one review handoff, no semantic correction cycle, no accepted or
  rejected finding, no integration conflict, and no defect found after review.
- Three setup-only evidence issues occurred before the final gates: shared
  dependencies were initially invisible to TypeScript in the isolated worktree,
  one npm configuration attempt reused an invalid path combination, and one
  source-inventory assertion trimmed the first porcelain byte. None changed
  production or test meaning. The final receipts used a temporary dependency
  symlink with exact cleanup, distinct npm configuration, and literal inventory
  comparison.
- The simple flow fit this bounded package-graph delta better than a multi-phase
  implementation team. Keep it as a task-specific option for similarly small,
  already-frozen delivery changes; isolated-worktree dependency visibility
  should be preflighted before future TypeScript evidence.

## Review Disposition

- `T7-R1-F1` was accepted and fixed by letting cleanup skip only an absent
  staged-main directory while still removing the known global platform link.
- `T7-R1-F2` was accepted and fixed by distinguishing the accepted non-shipping
  package source from the separately authorized publication effect in the
  development guide.
- `T7-XR1` passed the platform-expansion candidate with no material findings;
  no follow-up correction identifier was consumed.

## Historical Evidence Boundary

Exactly one `PackageEndToEnd` Node test owns one Linux-host journey. It creates an
X_OK inert native payload, invokes the package-owned stage entry as a black box,
packs the staged main and Linux packages, and installs both tarballs together in
an isolated offline npm prefix. It asserts only that:

- the installed main package directory exists;
- the installed Linux package directory exists;
- the installed native payload is a regular X_OK file; and
- the installed `.bin/open-forge` resolves to the installed main launcher.

The journey never invokes the launcher or CLI, simulates Windows, imports package
internals, or tests npm, Node, manifest matrices, forwarding, missing-package, or
unsupported-platform behavior. Focused verification also requires strict
typecheck, lint, format, diff, source-inventory, protected-root, and no-C# gates.

## Follow-up Evidence Boundary

The follow-up owns one simple host-selected `PackageEndToEnd` journey. It may
stage, pack, install offline in an isolated prefix, resolve the launcher, and
make one harmless reachability invocation such as `--version`. It asserts only
Open Forge package placement, launcher reachability, argument/process forwarding,
and completion. It never tests npm, Node, the operating system, a third-party
library, or CLI command behavior, and it does not use live global link or unlink
in evidence.

## Final Acceptance

- Node `v24.19.0` with TypeScript `6.0.2`, ESLint `10.9.1`,
  `typescript-eslint` `8.68.0`, and `@types/node` `26.3.0` completed strict
  typecheck, lint, and targeted formatting with no reported warnings or errors.
- The sole package-manager test selected, executed, and passed `1/1`; failures,
  cancellations, skips, and todos were all zero.
- All six relevant JSON documents parsed, the template package graph and
  protected root package contract passed, and tracked JavaScript/MJS and C#
  changes were both zero.
- No publication, live npm link/unlink, registry or remote contact, CLI
  invocation, or live Windows claim was made.

## Stop Conditions

The historical package horizons stopped before live link or unlink. The current
provisional horizon keeps package publication, registry or network contact,
remote mutation, a new dependency, a generic packaging framework, postinstall
or download behavior, C# changes, package-source changes, and user-project smoke
outside scope. It also stops before live link or unlink until the reviewed
candidate is integrated and the Overseer releases the exact pre-authorized
local effect.
