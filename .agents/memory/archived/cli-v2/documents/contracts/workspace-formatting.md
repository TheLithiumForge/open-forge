---
open-forge:
  description: "Historical CLI-v2 source: Workspace-aligned formatter discovery, trust classification, exact-file post-processing, checksum refresh, and manual follow-up"
  responsibility: Define how Open Forge formats affected files without installing tools, guessing among competing formatters, formatting unrelated workspace content, or trusting a tool name more than its resolved execution behavior
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# Workspace Formatting Contract

## Scope

Open Forge does not preserve distributed whitespace against the workspace's
established formatter. Every complete file affected by an Open Forge operation
may be formatted under the same workspace rules, including `.agents` content
and the workspace-owned `AGENTS.md`.

Formatting is an explicit post-processing stage after the primary mutation has
completed. It is not an effect inside the immutable domain plan, ownership
evidence, or a reason to roll back already verified primary work. The request
still resolves the strategy, exact affected paths, and required execution
authority before mutation so post-processing never expands its scope or invents
authority afterward.

## Guarantees

### Strategy Selection

Resolve one whole-file strategy for each affected file in this order:

1. Explicit matching formatter configuration in `.agents/open-forge.json`.
2. Explicit workspace or editor formatter configuration applicable to the file.
3. The closest applicable supported formatter configuration.
4. Exactly one supported already-installed formatter that can handle the file.
5. Guided choice or manual post-operation guidance.

Configuration evidence wins over Open Forge preference. If several strategies
remain equally applicable, the CLI shows them and asks. It never chooses one
through an undocumented ranking or chains multiple whole-file formatters.

Open Forge aims to support up to three mainstream formatters for each popular
language or file family. Ecosystems with one canonical formatter keep one;
weak integrations are not added to satisfy a number. Coverage evolves through
a formatter registry without changing the operation pipeline.

### Initial Built-In Coverage

The initial target is deliberately broader than one popularity ranking. It
covers the current high-usage GitHub families, the Markdown and configuration
formats Open Forge writes, and the explicitly accepted established ecosystems:
Rust, Kotlin, Dart, Ruby, Swift, Zig, Elixir, Scala, Lua, R, PowerShell, and SQL.
“Up to three” is a ceiling, not a quota. A tool is included only when it has a
maintained, documentable exact-file invocation and materially represents
workspace practice.

