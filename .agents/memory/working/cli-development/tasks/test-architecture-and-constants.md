---
open-forge:
  description: Audit symbolic constants, generated-region authority, and composable active-test foundations across the replacement CLI
  tags: [Memory, Working, CLI, Task, Testing, Architecture, Constants, Fixtures, Snapshot, Contextual]
---

# Audit CLI Constants And Test Architecture

## Task State

- State: Complete from exact clean `develop` baseline `8a1cb86`. Mutation
  Foundation contract closure remains paused at its persisted Framework-schema
  decision until this accepted branch is integrated.
- Responsible role: Mastermind.
- Parent: [Complete The Replacement CLI](00-cli-development.md).
- Task source: Maintainer direction recorded on 2026-08-27.

## Expected Outcome

The replacement CLI and its active tests use named, nearest-owner symbolic values
for constant-like contracts and a small composable test foundation for repeated
workspace, document, catalogue, process, and result scenarios. Tests remain cheap,
independently runnable, readable, thorough, and faithful to the behavior each tier
proves. The audit introduces no mocks, fake filesystem, generic utility bag,
universal fixture, or speculative browser abstraction.

## Authority

- [C# Callable Design](../../../../directives/csharp/design.md), including
  nearest-owner symbolic constants and readable cohesive call surfaces.
- [C# Style](../../../../directives/csharp/style.md), including interpolated
  templates and coherent raw multi-line construction.
- [Test Evidence Integrity](../../../../directives/open-forge/testing/evidence-integrity.md).
- [Evidence Tiers](../../../../patterns/testing/evidence-tiers.md).
- [Nearest Shared Scope](../../../../patterns/software/source-locality/nearest-shared-scope.md).
- [CLI Architecture](../../../crystallized/documents/cli/architecture.md).
- The completed [Active-Test Architecture](generic-improvements/active-test-architecture.md)
  remains accepted evidence, not a prohibition on a new audit of later code.

## Preflight Inventory

Before changing source, inventory all authored production and active C# test paths
and classify each finding by ownership and meaning:

1. Repeated or semantically constant strings and numbers: schema versions, marker
   text, policy names, stable IDs, path names, statuses, limits, timeouts, and
   formatting tokens.
2. Direct test literals that should consume an owned contract value versus
   independent literal oracles that must remain separate to detect an accidental
   production-constant change.
3. Duplicated document builders such as metadata frontmatter, Loader and routed
   entrypoints, generated navigation, overwrite companions, catalogues, lifecycle
   documents, and malformed variants.
4. Duplicated real-workspace, process, snapshot, setup, and cleanup behavior,
   including which consumers have exactly identical semantics and which only look
   structurally similar.
5. Expensive, timing-sensitive, hard-to-select, or over-broad tests and detailed
   branch coverage repeated above its cheapest proving tier.
6. Large exact strings or structured results that would be clearer as focused,
   explicitly updated snapshots without hiding safety assertions.

Record the complete candidate ledger before selecting mutations. Duplication is
evidence to inspect, not automatic authority to centralize.

## Accepted Preflight

The read-only Preflight ran from exact clean `8a1cb86` on
`codex/cli-test-architecture-audit`. It inspected 490 production C# files and 174
active test/support C# files containing 845 durable Fact/Theory declarations.
Every declaration has one `DisplayName`, `Feature`, and `Evidence` identity. No
mocking library, timing sleep, or network-test dependency is present.

The warning-free Release solution build completed in 39.3 seconds. Managed Unit
passed `1054/1054` in 6.5 seconds, Integration passed `378/378` in 6.4 seconds,
and EndToEnd passed `116/116` in 42.6 seconds, all with zero skips. The suite is
already quick and deterministic enough that this Task must not manufacture a
performance rewrite.

The candidate ledger freezes these material findings:

1. The exact generated-index start token occurs four times in production and 65
   times across active tests/support. Production has four separate owners for the
   same marker pair. Parser rejection vectors and public-format oracles still need
   independent literal bytes; they are not mechanical constant substitutions.
2. The canonical `.agents/loader.md` identity occurs in nine production call
   sites outside help prose, with several local constants. The accepted Source
   identity boundary can own it once without moving command policy.
3. `open-forge-markdown-v1` is repeated across Markdown, Lifecycle, and Extension
   Inspect. Markdown owns the policy identity; lifecycle and command consumers
   should reference that authority after the paused lifecycle branch is rebased.
4. Forty-seven active test files contain Open Forge YAML examples. Many are
   intentionally malformed parser or serialization oracles and remain explicit.
   Valid metadata, entrypoint, Loader, Skill, and generated-region builders are
   duplicated across Generated Navigation, Context, Route, Integration, and
   EndToEnd fixtures and have a genuine pure seed boundary to inspect.
5. Twenty-one Context operation/finding test methods plus one case-mismatch method
   create or mutate real directories and files while declaring Unit evidence.
   Two Extension embedded-catalogue tests enumerate and hash authored files in the
   Unit project. These are Integration evidence; their neighboring pure grammar,
   model, formation, and presentation cases remain Unit.
6. No checked-in snapshot artifact or snapshot dependency exists. Large exact
   presentation data is a candidate for focused snapshots, but no snapshot
   infrastructure is accepted without a concrete projection that becomes clearer
   and retains direct safety assertions.

The Preflight changed no source, tests, project, package, configuration, generated
area, contract, lifecycle schema, public behavior, or test identity.

## Protected Boundaries

- The paused `codex/mutation-foundation-contracts` branch and worktree, all
  `Framework/Mutation/**`, `Framework/Recovery/**`, lifecycle writer/schema
  meaning, and its Task records.
- Public command grammar, results, streams, exits, JSON property order, findings,
  help, diagnostics, filesystem behavior, Native AOT behavior, and generated
  Markdown bytes.
- Intentionally malformed parser fixtures, independent public-format oracles, and
  exact-byte compatibility cases whose literal independence detects contract
  drift.
- Packages and dependency versions. No snapshot or test-helper package is needed.
- Preserved/archived tests, sealed records, generated output, remotes, and user Git
  configuration.

## Accepted Slices

### Slice 1 — Production symbolic ownership

- Add one neutral Markdown generated-region syntax owner and replace the four
  production marker definitions without changing parsing or fingerprint behavior.
- Let Find consume accepted parsed generated-region facts instead of reparsing the
  same markers locally when the fact graph is sufficient.
- Add one Source-owned canonical Loader identity and replace exact production
  copies where that source identity—not help prose—owns the value.
- Centralize the Markdown fingerprint policy only if Lifecycle and Extension
  regressions prove the dependency direction. Do not touch persisted schema.

### Slice 2 — Pure document seeds and local fixture composition

- Add a cohesive pure document-seed capability under TestSupport for valid Open
  Forge metadata, entrypoints, Loader/generated regions, and Skill documents.
- Add a Unit-to-TestSupport reference only after direct Unit consumers are
  selected; the reference is not itself evidence of reuse.
- Migrate exact valid builders in bounded feature groups. Keep malformed inputs,
  byte or line-ending variations, and independent output oracles explicit.
- Keep command-specific workspace topology, scenario policy, and assertions local.

### Slice 3 — Evidence-tier correction

- Move the real-filesystem Context operation/finding and case-mismatch evidence to
  the mirrored Integration Context scope without changing identities or assertions.
- Split the two authored embedded-catalogue filesystem cases from the pure
  Extension package contract tests and place them in mirrored Integration scope.
- Audit the small missing-path `Directory.Exists` checks separately; move them only
  when they prove OS behavior rather than a direct no-effect callable contract.

### Slice 4 — Focused snapshot decision

- Inspect the largest stable presentation/result projections after seed and tier
  cleanup. Add no snapshot mechanism when explicit typed assertions remain clearer.
- If one projection earns a snapshot, use a read-only checked-in artifact with an
  explicit targeted update path and retain direct safety/state assertions.

Each slice is independently reviewable and may end as a justified no-op. Production
constants, test support, tier moves, and snapshots do not enter one broad cleanup
commit.

## Slice Progress

### Slice 1 — Complete

- `MarkdownGeneratedRegionSyntax` now owns the exact Entries heading and generated
  marker syntax consumed by Markdown parsing, semantic fingerprinting, Loader
  entry parsing, and Find body-tag exclusion. Independent malformed-input and
  public-format test literals remain unchanged.
- `SourceLogicalPath.LoaderPath` now owns the canonical Loader source identity.
  Context and Route consumers retain their local policy while consuming that
  source-owned value.
- `MarkdownFingerprintPolicy.Name` now owns the semantic Markdown fingerprint
  policy. The existing reader, lifecycle, and Extension Inspect constants remain
  compatible aliases, so persisted and public contracts are unchanged.
- Find does not consume `MarkdownDocumentFacts.GeneratedRegion` in this slice.
  Its accepted scanner semantics recognize one ordered marker pair without
  requiring the parser's stricter final canonical Entries section. Substituting
  that fact would change availability behavior and therefore requires a separate
  contract decision rather than a constants cleanup.

Evidence from the completed slice is a warning-free Release solution build,
managed Unit `1054/1054`, Integration `378/378`, and EndToEnd `116/116`, all with
zero failures or skips, plus a clean `git diff --check`.

### Slice 2 — Complete

- `OpenForgeDocumentSeed` now owns composable valid Open Forge metadata and
  frontmatter, Skill metadata and documents, and generated Entries documents.
  `GeneratedEntriesSeed` uses public `required`/`init` configuration for the
  required content and named initializer properties for line ending, prefix, and
  final-line-ending choices instead of same-type positional parameters.
- Unit references TestSupport because Generated Navigation and Context now have
  direct valid-document consumers there. Generated Navigation Unit/Integration,
  Context Unit/Integration/EndToEnd, and Route List/Inspect Integration fixtures
  consume the shared pure seed while retaining local scenario topology and
  assertions.
- `GeneratedLoaderDocumentBuilder` remains the Loader-specific facade and now
  composes the shared generated-region seed without changing its exact final
  newline behavior. Intentionally malformed parser data, line-ending rejection
  vectors, serialization oracles, alias-specific YAML, and public output strings
  remain explicit and independent.

Evidence from the completed slice is a warning-free Release solution build,
focused Generated Navigation Unit `14/14` and Integration `3/3`, and full managed
Unit `1054/1054`, Integration `378/378`, and EndToEnd `116/116`, all with zero
failures or skips, plus a clean `git diff --check`.

### Slice 3 — Complete

- Twenty-nine real-filesystem Context cases now live in the mirrored Integration
  Context scopes with unchanged claims and case data. They exercise the real
  operation, authored documents, operating-system case behavior, or injected
  terminal events over a real workspace.
- The first tier move also moved five presentation cases because they used the
  real operation only to manufacture a typed result. Final audit corrected that
  over-broad placement: `ContextPresentationTestData` now forms representative
  typed results entirely in memory, the renderer cases are Unit evidence, and the
  previously combined help/diagnostic case is split into two focused Unit claims.
- The Context Integration workspace now composes the ownership-safe
  `TemporaryWorkspace`. New tracked replace-text, replace-bytes, and move-file
  operations preserve strict cleanup ownership when a scenario mutates an
  existing seed. The fixture did not weaken cleanup or recursively delete
  unproved content.
- Two embedded Extension catalogue cases and their copied authored inventory
  moved from Unit to Integration. The catalogue case discovers every package
  from the authored inventory and manifest, then relates all package fields,
  payload paths, embedded bytes, and hashes without hard-coded package identity,
  version, dependency, or payload-count expectations. The archive case compares
  its exact decoded canonical asset stream instead of recompressing gzip bytes,
  whose implementation output differed under Native AOT. Pure stable-ID, path
  grammar, and manifest parsing remain Unit.
- The three remaining Unit `Directory.Exists` observations use unique missing
  paths only to prove terminal or absent-workspace callables perform no selection
  or mutation. They create, enumerate, read, write, move, and delete nothing, so
  moving them would not add meaningful operating-system evidence.

The final managed suite is Unit `1024/1024`, Integration `409/409`, and EndToEnd
`116/116`, all with zero failures or skips. Unit plus Integration contains `1433`
focused cases: the one-case increase is the deliberate help/diagnostic split.
The supported `linux-x64` Native AOT Integration executable passes `409/409`,
including the corrected catalogue and archive evidence.

### Slice 4 — Complete As A No-Op

No snapshot mechanism is introduced.

- `FindPresentationTestData` is large because it builds a typed scenario catalogue
  covering status, query, selection, match, projection, finding, and next-action
  states. Its renderer tests assert exact summaries, ordering, bounded escaping,
  content presence, and safety omissions at the claim that would fail. A golden
  rendering would duplicate those assertions and make failures less local.
- `RouteListTopologyTests` is large because it proves distinct graph, ancestry,
  depth, selection, provenance, coverage, and failure branches. The relevant
  outputs are small typed rows and findings; snapshotting the object graph would
  hide the invariant being proved.
- Find, Route List, Context, and Extension JSON evidence already asserts concrete
  property order, scalar/null forms, status, codes, and selected values. Their
  current structural assertions are easier to review than a full JSON artifact
  and preserve direct safety/state claims.
- No active test emits one otherwise-unreviewable large stable projection that
  earns a checked-in golden artifact. Adding a package or a custom update harness
  would therefore create infrastructure without evidence value.

The decision changes no source, test, package, project, expected output, or test
identity. Revisit snapshots only when one concrete stable projection becomes
materially clearer as a reviewed artifact while retaining direct safety and state
assertions.

The maintainer-requested future adoption boundary is recorded separately as the
[LithSnap-Backed CLI Presentation Snapshots](../../../emerging/ideas/lithsnap-backed-cli-snapshots.md)
idea. It names the first concrete candidate suites and requires LithSnap to prove
explicit read-only baselines, source-generated serialization, and xUnit v3 Native
AOT compatibility before Open Forge adds any snapshot dependency.

## Final Acceptance

- Locked dependency state remains unchanged; no package or project dependency was
  added.
- Locked restore and the final Release solution build are warning-free;
  `dotnet format whitespace --verify-no-changes` reports zero file changes while
  retaining the formatter's pre-existing workspace-load warnings, and
  `git diff --check` passes. Full managed Unit `1024/1024`, Integration `409/409`,
  and EndToEnd `116/116` evidence has zero failures and skips.
- The supported local `linux-x64` root and EndToEnd Native AOT gates remain
  `116/116` from the behavior-complete audit branch. The final republished Native
  AOT Integration executable passes `409/409` after the last test-only correction.
- Unit contains no real filesystem mutation or enumeration. Its three remaining
  `Directory.Exists` observations are unique missing-path no-effect checks. No
  Unit/Integration namespace mismatch, mock dependency, snapshot dependency,
  timing sleep, remote action, or Open Forge push was introduced.
- Production contracts, public output, persisted schema, generated Markdown
  bytes, malformed independent oracles, and all protected mutation surfaces are
  unchanged.

## Generated Navigation Boundary Investigation

Treat marker constants and marker removal as two separate decisions.

- Centralize any retained exact generated-region tokens at the capability that
  owns their syntax; tests should not scatter copies such as
  `<!-- open-forge:generated-index:start -->`.
- Independently evaluate whether Markdig can safely identify one owned `Entries`
  heading and list from the AST, preserve all unrelated authored bytes, distinguish
  managed from authored lists, handle empty or malformed regions, and reject
  ambiguity.
- Do not remove explicit markers merely because a heading and list can be found.
  The replacement must prove ownership, idempotence, bounded replacement,
  compatibility, diagnostics, and recovery for every accepted rooted, detached,
  overwrite, malformed, empty, and adversarial case.
- If AST ownership changes the accepted authored document contract or lifecycle
  compatibility, return to Architecture and contracts before production work.

## Desired Test Foundation

- Unit tests exercise focused methods, classes, or pure stages over explicit
  in-memory inputs and outputs.
- Integration tests exercise one module across the real boundary it owns, usually
  an isolated real filesystem, process, serialization, or package layout.
- EndToEnd tests treat the published CLI as a black box and retain only complete
  critical journeys, public streams, exits, cancellation, and unchanged-state
  evidence.
- Tests use stable descriptive identities and cover accepted happy, invalid,
  boundary, failure, and interruption paths.
- Scenario data is assembled from small named seeds, builders, and factories.
  A metadata document and a metadata-plus-navigation document should share the
  same frontmatter/body primitives rather than duplicate concatenation, while
  malformed evidence keeps the explicit bytes needed to prove rejection.
- Real setup and teardown remain composable and owned. Promote support only to the
  nearest common scope of at least two consumers with identical semantics; keep
  feature-specific policies and richer fixtures local.
- Prefer real boundaries and observable state. Permit a test double only when the
  real boundary is unsafe or cannot be made deterministic at reasonable cost, and
  record that exception locally.
- Use focused snapshots when they improve review of exact strings or large stable
  projections. Keep safety, containment, mutation, receipt, and state-transition
  claims as direct assertions.

## Execution Shape

1. Read-only Preflight and candidate ledger.
2. Freeze any missing generic Directive or Pattern meaning before code depends on
   it; link reusable shapes rather than embedding framework-specific examples in
   generic instructions.
3. Split the accepted findings into small locality-safe Purple slices. Keep
   production constant ownership separate from test-support promotion and from
   any generated-region contract change.
4. For each slice, preserve test identity and behavior, run focused evidence,
   obtain correctness and improvement review, and commit only an accepted boundary.
5. Finish with warning-free Release build, format and diff checks, full managed
   Unit/Integration/EndToEnd evidence, supported Native AOT gates, source/locality
   audits, and an independent architecture review.

## Stop Conditions

- Stop if a shared helper would mix distinct scenario meaning, hide required setup,
  create a generic repository or utility bag, or grant tests broader authority.
- Stop if importing a production constant would make a conformance assertion
  tautological or prevent detection of an accidental protocol change.
- Stop if a snapshot would hide a safety invariant or ordinary test execution
  would mutate its expected output.
- Stop if parser-based generated-region replacement cannot prove one unambiguous
  owned target and exact preservation outside it.
- Stop on a new package, test framework, public behavior, persisted schema,
  platform promise, or browser-specific production abstraction without a separate
  accepted decision.

## Deferred Relationship

The [Browser WebAssembly CLI Playground](../../../emerging/ideas/cli-browser-wasm-playground.md)
may later consume identical seed data, but it does not justify shared code during
this audit. Promotion requires demonstrated semantic reuse after a browser spike.
