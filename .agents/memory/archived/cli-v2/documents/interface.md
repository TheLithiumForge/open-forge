---
open-forge:
  description: "Historical CLI-v2 source: Accepted replacement CLI command inventory, arguments, flags, interaction states, outputs, guidance, and lifecycle behavior"
  responsibility: Define the complete accepted public interaction contract of the replacement Open Forge CLI without claiming that it ships today
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# Open Forge CLI Interface

## Status And Scope

This document is authoritative for the accepted public surface of the replacement Open Forge CLI. It fixes the command inventory, command paths, arguments, flags, interaction rules, and intended operation boundaries that implementation must expose.

The replacement does not ship yet. The [MVP user contract](../../../../../docs/cli.md) remains exact for the currently distributed binary, and the [MVP architecture](mvp-architecture.md) preserves its proven behavior. The [CLI Architecture](architecture.md) owns replacement implementation boundaries. The [command-surface decision](../../decisions/cli/cli-command-surface.md) preserves why this interface was selected.

Production TypeScript is authoritative for exact implemented declarations, named values, and import paths. This document retains their public meaning, behavior, and compatibility promises and links to the authoritative source. A TypeScript example is schematic only when it is explicitly labeled as a contract that does not yet have production source.

The [runtime compatibility contract](contracts/runtime-compatibility.md) owns
the accepted Node.js, Bun, and Deno floors and the one-artifact parity promise.

## Experience Contract

The interface is optimized for any capable agent and remains complete for people, scripts, continuous integration, editors, and future integrations.

It should feel intuitive, predictable, helpful, and technically boring:

- The command path visibly identifies one operation.
- Related operations share one discoverable family.
- The same command, argument, or flag never acquires another meaning from terminal state or unrelated workspace state.
- Read operations do not mutate.
- Mutation previews use the same planner as application.
- Human and JSON presentations describe the same typed result.
- Wizards collect missing input without creating another implementation.
- Errors explain the failed operation, cause, and useful next action in ordinary language.
- Completion makes the accepted vocabulary cheap to discover without creating a shorthand vocabulary.

The interface does not preserve legacy command names, flags, result shapes, aliases, or behavioral quirks. The replacement entrypoint never dispatches to the frozen MVP or translates legacy input. Developers may invoke the frozen MVP source explicitly when comparison evidence is useful.

## Grammar And Grouping

Use the following general shape:

```text
open-forge <command-path...> [arguments...] [global flags] [command flags]
```

Operations are grouped around their semantic center:

| Relationship                                     | Public shape   |
| ------------------------------------------------ | -------------- |
| One complete stable job                          | Direct command |
| Several operations share one domain or lifecycle | Subject family |
| One operation genuinely spans explicit kinds     | Action family  |

The accepted interface currently uses direct commands and three subject
families. No ordinary command path has more than one grouping level. A group
node exposes local help and completion but performs no domain operation.

```text
open-forge
  status
  context
  find
  doctor
  repair
  create
  install
  route
    list
    inspect
    init
    rebuild
  extension
    list
    inspect
    add
    update
    remove
  completion
    install
    remove
    script
```

There are nineteen public leaf commands. Bare `open-forge` shows root help and
succeeds. A bare group such as `open-forge route` shows local help and returns
an invalid-invocation result because no operation was selected. A
shell-completion implementation may expose one hidden `complete` protocol
callback; it is absent from help and completion, produces protocol output
rather than a domain result, and does not enlarge the public command inventory.

There is no root `help` command. Use bare root help, group-local help, or `--help` on a selected path. There is no `plan` command. Mutating leaves use `--dry-run`.

## Global Flags

Global flags are defined and registered once. Each retains the same grammar and
meaning everywhere it applies. Registration does not make every concern
meaningful to every operation; one typed operation-applicability map validates
the selected leaf. An inapplicable flag returns `cli.parse` with `invalid`
status before workspace selection or domain execution.

| Flag                      | Alias              | Meaning                                                                         |
| ------------------------- | ------------------ | ------------------------------------------------------------------------------- |
| `--json`                  | `--j`              | Render the operation's typed result as one versioned JSON document              |
| `--workspace <directory>` | `--ws <directory>` | Select the logical workspace root                                               |
| `--yes`                   | none               | Accept documented defaults and confirmations for the already selected operation |
| `--help`                  | `--h`              | Show help for the selected command path without executing it                    |
| `--version`               | `--v`              | Show the distributed CLI version without executing a command                    |

Applicability is fixed by selected operation:

| Operation                                               | `--json` | `--workspace` | `--yes` | `--help` | `--version` |
| ------------------------------------------------------- | -------: | ------------: | ------: | -------: | ----------: |
| Root or group help                                      |      yes |            no |      no |      yes |         yes |
| `status`, `context`, `find`, `doctor`                   |      yes |           yes |      no |      yes |         yes |
| `repair`, `create`, `install`                           |      yes |           yes |     yes |      yes |         yes |
| `route list`, `route inspect`                           |      yes |           yes |      no |      yes |         yes |
| `route init`, `route rebuild`                           |      yes |           yes |     yes |      yes |         yes |
| `extension list`, `extension inspect`                   |      yes |           yes |      no |      yes |         yes |
| `extension add`, `extension update`, `extension remove` |      yes |           yes |     yes |      yes |         yes |
| `completion script`                                     |      yes |            no |      no |      yes |         yes |
| `completion install`, `completion remove`               |      yes |            no |     yes |      yes |         yes |

Applicability does not vary with current facts or another flag. For example,
`repair --dry-run --yes` remains syntactically valid because `repair` owns
ordinary confirmation policy even when the selected preview does not apply
writes. Conversely, `status --yes` and `completion install --workspace .` are
invalid rather than accepted as no-ops.

`--json` makes interaction unavailable but remains applicable to every
operation. A guided operation may therefore accept the flag and still return
`invalid` because required subjects were omitted. For example, `extension add
--json` has valid presentation syntax but lacks the Extension ids required
when no selector may open.

### Workspace Selection

Every workspace-aware operation uses one explicitly selected logical workspace root:

```text
--workspace supplied
  -> resolve exactly that path

--workspace omitted
  -> use the exact current working directory
```

A relative `--workspace` value resolves from the current working directory. The CLI never searches parent directories or substitutes a Git root, package root, nearest `.agents` directory, environment-selected root, or another inferred location.

Selection does not claim that the directory contains a valid installation. The selected operation inspects that exact root and reports an uninstalled, incomplete, installed, or invalid state as its own contract requires.

After workspace selection, every explicit positional `<path>` that addresses Open Forge content uses one grammar: it is workspace-relative and rooted at `.agents`. Both `.agents/...` and `./.agents/...` are accepted and normalize to the same canonical `.agents/...` path. The CLI never adds the `.agents` root, accepts another content root, or searches for one. Commands with no path operand own their exact default internally. For example, `status` inspects the selected workspace's fixed anchors and bare `route rebuild` selects `.agents/loader.md`.

Other filesystem values use purpose-specific operands instead of the `<path>`
grammar. A custom `--template` reference begins with `./`, resolves from the
selected workspace, and remains contained within it. Extension `--path`
instead selects one exact invocation-only external Extension source directory.
A relative value resolves from the process current working directory, may be
inside or outside the workspace, and is never persisted in committed state. It
may point either to a single package root containing `extension.json` or to a
catalogue root whose immediate child package directories contain
`extension.json`. Discovery never recurses. A repeated flag keeps the same
value grammar and meaning everywhere it appears.

The CLI inspects the exact `AGENTS.md` and `.agents/loader.md` anchors when an operation needs canonical Framework presence. It does not parse `AGENTS.md` prose as configuration, discover another loader from a link inside it, or scan ancestors for a different entry.

`--yes` is confirmation policy, not additional authority. It cannot:

- Select an operation or Extension.
- Choose among Templates or Extensions.
- Resolve an ownership ambiguity.
- Substitute for `--overwrite` or explicit interactive collision approval.
- Bypass validation, containment, preservation, or another safety boundary.
- Turn a blocked-repair or manual-decision condition into success.

Do not add generic `--force`, `--pro`, or command-local aliases for global behavior.

## Guided Selection

When a command deliberately makes its subject selection optional, omission has
one uniform meaning:

```text
interactive + no explicit subjects
  -> open the command's selection wizard

explicit subjects
  -> bypass subject selection and resolve those exact values

non-interactive + no explicit subjects
  -> invalid unless the command documents one deterministic semantic default
```

An explicit catalogue, filter, output mode, or confirmation flag is not a
subject selection. For example, `extension add --path ./team-extensions`
opens the add wizard over that catalogue, while `extension add one two --path
./team-extensions` resolves the exact two ids without opening the selection
wizard.

