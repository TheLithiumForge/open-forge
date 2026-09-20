---
open-forge:
  description: Define shared authored destination admission and truthful permission result coordinates
  tags: [Memory, Crystallized, CLI, Contract, Shared, Permission, Interface, CurrentTruth]
---

# Workspace Permissions Interface

Extension Install/Update/Remove and Library Attach/Sync/Detach consume the one
shared `allowInstallPaths` in `.agents/open-forge.json`. Library recovery checks
current admission without prompting or publishing grants. The accepted
[Workspace State Files decision](../../../../../decisions/framework/workspace-state-files.md)
owns this boundary.

## Authored Settings

There is no per-owner permission document. Grants are workspace-relative paths
in the authored settings file, shared by every Extension and Library. They do
not bind a package ID or Library source root. Changing a source does not rebind,
transfer or replace a grant. Ownership and source selection remain independent
command facts and keep their own safety checks.

Entries are paths, not glob patterns. Exact paths and descendants of a named
directory are admitted through the shared portable identity. `.agents/` is
implicitly admitted. Reserved controls, `.git`, protected hosts, Library source
boundaries, ownership and physical containment remain binding even with a grant.
The retired permission file is ordinary unrelated content: no legacy reader,
migration, automatic deletion or grant authority survives.

Use the existing authored settings grammar: strict UTF-8 JSON with comments and
trailing commas allowed, unknown keys accepted, and a non-binding schema version.
Missing or unreadable settings supply no external grants. An explicit grant
write preserves unrelated authored keys and their order; comments need not
survive. Invalid or unsafe authored settings are never overwritten by approval.

## Explicit Grant Input

All six gated commands expose repeatable `--allow-path <path>`, with one value
per occurrence and the parser's normal supported option-value forms. It authors
a persistent shared grant through the settings writer. It writes nothing under
`--dry-run`. Automatic or redirected execution does not prompt; a missing grant
names `--allow-path` and the authored settings file as the available remedy.

An eligible interactive apply can choose allow always, allow once, or cancel.
Always publishes the displayed paths to settings. Once grants only the current
operation and writes no settings. Cancel authorizes no content effects. Neither
choice adds ownership or bypasses a destination, source or physical boundary.

## Common Result Coordinates

Each consuming command retains its command-local `permissions` object and the
existing shared envelope, statuses and exits. Its members are:

| Member     | Meaning                                                                                                |
| ---------- | ------------------------------------------------------------------------------------------------------ |
| `path`     | `.agents/open-forge.json`.                                                                             |
| `required` | Ordinally ordered distinct destination path strings for this operation.                                |
| `missing`  | The ordered subset not admitted when reviewed, retained after approval.                                |
| `decision` | `not-evaluated`, `not-required`, `granted`, `required`, `approved`, or `declined`.                     |
| `action`   | `none`, `create`, or `replace` for the planned interactive settings publication.                       |
| `outcome`  | `not-requested`, `planned`, `not-started`, `verified`, `verification-failed`, or `completion-unknown`. |

`granted` means the observed shared settings admit every required path.
`approved` means this invocation received explicit interactive approval. Once
has action `none` and outcome `not-requested`; it never claims a persisted grant.
Only a verified receipt supports saying an interactive grant was saved. The
separate explicit `--allow-path` authored edit precedes permission observation;
its granted paths are observed as settings on this and later runs.

`not-required` means the operation needs no external grant. A boundary before
permission evaluation uses `not-evaluated`, action `none` and outcome
`not-requested`. Empty arrays stay present and never imply an unevaluated missing
set is complete. Required and missing paths contain no grant-subject ID or source.

## Library Scope Coordinates

Library results additionally retain `proposedScopes` and `approvedScopes` as
ordered `{kind, path}` objects, with kind `file` or `directory`. A directory
proposal describes admission of descendants; concrete required/missing leaves
remain separate. Order scopes by path and kind. There is no `rebinding` object,
ID or source-root coordinate in a permission scope. Source identity remains
available in the Library command's ordinary identity/source fields.

Approved scopes describe what was approved; action/outcome state whether they
were persisted. Once is confined to the current checked operation. Content
effects do not duplicate a settings publication effect. Recovery protected paths
include settings when an interactive grant has a planned create/replace.

See [Behavior](behavior.md) for ordering and
[Technical Design](../../../technical-designs/workspace-permissions.md) for owners.
