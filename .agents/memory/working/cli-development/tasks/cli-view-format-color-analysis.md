---
open-forge:
  description: Analyze compact and expanded views for people and AI, JSON detail selection, and terminal colour without changing command behavior
  tags: [Memory, Working, Contextual, CLI, Task, Presentation, JSON, Design]
---

# Views, JSON Detail And Colour

## Status And Recommendation

The user endorses the natural wording and overall direction of the
[command examples](cli-command-output-examples.md), and asks whether concise AI
output, JSON detail selection and colour fit the design. This analysis uses
local `develop` `6cd93fcf`; the intervening delivery changes are preserved. No
Interface, Behavior, renderer, flag or dependency changes in this set.

Recommend two explicit views across text and, after a separate contract change,
JSON: compact for concise actionable information, expanded for explanation and
full evidence. Do not add an overlapping `ai` view. Add optional terminal colour
as an independent presentation setting. Keep diagnosis, selection, actions,
status, exit and filesystem effects independent of all three choices.

The expanded human style is endorsed. Compact membership, projected JSON and
colour semantics below are proposals, not inferred implementation approval.
The [per-command proposal](cli-command-output-proposals.md) still defines the
human detail boundaries; the [sequential plan](cli-dogfood-follow-up-plan.md)
owns implementation order.

## How Selection Works Today

Views are not an ordered fallback chain. `CliSyntaxDefinitions.View` explicitly
defaults to `CliView.Expanded`; `CliGlobalInputReader` binds the selected value.
`CliView` has exactly Compact and Expanded. The human renderer explicitly selects
its shape; a single-shape renderer may ignore view. No available-data, AI, output
length or terminal check chooses compact automatically.

`CliRendererSet` selects Human or Json separately. The [current global flags Interface](../../../crystallized/documents/cli/contracts/shared/global-flags/interface.md)
requires JSON to contain the complete result and ignore view. The
[shared result coordinates](../../../crystallized/documents/cli/contracts/shared/result-coordinates/interface.md)
define its exact envelope and status/exit guarantees. JSON source-generation
contexts currently select `WriteIndented = true`. Context/Find have their own
presentation facts, so a future JSON-view change must check their binding and
projection contracts rather than assume every command has identical metadata.

The host observes stdout redirection for help width and stdin/stderr redirection
for prompts. It does not currently pass an output colour policy to result
renderers. A redirected terminal does not imply a different result view.

Source owners are under `src/cli/core/OpenForge.Cli.Core/Shell`:
`Definitions/CliSyntaxDefinitions.cs`, `Definitions/CliPresentationDefinitions.cs`,
`Parsing/CliGlobalInputReader.cs`, `Presentation/Models/CliPresentation.cs`,
`Pipeline/CliRendererSet.cs`, and `Pipeline/CliPipelineStages.cs`. The process
boundary is `src/cli/root/OpenForge.Cli/Hosting/CliHost.cs`.

## Two Views, Two Formats

| Requested output | Recommended meaning | Current contract |
| --- | --- | --- |
| Default text, or `--view expanded` | Natural explanation, related evidence once, useful action | Already the default; revised layout needs the approved command Interface updates. |
| `--view compact` | Short, labelled facts suitable for people and AI | Already intended for this use; six mutation renderers do not yet honor the promised distinction. |
| `--json`, or `--json --view expanded` | Complete structured result, readable indentation | Current behavior. |
| `--json --view compact` | Proposed explicit compact structured projection with little formatting whitespace | New behavior. Today it produces the same complete, indented result. Requires shared and command-specific schema/contract work. |

A missing optional fact should not force a view change. Print its real unavailable
state when significant; omit only optional framing that has no useful content.
A command may have identical views for a tiny result. Keep the default stable in
pipes and terminals; the caller selects compact when useful.

Do not add a second detail flag or a third `ai` profile for the same purpose.
A named AI view does not guarantee model comprehension. Stable keys, meaningful
short labels, exact identities and explicit uncertainty are useful to both people
and models. Cryptic abbreviations, positional fields without a schema, truncated
paths and undocumented omissions work against that goal.

Compact is not a universal summary-only mode. It can omit supporting explanation
while retaining enough facts to inspect and act. A large set of distinct affected
paths or selected content can still require a large compact result.

## What Compact Must Retain

Keep operation identity, workspace/selection, actual status, mode, coverage and
limitations. Preserve required actions, exact affected subjects, every distinct
finding's identity and meaning, effect outcomes and recovery. Keep selected
content bytes and exact mutation preview effects/diffs wherever currently
required. Excluding a supporting explanation is different from hiding a finding.

| Family | Compact core | Expanded detail |
| --- | --- | --- |
| Doctor/Status | Coverage and counts, distinct actionable findings, subject/occurrence, actual severity/status, required measurements/state facts and actions | Related evidence, candidate reasons, observation sources and full supporting state detail |
| Find/Context/References/Route reads | Requested identities/content/links/order, coverage and unresolved selections; retain Find TSV and authored projections | Match/loading explanations, exact supporting observations and optional context |
| Mutations | Preview/applied mode, authority choices, every affected path and required exact edit/diff, preserved or uncertain effects, blockers and recovery | Planning comparisons, evidence and verification explanation beside each effect |
| Library/Extension lists and inspection | IDs, recorded/available state, source availability, key comparisons and required differences | Full inventory and supporting comparisons where the command actually inspects them |