Bypassing selection does not bypass external source review, plan review,
collision approval, deletion decisions, formatter trust, executable
configuration, or another safety decision. Explicit ids skip only the
selection wizard. `--yes` may accept documented confirmations after selection;
it never invents the selected subjects or supplies authority reserved for
another boundary. Direct `install --yes` has one deterministic whole-Framework
target and therefore needs no subject selection.

## Shared Mutation Flag

Every mutating leaf accepts:

```text
--dry-run
```

Its `--dry` alias remains explicit and recognizable while retaining the uniform two-dash flag grammar.

`--dry-run` performs discovery and input validation, builds the complete plan, and preflights the plan and every effect. Preflight includes containment, ownership, protected-boundary, expected-state, verification-readiness, and recovery-readiness checks applicable to the operation. It returns the exact intended effects and verification expectations without applying workspace or shell-profile writes.

The direct mutation and its dry run use the same request, planner, and preflight. Application revalidates the complete plan immediately before the first write and revalidates volatile effect facts immediately before each effect. `--dry-run` changes application policy, not operation intent.

Every workspace-mutating leaf also accepts the canonical-only shared policy:

```text
--skip-git-check
```

It bypasses only the relevant-path Git cleanliness prerequisite and activates
applicable Gitless backup requirements. It never means yes, overwrite, delete,
take ownership, authorize formatter `RUN`, weaken `BLOCK`, or ignore validation.

## Workspace Formatting

Every Open Forge-related complete file affected by a mutation may follow the
same formatter rules as the selected workspace, including `.agents` files and
`AGENTS.md`. Formatting runs only after the verified primary mutation. The CLI
formats only exact affected files; it never broadens the operation into a
workspace-wide format or rolls back completed primary work because formatting
failed.

Formatter selection follows explicit `.agents/open-forge.json` configuration,
workspace or editor configuration, the closest unambiguous supported tool, and
then a guided or manual fallback. Open Forge supports up to three meaningful
mainstream tools per popular language or file family and fewer when an
ecosystem has one canonical formatter. The accepted initial language and tool
matrix lives only in the [workspace formatting
contract](contracts/workspace-formatting.md); command docs do not duplicate it.

Every resolved formatter invocation is classified as `SAFE`, `RUN`, or
`BLOCK`. `SAFE` proves data-only configuration, exact affected files, an
existing executable, and no configured-code, restore, installation, download,
or network behavior. `RUN` preserves the exact-file boundary but loads
executable configuration, plugins, analyzers, project code, or a custom
command, so it requires an explicit interactive `RUN` decision. `BLOCK`
refuses any download, installation, restore, network access, or unbounded file
effect. `--yes` can accept `SAFE`; it cannot authorize `RUN` or weaken
`BLOCK`. The complete behavior is defined by the
[workspace formatting contract](contracts/workspace-formatting.md).

## Command Flags

Repeated command flags keep one CLI-wide grammar and meaning:

| Flag                                  | Alias                              | Meaning                                                                                                                  |
| ------------------------------------- | ---------------------------------- | ------------------------------------------------------------------------------------------------------------------------ |
| `--all`                               | none                               | Select every safely discoverable Completion target for the selected Completion install or remove operation               |
| `--depth <count>`                     | `--dpth <count>`                   | Select a positive bounded route-descendant depth                                                                         |
| `--description <text>`                | `--desc <text>`                    | Set destination-owned routing description metadata                                                                       |
| `--dry-run`                           | `--dry`                            | Return exact planned effects without applying writes                                                                     |
| `--has-tag <tag>`                     | `--htag <tag>`                     | Require one exact existing tag in `find` results                                                                         |
| `--heading <heading>`                 | `--head <heading>`                 | Project one exact Markdown heading from context sources                                                                  |
| `--overwrite`                         | none                               | Authorize every explicitly reported eligible target replacement in the selected lifecycle plan                           |
| `--path <extension-source-directory>` | `--p <extension-source-directory>` | Select one exact invocation-only external Extension catalogue or direct package source for list, inspect, add, or update |
| `--profile <absolute-path>`           | `--prof <absolute-path>`           | Select one exact nonstandard shell startup profile for one explicit Completion shell                                     |
| `--redact`                            | none                               | Replace sensitive status values through the exhaustive share-safe status projection                                      |
| `--restore <route...>`                | none                               | Re-enable one or more exact persisted Framework route exclusions during `install`                                        |
| `--responsibility <text>`             | `--resp <text>`                    | Set destination-owned responsibility metadata                                                                            |
| `--state <state>`                     | `--st <state>`                     | Filter a lifecycle inventory by one exact domain state                                                                   |
| `--tag <tag>`                         | `--t <tag>`                        | Add one repeatable destination-owned tag                                                                                 |
| `--template <reference>`              | `--tpl <reference>`                | Select one explicit Template body source for creation                                                                    |
| `--skip-git-check`                    | none                               | Bypass only the relevant-path Git cleanliness prerequisite for a workspace mutation                                      |

Every visible flag uses two leading dashes. A canonical flag may have one lowercase alias of one to four characters when the abbreviation is recognizable and globally unique. The parser does not accept single-dash forms, clustered aliases, or automatic prefix abbreviation. Canonical and alias spellings parse to the same option value and behavior.

`--all` remains canonical-only because broad selection should stay visible.
`--yes`, `--overwrite`, `--restore`, and `--skip-git-check` remain
canonical-only because their authority should stay fully visible. `--redact`
remains canonical-only because its occasional privacy intent should be
explicit. The remaining aliases are `--j`,
`--ws`, `--h`, `--v`, `--dpth`, `--desc`, `--dry`, `--htag`, `--head`, `--p`,
`--prof`, `--resp`, `--st`, `--t`, and `--tpl`. They form one audited CLI-wide
vocabulary; no command may reuse one for another flag.

## Command Map

This compact map is the review surface for the complete inventory:

| Command                                                                                    | Primary input                                                                                             | Primary output                                                                                                             | Writes |
| ------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------- | ------ |
| `status [--redact]`                                                                        | Selected workspace                                                                                        | Framework presence, payload identity, managed lifecycle summary, Git readiness, recovery evidence, and advisory operations | No     |
| `context [route...] [--heading <heading>]`                                                 | Zero or more explicit routes                                                                              | Ordered complete source bodies with reasons and provenance                                                                 | No     |
| `find [route...] [--has-tag <tag>]...`                                                     | Optional route bounds and exact tag filters                                                               | Matching route metadata without bodies                                                                                     | No     |
| `doctor`                                                                                   | Selected workspace                                                                                        | Complete classified workspace findings                                                                                     | No     |
| `repair [--dry-run]`                                                                       | Selected workspace                                                                                        | Planned or applied safe fixes plus remaining findings                                                                      | Yes    |
| `create <destination.md> [metadata] [--template <reference>] [--dry-run]`                  | Exact workspace-relative `.agents/...` Markdown destination and optional source                           | Created file, route update, provenance, and authoring findings                                                             | Yes    |
| `install [--restore <route...>] [--overwrite] [--dry-run]`                                 | Complete Framework payload embedded in the running CLI and optional exact persisted exclusions to restore | Planned or applied whole-Framework installation or reconciliation                                                          | Yes    |
| `route list [route] [--depth <count>]`                                                     | Optional route and descendant depth                                                                       | Ordered route summaries                                                                                                    | No     |
| `route inspect <route...>`                                                                 | One or more exact routes                                                                                  | Identity, ancestry, metadata, relationships, and provenance                                                                | No     |
| `route init <path> [metadata] [--template <reference>] [--dry-run]`                        | Exact workspace-relative `.agents/...` directory chain                                                    | Initialized entrypoints and navigation effects                                                                             | Yes    |
| `route rebuild [path...] [--dry-run]`                                                      | Optional exact workspace-relative `.agents/...` boundaries                                                | Derived `Entries` and applicable exact-file formatter post-processing evidence                                             | Yes    |
| `extension list [--path <extension-source-directory>] [--state <state>]`                   | Selected embedded or external catalogue and workspace lifecycle state                                     | Available and installed Extension summaries                                                                                | No     |
| `extension inspect <id> [--path <extension-source-directory>]`                             | Exact Extension id, selected catalogue, and installed lifecycle state                                     | Package facts, declared targets, installed ownership, and current state                                                    | No     |
| `extension add [id...] [--path <extension-source-directory>] [--overwrite] [--dry-run]`    | Optional guided choice or exact ids from one selected catalogue                                           | Planned or applied Extension installation                                                                                  | Yes    |
| `extension update [id...] [--path <extension-source-directory>] [--overwrite] [--dry-run]` | Optional guided choice or exact installed ids from one selected catalogue                                 | Planned or applied managed reconciliation                                                                                  | Yes    |
| `extension remove [id...] [--dry-run]`                                                     | Optional guided choice or exact installed ids                                                             | Planned or applied safe removal                                                                                            | Yes    |
| `completion install [shell...] [--all] [--profile <absolute-path>] [--dry-run]`            | Optional guided selection, exact shells, or every safely detected supported target                        | Generated assets and marker-owned shell integration                                                                        | Yes    |
| `completion remove [shell...] [--all] [--profile <absolute-path>] [--dry-run]`             | Optional guided selection, exact shells, or every safely discovered owned target                          | Removed marker-owned shell integration and generated assets                                                                | Yes    |
| `completion script <shell>`                                                                | Exact supported shell                                                                                     | Generated completion script                                                                                                | No     |

