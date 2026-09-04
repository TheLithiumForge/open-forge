---
open-forge:
  description: Prepare and prove the thin npm package graph and explicit local-link workflow
  tags: [Memory, Working, CLI, Task, Delivery, Npm, Package, Contextual]
---

# Task 7: npm Package Manager Release and Local Linking

## Task State

- State: Historical 8/8 horizon complete and squash-integrated; the separate
  platform-expansion horizon is queued at phase 1 of 4, milestone 0 of 7.
  Preparation is complete and activation is authorized from fresh Task 14
  integration base `20807781` in a lane disjoint from Status.
- Parent: [CLI Delivery](_delivery.md).
- Profile: Reopened maintainer-selected simplified single-owner flow: one Task
  Mastermind, one Brilliant Implementer, one fresh reviewer, and at most one
  grouped correction.
- Historical review budget: maximum 1; `T7-R1` was consumed with
  `CHANGES_REQUIRED`.
- Historical correction budget: maximum 1; `T7-C1` was consumed by the grouped
  M7 repair.
- Follow-up review budget: maximum 1, unused.
- Follow-up correction budget: maximum 1, unused.
- Council budget: 0.
- Current owner: none until activation; Task 13 owns Linux D1 only, while this
  follow-up owns the accepted x64 package graph and journeys.
- Restart baseline: `195ff13ecff6a598dd18ef22a0335c1dc75e6736`.
- Historical progress: 8 of 8 milestones complete in phase 5 of 5.
- Follow-up progress: 0 of 7 milestones complete in phase 1 of 4; preparation is
  complete.
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
implementation realizes Linux and Windows x64; the Darwin x64 package is the
missing follow-up implementation. The same package-owned tooling can stage the
current host packages and explicitly link or unlink them for local CLI use. It
does not publish, download, install from a registry, or add CLI domain behavior.
ARM remains undecided and outside this Task.

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

## Review Disposition

- `T7-R1-F1` was accepted and fixed by letting cleanup skip only an absent
  staged-main directory while still removing the known global platform link.
- `T7-R1-F2` was accepted and fixed by distinguishing the accepted non-shipping
  package source from the separately authorized publication effect in the
  development guide.

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

Stop before package publication, registry or network contact, actual global
link/unlink, remote mutation, a new dependency, a generic packaging framework,
postinstall or download behavior, C# changes, CLI runtime behavior tests, broader
architecture or plan edits, or any expansion of the single-journey evidence
boundary.
