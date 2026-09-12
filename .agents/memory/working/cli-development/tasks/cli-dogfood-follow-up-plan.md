---
open-forge:
  description: Fix stale test fixtures, audit manual parsing, and make Doctor diagnostics accurate and understandable in sequential steps
  tags: [Memory, Working, Contextual, CLI, Task, Plan, Testing, Parsing, Doctor, Dogfood]
---

# CLI Dogfood Follow-Up Plan

## Current Status And Next Work — 2026-09-12

Completed and squash-integrated: stale fixture repairs plus the brittle-fixture
Observation and automatic Extension embedding (b4a06740); native option delimiter
alignment (6b6f054b); Doctor coverage/source correctness (ef7bdb3b); Doctor/Status
generated-navigation alignment (c5883662); documentation, source examples and
durable artifact-independent checkpoints (5e34f1a8). The installed CLI is the
qualified 29de40a0 native source, copied into global npm packages. Latest full gate:
3,232 Unit, 1,745 Integration in both modes, 123 public cases in all three modes,
zero failures/skips. Those are prior qualification results, not a new test run.

The parsing audit covers all commands; only its native-delimiter fix is complete.
Further YAML/Markdown/parser simplifications are backlog, not work silently
implemented or prerequisites for clearer rendering. The full 28-command
presentation surface audit is done; Doctor/Status renderer changes and deduplication are
implemented and fully qualified. Six mutation-command comparisons expose ignored view selection
and missing Library affected-path details.

The user approved implementation of the presentation proposals, both views for
all commands and JSON, compact-to-expanded fallback, and automatic terminal
colour with no colour flag or configurable palette. No AI view is added.
The [implementation capsule](cli-presentation-implementation.md) owns the
sequential sets, frozen boundaries and evidence. Set 1 is squash-merged as
`5bb4aefa`; Set 2 Doctor/Status is squash-merged as `54c28134`. Set 3
Library human views are squash-integrated as `56538d63`; Set 4 Extension human
views are squash-integrated as `a8a2e48f` after all six suites passed. The user
narrowed the next slice to References, Context and Find, then a pause for the
minimal-output discussion. References, Context and Find human views and affected
contracts are complete at qualified source `39eed987`: 3,306 Unit, 1,745
Integration in each managed/native mode and 123 public journeys in each of three
runtime configurations pass, with zero failures/skips. Managed and native direct
trials pass both human views and JSON, preserve exact Context body bytes and Find
TSV rows, and leave workspace/state unchanged. This closeout accompanies the
authorized local squash onto develop; its commit completes the narrowed slice.

Human views are complete for all 28 commands. Route Create/Init/Update/Move/
Remove is qualified at source `c31915e3`: 3,334 Unit, 1,745 Integration in each
managed/native mode and 123 public journeys in each of three runtime
configurations pass, with zero failed, pending, skipped or other results.
This closeout accompanies its local squash. Core maintenance is integrated as
`638a850e`; Route List/Inspect as `5c325960`. Accepted compact JSON is next, with
field membership and implementation readiness recorded in the capsule.
Stop after command work and before colour; final guidance remains pending.
Human command coverage is distinct from completion of the JSON work.
The installed command was directly observed at `56538d63`; this set uses the
qualified built executables and did not change the global installation.
Expanded remains the default. Result limits, a compact default and an even smaller
file-oriented view are saved proposals; none is implemented. Display filtering,
parser simplifications P2–P6 and configurable-threshold analysis stay backlog.
The implementation capsule records each sequential freeze and qualified squash.

Cleanup completed: the parser probe was outside the repository and has no matching
committed project/path history. Its source/project and temporary compiled output,
nine task helper/draft files, and five owned task artifact directories are removed.
Committed regressions and ordinary delivery scripts remain. The copied installed
CLI still reports its qualified version after deletion. Do not create disposable
experiment programs as retained repository requirements.

## Outcome And Authority

The user set this order on 2026-09-12 after direct use of the locally installed
CLI. This is a new bounded horizon under Task 27 “C# Structural Streamlining”.
Work stays sequential, with local squash integration into `develop` after each
verified set. Remote operations remain unauthorized. The earlier helper
refactor is preserved at `50a36f35` and will join the qualified baseline restoration
after the higher-priority fixture and catalogue work. Its predecessor fixture correction is
`b07f239e`. Do not lose either commit or their receipts.