Metadata means:

```text
[--description <text>] [--tag <tag>]... [--responsibility <text>]
```

`--tag` is repeatable and adds destination metadata. `find` deliberately uses `--has-tag` because filtering existing metadata is a different operation from authoring metadata.

## Operation Prerequisites

Framework presence is not a universal command gate. The
[operation prerequisite contract](contracts/operation-prerequisites.md) maps
all nineteen leaves to their exact workspace, route, lifecycle, source,
mutation, or shell evidence.

Handlers call those focused typed inspections directly. There is no generic
string-keyed capability registry or middleware chain. Missing state blocks only
the operation that actually requires it, while read-only consumers retain safe
partial evidence when their focused contract permits it.

## Direct Commands

### `status`

```text
open-forge status [--redact]
```

`status` answers mechanically knowable orientation questions:

- Which logical workspace was selected?
- Is an Open Forge canonical entry present?
- Is the exact `.agents/loader.md` recognizable?
- Does the workspace appear uninstalled, installed, incomplete, or structurally unrecognizable?
- Which CLI build and embedded Framework payload fingerprints are in use?
- Is managed Framework or Extension lifecycle state present?
- How many Framework targets are current, changed, missing, newly
  supplied, retired, or deliberately excluded?
- Is Git available and are relevant Open Forge paths currently clean?
- Are adjacent Open Forge backups or visible residuals present?
- Which operations, if any, are useful to consider from the detected state?

The [Status Result Contract](contracts/status-results.md) owns the exact typed
data, shallow inspection boundary, semantic statuses, deterministic advisory
suggestions, and share-safe exposure behavior. `suggestions` is always present
and may be empty. A suggestion names one typed domain operation and reason; it
contains no executable, argv, inferred parameters, or authority.

An operation performs deterministic steps that are already inside its contract
and authority. It suggests another operation only when that operation has a
different purpose or authority boundary, such as read-only `status` suggesting
mutating `install`. If required semantic input is unknown, status explains the
missing choice instead of fabricating a runnable command.

Framework presence uses one shallow named classification.
[`status-values.ts`](../../../../../src/cli/commands/status/status-values.ts) is
authoritative for the exact `FrameworkPresenceState` values, derived type, and
import path.

| State         | Meaning                                                                                                                                         |
| ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| `uninstalled` | No recognizable root installation anchor is present                                                                                             |
| `installed`   | The canonical Open Forge root entry and loader are recognizable                                                                                 |
| `incomplete`  | At least one root installation anchor is recognizable, but the required entry-and-loader pair is missing or malformed                           |
| `conflicting` | A required root target contains incompatible unowned content or an unsafe filesystem kind that prevents reliable classification or installation |

Arbitrary routed files, an Extension payload, or a nested `.agents` fragment do not make their containing directory an incomplete installation. For example, `src/open-forge` is installed because it contains the recognizable canonical pair, while `src/extensions/development-toolkit/payload` remains uninstalled as a workspace even though it is a valid detached Extension payload.

`installed` establishes recognizable presence, not health or managed ownership. `status` reports lifecycle state separately and returns `attention` for incomplete or conflicting presence. `doctor` owns complete health findings.

Presence is orientation evidence, not a universal permission gate. Every
operation checks only its declared prerequisites. In particular, a rooted
Route read requires a recognizable loader inventory, while `route rebuild
<path>` can operate on a detached payload selected as the workspace and
requires only its explicit workspace-relative `.agents/...` source.

It does not run the complete diagnostic suite, infer a semantically active scope, summarize project meaning, or mutate anything. `doctor` owns health. `context` owns routed bodies.

Status performs shallow exact-anchor and lifecycle-state inspection. It does not build the complete route or local-reference inventory and never enumerates unrelated workspace files.

Framework reconciliation counts inspect only the embedded payload, recorded
Framework paths and exclusions, and their exact current targets. They do not
build a mutation plan or imply replacement authority. `doctor` identifies
problem details; `install --dry-run` projects exact effects.

A human result should resemble:

```text
Workspace: D:/work/example
Framework: installed
Entry:     AGENTS.md
Loader:    .agents/loader.md
Payload:   embedded in Open Forge CLI 0.x
Health:    not checked
Managed:   20 current, 1 changed
Excluded:  patterns
Git:       clean
Backups:   none
Suggestions: none
```

Exact labels may evolve with the display adapter. The facts and distinctions do not.

`--redact` changes exposure only. Status performs the same inspection and
returns the same semantic status, suggestions, messages, and process
completion. Before terminal or JSON rendering, an exhaustive typed projection
reconstructs share-safe data and messages; this is neither a logging mode nor
a best-effort string scrub.

### `context`

```text
open-forge context [route...] [--heading <heading>]
```

With no route argument, `context` returns the baseline and continuity set required at task entry or resume. It follows the canonical entry, loader, visible #HistoricalLoadNow closure, every routed #KeepInMind result, visible #HistoricalLoadNow descendants of those results, and applicable overwrite companions in deterministic order.

Each explicit `route` adds that selected route, its applicable parent chain,
its overwrite companion, and visible #HistoricalLoadNow descendants. A #KeepInMind
result likewise adds any otherwise-unloaded parent chain required to establish
its scope and inherited Axioms. Multiple selected scopes remain distinct in
provenance rather than being merged into synthetic authority. Ordinary outgoing
links are checked and reported but do not load their target bodies.

Every emitted source includes:

- Canonical workspace-relative path.
- Route identity where applicable.
- Why it loaded, such as baseline, continuity, selected target, ancestor, or overwrite.
- Order in the effective result.
- Base and overwrite relationship.
- Complete requested body.

`--heading <heading>` projects the exact named Markdown heading from every source. It does not change loading. A missing heading remains visible as a coded finding rather than silently removing the source.

Human output uses clear source boundaries:

```text
Workspace: <workspace>\open-forge
Selected by: current directory

Context: baseline + directives
Sources: 2

1. .agents/loader.md
   Reason: baseline
   Route: loader

   <complete requested body>

2. .agents/directives/_directives.md
   Reason: selected ancestor
   Route: directives

   <complete requested body>
```

`context` does not perform semantic search, infer which on-demand route matches an unstated goal, or truncate bodies without making the truncation an explicit result condition.

### `find`

```text
open-forge find [route...] [--has-tag <tag>]...
```

`find` returns routed metadata, not file bodies.

With no route argument, search the selected workspace's routed inventory. Route arguments bound the search to explicit routes and their routed descendants. Multiple bounds form one deterministic union, and canonical route identity removes duplicates.

Every repeated `--has-tag` is an exact required tag. Multiple filters use AND semantics. Tag spelling remains exact rather than fuzzy or case-corrected.

Each match exposes:

- Route identity.
- Workspace-relative path.
- Description.
- Optional responsibility.
- Effective tags.
- Loading classification.
- Relevant parent route.

Human output should resemble:

```text
Workspace: <workspace>\open-forge
Selected by: current directory

Matches: 2

directives/open-forge/cli/cli-interface-consistency
  Path: .agents/directives/open-forge/cli/cli-interface-consistency.md
  Tags: LoadNow, Directive, CLI, Interface
  Description: Keep the Open Forge CLI explicit, orthogonal, predictable, helpful, and free of context-dependent command or flag behavior

memory/crystallized/documents/cli/interface
  Path: .agents/memory/crystallized/documents/cli/interface.md
  Tags: Memory, Document, CurrentTruth, Evergreen, CLI, Interface
  Description: Accepted replacement CLI command inventory, arguments, flags, interaction states, outputs, guidance, and lifecycle behavior
```

An empty match set is successful and explains the searched boundary. `route list` remains the structural tree view. `find` remains the flat exact-filter view.

### `doctor`

```text
open-forge doctor
```

Bare `doctor` always inspects the complete applicable Open Forge surface of the selected workspace and never writes. It does not recursively scan every workspace file.

Diagnosis starts from the exact workspace entry and loader, the authored route
topology, embedded Framework targets, `.agents/open-forge.json`, adjacent
Open Forge backups, and visible residual state. It then follows contained local
references reachable from known Markdown sources. An unrelated file is outside
diagnosis until routing, a reference, lifecycle evidence, recovery state, or
another explicit contract identifies it.

