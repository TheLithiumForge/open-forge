---
open-forge:
  description: Reviewed published help snapshot for the remaining shared allow-list bindings and Update authority claims
  tags: [Memory, CLI, Task, Evidence, Contextual, Archived, Historical]
---

# P1 Published Permission Help Snapshot

Captured from the verified A6 native executable before any of these binding
or product-help sources changed. CLI SHA-256:
`e1a08d654b39811797e2ee81914ecf6469119f6780db07f6c6350a15febf32d5`. Only line endings are normalized. This is a reviewed
publication snapshot; the binding tests separately establish parser behavior.

> Historical G1 snapshot: its help text predates Task 30 G4. Current help uses
> `--format text|json`, `--detail minimal|standard|full|debug`, and repeatable
> `--detail-filter`; the older options below remain only as evidence of the
> captured executable.

## extension remove

```text
Description:
  Release selected managed Extension ownership and remove only eligible content.

Usage:
  OpenForge.Cli extension remove [<stable-id>...] [options]

Arguments:
  <stable-id>  Select one exact managed Extension stable ID.

Options:
  --automatic         Disable prompts; does not select packages or permit extra
                      deletion.
  --dry-run           Preview the complete removal without writing files.
  --workspace <path>  Select the workspace explicitly.
  --json              Write JSON; --view selects compact or expanded detail.
  --view <view>       Result detail: compact or expanded. [default: Expanded]
  --verbose           Write diagnostic details to stderr.
  --help              Show help and exit.
  --version           Show the executable version and exit.

Syntax:
  open-forge extension remove [<stable-id>...] [--automatic] [--dry-run] [global
  options]

Selection and dependencies:
  Select exact managed stable IDs, or choose from the interactive package list.
  A dependency cannot be removed while a retained package needs it. Unused
  dependencies remain registered.

Ownership and recovery:
  Shared paths remain owned by retained packages.
  When the last owner is removed, eligible existing files are deleted, including
  edited files.
  A recovery bundle preserves their bytes before deletion and remains after
  success.
  Review the bundle before running cleanup; retained recovery blocks later
  mutations.

Results and streams:
  Dry-run writes nothing.
  Human and JSON output preserve the shared semantic status, stream, recovery,
  and next-action mapping.
```

## extension update

```text
Description:
  Update managed Extension packages from one reviewed source.

Usage:
  OpenForge.Cli extension update [<stable-id>...] [options]

Arguments:
  <stable-id>  Exact managed stable IDs selected from the reviewed source.

Options:
  --source <package-or-catalogue-path>  Read one exact local package or
                                        catalogue source.
  --all                                 Select every managed package
                                        represented by the reviewed source.
  --force                               Replace changed or restore missing
                                        managed content when eligible.
  --prune                               Delete eligible retired managed content.
  --automatic                           Disable prompts; does not select
                                        packages or imply --force or --prune.
  --dry-run                             Preview the complete update without
                                        writing files.
  --workspace <path>                    Select the workspace explicitly.
  --json                                Write JSON; --view selects compact or
                                        expanded detail.
  --view <view>                         Result detail: compact or expanded.
                                        [default: Expanded]
  --verbose                             Write diagnostic details to stderr.
  --help                                Show help and exit.
  --version                             Show the executable version and exit.

Syntax:
  open-forge extension update [<stable-id>...] [--source
  <package-or-catalogue-path>] [--all] [--force] [--prune] [--automatic]
  [--dry-run] [global options]

Selection and dependencies:
  Select exact managed stable IDs or --all from one reviewed source.
  Dependencies, including dependencies of dependencies, are resolved first.

Authority:
  Normal mode preserves changed, missing, retired, shared, and unknown content.
  --force replaces or restores current expected content; --prune deletes
  eligible retired content.

Results and streams:
  Dry-run writes nothing. Human and JSON output preserve the shared semantic
  status, stream, and exit mapping.
```

## library detach

