---
open-forge:
  description: Accepted current technology-neutral resolution, graph use, measurement, topology, safety, and conformance for `route inspect`
  responsibility: Define how a conforming implementation forms the one-source route profile without selecting implementation technology
  tags: [Memory, Crystallized, CLI, Release, Command, Route, Inspect, Behavior, CurrentTruth]
---

# route inspect Behavior Contract

## Status And Authority

This is the accepted current Crystallized authority for the technology-neutral
Behavior Contract for `route inspect`. The command is implemented in the
merged native CLI; implementation and executable evidence are tracked in
[CLI Development](../../../../../../working/cli-development/_cli-development.md).

The [Interface Contract](interface.md) defines the complete public grammar,
observable profile, output, semantic result names, errors, and non-goals that
this behavior satisfies. This file defines deterministic resolution, graph use,
classification, measurement, topology, result formation, read-only safety,
presentation, and conformance without adding a public operand, flag, output
shape, status, or implementation technology.

The [Shared Result Coordinates](../../shared/result-coordinates/interface.md)
define the accepted shared structured schema and process-status mapping. The [CLI
Architecture](../../../architecture.md) defines parser and concrete serialization,
filesystem and physical-identity structure, diagnostics, source and runtime
boundaries, and test boundaries. This Behavior Contract makes no
implementation choice to change those boundaries and remains technology-neutral.

## Operation Invariants

- The operation performs one complete route-profile inspection for one resolved
  source reference. It does not select a hidden child operation.