| Language or file family              | Accepted built-in strategies                                                                                                                                                        |
| ------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Markdown                             | [Prettier](https://prettier.io/docs/), [dprint](https://dprint.dev/), [mdformat](https://github.com/hukkin/mdformat)                                                                |
| JSON and JSONC                       | Prettier, [Biome](https://biomejs.dev/internals/language-support/), dprint                                                                                                          |
| YAML                                 | Prettier, dprint, [yamlfmt](https://github.com/google/yamlfmt)                                                                                                                      |
| TOML                                 | [Taplo](https://taplo.tamasfe.dev/cli/usage/formatting.html), dprint                                                                                                                |
| JavaScript, TypeScript, JSX, and TSX | Prettier, Biome, dprint                                                                                                                                                             |
| HTML, CSS, and GraphQL               | Prettier, Biome, dprint where the installed strategy explicitly supports the selected file                                                                                          |
| Python                               | [Ruff](https://docs.astral.sh/ruff/formatter/), [Black](https://black.readthedocs.io/en/stable/usage_and_configuration/the_basics.html), [YAPF](https://github.com/google/yapf)     |
| Java                                 | [google-java-format](https://github.com/google/google-java-format), [Palantir Java Format](https://github.com/palantir/palantir-java-format)                                        |
| C, C++, and Objective-C              | [clang-format](https://clang.llvm.org/docs/ClangFormat.html), [Artistic Style](https://astyle.sourceforge.net/), [Uncrustify](https://github.com/uncrustify/uncrustify)             |
| C#                                   | [CSharpier](https://csharpier.com/docs/CLI), [`dotnet format`](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format)                                                   |
| PHP                                  | [PHP-CS-Fixer](https://cs.symfony.com/doc/usage.html), [PHP_CodeSniffer `phpcbf`](https://github.com/PHPCSStandards/PHP_CodeSniffer), [Laravel Pint](https://laravel.com/docs/pint) |
| Shell                                | [shfmt](https://github.com/mvdan/sh)                                                                                                                                                |
| Go                                   | [gofmt](https://pkg.go.dev/cmd/gofmt), [goimports](https://pkg.go.dev/golang.org/x/tools/cmd/goimports)                                                                             |
| Rust                                 | [rustfmt](https://github.com/rust-lang/rustfmt)                                                                                                                                     |
| Kotlin                               | [ktfmt](https://github.com/Kotlin/ktfmt), [ktlint](https://pinterest.github.io/ktlint/latest/install/cli/)                                                                          |
| Dart                                 | [`dart format`](https://dart.dev/tools/dart-format)                                                                                                                                 |
| Ruby                                 | [Syntax Tree](https://github.com/ruby-syntax-tree/syntax_tree), [RuboCop layout formatting](https://docs.rubocop.org/rubocop/latest/usage/autocorrect.html)                         |
| Swift                                | [`swift-format`](https://github.com/swiftlang/swift-format), [SwiftFormat](https://github.com/nicklockwood/SwiftFormat)                                                             |
| Zig                                  | [`zig fmt`](https://ziglang.org/documentation/master/)                                                                                                                              |
| Elixir                               | [`mix format`](https://hexdocs.pm/mix/Mix.Tasks.Format.html)                                                                                                                        |
| Scala                                | [Scalafmt](https://scalameta.org/scalafmt/docs/installation.html)                                                                                                                   |
| Lua and Luau                         | [StyLua](https://github.com/JohnnyMorganz/StyLua)                                                                                                                                   |
| R                                    | [Air](https://github.com/posit-dev/air), [styler](https://styler.r-lib.org/)                                                                                                        |
| PowerShell                           | [PSScriptAnalyzer `Invoke-Formatter`](https://learn.microsoft.com/en-us/powershell/module/psscriptanalyzer/invoke-formatter)                                                        |
| SQL                                  | [SQLFluff `format`](https://docs.sqlfluff.com/en/stable/reference/cli.html#sqlfluff-format), with an explicit or unambiguous dialect                                                |
| Terraform and OpenTofu HCL           | [`terraform fmt`](https://developer.hashicorp.com/terraform/cli/commands/fmt), [`tofu fmt`](https://opentofu.org/docs/cli/commands/fmt/)                                            |
| Packer HCL                           | [`packer fmt`](https://developer.hashicorp.com/packer/docs/commands/fmt)                                                                                                            |

The matrix is an accepted adapter target, not permission to claim an adapter
before it exists. Runtime discovery exposes only implemented strategies whose
executables and applicable configuration are actually available. Everything
else uses guided selection or manual post-operation guidance.

The first implementation order is driven by direct Open Forge use, broad
coverage, and bounded formatter behavior: Prettier, Biome, dprint, mdformat,
Ruff, Black, Taplo, clang-format, CSharpier, google-java-format, PHP-CS-Fixer,
`phpcbf`, Pint, shfmt, gofmt, rustfmt, `dart format`, `zig fmt`, `swift-format`,
StyLua, Air, Terraform, and OpenTofu. The remaining accepted alternatives
follow through the same strategy contract; adding them never changes mutation
stages or public command grammar.

#### Strategy-Specific Trust

The matrix does not pre-classify a tool as safe. These known boundaries guide
resolved invocation inspection:

- Prettier data files may be `SAFE`; JavaScript or TypeScript configuration,
  shareable configuration, and plugins are `RUN`.
- A dprint invocation with already-available sandboxed Wasm plugins and
  data-only configuration may be `SAFE`. Process plugins are `RUN`; a missing
  or remote plugin that would be fetched is `BLOCK`.
- mdformat plugins, executable PHP configuration, and custom PHP_CodeSniffer
  standards or sniffs are `RUN`.
- Palantir Java Format is eligible as a standalone already-installed formatter.
  Gradle, Maven, or Spotless integration loads project build code and is `RUN`
  only when exact-file scope and offline execution can be proven; otherwise it
  is `BLOCK`.
- `dotnet format` is `RUN` because it loads a project and may run analyzers. It
  must use an explicit project or folder boundary and `--no-restore`; any
  restore, download, or broader write set is `BLOCK`.
- `goimports` is `RUN` because it changes imports using project context rather
  than performing whitespace-only formatting. Its invocation must enforce
  offline dependency resolution; otherwise it is `BLOCK`.
- rustfmt invoked with a path may also format referenced out-of-line modules.
  Open Forge therefore uses stdin and prepared output for one exact logical
  file; `cargo fmt` and any invocation that broadens to a crate are `BLOCK`.
- Standalone ktfmt may be `SAFE`. ktlint custom rulesets and Kotlin formatter
  integrations through Gradle, Maven, or project build logic are `RUN` only
  when exact-file and offline execution remain enforceable; otherwise they are
  `BLOCK`.
- Syntax Tree plugins and RuboCop plugins, required Ruby files, or executable
  workspace configuration are `RUN`. RuboCop is limited to layout-only
  correction; broad or unsafe autocorrection is not a formatting strategy.
- `mix format` is `RUN` because `.formatter.exs` is evaluated and configured
  plugins may compile project or dependency code. Missing dependencies,
  compilation downloads, or an unbounded configured input set are `BLOCK`.
- Scalafmt is `BLOCK` when its configured version would be resolved or
  downloaded. An already-installed matching version with data-only
  configuration and an exact file may be `SAFE`.
- Air, StyLua, `dart format`, `zig fmt`, `swift-format`, SwiftFormat, and
  PSScriptAnalyzer `Invoke-Formatter` may be `SAFE` only with an already
  available tool, data-only settings, and an exact file or in-memory input.
  Any plugin, executable configuration, or project wrapper moves the resolved
  invocation to `RUN` or `BLOCK` under the general rules.
- styler runs through R and may load project or package behavior, so it is
  `RUN` unless a future adapter proves a narrower invocation. SQLFluff with a
  raw templater, explicit dialect, data-only configuration, and exact file may
  be `SAFE`; Python/Jinja/dbt templaters, libraries, or plugins are `RUN`, and
  any download or unbounded expansion is `BLOCK`.
- Data-only exact-file invocations such as Ruff, Black, YAPF,
  google-java-format, clang-format, CSharpier with `--no-msbuild-check`, shfmt,
  gofmt, `terraform fmt`, `tofu fmt`, and `packer fmt` may be `SAFE` only after
  executable, configuration, plugin, network, and exact-path inspection passes.

The coverage boundary is reviewed deliberately rather than inferred at
runtime. Languages outside this explicit set remain supported through an
explicit configured command and manual post-operation guidance; they do not justify a
built-in adapter without evidence. Popularity evidence is one input, not a
mechanical cutoff.

### Strategy Shape

Each built-in strategy declares one focused inspectable unit:

- Named formatter identifier.
- Supported languages and file extensions.
- Configuration evidence.
- Existing executable resolution.
- Configuration and plugin trust inspection.
- Network, restore, project-loading, and exact-file capability evidence.
- Exact-file argument construction.
- Success, warning, failure, and unsupported result interpretation.

Languages, extensions, formatter identifiers, and result states use named
const objects or enums in production source. Direct strategy references select
behavior; serialized strings do not resolve functions through a registry.

The reusable implementation shape is defined by the
[workspace formatter strategy Pattern](../../../../../patterns/open-forge/cli/workspace/workspace-formatter-strategy.md).

### Explicit Workspace Commands

Users may provide formatter commands in `.agents/open-forge.json`:

```json
{
  "formatters": [
    {
      "extensions": [".md", ".json"],
      "command": ["prettier", "--write"]
    },
    {
      "extensions": [".py"],
      "command": ["ruff", "format"]
    }
  ]
}
```

The command is an argument array, never a shell string. Open Forge appends the
exact affected paths. It performs no shell interpolation, piping, redirection,
glob expansion, placeholder substitution, or arbitrary package-script
inference. The executable must already exist; Open Forge never downloads or
installs a formatter and never invokes `npx` or an equivalent network-capable
runner on the user's behalf.

Configured extension sets must not overlap within the same selection scope. A
file with several matching explicit commands is invalid configuration rather
than an implicit execution order.

### Trust Boundary

Classify the exact resolved invocation rather than trusting a formatter name:

| Class   | Required evidence                                                                                                                                                                                 | Behavior                                                                                                |
| ------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------- |
| `SAFE`  | Existing executable, data-only configuration, exact affected-file boundary, and no plugin, project-code, analyzer, restore, installation, download, or network execution                          | Ordinary post-format confirmation; documented defaults may be accepted by `--yes`                       |
| `RUN`   | Exact affected-file boundary remains enforceable, but execution loads configured code such as a JavaScript configuration, plugin, process plugin, analyzer, or project-defined formatter behavior | Show the complete trust evidence before mutation and require an explicit interactive `RUN` decision     |
| `BLOCK` | The tool would install, download, restore, contact the network, broaden beyond exact affected files, or execute behavior Open Forge cannot bound                                                  | Complete the primary operation without running it and return manual guidance, or cancel before mutation |

These names become production-owned const values rather than repeated control
strings. Classification is part of complete preflight and cannot change during
application.

A custom command is executable code obtained from a Git-tracked workspace and
is at least `RUN`. The decision shows:

- Executable and arguments.
- Exact affected files or file count with an inspection path.
- Selected workspace.
- How to decline, format manually, skip, or cancel.

Generic `--yes` never authorizes `RUN`. Initially, executable configuration,
plugins, analyzers, project code, and custom commands therefore require
interactive execution approval. A future canonical-only non-interactive
authorization flag may be added only when a real agent journey requires it and
its trust meaning is accepted.

`RUN` is not permission to fetch code. Network access, package installation,
implicit restore, and missing-plugin download remain `BLOCK` even after user
confirmation. A strategy may become eligible only when its complete required
toolchain is already available and the exact invocation can enforce offline,
bounded behavior.

### Guided States

Detected formatter:

```text
Formatter detected: Prettier
Affected files: 12
Trust: RUN
Reason: prettier.config.mjs executes workspace code

Decision
RUN FORMATTER
FORMAT MANUALLY
SKIP FORMATTING
CANCEL
```

Manual follow-up:

```text
Open Forge created or updated 12 files.
Formatting was not run.
Review and format these exact files when ready:
  <affected files or inspection command>
```

Open Forge never pauses an active transaction while a user or editor changes
files. Choosing manual formatting or declining a formatter completes the
primary operation and returns `attention` with exact paths and safe command
guidance. Later manual formatting is ordinary user work and may make advisory
checksums report changed bytes.

Structured or non-interactive execution that cannot select or authorize a
strategy completes eligible primary work and returns a typed attention result
with the exact trust reason, manual command, or next action. A caller may still
cancel before mutation. The CLI never waits for terminal input after primary
application, silently executes configured code, or weakens `BLOCK` into `RUN`.

### Execution Ordering

Formatting follows the completed primary plan:

```text
compose complete intended content
  -> resolve one strategy per affected file
  -> collect custom-command authority when required
  -> plan, preflight, apply, and verify primary effects
  -> write primary advisory checksums and lifecycle state
  -> run the selected formatter on exact affected files
  -> rescan and validate exact formatted files
  -> on success, atomically refresh their advisory checksums
  -> return post-processing evidence
```

A trusted automatic strategy runs only after primary application succeeds. A
formatter failure never reverses completed primary work. It returns
`attention`, leaves the existing advisory checksums unchanged so divergent bytes
remain visible, and reports the exact affected files and failure classification.

After successful formatting and validation, a small separate finalization
recomputes checksums from actual bytes and atomically rewrites only the affected
lifecycle entries. A failure or hard stop before that refresh is safe: the
existing record still identifies every managed path and reports changed bytes.
`.agents/open-forge.json` does not checksum itself and is emitted as
deterministic canonical JSON rather than being sent through an in-place
workspace formatter.

`--dry-run` does not execute formatting or pretend to know formatter output. It
reports the selected strategy, trust class, and exact affected paths as
post-processing evidence beside the exact primary mutation plan.

### Result Evidence

Every applicable mutating result contains one focused formatting projection
with a named outcome:

| Outcome          | Meaning                                                             |
| ---------------- | ------------------------------------------------------------------- |
| `not-applicable` | No affected file has a selected formatter strategy                  |
| `planned`        | Preview reports the strategy and paths without execution            |
| `skipped`        | Formatting was deliberately declined before mutation                |
| `manual`         | Primary work completed and exact manual follow-up is required       |
| `succeeded`      | Automatic formatting and validation completed                       |
| `failed`         | Automatic formatting or checksum refresh did not complete correctly |

The projection reports the formatter identifier when selected, trust class,
canonical logical paths, validation outcome, and whether advisory checksums are
`unchanged`, `refreshed`, or `stale`. It never exposes a shell string, physical
path, environment, or raw process output. These outcomes and checksum states
are production-owned readonly named values rather than repeated strings.
`manual` or `failed` formatting yields overall `attention` when the primary
mutation completed correctly; it does not relabel completed domain work as
failed.

## Boundaries

Formatting applies only to the exact complete files affected by the selected
operation and only through one resolved whole-file strategy per file. It never
installs or restores tools, invokes a shell, gains network access, expands to
unrelated workspace content, lets `--yes` authorize executable configuration,
or rolls back completed primary work after a post-processing failure.

## Verification

- Every affected Open Forge file may follow workspace formatting rules.
- Explicit configuration wins; ambiguity is shown rather than guessed.
- At most one whole-file formatter applies to one file.
- Only already-available tools execute.
- Exact paths are appended as arguments without a shell.
- Every invocation is classified as `SAFE`, `RUN`, or `BLOCK` from its resolved
  configuration and execution behavior, not its tool name.
- Executable configuration, plugins, analyzers, project code, and custom
  commands require explicit `RUN` authority.
- `--yes` cannot authorize `RUN`.
- Download, installation, restore, network access, or an unbounded file set is
  always `BLOCK`.
- Manual formatting remains a complete supported follow-up without pausing an
  active transaction.
- Formatter failure never rolls back completed primary work.
- Successful automatic formatting is validated before advisory checksums are
  refreshed from actual bytes.
- Formatting never expands to unrelated workspace files.

## Related Current Sources

- [CLI interface](../interface.md)
- [Request construction](request-construction.md)
- [Mutation execution](mutation-execution.md)
- [Managed lifecycle](managed-lifecycle.md)
- [Source review](source-review.md)