It validates every mechanically checkable Framework and lifecycle contract that the running CLI understands, including:

- Canonical entry and loader reachability.
- Routed entrypoint shape and generated navigation.
- Frontmatter and reserved tag structure.
- Route identity, scope, overwrite, and managed-route invariants.
- Local Markdown references.
- Managed Framework and Extension lifecycle records.
- Git readiness, adjacent `.bak` evidence, temporary residuals, and lifecycle disagreement.
- Containment, collisions, and derived-state consistency.

Reference traversal records canonical physical real paths, so repeated links
and cycles terminate. Exact file identifiers enrich alias findings only after
the runtime and filesystem prove their precision. Local links may leave
`.agents` while remaining inside the selected workspace. HTTP and HTTPS
destinations are identified as external references but are never fetched or
declared valid.

The result classifies findings by stable code, named severity, typed subject,
evidence, resolution class, and next actions. Resolution is one of safe repair,
manual decision, blocked repair, or informational. Severity never implies
fixability.

The [diagnosis and repair contract](contracts/diagnosis-and-repair.md) owns
explicit domain composition, completeness, stable ordering, and repair
proposal behavior. Codes identify output evidence and never resolve a check or
fixer.

Diagnosis that completes but finds warning or error conditions returns
`attention`. A required boundary that cannot be inspected safely returns
`blocked` with clearly labelled partial evidence. Unexpected diagnostic
execution failure returns `failed`.

### `repair`

```text
open-forge repair [--dry-run]
```

`repair` is the complete mutation counterpart to `doctor`. It targets the whole selected workspace and means every mechanically safe fix. There is no `--all`.

The operation:

1. Runs the complete diagnostic inventory.
2. Classifies safe-repair, manual-decision, blocked-repair, and informational findings.
3. Plans every safe repair through direct typed domain planners.
4. Revalidates preconditions.
5. Applies one recoverable plan unless `--dry-run` is set.
6. Reruns complete diagnosis after application.
7. Reports fixed, remaining, manual-decision, and blocked-repair findings.

Repair may rebuild derived navigation, normalize mechanically canonical metadata, or fix a local reference only when the correction is unique and meaning-preserving. It never guesses authored meaning, ownership, lifecycle intent, or an ambiguous target.

All ordinary safe repairs form one plan. Duplicate targets with the same exact
result may coalesce only through an explicit rule. Incompatible target
expectations block before the first write; order never chooses a winner.

An adjacent backup, temporary residual, incomplete lifecycle write, or mixed
post-stop state is safe-repairable only when one exact result is mechanically
proven and explicitly authorized. Otherwise repair changes nothing and reports
the exact manual comparison, Git command, backup, or rerun path. Workspace
repair never treats Git history as hidden permission or guesses recovery for an
exact-external Completion target.

### `create`

```text
open-forge create <destination.md> \
  [--template <reference>] \
  [--description <text>] \
  [--tag <tag>]... \
  [--responsibility <text>] \
  [--dry-run]
```

`create` writes one new Markdown file at an exact workspace-relative `.agents/...` destination. The optional leading `./` has no semantic effect. The parent must already be routed. The command never overwrites an existing file and never creates missing route topology implicitly.

The destination path does not select an artifact kind. Placement under an existing route and the resulting authored body establish whether the file acts as a Directive, Pattern, Workflow, Memory record, ordinary routed document, or another supported role.

When `--template` is omitted, the generic scaffold is:

```yaml
---
open-forge:
  description: Humanized file stem
  tags: [NeedsAuthoring]
---
# Humanized File Stem
```

Explicit metadata replaces the corresponding fallback. `responsibility` remains absent unless supplied. `NeedsAuthoring` is ordinary searchable metadata, not a reserved loading, authority, or validity tag. The scaffold may remain incomplete for the destination's role-specific content contract. The result returns `attention` because generated fallback meaning or unresolved placeholders still require authoring, and validation reports any additional structural requirement.

`--template` accepts two non-overlapping forms:

| Reference                                       | Meaning                                                      |
| ----------------------------------------------- | ------------------------------------------------------------ |
| `directive`, `directive.md`, or `_directive.md` | Exact normalized natural-name match through routed Templates |
| `./temp-templates/directive.md`                 | Exact custom workspace-contained Markdown file               |

Natural-name normalization removes one optional leading underscore and one optional `.md` suffix. It does not perform fuzzy matching. Missing or ambiguous matches block and list exact candidates.

A custom path must begin with `./`, resolves from the selected workspace, and remains contained there. The CLI does not guess between a name and a path from file existence. Absolute paths, parent traversal, and sources outside the workspace are invalid.

Template frontmatter is discarded. The destination always receives destination-owned frontmatter from explicit metadata and deterministic fallbacks. The Template body is instantiated, affected generated navigation is rebuilt, and the result reports Template provenance. The destination has no update or ownership relationship with the Template after creation.

If the parent route is missing, the operation blocks and suggests:

```text
open-forge route init <parent-directory>
```

The [Template instantiation boundary Pattern](../../../../patterns/open-forge/cli/commands/template-backed-creation.md) owns the reusable implementation shape used when `--template` is present.

### `install`

```text
open-forge install [--restore <route...>] [--overwrite] [--dry-run]
```

`install` creates or reconciles the complete Framework payload embedded in the
running CLI. That payload contains the root integration, Core, and Memory as
one coherent version. The command accepts no item, category, or file selection
because Open Forge does not offer partial Framework installation as a product
mode.

First installation plans the complete payload. Interactive use explains the
workspace, Git state, formatting, effects, verification, and recovery before
confirmation. Non-interactive first installation requires `--yes`; the
deterministic target remains the same complete payload.

Later installation reconciles the workspace against the running CLI's payload:

| Condition                                               | Default behavior                                      |
| ------------------------------------------------------- | ----------------------------------------------------- |
| Absent unoccupied Framework target                      | Create it                                             |
| Current managed target                                  | Report a no-op                                        |
| Embedded payload changed, local bytes unchanged         | Replace it after plan confirmation                    |
| Managed target changed locally                          | Preserve it unless explicitly authorized after review |
| Unowned occupied target                                 | Block without claiming it                             |
| Previously managed target missing                       | Ask `RESTORE` or `KEEP REMOVED`                       |
| Recorded target retired from the payload                | Ask `DELETE` or `KEEP`                                |
| New payload target outside a deliberately removed route | Add it                                                |

`KEEP REMOVED` persists deliberate absence in the reviewable workspace record
so the same route is not repeatedly restored and future payload additions
inside that excluded route remain absent. Interactive installation lists every
persisted exclusion as a literal `KEEP REMOVED` or `RESTORE` toggle and defaults
to `KEEP REMOVED`.

`--restore <route...>` is the deterministic spelling for re-enabling one or
more exclusions. Its space-separated values must exactly match persisted
exclusion identities. Each selected exclusion is removed from final workspace
state only after its current Framework route and Framework-supplied subtree
have been reconciled successfully. Unknown routes, routes that are not exact
persisted exclusions, and descendants covered only by a broader exclusion are
invalid. The flag neither selects a partial initial payload nor grants
replacement or other safety authority.

`KEEP` preserves retired content, releases Framework management, and makes the
resulting ownership change visible.

Interactive approval or `--overwrite` permits only the reported eligible
replacement of divergent Framework-managed files. Neither claims unowned
content, overwrites a user-owned `.overwrite.md` companion, restores a kept
removal, broadens the embedded payload, or bypasses containment, source
validation, formatter trust, Git policy, and recovery preflight.

Payload writes, generated navigation, and `.agents/open-forge.json` form one
complete primary plan. The workspace record is the
last persistent effect. `status` provides the shallow installation summary,
`doctor` explains problems, and `install --dry-run` returns the exact proposed
reconciliation plus non-executed formatter post-processing evidence.

After successful application, `install` ends with its one verified Framework
result. It never creates a Git commit or starts another command wizard. Human
output gives exact review and commit guidance when Git is available and lists
typed advisory next operations such as `extension.add` and
`completion.install`. Structured output exposes the same suggestions without
fabricating argv, commit messages, or inferred subjects.

Gitless and `--skip-git-check` journeys explain their weaker review boundary.
Every journey keeps review, commit, Extension selection, and Completion
installation as deliberate follow-up actions rather than hidden continuation
inside `install`.

## Route Family

Routes are visible navigation paths through entrypoints. `route list` and
`route inspect` accept one natural exact identity such as
`directives/open-forge/cli/cli-interface-consistency`; they do not accept `.agents` paths,
entrypoint filenames, optional `.md`, leading slashes, or case correction as
alternate forms. Results report the canonical workspace-relative source
separately.

