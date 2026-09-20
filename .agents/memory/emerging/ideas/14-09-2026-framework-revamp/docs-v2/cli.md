# The Open Forge CLI

The CLI helps you read context, inspect routes, keep navigation current, and manage Framework files. Everything it does can be done by hand. It makes the repetitive parts faster and easier to review.

The Framework stays complete in plain files. The files define what Open Forge means. Commands automate the work around them.

Start here whenever you need the exact options for the executable you have:

```sh
open-forge --help
```

For building the CLI and using a worktree-local executable, see [Development](development.md). For package structure and manual Extension use, see [Extensions](extensions.md).

## Five commands to know

```sh
open-forge context                       # what an agent loads at startup
open-forge route list --depth=all        # every route in the workspace
open-forge find --tag=Decision           # records by tag
open-forge index --dry-run               # preview a navigation rebuild
open-forge status                        # one-screen summary of the workspace
```

Read the sections below when you need the details.

## Command shape

```text
open-forge <command> [command options] [global options]
```

`open-forge --help` lists commands. `open-forge <command> --help` shows arguments, options, and examples. Help wraps to the terminal width. Redirected help uses 80 columns, and long source references stay intact.

The CLI works against the current directory unless `--workspace` selects one explicit directory. A source reference is either an automatic source ID, such as `memory/crystallized/documents`, or an exact path under `.agents`, such as `.agents/memory/crystallized/documents/_documents.md`. [Source references](#source-references) has the exact grammar.

Reading and changing are kept apart. `status`, `context`, `find`, `references`, `route list`, and `route inspect` only read. Commands that can change files take `--dry-run` when a complete preview is useful. A dry run forms and reports the plan without touching files. An operation still needs enough evidence to know which files it may manage.

### Global options

These keep the same spelling and meaning on every command that accepts them.

| Option                     | Meaning                                                                                                                                              |
| -------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------- |
| `--workspace <path>`       | Use one exact directory instead of the current directory. Relative paths resolve from the process current directory. Parent directories are not searched. |
| `--json`                   | Write one complete structured result to standard output. The operation and its status are the same as in text output.                                |
| `--view=compact\|expanded` | Choose text or JSON detail. `expanded` is the default. `compact` keeps identities, order, status, and next action for scanning.                        |
| `--verbose`                | Add bounded diagnostic detail on standard error. It changes neither the operation nor its status.                                                    |
| `--help`                   | Show help for the selected command path and exit.                                                                                                    |
| `--version`                | Show the executable version and exit.                                                                                                                |

`--help` and `--version` are terminal modes. They do not resolve a workspace or run an operation, and they cannot be combined. Other well-formed global options may accompany them. Command operands and command-specific options are invalid in a terminal invocation.

`--json` writes the full expanded document by default. Add `--view=compact` for a minified document that keeps core facts and selected content and omits supporting evidence. Compact JSON identifies itself with `schemaVersion: 2` and `view: "compact"`. Expanded JSON uses `schemaVersion: 1`. Mutation commands keep complete plans, effects, and recovery details in both views.

Where output goes: `--json` always writes to standard output. In text mode, `complete`, `attention`, and `incomplete` results use standard output. `failed`, `invalid`, `blocked`, and `interrupted` results use standard error. Pipelines stay usable without hiding the status.

Text results use colour automatically on supported Unix terminals: green for success, yellow for attention, red for errors, cyan for labels. Written status labels always remain visible. Redirected output, Windows, a missing or `dumb` `TERM`, and a nonempty `NO_COLOR` variable produce plain text. JSON, selected file content, and preview diffs stay plain. There is no colour option.

### Status and exit codes

Every operation reports one status and the matching exit code:

| Status        | Exit code | Meaning                                                                          |
| ------------- | --------: | -------------------------------------------------------------------------------- |
| `complete`    |       `0` | The requested operation, or a verified no-op, completed.                         |
| `failed`      |       `1` | The operation began or could not finish and reported a failure.                  |
| `attention`   |       `2` | The result is usable, but a reported condition needs review.                     |
| `incomplete`  |       `3` | Some required facts were unavailable, so the result is not complete.             |
| `invalid`     |       `4` | The input does not follow the command's grammar or metadata rules.               |
| `blocked`     |       `5` | A workspace, ownership, safety, or authority boundary prevents the operation.    |
| `interrupted` |     `130` | The operation ended before completion, usually through input or cancellation.    |

A status says more than a Boolean. A route inspection can complete while reporting a structural observation. A mutation that cannot establish trusted lifecycle facts is blocked. Follow the suggested next action in the result, then rerun from a fresh look at the workspace.

## Read the workspace

### `status`

A bounded summary of workspace structure, startup context, generated navigation, Framework and Extension lifecycle state, Libraries, and recovery observations.

```sh
open-forge status
open-forge status --view=compact
open-forge status --json
```

It is a report. It does not repair stale navigation, adopt changed files, or make a lifecycle record trusted. Use `doctor` when the summary names something that needs diagnosis.

### `context`

Ordered startup context, or the selected sources added to it. With no arguments it returns the startup files. Add source IDs or exact paths to include what you select.

```sh
open-forge context
open-forge context memory/crystallized/documents
open-forge context \
  templates \
  --content=frontmatter,headings \
  --additions-only
```

Useful options:

- `--additions-only` omits files already required at startup.
- `--content=part[,part...]` returns selected parts: `metadata`, `frontmatter`, `headings`, `body`, or a named `section:<heading>`.
- `--follow-links=positive-depth|all` follows contained local Markdown links to the requested depth or through the complete reachable set.

The returned file identities show what was included. The [loader](../src/open-forge/.agents/loader.md) defines loading and scope. Command output does not change those rules.

How the loading tags resolve: `LoadNow` and `KeepInMind` both load through exposed entries of already-loaded parents. Selecting an on-demand scope activates its applicable child loading rules. Tagged files inside other inactive scopes stay excluded. `KeepInMind` adds refresh instructions while the scope remains active. The CLI resolves each invocation independently and does not track an agent session.

### `find`

Select Markdown sources by authored tags and structural headings. Body text is not a substitute for route or metadata meaning.

```sh
open-forge find --tag=Memory --tag=CurrentTruth --require=all
open-forge find --heading=Axioms --within=body
open-forge find --include=memory/crystallized/documents --content=metadata,headings
```

Selectors:

- `--include <source-reference>` adds a set of sources to search.
- `--exclude <source-reference>` removes a set from the search.
- `--tag <tag>` matches one authored tag. Repeat it for more predicates.
- `--heading <heading>` matches one complete structural heading.
- `--require=all|any` chooses whether all or any predicates must match.
- `--within=part[,part...]` limits evaluation to authored regions.
- `--content=part[,part...]` chooses which parts of matched sources to return.

Use an exact path when an ID is ambiguous. A blocked or incomplete result is not permission to guess which source was meant.

### `references`

Direct authored Markdown references, incoming and outgoing, for one source. The default direction is `both`.

```sh
open-forge references memory/crystallized/documents
open-forge references memory/crystallized/documents --direction=out
open-forge references memory/crystallized/documents --direction=in \
  --include=memory/crystallized/documents/framework
```

`--direction=in|out|both` selects the report. `--include` and `--exclude` limit an incoming scan. They do not apply to an outgoing-only request.

### `route list` and `route inspect`

An `entrypoint` makes a folder routable. A `route` is the navigable path its entrypoint and generated `Entries` expose. Route commands accept the source-reference grammar unless their target has a narrower shape.

`route list` shows routed sources and descendants at a structural depth. The default depth is `1`. Use `all` for the complete routed descendant set.

```sh
open-forge route list
open-forge route list memory/crystallized/documents --depth=all
```

`route inspect` explains one source's route behavior without returning its body.

```sh
open-forge route inspect memory/crystallized/documents
open-forge route inspect .agents/memory/crystallized/documents/_documents.md
```

## Change routes and Markdown

Route commands keep authored content and generated navigation apart. They plan the complete bounded effect, recheck the expected state before writing, and verify the result. Use `--dry-run` before any operation whose target or metadata deserves a look.

### Frontmatter and metadata

An indexed Open Forge Markdown source uses this frontmatter shape:

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

`description` helps a reader decide whether to open a source. An optional `responsibility` helps an editor decide what belongs in it by stating what the file defines. It creates no authority or loading behavior. Tags are bare values in frontmatter, without `#`. A tag starts with a letter, then letters or digits, with single internal hyphens allowed. It may not end with a hyphen or contain two adjacent hyphens.

The command flags use the same concepts:

- `--description <text>` sets one nonblank description.
- Repeated `--tag=<tag>` values give an ordered tag list. Create and update require unique canonical tags when the list is supplied.
- `--responsibility <text>` sets the optional responsibility. On update, an exact empty value removes the key.

The CLI never infers these values from a filename, parent, Template, body, or generated entry.

### `route init`

Initialize each missing entrypoint in one exact route chain. A target is a route ID or an exact canonical `.agents` entrypoint path. The generic scaffold accepts optional metadata:

```sh
open-forge route init memory/crystallized/documents/project-alpha \
  --description="Project Alpha documents" \
  --responsibility="Define the documents route" \
  --tag=Document \
  --tag=ProjectAlpha \
  --dry-run
```

`--framework` selects the trusted embedded Framework scaffold. It cannot be combined with `--description`, `--responsibility`, or `--tag`, because those are a different scaffold. `route init` does not initialize the Loader itself.

The examples that follow use this scope. Apply each previewed creation before running a command that depends on the new path.

### `route create`

Create one ordinary routed Markdown file below an existing routable parent. It requires a nonblank description and at least one unique canonical tag. A responsibility is optional. `--template` copies the body of one existing routed Template as starting content. The new source gets its own metadata and does not retain Template ownership.

```sh
open-forge route create memory/crystallized/documents/project-alpha/architecture \
  --description="Current service boundaries and request flow" \
  --tag=Document \
  --tag=Architecture \
  --responsibility="Define the current service structure and dependencies" \
  --dry-run
```

The target is an ordinary Markdown file below an existing route. It is not a directory, entrypoint, overwrite companion, Loader, or general external path. The command never overwrites an existing target. Use `route update` for an existing source.

### `route update`

Apply one or more explicit metadata changes, or copy one Template body when the target is eligible.

```sh
open-forge route update memory/crystallized/documents/project-alpha/architecture \
  --description="Current service structure and dependency boundaries" \
  --tag=Document \
  --tag=Architecture \
  --dry-run
```

The options are patches, never inferred replacements:

- `--description` replaces the description.
- Repeated `--tag=<tag>` values replace the complete ordered tag list.
- `--responsibility <text>` sets the responsibility. `--responsibility ""` removes it.
- `--template <template-reference>` completes an eligible frontmatter-only body. If the target already has body content, the CLI keeps it and reports `attention` instead of overwriting.

At least one metadata or Template operation is required. The Template contributes body content only. Its frontmatter and lifecycle do not transfer.

### `route move` and `route remove`

`route move` moves one routed source or category to an exact destination path. Check the destination's scope and inherited rules first.

```sh
open-forge route move \
  memory/crystallized/documents/project-alpha/architecture \
  .agents/memory/crystallized/documents/project-alpha/service-architecture.md \
  --dry-run
```

`route remove` removes one eligible unmanaged routed source or complete category. It does not release managed ownership.

```sh
open-forge route remove memory/crystallized/documents/project-alpha/service-architecture --dry-run
```

Either command can be blocked by ambiguous routes, managed ownership, changed targets, unsafe generated regions, or incomplete evidence. Read the plan and resolve the named boundary before applying.

### `index`

Rebuild bounded generated `Entries` regions from routed sources. With no operands it follows the Loader's selected topology. With operands it uses the supplied source IDs or exact `.agents` paths.

```sh
open-forge index --dry-run
open-forge index memory/crystallized/documents --dry-run
open-forge index
```

It changes only generated regions it can identify and verify. It does not invent routes or treat a generated entry as an independent authority.

### `doctor`

Diagnose workspace, route, reference, lifecycle, Library, Extension, and recovery facts without changing them.

```sh
open-forge doctor
open-forge doctor --json
open-forge doctor --verbose
```

Use it when `status` or another command reports `blocked`, `incomplete`, or `attention`. Diagnosis can point at the next repair boundary. It does not apply a proposal.

### `repair`

Apply selected bounded local-reference repairs and accepted Workspace Library residual recovery. Preview first:

```sh
open-forge repair --dry-run
open-forge repair --automatic --dry-run
```

`--automatic` selects every current safe-exact repair without prompting. An explicit relink selects one source occurrence, expected destination, and target path:

```text
open-forge repair --relink <source-location> <expected-destination> <target-path>
```

Repair does not rewrite an ambiguous reference or apply an unaccepted guess. When evidence is insufficient it reports the candidate or blocked boundary for review.

## Framework lifecycle

Lifecycle commands manage Framework content through one complete plan. They keep your changes visible and require explicit boundaries for replacement and deletion.

### `install`

Establish Framework management in the selected workspace.

```sh
open-forge install --dry-run
open-forge install --automatic --dry-run
```

`--force` permits eligible existing files to be replaced while management is established. It does not adopt arbitrary existing content or bypass a conflict. `--automatic` removes prompting and adds no force or safety authority.

### `update`

Reconcile managed Framework content with the current embedded payload.

```sh
open-forge update --dry-run
open-forge update --force --dry-run
open-forge update --force --prune --dry-run
```

A normal update applies safe new or unchanged content and preserves changed, missing, and retired divergence for review. `--force` replaces or restores changed or missing current content when eligible. `--prune` deletes eligible retired managed content. These are separate named boundaries. `--automatic` implies neither.

The lifecycle record is `.agents/open-forge.lifecycle.json`. Its Framework and Extension sections are independent.

### `cleanup`

Remove only recognized Open Forge recovery bundles and drafts for the selected workspace.

```sh
open-forge cleanup --dry-run
open-forge cleanup
```

It does not scan arbitrary workspace files, extract a bundle, restore a target, or create a replacement bundle for its own support-artifact deletion. Preview to see the exact recognized candidates.

The complete safety and residual-state rules are in the contracts:

- [Install contract](../.agents/memory/crystallized/documents/cli/contracts/install/_install.md)
- [Update contract](../.agents/memory/crystallized/documents/cli/contracts/update/_update.md)
- [Cleanup contract](../.agents/memory/crystallized/documents/cli/contracts/cleanup/_cleanup.md)

## Extensions

Extensions are optional packages of routed Framework content and supporting capabilities.

```text
open-forge extension <operation>
```

| Operation                            | Purpose and useful form                                                                                                                                                                                                          |
| ------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `extension list`                     | List installed and available packages. Add `--installed`, `--available`, or `--source <package-or-catalogue-path>`.                                                                                                             |
| `extension inspect <stable-id>`      | Inspect one installed or available package. Add `--source <package-or-catalogue-path>` for an exact local source.                                                                                                               |
| `extension create [<stable-id>]`     | Create a package scaffold. Use `--path <catalogue-path>`, `--name <text>`, `--description <text>`, `--package-version <text>`, repeat `--dependency <stable-id>` as needed, and add `--automatic` or `--dry-run` when useful.     |
| `extension install [<stable-id>...]` | Install selected packages. Use `--source`, `--all`, `--force`, `--automatic`, or `--dry-run`.                                                                                                                                   |
| `extension update [<stable-id>...]`  | Reconcile selected managed packages. Use `--source`, `--all`, `--force`, `--prune`, `--automatic`, or `--dry-run`.                                                                                                              |
| `extension remove [<stable-id>...]`  | Release selected package ownership and remove only eligible content. Use `--prune`, `--automatic`, or `--dry-run`.                                                                                                              |

For example:

```sh
open-forge extension list --available
open-forge extension inspect development-toolkit
open-forge extension install development-toolkit --dry-run
open-forge extension update --all --dry-run
open-forge extension remove development-toolkit --dry-run
```

`--source` is one exact local package or catalogue path. An install or update source is read-only and must be separate from the target workspace. Dependencies resolve offline within that source. Manual installation is also valid: copy reviewed package content into the workspace, rebuild the affected `Entries`, and review the diff. Manual copying does not create managed lifecycle state.

[Extensions](extensions.md) covers package structure and examples.

## Workspace Libraries

A Library registers a contained source root and projects it into another workspace location through relative file links.

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

Add `--dry-run` to `attach`, `sync`, or `detach` to see the complete plan before writing. `attach --to` selects the workspace-relative projection directory and defaults to the workspace root. A Library ID is a management identity, not a source ID. Use `doctor` when a record, source root, link capability, permission, or projection is blocked.

## Source references

An existing `.agents` source is addressed in one of three forms:

```text
<source-id>
.agents/<path>
./.agents/<path>
```

The first two address the same default route. The third shows quoting for a custom scope whose folder name contains a space. Create that scope before selecting it.

```sh
open-forge context memory/crystallized/documents
open-forge context .agents/memory/crystallized/documents/_documents.md
open-forge context "memory/crystallized/documents/project alpha"
```

An automatic source ID derives from the canonical path below `.agents`:

- `/` separates segments on every platform.
- `.md` is removed from Markdown file names.
- A recognized entrypoint uses its containing folder ID rather than `_{folder}.md`.
- `SKILL.md` uses its containing Skill folder ID.
- Exact case, spaces, and Unicode are preserved.

The `.agents/` prefix means an exact workspace path. Any other value is an ID, even when it looks like a relative filesystem path. Use the exact `.agents/...` form when an ID has more than one candidate. There is no fuzzy, case-correcting, or likely-intent matching.

An adjacent `{name}.overwrite.md` shares its base source's ID and route. The base is read first and the overwrite second. The overwrite is not an independent source. An orphan overwrite is broken evidence, not a standalone source.

Quote one complete operand when an ID or path contains spaces or special characters:

```sh
open-forge route inspect "memory/project alpha/documents"
```

The shell removes the quotes before the CLI sees the value. Exact paths resolve inside the selected workspace and cannot escape it through lexical or physical aliases.

## Where to go next

[Development](development.md) covers local prerequisites and worktree-local setup. The command contracts hold the exact public boundary when you need more:

- [Global flags](../.agents/memory/crystallized/documents/cli/contracts/shared/global-flags/interface.md)
- [Source references](../.agents/memory/crystallized/documents/cli/contracts/shared/source-references/interface.md)
- [Context](../.agents/memory/crystallized/documents/cli/contracts/context/_context.md)
- [Find](../.agents/memory/crystallized/documents/cli/contracts/find/_find.md)
- [Route commands](../.agents/memory/crystallized/documents/cli/contracts/route/_route.md)
- [Index](../.agents/memory/crystallized/documents/cli/contracts/index-candidate/_index-candidate.md)
- [Repair](../.agents/memory/crystallized/documents/cli/contracts/repair/_repair.md)
- [Extension commands](../.agents/memory/crystallized/documents/cli/contracts/extension/_extension.md)
- [Workspace Libraries](../.agents/memory/crystallized/documents/cli/contracts/library/_library.md)

The Framework stays usable with no executable present. Markdown is the durable interface. The CLI makes inspection and maintenance faster, repeatable, and easier to review.
