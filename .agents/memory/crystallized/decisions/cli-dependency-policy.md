---
open-forge:
  description: Why replacement CLI dependency roles are narrow, centrally pinned, and constrained by Native AOT and trimming
  tags: [Memory, Decision, CurrentTruth, CLI, Dependency, DotNet, NativeAOT, Security]
---

# CLI Dependency Policy

## Context

The replacement CLI needs a command parser, YAML support, Markdown body parsing
when an accepted consumer exists, JSON serialization from the runtime, and one
test platform. Duplicating exact package versions in prose made executable
configuration and Architecture two manually synchronized version authorities.

## Decision

The [CLI Architecture](../documents/cli/architecture.md) approves dependency
roles and constrains their use. The repository-root
[`Directory.Packages.props`](../../../../Directory.Packages.props) is the sole
source for exact current package versions.

The accepted direct roles are:

- `System.CommandLine` for tokenization, typed parsing, standard help, and
  command dispatch input;
- YamlDotNet plus its accepted static generator for Framework metadata;
- Markdig only after an accepted command needs Markdown body parsing; and
- xUnit v3 on Microsoft Testing Platform for active test projects.

JSON uses `System.Text.Json` source generation from the runtime. A dependency
does not gain broader semantic authority from its role. Command behavior,
filesystem safety, source identity, recovery, output, and compatibility remain
in Open Forge contracts and designs.

Every direct or transitive dependency must remain compatible with trimming and
Native AOT and justify its maintenance, security, package-audit, binary-size,
and source-generation cost. Reflection fallback, dynamic resolver discovery,
floating versions, command-local package additions, and duplicate version pins
are not accepted.

### Optional Git inventory

On 2026-09-30 the maintainer accepted an optional Git executable and a bounded,
read-only external process for `route inspect --matching-files`. No NuGet
package is added. The [Route Inspect contracts](../documents/cli/contracts/route/inspect/_inspect.md)
define the option's current result and failure behavior.

The host uses shell-free BCL process invocation to probe whether the selected
workspace is in a work tree, for example:

```text
git -C <workspace> rev-parse --is-inside-work-tree
```

After a confirmed probe, it enumerates with:

```text
git -C <workspace> ls-files --cached --others --exclude-standard -z -- .
```

Git interprets per-directory `.gitignore`, repository exclusions, and global
exclusions. NUL-delimited output supplies unquoted paths. Tracked files remain
listed even when an ignore pattern matches them. Keep the selected workspace,
including one inside a larger repository, and support ordinary repositories
and linked worktrees without substituting the repository root.

Both calls disable optional locking and filesystem-monitor integration. They
make no Git configuration changes, automatic trust overrides, fetches, or
installation attempts. A nonzero exit, warning-bearing inventory, or malformed
inventory after a confirmed probe selects the fallback. Keep raw Git diagnostics
out of normal output. Cancel and clean up the child process, and bound the
probe, inventory, and fallback walk with one 30-second deadline. Never restart
that deadline.

The fallback is a bounded translator exception. Operations translates the ten
documented `.gitignore` rules onto the existing Framework matcher. No `gitignore`
package is added, and no parser beyond those rules is allowed. The translator
stores each rule's base directory separately, evaluates paths relative to that
base with the entry's directory flag, and prunes directories only after the
final inclusion decision. The Framework matcher itself remains unchanged.

When Git cannot start, the workspace is not a work tree, there is a repository
refusal, or inventory fails after confirmation, discard any Git output and use
an ignore-aware BCL walk. Read regular `.gitignore` files when entering eligible
directories, before examining children. Never enter an excluded directory to
discover more rules. Read nothing above the workspace, use no global excludes or
`.git/info/exclude`, and give tracked files no exemption. Skip every entry named
`.git`, whether file or folder. Both strategies exclude symbolic links, directory
links, other reparse points, Git administrative paths, and submodule contents.
Filter deleted tracked paths and directories. Use existing physical-containment
checks. Read candidate metadata and required `.gitignore` contents, not other
file contents. An unreadable or undecodable required `.gitignore` returns
`files-unavailable` and discards partial results. Unsupported lines are skipped
and counted according to the ten rules.

Timeout or cancellation keeps the existing unavailable or cancelled result and
does not start the fallback. Unexpected implementation failures remain
`scan-failed`.

The host owns process launch, the walk, and cancellation. It returns a
command-owned enumeration result to Inspect. Operations owns eligibility,
matching, count, the 100-path cap, completeness, and status. It reuses the
Framework applicability evaluator and projects a command-owned matching-files
result for presentation. Framework gains no command or process dependency.

This choice keeps the command usable without Git while preserving ignore-aware
enumeration. Git remains the primary inventory when it works. The fallback
applies the documented ten-rule translation through the existing matcher and
makes its scope and skipped-line count visible. It does not use global excludes
or `.git/info/exclude`, does not exempt tracked files, and keeps case-sensitive
matching. The maintainer accepted the bounded translation, link and submodule
exclusions, single deadline, and display cap, replacing the proposed
unavailable-only fallback.

## Rationale

Central package management makes the executable configuration the one exact
version source while keeping durable architecture focused on roles and
constraints. Narrow roles reduce binary and security surface and keep library
behavior from becoming an accidental product contract. Source generation keeps
serialization and metadata paths visible under trimming and Native AOT.

## Changes And Evidence

A package addition, removal, role change, or version update requires an explicit
dependency decision before a command Task relies on it. The change must reverify
documented callable behavior, locked restore and audit, warning-free managed
build and affected tests, trimming, Native AOT publication and execution, binary
and package consequences, and every compatibility boundary affected by the
dependency.

An ordinary command Task may not add or update a package. Exact version changes
edit the central package source and do not add a matching version literal to
Architecture or Directives.

## Alternatives And Tradeoffs

- Duplicating version literals in Architecture makes current values easy to see
  but creates drift and requires two coordinated edits.
- Floating or command-local versions reduce the apparent ceremony of an update
  but make restore identity and evidence inconsistent.
- Replacing accepted libraries with local parsers or reflective frameworks would
  expand maintenance and Native AOT risk without changing the product need.

## Related Current Sources

- [Replacement CLI Architecture](../documents/cli/architecture.md)
- [CLI Implementation Directive](../../../directives/open-forge/cli/implementation.md)
- [Repository-Root CLI Tooling Decision](repository-root-cli-tooling.md)