The [Task 27 record](csharp-structural-streamlining.md) retains the initial
installed-command findings and exact evidence. Current CLI contracts define
behavior, the Architecture defines placement, and the public-facing writing
standard governs revised explanations. This plan does not accept unreviewed
parser behavior changes or hide genuine product defects by changing tests.

## User Boundary — 2026-09-12 Continuation

The user explicitly requires preservation of useful functionality. Authorized
work is alignment fixes, improvements and optimizations. Check with the user
before any other behavior change. Keep the plan and checkpoints current so the
reasoning, frozen baseline, exact changes and next action survive restoration.

Implemented so far: stale fixture repairs and Observation; automatic Extension
embedding from manifests/resources; native option delimiters with typed validation
preserved; D1 Doctor coverage/source/host alignment. No diagnostic kind, JSON
field, warning visibility default, mutation capability, ownership or recovery
policy has been removed. D1 source candidate `c8d786ed` is qualified: all six managed/native suites
pass (3,232 Unit, 1,745 Integration and 117 public cases per applicable mode).
D1 was squash-integrated at `ef7bdb3b`; the installed CLI is its qualified build.

The [presentation audit](cli-presentation-audit.md) records proposals,
not implemented behavior. Diagnostic-kind retirement is withdrawn. Changing
default visibility, adding flags or changing JSON representation requires
explicit user review and an aligned public contract before implementation. Preserve candidate facts,
counts, exact edit coordinates and real errors. A clearer human presentation
must not silently remove useful information.

D2 is complete. The [generated-navigation capsule](cli-generated-navigation-alignment.md)
records source `29de40a0`, all six qualified suites, 24 direct native checks and
the local integration receipt. The installed CLI is this candidate. Doctor
presentation is next. Preserve diagnostic kinds and existing default visibility;
no new filter is selected. User review of the command examples precedes Interface
updates and renderer implementation.

## Parsing Backlog — Accepted Deferral 2026-09-12

The user explicitly moves the remaining parsing simplifications to backlog.
They do not block presentation work. The [parsing audit](cli-parsing-doctor-audit.md)
retains exact source owners, reasons and evidence required before each change.

| Item | Deferred work | Boundary to resolve when selected |
| --- | --- | --- |
| P2 | References attached-token fallback | Prove parser-owned typed tokens preserve ordered include/exclude selection. |
| P3 | Retain YAML event facts; simplify Skill metadata recovery | Preserve source spans, duplicate/alias semantics and byte-preserving edits; resolve differing Find alias policy explicitly. |
| P4 | Route Inspect Markdown facts and compatible generated-entry recognizers | Keep canonical sentinel/heading rules, opaque sections and intentionally distinct generated-region policies. |
| P5 | Evaluate library frontmatter support | Preserve incomplete-input behavior and exact leading-boundary spans; no automatic replacement. |
| P6 | Library invalid-result operand extraction | Use typed argument facts where applicable; no demonstrated public misidentification yet. |

P1 native option delimiters is complete. No deferred parser work is implemented
in the presentation analysis. Human grouping also leaves repeated JSON payload
size as a separate machine-output design candidate; no schema change is implied.

## Sequential Steps

### 1. Repair Outdated Fixtures

- Run the current managed CLI suites and ordinary repository-tooling tests.
  Classify every failure as an outdated fixture, a product mismatch, an
  environment failure, or an unresolved issue before editing expectations.
- Fix every confirmed outdated fixture. Preserve assertions that express the
  intended scenario, public behavior, identity, safety, ordering and effects.
  Keep fixture corrections in separate commits from production refactoring.
- Avoid constructing test scenarios by replacing incidental prose in the live
  Framework payload. Prefer small explicit fixtures. When exact Markdown or
  output wording is itself the contract, prefer reviewed snapshots and explicit
  snapshot updates. Snapshot use must not replace semantic safety assertions.
- Record an Observation with the user's preference, concrete failure, correction,
  and future guidance. Re-run affected tests, then qualify the complete relevant
  test boundary before local integration. Report remaining genuine defects.

Completion: all failures caused only by outdated fixtures are corrected and
verified, with an honest inventory of any other remaining failures.

### 2. Audit CLI, YAML And Markdown Parsing

- Reproduce both spaced and equals `--tag` forms against the same installed
  executable. Trace parser configuration, delimiter guards and binder behavior.
  Verify the pinned System.CommandLine behavior through its supported API and
  official documentation. Include other options with equivalent custom guards.
- Inventory manual YAML and Markdown parsing across every command and the
  shared Framework capabilities they consume. Inspect actual call paths, not
  just lexical matches. Include frontmatter, tags, headings, links, generated
  regions, source-preserving edits and command-specific raw-token processing.
