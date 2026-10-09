---
open-forge:
  description: Accepted current shared operation contract for the Open Forge CLI
  responsibility: Define cross-command input, modifier, execution, result, safety, and locality boundaries without choosing implementation
  tags: [Memory, Crystallized, CLI, Release, Document, Evergreen, CurrentTruth, Contract, Operation, Shared, Interface, Behavior, Determinism, Output, Safety, Locality]
---

# Shared CLI Operation Contract

## Status And Authority

This is the accepted current `#Evergreen` Crystallized Document/Contract for the
shared operation shape of the replacement Open Forge CLI. It is the authoritative
current source for the cross-command conventions stated here: command and group
shape, input roles, modifier composition, typed operation flow, semantic
results, output streams, deterministic no-ops, recovery boundaries, and source
locality.

The [Command Contract Set](command-contract-set.md) defines the roles, topology,
and authority boundaries for those local contracts. The command-local Interface
and Behavior Contracts remain authoritative for each command's exact syntax,
subjects, finite conditions, effects, and result facts. The shared [Result
Coordinates](contracts/shared/result-coordinates/_result-coordinates.md) define
the exact public envelope, source locations, statuses, exits, streams, and
compatibility used by those results.

This Contract does not replace or duplicate those definitions. It is a
Crystallized Document/Contract, not a Pattern, Directive, Architecture, or
implementation design. The CLI is available as a public beta; [Distribution](distribution.md) defines publication and qualification requirements. The accepted [CLI
Architecture](architecture.md) controls high-level implementation choices while
the routed [Technical Designs](technical-designs/_technical-designs.md) define
exact shared-capability realization. This Contract remains technology-neutral
and chooses no library, module, storage path, archive format, lock API, or file
application mechanism.

The [generic CLI Operation Pattern](../../../../patterns/open-forge/cli/composable-operation.md)
is optional reusable shape and context. It is not authority for current Open
Forge command semantics. This Contract and the command-local contracts control
the accepted Open Forge behavior.

## Frontmatter Form

