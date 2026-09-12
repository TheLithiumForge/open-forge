---
open-forge:
  description: Implement approved compact and expanded command views, compact JSON, automatic colour and reusable CLI guidance in verified sequential sets
  tags: [Memory, Working, Contextual, CLI, Task, Implementation, Presentation]
---

# CLI Presentation Implementation

## Accepted Outcome

The user approved the command proposals, example style and views/JSON analysis
on 2026-09-12, with these corrections: implement compact and expanded for every
command in text and JSON; retain compact-to-expanded fallback when a renderer is
unavailable; no separate AI view; automatic fixed terminal colours with no colour
flag or user-defined palette. Written labels remain. Existing operation meaning,
diagnostic kinds, statuses, actions, effects and recovery remain intact.

The [command proposals](cli-command-output-proposals.md),
[gallery](cli-command-output-examples.md) and
[view analysis](cli-view-format-color-analysis.md) supply accepted direction.
Their placeholders are not frozen executable evidence. This capsule records
implementation decisions and supersedes their approval-pending language.
The colour-option proposal is rejected; do not add `--color` or configuration.
Automatic host capability selects colour on supported terminals; redirected
output, JSON and selected authored content stay plain. Colour is not a new
operation or a source of status information.

## Sequential Sets

1. View resolution: explicit per-format compact/expanded renderer selection and
   compact-to-expanded fallback, preserving existing output; meaningful tests.
2. Doctor/Status human output: approved wording/grouping, no repeated evidence,
   preserved distinct findings/counts, clear compact and expanded views.
3. Library human output: all five commands; restore mutation mappings/effects.
4. Extension human output: all six commands; distinct compact/expanded views.
5. Remaining read and mutation commands, grouped into bounded sets; exact content,
   Find TSV and preview diffs remain protected.
6. Compact JSON: freeze shared representation identity and per-command compact
   field membership, implement all 28 commands with full JSON default, then
   qualify the complete public boundary before exposing the new global behavior.
7. Automatic colour: fixed semantic accents, host/stream capability transport,
   no option/configuration, no user-content recolouring; qualify supported hosts.
8. Complete help/docs and reusable CLI UX/development Guidance and CLI/C# Directive
   consolidation. Refresh installed CLI and dogfood the final result.

Root works directly and sequentially. No agents or remote operations. Squash each
qualified set into local develop. Keep required source/tests/snapshots and these
records tracked; artifact outputs are disposable. Existing unrelated delivery
work at 6cd93fcf remains intact.

## Set 1 Freeze

Baseline: `8770d24a`. Branch: `codex/cli-presentation-implementation`, existing
worktree `/tmp/open-forge-cli-refactor-sequential`.

Scope: neutral Shell per-view rendering and its pipeline integration. A required
expanded renderer plus optional compact renderer defines one output format.
Selecting unavailable compact calls expanded with effective view expanded.
Selecting expanded never chooses compact. Format, result identity, operation,
status, exit and stream stay unchanged. Undefined views are rejected; failures
inside a renderer propagate and never trigger retry/fallback or another operation.

Existing combined renderers that already accept a view keep both views registered
through the existing two-delegate RendererSet construction. Explicit per-view
construction supports absence/fallback. Every command's final delivery must
provide both meaningful views; fallback is not evidence that compact is complete.

Placement: neutral behavior under Shell/Presentation/Shared/Rendering, used by
Shell/Pipeline/CliRendererSet. No command imports or universal view-model tree.
No new dependency; ordinary delegates and immutable fields are sufficient.

Evidence: focused Unit cases prove selected delegate/result identity/effective
view, unselected renderer non-execution, JSON format preservation, undefined input
and exception propagation. Existing Shell pipeline cases freeze operation and
stream behavior. This shared composition change triggers full managed/native
qualification before squash. Exact commands and receipts will be recorded here.

Applicability: local unreleased CLI; source changes are reversible in Git. The
set changes no filesystem behavior, input parser, serialized schema or content.
The supported cooperating-process and stable-workspace safety boundary remains.
Tests are under `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests`; no new test
project. Native and Integration gates cover the real composition boundary.

## Set 2 Freeze

Scope: Doctor and Status human rendering and affected Interface/Behavior text.
The rendering graph consumes the existing typed results. No inspection, mutation,
count, status, action formation, JSON projection, help grammar or default changes.
No new dependency or exceptional machinery. The local, read-only presentation
risk is hiding distinct facts or overstating candidate certainty; source changes
are reversible in Git. Preserve the cooperating-process/workspace boundary.

Doctor groups by the full typed subject within its existing six categories.
Distinct findings remain separate rows with severity, plain title, stable code
and resolution. Equal candidate lists use full typed equality of cardinality,
ordered candidates, subjects, evidence and provenance. Equal evidence/actions
use their record equality; no text-based identity or message parsing. Supporting
facts render once per group. Category/overall actions already shown below are
omitted only from those duplicated summaries; additional actions remain visible.
Line and column are inline; byte ranges remain in unchanged JSON. Remove the
512-character human truncation. Candidate inclusion is explained without ranking
or automatic selection. All finding kinds get deliberate human labels.

