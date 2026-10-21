# Open Forge CLI

The Open Forge CLI helps you find context, inspect routes, keep generated
navigation correct, and manage Framework files. It's an accelerant, not a
requirement: the Framework is complete in plain files, and you can read, edit,
and follow its rules without the CLI. The commands automate the work around
those files. The files still define what the Framework means.

This is the complete command reference, with one section per operation. For which command
helps with which job, and the everyday flows that tie them together, start with
[Working with the CLI](https://thelithiumforge.github.io/open-forge/docs/cli).

To see the exact options of the executable you have, start here:

```sh
open-forge --help
```

For repository setup, build prerequisites, and a worktree-local executable,
see [Development and setup](development.md). For package structure and manual
Extension use, see [Extensions](extensions.md).

## Command index

Choose a task, then follow its command link. Each operation has the same shape:
purpose, common examples, options, behavior notes, and next step. Read-only
examples need no preview. Mutation examples show a preview before application.

| Task                          | Commands                                                                                                                                                                                                                                 |
| ----------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Read and discover context     | [status](#status), [context](#context), [find](#find), [references](#references), [route list](#route-list), [route inspect](#route-inspect)                                                                                             |
| Add and organize knowledge    | [route init](#route-init), [route create](#route-create), [route update](#route-update), [route move](#route-move), [route remove](#route-remove)                                                                                        |
| Check and repair              | [index](#index), [doctor](#doctor), [repair](#repair), [cleanup](#cleanup)                                                                                                                                                               |
| Manage the Framework          | [install](#install), [update](#update), [remove](#remove)                                                                                                                                                                                |
| Manage Extensions             | [extension list](#extension-list), [extension inspect](#extension-inspect), [extension create](#extension-create), [extension install](#extension-install), [extension update](#extension-update), [extension remove](#extension-remove) |
| Share files through Libraries | [library list](#library-list), [library inspect](#library-inspect), [library attach](#library-attach), [library sync](#library-sync), [library detach](#library-detach)                                                                  |

## Start with the everyday commands

After installing the Framework, give your agent a normal task. Reach for the
CLI when you want to inspect or maintain its files:

| Need                                              | Command                           |
| ------------------------------------------------- | --------------------------------- |
| Read startup context                              | `open-forge context`              |
| Find rules                                        | `open-forge find --tag=Directive` |
| Check links and navigation                        | `open-forge doctor`               |
| Preview navigation updates after editing Markdown | `open-forge index --dry-run`      |

Add `--detail standard` for more explanation, `--workspace <path>` for another
project, or `--format json` for structured output. Run `open-forge index` after
reviewing its preview to apply it. These are maintenance commands, not steps
you must repeat before each agent task.

## After you edit files by hand

Create, rename or move user-owned Markdown inside routed folders, then let the CLI catch up.

For new notes or folders, preview and apply Index:

```sh
open-forge index --dry-run
open-forge index
```

Index fills missing descriptions and category tags, creates missing navigation files below existing roots, and updates Entries. It keeps your content and supplied metadata. Refine the prefilled values later.

After a rename or move, preview and apply Repair:

```sh
open-forge repair --automatic --dry-run
open-forge repair --automatic
```

Repair fixes links and affected navigation when exactly one successor is proven by committed content or unique recorded metadata. With committed Git evidence, Index and Repair work in either order. Without Git, run Repair first while old Entries still record the file's metadata.

If a target needs your choice, run interactive Repair or copy its candidate's relink command and preview it with `--dry-run`. If a file was deleted with no replacement, edit or remove its authored link. Repair preserves managed files and other protected sources.

Run `open-forge doctor` and follow its one next step until every check completes with no warnings or errors. A Git timeout asks you to retry. See [Index](#index), [Doctor](#doctor) and [Repair](#repair) for the boundaries and folder-rename advice.

## The command shape

Commands use this general form:

```text
open-forge <command> [command options] [global options]
```

Use `open-forge --help` to see commands and `open-forge <command> --help`
for arguments, options, and examples. Root help starts with an Essentials
preview and installation, then groups commands by task. Each command's help
leads with its purpose, Usage, Examples, and Behavior, followed by its arguments
and options, Details, Automation, and Related commands where applicable.
Shared options are listed in full in root help and summarized on command pages.
Help wraps explanations to the terminal width. Redirected help uses 80 columns.
Command examples stay on one line so copying one runs one shell command.

The CLI works against the current directory unless `--workspace` selects one
explicit directory. A source reference is either an automatic source ID, such
as `memory/crystallized/documents`, or an exact path under `.agents`, such as
`.agents/memory/crystallized/documents/_documents.md`. The exact grammar is
covered in [Source references](#source-references).

Most examples use `memory/crystallized/documents`, the Documents category that
the Project Documents Extension installs. The base install doesn't include it.
In a base-only workspace, use a Core route such as `memory/crystallized`
instead.

The CLI separates observation from change. Commands such as `status`,
`context`, `find`, `references`, `route list`, and `route inspect` read the
workspace. Commands that can change files offer `--dry-run` when a complete
preview is useful. A dry run builds and reports the operation's plan without
changing any files. Either way, the operation needs enough evidence to tell
which files it may manage, and it reports a boundary rather than guessing.

## Global options

These options keep the same spelling and meaning on every command that accepts
them.

| Option                                        | Meaning                                                                                                                                                          |
| --------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `--workspace <path>`                          | Use one exact directory instead of the current directory. Relative paths resolve from the process current directory. The CLI does not search parent directories. |
| `--format <text\|json>`                       | Select human text or one schema-3 JSON result. The default is `text`. JSON always goes to standard output.                                                       |
| `--detail <minimal\|standard\|full\|debug>`   | Select result detail. The default is `minimal`. `debug` adds bounded diagnostics on standard error.                                                              |
| `--detail-filter <error\|warning\|info\|all>` | Repeat to select listed finding severities. Counts, effects, status, and exit are unchanged.                                                                     |
| `--help`                                      | Show help for the selected command path and exit.                                                                                                                |
| `--version`                                   | Show the executable version and exit.                                                                                                                            |

`--help` and `--version` are terminal modes. They do not resolve a workspace
or run a domain operation, and they cannot be used together. Other well-formed
global options may accompany them, but command operands and command-specific
options remain invalid in a terminal invocation.

The detail levels are cumulative. `minimal` leads with the result, subject or
path, cause, and supported action. `standard` adds reasons and per-finding
actions. `full` adds evidence, candidates, provenance, hashes, and finding
codes. `debug` adds bounded diagnostics on standard error. Changed, restored,
deleted, kept, and rewritten paths remain visible at every level. A dry run
ends with `No files were changed.` and a verified no-op says `Nothing to do.`
Install's interactive review is a plan awaiting confirmation, so it does not
use the dry-run closing line.

`--detail-filter` changes which findings are listed, not their totals or the
operation's status. JSON records the selected detail and filter in one schema-3
document with `summary`, `findings`, `effects`, `counts`, `limitations`, `data`,
`recovery`, and `next` members. The text and JSON forms use the same typed
operation result and never rerun the operation.

In text mode, `completed`, `completed-with-warnings`, and `incomplete` results
use standard output. `failed`, `invalid-input`, `blocked`, and `cancelled`
results use standard error. JSON always uses standard output. Human results use
colour automatically on capable terminals: green for completed results, yellow
for results that need attention and for warnings, red for invalid, blocked, or
failed results and for errors, and cyan for information labels. Written labels
remain visible. Redirected output, Windows, missing or `dumb` `TERM`, and a
nonempty `NO_COLOR` use plain text. JSON, selected file content, and preview
diffs stay plain. There is no colour option to configure.

### Prompts and noninteractive runs

Commands that write files after confirmation show their already-built minimal
plan on standard error and ask `Apply these changes? [y/N]`. When the plan
deletes, replaces or removes something, the question names that part, such as
`Apply these changes, including deleting 3 files? [y/N]`. The answer still
approves the whole plan shown above it. Extension Create asks
`Create these files? [y/N]`.

The plan says what happens to each existing file. `replaced` means the whole
file is swapped. The row adds `(your previous file is in the recovery bundle)`
when that copy is kept after the command finishes. Smaller
edits say what changes, such as `Entries updated`, `Open Forge section added,
your content was kept`, `settings updated` or `ownership record updated`.

Selection lists mark each row. `[+]` installs or updates a package, `[-]`
removes it, `[*]` marks a package included because another choice needs it,
and `[ ]` is not chosen. Move with the arrow keys, press space to choose, and
press Enter to continue. The focused option explains its effect. A terminal
that cannot read keys shows a numbered list with option descriptions and asks
for numbers instead. Package lists explain their marks and accept an empty
line to continue or `cancel` to stop without changes.

Install can ask setup questions before building the plan on a first interactive
apply or with `--configure`. Dry-run, automatic, JSON, and redirected requests
skip those questions. Noninteractive Configure requires an explicit preset or
`--frontmatter`.

These commands confirm this way and accept `--automatic`, which skips
interaction and adds no force, prune, or other authority:

- `install`, `update`, `remove`, `route remove`, and `repair`
- `extension create`, `extension install`, `extension update`, and
  `extension remove`
- `library attach`, `library sync`, and `library detach`

Permission prompts list the affected path and scope in the order `1. Allow always`,
`2. Allow once`, `3. Cancel`. Key mode starts focused on `Allow once`, so Enter
approves only this run without saving a grant. Choose `Allow always` deliberately
to save a grant that every Extension and Library can reuse. Line mode accepts
`always`, `once`, or `cancel`, and an empty line cancels. A directory grant means
everything beneath that directory. `--allow-path <path>` supplies the same
grant noninteractively. Explicit grants and interactive Always answers are
published only after final confirmation. A declined, exhausted, or cancelled
prompt leaves files and settings unchanged and returns `cancelled` with exit
`130`. JSON, redirected operation, `--automatic`, and dry-run never prompt. A
noninteractive request that needs confirmation returns `invalid-input`, writes
nothing, and names the `--automatic` form to run instead. Cancellation before
any change reports that the operation was cancelled and nothing changed,
without an error row. Unknown commands and invalid option values include a
`Next:` help command. Invalid values name the supplied value and the allowed
values. Parser choices appear on separate lines.

Suggested next actions and retries keep the selected workspace and the options
needed for that action, including a local Extension source and selected IDs.
Use the reported command after resolving its explanation.

### Status and exit codes

Every operation reports one status and the corresponding process exit
code:

| Status                    | Exit code | Meaning                                                                                |
| ------------------------- | --------: | -------------------------------------------------------------------------------------- |
| `completed`               |       `0` | The requested operation or verified no-op completed.                                   |
| `failed`                  |       `1` | The operation began or could not finish and reported a failure.                        |
| `completed-with-warnings` |       `2` | The requested result is usable, but a reported warning needs review.                   |
| `incomplete`              |       `3` | Some required facts were unavailable, so the result is not complete.                   |
| `invalid-input`           |       `4` | The command input does not follow its grammar or metadata rules.                       |
| `blocked`                 |       `5` | A workspace, ownership, safety, or authority boundary prevents the operation.          |
| `cancelled`               |     `130` | The operation ended before completion, usually because input or cancellation ended it. |

The status says more than a plain success or failure. For example, a route
inspection can complete while reporting a structural observation, while a
change that can't establish trusted lifecycle facts is blocked. Follow the
suggested next action in the result, then run the command again so it starts
from the workspace's current state.

## Read the workspace

### `status`

Show a short summary of workspace structure, context, navigation, managed content, and recovery.

#### Common examples

```sh
open-forge status
open-forge status --detail minimal
open-forge status --format json --detail standard
```

#### Options

No command-specific options.

See [global options](#global-options) for shared flags.

#### Behavior notes

`status` gives a bounded summary of workspace structure, startup context,
generated navigation, Framework and Extension lifecycle state, Libraries, and
recovery observations.

It is a report. It does not repair stale navigation, adopt changed files, or
make a lifecycle record trusted. Use `doctor` when the summary names a fact
that needs diagnosis.

#### Next step

Use [`doctor`](#doctor) when the summary names a fact that needs diagnosis.

### `context`

Read ordered startup context and additional sources selected for the task.

#### Common examples

```sh
open-forge context
open-forge context --for src/Order.cs --for web/order.ts
open-forge context memory/crystallized/documents --for src/Order.cs
open-forge context \
  templates \
  --for src/Order.cs \
  --for web/order.ts \
  --content=frontmatter,headings \
  --additions-only
```

```sh
open-forge context --for src/Order.cs
open-forge find --for src/Order.cs --tag=Directive
open-forge context directives/backend/csharp --for src/Order.cs
```

#### Options

- Repeat `--for <path>` for every working file in the task. Paths are relative
  to the selected workspace, and planned paths may not exist yet.
- `--additions-only` omits files already required at startup. It needs at
  least one explicit source argument. With `--for`, the same complete path set
  filters both startup and combined context before their difference is computed.
- `--content=part[,part...]` projects selected parts: `metadata`, `paths`,
  `frontmatter`, `headings`, `body`, or a named `section:<heading>`. The
  default is `frontmatter,body`.
- `--follow-links=positive-depth|all` follows contained local Markdown links to
  the requested positive depth or through the complete reachable link set.

See [global options](#global-options) for shared flags.

#### Behavior notes

`context` returns ordered startup context or the selected sources added to it.
With no source arguments it returns the startup files. Add source IDs or
exact paths to include the context you select. An unresolved source reports why
Context could not read it and suggests `open-forge route list --depth all`.
Unavailable counts are explained once. `context`, `find`, and
`route inspect` accept repeated `--for <path>` values. Paths resolve against
the selected workspace, may name planned files that do not exist yet, and must
stay inside that workspace.

Use `open-forge context skills/open-forge-cli` for an initial batch containing
the default loading-tag closure and the CLI usage guide required by the Skills
entrypoint. The default projection includes frontmatter and body. Plain
`context` derives its closure from loading tags. Explicit read instructions
in returned files still apply. For subsequent selected route chains, batch
source operands and add `--additions-only` to omit repeated startup context.

Use the returned file identities to inspect what the command included. The
[loader](../src/open-forge/.agents/loader.md) defines Framework loading and scope.
Command output does not change those rules.

`applyTo` only filters a source that another loading or selection rule would
include. It never selects or loads an entry by itself. Context includes a
conditioned entry only when a loading tag, an explicit source selection, or a
followed link would include it. With no `--for` paths, only entries that would
otherwise load automatically are deferred and reported as pending. Untagged
conditioned entries stay on demand and are not reported as pending. The command
returns `incomplete`, exit 3, and shows those pending conditions until you
provide working paths. The CLI does not infer paths from Git or discover code
dependencies. Repeat `--for` for the full working set, including planned files
and related callers or tests. Reading a Markdown source only to obtain context
does not add that source to the working set. A source passes the filter when at
least one supplied working path satisfies every condition in its inherited
chain. A condition does not select a hidden ancestor or grant or restrict edit
permission.

To look further than the loading rules, use Find for discovery and Context to
load what you choose. `find --for` lists every compatible source, including
sources below scopes nobody selected, without loading anything:

The first command returns what the loading rules load for that file. The second
lists other compatible sources. The third loads a source you selected, with its
route context.

With supplied paths, an explicit source or followed reference can retrieve a
known nonmatch for inspection with its needed route context. That inspection
does not activate the source's automatic children. The `--for` values only
filter applicable sources. They do not supply a loading reason.
`LoadNow` and `KeepInMind` still load through exposed entries of already-loaded
parents. Selecting an on-demand scope activates its applicable child loading
rules. `KeepInMind` adds refresh instructions while the scope remains active.
The CLI resolves each invocation independently and does not track an agent
session.

#### Next step

Use [`find`](#find) to discover additional sources, then batch selected IDs in a Context request.

### `find`

Find Markdown sources by authored tags, structural headings, and working-file conditions.

#### Common examples

```sh
open-forge find --tag=Memory --tag=CurrentTruth --require=all
open-forge find --heading=Axioms --within=body
open-forge find --tag=Directive --require=all \
  --for src/Order.cs --for web/order.ts
open-forge find --include=memory/crystallized/documents --content=headings
```

#### Options

- `--include <source-reference>` adds a set of sources to search.
- `--exclude <source-reference>` excludes a set of sources from the search.
  Exclusions win over inclusions.
- `--tag <tag>` matches one authored tag. Repeat it for additional predicates.
- `--heading <heading>` matches one complete structural heading.
- `--require=all|any` chooses whether all or any supplied predicates must match.
  The default is `all`.
- `--within=part[,part...]` limits predicate evaluation to authored regions:
  `document`, `frontmatter`, `body`, or `section:<name>`. By default, tags
  match in frontmatter and headings match in the body.
- `--content=part[,part...]` chooses which parts of matched sources to return:
  `metadata`, `frontmatter`, `headings`, `body`, or `section:<name>`.
- Repeat `--for <path>` to keep only sources whose declared or inherited file
  conditions match at least one supplied working path. A source without an
  effective condition matches every path. This is an additional filter over
  `find` command's existing source universe. Ancestor facts are read to
  evaluate the condition, but ancestors are not added to the results.

See [global options](#global-options) for shared flags.

#### Behavior notes

`find` selects Markdown sources by authored tags and structural headings. It
does not treat arbitrary body text as a substitute for route or metadata
meaning.

`--for` is independent of tag and heading predicates. `--require=all|any` still
combines only supplied tags and headings, and `--for` does not satisfy the
existing tag or heading requirements for `--require` or `--within`.

Use an exact path when an ID is ambiguous. A blocked or incomplete result is
not permission to guess which source was intended.

#### Next step

Load a chosen result with [`context`](#context) before relying on its rules.

### `references`

Show direct authored incoming and outgoing Markdown links for one source.

#### Common examples

```sh
open-forge references memory/crystallized/documents
open-forge references memory/crystallized/documents --direction=out
open-forge references memory/crystallized/documents --direction=in \
  --include=memory/crystallized
```

#### Options

- `--direction <in|out|both>` selects the report. Default: `both`.
- Repeat `--include <source-reference>` and `--exclude <source-reference>` to bound the incoming scan. Exclusions win.

See [global options](#global-options) for shared flags.

#### Behavior notes

`references` reports direct authored incoming and outgoing Markdown references
for one source. The default direction is `both`.

`--direction=in|out|both` selects the report. `--include` and `--exclude`
limit the incoming scan. Combining them with `--direction=out` is invalid
input. Links listed under `Entries` are generated navigation, so they aren't
counted as references.

#### Next step

Use [`context --follow-links`](#context) to read the linked explanations you select.

<a id="routes"></a>

### `route list`

List source IDs and routed descendants at the selected structural depth.

#### Common examples

```sh
open-forge route list
open-forge route list memory/crystallized/documents --depth=all
```

#### Options

`--depth <nonnegative-integer|all>` selects depth. Default: `1`. Use `0` for selected sources only or `all` for all descendants.

See [global options](#global-options) for shared flags.

#### Behavior notes

An `entrypoint` makes a folder routable. A `route` is the navigable path that
the entrypoint and its generated `Entries` expose. Route commands accept the
source-reference grammar described below unless their target has a narrower
shape.

`route list` shows routed sources and descendants at a structural depth. The
default depth is `1`. Use `--depth=0` for the selected sources only, or
`--depth=all` to include the complete routed descendant set. Both `--depth=2` and `--depth 2` are accepted.

#### Next step

Choose a source ID and inspect it with [`route inspect`](#route-inspect).

### `route inspect`

Explain one source's route, loading behavior, and working-file conditions.

#### Common examples

```sh
open-forge route inspect memory/crystallized/documents
open-forge route inspect memory/crystallized/documents \
  --for src/Order.cs \
  --for web/order.ts
open-forge route inspect .agents/memory/crystallized/documents/_documents.md
```

```sh
open-forge route inspect directives/backend/csharp --matching-files
```

#### Options

- Repeat `--for <path>` for known working files, including planned files.
- `--matching-files` lists up to 100 current files matching the full condition chain. It supplies no working paths and does not activate the scope.

See [global options](#global-options) for shared flags.

#### Behavior notes

`route inspect` explains one source's route behavior without returning its
authored body. Repeat `--for <path>` to see how supplied working paths match
the source's declared and inherited conditions. A planned path may not exist.
An explicit inspection can explain a nonmatch, but it does not activate the
source's automatic children. Without `--for` paths, only entries that a loading
tag would otherwise load automatically have pending conditions. Explicitly
selected entries remain inspectable without a path filter.

##### Current matching files

Use `route inspect <source> --for <path>` to check specific working paths. Use
`route inspect <source> --matching-files` to preview which current files match
the source's own and inherited `applyTo` conditions. When the whole chain has
no conditions, or only match-all conditions such as `**`, `**/*`, `**/**`, or
`*/**`, every file matches. The preview then answers `all files` without
scanning.

Otherwise, the command scans. It counts every match, lists the first 100 paths,
and reports the scan's scope and completeness. Reaching the 100-path cap doesn't
make the scan incomplete. Paths are relative to the workspace, use `/`
separators, and are sorted by case-sensitive ordinal comparison.

When the workspace is confirmed to be in a Git work tree, the scan asks Git with
`git ls-files --cached --others --exclude-standard`. That lists existing tracked
files and any untracked files that aren't ignored. A tracked file stays eligible
even when an ignore pattern matches it.

The command discards any Git output and uses the
[workspace fallback](#workspace-fallback) instead when any of these happen:

- Git can't start.
- The workspace isn't in a Git work tree.
- Git refuses the repository.
- Git's inventory fails.

Both strategies stay inside the selected workspace and omit deleted tracked
files. They skip `.git`, symbolic links, directory links, other reparse points,
and submodule contents.

One 30-second deadline covers the scan and any fallback. A timeout leaves the
scan unavailable, and a cancellation leaves it cancelled. Neither one starts a
fallback. Completeness describes what the scan observed, not an atomic snapshot
of the filesystem.

The two options do different jobs, and you can combine them. `--for` supplies
working paths, so it can change whether the source applies. `--matching-files`
only lists current files. It supplies no working paths and doesn't change
loading. So a complete scan can still come with an `incomplete` report when the
loading facts need explicit working paths. A scan with no matches reports
`No current files match this condition. Planned files may still match.`

##### Workspace fallback

Without a usable Git inventory, the command walks the workspace itself and
applies its `.gitignore` files with the Open Forge matcher. It reports the scope
as `workspace files filtered by .gitignore using Open Forge rules`.

The walk reads only `.gitignore` files inside the workspace, and tracked files
get no exemption. Matching is case-sensitive. The walk skips and counts lines it
can't express. The [fallback rules](#gitignore-fallback-rules) list exactly how
each pattern is translated.

#### Next step

Use [`context`](#context) to load the source and applicable route context.

## Maintain routes and Markdown

The route commands preserve the distinction between authored source content and
generated navigation. They plan the complete bounded effect, recheck the
expected state before writing, and verify the result. Use `--dry-run` before an
operation whose target or metadata needs review.

Resolve a reported boundary before retrying. When a result suggests retrying a
blocked Route request, its command retains the requested targets and options, including
metadata and Template choices.

### Frontmatter and metadata

Choose root fields or an `open-forge:` mapping for your workspace. Both are readable in every workspace. New files follow `.agents/open-forge.json`. A missing `frontmatter` setting means scoped.

Root form:

```yaml
---
description: C# rules
tags: [Directive, LoadNow]
applyTo: ["**/*.cs"]
---
```

Scoped form:

```yaml
---
open-forge:
  description: C# rules
  tags: [Directive, LoadNow]
  applyTo: ["**/*.cs"]
---
```

When an `open-forge` mapping exists, root descriptions and tags belong to another tool. Open Forge does not merge them. `applyTo` is read at both locations, with equivalent sets counting once and conflicting sets rejected.

The same precedence applies to `responsibility`. An explicit `open-forge` key
with an invalid value never falls back to root fields. Unknown root keys are
ignored and preserved. Metadata alone does not make a file a routed source.
Native `SKILL.md` metadata keeps its own contract and parser.
Both forms use the same value grammar and diagnostics.

`description` helps a reader decide whether to open a source. An optional
`responsibility` helps an editor decide what belongs in it by stating what the
file defines. It does not create authority or loading behavior. Tags are bare
values in frontmatter, without `#`. They must be valid and useful for
classification, loading, routing, or search. A tag starts with a letter and
then contains letters or digits, with single internal hyphens allowed. It may
not end with a hyphen or contain two adjacent hyphens.

`applyTo` is an optional file condition. A quoted string is an expression and
a list of quoted strings contains one atomic pattern per entry. Both forms are
accepted at the frontmatter root or under `open-forge:`, with the same meaning
in either place. A root declaration may sit beside an existing `open-forge:`
block. A string expression separates top-level patterns with commas, trims
whitespace around each fragment, and drops empty fragments. For example,
`"**/*.cs, **/*.csproj"` is equivalent to a two-item list. Commas inside brace
alternatives or character classes do not separate patterns. For example,
`"**/*.{cs,csproj}"` uses brace alternatives and `"**/*.[ch]"` uses a character
class. Include `**/` when a pattern should match at any depth, such as
`"**/*.cs"`.

`!` at the start of a pattern is literal. It negates only when it is the first
member of a character class, such as `[!abc]`.

Within a string expression, `\,` is a literal comma. A list entry is not split or decoded, so its comma remains part of
that one atomic pattern. Empty expressions or entries, and non-string values,
are invalid. New CLI-authored conditions use a quoted-string list in the
workspace's chosen form. If both locations declare the field, equivalence
compares their sets of atomic pattern texts, and conflicting sets are invalid. A brace
shorthand is not equivalent to its expanded list. `route update` keeps an
existing declaration at its authored location and updates equivalent root and
scoped declarations together. For example, `applyTo: "**/*.cs"` and
`applyTo: ["**/*.cs", "src/report,legacy.cs"]` are valid at either location.
The list example remains valid because each list entry is atomic. A string such
as `applyTo: "src/report,legacy.cs"` now means two patterns. To keep one
literal-comma pattern, write `applyTo: 'src/report\,legacy.cs'` or move it to a
list entry such as `applyTo: ["src/report,legacy.cs"]`. See the
[Loader](../src/open-forge/.agents/loader.md) for matching and loading rules.

The command flags use the same concepts:

- `--description <text>` sets one nonblank description.
- Repeated `--tag=<tag>` values provide an ordered tag list. The route create
  and update commands require unique canonical tags when the list is supplied.
- `--responsibility <text>` sets the optional responsibility. On update, an
  exact empty value removes the key.
- Each `--apply-to <glob>` on `route create`, `route init`, or
  `route update` accepts comma-separated patterns. Repeat the option to supply
  additional expressions. Patterns match from the workspace root and are
  case-sensitive. For example, `*.py` matches only files at the root,
  `**/*.py` matches at any depth, and `docs/**` matches everything under
  `docs`. A trailing `/`, as in `docs/`, is rejected. A leading `!` is literal
  and does not exclude files.
- `route update --clear-apply-to` removes the condition and cannot be combined
  with `--apply-to`.

After authoring a condition, [inspect its matches](#current-matching-files) with
`route inspect <source> --for <path>` or
`route inspect <source> --matching-files`.

The CLI does not infer these values from a filename, parent, Template, body, or
generated entry.

Route creation, route initialization, and adoption write new metadata in the
workspace's chosen form. `route update` edits existing metadata where it was
authored. When a routed file has no Open Forge metadata, it creates metadata
in the workspace's form and requires a complete description and tags.

<a id="initialize-a-route-chain"></a>

### `route init`

Create missing entrypoints in one route chain or restore a selected omitted built-in category.

#### Common examples

```sh
open-forge route init patterns --framework --dry-run
# Review the plan before applying it.
open-forge route init patterns --framework
```

```sh
open-forge route init memory/crystallized/documents/project-alpha \
  --description="Project Alpha documents" \
  --responsibility="Define the documents route" \
  --tag=Document --tag=ProjectAlpha --apply-to "**/*.md" --dry-run
# Review the plan before applying it.
open-forge route init memory/crystallized/documents/project-alpha \
  --description="Project Alpha documents" \
  --responsibility="Define the documents route" \
  --tag=Document --tag=ProjectAlpha --apply-to "**/*.md"
```

#### Options

- `--framework` uses the trusted embedded Framework scaffold.
- `--description`, `--responsibility`, repeated `--tag`, and repeated `--apply-to` set metadata on a missing generic final entrypoint. See [metadata](#frontmatter-and-metadata).
- `--dry-run` previews the complete plan.

See [global options](#global-options) for shared flags.

#### Behavior notes

`route init <route-target>` initializes each missing entrypoint in one exact
route chain. A target is a route ID or an exact canonical `.agents` entrypoint
path. The generic scaffold accepts optional metadata for the final entrypoint.
Without it, the new entrypoint gets a placeholder description and tags, and
the result reminds you to edit them.

`--framework` selects the trusted embedded Framework scaffold. It cannot be
combined with `--description`, `--responsibility`, `--tag`, or `--apply-to`, because those are
different scaffold choices. Metadata options are valid only for a missing
generic final entrypoint and cannot update an existing final target. `route init`
does not initialize the Loader itself.

To add an omitted Core category later, name its canonical root with
`--framework`. This restores the category's packaged contents and necessary
missing ancestors in the installed Framework:

Use `skills` to restore the bundled Skills payload, including the CLI Skill,
or `memory/archived` to add just that Memory state. The plan clears only the
selected root or state's exact exclusion. It retains unrelated descendant
and file exclusions, existing authored files, and overwrite companions.
If a broader ancestor is excluded, the command asks you to add that ancestor
explicitly first. An orphan overwrite that prevents safe navigation blocks
the plan without writes.

These restoration semantics apply to unscoped canonical Core roots and Memory
states. A scoped Framework target still creates its sparse entrypoint chain.

Apply a previewed creation before running a command that depends on the new path.

#### Next step

Inspect the created route with [`route inspect`](#route-inspect), then create a file if needed.

<a id="create-one-routed-file"></a>

### `route create`

Create one ordinary routed Markdown file and its missing intermediate entrypoints.

#### Common examples

```sh
open-forge route create memory/crystallized/documents/project-alpha/architecture \
  --description="Current service boundaries and request flow" \
  --tag=Document --tag=Architecture \
  --responsibility="Define the current service structure and dependencies" \
  --apply-to "**/*.cs" --apply-to "**/*.csproj" --dry-run
# Review the plan before applying it.
open-forge route create memory/crystallized/documents/project-alpha/architecture \
  --description="Current service boundaries and request flow" \
  --tag=Document --tag=Architecture \
  --responsibility="Define the current service structure and dependencies" \
  --apply-to "**/*.cs" --apply-to "**/*.csproj"
```

#### Options

- `--description`, `--responsibility`, repeated `--tag`, and repeated `--apply-to` set optional [metadata](#frontmatter-and-metadata).
- `--template <source-reference>` copies the body of an existing routed source tagged `Template`.
- `--dry-run` previews the creation.

See [global options](#global-options) for shared flags.

#### Behavior notes

`route create <file-target>` creates one ordinary routed Markdown file inside
an existing root category, such as `memory` or `guidance`. When folders between
the root and the new file are missing, it creates them with their entrypoints
in the same plan. Description, tags, and responsibility are all optional. When
the description or tags are omitted, the file is still created, and the result
is `completed-with-warnings` with a `route update` next step to add them.
`--template` copies the body of one existing routed Markdown source tagged
`Template` as starting content. The new source gets its own metadata and does
not retain Template ownership.

The target is an ordinary Markdown file below an existing root category. It is
not a directory, entrypoint, overwrite companion, Loader, or general external
path. An unknown root is invalid input. The command never overwrites an
existing target. Use `route update` for an existing source.

Planning supplies `templates/planning/decision`. Essentials leaves Templates out. Before installing Planning or `development-toolkit` for its Templates, preview `open-forge route init templates --framework --dry-run`, review it, then run `open-forge route init templates --framework`. After installation, continue when `open-forge find --tag Template` lists the required Template. For an already installed package with skipped files, follow the [restoration and update guidance](#extension-install).

#### Next step

Open the new file and fill in its body, then check it with [`doctor`](#doctor).

<a id="update-one-routed-source"></a>

### `route update`

Patch one source's selected metadata or fill an eligible empty body from a Template.

#### Common examples

```sh
open-forge route update memory/crystallized/documents/project-alpha/architecture \
  --description="Current service structure and dependency boundaries" \
  --tag=Document --tag=Architecture \
  --apply-to "**/*.cs" --apply-to "**/*.csproj" --dry-run
# Review the plan before applying it.
open-forge route update memory/crystallized/documents/project-alpha/architecture \
  --description="Current service structure and dependency boundaries" \
  --tag=Document --tag=Architecture \
  --apply-to "**/*.cs" --apply-to "**/*.csproj"
```

For an existing declared condition:

```sh
open-forge route update memory/crystallized/documents/project-alpha/architecture \
  --clear-apply-to --dry-run
# Review the plan before applying it.
open-forge route update memory/crystallized/documents/project-alpha/architecture \
  --clear-apply-to
```

#### Options

- `--description` replaces the description.
- Repeated `--tag=<tag>` values replace the complete ordered tag list.
- `--responsibility <text>` sets the responsibility. `--responsibility ""`
  removes it.
- Repeated `--apply-to <glob>` values replace the complete condition.
  Existing root or scoped placement is preserved, and equivalent dual
  declarations are updated together. `--clear-apply-to` removes both locations
  and is mutually exclusive with `--apply-to`.
- `--template <template-reference>` completes an eligible frontmatter-only
  body. If the target already has authored body content, the CLI preserves it,
  still applies any metadata changes, and returns `completed-with-warnings`
  rather than overwriting the body.
- `--dry-run` previews the patch.

See [global options](#global-options) for shared flags.

#### Behavior notes

`route update <source-reference>` applies one or more explicit metadata
changes, or copies one Template body when the target is eligible.

For an existing source with a declared condition, preview its removal with
`--clear-apply-to` by itself:

At least one metadata or Template operation is required. The selected Template
contributes body content only. Its frontmatter and continuing lifecycle do not
transfer to the destination.

#### Next step

Check the new metadata with [`route inspect`](#route-inspect) and applicable Context requests.

<a id="move-or-remove-a-route"></a>

### `route move`

Move one unmanaged routed file or category and update affected links and navigation.

#### Common examples

```sh
open-forge route move \
  memory/crystallized/documents/project-alpha/architecture \
  .agents/memory/crystallized/documents/project-alpha/service-architecture.md --dry-run
# Review the plan before applying it.
open-forge route move \
  memory/crystallized/documents/project-alpha/architecture \
  .agents/memory/crystallized/documents/project-alpha/service-architecture.md
```

#### Options

Its only command-specific option is `--dry-run`.

See [global options](#global-options) for shared flags.

#### Behavior notes

`route move` moves one routed source or category to an exact destination path.
Select a category by its entrypoint path. The destination's parent route must
already exist, so run `route init` first when it doesn't. Review the
destination's scope and inherited rules when choosing the new location.

Both commands can be blocked by ambiguous routes, changed targets, unsafe
generated regions or incomplete evidence. `route move` still refuses managed
content. Read the plan and resolve the named boundary before applying it.

See [reference scans](#reference-scans-for-moves-and-removals) for the shared scan boundary.

#### Next step

Check the destination and rewritten links, then run [`doctor`](#doctor).

### `route remove`

Remove one routed file or category, release its claims, and record that it should stay removed.

#### Common examples

```sh
open-forge route remove memory/crystallized/documents/project-alpha/service-architecture --dry-run
# Review the plan before applying it.
open-forge route remove memory/crystallized/documents/project-alpha/service-architecture --automatic
```

#### Options

- `--dry-run` previews deletion, detached references, navigation, and exclusions.
- `--automatic` skips confirmation.

See [global options](#global-options) for shared flags.

#### Behavior notes

`route remove` removes one eligible routed source or complete category, releases
its file ownership and records the removal choice. Supported links that point
at the removed file are detached: each keeps its visible text as plain text.
The root [`remove`](#remove) command also handles ordinary
files, directories, packages and Libraries.

The same [reference scan](#reference-scans-for-moves-and-removals) and evidence boundaries apply as for Route Move. Framework- and Extension-managed content, including edited files, can be removed when safety checks allow it. Selecting a category by its entrypoint removes the whole category, including contained native and binary resources. The loader and `.agents` root are protected. A single native Skill or non-Markdown resource is not a routed Markdown leaf. Links, unsafe references, incomplete scans, and nested Git repositories block category removal. Review the plan before applying it.

#### Next step

Check the removed paths and detached link text, then run [`doctor`](#doctor).

### Reference scans for moves and removals

Both commands scan supported Markdown across the workspace for affected
references. In a Git work tree, they skip paths Git ignores through standard
exclude rules, including dependency and build folders such as `node_modules`,
`bin`, `obj` and `artifacts`. Ignored Markdown is left unchanged. Tracked files
remain covered, and the entire `.agents` tree is scanned, including Git-ignored
private routes. Without a usable Git inventory, they keep the complete scan.
`--detail debug` reports `gitIgnoredPaths=<count>` on stderr when paths were
excluded. It counts skipped files and pruned directory roots, not their
unvisited descendants. Root `remove` uses the same scan for routed subjects.

## Generated navigation and repair

### `index`

Complete missing metadata and navigation after edits to routed Markdown.

#### Common examples

```sh
open-forge index --dry-run
# Review the plan before applying it.
open-forge index
```

For a narrower rebuild:

```sh
open-forge index memory/crystallized/documents --dry-run
# Review the plan before applying it.
open-forge index memory/crystallized/documents
```

#### Options

Its only command-specific option is `--dry-run`.

See [global options](#global-options) for shared flags.

#### Behavior notes

`index` rebuilds bounded generated `Entries` regions from routed sources. With
no operands it rebuilds the Loader and every reachable entrypoint. With
operands it uses the supplied source IDs or exact `.agents` paths: an
entrypoint selects its subtree and the parent that lists it, and a routed file
selects the parent that lists it.

Default Index and explicit Loader or entrypoint selections also maintain
recognized catalogues in the immediate child directories of native Skills
already reachable beneath `.agents/skills`, including their catalogue
subtrees. This updates navigation without changing runtime loading or
activating a Skill. Selecting a native `SKILL.md` alone still selects only its
exposing parent.

Index also fills missing descriptions and tags in user-owned routed Markdown and creates missing entrypoints below existing roots. A leaf selection includes its missing chain and exposing parent. Existing content and supplied metadata stay intact. See [manual edits](#after-you-edit-files-by-hand) for the everyday procedure.

Completion leaves overwrite companions, managed authored content, and excluded or recorded private contents to their own maintenance rules. A user-owned `SKILL.md` gets its native `name` from the folder and its `description` from the heading, and other Markdown in its package gets the usual description and tags. Index does not invent a root. Invalid frontmatter stays unchanged with a precise edit instruction, while independent valid files are indexed.

#### Next step

Run [`doctor`](#doctor) and review the generated navigation diff.

When a renamed folder retains its old canonical navigation file, Index renames that file if Git proves its identity and keeps its authored rules. Compatibility entrypoint names remain valid. Without proof, Doctor offers a manual rename or deletion. If a new navigation file already exists, copy any wanted rules into it before deleting the old one and running Index.

### `doctor`

Diagnose workspace, route, link, lifecycle, Library, Extension, and recovery problems without changes.

#### Common examples

```sh
open-forge doctor
open-forge doctor --format json
open-forge doctor --detail debug
```

#### Options

No command-specific options.

See [global options](#global-options) for shared flags.

#### Behavior notes

`doctor` diagnoses workspace, route, reference, lifecycle, Library, Extension,
and recovery facts without changing them.

Links to existing folders inside the workspace are valid, with or without a
trailing slash. A folder has no heading fragments. A missing folder named with
a trailing slash needs a manual link edit, and Repair never suggests a file in
its place. Existing local files keep their normal checks. Web URLs stay unchecked.

Index encodes local link paths, such as `old%20guide.md`.
Doctor also accepts readable paths: `<old guide.md>` and `café(one).md`.
Escape an entity-opening ampersand as `%26`: write `entity&amp;.md` as
`entity%26amp;.md`. Keep `#`, `%` and `?` in names escaped.
Repair writes Index's form when
correcting other spellings. Command arguments use literal source paths.
Images with invalid heading fragments need a manual edit.

Use it when `status` or another command reports `blocked`, `incomplete`, or
`completed-with-warnings`. Diagnosis can show the next repair boundary, but it does not
apply a proposal.

Doctor gives one next action: safe Repair, eligible Index completion, a target choice, or a precise edit. It uses the same rename proof and protected-source rules as Repair. A missing generated Entries target is reported once as stale navigation. A clean Doctor prints no next step.

#### Next step

Follow the reported next action, using [`repair`](#repair) for supported link corrections.

If Git times out, Doctor says renamed files could not be proven and retries the complete request. It marks reference coverage incomplete and postpones missing-target findings instead of telling you to remove the links. Other unavailable Git evidence still permits recorded-metadata proof and independent diagnosis. Printed commands retain an explicit `--workspace`, including commands embedded in edit instructions.

### `repair`

Apply selected local-link corrections or supported recovery of leftover Library state.

#### Common examples

```sh
open-forge repair --automatic --dry-run
# Review the plan before applying it.
open-forge repair --automatic
```

#### Options

- `--dry-run` previews the selected repair plan.
- `--automatic` selects every currently verified safe-exact repair without prompting.
- Repeat `--relink <source-location> <expected-destination> <target-path>` to select exact current reference occurrences.

See [global options](#global-options) for shared flags.

#### Behavior notes

`repair` applies the local-reference repairs you select and finishes accepted
recovery of leftover Workspace Library state. Preview the selected repairs before applying them.

Without `--automatic` or `--relink`, Repair asks which repairs to apply in an
interactive terminal. When it cannot ask, it lists possible targets with
copyable `--relink ... --automatic` commands. `--automatic` selects every current safe-exact
repair without prompting. An explicit relink can select one source occurrence,
expected destination, and target path:

```text
open-forge repair --relink <source-location> <expected-destination> <target-path>
```

Repair does not rewrite an ambiguous reference or apply an unaccepted guess.
When the evidence is insufficient, it reports the candidate or blocked
boundary for review.

Proven manual renames and moves are included in automatic repairs. The same plan completes eligible metadata and missing entrypoints and refreshes Entries in the affected old and new routes. Unrelated new notes still need Index. See [manual edits](#after-you-edit-files-by-hand) for proof, choices, and deleted files.

Repair preserves managed Framework and Extension files, overwrite companions, excluded or recorded private contents, and unsafe link sources or targets. Managed-file findings point to the managing Update. A partial result names the repairs it applied before the remaining choices or edits. If Git times out, retry the complete request before deciding that a missing file has no successor.

Interactive Repair explains possible targets with reasons such as a matching
file name or title. Choose `Leave this link unchanged` to keep the
current destination. Remaining broken links are still reported, so leaving
them unchanged does not report that every problem was fixed.

When incomplete Library operations can be recovered, choose one recovery step
or leave all recovery steps out. Repair checks the selected step against its
current recovery record, link identity, and destination permission. Recovery
steps have their own counts, separate from rewritten links.

A relink source location has the form `path@line:column`, with one-based numbers
at the start of the link destination. Doctor's displayed location points to the
link and cannot be copied directly as a relink location. Inspect the current
source, or use Repair's reported destination location, before constructing the
request. The expected destination must be the exact current authored value, and
the target must be an admitted reported candidate.

For `[Rule](../working/order-rule.md)` at the start of line 7, the destination
begins at column 8. With that occurrence and reported candidate, preview and
review the selected occurrence before applying:

```sh
open-forge repair --relink \
  ".agents/memory/crystallized/repair-note.md@7:8" \
  "../working/order-rule.md" ".agents/memory/crystallized/order-rule.md" --dry-run
# Review the plan before applying it.
open-forge repair --relink \
  ".agents/memory/crystallized/repair-note.md@7:8" \
  "../working/order-rule.md" ".agents/memory/crystallized/order-rule.md" --automatic
```

When Repair cannot ask, each candidate appears once under its link, with its proof or reason and a copyable relink command. Explicit workspace coordinates are retained. Renamed navigation files appear as one rename row, and newly created entrypoints have no duplicate Entries row.

Plain `repair --dry-run` still needs an interactive selection when guided repairs
are available. Use the automatic form for a preview limited to safe selections.

#### Next step

Follow the reported next action for remaining work, then run [`doctor`](#doctor). Repair refreshes affected Entries. Use [`index`](#index) for unrelated metadata or navigation changes. If automatic Repair leaves target choices, continue with interactive `open-forge repair`.

## Framework lifecycle

Lifecycle commands manage Framework content through one complete
plan. They keep user changes visible and require explicit boundaries for
replacement and deletion.

### `install`

Install or configure the Framework bundled with the running CLI.

#### Common examples

```sh
open-forge install --preset essentials --dry-run
# Review the plan before applying it.
open-forge install --preset essentials --automatic
```

To start small in an existing repository, install the loader with Directives and Crystallized Memory, then link the documents you already have:

```sh
open-forge install --preset minimal --dry-run
# Review the plan before applying it.
open-forge install --preset minimal --automatic
```

To change several setup choices interactively:

```sh
open-forge install --configure
```

For a repeatable form-only change:

```sh
open-forge install --configure --frontmatter root --dry-run
# Review the plan before applying it.
open-forge install --configure --frontmatter root --automatic
```

#### Options

| Option                                              | What it does                                                                                                                                        |
| --------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| `--configure`                                       | Revisit route choices or the frontmatter form in an installed workspace                                                                             |
| `--preset <minimal\|essentials\|full-core\|custom>` | Select built-in routes without the preset question                                                                                                  |
| `--frontmatter <root\|scoped>`                      | Choose where Open Forge writes file metadata: root or scoped. A fresh unattended Install uses root. Change an installed workspace with --configure. |
| `--route <id>=<add\|remove\|git-ignore>`            | Override a Custom or Minimal row. Repeat for different rows                                                                                         |
| `--force`                                           | Allow eligible initial replacements                                                                                                                 |
| `--automatic`                                       | Skip interaction and use supported automatic choices                                                                                                |
| `--dry-run`                                         | Preview the complete plan without writing files                                                                                                     |

See [global options](#global-options) for shared flags.

#### Behavior notes

`install` establishes Framework management in the selected workspace. Explicit
configuration selects built-in routes, restores eligible missing defaults,
and converts eligible owned files to the chosen frontmatter form.

Install has no operands.

First interactive Install offers Minimal, Essentials, Full Core, or Custom before
showing the selected plan and asking for confirmation. An explicit preset
skips the preset question. `--dry-run` changes nothing and never
asks setup questions. Naming the preset makes a preview use the intended
selection. Minimal is marked as recommended. Each built-in preset includes the
one before it. Essentials adds patterns, skills and notes for active work, and
its focused detail explains that new notes for active work stay out of Git.
Full Core adds advice, pointers to sources, starter files, and archived
records, without Install-owned Git-ignore rules.

After resolving the preset, first interactive Install asks `How should Open Forge write file metadata?` and shows a short example of each form when settings do not already declare a preference. Root is preselected. An explicit `--frontmatter` skips that question.

| Choice                     | Explanation                                |
| -------------------------- | ------------------------------------------ |
| `Root keys`                | `description: and tags: at the top level`  |
| `Scoped under open-forge:` | `open-forge: holds description: and tags:` |

Root keys are recommended. Choose the scoped form when other tools already
use top-level frontmatter keys in your files.

The interactive plan starts with `Review changes`. Both the plan and result
recap `Setup`, routes to `Add`, routes with `Git-ignored contents`, routes to
`Leave out`, and `Frontmatter`. Configure also reports changed route choices
and explains that existing files stay routable.

Install's headline describes the result, with the path on a separate `Workspace:`
line. Explanations wrap at spaces to fit the terminal width.

##### Setup choices

| Preset       | Supplied defaults                                                                                                                                           |
| ------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `minimal`    | Directives and Crystallized Memory, with the Memory catalogue, plus the loader, `.agents/README.md` and the managed sections in `AGENTS.md` and `CLAUDE.md` |
| `essentials` | Everything in Minimal, plus Patterns, Skills and Emerging Memory, and Working Memory with private contents and a shared entrypoint                          |
| `full-core`  | Every built-in category and Memory state, with no Install-owned Git-ignore entries                                                                          |
| `custom`     | The current concrete selection in an existing workspace, or Essentials in a fresh one, with selected row overrides                                          |

Essentials omits Guidance, Maps, Templates, and Archived Memory. Full Core
includes the base categories, not optional Extensions or an agent runtime.
Minimal installs Directives and Crystallized Memory, so agents read your rules
and accepted knowledge from the first task. Link existing documents from the
loader, or add rows with `--route`, such as `--route memory/working=git-ignore`.
A Memory state brings the Memory catalogue with it. Presets select supplied
defaults. Existing content in an omitted route remains in place and routable.

The loader's `Entries` are the workspace's list of what is on. Install,
Update, Configure, and Extension commands keep that list as you left it: a
root line you removed stays removed, links you added to documents outside
`.agents/` stay in place, and an empty list stays empty. A root joins the
list only when the command creates its catalogue, or when Configure adds a
route that was left out. A loader that cannot be parsed is treated as listing
every root.

`--configure` revisits the setup choices in an installed workspace. An explicit
`--preset` or `--frontmatter` on an already installed workspace requires
`--configure`. Ordinary repeated Install and Update retain the effective form
and never ask about it. Repeated identical `--frontmatter` values are
idempotent. Conflicting values or values other than `root` and `scoped` are
invalid input.

A fresh Install uses an explicit flag first, then a preference already declared
in settings. Without either, interactive Install asks and unattended Install
uses root. Every fresh Install writes the resolved `frontmatter` key.

Form-only configuration leaves route choices unchanged. See the reviewed example above.

Interactive `--configure` asks with the current form preselected unless the
flag supplies it. Noninteractive `--configure --preset` without the flag
retains the effective form. Every Configure checks eligible owned files for
conversion, even when the selected form is unchanged. An explicit
`--frontmatter` or an answered question writes the setting when its declared
value differs, including when the key was missing. Noninteractive
`--configure --preset` without the flag leaves the declaration as it is,
including an absent key.

Custom offers three actions for each row. The wizard shows each as a mark:

| Mark  | Action       | Effect                                                                                                                                               |
| ----- | ------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------- |
| `[+]` | `add`        | Select the route and restore eligible missing packaged defaults                                                                                      |
| `[~]` | `git-ignore` | Add the route, register its sharing policy, then ignore its contents while keeping its entrypoint eligible for Git                                   |
| `[-]` | `remove`     | Omit supplied defaults and release only their Framework management, preserving existing files, authored notes, overwrite companions, and routability |

Repeat `--route <id>=<action>` to override rows. It requires
`--preset custom` or `--preset minimal`. Minimal starts from its two rows and
adds only what you name. Accepted IDs are:

```text
directives
guidance
maps
patterns
skills
templates
memory/working
memory/emerging
memory/crystallized
memory/archived
```

Unknown presets, row IDs, or actions are invalid input. Repeated identical
presets or actions for one row are idempotent. Conflicting repeated values are
invalid. Interactive Custom shows one list of all ten rows with their current marks:
`[+]` for `add`, `[~]` for `git-ignore` and `[-]` for `remove`. Press `+`,
`~` or `-` to set the focused row, or space to move it to the next mark, then
press Enter to accept the list. Rows supplied by `--route` are locked. A
terminal that cannot read keys shows a numbered list and accepts edits such as
`2+ 9~`. Press Enter on an empty line to continue. Line mode explains that
`~` keeps the entrypoint shared, existing tracked files stay tracked, and `-`
keeps existing files routable. It says `Type cancel to stop without changes.`
Noninteractive Custom applies its base plus the supplied overrides.

To add Guidance and keep Working's records private while sharing its
entrypoint and retaining the other current choices:

```sh
open-forge install --configure --preset custom \
  --route guidance=add --route memory/working=git-ignore --dry-run
# Review the plan before applying it.
open-forge install --configure --preset custom \
  --route guidance=add --route memory/working=git-ignore --automatic
```

Dry-run, automatic, JSON, and redirected requests never ask setup questions.
Noninteractive `--configure` requires `--preset` or `--frontmatter`. Without
explicit setup input, ordinary unattended first Install keeps Full Core and
existing omissions.
Applying a writing plan noninteractively also needs `--automatic`.

Install creates or restores the route and verifies its settings and lock record
before applying Gitignore. For Working Memory, its owned section contains:

```gitignore
/.agents/memory/working/*
!/.agents/memory/working/_working.md
```

The actual entrypoint name is retained, including compatible names such as
`index.md`. Commit the entrypoint, `.agents/open-forge.json`, and
`.agents/open-forge.lock.json` so teammates receive the route and its rules.
Index uses the lock's sharing policy to omit private children, nested entrypoints
and overwrite metadata from shared Entries. Explicit selection does not bypass
that policy. Context, Find and References still read local content. An absent
lock means no privacy policy until configuration restores it. An invalid or
unreadable existing lock blocks Index.

Files already tracked by Git stay tracked. Add and Remove clear only that row's
Install-owned patterns. User rules and text outside the owned section stay
unchanged. Existing whole-directory patterns upgrade during configuration.
Malformed or duplicated section boundaries block the change.

Explicit configuration can restore eligible missing packaged defaults,
including ignored scaffolding after a checkout with or without the lock file.
It preserves existing authored files, compatible route hosts, overwrite
companions, and narrower omissions. Missing private notes need your own copy or
backup. Route configuration grants no general Update or deletion authority.

Frontmatter conversion is a narrow exception: Configure converts unedited
owned Framework and Extension Markdown under `.agents/` whose leading
frontmatter has Open Forge metadata, except native `SKILL.md` files. It
compares each file's authored content with the delivered content in both forms. A file already
matching the selected form needs no change. A file matching the other form is
replaced in the selected form with rebuilt `Entries`.

Edited owned files and Extension files whose source is unavailable stay
unchanged and are reported. Keeping these files does not block the form change.
Excluded files, user-authored files, Library files, and overwrite companions
are never converted. The settings write and conversions share one reviewed
plan, with the existing safety checks and recovery. Body content, including
fenced examples, stays unchanged. Native Skills keep their own metadata.

The text plan shows `Frontmatter: root` or, for a change,
`Frontmatter: scoped -> root`. When files are kept, it adds
`Kept <n> files in their previous form.` Paths and reasons appear at
`standard` detail and above. Kept files add no finding code.

Install JSON exposes `data.frontmatter` separately from `data.configuration`.
It always contains `form` when the form is resolved, includes `previousForm`
only when the form changed, and includes `kept` at every detail level when
files were kept.
Each kept item has a `path` and a `reason` of `edited` or `source-unavailable`.
For example, this is the `data.frontmatter` object for a scoped-to-root change
that kept an edited file:

```json
{
  "form": "root",
  "previousForm": "scoped",
  "kept": [
    {
      "path": ".agents/directives/_directives.md",
      "reason": "edited"
    }
  ]
}
```

Only the selected root or Memory state's exact exclusions and necessary
ancestor exclusions may change. Unselected ownership, unrelated omissions, and
unknown settings members remain preserved. To add just one omitted canonical
Core category, [`route init --framework`](#route-init) remains
available.

##### Adopt an existing workspace

Install can also adopt an existing `.agents/` tree in the selected standard
routes. It completes missing `name` and `description` fields in native
`SKILL.md` frontmatter and creates needed navigation entrypoints. Ordinary
Markdown without optional frontmatter stays unchanged. Valid native metadata,
unknown YAML fields, Skill bodies, references, scripts, assets, and other
support-file bytes stay intact. Skills remain native `SKILL.md` packages,
without an Open Forge wrapper or competing Skill-root entrypoint. New resource
catalogues and migrated original Skill files remain user-owned. Framework
manages only its generated navigation regions in those catalogues.

During first installation, an unowned file at a bundled category entrypoint
path in a selected route is preserved in its adjacent overwrite companion. For example, existing
content in `.agents/guidance/_guidance.md` moves to
`_guidance.overwrite.md`, and the Framework base is installed at `_guidance.md`.
Existing overwrite content stays last, and generated `Entries` are rebuilt in
the base. An unowned native Skill at the bundled `SKILL.md` path stays in place.
These preservation cases do not require `--force`.

Previews report planned migration paths. Applied results report migration paths
only after verification. JSON results include `data.migrations` only when
migration rows exist. Safe adoption needs no special migration flag or
`--force`. After adding a downloaded Skill, run `install` again when the
managed Framework matches the embedded payload to adopt its missing metadata
or navigation. After a successful migration, another install makes no
migration changes. `install` does not reconcile a divergent managed payload.
after upgrading the CLI, use `update` for that reconciliation and compatible
Skill adoption.

Install reports malformed or ambiguous metadata and unsafe conflicts with the
affected paths. It does not repair arbitrary invalid YAML or overwrite edited
managed content. Existing `--force` and update boundaries still apply.

`--force` permits eligible existing files to be replaced while management
is being established. It does not adopt arbitrary existing content or bypass a
conflict. `--automatic` removes prompting but adds no force or safety authority.

Restore one left-out built-in category with [`route init <category> --framework`](#route-init), previewed with `--dry-run` first. Use `install --configure` for the interactive way to change several setup choices. Name the same preset in a fresh installation preview and its apply request. Omitting it uses Full Core unattended, while the first interactive choice is Essentials.

An Install retry command preserves the request, including the selected
workspace, resolved preset and frontmatter choices, Custom rows, force, and
output options. Wizard answers are included even when no flag supplied them. It adds
`--automatic` only when missing confirmation prevented the request from running.

#### Next step

Review the new files with `git status` and `git diff`, including untracked files,
then commit the setup and give your agent a normal task. If your agent tool does
not read `AGENTS.md` on its own, ask it to read that file first. Use
[`context`](#context) or [`status`](#status) when you need to inspect the setup.

### `update`

Update managed Framework files to the payload bundled with the running CLI.

#### Common examples

```sh
open-forge update --dry-run
# Review the plan before applying it.
open-forge update --automatic
```

To preview optional pruning:

```sh
open-forge update --prune --dry-run
```

#### Options

- `--dry-run` previews the update.
- `--automatic` skips confirmation.
- `--prune` permits eligible retired managed files to be deleted.
- `--force` is accepted but adds no authority.

See [global options](#global-options) for shared flags.

#### Behavior notes

`update` reconciles managed Framework content with the current embedded payload.

After upgrading the CLI, use `update --dry-run` and then plain `update` for an
already managed Framework. `update` also adopts compatible native Skills while
reconciling older or edited Framework content it owns, under the same metadata,
navigation, ownership, and conflict rules described under `install`. Previews
report planned migration paths. Applied paths appear only after verification.
The adoption totals in the preview and result count the same selected files,
including files that only need metadata or navigation changes. JSON results
include `data.migrations` only when rows exist. Normal Update
confirmation, recovery, verification, exclusions, and foreign-owner boundaries
apply. `--automatic` skips confirmation but grants no force. After a successful
migration, repeating `update` makes no further migration changes. `update` does
not establish management for an absent or unknown installation. Safe adoption
needs no migration flag or `--force`.

Managed `AGENTS.md` and `CLAUDE.md` hosts use a visible boundary. The
canonical form starts with a top-level `# Open Forge` heading and ends with a
standalone `**End of Open Forge managed section.**` paragraph. New installs
write this form. Update still accepts the exact legacy pair
`<!-- open-forge:start -->` and `<!-- open-forge:end -->` as input.
Instructions and Claude imports remain ordinary Markdown between the
boundaries. Put user instructions before or after the managed span so Update
leaves them outside managed content.

To convert an existing legacy host, preview the conversion with
`open-forge update --dry-run`, then apply it with
`open-forge update --automatic`. This uses the existing Update ownership,
confirmation, recovery, and preservation rules. It preserves the exact bytes
outside the recognized managed span, so no migration flag or `--force` is
needed solely for this syntax change. `index` maintains routed metadata and
navigation and does not migrate these hosts.

Missing, reversed, duplicated, mixed, or otherwise ambiguous boundaries block
writes. A heading without its named closing paragraph is incomplete and does
not make the rest of the file managed.

Normal update replaces changed owned files and restores missing ones when the
current ownership facts authorize the effect. It reports every replaced,
restored, deleted, and retained path. Retired managed content is retained unless
`--prune` is supplied. `--force` is accepted but adds no authority, because
normal update already replaces and restores. `--automatic` implies neither
`--force` nor `--prune`. When the plan needs a recovery bundle to protect
existing bytes, Update retains it after successful verification. The result
identifies that bundle as the source of previous content. In a Git repository,
`git diff` helps review changes, but it cannot recover untracked files or edits
you never saved. Use `--detail standard` for the bundle's exact path. Explicit
Cleanup removes retained recovery data after review.

Ownership state is recorded in `.agents/open-forge.lock.json`. Authored settings
are in `.agents/open-forge.json`. Framework and Extension records share the lock
but remain independent command domains.

The settings key `frontmatter` accepts `root` or `scoped`. A missing key means
scoped, and unsupported values are invalid settings. New settings use `schemaVersion` 2:

```json
{
  "schemaVersion": 2,
  "frontmatter": "root"
}
```

The setting controls output, not reading. Install, Update, and Extension
delivery use it for leading Open Forge metadata. Their managed-file comparisons
use the same form. Repository sources and body examples stay unchanged.
To change an installed workspace, use `install --configure --frontmatter root`
or `install --configure --frontmatter scoped` so the setting and eligible file
conversions happen together.

#### Next step

Review changed files and retained recovery data, then preview [`cleanup`](#cleanup) when the result has been checked.

### Choosing which links to check

Add `ignoreLinksFrom` or `ignoreLinksTo` to `.agents/open-forge.json`:

```json
{
  "$schema": "https://raw.githubusercontent.com/TheLithiumForge/open-forge/main/schemas/v2/open-forge.schema.json",
  "schemaVersion": 2,
  "ignoreLinksFrom": [".agents/memory/archived"],
  "ignoreLinksTo": []
}
```

Entries name workspace-relative files or folders, not patterns. A file entry covers that file. A folder entry covers everything beneath it. Paths use the same case-insensitive comparison as `allowInstallPaths`.

`ignoreLinksFrom` skips links written inside a listed path. Links into that path from elsewhere stay checked, and Repair can fix them when an archived file moves. `ignoreLinksTo` skips links whose resolved target lies inside a listed path. Doctor, Repair, Context link expansion, Route Move and Route Remove honor these settings. References still lists every link.

Doctor shows how many links it ignored and notes listed paths that no longer exist. Route Move updates matching entries. Route Remove drops them. Both preserve ignored links as written. Install adds the archive entry when it installs Memory archive and the key is absent. An existing key, even `[]`, is your choice. Update leaves your settings unchanged. Add the entry by hand in an existing workspace.

An older CLI ignores settings keys it does not know. A beta14 CLI ignores both ignore lists. Doctor reports an unrecognized settings version as information and continues with the keys it understands.

<a id="remove-and-keep-removed"></a>

### `remove`

Remove one selected path, route, Extension, or Library and record that it should stay removed.

#### Common examples

```sh
open-forge remove .agents/templates --dry-run
# Review the plan before applying it.
open-forge remove .agents/templates --automatic
```

Typed selections:

```sh
open-forge remove planning --kind extension --dry-run
open-forge remove team-knowledge --kind library --dry-run
open-forge remove guidance/old-note --kind route --dry-run
open-forge remove docs/obsolete.txt --dry-run
```

#### Options

- `--kind <path|route|extension|library>` selects the target kind. Default: `path`.
- `--dry-run` previews removal.
- `--automatic` skips confirmation.
- Repeat `--allow-path <path>` for supported additional destination permissions.

See [global options](#global-options) for shared flags.

#### Behavior notes

Use `remove` when content should stay removed through later Open Forge installs,
updates and Library synchronization. It accepts one workspace-relative path by
default. Use `--kind` for a route reference, package ID or Library ID. The workspace
root (`.` or `./`) cannot be removed.

A file selection removes that file. A directory selection removes its complete
tree. Routed leaves also include their adjacent overwrite and update supported
incoming links and generated navigation. Naming an entrypoint file does not
silently delete its folder: select the directory or use `--kind route` for the
whole category. Removing a Library detaches its local links and registration.
Its source files stay untouched. An individual owned Library link can also be
removed by path while retaining the registration.

The existing `route remove`, `extension remove` and `library detach` commands
remain available and record the same removal intent. Package removal retains
files still owned by another package. Explicit file removal removes that file
for every manager and releases its matching ownership claims.

Removal records exclusions in `.agents/open-forge.json`:

| Setting              | Keeps removed                                            |
| -------------------- | -------------------------------------------------------- |
| `removedCategories`  | Root categories under `.agents`                          |
| `removedFiles`       | Exact workspace-relative file destinations               |
| `removedDirectories` | Directories and every descendant, including future files |
| `removedExtensions`  | Packages, including requests through dependencies        |
| `removedLibraries`   | Local Library registrations                              |

Paths use `/`, with no trailing slash or wildcard. Excluded existing files stay
untouched. Force, prune and automatic mode do not bypass exclusions. Unknown
settings keys are preserved when the CLI writes the settings. JSON comments
need not survive. Deleting a file by hand does not infer an exclusion.

```json
{
  "schemaVersion": 2,
  "removedCategories": ["skills"],
  "removedFiles": [".agents/patterns/_patterns.md", "AGENTS.md"],
  "removedDirectories": ["docs/obsolete"],
  "removedExtensions": ["planning"],
  "removedLibraries": ["team-knowledge"]
}
```

To restore one selected built-in category, preview `open-forge route init <category> --framework --dry-run`, review it, then apply the same request without `--dry-run`. This preserves narrower omissions and authored content. Use [`install --configure`](#setup-choices) as the interactive way to change several setup choices. A broader excluded ancestor requires
an explicit add first for `route init`. Alternatively, remove every applicable
exclusion from the settings and run the relevant install, update, attach, or
sync command. Use `update` when managed Framework content needs reconciliation.
For individual Library
links whose registration remains, use `library sync <id>`. Use `library attach`
when the registration was removed. Recreate ordinary user files
from your own source or backup. Clearing an exclusion does not recover deleted
bytes. A required excluded dependency or missing excluded ancestor is reported
rather than silently restored.

Review `--dry-run` before applying a removal. Protected controls, Git metadata,
Library source trees and unsafe links remain protected even with `--automatic`.
The [Remove contract](../.agents/memory/crystallized/documents/cli/contracts/remove/_remove.md)
defines the complete selection and recovery boundary.

#### Next step

Inspect exclusions and remaining content, then run [`doctor`](#doctor).

### `cleanup`

Delete recognized Open Forge recovery bundles and drafts after the changes have been checked.

#### Common examples

```sh
open-forge cleanup --dry-run
# Review the plan before applying it.
open-forge cleanup
```

#### Options

Its only command-specific option is `--dry-run`. Cleanup has no operands, selectors, age or glob filters, force mode, or confirmation prompt.

See [global options](#global-options) for shared flags.

#### Behavior notes

`cleanup` removes only recognized Open Forge recovery bundles and drafts for
the selected workspace. The bundles are stored outside the workspace, and
`status` reports the ones that remain.

Install compares a verified bundle with the current files before deciding
whether it blocks the operation. Fully restored, fully applied and superseded
snapshots remain as history. A current partial operation or an unsafe or
uninspectable record still blocks. Reusing a worktree or changing its files can
make a record obsolete immediately. There is no waiting period.

It does not scan arbitrary workspace files, extract a bundle, restore a target,
or create a replacement bundle for its own support-artifact deletion. Use the
preview to review the exact recognized candidates before applying cleanup.

The lifecycle and recovery contracts contain the complete safety and residual
state rules:

- [Install contract](../.agents/memory/crystallized/documents/cli/contracts/install/_install.md)
- [Update contract](../.agents/memory/crystallized/documents/cli/contracts/update/_update.md)
- [Cleanup contract](../.agents/memory/crystallized/documents/cli/contracts/cleanup/_cleanup.md)

#### Next step

Use [`status`](#status) to check remaining recovery observations.

## Extension operations

Extensions are optional packages of routed Framework content and supporting
capabilities. A local package has `extension.json` beside `content/`. Its manifest
defines `id`, `name`, `description`, `version`, and `dependencies`. Use exact
lowercase stable IDs. See [Extension packages](extensions.md) for structure and
manual use.

`--source` selects one exact local package or catalogue. Install and Update read
that source without changing it, and it must be separate from the target
workspace. Without `--source`, they use the embedded catalogue. Dependencies
resolve offline within the selected source and install first. There is no
network, registry, cache, or fallback lookup.

`--allow-path <path>` grants supported write permission for a destination outside
`.agents` and records it in `.agents/open-forge.json`. It adds no task authority.
Unattended writing operations that confirm need `--automatic` and an explicit
package selection or supported `--all`. With no IDs, interactive Install, Update,
and Remove offer package selection.

A manual installation is also valid: copy reviewed package content into the
workspace, rebuild affected Entries, and review the assembled diff. Manual
copying does not create managed lifecycle state.

### `extension list`

List installed and available Extension packages.

#### Common examples

```sh
open-forge extension list
open-forge extension list --available
open-forge extension list --installed
```

#### Options

- `--installed` selects managed packages in this workspace.
- `--available` selects available packages.
- `--source <package-or-catalogue-path>` selects an exact local source.

See [global options](#global-options) for shared flags.

#### Behavior notes

Without section flags, both Installed and Available are shown. Either flag selects its section, and both show both. Available does not mean installed. Unknown ownership is reported as unavailable coverage.

#### Next step

Inspect a chosen ID with [`extension inspect`](#extension-inspect).

### `extension inspect`

Show one Extension's files, dependencies, ownership, and intended content.

#### Common examples

```sh
open-forge extension inspect development-toolkit
```

#### Options

`--source <package-or-catalogue-path>` selects an exact local source.

See [global options](#global-options) for shared flags.

#### Behavior notes

Name one stable ID. Inspection changes nothing and executes no package content. See [shared Extension rules](#extension-operations) for source selection.

The package count includes the named package and its required packages.
For Planning, `2 packages, including planning.` means Planning and Workflow
Support, not two additional dependencies. Standard detail lists them under
`Packages in this selection`.

#### Next step

Preview installation or update of the selected package.

### `extension create`

Create a local Extension package scaffold under an existing catalogue.

#### Common examples

The `catalogue` directory must already exist.

```sh
open-forge extension create team-rules --path catalogue --dry-run
# Review the plan before applying it.
open-forge extension create team-rules --path catalogue --automatic
```

#### Options

- `--path <catalogue-path>` is the required existing parent. Only its `<stable-id>/` child is inspected or created.
- `--name <text>` sets the display name. Default: words from the ID.
- `--description <text>` sets the description. Default: `Open Forge Extension package <stable-id>.`
- `--package-version <text>` sets the descriptive version. Default: `0.1.0`.
- Repeat `--dependency <stable-id>` for distinct non-self dependencies. Creation does not resolve availability.
- `--dry-run` previews the scaffold.
- `--automatic` suppresses questions and confirmation.

See [global options](#global-options) for shared flags.

#### Behavior notes

The stable ID and destination are required to plan creation. Interactive text can ask for missing inputs. Metadata overrides are nonblank and supplied once. The scaffold contains `extension.json` and `content/.agents/`. Identical existing content is a no-op. Partial, divergent, additional, or colliding content blocks creation. The parent is never created. Unattended creation requires explicit ID and `--path`, plus `--automatic` when applying writes. `--workspace` has no effect on the explicit catalogue destination.

The parent prompt says `Existing parent folder for the new Extension:` and
explains that the CLI creates a folder named after the stable ID inside it.

If creation stops partway through, created files stay in the package folder.
Review that folder before retrying. Create does not make a recovery bundle or
install the package in a workspace.

#### Next step

Fill `content/` with reviewed files, then inspect the local package using `extension inspect team-rules --source catalogue`.

### `extension install`

Install selected Extensions and the dependencies they require.

#### Common examples

On Essentials, restore Templates before installing Planning or the toolkit for its supplied Templates:

```sh
open-forge route init templates --framework --dry-run
# Review the plan before applying it.
open-forge route init templates --framework
```

After restoration, discover and inspect the toolkit:

```sh
open-forge extension list --available
open-forge extension inspect development-toolkit
```

```sh
open-forge extension install development-toolkit --dry-run
# Review the plan before applying it.
open-forge extension install development-toolkit --automatic
```

#### Options

- `--source <package-or-catalogue-path>` selects the separate local source.
- `--all` selects every eligible package from that source.
- `--force` permits eligible initial replacement of existing content.
- Repeat `--allow-path <path>` for supported destination permissions.
- `--dry-run` previews packages, dependencies, and file changes.
- `--automatic` skips prompts without selecting packages or adding force.

See [global options](#global-options) for shared flags.

#### Behavior notes

Select explicit IDs, `--all`, the sole package in a one-package source, or an interactive choice. Use Extension Update for already managed content. Packages honor exclusions. Essentials leaves Templates out, so restore that category before installing Planning or `development-toolkit` when their Templates are needed. Confirm `find --tag Template` lists `templates/planning/decision`. For an already installed package with skipped files, follow the restoration and update guidance below. See [shared Extension rules](#extension-operations).

A fully excluded package is recorded as installed without copying package
files. The result names the skipped files and excluded category and suggests a
`route init <category> --framework --dry-run` preview. It does not claim the
package was already installed. Plans and receipts also name partially excluded
packages and explain which files are skipped. For example, Planning's starters
are skipped while Templates is left out, but its other enabled files install.
The guidance previews restoring the category, then updating each affected
package. Review and apply the restoration before previewing the update. Those
commands keep the selected workspace and any local package source.

A cancelled installation reports no changes and does not claim that Entries
were updated.

#### Next step

Run `extension list --installed` and check the expected files and Templates.

### `extension update`

Update selected managed Extensions from the bundled or selected local source.

#### Common examples

After installing the packages:

```sh
open-forge extension update --all --dry-run
# Review the plan before applying it.
open-forge extension update --all --automatic
```

#### Options

- `--source <package-or-catalogue-path>` selects the separate local source.
- `--all` selects managed packages represented by that source.
- `--prune` permits eligible retired file deletion.
- `--force` is accepted but adds no authority.
- Repeat `--allow-path <path>` for supported destination permissions.
- `--dry-run` previews the update.
- `--automatic` skips prompts without selecting packages.

See [global options](#global-options) for shared flags.

#### Behavior notes

Select exact managed IDs, `--all`, or an interactive choice. Dependencies are processed within the selected source. Extension Update replaces files you edited and restores missing eligible files, retaining previous bytes in a recovery bundle. Its force and prune boundaries follow Framework Update. See [shared Extension rules](#extension-operations).

#### Next step

Inspect the updated package and review changed files before previewing [`cleanup`](#cleanup).

### `extension remove`

Remove selected Extension management and files no retained package owns.

#### Common examples

After installing the toolkit:

```sh
open-forge extension remove development-toolkit --dry-run
# Review the plan before applying it.
open-forge extension remove development-toolkit --automatic
```

Removing `development-toolkit` leaves its dependency packages installed. Apply the selected removal before previewing removal of its dependencies. The applied result suggests dependency-removal commands. Preview each package you also want to remove. A dependency cannot be removed while a retained package still needs it. When no retained package requires Development, the next preview can be:

```sh
open-forge extension remove development --dry-run
```

#### Options

- Repeat `--allow-path <path>` for supported destination permissions.
- `--dry-run` previews deletions, exclusions, and retained shared files.
- `--automatic` skips confirmation without selecting packages or expanding deletion.

See [global options](#global-options) for shared flags.

#### Behavior notes

Select exact managed IDs or choose interactively. Extension Remove deletes an eligible file, including an edited file, once no retained package owns it. Previous bytes are kept in a recovery bundle. Files owned by another installed package remain. Dependency packages remain installed when their umbrella is removed. See [shared Extension rules](#extension-operations).

A removal preview names every selected Extension in its summary, including
packages with no installed files. Required-package counts use `1 package` for
a single package.

The preview describes dependencies that would remain installed. Its `Next:`
command applies the selected Extension removal with the same workspace and
options, without `--dry-run`. Dependency-removal commands appear after that
removal is applied. Retained shared-file rows name the packages that still own
the file.

#### Next step

Run `extension list --installed` and preview any further package removal you want.

## Workspace libraries

Workspace Libraries let a workspace register a contained source root and
project it into another workspace location through relative file links.

### `library list`

List registered shared folders and their link status.

#### Common examples

```sh
open-forge library list
```

#### Options

No command-specific options.

See [global options](#global-options) for shared flags.

#### Behavior notes

This is a bounded report of Library records and link observations. It does not scan every source file. A Library management ID is distinct from a source ID.

#### Next step

Inspect a listed ID with [`library inspect`](#library-inspect).

### `library inspect`

Compare one registered Library's complete source inventory and projected links.

#### Common examples

For an already attached `team-rules` Library:

```sh
open-forge library inspect team-rules
```

#### Options

No command-specific options.

See [global options](#global-options) for shared flags.

#### Behavior notes

Inspection writes nothing. Use [`doctor`](#doctor) when the record, source root, link capability, permission, or projection is blocked.

On Windows, legacy links with forward slashes in their on-disk targets can be
unreadable. Inspect, List, Doctor, and Sync report the problem and provide
detach and attach commands. Follow [recreate legacy Windows links](#recreate-legacy-windows-links)
before using the affected files.

#### Next step

Preview [`library sync`](#library-sync) when source files and links differ.

### `library attach`

Register a shared folder and project its files through relative file links.

#### Common examples

`vendor/team-rules` must exist and contain reviewed source files. This example projects them into `docs/team`:

```sh
open-forge library attach team-rules vendor/team-rules --to docs/team \
  --git-ignore true --allow-path docs/team --allow-path .gitignore --dry-run
open-forge library attach team-rules vendor/team-rules --to docs/team \
  --git-ignore true --allow-path docs/team --allow-path .gitignore --automatic
```

#### Options

- `--to <workspace-relative-directory>` selects the projection root. Default: workspace root.
- `--git-ignore <true|false>` selects whether to ignore projected links.
- `--dry-run` previews the complete plan.
- `--automatic` skips prompts.
- Repeat `--allow-path <path>` for supported destination permissions.

See [global options](#global-options) for shared flags.

#### Behavior notes

Add `--dry-run` to `attach`, `sync`, or `detach` to inspect the complete plan
before writing. They also accept `--automatic` and `--allow-path <path>`.
`attach --to` selects the workspace-relative projection directory and defaults
to the workspace root. Each projected file is a relative file symlink. A file
that lands under `.agents` joins an existing route: its folder and entrypoint
must already exist, and the command updates that entrypoint's `Entries`. A
Library ID is a management identity. It is not a source ID.
Use `doctor` when a record, source root, link capability, permission, or
projection is blocked.

On-disk link targets use the platform's path separator. Recorded paths stay
portable, and ownership checks accept either separator.

##### Keep projected links out of Git

Interactive Attach asks whether to Git-ignore the projected links, with No as
the default. Use `--git-ignore true` when the source folder supplies the files
and you want each checkout to recreate its local links. Use
`--git-ignore false` when you want to commit those links, or skip the question.

Make the choice explicit in scripts and previews, as in the example above.

The plan lists the projected paths and any `.gitignore` change. Writing that
file needs the same shared path permission as other destinations. Interactive
use can approve it once or save the grant. Automatic, JSON, redirected and
dry-run requests never ask the ignore question. When the flag is omitted in
those modes, Attach leaves `.gitignore` unchanged.

Rules cover exact projected paths, such as `/docs/team/review.md`, in a separate
Library-owned section. They work with `--to .` and shared folders without hiding
unrelated files or the source folder. Authored rules and the Install-owned
section remain intact. Already tracked files stay tracked, and Git's normal
rule precedence still applies. See the [Git ignore manual](https://git-scm.com/docs/gitignore).

If the source itself projects a root `.gitignore`, opt-in would need to write
that same path and is blocked. Interactive omission reports the conflict before
asking. Use `--git-ignore false` to attach that file as ordinary Library content.

`library sync` remembers the choice and updates owned rules when source
membership changes. Detach, `remove --kind library`, and removal of an individual
projected link release the corresponding owned rules while preserving other
Libraries. Sync and Detach need permission for `.gitignore` when they change it.
An explicitly removed ignore file is not silently restored. Ignoring a link
does not change Open Forge context loading or authorize writing its source.

##### Recreate legacy Windows links

Save the Library ID, source root, projection root, and Git-ignore choice from
Inspect before detaching. Preview and review each operation before applying it:

```sh
open-forge library detach team-rules --dry-run
open-forge library detach team-rules --automatic
```

Detach keeps source files and records the Library ID in `removedLibraries` in
`.agents/open-forge.json`. Remove only that ID from the list, then recreate the
registration with the same source, destination, and ignore choice:

```sh
open-forge library attach team-rules vendor/team-rules --to docs/team \
  --git-ignore true --allow-path docs/team --allow-path .gitignore --dry-run
open-forge library attach team-rules vendor/team-rules --to docs/team \
  --git-ignore true --allow-path docs/team --allow-path .gitignore --automatic
open-forge library inspect team-rules
```

Use the reported commands for your Library's exact options and workspace.
Supply any reported destination or `.gitignore` permission. Sync does not
replace legacy unreadable links, and Repair does not convert their targets.

#### Next step

Run [`library inspect`](#library-inspect) and check each destination link and its source.

### `library sync`

Update one attached Library's links to match its current source files.

#### Common examples

For an already attached `team-rules` Library:

```sh
open-forge library sync team-rules --dry-run
# Review the plan before applying it.
open-forge library sync team-rules --automatic
```

#### Options

- `--dry-run` previews the complete plan.
- `--automatic` skips prompts.
- Repeat `--allow-path <path>` for supported destination permissions.

See [global options](#global-options) for shared flags.

#### Behavior notes

Sync reads the complete source inventory and preserves source files. It remembers the Git-ignore choice and updates Library-owned rules as files come and go. Permission for `.gitignore` is required when those rules change, unless a saved grant covers it. Other authored rules and Libraries remain intact. See [Library permissions and ignore rules](#keep-projected-links-out-of-git).

#### Next step

Run [`library inspect`](#library-inspect) and verify new and removed links.

### `library detach`

Remove one Library's registration and projected links while preserving its source files.

#### Common examples

For an already attached `team-rules` Library:

```sh
open-forge library detach team-rules --dry-run
# Review the plan before applying it.
open-forge library detach team-rules --automatic
```

#### Options

- `--dry-run` previews the complete plan.
- `--automatic` skips prompts.
- Repeat `--allow-path <path>` for supported destination permissions.

See [global options](#global-options) for shared flags.

#### Behavior notes

Detach releases corresponding Library-owned Git-ignore rules and preserves authored rules and other Libraries. It needs `.gitignore` permission when changing that file. Source files remain unchanged. See [Library permissions and ignore rules](#keep-projected-links-out-of-git).

#### Next step

Use [`library list`](#library-list) and check that the source folder remains intact.

## Source references

Existing `.agents` sources accept one of these forms:

```text
<source-id>
.agents/<path>
./.agents/<path>
```

The first two examples address the same route. The third shows quoting
for a custom scope whose folder name contains a space. Create that scope before
selecting it.

```sh
open-forge context memory/crystallized/documents
open-forge context .agents/memory/crystallized/documents/_documents.md
open-forge context "memory/crystallized/documents/project alpha"
```

An automatic source ID is derived from the canonical path below `.agents`:

- `/` separates segments on every platform.
- `.md` is removed from Markdown file names.
- A recognized entrypoint uses its containing folder ID rather than
  `_{folder}.md`.
- `SKILL.md` uses its containing Skill folder ID.
- Exact case, spaces, and Unicode are preserved.

The `.agents/` prefix means an exact workspace path. Any other value is an ID,
even if it looks like a relative filesystem path. Use the exact `.agents/...`
form when an ID has more than one candidate. The CLI does not use fuzzy,
case-correcting, or likely-intent matching.

An adjacent `{name}.overwrite.md` file shares its base source's ID and route.
The base is read first and the overwrite second. The overwrite is not an
independent source. An orphan overwrite is broken evidence rather than a valid
standalone source.

Quote one complete operand when an ID or path contains spaces or special
characters:

```sh
open-forge route inspect "memory/project alpha/documents"
```

The shell removes the quotes before the CLI receives the value. Exact paths
resolve inside the selected workspace and cannot escape it through lexical or
physical aliases.

## .gitignore fallback rules

When `route inspect --matching-files` walks the workspace instead of asking
Git, it translates `.gitignore` patterns for the Open Forge matcher using these
ten rules:

1. Read a directory's regular `.gitignore` file before looking at its children.
   Never enter an excluded directory. Don't read anything above the workspace,
   and ignore global excludes and `.git/info/exclude`. Tracked files get no
   exemption.
2. Skip empty lines and lines that start with `#`. Keep leading spaces, and
   remove only trailing spaces that aren't escaped. An initial `\#` or `\!`
   stands for a literal `#` or `!`.
3. Every entry starts out included. A matching rule excludes it, and a rule
   with an unescaped initial `!` includes it again. Apply ancestor rules before
   local rules, keeping line order. The last matching rule decides whether the
   entry is included.
4. A leading or internal slash anchors a pattern to the directory that holds
   its `.gitignore`. Remove the leading slash before matching.
5. Otherwise, remove any directory-only trailing slash and then prefix `**/`.
   For example, `*.log` becomes `**/*.log`.
6. A trailing slash matches only directories. Apply every applicable rule
   before pruning an excluded directory. Once a directory is pruned, nothing
   beneath it can be included again.
7. Use Open Forge matching for `*`, `?`, bracket lists and ranges, and negated
   character classes. Matching stays case-sensitive regardless of Git settings.
8. Keep recursive `**` segments, but translate a terminal `/**` to `/**/*`.
   That way `docs/**` excludes everything inside `docs` without excluding
   `docs` itself, so `!docs/keep.md` can still include that file.
9. Treat commas and braces as literal characters. Outside brackets, an escaped
   character is literal, including escaped spaces and wildcards.
10. Skip and count unsupported lines: extended POSIX bracket constructs, escapes
    inside brackets, escaped slashes or literal backslashes, dangling escapes,
    and patterns the Open Forge matcher rejects.

The fallback differs from Git in four ways: which ignore files it reads, how it
treats tracked files, case sensitivity, and unsupported patterns.

When the walk skips unsupported lines, it adds a
`Skipped unsupported .gitignore lines: N.` note. The note doesn't make the scan
incomplete or raise its status. If a `.gitignore` file can't be read or
decoded, the command discards partial results and reports the scan as
unavailable, with reason `files-unavailable` and exit code `3`.

## Where to go next

Use [Development and setup](development.md) for local prerequisites and
worktree-local setup. The command contracts contain the exact public boundary
for operations that need more detail:

- [Global flags](../.agents/memory/crystallized/documents/cli/contracts/shared/global-flags/interface.md)
- [Source references](../.agents/memory/crystallized/documents/cli/contracts/shared/source-references/interface.md)
- [Context](../.agents/memory/crystallized/documents/cli/contracts/context/_context.md)
- [Find](../.agents/memory/crystallized/documents/cli/contracts/find/_find.md)
- [Route commands](../.agents/memory/crystallized/documents/cli/contracts/route/_route.md)
- [Index](../.agents/memory/crystallized/documents/cli/contracts/index-candidate/_index-candidate.md)
- [Repair](../.agents/memory/crystallized/documents/cli/contracts/repair/_repair.md)
- [Extension commands](../.agents/memory/crystallized/documents/cli/contracts/extension/_extension.md)
- [Workspace Libraries](../.agents/memory/crystallized/documents/cli/contracts/library/_library.md)

The Framework stays usable when no executable is present. Plain Markdown is
the durable interface. The CLI makes common inspection and maintenance tasks
faster, more repeatable, and easier to review.
