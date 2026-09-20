---
open-forge:
  description: Reviewed published help before replacing generated Entries guard boundaries
  tags: [Memory, Working, CLI, Task, Evidence, Contextual]
---

# B1 Published Help Before Heading Migration

Captured before B1 production changes from the built M1 managed CLI.
Line endings, trailing spaces from help wrapping, and blank lines around Markdown fences normalize.

> Historical help capture: this executable predates Task 30 G4. The current
> CLI uses one schema-3 report with `--format`, `--detail`, and
> `--detail-filter`; the options and status names below remain evidence of the
> captured pre-G4 executable.

## index --help

```text
Description:
  Rebuild generated Entries for selected routes.

Usage:
  OpenForge.Cli index [<source-reference>...] [options]

Arguments:
  <source-reference>  Select zero or more source IDs or exact .agents/... paths.

Options:
  --dry-run           Preview all generated Entries changes without writing
                      files.
  --workspace <path>  Select the workspace explicitly.
  --json              Write JSON; --view selects compact or expanded detail.
  --view <view>       Result detail: compact or expanded. [default: Expanded]
  --verbose           Write diagnostic details to stderr.
  --help              Show help and exit.
  --version           Show the executable version and exit.

Syntax:
  open-forge index [source-reference...] [--dry-run] [global options]

Selection:
  With no source operands, Index rebuilds the Loader and every reachable
  entrypoint region. An entrypoint selects its subtree and direct exposing
  parent; a routed leaf selects its direct exposing parent.

Write policy:
  Omit --dry-run to apply bounded generated-interior changes. --dry-run previews
  the same complete checked plan and writes nothing. Repeating the command makes
  no further changes.

Global options:
  --workspace <path>, --json, --view <compact|expanded>, --verbose, --help, and
  --version apply to this command. --view selects detail in text and JSON.

Examples:
  open-forge index
  open-forge index memory --dry-run
  open-forge index .agents/memory/_memory.md --json

Related commands:
  open-forge doctor — inspect blocked topology, metadata, or generated-region
  facts.
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
  Index changes only valid bounded generated Entries interiors. It does not
  repair markers, format complete files, modify overwrites, search for another
  workspace, or create Git commits.
```

## extension inspect --help

```text
Description:
  Inspect one installed or available Extension package.

Usage:
  OpenForge.Cli extension inspect [<stable-id>] [options]

Arguments:
  <stable-id>  One exact lowercase Extension stable ID.

Options:
  --source <package-or-catalogue-path>  Read one exact local package or
                                        catalogue source.
  --workspace <path>                    Select the workspace explicitly.
  --json                                Write JSON; --view selects compact or
                                        expanded detail.
  --view <view>                         Result detail: compact or expanded.
                                        [default: Expanded]
  --verbose                             Write diagnostic details to stderr.
  --help                                Show help and exit.
  --version                             Show the executable version and exit.

Syntax:
  open-forge extension inspect <stable-id> [--source
  <package-or-catalogue-path>] [global options]

Subject and source:
  Supply one exact lowercase stable ID. Available package details come from the
  bundled catalogue, or only from the separate local package or catalogue named
  by --source. There is no fallback, network, registry, cache, or approximate ID
  matching.

Inspection:
  Inspect reports ownership, available package, dependency, path, current-byte,
  generated-boundary, and current-versus-intended comparison facts. It never
  writes, mutates, executes package content, or invokes another command.

Global options:
  --workspace <path>, --json, --view <compact|expanded>, --verbose, --help, and
  --version apply to this command. --view selects detail in text and JSON.

Examples:
  open-forge extension inspect development-toolkit
  open-forge extension inspect development-toolkit --source ./packages/toolkit
  --json

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

Fingerprint boundary:
  open-forge-markdown-v1 uses strict UTF-8, LF-only normalization, exact final
  Entries marker exclusion, and exact-byte fallback. Current and intended hashes
  describe this comparison; no stored baseline is read.
```