Status leads with the observed installation and status, then non-complete
findings, context, routes, lifecycle, Libraries, recovery and the existing Next.
Compact summarizes current navigation by count; all other paths remain visible.
Expanded shows every navigation path and existing bounded largest-source detail.
Every recovery candidate and honest unavailable/count state remains visible.

Placement remains each command's Shared/Rendering. Doctor local presentation
helpers own titles, grouping/equality, and evidence/proposal rendering. Do not
promote command semantics into Shell. Snapshot expectations use small authored
results, independent of shipped Markdown wording. Focused Unit assertions prove
no lost kinds/actions, distinct occurrences, candidate equality distinctions,
unchanged typed results/JSON, full paths, and meaningful view differences.
Affected Integration/public output assertions change only to the approved text;
existing status/stream/effect assertions remain. Qualify managed and native at
this first human-rendering archetype before the second squash.

## Set 1 Qualification

Frozen source: `f3e1729630c7f8a6aab711280c04ebd1af116c70`, clean executable
inputs. Linux x64, .NET SDK 10.0.111, Node 24.19.0, npm 11.17.0.
`npm run build:native -- --sha --offline` passed without build warnings/errors.
`npm run test:built` passed all six suites: Unit 3238; managed and native
Integration 1745 each; managed, native and managed-to-native Public 123 each.
Every selected suite discovered/executed its reported count, with zero failures
or skips. The delivery manifest reports `tested: true` and binds the source and
artifact hashes. Native CLI SHA-256:
`c8e548d57791739ddb0b465dabf055c6f114ec76e758d0feb56fb75c5f250735`.
Direct native `--version` and `--help` passed. Targeted `dotnet format
OpenForge.Cli.slnx --no-restore --verify-no-changes --severity warn --include`
on the three changed C# files passed. `git diff --check` passed.
The initial sandbox denied compiler IPC/process spawning; authorized offline
execution with local process access passed. No product failure was found.

Root reviewed the actual three-file source/test boundary: neutral dependency
direction, selected delegate only, unchanged operation/status/streams, and
exception propagation. No material finding remained. The next set's contract
edits and this plan do not affect the qualified executable inputs. Raw reports
and native binaries are disposable; this tracked receipt preserves acceptance.

## Set 2 Review Checkpoint

Doctor/Status human renderers and their contracts are implemented. The first
full managed pass completed with Unit 3249, Integration 1745 and Public 123,
zero failures/skips; build and targeted formatting were warning/error free.
The representative worktree comparison used the qualified Set 1 native CLI and
this set's managed development CLI on the same workspace: Doctor expanded fell
from 46,286,800 to 8,794,916 UTF-8 bytes; compact from 12,936,317 to 5,974,387.
Both builds/views returned exit 3 and empty stderr. This is an output-size
observation, not a managed/native execution-speed comparison. Counts reflected
1 error, 3073 warnings and 8238 information findings in the observed result.

The root review found one final readability correction pack: put compact
resolution on the finding row, omit the redundant static valid-link sentence,
show known-empty counts once, preserve unknown counts explicitly, and use
correct singular/plural count labels. The valid-link title retains validity,
not merely existence. Candidate match coordinates now attach to the match label.
A new direct check prevents unknown counts from collapsing into no findings.
These final source changes invalidate the earlier executable receipt; rebuild
and rerun focused evidence, then the complete frozen managed/native gate before
squash. No operation or JSON implementation changed. The earlier byte-size
measurements remain historical observations until remeasured on the final build.

## Set 2 Qualified Predecessor And Status Grouping Correction

Candidate `065e2427a3d4c0104a848f24903c48fc79cdf818` passed
`npm run build:native -- --sha --offline` and all six `npm run test:built`
suites: Unit 3250, managed/native Integration 1745 each, all three Public modes
123 each; zero failures/skips, no build warnings/errors. Manifest `tested: true`.
Final managed dogfood output at this candidate: Doctor compact 5,590,885 bytes /
78,679 lines, expanded 8,581,094 bytes / 164,936 lines, both exit 3 and empty
stderr. Compact Status was 7,458 bytes, exit 3, empty stderr.

R2, presentation/readability: the real Status report repeated the same generated
navigation cause once per path, then listed those paths again under Routes.
This was revealed by the Status dogfood excerpt after the earlier Doctor-focused
correction. Group findings by exact code, status and cause; keep every distinct
subject. For the four typed generated-navigation finding codes, a subject may
be referred to Routes only when its exact path is present in the non-current
navigation rows. Missing or unmatched subjects stay in the finding group.
Keep all original findings, statuses, causes, ordering and JSON untouched.
Add focused evidence for both views, matching/unmatched paths and same-JSON
preservation. This is a second bounded correction justified by new real-output
evidence, not a restarted broad review. Requalify the revised candidate before
squash; the predecessor receipt does not claim the later source was tested.

