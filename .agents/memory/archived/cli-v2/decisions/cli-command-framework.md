---
open-forge:
  description: "Historical CLI-v2 source: The accepted replacement command boundary uses Commander with inferred TypeScript definitions and contract-preserving shell completion"
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Command Framework

## Context

The replacement needs typed command definitions, direct handler testing, modular TypeScript source, and predictive completion for Bash, Zsh, Fish, and PowerShell. It must run under Node.js, Bun, and Deno without adopting a large application framework or preserving the MVP command surface.

## Decision

Use `commander` as the replacement CLI command framework and import its strongly inferred API from the matching `@commander-js/extra-typings` release. Keep the Commander and extra-typings major and minor versions aligned as required by the typings package.

Use `@bomb.sh/tab` as the completion engine through a focused Open Forge adapter. The adapter maps Commander metadata into the accepted `completion install`, `completion remove`, and `completion script` family and may register one hidden `complete` callback. It must not add another visible generator command. The hidden callback is internal shell protocol, not a public domain leaf.

The hidden protocol provides static Commander metadata and bounded dynamic
completion for local Route, Template, persisted Framework exclusion, and Extension identities. It delegates
general paths to the shell, performs no mutation or executable discovery, and
treats suggestions as convenience rather than authority. The [Completion
protocol contract](../../documents/cli/contracts/completion-protocol.md) defines
the exact boundary.

Commander owns command routing, option and argument parsing, command-input validation, help metadata, and action invocation. It does not own application operations, filesystem access, domain policy, result rendering, transactions, or process-completion policy.

Register each canonical flag and optional two-dash alias as one Commander option declaration, such as `--j, --json` or `--ws, --workspace <directory>`. Do not register the spellings twice or add a repository-owned alias parser.

The composition root disables Commander's implicit public help command, configures the accepted `--h, --help` and `--v, --version` spellings, retains global options before or after subcommand words, intercepts parser output and exits, and invokes `parseAsync()`. It does not enable positional global options, automatic prefix abbreviation, or another parser mode that would let one flag name acquire command-local meaning.

Each same-named leaf module keeps its readonly metadata, independently callable named handler, and `register<Command>()` function together. Registration builds the Commander definition and action beside the behavior it invokes. Direct handler tests do not spawn a process. Group registration composes child registration functions; global options remain defined and registered once at the root.

Lazy command loading is not a requirement at the expected command count. Preserve ordinary module boundaries. If measurement later establishes a startup problem, a handler may dynamically import its implementation without changing the public command contract.

## Rationale

Commander directly supports separate prepared commands, inherited global options, global options before or after subcommand words, two-long-option aliases such as `--ws, --workspace`, async actions, generated help, and complete output and exit interception. The companion typings infer option and action values from chained definitions without a second command schema. This matches the accepted public interface and the requested definition, handler, and registration locality more directly than Clerc.

Commander officially targets Node.js. The distributed ESM artifact must still execute under Node.js, Bun, and Deno; that compatibility remains an implementation and release-test requirement rather than a reason to fork application behavior by runtime.

Selecting focused packages keeps the command layer small and prevents optional presentation or update behavior from silently becoming product policy. Deferring lazy loading avoids optimizing a CLI that is not expected to have enough commands for eager registration to be a demonstrated problem.

## Consequences

- Cross-file inference, root-to-group global-option typing, bare-root and bare-group help behavior, packaged execution, completion behavior, output streams, exit codes, and cancellation remain implementation verification targets.
- Command definitions validate their own options and positional arguments; handlers do not apply a redundant schema pass.
- Every leaf exposes a visible registration function beside its metadata and handler.
- Dynamic workspace-aware completion is limited to the accepted finite local
  identity providers and must remain deterministic, bounded, read-only, and
  safe to invoke frequently.
- The hidden completion callback remains outside public help, domain operation
  identifiers, and the nineteen public leaf count.
- Runtime portability remains exercised against supported versions rather than inferred from Node.js compatibility.
- The selected library does not decide the replacement command vocabulary or broader CLI architecture.

## Authoritative Sources

- [Open Forge CLI Architecture](../../documents/cli/architecture.md)
- [CLI command slice pattern](../../../../patterns/open-forge/cli/commands/command-slice.md)

## Evidence And Context

- [Archived CLI command-framework comparison](../../../archived/analysis/2026-07-30_cli-command-frameworks.md)
- [Archived command-framework reassessment](../../../archived/analysis/2026-07-31_cli-command-framework-reassessment.md)

## Decision Relationships

- [CLI runtime boundary validation](cli-runtime-boundary-validation.md)
- [CLI direct replacement development](cli-direct-replacement-development.md)
- [CLI testing architecture](cli-testing-architecture.md)