- Classify each case: ordinary library adaptation, product-owned value grammar,
  source-coordinate preservation, redundant parsing, or an accepted exception
  requiring reconsideration. Distinguish parsing from formatting and exact
  marker recognition. Do not mechanically replace legitimate byte-preserving
  mutation logic with a parser that discards source coordinates.
- Report exact locations, current reason, user impact, preferred library/BCL
  replacement, proposed contract updates, risk and evidence needed per change.
  Keep consequential parser and accepted-exception changes pending user review.

Completion: an auditable command/capability coverage inventory and a prioritized
change report, including the concrete cause of the `--tag` discrepancy.

### 3. Diagnose And Improve Doctor

- Explain why a fresh installation returns incomplete route facts and an
  untrusted Extension lifecycle while Status reports complete. Separate missing
  observation from a real invalid installation and reproduce both conditions.
- Measure result size by diagnostic domain, finding kind and repeated candidate
  payload. The observed repository JSON was 168,127,363 bytes, with 10,986
  local-reference findings. Attribute the growth before choosing a reduction.
- Inspect compact/default/expanded human output and JSON separately. Propose
  actionable summaries, grouping, counts, deliberate detail expansion and
  formatting. Preserve useful complete machine evidence unless a reviewed
  public-result change explicitly changes that promise.
- The user's follow-up identifies a warning buried among candidate `basis`,
  `provenance`, raw byte locations and “bounded reference” language. Make one
  actionable broken-link finding the primary unit. Show a clear severity label,
  a useful source path and line, the broken destination and the next step.
  Put candidate matches and internal evidence behind deliberate detail.
  Source coordinates are not memory addresses; raw offsets are usually
  unnecessary in the normal human view. Investigate whether candidate facts
  are being counted as separate warnings instead of details of one problem.
- “Errors by default, warnings on demand” is the user's tentative preference,
  not yet an accepted filtering contract. Assess it alongside concise actionable
  errors and warnings with informational facts hidden. Keep warning/error counts
  and overall health truthful when details are filtered. Colour is deferred.
- Use the [Writing Standard](../../../crystallized/documents/maintenance/writing.md)
  to replace opaque language with the affected source, concrete problem,
  practical consequence and next action. Distinguish unavailable facts from
  broken content. Keep severity and certainty truthful.
- Implement unambiguous correctness and wording fixes with focused evidence.
  Return consequential defaults, output-schema or diagnostic-policy choices
  as concrete reviewable proposals before relying on them.

Completion: explained causes, verified accepted corrections, clearer presentation,
and explicit disposition of any choices still requiring user review.

## Execution And Evidence

Root owns direct implementation, investigation, review and integration. No
parallel agent work is selected. Each step freezes its own source boundary and
tests before mutation. Stale-fixture work is a direct test-support change;
parser and diagnostic work are reassessed when actual product impact is known.
Use existing .NET, System.CommandLine, YamlDotNet and Markdig capabilities.
No new dependency or custom parsing framework is authorized.

Focused managed evidence is the default inner loop. Broader managed and Linux
Native AOT gates apply when shared parsing, public results, serialization or
composition changes. No foreign-host execution or publication is claimed.
Record source identity, exact reproduction commands, toolchain, counts, failures
and conclusions in tracked Task/checkpoint records. Raw logs, generated binaries
and full machine reports under `artifacts/` are disposable. Required regression
code and helpers belong in tracked source. Keep all unrelated work intact.

## Current State

The user's later `continue` instruction extends the work: make Extension
embedding automatic using the existing resource pattern, review the complete
presentation layer in frozen sequential stages, and remove duplicate Doctor
diagnostic detail. The concrete broken-link wording is accepted as the direction
for human messages. Check maintenance, Interface and Behavior sources before
each stage; update them for intentional changes. Do not infer new semantics
from a wording rewrite.

Task 30 [Extension Catalogue Synchronization](extension-catalogue-synchronization.md)
is complete and squash-integrated at `b4a06740`. Task 27 retains the later
parser and presentation work. Its audit is a baseline report, not a current
claim that the later accepted changes remain unauthorized.

Frozen sequence: automatic embedded catalogue and its parity evidence; native
option delimiters; Doctor correctness; presentation review and bounded command
sets, including human deduplication. JSON compaction is separate backlog. Freeze
each stage's baseline, intended behavior delta, tests and applicable contracts
before editing production.
Squash verified stages into local `develop` while preserving unrelated changes.

