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