`route init` and `route rebuild` operate on physical topology and therefore
accept exact workspace-relative `.agents/...` paths, with an optional leading
`./`. To operate on another Framework
source or Extension payload, select that containing directory with
`--workspace`; the path grammar does not change. This separation prevents one
argument from being reinterpreted as a route identity or filesystem path.

`loader` is the root identity. Each later segment is the exact routed file stem
or child folder slug. An ordinary file and child entrypoint with the same exact
segment are ambiguous and neither is selected until repaired.

The CLI does not expose a global `--scope` flag because scopes may occur
recursively at several positions in one route.

### `route list`

```text
open-forge route list [route] [--depth <count>]
```

Without a route, list root routes. With a route, list its direct entries. `--depth` expands descendants to a positive bounded depth and defaults to `1`.

Results contain route identity, description, tags, source, and child count
without loading bodies. Ordering follows the runtime-independent authored route
projection. A stale generated order is an `attention` finding rather than a
second ordering authority. There is no unbounded `--recursive` or
context-dependent `--all`.

### `route inspect`

```text
open-forge route inspect <route...>
```

Inspect one or more exact routes and return:

- Canonical route identity and workspace-relative source.
- Physical containment identity where relevant.
- Parent and entrypoint chain.
- Description, responsibility, and tags.
- Direct entries and explicit outgoing local and external references.
- Loading classification.
- Base and overwrite pairing.
- Declared manager relationship.
- Existing route-specific findings.

The command reports established facts. It does not infer semantic relevance or claim that an ordinary slug is a scope unless routed structure makes that role concrete.

All direct, Route, and diagnostic consumers share the
[route inventory contract](contracts/route-inventory.md). They do not maintain
private metadata or topology parsers.

### `route init`

```text
open-forge route init <path> \
  [--description <text>] \
  [--tag <tag>]... \
  [--responsibility <text>] \
  [--template <reference>] \
  [--dry-run]
```

Initialize one exact directory chain beneath the selected workspace's explicit `.agents` boundary. Every later directory segment becomes routable through its canonical `_{folder}.md` entrypoint. Missing directories and entrypoints are planned together, existing valid entrypoints are preserved, and every affected generated `Entries` region is rebuilt.

For example:

```text
open-forge route init ./.agents/team/backend/api
```

plans `.agents/team/_team.md`, `.agents/team/backend/_backend.md`, and `.agents/team/backend/api/_api.md` when all three are absent. It also plans every available parent navigation change required to expose the new chain.

The path begins with `.agents` or `./.agents`; both normalize to the same canonical path. Absolute paths, parent traversal, another content root, physical escape, incompatible existing nodes, ambiguous entrypoints, and any unsafe segment block the complete plan before writes. The command never creates an `_.agents.md`; a recognizable `.agents/loader.md` remains the canonical rooted inventory boundary.

A Framework source or Extension payload becomes the selected workspace through `--workspace` or the current working directory. Its same workspace-relative `.agents/...` path may be initialized without classifying it as an installed Framework. When no local loader or exposing parent exists, the result identifies the initialized tree as detached instead of inventing a parent or root route.

Metadata and `--template` apply only to the final entrypoint. Each generated intermediate entrypoint receives canonical frontmatter with its exact folder slug as placeholder `description`, `NeedsAuthoring` as its tag, no `responsibility`, a deterministic heading, and a generated `Entries` region. The final entrypoint uses supplied metadata and Template body; omitted metadata uses the same fallback.

Template frontmatter is discarded. A Template contributes authored body only to the final entrypoint and cannot supply or replace generated-region markers. The CLI owns the final canonical `Entries` region and rejects a Template whose body conflicts with that boundary.

If the complete chain already exists and no final authoring input was supplied, the command succeeds without effects and reports that it was already initialized. If the final entrypoint exists while metadata or a Template was supplied, the command blocks rather than silently ignoring input or overwriting authored content.

Any generated fallback meaning produces an `attention` result and remains searchable through `NeedsAuthoring`. The directory, entrypoint, Template, generated-navigation, verification, and recovery effects form one complete preflighted mutation plan.

### `route rebuild`

```text
open-forge route rebuild [<path>...] [--dry-run]
```

With no arguments, use the fixed `.agents/loader.md` boundary. A missing
loader blocks this form; the CLI never searches for another root.

Every explicit argument begins with `.agents` or `./.agents`; both normalize to the same canonical path. It may name that directory, a routed directory with its canonical entrypoint, an exact entrypoint, or an ordinary routed Markdown file. Absolute paths, parent traversal, another content root, sources outside the selected workspace, physical escapes, and filesystem-based correction are invalid.

Examples:

```text
open-forge route rebuild ./.agents/memory
open-forge --workspace ./src/open-forge route rebuild ./.agents/loader.md
open-forge --workspace ./src/extensions/development-toolkit/payload route rebuild ./.agents
```

The latter two forms select detached source payloads through explicit workspace boundaries. The command inspects only the named payload's `./.agents` tree and does not infer Framework installation from the outer repository.

An explicit route or detached source identifies a source boundary rather than requiring the caller to identify its generated-region owner:

- A routed ordinary file rebuilds the direct `entrypoint` that exposes it.
- Every selected `entrypoint` rebuilds itself, its complete routed subtree, and its direct parent when one exists.
- Overlapping selections are deduplicated, so each generated region is planned at most once.

The direct parent is included because its generated entry reflects the selected `entrypoint`'s `description` and tags. Higher ancestors do not depend on that metadata. A detached tree rebuilds only owners and parents present in that tree; it never invents a missing destination parent.

The operation owns changes to derived `Entries` bodies only. Separately
reported workspace formatter post-processing may normalize the complete
touched entrypoint after rebuilding under the accepted formatting contract; it
does not grant the route
planner authority to rewrite authored meaning, overwrite companions, unrelated
frontmatter, or `.agents/open-forge.json`.

Handled application or verification failure restores already applied regions in reverse order. A hard process stop may leave a mixture of complete old and new generated regions, but never a partially written target. Rerunning the command recomputes the selected derived closure. `route rebuild` keeps no persistent crash journal.

## Extension Family

Extensions are optional installation and ownership units. Their installed
files retain ordinary Framework meaning without the CLI. The accepted
lifecycle remains deliberately small. Dependencies use the flat exact-id
graph defined by the [managed lifecycle contract](contracts/managed-lifecycle.md).

### `extension list`

```text
open-forge extension list [--path <extension-source-directory>] [--state <state>]
```

List the union of the selected catalogue and installed lifecycle records with
id, source kind, availability, installed state, dependency summary, ownership
state, and available next operation. Without `--path`, the selected catalogue
is embedded in the running CLI. With `--path`, it is the exact external source
directory. Installed ids absent from the selected catalogue remain visible as
`installed-only`. `--state` accepts `not-installed`, `installed`, `changed`,
`missing`, `dependency-only`, or `installed-only`. These are exact independent
predicates, so one Extension may match several values across separate
invocations. The [managed lifecycle contract](contracts/managed-lifecycle.md)
owns their complete meaning.

### `extension inspect`

```text
open-forge extension inspect <id> [--path <extension-source-directory>]
```

Return selected-catalogue package facts and installed lifecycle facts for one
id. An id may be catalogue-only, installed-only, or both. When available,
include source classification, manifest facts, dependencies, and declared
canonical payload targets. When installed, include recorded `open-forge` or
`external` classification, owned targets, advisory checksums, and current,
changed, or missing state. An external source location is never persisted.
`inspect` does not speculate about replacements, collisions, deletions,
formatting, or derived navigation. Exact mutation effects belong only to
`extension add --dry-run` or `extension update --dry-run`.

### `extension add`

```text
open-forge extension add [id...] [--path <extension-source-directory>] [--overwrite] [--dry-run]
```

Without ids in an interactive terminal, open a multi-selection wizard over the
selected catalogue. Without `--path`, that catalogue is embedded in the CLI;
with `--path`, it is the exact external source directory. Explicit
space-separated ids bypass the selection wizard. Non-interactive execution
requires at least one explicit id.

If the selected catalogue has no eligible Extension, the wizard returns a
successful no-op and explains why. Leaving without a selection returns
`cancelled`; it never guesses or reports a successful add.

An external source directory is interpreted deterministically:

```text
<source>/extension.json
  -> one-package catalogue

<source>/<package>/extension.json
  -> immediate-child multi-package catalogue
```

When the root manifest exists, the source is the direct package and child
catalogue discovery does not run. Otherwise, only immediate child directories
with manifests are candidates; discovery never recurses. Manifest ids, not
folder names, establish identity. Duplicate ids, invalid manifests, an empty
source, or an explicit unknown id is invalid. The resolved path is used only
for this invocation and never written to `.agents/open-forge.json`. A relative
source path resolves from the process current working directory, independently
of `--workspace`.