```text
Description:
  Remove a Library registration and its links; preserve source files.

Usage:
  OpenForge.Cli library detach [<library-id>] [options]

Arguments:
  <library-id>  Select one exact Library management ID.

Options:
  --dry-run           Preview the complete plan without writing.
  --workspace <path>  Select the workspace explicitly.
  --json              Write JSON; --view selects compact or expanded detail.
  --view <view>       Result detail: compact or expanded. [default: Expanded]
  --verbose           Write diagnostic details to stderr.
  --help              Show help and exit.
  --version           Show the executable version and exit.

Syntax:
  open-forge library detach <library-id> [--dry-run] [global options]

Write policy:
  Remove the registered destination links while preserving source files. Use
  --dry-run to preview all changes without writing.

Examples:
  open-forge library detach shared --dry-run
  open-forge library detach shared
```

## library sync

```text
Description:
  Synchronize a registered Library with all its source files.

Usage:
  OpenForge.Cli library sync [<library-id>] [options]

Arguments:
  <library-id>  Select one exact Library management ID.

Options:
  --dry-run           Preview the complete plan without writing.
  --workspace <path>  Select the workspace explicitly.
  --json              Write JSON; --view selects compact or expanded detail.
  --view <view>       Result detail: compact or expanded. [default: Expanded]
  --verbose           Write diagnostic details to stderr.
  --help              Show help and exit.
  --version           Show the executable version and exit.

Syntax:
  open-forge library sync <library-id> [--dry-run] [global options]

Write policy:
  Reconcile a registered Library with its complete source inventory. Use
  --dry-run to preview all changes without writing.

Examples:
  open-forge library sync shared --dry-run
  open-forge library sync shared
```

## update

```text
Description:
  Update managed Framework files.

Usage:
  OpenForge.Cli update [options]

Options:
  --force             Replace changed or restore missing managed content.
  --prune             Delete eligible retired managed content.
  --automatic         Use safe defaults without prompting; does not imply
                      --force or --prune.
  --dry-run           Preview the complete update without writing.
  --workspace <path>  Select the workspace explicitly.
  --json              Write JSON; --view selects compact or expanded detail.
  --view <view>       Result detail: compact or expanded. [default: Expanded]
  --verbose           Write diagnostic details to stderr.
  --help              Show help and exit.
  --version           Show the executable version and exit.

Syntax:
  open-forge update [--force] [--prune] [--automatic] [--dry-run] [global
  options]

Reconciliation:
  Update compares the managed Framework installation with the complete version
  bundled in this CLI. An installation already at that version needs no writes.

Authority:
  --force may replace changed or restore missing current managed content.
  --prune may delete eligible retired managed content. The flags are
  independent; --automatic enables neither. Safe source changes and new targets
  do not require force.

Execution:
  --automatic suppresses confirmation without adding force or prune. --dry-run
  previews the same complete checked plan and writes nothing. An interactive
  text request asks once after checks if it would write files. JSON and
  redirected execution never prompt.

Global options:
  --workspace <path>, --json, --view <compact|expanded>, --verbose, --help, and
  --version apply to this command. --view selects detail in text and JSON.

Examples:
  open-forge update
  open-forge update --automatic --dry-run --json
  open-forge update --force
  open-forge update --force --prune --automatic

Related commands:
  open-forge doctor — inspect blocked or unavailable lifecycle and safety facts.
  open-forge cleanup — remove a reported retained recovery artifact after
  review.

Results and streams:
  Human complete, attention, and incomplete results use stdout; invalid,
  blocked, failed, and interrupted results use stderr.
  Expanded JSON writes one schema-version-1 envelope to stdout for every
  semantic status. Verbose diagnostics use bounded stderr.
  complete: exit 0 and human stdout.
  failed: exit 1 and human stderr.
  attention: exit 2 and human stdout.
  incomplete: exit 3 and human stdout.
  invalid: exit 4 and human stderr.
  blocked: exit 5 and human stderr.
  interrupted: exit 130 and human stderr.

Notes:
  Update uses only trusted local lifecycle state and the Framework payload
  embedded in the running CLI. It does not fetch content, adopt unmanaged files,
  manipulate Git, restore targets automatically, or remove recovery artifacts
  owned by Cleanup.
```