## Set 3 Freeze: Library Human Views

This set starts after Set 2's qualified squash. It implements all five Library
human views from their existing typed payloads and aligns their Interfaces.
No operation, parser, filesystem, permission decision, serializer or JSON schema
change is needed. List remains a bounded registered-link read; Inspect consumes
its existing inventory. No renderer performs a scan or reconstructs missing facts.

Promote the identical human header/status/selection/text mechanics now used by
Doctor and Status into Shell/Presentation/Shared/Rendering/CliHumanText, with
those existing consumers and the five Library commands. Header and Next helpers
consume the existing CliPresentationRequest and ICliCommandResult boundary.
Keep title wording and all command meanings local. This is neutral repeated
formatting, not a universal result model or rendering tree. Existing Doctor and
Status output must remain byte-equivalent under this promotion; their snapshots
and focused tests remain the regression boundary. It also provides one place
for the later accepted terminal accents without adding colour in this set.

Library List identifies missing/empty/unavailable records honestly, retains
workspace selection and the explicit statement that source inventory was not
scanned, then shows registered paths/IDs and link states. Expanded adds exact
expected/observed link targets. A missing count is not inferred from an empty
array. Inspect groups comparison facts beside each source/destination identity,
retains actual registration and eligibility membership, and separately retains
registered or eligible rows absent from the comparison. Identity uses the full
typed path/ID values; no comparison relation invents missing membership facts.
Expanded adds raw link targets and supporting observations. Both retain findings,
coverage, exact paths and source IDs when available.

Attach/Sync/Detach consume their shared Identity, Record, Projection, Plan,
Permissions and Application records through Library-local rendering helpers.
Attach/Sync additionally render their Source observation; Detach stays source
independent and does not gain a source scan. Show every mapping and blocker,
planned directory/link/generated-region/record effect, relevant permissions,
application/verification/recovery state and residual path. Partial or interrupted
application does not make planned paths verified effects. Library generated-region
payloads contain hashes rather than diff content: show those actual facts in
expanded output and never read files or invent a diff to fill that absence.
Next comes only from the result, once; expanded may add its actual reason.

Supporting helpers stay below Library/Shared/Rendering, grouped by observation,
plan, permissions and application only when that improves navigation. Existing
closed result types and source-generated JSON remain intact. Human labels may
explain existing coordinates; stable finding codes and direct causes remain.
Avoid a generic mutation result, string dispatch, parser or new dependency.

Evidence: use small authored presentation fixtures and reviewed snapshots;
assert required mappings/effect paths, preserved/unknown state, interrupted
application and source independence directly. Compare JSON before/after human
rendering. Retain existing status/stream/exit and mutation safety evidence.
The shared-header promotion triggers the full managed/native gate. Qualify all
five command views and the unchanged Doctor/Status neighborhood before squash.
The set is reversible source/prose work for the local unreleased CLI, with no
new trust boundary or exceptional machinery. Raw artifacts remain disposable.

## Current Checkpoint

Set 1 squash: `5bb4aefa`. Set 2 is qualified and ready for its authorized local
squash. Set 3 contracts are drafted; implementation starts after that squash.
Sets 3–8 remain required. User approval already covers these changes.


## Set 2 Final Qualification

Frozen source: `39f0ec3950aaa0caab76436c8e4727bc4f9dd9cd`. The R2 correction
passes 67 focused Status tests and formatting verification. Final reproduction:
`npm run build:native -- --sha --offline`, then `npm run test:built`, using
SDK 10.0.111, Node 24.19.0, npm 11.17.0 and Linux x64. Build completed with zero
warnings/errors. All six suites passed with zero failures, skips or pending
cases: Unit 3,252; Integration 1,745 in managed and native modes; public 123 in
managed, native and managed-against-native modes. The delivery manifest verifies
source and executable closures before/after execution and records tested=true.

Native CLI SHA-256:
`fade6dbb1cf48e2017411f66cc5bf5ae8c30eb5512a09d169aba7983fe06ced2`.
Native Integration SHA-256:
`d77ea5e15a002eccc06eb99c53089e311f058c7458151c8c3b18562287f5ba87`.
Native public runner SHA-256:
`d6d19be6f5b72ab7932547f5a9233d79281578196e6bb96ad42d42d978b71bb8`.

Direct native `status --view compact` in this worktree returns exit 3, empty
stderr and 4,303 stdout bytes. Its observed 13 changed and 16 unavailable
navigation paths appear once under Routes; grouped findings retain their distinct
codes/status/cause and reference that section. Incomplete workspace observations
remain visible. Earlier Doctor output-size evidence remains above; no operation,
kind, count, exit, default visibility or JSON changes occurred in this set.
Required evidence and snapshots are tracked; raw artifacts are disposable.
This receipt accompanies the authorized local squash; no remote effect occurred.