The selected catalogue classifies installed additions as `open-forge` when
embedded and `external` when selected through `--path`. IDs remain globally
unique across installed Extensions. The CLI never guesses whether an id token
is a filesystem path.

Before an external source contributes to a plan, inspect the complete selected
package and dependency closure for concealed or active source constructs.
Reviewable HTML comments, collapsed or indirect content, and source-only
destinations are shown as escaped `path:line:column` evidence and require a
dedicated interactive source-review decision. Active content, concealment,
unsafe controls, invalid text, or an uninspectable source block the complete
operation. Explicit ids,
`--yes`, `--overwrite`, and `--skip-git-check` do not bypass this boundary. The
complete behavior is owned by the [external source review
contract](contracts/source-review.md).

Add plans the complete exact-id dependency closure, targets, ownership,
collisions, generated navigation, verification, and recovery before
installation. Missing dependencies resolve only from the selected catalogue;
already-installed exact ids are reused without hidden reconciliation or source
switching. It blocks an already managed selected identity and suggests
`extension update`.

An existing unowned ordinary file at a payload target is an explicit ownership collision whether its bytes are equal or divergent. Interactive use shows the complete collision set, current and proposed ownership, advisory checksums, replacement effects, and future update/removal consequences, then asks for approval. Non-interactive and JSON use require `--overwrite`; without it, complete preflight blocks before any write.

Approval or `--overwrite` authorizes only eligible ordinary-file collisions already present in the selected package plan. It permits replacement and transfer into the Extension's recorded ownership. It never broadens package selection, replaces an incompatible directory or special node, bypasses containment, claims another manager's incompatible target, or writes a user-owned `.overwrite.md` companion or protected control path. Equal-byte takeover records ownership without manufacturing a content change, but remains explicit because later update and removal authority changes.

Framework and Extension ownership never overlap. Several Extensions may own one
path only when their proposed bytes are byte-identical. A divergent managed
target blocks the complete lifecycle mutation and cannot be authorized by
`--overwrite`. Readable interactive presentation may offer `EXPORT INCOMING` as
a next action. Choosing it rebuilds an export-only plan that appends `.incoming`
to each complete target filename, creates only absent proposed user-owned
sidecars, and returns attention without installing or updating the package. The
sidecars are neither managed nor routed and are never replaced. Open Forge
never automatically merges authored payloads; the user resolves the source
conflict and reruns the operation.

Generated `Entries` regions are derived state, not authored collisions and not Extension-owned bytes. Add projects the complete assembled destination, includes every affected generated-region replacement in the same preflighted plan, applies those rebuild effects automatically after payload effects, and verifies route availability before committing lifecycle state. A rebuild failure fails and recovers the complete Extension mutation.

### `extension update`

```text
open-forge extension update [id...] [--path <extension-source-directory>] [--overwrite] [--dry-run]
```

Update reconciles already managed Extensions against one selected catalogue.
Without ids in an interactive terminal, open a multi-selection wizard over the
installed ids available from that catalogue. Explicit space-separated ids
bypass subject selection. Non-interactive execution requires explicit ids.

Without `--path`, all selected ids resolve from the embedded catalogue. With
`--path`, they resolve from the exact external catalogue or direct package
source. One catalogue therefore serves the complete batch without positional
subject-to-source pairing. An installed id absent from the selected catalogue
is ineligible and produces a useful source-selection error. The external path
is supplied again because it is never persisted.

When no installed Extension is eligible, the interactive omitted-id form returns a successful no-op with an empty selected set and explains that there is nothing to update. It does not manufacture an invalid request or suggest adding an unrelated package.

Update applies the same bounded collision evidence and authorization policy as
add. Changed managed targets and newly introduced unowned collisions remain
preserved unless the interactive user approves the complete eligible set or
the invocation supplies `--overwrite`. Dropped files receive literal `DELETE`
or `KEEP` decisions; keeping releases the departing owner and preserves the
file. New dependencies join the same update plan. A dropped relationship that
leaves a dependency-only Extension unused makes that Extension a visible
orphaned removal candidate. External update repeats source inspection for the
exact newly supplied closure; a prior install or review never becomes durable
source trust. It never becomes implicit add, source replacement, or authority
over an ineligible target. Every successful update rebuilds and verifies
generated navigation from the assembled destination before writing workspace
lifecycle state, then runs selected formatter post-processing.

### `extension remove`

```text
open-forge extension remove [id...] [--dry-run]
```

Remove safely detaches explicitly selected managed Extensions. In an interactive terminal, omitted ids open a wizard over installed Extensions. Non-interactive execution requires explicit ids.

When no managed Extension is installed, the interactive omitted-id form returns a successful no-op with an empty selected set and explains that there is nothing to remove.

The plan protects changed content, shared ownership, retained dependents,
generated route reachability, unowned files, and user-authored overwrite
companions. It blocks removal of an id required by a retained Extension.
Dependency-only Extensions made orphaned by the selected removal default to a
visible `REMOVE` decision that may be changed to `KEEP INSTALLED`; keeping one
makes it directly requested. Exclusive files receive literal `DELETE` or
`KEEP` decisions; keeping releases Extension management and leaves user-owned
content. Shared files lose only the selected owner. Remove never recursively
removes retained dependents or deletes content whose authority remains
unresolved. Every successful removal rebuilds and verifies navigation, writes
`.agents/open-forge.json` last, and then runs selected formatter
post-processing.

The replacement surface has no separate `extension adopt`, `extension restore`, or `extension scaffold`. Eligible ownership transfer is already an explicit collision resolution inside add or update; the other standalone operations do not currently solve a sufficiently common agent or user job with accepted semantics. A future Extensions overhaul may earn another explicit operation from evidence.

## Completion Family

Supported initial shells are:

- Bash
- Zsh
- Fish
- PowerShell

### `completion install`

```text
open-forge completion install [shell...] [--all] [--profile <absolute-path>] [--dry-run]
```

Install completion for one or more supported shells. Omitted shells open an
interactive multi-select with every safely detected supported target selected
by default. Explicit space-separated shells bypass selection. `--all` selects
every safely discoverable target without bypassing plan review or safety.
Non-interactive execution requires explicit shells or `--all`.

Default targets need no flags. `--profile` selects one exact nonstandard
absolute startup profile and is valid only with one explicit Bash, Zsh, or
PowerShell shell. It is invalid with omitted or multiple shells, `--all`, or
Fish.

Fish uses its dedicated per-user completion file. Bash, Zsh, and PowerShell
use one generated Open Forge asset plus one minimal marker-owned activation
block in the exact default or custom profile. An absent exact selected profile
is a visible create effect. Installation never scans for or edits additional
startup files.

The complete target, ownership, symlink, divergence, mutation-order, and
recovery behavior is defined by the [Completion lifecycle
contract](contracts/completion-lifecycle.md).

### `completion remove`

```text
open-forge completion remove [shell...] [--all] [--profile <absolute-path>] [--dry-run]
```

Remove only proven Open Forge-owned completion integration. Guided selection
preselects every safely discovered owned installation. Explicit shells,
`--all`, custom profile selection, and non-interactive behavior match install.
A missing owned installation is a successful no-op. Divergent intact ownership
requires an explicit interactive remove-or-keep decision; malformed ownership
blocks.

### `completion script`

```text
open-forge completion script <shell>
```

Write the generated completion script for one explicit shell. Human mode writes the script to stdout. JSON mode returns the same script as structured data.

Completion provides static command, flag, alias, and finite-choice candidates.
It also provides bounded dynamic Route, Template, persisted Framework
exclusion, and Extension id candidates
through the [Completion protocol
contract](contracts/completion-protocol.md). General paths remain shell-owned.
Suggestions perform no mutation, grant no authority, and do not replace normal
command validation.

Completion is user-level state and does not select a workspace. Supplying
`--workspace` or `--skip-git-check` to a Completion command is invalid rather
than silently ignored. Interactive Framework installation ends with the exact
`open-forge completion install` reminder but never performs Completion effects
inside the Framework operation.

## Guided And Non-Interactive Behavior

Interactive behavior follows the
[request construction contract](contracts/request-construction.md) as a
presentation over the same operation:

1. Detect mechanically knowable facts.
2. Explain the selected target and consequences.
3. Ask only for missing input that can materially change the request.
4. Present documented defaults.
5. Show the inspectable plan.
6. Request confirmation when warranted.
7. Call the ordinary handler.
8. Render the ordinary result.

Terminal detection may decide whether a wizard is available. It never changes operation semantics.