Do not guess an action from a shorter message. Preserve multiple findings at one
location as distinct facts; show shared detail once when its identity is established. A compact healthy
summary may combine current navigation paths into a count when its approved
contract permits that, but a deletion preview cannot hide paths behind a count.

An illustrative compact Doctor finding section can be short without inventing
shorthand; this is an excerpt after workspace, coverage and full counts:

```text
Links
WARNING reference.target-missing .agents/directives/review.md:12:4
  Missing target: ../guidance/testing.md
  Resolution: manual decision
  Action: check the destination, then update or remove the link.
```

Expanded uses the natural explanation and available evidence from the gallery.
Both describe the same finding. Neither implies that a nearby candidate is the
correct target.

## Smaller JSON: Three Different Changes

### Formatting

Removing indentation saves output bytes without removing fields. The existing
serializer provides this through `JsonSerializerOptions.WriteIndented`; there is
no reason to strip whitespace manually or serialize, parse and serialize again
in production. [System.Text.Json documentation](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.jsonserializeroptions.writeindented?view=net-10.0)
defines this setting. Preserve source-generated serialization and Native AOT.

Two small installed-command measurements show the distinction:

| Command/result in this worktree | Current full JSON | Equivalent minified JSON | Reduction |
| --- | --- | --- | --- |
| Library List, missing record | 454 bytes | 352 bytes | 102 bytes, about 22% |
| Extension List, missing lifecycle record and six available packages | 2,804 bytes | 2,091 bytes | 713 bytes, about 25% |

These are equivalent reserializations used to estimate formatting overhead, not
an implementation benchmark or a promised token reduction. Both current JSON
views were byte-identical. Tiny examples do not predict the large Doctor report.
Whitespace removal does not fix repeated candidate arrays.

### Repeated Facts

An explicit compact representation can retain a candidate/evidence set once and
reference it from related findings. That changes the wire structure even when it
loses no information. It needs deterministic references, a rule for meaningful
identity/equality and referential-integrity tests. Do not merge similar-looking
candidates or collapse distinct diagnostic kinds. A dictionary full of opaque
IDs is not automatically easier for an AI to use; keep references local and
readable where possible.

### Detail Selection

The proposed compact JSON should retain the per-command compact core and identify
itself as a compact projection. Supporting evidence may be absent by design.
The schema must distinguish that from evidence that was not observed, a null
value, an empty collection or incomplete inspection. Full operation counts and
coverage remain truthful even when supporting detail is omitted.

Freeze a projection discriminator and field membership before implementation.
An explicit presentation/view field is one possible design; its placement and
schema version are not frozen here. Keep `result` typed per command rather than
introduce a universal object dictionary. Keep current command/status/workspace/
next semantics. Every omitted detail category must be defined by the compact
schema, and any filtered or truncated collection needs explicit accounting.
This proposal does not authorize arbitrary truncation or a findings filter.

Normal `--json` remains the complete default. Opting into compact is the caller's
explicit choice to receive the smaller representation. Selecting expanded after
an operation would require another invocation today: there is no retained result
handle. Never tell a caller to rerun a mutation merely to obtain missing detail.
Therefore compact mutation JSON must retain all essential effects and recovery;
callers needing complete evidence should select expanded on the original call.

This intentionally changes today's rule that view has no effect on JSON. It is a
reasonable product change if approved, not a renderer alignment fix. Because the
product is unreleased, update the current contract and consumers directly; no
legacy adapter or migration feature is needed. The current exact schemas and
qualification evidence still must be updated honestly.

Recommendation: keep the two-view interface as the target. Ship compact JSON only
when its per-command projection is designed and tested. Do not temporarily teach
users that compact JSON only removes spaces if the intended stable meaning is
less supporting detail. Minification is an implementation part of that slice.

## Colour

Colour is useful for scanning status and section labels. Recommend a small set
of semantic accents rather than seven unrelated hues for the seven statuses:

| Meaning | Suggested accent |
| --- | --- |
| Complete/success | Green |
| Attention, warning, incomplete inspection | Yellow; keep the distinct written label |
| Invalid, blocked, failed, error | Red; keep the distinct written label and cause |
| Interrupted | Yellow; explicitly say interrupted and preserve partial effects |
| Informational/section label | Cyan |
| Ordinary explanation, paths, IDs and copyable commands | Terminal default foreground |

This is a proposed palette, subject to review on light and dark terminal themes.
Colour the label or short outcome, not entire paragraphs. Avoid forced grey/dim
body text or fixed RGB assumptions. Colour must not be the only way to distinguish
states. Do not colourize selected authored content, JSON or literal preview diffs
in the initial slice; preserve their byte/format obligations. Existing diagnostic
severity and overall operation status remain different facts even if they share
an accent.

