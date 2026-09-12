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

## Current Checkpoint

Set 1 is qualified for local squash. Set 2 contracts and implementation boundary
are frozen; Doctor/Status renderer implementation follows the squash. Sets 2–8
remain required. User approval already covers the intended public presentation
changes; do not ask again for routine implementation details.