`--json` is non-interactive. A JSON invocation never emits prompts or progress. Missing semantic input without a documented accepted default returns `invalid`; missing workspace evidence or required authority for otherwise complete intent returns `blocked`.

`--yes` accepts documented defaults and ordinary confirmations. If a material semantic choice has no safe default, the request remains invalid until the caller supplies it. Missing collision or safety authority remains blocked.

The complete request contains only selected semantic intent. Resolved
workspace, preview or apply, default acceptance, collision authority,
cancellation, terminal availability, and presentation remain typed context or
policy around it. Exact plan confirmation happens after complete preflight; a
changed choice discards that plan and starts request resolution again.

## Results And Presentation

Handlers return typed values. They do not call `console`, write streams, choose human or JSON presentation, or set process state.

### Named Protocol Values

Protocol-significant strings and numbers are defined once through enums or readonly `as const` objects. Application code never repeats raw schema versions, operation identifiers, statuses, message levels, diagnostic codes, or exit values.

[`result.ts`](../../../../../src/cli/result/result.ts) is authoritative for the
exact implemented result schema version, statuses, message levels, operation
identifiers, message shape, common result envelope, and import paths. The public
envelope currently uses schema version `1`. Message levels are `info`,
`warning`, and `error`. They classify the message and do not independently
choose process completion.

The accepted semantic statuses are:

| Status      | Meaning                                                                            |
| ----------- | ---------------------------------------------------------------------------------- |
| `success`   | The selected operation completed correctly without unresolved conditions           |
| `attention` | The operation completed correctly, but findings or incomplete authored work remain |
| `invalid`   | The invocation does not identify a valid request                                   |
| `blocked`   | A valid request cannot proceed safely under current facts or authority             |
| `failed`    | Execution, verification, or recovery did not complete correctly                    |
| `cancelled` | The caller cancelled before successful completion                                  |

### Operation Identifiers

Domain operation identifiers mirror the command tree with dot-separated family paths. Parser-level behavior has an explicit `cli` namespace:

| Operation class          | Public identifiers                                                                             |
| ------------------------ | ---------------------------------------------------------------------------------------------- |
| CLI boundary             | `cli.help`, `cli.version`, `cli.parse`, `cli.failure`                                          |
| Direct domain operations | `status`, `context`, `find`, `doctor`, `repair`, `create`, `install`                           |
| Route family             | `route.list`, `route.inspect`, `route.init`, `route.rebuild`                                   |
| Extension family         | `extension.list`, `extension.inspect`, `extension.add`, `extension.update`, `extension.remove` |
| Completion family        | `completion.install`, `completion.remove`, `completion.script`                                 |

[`result.ts`](../../../../../src/cli/result/result.ts) is authoritative for
their exact const object, derived types, and import path. This table is the
public semantic reference for identifiers emitted in structured results.

`cli.parse` covers an invalid invocation for which no domain operation could be selected. `cli.help` covers bare root, group-local, and `--help` output. `cli.version` covers `--version`. `cli.failure` contains an unexpected failure at the executable boundary. These CLI-level identifiers do not add command leaves.

### Unexpected Failure

One outer safety boundary converts an unexpected exception into
`cli.failure` with `failed` status and the stable diagnostic code
`cli.unexpected-failure`. Its data contains exactly:

| Field                | Meaning                                                                                                 |
| -------------------- | ------------------------------------------------------------------------------------------------------- |
| `stage`              | `startup`, `operation`, or `presentation`                                                               |
| `attemptedOperation` | The selected operation id, or `null` when selection never completed                                     |
| `workspace`          | The selected `WorkspaceReference`, or `null` when workspace selection never completed or does not apply |

The public message is fixed, safe, and useful without including the caught
error text, stack, argv, physical path, environment, or internal state. The
failure is written through a minimal emergency terminal or JSON presentation
path that does not invoke the failed operation renderer. If even that final
write fails, the process still exits with the `failed` exit value and makes no
claim that a valid result was emitted.

A rendered completion has at most one non-empty final stream. The ordinary
writer registers temporary error observation before invoking `write`, retains
it through the callback and error-event pair, settles once, and then removes
the temporary listener. A callback failure followed by its paired error event
is one contained write failure, never an uncaught second exception. The writer
enters the minimal emergency path with the `failed` exit value. Emergency
output is a best-effort failure response outside the already failed completion;
it never claims that the original result was emitted completely.

### JSON Envelope

Every human and structured presentation consumes this small common result:

[`result.ts`](../../../../../src/cli/result/result.ts) is authoritative for the
exact implemented message and result interfaces, their generic constraints,
and their import paths.

The top-level fields have one responsibility:

| Field           | Contract                                                    |
| --------------- | ----------------------------------------------------------- |
| `schemaVersion` | Major version of the public result envelope                 |
| `operation`     | Stable selected domain or parser-level operation identifier |
| `status`        | Complete semantic outcome                                   |
| `messages`      | Stable coded explanations and useful next actions           |
| `data`          | Operation-specific typed payload                            |

Each message contains a stable `code`, one `info`, `warning`, or `error`
`level`, and safe human-readable `message`. A `suggestion` is optional and is
present only when the CLI can offer a truthful useful next action.

The envelope does not contain a derived `ok` boolean, numeric `exitCode`, timestamp, duration, working directory, argv copy, stack trace, or universal optional fields for operation-specific concerns. Workspace, provenance, findings, effects, and recovery evidence belong in the data type of operations that actually produce them.

Every workspace-aware operation includes one shared workspace reference in its operation-specific `data`:

[`workspace-reference.ts`](../../../../../src/cli/result/workspace-reference.ts)
is authoritative for the exact implemented workspace-selection values,
reference interface, derived type, and import path.

`root` is the resolved absolute logical workspace path. It is not a physical real path and does not expose internal containment evidence. `selectedBy` is `current-directory` when selection came from the exact current working directory and `workspace-flag` when it came from `--workspace`. These values come from the shared named object rather than repeated string literals.

Once workspace selection succeeds, the reference remains present in successful, attention, blocked, failed, and cancelled operation data. Parser-only results and operations that do not select a workspace do not manufacture one. Installation state, findings, plans, effects, and recovery remain focused operation data rather than fields on `WorkspaceReference`.

Human presentation shows the workspace before domain-specific facts:

```text
Workspace: <workspace>\open-forge
Selected by: current directory
Installation: installed
```

The shared workspace reference has this structured shape. The
[Status Result Contract](contracts/status-results.md) owns the complete status
payload rather than duplicating it here:

```json
{
  "workspace": {
    "root": "<workspace>\\open-forge",
    "selectedBy": "current-directory"
  }
}
```

