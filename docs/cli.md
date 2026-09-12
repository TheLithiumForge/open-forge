# Open Forge CLI

The Open Forge CLI helps you find context, inspect routes, maintain navigation,
and manage Framework files. It makes repeated operations easier to run and review.

The Framework remains complete in plain files. You can read, edit, and use its
rules without the CLI. Commands automate the work around those files; the
files still define Framework meaning.

Run this first when you need the exact options for the executable you have:

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
for arguments, options, and examples. Help wraps to the terminal width;
redirected help uses 80 columns. Long source references stay intact.

The CLI works against the current directory unless `--workspace` selects one
explicit directory. A source reference is either an automatic source ID, such
as `memory/crystallized/documents`, or an exact path under `.agents`, such as
`.agents/memory/crystallized/documents/_documents.md`. The exact grammar is
covered in [Source references](#source-references).

The CLI separates observation from change. Commands such as `status`,
`context`, `find`, `references`, `route list`, and `route inspect` read the
workspace. Commands that can change files expose `--dry-run` when a complete
preview is useful. A dry run forms and reports the operation's plan without
changing files. An operation still needs sufficient evidence to determine
which files it may manage.

## Global options

These options keep the same spelling and meaning on every command that accepts
them.

| Option                     | Meaning                                                                                                                                                          |
| -------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `--workspace <path>`       | Use one exact directory instead of the current directory. Relative paths resolve from the process current directory. The CLI does not search parent directories. |
| `--json`                   | Write one complete structured result to standard output. The operation and its status are the same as in text output.                                            |
| `--view=compact\|expanded` | Choose text or JSON detail. `expanded` is the default; `compact` keeps the identities, order, status, and next action needed for scanning.                       |
| `--verbose`                | Add bounded diagnostic detail. Diagnostics go to standard error and do not change the operation or its status.                                                   |
| `--help`                   | Show help for the selected command path and exit.                                                                                                                |
| `--version`                | Show the executable version and exit.                                                                                                                            |

`--help` and `--version` are terminal modes. They do not resolve a workspace
or run a domain operation, and they cannot be used together. Other well-formed
global options may accompany them, but command operands and command-specific
options remain invalid in a terminal invocation.

`--json` uses the full expanded document by default. Add `--view=compact` for
a minified document that retains core facts and selected content while omitting
specified supporting evidence. Compact JSON identifies itself with
`schemaVersion: 2` and `view: "compact"`; expanded JSON uses `schemaVersion: 1`.
Mutation commands retain complete plans, effects and recovery details in both
JSON views.

`--json` always writes its result to standard output. In text mode, `complete`,
`attention`, and `incomplete` results use standard output. `failed`, `invalid`,
`blocked`, and `interrupted` results use standard error. This keeps command
output usable in a pipeline without hiding the operation's status.

### Status and exit codes

Every operation reports one status and the corresponding process exit
code:

| Status        | Exit code | Meaning                                                                                |
| ------------- | --------: | -------------------------------------------------------------------------------------- |
| `complete`    |       `0` | The requested operation or verified no-op completed.                                   |
| `failed`      |       `1` | The operation began or could not finish and reported a failure.                        |
| `attention`   |       `2` | The requested result is usable, but a reported condition needs review.                 |
| `incomplete`  |       `3` | Some required facts were unavailable, so the result is not complete.                   |
| `invalid`     |       `4` | The command input does not follow its grammar or metadata rules.                       |
| `blocked`     |       `5` | A workspace, ownership, safety, or authority boundary prevents the operation.          |
| `interrupted` |     `130` | The operation ended before completion, usually because input or cancellation ended it. |

The status is more useful than a Boolean success value. For example, a route
inspection can complete while reporting a structural observation, whereas a
mutation that cannot establish trusted lifecycle facts is blocked. Use the
suggested next action in the result, then rerun the command from a fresh
workspace observation.

## Read the workspace

### `status`

`status` gives a bounded summary of workspace structure, startup context,
generated navigation, Framework and Extension lifecycle state, Libraries, and
recovery observations.

```sh
open-forge status
open-forge status --view=compact
open-forge status --json
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
[loader](../src/open-forge/.agents/loader.md) defines Framework loading and scope;
command output does not change those rules.

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
limit an incoming scan; they do not apply to an outgoing-only request.

### Routes

An `entrypoint` makes a folder routable. A `route` is the navigable path that
the entrypoint and its generated `Entries` expose. Route commands accept the
source-reference grammar described below unless their target has a narrower
shape.

`route list` shows routed sources and descendants at a structural depth. The
default depth is `1`; use `all` to include the complete routed descendant set.

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
values in frontmatter, without `#`; they must be valid and useful for
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
body of one existing routed Template as starting content; the new source gets
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
The command never overwrites an existing target; use `route update` for an
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
- `--responsibility <text>` sets the responsibility; `--responsibility ""`
  removes it.
- `--template <template-reference>` completes an eligible frontmatter-only
  body. If the target already has authored body content, the CLI preserves it
  and reports `attention` rather than overwriting it.

At least one metadata or Template operation is required. The selected Template
contributes body content only; its frontmatter and continuing lifecycle do not
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

`route remove` removes one eligible unmanaged routed source or complete
category. It does not release managed ownership.

```sh
open-forge route remove memory/crystallized/documents/project-alpha/service-architecture --dry-run
```

Both commands can be blocked by ambiguous routes, managed ownership, changed
targets, unsafe generated regions, or incomplete evidence. Read the plan and
resolve the named boundary before applying the operation.

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
open-forge doctor --json
open-forge doctor --verbose
```

Use it when `status` or another command reports `blocked`, `incomplete`, or
`attention`. Diagnosis can show the next repair boundary, but it does not
apply a proposal.

### `repair`

`repair` applies selected bounded local-reference repairs and accepted Workspace
Library residual recovery. Preview it first:

```sh
open-forge repair --dry-run
open-forge repair --automatic --dry-run
```

`--automatic` selects every current safe-exact repair without prompting. An
explicit relink can select one source occurrence, expected destination, and
target path:

`LoadNow` and `KeepInMind` both load through exposed entries of already-loaded
parents. Selecting an on-demand scope activates its applicable child loading
rules; tagged files inside other inactive scopes stay excluded. `KeepInMind`
adds refresh instructions while the scope remains active. The CLI resolves each
invocation independently and does not track an agent session.

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

Normal update applies safe new or unchanged content and preserves changed,
missing, and retired divergence for review. `--force` replaces or restores
changed or missing current content when eligible. `--prune` deletes eligible
retired managed content. These are separate named boundaries; `--automatic`
does not imply either one.

The lifecycle record is `.agents/open-forge.lifecycle.json`. The Framework and
Extension sections are independent.

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

## Extension Operations

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
| `extension remove [<stable-id>...]`  | Release selected package ownership and remove only eligible content. Use `--prune`, `--automatic`, or `--dry-run`.                                                                                                                 |

For example:

```sh
open-forge extension list --available
open-forge extension inspect development-toolkit
open-forge extension install development-toolkit --dry-run
open-forge extension update --all --dry-run
open-forge extension remove development-toolkit --dry-run
```

The `--source` value is one exact local package or catalogue path. An install
or update source is read-only and must be separate from the target workspace;
dependencies resolve offline within that selected source. A manual installation
is also valid: copy reviewed package content into the workspace, rebuild the
affected `Entries`, and review the assembled diff. Manual copying does not
create managed lifecycle state.

See [Extension packages](extensions.md) for package structure and examples.

## Workspace Libraries

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
| `library detach`  | `open-forge library detach <library-id>`                                                     | Remove one exact projection while preserving its source.                    |

Add `--dry-run` to `attach`, `sync`, or `detach` to inspect the complete plan
before writing. `attach --to` selects the workspace-relative projection
directory and defaults to the workspace root. A Library ID is a management
identity; it is not a source ID.
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
for a custom scope whose folder name contains a space; create that scope before
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
The base is read first and the overwrite second; the overwrite is not an
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

The Framework remains usable when no executable is present. Plain Markdown is
the durable interface; the CLI makes common inspection and maintenance tasks
faster, repeatable, and easier to review.