- Done: Task 27's [native-delimiter stage](cli-native-delimiters.md), phase 3/3,
  milestone 3/3. Candidate `5bef9a24` passes the six-suite managed/native gate
  and 19 direct native commands; this completion record accompanies local squash.
- Done: fixture repairs, the Observation, and automatic catalogue implementation.
  Candidate `ee5a5599` passes 3,231 Unit, 1,724 Integration and 112 public tests.
  Both strict catalogue parity tests pass. No behavioral assertion was weakened.
- Report: the [parsing and Doctor audit](cli-parsing-doctor-audit.md) records the
  complete command coverage, concrete delimiter leak, manual syntax recovery,
  Doctor size attribution and fresh-install diagnostic contradictions.
- Next: correct Doctor before the full presentation
  pass. Doctor default filtering and JSON changes remain proposals; colours follow.
- Integration: the restored baseline and earlier helper refactor are squash-merged
  into local develop at `b4a06740`. The earlier catalogue mismatch is fixed;
  Task 30 records native qualification and existing ownership-transition limits.
  No remote operations occurred.

## Final Stage: Reusable CLI And C# Guidance

Requested by the user after D2; perform this consolidation at the end of the
remaining CLI presentation/refactoring sequence. This is required queued work,
not a completed guideline. Update the existing CLI implementation Directive and
C# design Directive, then add or extend focused CLI design Guidance that can be
shared between projects. Keep project-specific wire contracts in their owners.

Cover native parser/runtime/library behavior before custom code; manifest/resource
discovery instead of duplicate catalogues; no speculative unreleased-version
compatibility or migration rules; focused snapshots for actual presentation
contracts and authored fixtures for behavior; disposable generated output and
tracked recovery records. Remove solved exceptions from current guidance instead
of carrying old decision history into product text. A required current exception
needs a demonstrated capability gap and explicit scope.

For CLI experience, cover predictable command/flag meanings, useful defaults,
concise outcome-first output, clear severity/path/line/action, progressive detail,
nonduplicated information, human versus JSON output, compact/expanded/verbose
roles, help/examples, terminal/piped/noninteractive behavior, prompts, dry-run
review, exit status/streams, and optional colours/accessibility. Preserve useful
capabilities; do not infer new defaults, diagnostic kinds or schemas from wording.
For implementation, keep parser, typed request, operation, result and rendering
boundaries clear; reuse standard capabilities and keep related tests/contracts
with their owner. Record verified lessons from each completed stage.

Existing authorities to reconcile: the CLI shared-operation contract, shared
global-flags and result-coordinates contracts, CLI Architecture, Writing Standard,
CLI implementation Directive and C# design/style Directives. No active portable
CLI UX Guidance was found in the current Guidance routes. Historical CLI design
notes are archived and do not define current rules. Choose one reusable guidance
owner and link it from the selected CLI route; avoid duplicating full contracts.

Acceptance: the full current presentation pass supplies real examples; guidelines
cover the listed experience and implementation lessons; current contracts remain
aligned; obsolete exceptions and artifact-only requirements are absent; navigation
and links work. Squash this final reviewed documentation set into develop.

### Review-Correction Stage Verification

The documentation-only stage starts at D2 squash `c5883662` on
`codex/cli-guidance-follow-up`. All 14 changed Markdown files have valid local
file links; `git diff --check` passes. No CLI production, test or build source
changed, so the previous qualified native binary remains applicable. Installed
Extension Install and Update help both expose `--source` and `--dry-run`.

The global npm packages were installed from the tested staged package tarballs
with `npm install --global --offline --ignore-scripts --no-audit --no-fund`.
Both installed packages are copies, with the native version/hash recorded in the
checkpoint. Running `open-forge extension list --available --json` from a fresh
temporary directory returns complete with empty stderr. Packaging inputs under
artifacts may be deleted; the installed packages do not link back to them.

Artifact-rule locations: CLI implementation Directive, Physical Workspace And
Projects; Testing Evidence Integrity, canonical execution receipts; this plan's
Execution And Evidence section. These now distinguish generated output from
durable tracked state. Calibrated Agent Reasoning recommends raw logs in artifacts;
that concerns disposable raw output, not exclusive storage for required inputs.
No tracked source/build entry imports the exploratory task scripts, and Git tracks
zero files under `artifacts/`. This is a documentation and local-install
verification, not a new execution of the full CLI suites.