The authored settings file `.agents/open-forge.json` accepts `frontmatter` with
the string value `root` or `scoped`. An absent key means scoped. Unsupported
values are invalid settings. `schemaVersion` stays 1. This setting controls
output, not reading. Every workspace reads both forms under the [Markdown
Syntax contract](../framework/markdown/syntax.md#frontmatter).

New route metadata and Install adoption use the selected form. Framework and
Extension delivery renders `.md` files under `.agents/`, except `SKILL.md`,
whose leading frontmatter has an `open-forge` mapping. Only that mapping moves.
Scoped rendering preserves canonical bytes. Root rendering rejects collisions
with foreign root `description`, `responsibility`, or `tags`, and conflicting
root `applyTo`. An equivalent root `applyTo` is kept and the moved copy is
dropped. Body bytes, including fenced examples, are preserved. Native Skills
and other assets retain their existing contracts.

Managed-content comparisons use intended payload bytes rendered in the
workspace's form before applying the existing comparison policy. Source
inventory hashes continue to identify repository bytes. Rendering neither
establishes ownership nor changes the fingerprint policy. Invalid settings
retain each command's existing invalid or unavailable boundary.

## Route Sharing

Every command that writes generated Entries applies the recorded route-sharing
policy. Shared entrypoints and their parent links remain visible. Private child
rows stay omitted. Explicitly requested region targets remain available to
authorized file and entrypoint operations. Index separately excludes private
write targets and metadata acquisition.

A private source intentionally omitted by a trustworthy sharing policy remains
locally operable when its complete filesystem route identity and existing
ownership and safety checks pass. Move and Route Remove do not require that
source's generated-row exposure. Their ordinary public unexposed-leaf refusal
remains unchanged. Replanning retains the exact lock observation. The
[sharing record](technical-designs/lifecycle-provenance.md#stored-ownership)
grants no deletion authority, and [generated navigation](technical-designs/generated-navigation.md#callable-surface)
defines the pure projection boundary.

## Command And Group Shape

The current Open Forge command forms are:

```text
open-forge <direct-command> [operands] [flags]
open-forge <group> <operation> [operands] [flags]
```

Use a direct root command for one stable operation. Use a group only when
several actual operations share a coherent subject and lifecycle. A group with
one actual child is ceremonial and adds no useful operation boundary. Keep
ordinary command paths shallow, normally one or two words after `open-forge`.

The current direct roots are `find`, `index`, `status`, `context`,
`references`, `doctor`, `repair`, `install`, `update`, `remove`, and `cleanup`. The
current grouped families are:

- `route` with `inspect`, `list`, `init`, `create`, `update`, `move`, and
  `remove` operations.
- `extension` with `list`, `inspect`, `create`, `install`, `update`, and
  `remove` operations.
- `library` with `list`, `inspect`, `attach`, `sync`, and `detach` operations.

The `library` family composes ordinary consumer paths from one registered
contained source root. Its command-local contracts own the exact source
inventory, mapping, collision, capability, and record policy. The shared
operation shape does not turn Library records into Framework runtime authority
or make a Library ID a source-reference operand.

The [Command Contract Set](command-contract-set.md) records the current roles,
placement, authority boundaries, and detailed local contract links. A group
performs no domain operation or prompt by itself. Its bare form shows help for
its child operations. Every leaf command performs one complete operation. A flag
may select input, add compatible behavior, choose output, or set write policy,
but it must not turn a leaf into another operation.

## Command Input Roles

The command path selects the operation. Positional operands provide its primary
subjects or query values:

```text
open-forge context [source-reference...]
open-forge route move <source-reference> <destination-target>
```

A leaf may require one or more operands when its complete operation needs a
subject or query. Do not require a value flag merely to choose what the command
does. When a leaf already selects one complete filtering operation, its typed
predicate flags narrow that operation rather than selecting another operation.

Typed predicate flags are different when the leaf already performs one complete
filtering operation and its operand-free form has a documented result:

```text
open-forge find
open-forge find --tag=Architecture --heading=Axioms
```

Here `find` selects one source-discovery operation. The first form returns its
unfiltered source inventory. Each typed predicate flag narrows that same
inventory by a named field, and compatible predicate flags compose without
changing the operation. Use a named value flag when positional ordering would
otherwise make different operand roles ambiguous, and document that exception.

Treat typed predicate flags as Selection flags that narrow results within the
same operation. Value flags remain appropriate for non-operation choices,
including workspace selection, optional authored metadata, typed predicates,
output projection, input location, combination behavior, and write policy. A
typed value keeps its default, composition, and future compatible values
explicit.

A required `--mode`, `--kind`, `--action`, `--by`, or similar discriminator is a
review signal that the leaf may be underspecified. Move materially different
operations into child operations when they have distinct intent, validation,
effects, or result meaning. Do not split one operation merely because an
optional flag changes one compatible dimension of it.

## Guided Leaves And Prompts

A prompt-capable leaf normally exposes its prompt from its simplest useful
interactive invocation, usually without operands or operation-specific flags.
If its primary subject cannot be safely and finitely enumerated, the subject
remains required instead of being invented by a prompt.

Prompt answers, positional operands, and operation-specific flags populate the
same typed request. Explicit inputs add to or narrow that request
incrementally. Conflicting explicit inputs are invalid. The operation does not
create hidden precedence, disjunctive modes, or a recommendation that is
selected merely because it was displayed. A recommendation may be emphasized
for review, but the user or an explicit input must supply its authority.

The host supplies three prompt capabilities: `CanPrompt`, `CanReadKeys`, and
`CanRedraw`, plus current viewport dimensions and a supported screen-clearing
operation when redraw is available. The host owns Console access; reusable
presentation consumes these supplied facts and callbacks. When `CanPrompt` is false, the command reports the shared family
message for the missing answer: `confirmation-required`, `selection-required`,
or `permission-required`, with the flag that supplies it. `--format json` and
`--automatic` never prompt. Missing semantic input is `invalid-input`.
Missing authority or an unresolved choice is `blocked`.

The host derives those capabilities as follows:

| Capability    | Condition                                                        | Effect                                                                               |
| ------------- | ---------------------------------------------------------------- | ------------------------------------------------------------------------------------ |
| `CanPrompt`   | stdin and stderr are not redirected                              | Any prompt is allowed.                                                               |
| `CanReadKeys` | `CanPrompt`, `TERM` is not `dumb`, and the console supports keys | Arrow, space, and Escape selection is used.                                          |
| `CanRedraw`   | `CanReadKeys` and the host can clear the current console view    | Each selection frame replaces the current view; unavailable hosts retain line input. |

### Selection viewport

Key-based single, multi, marked-list, and permission selection use one bounded
current view. Keep the question, legend, focused choice, marks and controls
together. The full finite inventory remains navigable, and it need not fit on
screen at once. Show a position line only when some rows are out of view. A row
shows its mark, its label and an optional one-line summary clipped to the view
width. Longer details appear only for the focused choice. Rows carry numbers
only where digit keys choose a row: single selection and every line-mode list. Clip or wrap through the actual viewport width and reserve
enough rows for controls and a useful current choice, without terminal wrapping
or scrolling stale rows into the current view. At small sizes, prioritize the
question, focus, position and next action over secondary detail.

Read viewport dimensions at each frame and refresh after a size change while
awaiting input. Replace the entire current view through the supported host
clearing operation rather than counting logical newlines in wrapped text.
Permission frames retain the owner and path context with their choices. When
paths exceed the viewport, page them with explicit position and PageUp/PageDown
controls; every complete path remains accessible before an authority choice.
Clearing is confined to capable interactive selection; never clear the final
plan review or a confirmation waiting for authority. No terminal control enters
JSON, redirected, automatic or dry-run report output. If key/geometry/clearing
capability is unavailable, preserve the existing numbered line-input semantics.

### Prompt primitives

All prompt primitives have key and line modes with identical semantics:

- Confirmation asks `Apply these changes? [y/N]`. When the plan deletes,
  replaces or removes something, the question names that part, such as
  `Apply these changes, including deleting 3 files? [y/N]`,
  `Apply these changes, including replacing 2 existing files? [y/N]` or
  `Apply these changes, including removing 4 links? [y/N]`. The question
  still approves the whole plan shown above it. The key answers are `y`,
  `n`, Enter, and Escape. Line mode accepts `y`, `yes`, `n`, `no`, or empty.
  The flag equivalent is `--automatic`.
- Single selection shows the possible paths or IDs, supports up/down, Enter,
  digits, and Escape, and has the line-mode question `Choose a number (1-2), or
press Enter to cancel:`. An exact path or ID is its flag equivalent.
- Multi-selection marks a chosen row with its action: `[+]` to install or
  update, and `[-]` to remove. `[*]` marks a row included because another
  choice needs it, and `[ ]` marks a row that is not chosen. A legend names
  the marks the list uses. Space toggles, `a` chooses all, `n` clears all,
  up/down moves, Enter continues, and Escape cancels. Direct dependencies
  appear in `needs:`. Reverse dependencies appear in `required by` or
  `needed by`. Its flag equivalents are package IDs or `--all`.
- A marked list gives every row one of a fixed set of marks and starts with a
  complete answer. Up/down moves, a mark key sets the focused row, space moves
  it to the next mark, Enter accepts the whole list, and Escape cancels. A
  locked row shows why it cannot change. Line mode prints the numbered list and
  accepts edits such as `3-` or `2+ 9~`, separated by spaces or commas, then
  reprints the list. An empty line accepts the list, and `cancel` or end of
  input cancels. An invalid edit repeats the rule and changes nothing. Install
  Custom is the only marked list, and its flag equivalent is `--route`.
- Text input is line mode only. A rejected value repeats the rule, such as
  `'My Tools' is not a valid ID. Use lowercase letters, digits, and hyphens.`
  The operand or option is its flag equivalent.
- Permission selection asks `Allow <owner> to change these paths outside .agents?`,
  lists each path, and shows `Allow always`, `Allow once`, and `Cancel` with a
  one-line description each, in key and line mode. Line mode asks
  `Type always, once or cancel:` and accepts `always`, `once`, or `cancel`. The flag equivalent is
  `--allow-path <path>` and means allow always.

Choosing a required dependency marks it `[*]`, and a required row cannot be
toggled off while a chosen package needs it. Trying prints
`To leave <dependency> out, unchoose <package> first.`. Enter with no
package chosen prints `Choose at least one package, or press esc to cancel.`.
Line mode reports added dependencies with `Also included: <dependency>,
required by <package>.`, for example `Also included: memory-starters, required
by development-toolkit.`. In a removal list, choosing a package that others
need includes them too and prints `Removing <package> also removes <dependents>.`.
An invalid line-mode answer to a confirmation prints `Type y or n.`, and to a
permission question `Type always, once or cancel.`, before asking again.

### Plan review

Before every confirmation, the command renders its already-established
dry-run report at `minimal` through the same report renderer to stderr, then
asks the confirmation question. It does not rerun the operation or acquire
additional mutation authority. This applies to each confirmation, including an
existing-file approval or a safe-repair confirmation. After an accepted answer,
only the apply result is rendered to stdout.

For example:

```text
Would install the Open Forge Framework into D:/work/myrepo.
  Would create 21 files and 20 directories under .agents, plus AGENTS.md and CLAUDE.md.
  Nothing that already exists would be changed.

Apply these changes? [y/N]
```

Escape, Ctrl+C, end of input, or `no` at any question produces the `cancelled`
status with the sentence `<Command> was cancelled. Nothing was changed.` and
exit `130` before effects. Cancellation after effects begins uses the partial
result wording `Stopped after <n> of <m> changes.` with the recovery path.

An operation-specific `--automatic` flag may suppress prompts and select only
the documented explicit inputs plus deterministic automatic selections and
defaults for that operation. An unresolved semantic choice remains blocked.
`--automatic` is not global. It never accepts a recommendation, replaces
divergent content, adds replacement or deletion authority, adopts or takes
ownership, bypasses safety checks or conflicts, or makes a fuzzy choice.
Repeating it is idempotent. A leaf must explicitly apply this flag; read-only
and unrelated commands reject it.

## Modifier Roles And Stable Meanings

Classify each flag before adding it:

| Role         | Purpose                                                                                  | Current examples                                                    |
| ------------ | ---------------------------------------------------------------------------------------- | ------------------------------------------------------------------- |
| Global       | Same meaning across the CLI                                                              | `--workspace`, `--format`, `--detail`, `--detail-filter`            |
| Selection    | Adds or narrows explicit input or results within the same operation                      | `--tag`, `--follow-links`, `--additions-only`                       |
| Guided input | Uses explicit input plus documented deterministic automatic selections without prompting | `--automatic`                                                       |
| Projection   | Chooses which parts of one result are shown                                              | `--content`                                                         |
| Write policy | Controls preview                                                                         | `--dry-run`                                                         |
| Authority    | Widens one explicitly named owned or replacement boundary within the same operation      | `--force` only where its local contract defines the complete effect |

Flags in one role compose when their meanings do not conflict. Prefer one
typed projection flag with named parts over several mutually exclusive Boolean
flags. A typed projection may accept named parts, for example:

```text
--content=frontmatter,body,section:Axioms
```

Use separate operands for repeated primary subjects:

```text
open-forge context route-a route-b
```

Do not hide several subjects in a comma-separated flag when each value needs
independent parsing, completion, validation, and errors. Do not add a Boolean
flag for one member when the underlying dimension has several compatible named
values.

When two flags represent incompatible operations rather than compatible choices,
split the operation into separate commands. Do not resolve that conflict through
precedence, terminal state, or workspace state. Prefer one typed composable flag
for a multi-value dimension rather than one-off Boolean flags for individual
members.

An Authority flag is distinct from write policy and from a safety bypass. It may
remain on one leaf only when the subject, intended operation, current-fact
resolution, plan, safety boundaries, result vocabulary, and verification and
recovery lifecycle remain the same while the flag widens exactly one explicit
owned or replacement boundary. The command's local contract must define that
boundary completely. `--force` is an example, not a generic meaning.

An Authority flag must be idempotent. It must not imply a safety bypass, deletion
outside its exact boundary, adoption or ownership, conflict or identity bypass,
containment or marker bypass, confirmation unrelated to its named authority, or
weaker verification or recovery-bundle handling. If it changes the job, subject, validation,
effect family, result meaning, or recovery lifecycle, split it into another
operation.

Define each global flag once. Every valid command path accepts a well-formed
global flag with the same meaning. A global flag is an explicit no-op when its
meaning does not apply to the selected operation. Reuse a flag name only when
its full meaning stays the same. Keep authority-bearing flags fully spelled and
separate from ordinary confirmation.

A safety bypass must not grant permission to overwrite, delete, force an
operation, or take ownership. A bounded Authority flag must not grant any
safety bypass or unrelated replacement, deletion, adoption, ownership,
conflict, identity, containment, marker, verification, or recovery authority.

Omitted input may use one documented deterministic default. It must not be
inferred from filesystem coincidence. Explicit workspace selection uses the
exact current directory or exact `--workspace` value; it never searches for
another root.

### `--dry-run`

`--dry-run` is the sole preview spelling. It uses the same typed request,
current facts, planner, and preflight as application, includes every selected
automatic and explicit effect, and writes nothing. Human output may call the
mode Preview. Do not add `--preview`, `--suggestions`, a `plan` operation, a
saved plan, or a generic `apply` operation.

Dry-run forms the same pre-effect planning status as application. Because it
performs no effects, it never produces an apply-time `failed` or `cancelled`
result. Planning or read failures and caller cancellation before effects retain
their own event meaning. Dry-run and application use the same status
conditions; planned changes alone do not create `completed-with-warnings`.

## Human Presentation

### Message style

Every user-facing sentence follows the
[Writing Standard](../maintenance/writing.md):

- Say what happened, name the affected item, and give the next action when
  useful.
- Use short sentences and familiar words. Do not use internal vocabulary:
  never `lifecycle`, `residual`, `preflight`, `projection`, `lease`,
  `occupant`, `provenance`, `topology`, `semantic`, `trusted`, or
  `payload` in text. The [Open Forge
  Dictionary](../maintenance/helpers/dictionary.md) defines the replacement
  words.
- Name paths workspace-relative, with `:line:column` when known.
- Never print an enum value as a word. `not-requested`, `not-applicable`,
  `residual: none`, and `verified` as a bare word are all defects.
- Write counts as words and numbers: `3 files`, `1 file`. Never `1 files`.
- Dry runs say `Would ...` and end with `No files were changed.`
- A no-op says `Nothing to do.`

The current `Next:` rule is defined in
[Repetition, Results, And Streams](#repetition-results-and-streams).

### Shared presentation rules

These are the rules the shared renderer enforces once. Command catalogues rely
on them and do not restate them.

| Rule            | Content                                                                                                                                                                                                                                                                                                                 |
| --------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Headline        | The first line is one complete sentence stating the outcome. There is no `Status:` line, ever.                                                                                                                                                                                                                          |
| Levels          | The [Global CLI Flags Interface Contract](contracts/shared/global-flags/interface.md) defines `minimal` (the default), `standard`, `full`, and `debug`, and the detail each level adds.                                                                                                                                 |
| Listing ladder  | `minimal` lists errors and, except for `doctor`, warnings. `standard` lists warnings for `doctor` too. `full` and `debug` list info. Everything not listed is counted.                                                                                                                                                  |
| Filter          | The [Global CLI Flags Interface Contract](contracts/shared/global-flags/interface.md) defines repeatable `--detail-filter <severity>`; it replaces the listing ladder with exactly the given severities at any level. `all` lists everything. The per-finding depth still follows the level. Counts are never affected. |
| Depth per level | `minimal`: subject, one-line cause, one action. `standard`: adds the reason for the action and per-finding actions. `full`: adds evidence, candidates with why they were included, provenance, hashes, codes. `debug`: adds run diagnostics on stderr.                                                                  |
| Payload         | Requested rows, content and mutation receipts are never shortened by a level. Every replaced, restored, deleted, kept or rewritten path is listed at `minimal`. Created paths are listed at `minimal` except in Framework `install`, which summarizes them by count and directory and names the lock file.              |
| Ordering        | Headline, workspace line (when shown), errors, warnings, what changed, what was kept, what could not be checked, counts, `Next`. Within one severity by path, then line, then column.                                                                                                                                   |
| Subjects        | Every listed finding names a workspace-relative path with `:line:column` when known, or an identifier. Nothing without either is listed or blocks.                                                                                                                                                                      |
| Empty           | Zero counts are not printed unless zero is the answer, said in words. `not-applicable`, `not-requested`, `none observed` and similar never appear. A fact that could not be obtained is one sentence naming the cause. An empty listing is one sentence repeating the query.                                            |
| Workspace echo  | When workspace facts exist, text prints `Workspace: <path>` at `minimal` detail when `--workspace` was supplied or the result is blocked, failed or cancelled. `standard` and higher detail always show it. Text never prints `Selected by:`. JSON retains its workspace representation.                                |
| Codes           | Finding codes appear in text only at `full` and `debug`, in brackets after the title. JSON carries them at every level.                                                                                                                                                                                                 |
| Streams         | The [Repetition, Results, And Streams](#repetition-results-and-streams) subsection defines the shared status and primary-stream mapping. Diagnostics and prompts go to stderr. Parser failures before binding stay text on stderr with no envelope.                                                                     |
| Exits           | The [Result Coordinates Interface Contract](contracts/shared/result-coordinates/interface.md#status-exit-and-stream-coordinates) defines the shared exit table. `route init` scaffold is complete (0). Read-only commands never print `No files changed.`                                                               |
| Formatting      | Two-space indentation, aligned columns for rows, ASCII framing only, paths and identifiers never truncated, no JSON escapes in text, colour only on supported terminals, none in JSON or authored content.                                                                                                              |

### One code, one situation

**A finding code names one situation and renders one sentence.** When a code is
reached from several situations that need different sentences, no message can
be right for all of them, and the wording ends up hedging: a generic fallback,
a cause passed through raw, or an alternation standing in for facts the result
does not carry. That hedging is the symptom. The overloaded code is the defect.

Two different commands may share a sentence: that is what the families below
are for, and a held workspace lock reads identically in thirteen commands.
**Two codes of the same command sharing a sentence is the signature of an
overload**, and
`CliReportInvariantsTests.NoTwoFindingCodesOfOneCommandRenderTheSameMessage`
fails on it across every captured situation.

Note what that test cannot see: a situation with no capture is not checked. When
a finding code has no fixture, the invariant is silent about it, so record the
gap rather than assuming it is covered.

Splitting an overloaded code is a behaviour change. It adds codes to JSON and
can move an exit code where the severity differs, so it is a maintainer
decision, not an implementer's.

### Shared message families

Most finding codes across the 28 commands belong to a family with one meaning.
The family's sentence is written once here, and command catalogues reference it
by family name with the command's subject filled in. `<command>` is the
command's verb phrase ("install", "update the Framework", "move the route").
`<path>` is the affected workspace-relative path.

| Family                       | Severity | Sentence                                                                                                                             | Next                                                                              |
| ---------------------------- | -------- | ------------------------------------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------- |
| invalid-input                | error    | `Cannot <command>: <the exact input problem>.`                                                                                       | The corrected command when it can be formed, otherwise `open-forge <cmd> --help`. |
| confirmation-required        | error    | `<Command> needs confirmation, and this session cannot ask.`                                                                         | `open-forge <cmd> --automatic` (or `--dry-run` to preview)                        |
| workspace-unavailable        | error    | `Cannot use <path> as the workspace: it does not exist or cannot be read.`                                                           | none                                                                              |
| workspace-not-directory      | error    | `Cannot use <path> as the workspace: it is not a directory.`                                                                         | none                                                                              |
| workspace-unsafe             | error    | `Cannot use <path> as the workspace: its location could not be verified (<a link leaves it \| its identity is ambiguous>).`          | none                                                                              |
| workspace-lock-unavailable   | error    | `Another Open Forge command holds the workspace lock. Nothing was changed.`                                                          | `open-forge <cmd> ...` (retry when it finishes)                                   |
| target-changed               | error    | `<path> changed after the plan was made. Nothing was changed.`                                                                       | rerun the same command                                                            |
| target-changed-during-apply  | error    | `<path> changed while changes were being written. Stopped after <n> of <m> changes.`                                                 | `open-forge doctor`                                                               |
| target-unsafe                | error    | `<path> cannot be written safely: <it is a link \| its location could not be verified \| it is reserved>.`                           | none                                                                              |
| target-occupied              | error    | `<path> already exists and is not managed by Open Forge.`                                                                            | `--force --dry-run` where the command supports force                              |
| generated-region-unsafe      | error    | `The Entries section of <path> could not be identified: <it is missing \| there is more than one \| it is malformed>.`               | `open-forge doctor`                                                               |
| projection-unavailable       | warning  | `The Entries content for <path> could not be computed because <child metadata could not be read>.`                                   | `open-forge doctor`                                                               |
| metadata-incomplete          | warning  | `The frontmatter of <path> could not be read completely.`                                                                            | `open-forge doctor`                                                               |
| metadata-unsafe              | error    | `The frontmatter of <path> cannot be used: <reason>.`                                                                                | edit the file                                                                     |
| inspection-incomplete        | warning  | `<path> could not be read completely.`                                                                                               | `open-forge doctor`                                                               |
| recovery-unavailable         | warning  | `Recovery data could not be prepared at <store>. Nothing was changed.`                                                               | check the recovery store location                                                 |
| recovery-conflict            | error    | `Recovery data from an earlier run exists at <path> and blocks this change. Nothing was changed.`                                    | `open-forge cleanup --dry-run`                                                    |
| recovery-artifact-retained   | warning  | `The changes were applied, but the recovery bundle at <path> could not be removed.`                                                  | `open-forge cleanup`                                                              |
| recovery-failed              | error    | `The changes were applied, but the final state of the recovery bundle is unknown.`                                                   | `open-forge doctor`                                                               |
| write-failed                 | error    | `Writing <path> failed. Stopped after <n> of <m> changes. Recovery data: <path>.`                                                    | `open-forge doctor`                                                               |
| verification-failed          | error    | `<path> did not verify after it was written. Recovery data: <path>.`                                                                 | `open-forge doctor`                                                               |
| lifecycle-publication-failed | error    | `The changes were applied, but the ownership record .agents/open-forge.lock.json could not be written.`                              | `open-forge doctor`                                                               |
| lifecycle-unavailable        | warning  | `.agents/open-forge.lock.json could not be read completely.`                                                                         | `open-forge doctor`                                                               |
| lifecycle-blocked            | error    | `.agents/open-forge.lock.json is invalid: <reason>.`                                                                                 | fix or remove the file                                                            |
| ownership-observation        | info     | `No ownership record exists, so <what cannot be listed> cannot be read from it.`                                                     | none                                                                              |
| ownership-conflict           | error    | `<path> is owned by <owner>, so <command> cannot change it.`                                                                         | none                                                                              |
| ownership-claimed            | error    | `<path> is managed by <the Framework \| the <id> Extension \| the <id> Library>, so <command> cannot <verb> it.`                     | the owning command                                                                |
| managed-divergence           | error    | `<path> has changed since it was installed.`                                                                                         | `open-forge update` or `open-forge extension update <id>`                         |
| permission-required          | error    | `Cannot <command>: it writes outside .agents and no grant allows that.` then one line per path                                       | `open-forge <cmd> ... --allow-path <path>`; also the settings file                |
| permission-declined          | error    | `You declined the destinations, so nothing was changed.`                                                                             | none                                                                              |
| permissions-invalid          | error    | `.agents/open-forge.json cannot be used: <reason>.`                                                                                  | fix the file                                                                      |
| permissions-unavailable      | warning  | `.agents/open-forge.json could not be read.`                                                                                         | none                                                                              |
| permissions-changed          | error    | `.agents/open-forge.json changed after the plan was made. Nothing was changed.`                                                      | rerun                                                                             |
| permission-write-failed      | error    | `The grant could not be saved to .agents/open-forge.json.`                                                                           | `open-forge doctor`                                                               |
| identity-collision           | warning  | `The ID <id> matches more than one file. Use the exact path.` then one line per path                                                 | none                                                                              |
| route-ambiguous              | error    | `<id> could match more than one route.` then one line per path                                                                       | use the exact path                                                                |
| source-ambiguous             | error    | `<reference> matches more than one source. Use the exact path.` then one line per path                                               | use the exact path                                                                |
| source-unsafe                | error    | `<path> could not be verified to be inside the workspace.`                                                                           | none                                                                              |
| selector-ambiguous           | error    | `--include or --exclude <value> matches more than one source. Use the exact path.`                                                   | none                                                                              |
| selector-unsafe              | error    | `--include or --exclude <value> points outside the workspace.`                                                                       | none                                                                              |
| payload-unavailable          | warning  | `The Framework bundled in this CLI could not be read completely.`                                                                    | reinstall the CLI                                                                 |
| payload-invalid              | error    | `The Framework bundled in this CLI is invalid.`                                                                                      | reinstall the CLI                                                                 |
| framework-unavailable        | warning  | `The Framework files this command needs could not be read completely.`                                                               | `open-forge doctor`                                                               |
| framework-unsafe             | error    | `The Framework files this command needs could not be verified.`                                                                      | `open-forge doctor`                                                               |
| selection-required           | error    | `<Command> needs to know which packages. Pass their IDs or --all.` (add `This session cannot ask.` when a prompt would have applied) | `open-forge extension list`                                                       |
| interaction-ended            | error    | `Input ended before a choice was made. Nothing was changed.`                                                                         | none                                                                              |
| operation-failed             | error    | `<Command> stopped because of an unexpected error: <bounded reason>.`                                                                | `open-forge <cmd> ... --detail debug`                                             |
| interrupted                  | error    | `<Command> was cancelled. Nothing was changed.` or `... Stopped after <n> of <m> changes.`                                           | rerun                                                                             |
| unknown-id                   | error    | `No <Library \| Extension> has the ID <id>.`                                                                                         | the list command                                                                  |
| unknown-source               | error    | `No source has the ID <id>.`                                                                                                         | `open-forge route list --depth=all`                                               |

The family names above are the vocabulary used in the `Family` column of each
command catalogue. A code whose family is `local` has its complete sentence in
the command file.

### Repetition, Results, And Streams

Keep repetition rules structural and explicit:

- A singleton one-value input rejects repetition, including repetition with an
  equal value, unless the command explicitly declares that input multi-value.
- An explicitly multi-value flag may repeat and combines under its command-local
  order, duplicate, and deduplication rules. Do not add last-wins behavior by
  convention.
- An applicable command-specific Boolean such as `--dry-run` may repeat
  idempotently. Repetition does not create another operation or multiply
  authority.

Use the same seven renamed semantic statuses across operations:
`completed`, `completed-with-warnings`, `incomplete`, `invalid-input`,
`blocked`, `failed`, and `cancelled`. A command defines whether
`completed-with-warnings` applies and what finite condition forms it. For
ordinary safety and coverage conditions, use `blocked` >
`incomplete` > `completed-with-warnings` > `completed`. Invalid input stops
before operation resolution. Failed and cancelled preserve their event meaning.
Text uses sentences and severity words rather than printing a status token.

Primary human `completed`, `completed-with-warnings`, and `incomplete` results
go to stdout. Primary human `invalid-input`, `blocked`, `failed`, and
`cancelled` results go to stderr. JSON selected by `--format json` renders one
complete schema-3 result from the same typed result to stdout for every
semantic status. Bounded diagnostics and prompts use stderr. Parser failures
before binding remain text on stderr with no envelope.

When a result needs a next action, text retains at most one `Next:` line. It
uses the exact runnable command or short sentence and its reason, and remains
the final text line. For Extension Install PermissionRequired results with
MissingPermissions, preserve both existing permission options and place the
settings alternative before the runnable `Next:` action.

When recovery warning coexists with another
command-local completed-with-warnings condition, exact recovery cleanup
guidance owns the
single `Next:` action; the other warning facts remain visible evidence.

The [Global CLI Flags contract](contracts/shared/global-flags/interface.md)
remains the detailed owner for `--format`, `--detail`, and
`--detail-filter`. Each command contract remains the detailed owner for its
local finite conditions and result facts. This shared status and stream rule
does not create a second command schema. The [Result Coordinates Interface
Contract](contracts/shared/result-coordinates/interface.md) defines the exact
numeric exits, schema-3 envelope, source-location coordinates, primary streams,
and compatibility.

## Typed Operation Flow

Keep the accepted operation stages visible and directly testable:

```text
validated command input
  -> complete operation request
  -> current facts
  -> read result or complete mutation plan
  -> complete preflight when mutating
  -> lease acquisition and under-lease revalidation
  -> preview or application consent
  -> immediate per-effect checks and monotonic apply
  -> verification and recovery-disposition reporting
  -> optional bounded post-processing
  -> typed operation result
  -> human or structured rendering
  -> process completion
```

Read-only operations stop after producing their result. They do not create an
empty mutation plan. Dry-run and application use the same request, fact
inspection, planner, and preflight. Dry-run stops before effects. Changing
intent discards the old plan and starts planning again.

Handlers return typed values. They do not write streams, select presentation,
or set process status. Human and JSON renderers consume the same result and do
not rerun the operation.

Every mutation that addresses a logical file leaf consumes the neutral
no-follow leaf observation before ordinary physical resolution, during initial
preflight, again under the held workspace lease, and immediately before its
effect. A present link, reparse point, or special final leaf blocks ordinary
`Create`, `Replace`, `Delete`, and `ReplaceGeneratedRegion`. Stable contained
directory-link ancestry remains governed by the ordinary filesystem contract.
This guard does not consult Library records, so route and Index mutations cannot
follow, write, or delete a Library projection.

After all preflight and recovery preparation succeeds, application is
monotonic. Effects run in the command-owned order, each with its immediate
no-follow and expected-state check; a failure or interruption stops new effects
and leaves already verified effects and residual evidence in place. Shared
support does not automatically compensate for an earlier effect. A Library
record is written last, after all relative file-link effects verify.

### Recovery And Cleanup Boundaries

The direct root [`cleanup` contract](contracts/cleanup/_cleanup.md) is a narrow
exception to the normal recovery shape. Other mutating operations prepare one
external recovery bundle for the complete operation before the first covered
effect. Cleanup still forms one complete catalogue and plan, runs preflight,
and revalidates selected artifacts immediately before deletion. It
may return a verified empty no-op without a lease. Before any deletion, it
acquires the same-workspace lease and repeats final catalogue and expected-state
validation under that lease. It creates no replacement bundle, staging copy,
journal, or tombstone merely to delete eligible cleanup artifacts, and it does
not reverse a deletion that it has verified.

Already verified deletions remain desired effects when a later deletion fails or
the caller interrupts. Remaining and residual facts stay visible for a fresh
plan. This exception applies only to cleanup. Other mutating operations retain
their accepted bundle-preparation and post-verification disposition rules.

Recovery writing and observation remain distinct. Only a mutation bundle writer
may create application-owned recovery storage during pre-effect preparation.
Status, Doctor, and Cleanup observe without creating storage. An absent store is
zero recognized bundles or drafts for Status and Doctor and a verified Cleanup
no-op. An unreadable selected-workspace catalogue remains unavailable. A final
bundle that cannot be validated retains its exact malformed, unsupported, or
unavailable condition. Neither condition is absence.

Status and Doctor do not acquire the workspace lease, report activity, or infer
activity from bundle contents, a filename, age, PID, marker, journal, or the
persistent external lock file.

Before applying any non-no-op effect that the operation must be able to reverse,
the complete operation prepares exactly one immutable verified bundle outside
the workspace. The bundle contains the exact prior bytes and state-specific
prior and intended identity for every covered effect. This includes ordinary
existing-file `Replace`, `ReplaceGeneratedRegion`, and `Delete` effects,
relative-file-link `Create` and `Delete` effects, and the prior-missing ordinary
`Create` that creates the Library record or consumer permission document. Semantic or byte no-op targets have no
bundle entry. An operation containing only no-ops creates no bundle. Unavailable
required storage forms a pre-effect incomplete result. Unknown, malformed,
mismatched, or colliding artifacts do not authorize an effect.

All bundle preparation and verification complete before the first target effect.
A draft never authorizes an effect. Every covered effect requires the matching
verified final preparation. An ordinary Create remains preparation-free unless
the command explicitly marks its prior-missing record creation as reversible,
as Library operations do. Exact store, ZIP, manifest, draft/final,
bounded-validation, and callable mechanics live in the [Mutation And Recovery
Technical Design](technical-designs/mutation-and-recovery.md).

Before post-verification deletion begins, handled application, verification, or
cancellation outcomes stop new effects and report the actual residual draft or
final path; a valid final bundle remains when preparation completed.

After whole-command verification succeeds, the command deletes only its
positively recognized bundle. `Deleted` with disposition `Removed` permits
normal completion. `Failed` with positively observed disposition `Retained`
keeps target effects successful and produces `completed-with-warnings` with the
exact recovery path and cleanup guidance. `Failed` with disposition `Unknown`
produces `failed`
and reports an exact expected path only when the deletion result provides one.
`Blocked` and `Cancelled`, with either `Retained` or `Unknown`, remain neutral
typed event facts for command-local mapping; disposition alone never selects a
command status. A closed final artifact may remain after an abrupt process
termination, but the CLI provides no executable crash or power-loss durability
guarantee. Shared support never
automatically restores a target, rolls back an effect, or compensates for target
effects, classifies current target state from recovery provenance, or saves a
journal, progress receipt, history, or replayable plan. A fresh invocation plans
from current facts.

Bundles are immutable after preparation and are never extracted by the CLI.
Status, Doctor, and Cleanup may inspect payload data only through bounded
validation. They never extract, disclose, render, log, return, retain, or
materialize payload bytes. Exact archive, hashing, and bounded-reading mechanics
belong to the [Mutation And Recovery Technical
Design](technical-designs/mutation-and-recovery.md).

Cleanup mechanically deletes only exact named final or draft candidates for the
selected workspace after it acquires the same-workspace lease and repeats the
complete catalogue and ordinary path/kind checks under that lease, then verifies
absence. The held lease provides cooperating-process exclusion only. If Cleanup
cannot acquire it, Cleanup performs no deletion. Unknown,
malformed, mismatched, differently keyed, or lookalike artifacts remain
untouched. Same-path normalized physical workspace identity is required for
deterministic rediscovery; workspace moves are outside the automatic guarantee,
though Doctor or Cleanup may report orphaned original-root bundles and never
auto-binds or restores them. Cleanup writes no marker, PID, journal, lock
metadata, or other lifecycle record and makes no activity inference.

The mutation lock is a persistent reusable external coordination artifact with
no activity metadata. Its existence does not demonstrate ownership or activity,
and read-only observation never acquires it. Composition and terminal no-effect
flows do not create recovery or lock infrastructure. Exact location, identity,
handle, and lifetime mechanics belong to the [Mutation And Recovery Technical
Design](technical-designs/mutation-and-recovery.md).

Standalone Extension Create uses a separate exact-destination, collision, and
revalidation path with no workspace lease, none of `Replace`,
`ReplaceGeneratedRegion`, or `Delete`, and no recovery bundle.

Cleanup's operand-free catalogue is explicit command intent, not an automatic
interaction policy. Invoking the dedicated command explicitly selects deletion
of all positively recognized eligible cleanup artifacts under its local
contract. An `--automatic` interaction policy or inferred recommendation never
adds authority to delete or replace content.

## Source Locality And Contract Boundaries

Keep one command's syntax, request resolver, handler, planner, renderer, and
direct tests near that command. Move parsing, graph, mutation, result, or
display support upward only after real sibling commands share it. The permanent
shared contract route is [`contracts/shared/`](contracts/shared/_shared.md), but
a shared location does not make a command-local meaning global.

Keep operation meaning visible and directly testable. Shared mechanisms must not
hide the operation's subjects, authority, stages, effects, or result semantics.
This is a visibility and locality boundary, not a choice of modules,
dependencies, libraries, dispatch, or runtime. Prefer direct typed relationships
and exhaustive handling of finite cases where they keep the contract visible.
Do not use string-keyed behavior registries, service locators, reflective
dispatch, or a universal operation engine that hides command meaning. The
accepted Architecture controls concrete implementation choices. The active
[CLI Development](../../../working/cli-development/_cli-development.md) route
records implementation and executable evidence.

## Errors And No-Ops

Every error states the operation, affected subject, cause, and useful next
action. `debug` detail may add bounded diagnostic evidence on stderr but must
not be required to understand an ordinary failure.

An idempotent write converges on one state. Repeating it against that state
returns a verified no-op. Do not invent an effect to represent unchanged state.
An operation may report a verified no-op only after its local contract has
established the complete facts needed to prove that no effect is required.

## Current Examples And Related Authority

The active [`context` contract set](contracts/context/_context.md) applies this
Contract through explicit route operands, composable selection and content
projection, one typed result, and no session state.

The active [`find` contract set](contracts/find/_find.md) applies the
typed-predicate exception through one meaningful bare inventory, composable tag
and heading filters, one flat requirement, independent region and content
selection, and one typed result.

The accepted [`install` Interface Contract](contracts/install/interface.md) and
root [`update` Interface Contract](contracts/update/interface.md) demonstrate
bounded composition:

```text
open-forge install --force --dry-run
```

Here `--force` is an Authority flag and `--dry-run` is a Write policy flag for
the same leaf. On `install`, force is limited to an eligible exact initial
occupant. It does not reconcile managed divergence. The Install Interface owns
that complete local meaning; this example does not define a generic effect for
either flag.

The separate managed-reconciliation composition is:

```text
open-forge update --force --prune --automatic --dry-run
```

Here `--force` widens current expected-footprint replacement, `--prune` widens
retired-content deletion, `--automatic` suppresses interaction without adding
authority, and `--dry-run` previews the same complete plan. The Update Interface
owns these boundaries; the flags do not form a generic lifecycle mode.

The accepted [`extension` contract set](contracts/extension/_extension.md) applies
the same request shape while keeping source, ownership, and package boundaries
explicit:

```text
open-forge extension install development-toolkit --automatic --dry-run
open-forge extension update development-toolkit --force --prune --dry-run
open-forge extension remove development-toolkit --prune --automatic --dry-run
```

The Extension contracts own the exact source universe, package identity,
dependency, ownership-release, and same-request prune meaning. `--automatic`
does not select packages, replace divergence, adopt content, or delete changed
content on its own. Read-only `extension list` and `extension inspect` reject
`--automatic` and mutation flags. `extension create` treats global
`--workspace` as a no-op because `--path` names its catalogue destination.

## Review Checks

- The command performs one complete operation.
- A direct root represents one stable operation. A group represents several
  actual coherent operations and is not ceremonial.
- The command path is shallow, and the current direct-root and grouped-family
  conventions remain visible in help.
- Primary subjects are explicit operands unless a command owns a complete,
  safely bounded operand-free catalogue. Cleanup's default-all catalogue is the
  accepted mutation exception; it never becomes arbitrary path discovery or a
  generic batch surface.
- Primary subjects and query values that do not name a predicate field are
  operands rather than required discriminator flags.
- A typed predicate narrows one complete filtering operation whose form without
  operands has a documented result. It does not supply a missing operation kind.
- Every flag has one named role and stable meaning. Compatible modifiers compose
  without hidden precedence; incompatible operations are separate commands.
- An Authority flag widens exactly one explicit owned or replacement boundary
  without changing the subject, plan, safety, result, verification, or recovery
  lifecycle. It is idempotent and does not imply a safety bypass or unrelated
  authority.
- Singleton and explicitly multi-value repetition rules are explicit, and
  applicable Boolean repetition is idempotent.
- A group performs no operation or prompt, and its bare form shows help.
- A prompt-capable leaf exposes its simplest useful interactive invocation when
  its primary subject can be safely and finitely enumerated; otherwise the
  subject remains explicit.
- Prompt answers and explicit inputs populate one typed request without hidden
  precedence, disjunctive modes, or automatic recommendation selection.
- JSON and other non-interactive modes never prompt.
- Operation-specific `--automatic` is explicit, deterministic, idempotent, and
  cannot grant replacement, deletion, ownership, bypass, or fuzzy-choice
  authority. Automatic selection never deletes or replaces content on its own.
- `--dry-run` is the sole preview spelling and shares planning and preflight
  with application while writing nothing. It shares status conditions, and
  planned changes alone do not create `completed-with-warnings`.
- Every reversible non-no-op effect in the complete operation has one matching
  verified immutable external recovery bundle prepared before the first effect.
  This includes ordinary existing-file effects, relative-file-link creates and
  deletes, and the prior-missing Library record Create. Semantic or byte
  no-ops receive none. A draft never authorizes an effect, and an ordinary
  Create remains preparation-free unless its command marks it reversible.
  Exact archive, draft/final, atomic-file, no-follow, callable, and validation
  mechanics belong to the [Mutation And Recovery Technical
  Design](technical-designs/mutation-and-recovery.md).
- The no-follow leaf guard runs before ordinary physical resolution, at initial
  preflight, under-lease revalidation, and immediately before each effect.
  Present link, reparse, or special final leaves block ordinary file effects;
  the guard does not require Library record authority.
- Library application is all-preflight, lease-revalidated, and monotonic. It
  applies only declared relative file-link and real-parent-directory effects,
  verifies them, and publishes the Library record last. Source bytes never
  become effects.
- Successful commands delete their command-owned bundle only after whole-command
  verification. `Deleted`/`Removed` permits normal completion;
  `Failed`/positively observed `Retained` preserves successful target effects
  with `completed-with-warnings`, the exact recovery path, and cleanup guidance;
  and
  `Failed`/`Unknown` produces `failed` and reports an exact path only when the
  deletion result supplies one.
  `Blocked` and `Cancelled`, with either `Retained` or `Unknown`, remain neutral
  typed event facts for command-local mapping.
  Before post-verification deletion, handled application, verification, or
  cancellation outcomes report the actual residual draft or final path. A closed
  final may remain after abrupt process termination without a crash- or
  power-loss-durability guarantee. The foundation never automatically restores a
  target, rolls back an effect, or compensates for target effects, and it does
  not save a journal, progress receipt, history, or replayable plan.
- Cleanup deletes only exact named selected-workspace final bundles or drafts
  under its nonrecursive support-artifact exception. Before deletion it holds
  the same-workspace lease, repeats catalogue and expected-state validation, and
  revalidates each candidate immediately before deletion; contention prevents
  all deletion. Unknown,
  malformed, mismatched, or differently keyed artifacts remain untouched. The
  persistent external lock carries no activity metadata.
- Status, Doctor, and Cleanup use bounded validation and never extract, disclose,
  retain, or materialize payload bytes. Status and Doctor neither report nor
  infer activity.
- Read-only and mutating authority remain separate.
- Important stages return typed results and can be tested directly.
- Human and structured output use one operation result. The seven statuses,
  ordinary precedence, stream assignment, bounded diagnostics, and one schema-3
  envelope rule are uniform; finite `completed-with-warnings` conditions remain
  command-local.
- Shared code reflects demonstrated reuse without hiding command meaning.
- Repeating unchanged input produces the same semantic result and, when the
  local contract permits proof, a verified no-op.
- Errors remain useful without `debug` detail.