Proposed independent global option: `--color auto|always|never`, default auto.
This follows the established three-choice convention documented by
[CLI Colours](https://bixense.com/clicolors/); it is a proposal, not an existing
option. Auto styles only human output on a capable terminal and checks the actual
selected stream. A failure rendered to stderr must use stderr's capability,
not stdout's. Redirected output stays plain in auto mode. `always` explicitly
allows escapes in redirected human output; JSON stays unstyled in every mode.

Honor non-empty `NO_COLOR` for automatic colour, with explicit per-invocation
always/never taking precedence. The [NO_COLOR convention](https://no-color.org/)
defines both the environment preference and explicit override. Empty values do
not disable colour. In auto mode, `TERM=dumb` or unsupported/unknown terminal
capability yields plain text. Do not add CLICOLOR/FORCE_COLOR aliases in the first
slice unless an existing chosen facility requires a documented policy for them.
Plain machine runs can explicitly request `--color never` once implemented.

Redirection alone is not proof of ANSI support. .NET exposes
[stdout redirection](https://learn.microsoft.com/en-us/dotnet/api/system.console.isoutputredirected?view=net-10.0),
while Windows virtual-terminal handling has its own
[documented capability boundary](https://learn.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences).
Use existing runtime/parser/console facilities where sufficient; do not grow a
terminal detection framework or add a new UI dependency for five accents. Freeze
the supported-host handling before implementation and fall back to plain output
when automatic support is not established.

## Implementation And Contract Impact

The existing typed-result/rendering boundary remains useful. Human compact versus
expanded projection is command-local. JSON projection is also command-local;
neutral serialization settings can be shared. Colour policy belongs at the
process/presentation boundary: the host supplies environment/stream facts, Shell
resolves neutral options, and renderers style known semantic labels. Domain
operations do not read environment variables, write Console colours or inspect
terminal state.

Do not regex-colour finished English output or scatter Console calls through
operations. Use neutral styling of generated labels; compose and reset styling
without leaking it into user content or another output stream. The current
string renderer interfaces may need a small neutral style helper or added
presentation settings, not a universal terminal document framework. Do not
introduce that abstraction until the accepted colour slice closes its needs.

| Proposed work | Required alignment |
| --- | --- |
| Human compact/expanded implementation | Per-command Interfaces/Behavior where presentation is specified; approved gallery snapshots; existing semantic safeguards |
| JSON view projections | Global flags Interface/Behavior, shared result coordinates, every affected command JSON contract, projection/serializer code, help/docs and managed/native evidence |
| Colour | Global flag and precedence contract, host capability/settings transport, help/error/result styling boundaries, plain/coloured stream tests |
| Additional `ai` view | Not recommended; adds overlapping mode combinations and an undefined promise without a distinct need |

Qualification must cover full/compact semantic-core parity; no changed operation
requests or effects; explicit omitted-detail semantics; candidate reference
integrity; unknown/missing/empty distinctions; and content/diff byte preservation.
Colour tests cover both streams, redirected versus terminal output, NO_COLOR
empty/non-empty, explicit overrides, unsupported terminals, escape/reset safety,
JSON purity and plain-output readability. Do not depend only on stripping ANSI
from snapshots: assert the expected styled spans and that user content is intact.
Measure tokens only with a named tokenizer; evaluate realistic AI tasks before
claiming an understanding improvement. Byte counts alone prove neither claim.

## Sequence And Checkpoint

1. Retain the endorsed expanded style. Add and freeze exact compact examples for
   the first Doctor/Status set, with preserved mandatory facts. Update approved
   human Interfaces and implement the set sequentially.
2. If the colour proposal is accepted, implement one bounded shared colour slice
   with terminal/stream evidence, then apply it consistently to generated labels.
3. Freeze compact JSON projections, starting with Doctor where repeated evidence
   is material, then cover other command families before exposing global JSON
   view behavior. Close the shared schema and command coverage first; do not
   expose a partially implemented global rule. Full JSON remains the default.
4. Continue the existing family sequence and consolidate reusable CLI/C# guidance
   at the end. Manual parsing and optional finding/category filters stay backlog.

The latest user message endorses style but requests analysis of these added
choices. Record them as proposals; no new mode, JSON omission or colour default
is approved by implication. The analysis is complete at the design-comparison
level; projection membership, exact schema and supported-host styling must be
closed in their selected implementation stages.

Direct evidence used the installed `0.0.0-dev.sha-29de40a0...` executable. Eight
read-only invocations covered compact/expanded human/JSON for Library List and
Extension List, with exits 0 and 3 respectively and empty stderr. Human byte/line
counts were 120/2 and 214/7 for Library List; 453/9 and 741/15 for Extension List.
Both pairs of JSON views were byte-identical. Equivalent minification was checked
in memory with parsed-value equality; no experimental script or artifact remains.
This documentation-only set does not rerun production qualification suites.

Documentation checks: installed Index refreshed the task navigation; its verified
two-blank-line normalization of the delivery index was reverted. Installed
References resolved the analysis's local links and eight new authored links
from the plans and earlier analysis/gallery documents. External references were
read from their primary websites. `git diff --check` passes. The new proposals
remain Working analysis when this documentation set is squash-integrated locally;
no product-contract approval or implementation is implied by integration.