After a complete plan exists, mutating operation data composes the shared
`mutation` projection defined by the [mutation execution
contract](contracts/mutation-execution.md#public-mutation-evidence). It exposes
one ordered effect array with logical targets, before and after evidence, and
final states. Human counts derive from that array rather than being stored as
parallel fields. Invalid or prerequisite-blocked results that never produce a
plan do not manufacture empty mutation evidence.

Mutation results never expose complete file contents, physical filesystem
paths, executor-temporary paths, or private rollback material retained by an
internal plan. Stable messages carry blockers and deterministic next actions.

`messages` is always an array. Every non-success result contains at least one message. Success may contain informational messages when they add value. Detailed findings remain in operation data rather than being flattened into a generic message bag.

Each operation or family defines its diagnostic-code const object in its
focused production source. The following future Doctor declaration is
schematic because Doctor domain production does not yet exist:

```ts
export const DoctorMessageCode = {
  unresolvedFindings: "doctor.unresolved-findings",
} as const;

export type DoctorMessageCode = (typeof DoctorMessageCode)[keyof typeof DoctorMessageCode];
```

Codes use stable lowercase dot-separated namespaces with kebab-case condition segments. Reuse moves a code object only to the nearest common scope of real consumers.

Increment the schema version only when an existing top-level field or accepted meaning changes incompatibly. Additive operation-data evolution may remain within the current envelope version when existing fields retain their meaning. The CLI emits one envelope version and does not add a schema-selection flag before a real compatibility need exists.

### Exit Mapping

The executable boundary maps the complete semantic status through one readonly object:

[`process-completion.ts`](../../../../../src/cli/result/process-completion.ts)
is authoritative for the exact implemented mapping, derived exit-code type,
and import path.

| Status      |  Exit | Process meaning                                                                                         |
| ----------- | ----: | ------------------------------------------------------------------------------------------------------- |
| `success`   |   `0` | The operation completed without unresolved conditions                                                   |
| `attention` |   `1` | The operation completed, but pipelines and continuous integration should stop for unresolved conditions |
| `invalid`   |   `2` | The invocation did not identify a valid request                                                         |
| `blocked`   |   `3` | A valid request could not proceed safely                                                                |
| `failed`    |   `4` | Execution, verification, or recovery failed                                                             |
| `cancelled` | `130` | A caught cancellation ended the operation before success                                                |

Diagnostic severity never chooses process completion independently. The result status does. Structured consumers branch on `status` and message codes rather than decoding the number.

Human output is concise, operation-aware, and explicit about subject, effect, provenance, important warnings, and next action. JSON emits exactly one versioned document on stdout. Prompts, progress, incidental logs, and stack traces never contaminate structured stdout.

The executable boundary alone selects the display adapter, receives its
union-typed at-most-one-stream output, writes the selected non-empty stream
when present, and applies `ExitCode`.

## Help And Error Behavior

The [Parser Result Contract](contracts/parser-results.md) owns exact
`cli.help`, `cli.version`, and `cli.parse` data, ordering, syntax-versus-semantic
ownership, stream placement, trailing newlines, and process completion.

Every group and leaf exposes local help containing:

- Purpose.
- Accepted invocation shapes.
- Arguments and flags.
- Defaults and material safety effects.
- Representative examples.
- Useful neighboring or next commands.

Behavior by state is:

| State             | Behavior                                                                             |
| ----------------- | ------------------------------------------------------------------------------------ |
| Bare `open-forge` | Show root help and succeed                                                           |
| Bare group        | Show group help and return `invalid`                                                 |
| `--help`          | Show selected help and succeed without domain execution                              |
| `--version`       | Show CLI version and succeed without domain execution                                |
| Unknown command   | Return `invalid`, show a likely correction when confidence is high, never execute it |
| Missing argument  | Name the missing value and show one valid invocation                                 |
| Empty read result | Succeed and explain the selected boundary                                            |
| Blocked mutation  | Explain the exact blocker, preserved state, and accepted next command                |
| Attention         | Explain completed work and every remaining condition                                 |

Parser-level meta behavior never selects a workspace or invokes a domain
handler. `--json` may accompany `--help` or `--version`; every other non-meta
flag conflicts with them. Bare root is successful help, while a bare group is
invalid help because no leaf operation was selected.

Typo suggestions are advisory only. The CLI never executes a correction automatically.

## Representative Journeys

### Agent Enters A Workspace

```text
open-forge status --json
open-forge status --json --redact
open-forge context --json
open-forge find --has-tag CurrentTruth --json
```

The agent receives stable orientation, an optional share-safe status document,
ordered bodies, and metadata matches without parsing decorative terminal
output.

### User Diagnoses And Repairs

```text
open-forge doctor
open-forge repair --dry-run
open-forge repair
```

Diagnosis is read-only. Preview and application expose the same safe repair plan.

### User Creates A Generic File

```text
open-forge create ./.agents/memory/working/release-notes.md \
  --description "Current release preparation notes" \
  --tag Memory \
  --tag Working
```

The exact destination owns its frontmatter and starts without a Template relationship.

### User Creates From A Routed Template

```text
open-forge create ./.agents/directives/release-safety.md \
  --template directive \
  --description "Protect release changes through explicit verification" \
  --tag LoadNow \
  --tag Directive
```

`directive` resolves naturally to `directive.md` or `_directive.md` through routed Templates. The source frontmatter is discarded.

### User Creates From A Custom Template

```text
open-forge create ./.agents/memory/working/release-notes.md \
  --template ./temp-templates/release-notes.md
```

The `./` prefix makes custom path selection explicit. Omitted semantic metadata remains visible as `NeedsAuthoring`.

### User Installs Or Reconciles The Framework

```text
open-forge install --dry-run
open-forge install
open-forge install --yes
open-forge install --restore patterns workflows --dry-run
open-forge install --overwrite --dry-run
```

The target is always the complete embedded Framework. `status` summarizes the
installation, `doctor` explains problems, and the dry run exposes exact
reconciliation before application. `--restore` re-enables only exact persisted
exclusions. Overwrite never broadens the payload or restores a deliberate
removal.

### User Manages An Extension

```text
open-forge extension add
open-forge extension add development-toolkit --dry-run
open-forge extension add development-toolkit
open-forge extension add --path ./team-extensions
open-forge extension add team-toolkit documentation-toolkit --path ./team-extensions --overwrite --dry-run
open-forge extension add team-toolkit --path ./team-extensions/team-toolkit
open-forge extension update development-toolkit
open-forge extension update development-toolkit documentation-toolkit
open-forge extension update team-toolkit documentation-toolkit --path ./team-extensions
open-forge extension remove development-toolkit --dry-run
```

Omitted ids open the operation's selection wizard. Space-separated ids bypass
selection. `--path` selects an external catalogue or direct package source
without adding another positional source-kind keyword.

### User Installs Shell Completion

```text
open-forge completion install
open-forge completion install bash zsh --dry-run
open-forge completion install --all --yes
open-forge completion install zsh --profile /home/me/dotfiles/.zshrc
open-forge completion remove --all --dry-run
```

Omitted shells open the multi-select wizard with all safely detected targets
selected. Explicit shells or `--all` keep automation deterministic. Completion
remains a separate operation even when Framework installation recommends it.

## Deliberately Excluded Surface

| Excluded surface                               | Reason                                                                                                             |
| ---------------------------------------------- | ------------------------------------------------------------------------------------------------------------------ |
| Root `help`                                    | Bare root, group help, and `--help` already provide one predictable discovery model                                |
| `plan <mutation...>`                           | `--dry-run` keeps preview attached to the selected operation and avoids a meta-command exception                   |
| `doctor --fix`                                 | Diagnosis remains strictly read-only                                                                               |
| `repair --all`                                 | Bare repair already means every mechanically safe fix                                                              |
| `route --list` or similar mode flags           | Operation flags would hide mutually exclusive leaf commands                                                        |
| `create <kind> <destination>`                  | Routed placement and content establish role, while explicit Template selection handles specialized starting bodies |
| Implicit Template matching                     | Destination spelling does not authorize a Template choice                                                          |
| Generic `sync` or `reset`                      | Neither word states authority, direction, or preservation policy                                                   |
| `--force` or `--pro`                           | Generic bypass vocabulary hides safety and ownership intent                                                        |
| Global `--scope`                               | Recursive route placement cannot be represented honestly by one inferred scope value                               |
| General network link validation                | HTTP and HTTPS validation is outside the Framework's trust and product boundary                                    |
| Automatic typo execution                       | A suggestion is not authorization                                                                                  |
| Framework file versions                        | The CLI build embeds one coherent payload and records only advisory exact-byte checksums                           |
| Framework item selection                       | The Framework installs as one coherent Core and Memory payload                                                     |
| `uninstall`                                    | Root harness and user-route ownership are not yet safe enough                                                      |
| Separate Extension adopt, restore, or scaffold | Eligible takeover belongs to add or update; the remaining standalone jobs are not accepted                         |

## Patterns

- [Predictable command surface](../../../../patterns/open-forge/cli/commands/predictable-command-surface.md)
- [Guided operation](../../../../patterns/open-forge/cli/commands/guided-operation.md)
- [Planned mutation](../../../../patterns/open-forge/cli/filesystem/planned-mutation.md)
- [Diagnostic domain slice](../../../../patterns/open-forge/cli/diagnostics/diagnostic-domain-slice.md)
- [Result and display boundary](../../../../patterns/open-forge/cli/commands/result-display-boundary.md)
- [Local reference inventory](../../../../patterns/open-forge/cli/markdown/local-reference-inventory.md)
- [Route inventory projection](../../../../patterns/open-forge/cli/markdown/route-inventory-projection.md)
- [Template instantiation boundary](../../../../patterns/open-forge/cli/commands/template-backed-creation.md)
- [Managed reconciliation](../../../../patterns/open-forge/cli/filesystem/managed-reconciliation.md)
- [Workspace formatter strategy](../../../../patterns/open-forge/cli/workspace/workspace-formatter-strategy.md)
- [Named TypeScript values](../../../../patterns/open-forge/typescript/named-values.md)

## Decisions And Rationale

- [Command surface](../../decisions/cli/cli-command-surface.md)
- [Interface consistency](../../decisions/cli/cli-interface-consistency.md)
- [Agent-first product contract](../../decisions/cli/cli-agent-first-product-contract.md)
- [Core job model](../../decisions/cli/cli-core-job-model.md)
- [Result and display boundary](../../decisions/cli/cli-result-display-boundary.md)
- [Doctor and repair contract](../../decisions/cli/cli-doctor-repair-contract.md)
- [Explicit diagnostic composition](../../decisions/cli/cli-explicit-diagnostic-composition.md)
- [Local reference integrity](../../decisions/cli/cli-local-reference-integrity.md)
- [Authored route inventory](../../decisions/cli/cli-authored-route-inventory.md)
- [Command framework](../../decisions/cli/cli-command-framework.md)
