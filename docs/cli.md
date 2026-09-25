# Open Forge CLI

The Open Forge CLI helps you find context, inspect routes, keep generated
navigation correct, and manage Framework files. It's an accelerant, not a
requirement: the Framework is complete in plain files, and you can read, edit,
and follow its rules without the CLI. The commands automate the work around
those files. The files still define what the Framework means.

This guide is the reference: every command and its options. For which command
helps with which job, and the everyday flows that tie them together, start with
[Working with the CLI](https://thelithiumforge.github.io/open-forge/docs/cli).

To see the exact options of the executable you have, start here:

```sh
open-forge --help
```

For repository setup, build prerequisites, and a worktree-local executable,
see [Development and setup](development.md). For package structure and manual
Extension use, see [Extensions](extensions.md).

## The command shape

Commands use this general form:

```text
open-forge <command> [command options] [global options]
```

Use `open-forge --help` to see commands and `open-forge <command> --help`
for arguments, options, and examples. Help wraps to the terminal width.
Redirected help uses 80 columns. Long source references stay intact.

The CLI works against the current directory unless `--workspace` selects one
explicit directory. A source reference is either an automatic source ID, such
as `memory/crystallized/documents`, or an exact path under `.agents`, such as
`.agents/memory/crystallized/documents/_documents.md`. The exact grammar is
covered in [Source references](#source-references).

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

`--detail-filter` changes which findings are listed, not their totals or the
operation's status. JSON records the selected detail and filter in one schema-3
document with `summary`, `findings`, `effects`, `counts`, `limitations`, `data`,
`recovery`, and `next` members. The text and JSON forms use the same typed
operation result and never rerun the operation.

In text mode, `completed`, `completed-with-warnings`, and `incomplete` results
use standard output. `failed`, `invalid-input`, `blocked`, and `cancelled`
results use standard error. JSON always uses standard output. Human results use
colour automatically on capable terminals: green for completed, yellow for
completed-with-warnings and warnings, red for errors, and cyan for information
labels. Written labels remain visible. Redirected output, Windows, missing or
`dumb` `TERM`, and a nonempty `NO_COLOR` use plain text. JSON, selected file
content, and preview diffs stay plain. There is no colour option to configure.

### Prompts and noninteractive runs

Mutating commands that need confirmation show their already-built minimal plan
on standard error and ask the command-specific question. Install and Update use
`Apply these changes? [y/N]`. Update includes the eligible prune deletion count
when `--prune` is supplied. Library Attach, Sync, and Detach use the same
confirmation boundary and expose `--automatic` to bypass only that question.

Permission prompts list the affected path and scope. A directory grant means
everything beneath that directory. `--allow-path <path>` supplies the same
grant noninteractively. Explicit grants and interactive Always answers are
published only after final confirmation. A declined, exhausted, or cancelled
prompt leaves files and settings unchanged and returns `cancelled` with exit
`130`. JSON, redirected operation, `--automatic`, and dry-run never prompt.

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

`status` gives a bounded summary of workspace structure, startup context,
generated navigation, Framework and Extension lifecycle state, Libraries, and
recovery observations.

```sh
open-forge status
open-forge status --detail minimal
open-forge status --format json --detail standard
```

It is a report. It does not repair stale navigation, adopt changed files, or
make a lifecycle record trusted. Use `doctor` when the summary names a fact
that needs diagnosis.

### `context`

`context` returns ordered startup context or the selected sources added to it.
With no source arguments it returns the startup files. Add source IDs or
exact paths to include the context you select.

```sh
open-forge context
open-forge context memory/crystallized/documents
open-forge context \
  templates \
  --content=frontmatter,headings \
  --additions-only
```

Useful options are:

- `--additions-only` omits files already required at startup.
- `--content=part[,part...]` projects selected parts, such as `metadata`,
  `frontmatter`, `headings`, `body`, or a named `section:<heading>`.
- `--follow-links=positive-depth|all` follows contained local Markdown links to
  the requested positive depth or through the complete reachable link set.

Use the returned file identities to inspect what the command included. The
[loader](../src/open-forge/.agents/loader.md) defines Framework loading and scope.
Command output does not change those rules.

`LoadNow` and `KeepInMind` both load through exposed entries of already-loaded
parents. Selecting an on-demand scope activates its applicable child loading
rules. Tagged files inside other inactive scopes stay excluded. `KeepInMind`
adds refresh instructions while the scope remains active. The CLI resolves each
invocation independently and does not track an agent session.

### `find`

`find` selects Markdown sources by authored tags and structural headings. It
does not treat arbitrary body text as a substitute for route or metadata
meaning.

```sh
open-forge find --tag=Memory --tag=CurrentTruth --require=all
open-forge find --heading=Axioms --within=body
open-forge find --include=memory/crystallized/documents --content=metadata,headings
```

The selectors are:

- `--include <source-reference>` adds a set of sources to search.
- `--exclude <source-reference>` excludes a set of sources from the search.
- `--tag <tag>` matches one authored tag. Repeat it for additional predicates.
- `--heading <heading>` matches one complete structural heading.
- `--require=all|any` chooses whether all or any supplied predicates must match.
- `--within=part[,part...]` limits predicate evaluation to authored regions.
- `--content=part[,part...]` chooses which parts of matched sources to return.

Use an exact path when an ID is ambiguous. A blocked or incomplete result is
not permission to guess which source was intended.

### `references`

`references` reports direct authored incoming and outgoing Markdown references
for one source. The default direction is `both`.

```sh
open-forge references memory/crystallized/documents
open-forge references memory/crystallized/documents --direction=out
open-forge references memory/crystallized/documents --direction=in \
  --include=memory/crystallized/documents/framework
```

`--direction=in|out|both` selects the report. `--include` and `--exclude`
limit an incoming scan. They do not apply to an outgoing-only request.

### Routes

An `entrypoint` makes a folder routable. A `route` is the navigable path that
the entrypoint and its generated `Entries` expose. Route commands accept the
source-reference grammar described below unless their target has a narrower
shape.

`route list` shows routed sources and descendants at a structural depth. The
default depth is `1`. Use `all` to include the complete routed descendant set.

```sh
open-forge route list
open-forge route list memory/crystallized/documents --depth=all
```

`route inspect` explains one source's route behavior without returning its
authored body.

```sh
open-forge route inspect memory/crystallized/documents
open-forge route inspect .agents/memory/crystallized/documents/_documents.md
```

## Maintain routes and Markdown

The route commands preserve the distinction between authored source content and
generated navigation. They plan the complete bounded effect, recheck the
expected state before writing, and verify the result. Use `--dry-run` before an
operation whose target or metadata needs review.

### Frontmatter and metadata

An indexed Open Forge Markdown source uses this canonical frontmatter shape:

```md
---
open-forge:
  description: Advice for choosing a stable route boundary
  responsibility: Define the boundary and tradeoffs for this advice
  tags: [Guidance, Routing]
---

# Route boundaries

The body contains the source's authored explanation.
```

`description` helps a reader decide whether to open a source. An optional
`responsibility` helps an editor decide what belongs in it by stating what the
file defines. It does not create authority or loading behavior. Tags are bare
values in frontmatter, without `#`. They must be valid and useful for
classification, loading, routing, or search. A tag starts with a letter and
then contains letters or digits, with single internal hyphens allowed. It may
not end with a hyphen or contain two adjacent hyphens.

The command flags use the same concepts:

- `--description <text>` sets one nonblank description.
- Repeated `--tag=<tag>` values provide an ordered tag list. The route create
  and update commands require unique canonical tags when the list is supplied.
- `--responsibility <text>` sets the optional responsibility. On update, an
  exact empty value removes the key.

The CLI does not infer these values from a filename, parent, Template, body, or
generated entry.

### Initialize a route chain

`route init <route-target>` initializes each missing entrypoint in one exact
route chain. A target is a route ID or an exact canonical `.agents` entrypoint
path. The generic scaffold accepts optional metadata:

```sh
open-forge route init memory/crystallized/documents/project-alpha \
  --description="Project Alpha documents" \
  --responsibility="Define the documents route" \
  --tag=Document \
  --tag=ProjectAlpha \
  --dry-run
```

`--framework` selects the trusted embedded Framework scaffold. It cannot be
combined with `--description`, `--responsibility`, or `--tag` because those are
different scaffold choices. `route init` does not initialize the Loader itself.

The following examples use this scope. Apply each previewed creation before
running a command that depends on the new path.

### Create one routed file

`route create <file-target>` creates one ordinary routed Markdown file below an
existing routable parent. It requires a nonblank description and at least one
unique canonical tag. A responsibility is optional. `--template` copies the
body of one existing routed Template as starting content. The new source gets
its own metadata and does not retain Template ownership.

```sh
open-forge route create memory/crystallized/documents/project-alpha/architecture \
  --description="Current service boundaries and request flow" \
  --tag=Document \
  --tag=Architecture \
  --responsibility="Define the current service structure and dependencies" \
  --dry-run
```

The target is an ordinary Markdown file below an existing route. It is not a
directory, entrypoint, overwrite companion, Loader, or general external path.
The command never overwrites an existing target. Use `route update` for an
existing source.

### Update one routed source

`route update <source-reference>` applies one or more explicit metadata
changes, or copies one Template body when the target is eligible.

```sh
open-forge route update memory/crystallized/documents/project-alpha/architecture \
  --description="Current service structure and dependency boundaries" \
  --tag=Document \
  --tag=Architecture \
  --dry-run
```

The options are patches, not inferred replacements:

- `--description` replaces the description.
- Repeated `--tag=<tag>` values replace the complete ordered tag list.
- `--responsibility <text>` sets the responsibility. `--responsibility ""`
  removes it.
- `--template <template-reference>` completes an eligible frontmatter-only
  body. If the target already has authored body content, the CLI preserves it
  and reports the applicable warning status rather than overwriting it.

At least one metadata or Template operation is required. The selected Template
contributes body content only. Its frontmatter and continuing lifecycle do not
transfer to the destination.

### Move or remove a route

`route move` moves one routed source or category to an exact destination path.
Review the destination's scope and inherited rules when choosing the new location.

```sh
open-forge route move \
  memory/crystallized/documents/project-alpha/architecture \
  .agents/memory/crystallized/documents/project-alpha/service-architecture.md \
  --dry-run
```

`route remove` removes one eligible routed source or complete category, releases
its file ownership and records the removal choice. The root [`remove`](#remove-and-keep-removed)
command also handles ordinary files, directories, packages and Libraries.

```sh
open-forge route remove memory/crystallized/documents/project-alpha/service-architecture --dry-run
```

Both commands can be blocked by ambiguous routes, changed targets, unsafe
generated regions or incomplete evidence. `route move` still refuses managed
content. Read the plan and resolve the named boundary before applying it.

## Generated navigation and repair

### `index`

`index` rebuilds bounded generated `Entries` regions from routed sources. With
no operands it follows the Loader's selected topology. With operands it uses
the supplied source IDs or exact `.agents` paths.

```sh
open-forge index --dry-run
open-forge index memory/crystallized/documents --dry-run
open-forge index
```

The command changes only generated regions it can identify and verify. It does
not invent routes or treat a generated entry as an independent authority.

### `doctor`

`doctor` diagnoses workspace, route, reference, lifecycle, Library, Extension,
and recovery facts without changing them.

```sh
open-forge doctor
open-forge doctor --format json
open-forge doctor --detail debug
```

Use it when `status` or another command reports `blocked`, `incomplete`, or
`completed-with-warnings`. Diagnosis can show the next repair boundary, but it does not
apply a proposal.

### `repair`

`repair` applies the local-reference repairs you select and finishes accepted
recovery of leftover Workspace Library state. Preview it first:

```sh
open-forge repair --dry-run
open-forge repair --automatic --dry-run
```

`--automatic` selects every current safe-exact repair without prompting. An
explicit relink can select one source occurrence, expected destination, and
target path:

```text
open-forge repair --relink <source-location> <expected-destination> <target-path>
```

Repair does not rewrite an ambiguous reference or apply an unaccepted guess.
When the evidence is insufficient, it reports the candidate or blocked
boundary for review.

## Framework lifecycle

Lifecycle commands manage Framework content through one complete
plan. They keep user changes visible and require explicit boundaries for
replacement and deletion.

### Install

`install` establishes Framework management in the selected workspace.

```sh
open-forge install --dry-run
open-forge install --automatic --dry-run
```

`--force` permits eligible existing files to be replaced while management
is being established. It does not adopt arbitrary existing content or bypass a
conflict. `--automatic` removes prompting but adds no force or safety authority.

### Update

`update` reconciles managed Framework content with the current embedded payload.

```sh
open-forge update --dry-run
open-forge update --force --dry-run
open-forge update --force --prune --dry-run
```

Normal update replaces changed owned files and restores missing ones when the
current ownership facts authorize the effect. It reports every replaced,
restored, deleted, and retained path. Retired managed content is retained unless
`--prune` is supplied. `--force` and `--prune` remain separate named boundaries,
and `--automatic` implies neither one. When the plan needs a recovery bundle to
protect existing bytes, Update retains it after successful verification and
reports its exact path. When an ordinary `.git`
directory is present, it also advises `git diff`. Otherwise it points to the
bundle for previous content. Explicit Cleanup removes retained recovery data.

Ownership state is recorded in `.agents/open-forge.lock.json`. Authored settings
are in `.agents/open-forge.json`. Framework and Extension records share the lock
but remain independent command domains.

### Remove and keep removed

Use `remove` when content should stay removed through later Open Forge installs,
updates and Library synchronization. It accepts one workspace-relative path by
default. Use `--kind` for a route reference, package ID or Library ID. The workspace
root (`.` or `./`) cannot be removed.

```sh
open-forge remove .agents/templates --dry-run
open-forge remove .agents/guidance/old-note.md --automatic
open-forge remove docs/obsolete.txt --automatic
open-forge remove planning --kind extension --automatic
open-forge remove team-knowledge --kind library --automatic
open-forge remove guidance/old-note --kind route --automatic
```

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
  "schemaVersion": 1,
  "removedCategories": ["skills"],
  "removedFiles": [".agents/patterns/_patterns.md", "AGENTS.md"],
  "removedDirectories": ["docs/obsolete"],
  "removedExtensions": ["planning"],
  "removedLibraries": ["team-knowledge"]
}
```

To restore managed content, remove every applicable exclusion from that file and
run the relevant install, update, attach or sync command. Use `update` to restore
a removed Core category in an installed Framework. For individual Library
links whose registration remains, use `library sync <id>`. Use `library attach`
when the registration was removed. Recreate ordinary user files
from your own source or backup. Clearing an exclusion does not recover deleted
bytes. A required excluded dependency or missing excluded ancestor is reported
rather than silently restored.

Review `--dry-run` before applying a removal. Protected controls, Git metadata,
Library source trees and unsafe links remain protected even with `--automatic`.
The [Remove contract](../.agents/memory/crystallized/documents/cli/contracts/remove/_remove.md)
defines the complete selection and recovery boundary.

### Cleanup

`cleanup` removes only recognized Open Forge recovery bundles and drafts for
the selected workspace.

```sh
open-forge cleanup --dry-run
open-forge cleanup
```

It does not scan arbitrary workspace files, extract a bundle, restore a target,
or create a replacement bundle for its own support-artifact deletion. Use the
preview to review the exact recognized candidates before applying cleanup.

The lifecycle and recovery contracts contain the complete safety and residual
state rules:

- [Install contract](../.agents/memory/crystallized/documents/cli/contracts/install/_install.md)
- [Update contract](../.agents/memory/crystallized/documents/cli/contracts/update/_update.md)
- [Cleanup contract](../.agents/memory/crystallized/documents/cli/contracts/cleanup/_cleanup.md)

## Extension operations

Extensions are optional packages of routed Framework content and supporting
capabilities. The command group is:

```text
open-forge extension <operation>
```

| Operation                            | Purpose and useful form                                                                                                                                                                                                            |
| ------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `extension list`                     | List installed and available packages. Add `--installed`, `--available`, or `--source <package-or-catalogue-path>`.                                                                                                                |
| `extension inspect <stable-id>`      | Inspect one installed or available package. Add `--source <package-or-catalogue-path>` for an exact local source.                                                                                                                  |
| `extension create [<stable-id>]`     | Create a package scaffold. Use `--path <catalogue-path>`, `--name <text>`, `--description <text>`, `--package-version <text>`, repeat `--dependency <stable-id>` as needed, and add `--automatic` or `--dry-run` when appropriate. |
| `extension install [<stable-id>...]` | Install selected packages. Use `--source`, `--all`, `--force`, `--automatic`, or `--dry-run`.                                                                                                                                      |
| `extension update [<stable-id>...]`  | Reconcile selected managed packages. Use `--source`, `--all`, `--force`, `--prune`, `--automatic`, or `--dry-run`.                                                                                                                 |
| `extension remove [<stable-id>...]`  | Uninstall selected packages and record their removal. Use `--automatic` or `--dry-run`.                                                                                                                                            |

For example:

```sh
open-forge extension list --available
open-forge extension inspect development-toolkit
open-forge extension install development-toolkit --dry-run
open-forge extension update --all --dry-run
open-forge extension remove development-toolkit --dry-run
```

The `--source` value is one exact local package or catalogue path. An install
or update source is read-only and must be separate from the target workspace.
Dependencies resolve offline within that selected source. A manual installation
is also valid: copy reviewed package content into the workspace, rebuild the
affected `Entries`, and review the assembled diff. Manual copying does not
create managed lifecycle state.

See [Extension packages](extensions.md) for package structure and examples.

## Workspace libraries

Workspace Libraries let a workspace register a contained source root and
project it into another workspace location through relative file links.

```text
open-forge library <operation>
```

| Operation         | Syntax                                                                                       | Effect                                                                      |
| ----------------- | -------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------- |
| `library list`    | `open-forge library list`                                                                    | List bounded Library records and link observations.                         |
| `library inspect` | `open-forge library inspect <library-id>`                                                    | Inspect one complete Library inventory and projection.                      |
| `library attach`  | `open-forge library attach <library-id> <source-root> [--to <workspace-relative-directory>]` | Register one contained source root and create its relative-link projection. |
| `library sync`    | `open-forge library sync <library-id>`                                                       | Reconcile one registered projection from its complete source inventory.     |
| `library detach`  | `open-forge library detach <library-id>`                                                     | Remove one local registration and its links while preserving its source.    |

Add `--dry-run` to `attach`, `sync`, or `detach` to inspect the complete plan
before writing. `attach --to` selects the workspace-relative projection
directory and defaults to the workspace root. A Library ID is a management
identity. It is not a source ID.
Use `doctor` when a record, source root, link capability, permission, or
projection is blocked.

## Source references

Existing `.agents` sources accept one of these forms:

```text
<source-id>
.agents/<path>
./.agents/<path>
```

The first two examples address the same default route. The third shows quoting
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