- For the same CLI payload, workspace bytes, explicit source reference, and
  observed file inventory, including Git inventory or ignore configuration used
  by the selected scan strategy, it forms the same route facts, measurements,
  ordering,
  availability, and semantic result described by the
  [Interface Contract](interface.md#purpose).
- The operation builds at most one current in-memory route and loading graph for
  the invocation. It does not persist that graph or create inspection state.
- Reading and parsing workspace bytes to establish facts does not make authored
  content part of agent context, select a Framework route for work, activate
  `Axioms`, or change authority, as required by the public [Interface operation
  boundary](interface.md#purpose).
- The operation is read-only. It acquires no mutation authority and performs no
  persistent workspace mutation, including content, route, index, repair,
  formatting, or recovery mutation.
- It does not return authored source content, perform diagnosis, or create route,
  health, content-placement, or diagnostic recommendations.
- An applicable fact that cannot be established remains `unavailable`. A measured
  zero and a `not-applicable` fact remain distinct. The operation never replaces
  either an unavailable or not-applicable fact with zero, `none`, a guessed role,
  or a guessed relationship.

## Request Resolution

### Workspace and source reference

Request resolution first applies the shared [Global CLI Flags](../../shared/global-flags/behavior.md)
workspace rules and the shared [CLI Source References](../../shared/source-references/behavior.md)
identity rules. It uses the exact current working directory or exact
`--workspace` value, resolves relative workspace values from the process current
working directory, and keeps the selected physical and lexical boundary. It
does not search upward, substitute a Git root, or infer another workspace from
the operand. Unsafe lexical, physical, or containment identity blocks the
operation under the public boundary in
[Workspace And Subject](interface.md#workspace-and-subject).

The resolver validates the one-subject command form and shared global inputs
before domain inspection. Terminal `--help` and `--version` modes stop before
domain resolution under the shared contract. In domain mode, it resolves one
automatic ID or exact path, preserves the requested reference, and retains both
automatic ID and canonical path in the result. It does not add a qualifier
syntax, fuzzy matching, case correction, or another source identity system.

An unknown or unsupported reference forms the public invalid boundary. An
unresolved ambiguous source ID forms `blocked`. Exact-path or interactive
disambiguation may resolve only source identity. If the resulting physical
source and route are safe and complete while the automatic ID remains
non-unique, resolution records the identity observation for `completed-with-warnings`.
Exact-path resolution does not repair a structurally ambiguous route. A Loader
reference is rejected as a workspace root rather than treated as the inspected
route subject. These resolution outcomes use the public [Errors](interface.md#errors-and-boundaries)
and [Semantic Results](interface.md#semantic-results) without inventing another
status.

An unresolved source-ID collision retains every candidate path. Non-interactive
and JSON requests do not prompt and direct the caller to rerun with one listed
exact path. Resolution never chooses by kind, order, depth, or likely intent.

A human collision request may use the host-supplied native interactive session
only when its prompt-capable fact is true. Present the ordinally ordered exact
candidate paths on stderr and ask once. Accept either a one-based displayed
candidate number or one exact displayed path. A valid answer selects that exact
physical source and records interactive selection. There is no default, retry,
or inferred choice. An invalid answer or end of input retains the existing
blocked collision and exact-path guidance; caller cancellation forms
`cancelled`. Each retains the known collision facts. JSON and redirected
requests never call the session, and no prompt text is written to stdout.

### Working-path applicability

Lexically normalize the complete repeated `--for` set against the selected
workspace, including planned paths that do not exist. Reject a path that escapes
the workspace. The command does not infer paths from Git, prior invocations, or
dependencies.

Use the same route graph to collect the inspected source's declared condition
and each inherited ancestor condition. Report those condition sources and their
patterns, then evaluate each supplied path against the chain: patterns within a
condition are OR alternatives, and conditioned ancestors are ANDed against the
same path. For a conditioned source with supplied paths, the state is `matched`
when at least one path satisfies the full chain and `unmatched` when none does.
`matchingPaths` contains only paths that satisfy every condition. With an
effective condition and no supplied paths, the state is `pending` and
`matchingPaths` is empty. Invalid condition metadata has state `invalid` and
does not claim a match. A missing local condition inherits every applicable
ancestor condition. With `--for`, a source without an effective condition has
state `unconditioned`, remains compatible with the supplied paths, and has an
empty `matchingPaths` list because no condition matched. Without `--for`,
omit applicability for an unconditioned source.

This is inspection only. It does not activate the inspected route, load
nonmatching automatic or `#KeepInMind` children, or change authored content. A
valid overwrite remains adjacent to its base during explicit inspection. Keep
authored tags as metadata, but suppress effective automatic and
`#KeepInMind` reread claims for an unmatched source. Pending applicability
makes `read.atStart` unavailable with a reason and leaves other effective
behavior facts unavailable when their values depend on applicability. An
unmatched condition makes `read.atStart` false. A matched condition does not by
itself make the source part of task-start context. Keep route and topology
counts structural. Mark a context-set or size measurement unavailable only
when pending conditions prevent its value from being established.

Carry the pending-working-path cause from loading evaluation into result
formation. A conditioned automatic entry may make measurements unavailable
even when the inspected source is unconditioned. In that case, the minimal
`route-inspect.unavailable-fact` warning says `Route facts could not be
measured: No working paths were supplied, so effective loading is pending.`
Its next action uses the inspected source's exact path with `--for <path>` and
the reason `Supply working paths with --for to evaluate pending loading
conditions.` Keep `incomplete` and exit 3. Keep generic wording for other
unavailable facts and preserve higher-priority source-resolution actions.

### Matching-file inspection

Only `--matching-files` requests candidate enumeration. Without it, perform no
enumeration and launch no Git process. Help and version stop before enumeration.
Preserve an existing invalid or blocked source-selection result without scanning.

An invalid or unresolved chain produces `condition-unavailable` before
enumeration, not zero matches. Validate the established condition chain before
candidate enumeration. Remove recognized match-all conditions from the scanner's
evaluation chain. They are redundant there. Authored conditions and ordinary
`--for` applicability are unchanged. If no restrictive condition remains,
return the complete all-files answer without invoking the host enumerator. Do
not start a Git process or walk candidate files. No Git probe or Git inventory
occurs.

**Match-all rule.** Examine `ApplyToPattern.Alternatives`, falling back to
`Segments` when alternatives are empty. An alternative is match-all when every
segment is nonempty and contains only `*`, at least one segment is exactly
`**`, and at most one segment is not exactly `**`. A pattern is match-all if any
alternative is. A condition is match-all if any of its patterns is. This covers
`**`, `**/*`, `**/**`, `*/**`, and brace forms that expand to them. It excludes
`*`, `*/*`, and `**/*/*`. `*` has no dotfile exception in this dialect, so
`**/*` matches every file.

Otherwise scan, evaluating only the remaining restrictive conditions.

The host chooses the strategy once per request. It creates one 30-second scan
deadline before the Git probe and never restarts it. It probes whether the
exact selected workspace is inside a Git work tree. A confirmed work tree
uses Git's existing tracked and untracked inventory, excluding ignored
untracked files. Tracked files stay eligible even when an ignore pattern
matches them. The probe reads no Git messages, so it does not depend on
locale. Skip every entry named `.git`.

The fallback triggers are:

| Situation | Result |
| --------- | ------ |
| Git cannot start | Ignore-aware workspace walk |
| Workspace is not a work tree | Ignore-aware workspace walk |
| Repository refusal | Ignore-aware workspace walk |
| Inventory fails after a confirmed probe, including warnings or malformed output | Discard Git output, then use the ignore-aware workspace walk |
| Timeout or caller cancellation | Keep the existing unavailable or cancelled result. Do not fall back. |

A timeout or cancellation never restarts the deadline. Unexpected
implementation failures remain `scan-failed`. The host preserves process
cleanup and does not apply trust overrides.

The ignore-aware workspace walk reads regular `.gitignore` files when entering
eligible directories, before examining children. The host carries an
immutable inherited rule set with each queued directory and never enters an
excluded directory to discover more rules. Reading `.gitignore` contents is
allowed for this purpose. Operations owns pure line translation and
evaluation beside the scanner. Each typed rule stores its base directory
separately. Operations evaluates an entry's path relative to that base and
its directory flag through the Framework matcher. It prunes a directory only
after the final inclusion decision. Directory names are not parsed as glob
patterns. The Framework matcher itself is unchanged.

The ten translation rules below translate the
[Git gitignore specification](https://git-scm.com/docs/gitignore) onto the
existing Framework matcher. `Δ` marks a documented difference from Git.

1. Read regular `.gitignore` files when entering eligible directories, before
   examining children. Never enter an excluded directory to discover more
   rules. **Δ:** Read nothing above the workspace. Do not use global excludes
   or `.git/info/exclude`. Tracked files receive no exemption.
2. Skip empty lines and initial `#` comments. Preserve leading spaces. Remove
   only unescaped trailing spaces. An initial `\#` or `\!` is literal.
3. Start included. An unescaped initial `!` means inclusion. Apply ancestor
   rules before local rules while preserving line order. The last matching
   rule decides each entry.
4. A leading slash or an internal slash anchors the pattern to its
   `.gitignore` directory. Remove the leading slash before matching.
5. Otherwise prefix `**/` after removing a directory-only trailing slash.
   Thus `*.log` becomes `**/*.log`.
6. A trailing slash requires a directory. Evaluate all applicable rules before
   pruning an excluded directory. Do not generate `p/**` for every rule.
   Descendants cannot reappear beneath a pruned parent.
7. Use the Framework matcher for `*`, `?`, ordinary bracket lists and ranges,
   and negated classes. **Δ:** Character matching remains Open Forge's
   case-sensitive semantics, regardless of Git configuration.
8. Preserve recursive `**` segments, but translate terminal `/**` to
   `/**/*`. This prevents `docs/**` from excluding `docs` itself and blocking
   `!docs/keep.md`.
9. Treat commas literally through `ApplyToPatternMatcher.Parse`, never the
   expression parser. Encode literal braces as `[{]` and `[}]`. Outside
   brackets, decode escaped characters into literals, quoting matcher
   metacharacters with singleton classes. Supported escapes include escaped
   spaces and wildcard characters. Braces are expressible, so they do not
   need to be skipped.
10. **Δ:** Skip and count unsupported lines: extended POSIX bracket
    constructs, escapes inside brackets, escaped slash or literal backslash,
    dangling escapes, and patterns rejected by the shared parser. Do not
    silently reinterpret them.

Count only unsupported lines encountered during the walk. When the count is
nonzero, reuse `note` for `Skipped unsupported .gitignore lines: N.`. At zero
matches, append that sentence after the existing zero-match note with one
space. Do not include paths or raw patterns for skipped lines. Skips do not
raise status.

A successful fallback is `complete: true` with `reason: null`, the exact
count, and the existing path cap. An unreadable or undecodable required
`.gitignore` discards partial candidates and returns `files-unavailable`.
Otherwise-complete inspection remains `completed` with exit `0`.

The host supplies a command-owned enumeration result. Inspect owns eligibility
and passes each eligible candidate separately to `SourceApplicabilityEvaluator`.
Reuse the remaining effective chain from the route graph. Every remaining
ancestor and local condition must match that same path, with OR alternatives
within each condition. Different files cannot jointly satisfy different
ancestors.
Overwrite references use the base source's established chain. Detached sources
use their established local chain. A complete empty inventory is zero current
matches, not the evaluator's working-paths-unknown state.

Enumeration never supplies `WorkingPaths`, changes `--for` applicability, or
changes loading calculations. A complete scan can therefore accompany an
`incomplete` report whose loading facts still need explicit working paths.

Keep the selected workspace boundary even inside a larger repository. Return
files only. Exclude deleted tracked paths, symbolic links, directory links,
other reparse points, Git administrative paths, and submodule contents. Check
physical containment using the existing capability. Read candidate metadata
and required `.gitignore` contents, not other candidate file contents. The
host owns process launch, the walk, cancellation, and child-process cleanup
under the single 30-second scan deadline. The accepted adapter
and hardening rules are in the [CLI Dependency Policy](../../../../../decisions/cli-dependency-policy.md#optional-git-inventory).

Normalize eligible matching paths to workspace-relative `/` paths, deduplicate
and sort them ordinally, and count every match before retaining the first 100.
Operations owns eligibility, matching, count, cap, completeness, and status.
Completeness covers the observed scan, not an atomic snapshot. A complete count
above 100 sets `truncated` without making the scan incomplete.

On scan failure, discard all partial candidates. Return the requested block
with `complete: false`, `count: null`, `paths: []`, and `truncated: false`,
retaining an established scope. The [Interface Contract](interface.md#matching-files-data)
defines all eight fields, finite reasons, and exact limitation sentences.
Safe unavailable facts select `incomplete`, containment violations select
`blocked`, cancellation selects `cancelled`, and unexpected internal failure
selects `failed`. A complete scan, including zero, does not itself raise status.

Use `route-inspect.unavailable-fact` with its existing finding wording. The
matching-files block carries the precise limitation. When scanning is the only
unavailable fact, select `open-forge route inspect --help` and the reason
`Use --for <path> to inspect supplied working paths without scanning.` Keep
higher-priority source-resolution actions.

Invalid source cardinality, invalid working paths, and workspace binding
failures retain a requested matching-files block with `source-unavailable` and
no established scope. These paths do not enumerate files. They remain
`invalid-input` with exit 4, and omit the block when it was not requested.

Candidate-metadata failures and required `.gitignore` read or decode failures
share `files-unavailable`. The exact limitation sentence covers both causes:
`Required file information or .gitignore content could not be read or decoded.`

### One route and loading graph

After workspace and source-reference validation, the operation constructs at
most one current in-memory graph using the same route resolver and loading
rules as `context`. It does not implement a second interpretation of startup,
selection, `#LoadNow`, `#KeepInMind`, or overwrite behavior.

The graph contains the route and loading relationships needed to establish the
selected logical source's identity, exposing parent, route chain, direct and
descendant topology, loading classifications, scope-local loading, inherited
`Axioms` provenance, and base/overwrite layers. Ordinary Markdown links are
not graph edges for this operation. The graph exists only for the current
invocation and is never persisted as workspace truth.

### Logical source resolution

When the shared source-reference rules identify an overwrite path, resolution
normalizes it to the base logical source and places the valid overwrite layer
immediately after its base. The overwrite inherits the base route, reading
behavior, and scope; it is not a second route node. An orphan or ambiguous pair
cannot be silently converted into an independent source. The public identity
and result boundary are [Identity](interface.md#identity) and
[Workspace And Subject](interface.md#workspace-and-subject).

The resolver distinguishes routed entrypoints, routed leaves, routed native
sources, accepted compatibility entrypoints, valid overwrite pairs, detached
entrypoints, known supported sources with no route, and unresolved or ambiguous
identity according to the current graph. A detached entrypoint can supply local
identity and topology, but no Loader-rooted task-start, parent-triggered, or
later-continuity reading is claimed. A known absence of route produces the
complete `not routed` identity only when that absence is mechanically
established; route-dependent facts remain not-applicable rather than zero.

An orphan or ambiguous overwrite pair, an ambiguous route, unsafe identity, or
containment failure blocks the safe source boundary. A valid pair is one logical
source with base-first layers and does not itself affect status.

## Current Facts And Coverage

The operation inspects only the current workspace bytes and recognized source
relationships needed to answer the route-profile questions in the [Interface
Purpose](interface.md#purpose). It establishes each applicable fact before
including it in the typed result. It does not turn the inspection into a
workspace inventory, content projection, link traversal, validation report, or
diagnosis.

### Source-state classification

The classifier applies the finite source-state table in the [Interface
Contract](interface.md#source-state-classification). It first establishes safe
physical identity and route meaning, then records the source state and each
applicable fact's availability. A routed entrypoint, routed leaf, routed native
source, accepted compatibility entrypoint, valid overwrite pair, detached
entrypoint, or known supported unrouted source can therefore be complete when
its applicable facts are complete. A safe non-unique automatic ID becomes
`completed-with-warnings` only after exact-path or interactive resolution has selected source
identity without leaving route meaning ambiguous.

Unreadable required sources, incomplete route chains, and unmeasurable
applicable facts remain `incomplete`. Orphan or ambiguous overwrite pairs,
ambiguous routes, unsafe identity, and containment failures remain `blocked`.
Zero or several operands, the Loader, and unknown, missing, or unsupported
sources remain `invalid-input`. The classifier does not reinterpret a compatibility
filename, detached route, unrouted source, overwrite customization, route
depth, added context, or size as a health condition.

Pending applicability remains a reported state. Pending conditions or invalid
metadata make the result `incomplete` only when they leave a requested reading or
measurement fact unknown under the existing completeness rules. A definitive
nonmatch sets `read.atStart` to false and does not by itself make an otherwise
structurally valid profile incomplete.

### Reading classification

#### Task start or resume

The classifier resolves the same startup-required context as `context` with no
explicit operands and the same normalized `--for` set when supplied. It tests
membership of the inspected logical source in that filtered set independently
of the inspection operand. Inspecting the source cannot add it to the set being
measured. The public result and its prospective meaning are [Task Start Or
Resume](interface.md#task-start-or-resume).

When the source's effective `applyTo` conditions are pending because no working
paths were supplied, or invalid metadata prevents evaluation, `read.atStart` is
null through the existing unavailable fact, with a reason. If supplied paths do
not match the complete chain, `read.atStart` is false. When paths match or no
condition applies, determine membership from the actual filtered startup
context. A condition match or an unconditioned state alone does not imply
task-start membership. Pending applicability also leaves an automatic or later
reread fact unavailable when its value depends on whether the source applies;
an unmatched source has no effective automatic or later `#KeepInMind` reread
claim, though the authored tag remains metadata.

For a detached entrypoint or a known supported unrouted source, no Loader-rooted
chain establishes task-start membership. The classifier records that fact as
`not-applicable`, not as a negative membership result.

#### Automatic reading trigger

The classifier evaluates the current Framework loading rules against the graph's
exposing relationships and tags. It distinguishes parent-triggered
`#LoadNow`, explicit selection for an on-demand source, routed-file
`#KeepInMind`, entrypoint `#KeepInMind`, and overwrite inheritance. For an
entrypoint or ordinary `#KeepInMind` file it retains the exposing parent and
later review events while that scope remains active. Neither tag activates an
otherwise unselected ancestor or scope. For `#LoadNow`, it retains the actual
exposing parent. For an overwrite it retains the immediate-after-base
relationship.

The resulting typed reason is paired with the actual parent or event needed by
the human renderer. The renderer receives ordinary event wording rather than
internal labels. A matching `applyTo` condition adds no reading reason, because
a condition only filters. It removes loading-tag reasons when no supplied path
matches. The route's applicability facts report the match itself. These mechanics satisfy the public [Automatic Reading
Trigger](interface.md#automatic-reading-trigger) boundary.

A detached entrypoint has no Loader-rooted automatic trigger. Its local
`#LoadNow` descendants can still be measured after local selection. A known
supported unrouted source has no route-triggered automatic reading, so the
route-dependent value is `not-applicable`.

#### Later reads

The classifier applies the Framework's continuity occasions to determine later
read membership: context restoration, handoff, closeout, and another transition
when standing follow-up work may have changed. It reports the applicable
occasions prospectively. It does not observe or claim reads on every message,
prompt, model request, or tool call. See [Later Reads](interface.md#later-reads).

Detached and known supported unrouted sources have `not-applicable`
Loader-rooted later-read membership.

### Measurement formation

All measurement sets use unique physical source layers and the shared record of
exact Unicode scalar-value counts, exact UTF-8 byte lengths, and aggregate
estimated tokens. The estimate is computed from the aggregate character count as
`ceiling(characters / 4)`, not by summing rounded per-file displays. IEC units
and the omission of human character counts are presentation decisions owned by
the [Context Cost](interface.md#context-cost) contract; the underlying exact
values remain in the typed result.

#### Own source

The own-source set contains the inspected base and its valid adjacent overwrite
layer, in base-then-overwrite order. Measurement reads complete physical file
bytes, including frontmatter and generated regions inside an entrypoint. It
does not measure a rendered identity block or other CLI framing.

#### Selected closure and additions

The resolver calculates the selected route closure with the same rules as
`context`, the same supplied `--for` set, and without ordinary link expansion.
It retains missing ancestor entrypoints needed for route establishment, the
target and overwrite, visible `#LoadNow` descendants, and applicable scope-local
loading. The selection-addition set is the ordered selected closure minus the
current filtered task-start set. The task-start overlap is retained as its own
measurement relationship, so an already present source is not counted as an
addition and is not described as having caused startup context.

Route-chain, parent, child, and descendant counts remain structural facts.
Pending conditions make a context-set or size measurement unavailable only
when they prevent that measurement from being established. Preserve other
measurable values and attach an explicit reason to each unavailable fact.

If the difference is empty, the typed result records the empty addition and the
human renderer uses the exact no-addition meaning in
[Added By Selection](interface.md#added-by-selection). No historical invocation
or caller-retained context is consulted.

The empty difference is a measured zero and selects `completed` when all other
applicable facts are complete. A detached entrypoint has no Loader-rooted
startup comparison, so that comparison is `not-applicable`; a known supported
unrouted source has route-based selection addition `not-applicable`. Own-source
bytes remain measurable for both when their physical layers are readable.

#### `#LoadNow` descendants

For an inspected entrypoint, the operation traverses visible `#LoadNow`
relationships after that entrypoint is read and forms the unique descendant set.
It removes the inspected source and ancestor chain from this measure and places
each valid overwrite immediately after its base. It does not include ordinary
on-demand descendants merely because they are structural descendants. A
source exposed only through `#KeepInMind` is not included in this narrow
`#LoadNow` descendant measure. For an ordinary routed leaf, the
measure is not applicable, not zero.

An entrypoint with no visible `#LoadNow` descendants has a measured zero. A
detached entrypoint uses its safe local `#LoadNow` topology when applicable; a
known supported unrouted source has no route descendant measure and therefore
records `not-applicable`.

#### Availability

Readable generated `Entries` in an ordinary entrypoint remain measurable when optional description or tags are absent. This includes intermediate entrypoints created by nested Route Create. Missing optional metadata alone does not make those Entries unavailable. Malformed metadata, unreadable bodies, unavailable Entries and required native metadata retain their existing strict boundaries. Independently unavailable selected-source reading facts remain unavailable.

The operation carries zero, unavailable, and not-applicable measurements as
different states. A readable zero is measured as zero. A safely identified but
unreadable required layer makes the affected measurement unavailable and selects
`incomplete` rather than producing a partial trusted count. An unsafe identity or
containment boundary selects `blocked`; it is never downgraded to a measurement
availability condition. An applicable unmeasurable safe fact remains visibly
unavailable and selects `incomplete`. Human minimal-detail output keeps zero and
not-applicable distinct, and structured output retains all three states.

### Route topology

Topology is derived from current filesystem structure and recognized source
contracts. The resolver establishes the root route, ordered route chain, direct
exposing parent, source-ID route depth, and, for an entrypoint, direct routed
file and child-entrypoint counts plus descendant routed file and entrypoint
counts. Route depth counts source-ID route segments from the root through the
subject, with a root entrypoint at depth `1`. A detached entrypoint can provide
safe local topology without a Loader-rooted route. A known supported unrouted
source has route-dependent topology `not-applicable`.

The operation does not use current generated `Entries` as an independent
inventory and does not compare generated navigation for drift. It does not
diagnose drift. `index` owns generated navigation and `doctor` owns structural
diagnosis, as stated in [Route Structure](interface.md#route-structure).

For an ordinary routed leaf, child and descendant counts are not applicable. An
entrypoint with no children has numeric zero for its direct and descendant
counts. A detached entrypoint reports safe local counts, while a known
supported unrouted source reports route-dependent counts as not-applicable. The
resolver does not manufacture a scope count from folders, route segments,
entrypoints, or tags. A named scope role is retained only when mechanically
established in structured provenance; an unknown role remains unknown. The
scope boundary is [Why There Is No Scope Count](interface.md#why-there-is-no-scope-count).

### Inherited rules and customization

The provenance collector walks the ordered Loader and ancestor-entrypoint chain
that establishes the inspected route and records the sources contributing
inherited `Axioms`. It separately checks whether the inspected entrypoint has
substantive local `Axioms`. An inherited sentinel, an empty section, or a
missing optional local section adds no local rule. An ordinary `Axioms` heading
in a routed leaf is not activated as a Framework rule, because only the Loader
and recognized entrypoints define active `Axioms`.

The collector records declared and inherited `applyTo` conditions, the
per-path matching state when `--for` is supplied, and the valid base/overwrite
relationship, but does not emit rule bodies. Human output lists source IDs, and `context` with exact section
projection remains the operation for reading inherited rules. Extension
ownership and managed-file state are not added to this route-profile result;
they belong to lifecycle behavior outside this operation. See [Rules And
Customization](interface.md#rules-and-customization).

## Selection And Result Formation

The operation forms one typed result after request resolution, graph inspection,
classification, measurement, topology, inheritance provenance, applicability,
and availability have been established. The result keeps the requested reference, resolved
identity, physical layers, reading facts and reasons, each applicable
measurement, route structure, Axioms provenance, overwrite relationship,
observations, availability conditions, semantic status, and useful next
operations only when one is required. It retains every independently available
fact without presenting an unavailable fact as a value.

For an unresolved source-ID collision, the result retains every candidate path
and the exact-path next operation required by the shared source-reference
contract. It does not choose a candidate or omit the collision from JSON.

Selected physical layers and route facts are deduplicated by their established
logical and physical identities. Base precedes overwrite. Route chain and
topology use the current route relationships rather than generated-line order.
Unchanged input produces deterministic ordering, and the operation does not use
filesystem enumeration timing or ordinary link order as an alternate public
ordering rule.

The result selector uses only the public conditions in
[Semantic Results](interface.md#semantic-results). A routed source, accepted
compatibility entrypoint, valid overwrite pair, safely detached entrypoint, or
known supported unrouted source is `completed` when every applicable fact is
available. A safe, complete source and route with a non-unique automatic ID is
`completed-with-warnings` only when exact-path or interactive resolution selected source
identity. An unreadable required source, incomplete route chain, or unmeasurable
applicable fact is `incomplete`. An orphan or ambiguous overwrite, ambiguous
route, unsafe identity, or containment failure is `blocked`. Zero or several
operands, the Loader, and unknown, missing, or unsupported sources are
`invalid-input`. Unexpected failure is `failed`, and cancellation before completion
is `cancelled`.

For ordinary conditions, the selector applies `blocked`, `incomplete`,
`completed-with-warnings`, then `completed` precedence. Invalid input stops before operation
work, while failure and interruption retain their event meanings. Size,
customization, compatibility filenames, detached routes, known unrouted
sources, added context, route depth, and context size do not create `completed-with-warnings`
or a recommendation.

The operation does not compare generated navigation, create diagnosis or
recommendations, or follow ordinary links while forming this result. It reports
only directly observed route facts and the availability conditions required by
the public route-profile questions.

## Effects

`route inspect` is a read operation. It may open and parse the workspace bytes
needed to establish the current graph, route facts, and exact measurements, but
it performs no persistent mutation. It does not write source content,
frontmatter, generated `Entries`, indexes, caches, receipts, recovery files, or
other inspection state. It does not create a mutation plan, invoke a repair,
activate a selected Framework route, or make the inspected content part of agent
context merely because it was mechanically read.

Read-only execution stops after the typed result is formed. It does not create
an empty mutation plan or acquire write-policy authority. This satisfies the
public [Non-Goals](interface.md) and [Purpose](interface.md#purpose)
boundaries.

## Safety And Recovery

Resolution keeps all consumed local paths inside the exact selected workspace
and applies the shared source-reference and physical-containment boundary.
Unsafe lexical or physical identity, containment escape, or an unresolved safe
source identity blocks rather than falling back to a different path or source.
Interactive disambiguation can resolve only source identity; it does not grant
write, overwrite, delete, force, or ownership authority.

The operation fails closed for orphan or ambiguous overwrite pairs, ambiguous
route chains, unsafe identity, containment failure, unreadable required sources,
and unmeasurable required facts. Orphan or ambiguous overwrite and route
identity conditions are `blocked`; unreadable or unmeasurable safe facts are
`incomplete`. It does not substitute zero, `none`, a guessed role, a guessed
route, or a guessed relationship. A cancellation or unexpected failure cannot
leave a persistent inspection effect, so there is no mutation rollback or
residual recovery state to manufacture. The public error and status boundaries
remain in [Errors](interface.md#errors-and-boundaries) and [Semantic Results](interface.md#semantic-results).

## Presentation Relationship

Human output and `--format json` consume one typed result. Renderers do not rerun graph
construction, source resolution, classification, measurement, topology, or
verification, and they do not reinterpret the semantic result.

The renderer applies the shared `--detail` and `--detail debug` presentation rules
after inspection. Both views put status and meaningful observations/conditions
before the profile, with full subject/path identities. full-detail adds the distinct
explanations, Axioms and measurements described by the
[Human Output](interface.md#human-output) contract, without repeating the same
measurement or reading explanation. Human Next displays the action already formed
by the operation; full-detail adds its reason. Both human views retain workspace identity, selection method, source
ID, and canonical path. minimal-detail output also retains source and route state,
route chain, applicable topology, reading behavior, own-source,
selection-addition, and `#LoadNow` descendant measurements, overwrite state,
status, completeness, safety, and at most one required `Next:` line. It omits
inherited-`Axioms` detail and optional explanation while keeping measured zero,
`unavailable`, and `not-applicable` distinct. JSON retains the complete typed
facts regardless of human view, including collision candidate paths,
observations, and availability conditions. `--format json` does not prompt or rerun
work.

Primary human `completed`, `completed-with-warnings`, and `incomplete` results go to stdout.
Primary human `invalid-input`, `blocked`, `failed`, and `cancelled` results go to
stderr. `--format json` writes one complete structured result to stdout for every
semantic status. Separate bounded diagnostics go to stderr, and human text is
never mixed into JSON stdout. The renderer emits a next operation only when the
Interface rules require one. It never emits route mutation proposals, health or
content-placement advice, or diagnostic recommendations.

When the Interface permits a direct safe correction, the renderer names only
the observed input or safety boundary. It does not turn that correction into a
route mutation proposal.

The requested matching-files block keeps all eight fields and up to 100 paths
at every detail level. Minimal retains the count, scope, completeness,
truncation, and limitation. Standard adds existing applicability explanations,
full adds existing provenance and evidence, and debug keeps full's primary
result with bounded stderr diagnostics. `--detail-filter` does not hide the
block or change its count. Presentation consumes the command-owned result
without enumerating again or importing Framework facts.

## Conformance Evidence

Implementation evidence must cover:

- No candidate enumeration or Git process without `--matching-files`, including
  help, version, and existing invalid or blocked source selection.
- Actual binding paths for missing and multiple sources, invalid working paths,
  and workspace failures retain a requested unavailable matching-files block.
  Without the flag, they omit it. Neither case invokes the operation.
- Git inventory, ignored untracked and tracked files, deleted paths, nested
  workspaces, linked worktrees, the ignore-aware walk, all four ordinary Git
  fallback triggers, discarded Git output after warnings or malformed output,
  and no fallback after timeout or cancellation.
- Condition validation before enumeration, match-all recognition and removal,
  all-files answers for unconditioned and match-all chains, restrictive
  per-file full-chain matching, OR alternatives, overwrite and detached chains,
  invalid ancestry, and empty inventories without changing `--for` applicability
  or loading.
- Ordinal deduplication and ordering, complete counts, the 100-path cap, all
  detail levels and filters, exact zero-match text, and partial-result discard.
- Link, reparse-point, Git-administration, submodule, and containment exclusions,
  unreadable metadata, required `.gitignore` reads, deadline, cancellation,
  child cleanup, and no other candidate content reads or raw Git diagnostics in
  normal output.
- The ten `.gitignore` translation rules, immutable inherited rule sets,
  per-rule base directories, directory pruning after final inclusion, the
  Framework matcher's case-sensitive behavior, skipped-line counts, and
  `files-unavailable` for unreadable or undecodable required `.gitignore` files.
- Requested schema-3 data, every finite reason, status and exit behavior, and
  next-action priority for scan-only unavailability.

- Exact current-directory and `--workspace` selection without workspace
  discovery, as specified by [Workspace And Subject](interface.md#workspace-and-subject).
- IDs, exact paths, quoting, collisions, and source-identity disambiguation from
  the shared [CLI Source References](../../shared/source-references/behavior.md) contract.
- Workspace identity, selection method, source ID, and canonical path in both
  human views and structured output.
- Routed entrypoints, routed leaves, native routed sources, canonical and each
  compatibility entrypoint filename, detached entrypoints, known unrouted
  sources, and unsupported source kinds, as specified by [Identity](interface.md#identity).
- Base, overwrite-path, valid-pair, orphan, ambiguous, and inherited overwrite
  selection, including base-first physical-layer order and blocked orphan or
  ambiguous boundaries.
- Safe non-unique automatic IDs selected by exact path and interactive choice,
  including the interactive exact-path next operation and the no-action
  exact-path observation.
- Host-supplied prompt capability, ordinal candidate presentation on stderr,
  one-based candidate-number and exact displayed-path responses, blocked invalid
  response/end of input, cancellation, and proof that JSON and
  redirected requests never call the interactive session or write prompt text to
  stdout.
- Unresolved non-interactive ID collisions that retain every candidate path and
  require rerunning with one listed exact path.
- Task-start membership independent of the inspection operand.
- Minimal warnings name pending working paths and offer `--for` for both a
  conditioned inspected source and an unconditioned source affected by another
  startup entry. Supplying matching paths resolves that cause. Other
  unavailable facts retain generic wording.
- The same normalized `--for` set for task-start and selected loading; pending,
  matched, and unmatched applicability; `read.atStart` unavailable with an
  explicit reason while pending, false when the full chain is unmatched, and
  otherwise determined by filtered startup membership. Preserve structural
  route counts and mark only affected context-set or size measurements
  unavailable.
- On-demand, parent-triggered `#LoadNow`, visible and selected `#KeepInMind`
  entrypoints, parent-exposed routed `#KeepInMind` files, and overwrite
  inheritance.
- Plain human explanations for task-start, parent-triggered, selected, and
  later reads, including another transition that may affect standing follow-up
  work, without `target-sensitive` or `continuity boundary` labels.
- Own-source, selected-closure, task-start-overlap, additions-beyond-startup,
  and `#LoadNow` descendant set definitions and deduplication.
- Proof that the `#LoadNow` descendant measure excludes descendants reached only through
  `#KeepInMind` and is labelled narrowly enough not to hide that boundary.
- Physical file, character, UTF-8 byte, and estimated-token measurements,
  including zero, unavailable, not-applicable, and overwrite-layer cases.
- Proof that estimated tokens are recomputed from aggregate characters and do
  not sum rounded per-file displays.
- Route root, chain, depth, parent, direct child, and descendant facts for root,
  nested, leaf, sparse, detached, and ambiguous structures.
- Proof that the command does not trust generated lines as an independent route
  inventory or report generated drift as diagnosis.
- No generic scope count and no inferred scope role.
- Inherited substantive `Axioms` provenance, local `Axioms`, inherited
  sentinel, empty local sections, and ordinary leaf headings.
- Measured zero, unavailable, and not-applicable distinctions for empty
  selection additions, empty `#LoadNow` descendant sets, ordinary leaves,
  detached entrypoints, and known unrouted sources.
- minimal-detail retention of the required identity, route, reading, measurement,
  topology, overwrite, status, completeness, safety, and `Next:` facts, with
  inherited-Axiom detail omitted.
- Structured and human rendering from one typed result.
- Observations and availability conditions without a diagnosis-like result
  collection or recommendations.
- Primary human stdout/stderr assignment, one complete JSON result on stdout for
  every status, bounded diagnostics on stderr, and no human text in JSON stdout.
- Completed, completed-with-warnings, incomplete, invalid-input, blocked, failed,
  and cancelled
  outcomes.
- Evidence that compatibility, detached and unrouted state, valid overwrite
  customization, route depth, added context, and size do not create
  `completed-with-warnings` results or recommendations.
- One graph construction, no content rendering, no link following, and no
  persistent inspection state.

Direct tests should prove reading classification, set calculation, measurement,
topology, inheritance provenance, availability, and semantic results.
Gate 5 executable proof should use real temporary rooted, detached, ambiguous,
compatibility, overwrite, Git-independent, and malformed route structures. It
should cover parsing, concise human output, structured output, exit behavior, and
packaged execution, including the accepted AOT boundary.

The Route Inspect Interface defines its exact command-local structured result.
The [Shared Result Coordinates](../../shared/result-coordinates/interface.md)
define the shared envelope, compatibility coordinates, and process-status
mapping. The [CLI Architecture](../../../architecture.md) defines parser and
filesystem structure, diagnostics and redaction boundaries, and source
boundaries. The source-stated exact heading-name comparison
belongs to the Context contract, not this route-profile operation. No
command-local Technical Design is needed for `route inspect`.

## Related Current Sources

- [Route Inspect Interface Contract](interface.md)
- [route inspect Command Contract Set](_inspect.md)
- [CLI Command Contract Set — Behavior Contract](../../../command-contract-set.md#behavior-contract)
- [Global CLI Flags Behavior Contract](../../shared/global-flags/behavior.md)
- [CLI Source References Behavior Contract](../../shared/source-references/behavior.md)
- [Context Behavior Contract](../../context/behavior.md)
- [Status Behavior Contract](../../status/behavior.md)
- [Doctor Behavior Contract](../../doctor/behavior.md)
- [Route Init Behavior Contract](../init/behavior.md)
- [Route Create Behavior Contract](../create/behavior.md)
- [Route Update Behavior Contract](../update/behavior.md)
- [Find Behavior Contract](../../find/behavior.md)
- [Index Behavior Contract](../../index-candidate/behavior.md)
- [CLI Architecture](../../../architecture.md)
- [Historical CLI Decision Agenda](../../../../../../archived/cli-release/decision-agenda-2026-08-21.md)
- [Shared CLI Operation Contract](../../../shared-operation-contract.md)
- [Routing Model](../../../../framework/routing/model.md)
- [Loading And Refreshing Context](../../../../framework/routing/loading.md)
- [Route Scope And Inheritance](../../../../framework/routing/scope.md)
- [Routing Paths And Identity](../../../../framework/routing/paths.md)
- [Overwrite Customization](../../../../framework/routing/overwrites.md)
- [Routed Markdown Representation](../../../../framework/markdown/routes.md)
- [Markdown Compatibility Boundary](../../../../framework/markdown/compatibility.md)
- [Canonical Markdown Syntax](../../../../framework/markdown/syntax.md)
- [Core Primitive Model](../../../../framework/primitives/model.md)
- [Open Forge Framework Architecture](../../../../framework/architecture.md)
- [Accepted State And Synchronization](../../../../framework/truth.md)
- [Open Forge Principles](../../../../principles.md)
