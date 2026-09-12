---
open-forge:
  description: Fix stale test fixtures, audit manual parsing, and make Doctor diagnostics accurate and understandable in sequential steps
  tags: [Memory, Working, Contextual, CLI, Task, Plan, Testing, Parsing, Doctor, Dogfood]
---

# CLI Dogfood Follow-Up Plan

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
not implemented behavior. In particular, retiring seven redundant diagnostic
kinds, changing default visibility, adding flags or changing JSON representation
requires explicit user review before implementation. Preserve candidate facts,
counts, exact edit coordinates and real errors. A clearer human presentation
must not silently remove useful information.

D2 is complete. The [generated-navigation capsule](cli-generated-navigation-alignment.md)
records source `29de40a0`, all six qualified suites, 24 direct native checks and
the local integration receipt. The installed CLI is this candidate. Doctor
presentation is next; diagnostic-kind retirement and default filtering still
await the user’s explicit answers.

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
Record exact commands, fresh artifacts, counts and failures in ignored receipts,
with concise conclusions and links in the Task. Keep all unrelated work intact.

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
sets; Doctor human/JSON deduplication. Freeze each stage's baseline, intended
behavior delta, tests and applicable contracts before editing production.
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
