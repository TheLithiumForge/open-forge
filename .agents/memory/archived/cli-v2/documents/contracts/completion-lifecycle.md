---
open-forge:
  description: "Historical CLI-v2 source: Explicit per-user shell-completion targets, selection, profile ownership, safe mutation, interruption recovery, and Framework-install handoff"
  responsibility: Define how Completion install and removal resolve and mutate external shell state without making completion an automatic Framework side effect
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Completion Lifecycle Contract

## Scope

Completion is an explicit user-level lifecycle independent from Framework and
workspace lifecycle. `completion script` generates one shell script without
writing. `completion install` and `completion remove` are the only operations
that may change durable completion state.

Framework installation never changes shell configuration as a hidden or
automatic side effect. Its interactive success journey reminds the user about
the separate `completion install` command. Structured and non-interactive
Framework installation performs no Completion operation.

This contract owns shell selection, default targets, custom profile selection,
owned integration state, mutation ordering, interruption behavior, and safe
removal. The [CLI interface](../interface.md) owns public spelling. The
[Completion protocol contract](completion-protocol.md) owns static and bounded
dynamic suggestion behavior. The
[mutation execution contract](mutation-execution.md) owns shared plan,
preflight, application, verification, and result evidence.

## Guarantees

### Supported Shells

The initial shell identifiers are:

- `bash`
- `zsh`
- `fish`
- `powershell`

PowerShell 7 and Windows PowerShell 5.1 are separate installation targets under
the one `powershell` script grammar. When both are present, guided selection
shows both exact profiles. Explicit `--profile` selects one exact profile.

### Selection

Install and remove accept zero or more space-separated shell identifiers.

```text
open-forge completion install
open-forge completion install bash zsh
open-forge completion install --all

open-forge completion remove
open-forge completion remove bash zsh
open-forge completion remove --all
```

The accepted selection rules are:

- Omitted shells in an interactive invocation open a multi-select wizard.
- Install preselects every safely detected supported shell target.
- Remove preselects every safely discovered Open Forge-owned installation.
- Explicit shell identifiers bypass only subject selection.
- `--all` selects every safely discoverable target for the selected operation
  without bypassing plan review, divergent-content decisions, or safety.
- A structured or otherwise non-interactive invocation requires explicit shell
  identifiers or `--all`.
- Detection recommends and scopes visible choices. It never writes or silently
  authorizes a target.

Only installed shells with an exact resolvable target participate in the
install default. Open Forge does not create configuration for an unavailable
shell merely because the CLI supports its script grammar.

### Default Targets

Open Forge prefers a dedicated shell-native per-user completion target when one
exists. Other shells receive one Open Forge-owned generated asset plus one
minimal marker-owned activation block.

The generated user-data root is:

| Platform  | Root                                                          |
| --------- | ------------------------------------------------------------- |
| Unix-like | `${XDG_DATA_HOME:-$HOME/.local/share}/open-forge/completions` |
| Windows   | `%LOCALAPPDATA%\open-forge\completions`                       |

The initial strategies are:

| Shell      | Generated asset                                                      | Activation profile                                   |
| ---------- | -------------------------------------------------------------------- | ---------------------------------------------------- |
| Fish       | `${XDG_CONFIG_HOME:-$HOME/.config}/fish/completions/open-forge.fish` | None                                                 |
| Bash       | `<user-data-root>/bash/open-forge.bash`                              | `$HOME/.bashrc`                                      |
| Zsh        | `<user-data-root>/zsh/open-forge.zsh`                                | `${ZDOTDIR:-$HOME}/.zshrc`                           |
| PowerShell | `<user-data-root>/powershell/open-forge.ps1`                         | The selected edition's `CurrentUserAllHosts` profile |

The resolver obtains PowerShell profile paths from the installed edition
without loading user profiles. It does not hardcode a Documents directory that
could be redirected. If more than one edition is available, guided selection
shows both. Non-interactive selection uses `--all` or one explicit
`--profile`.

An absent exact default profile becomes a visible `CREATE` effect after the
shell target is selected. Parent directories required by that exact profile
may also be created. Open Forge never scans for, edits, or creates additional
startup files to compensate for a nonstandard shell configuration.

### Custom Profiles

`--profile <absolute-path>` means one thing: use this exact shell startup
profile for activation.

It is valid only with one explicit `bash`, `zsh`, or `powershell` shell. It is
invalid with omitted shells, multiple shells, `--all`, or `fish`. Fish uses its
dedicated completion file and has no activation-profile mutation.

The default paths require no flag. `--profile` exists only for intentional
nonstandard configurations:

```text
open-forge completion install zsh \
  --profile /home/me/dotfiles/.zshrc
```

A relative profile path is invalid. The plan always shows the supplied logical
path and any safely resolved symlink referent before confirmation.

### Owned State

The generated asset and activation block are the complete transparent
ownership receipt. Completion creates no workspace lifecycle entry, user-home
lockfile, transaction directory, or hidden registry.

Every generated asset contains a visible Open Forge header and a checksum of
its generated body. The checksum distinguishes an intact generated asset from
manual divergence without recording external profile state elsewhere.

Activation uses exact shell-comment markers whose source values live once in
production code:

```text
# open-forge:completion:start
<shell-native activation statement>
# open-forge:completion:end
```

The body contains only the minimal statement that loads the exact generated
asset. Bytes outside the marker block remain unchanged.

Owned state has the following semantic classifications:

| State      | Meaning                                                               |
| ---------- | --------------------------------------------------------------------- |
| Current    | Asset and activation match the current generated plan                 |
| Absent     | No owned asset or activation exists                                   |
| Outdated   | Owned state is intact but differs from the current generated plan     |
| Incomplete | Only the asset or activation exists                                   |
| Divergent  | An owned asset body or intact marker body was manually changed        |
| Malformed  | Marker boundaries are duplicate, unmatched, or ambiguous              |
| Unsafe     | Target identity or required metadata cannot be preserved and verified |

Exact discriminants, marker strings, header fields, and message codes live in
readonly const objects or enums in production source. Documentation defines
their meaning rather than duplicating implementation declarations.

### Installation

Installation uses this order:

```text
generate script
  -> create or replace and verify owned asset
  -> create or update and verify activation profile when required
  -> verify complete installed state
```

The asset always precedes activation. A hard stop after the asset write leaves
only a dormant asset. A rerun recognizes the incomplete state and finishes the
same plan. A hard stop after activation leaves a usable installation because
the asset already exists.

Current state is a successful no-op. Outdated intact state is eligible for
ordinary replacement. Divergent state requires an interactive `REPAIR` or
`KEEP` decision after the exact owned-region difference is shown. `--yes` does
not supply that decision. Malformed ownership blocks because the mutation
boundary cannot be proven.

### Removal

Removal reverses external activation before deleting generated data:

```text
remove and verify activation block when present
  -> delete and verify intact owned asset when present
  -> verify no owned active integration remains
```

A hard stop after activation removal leaves completion disabled and only a
dormant asset. A rerun safely removes that asset. Missing owned state is a
successful no-op.

An intact marker block may be removed while preserving every surrounding byte.
A divergent intact block requires an interactive `REMOVE` or `KEEP` decision.
An asset is deleted automatically only when its header and body checksum prove
intact Open Forge-generated content. Malformed markers or unproven assets are
preserved and reported for manual correction.

### Filesystem Safety

Every target uses the shared contained-identity and preservation-first effect
contracts where their platform semantics apply. Existing profiles preserve
bytes outside the owned block, encoding, byte-order mark, newline style, and
every required metadata property that the active runtime can reproduce and
verify.

A safely resolvable profile symlink is supported. The plan shows both the link
and referent, keeps the link itself unchanged, and revalidates both identities
before mutation. An imprecise runtime or unsupported link, hard-link, special
file, permission, or metadata state blocks and returns `completion script` plus
manual activation instructions.

Application revalidates the expected bytes immediately before replacement.
Concurrent external edits are preserved. Handled failure restores applied
effects in reverse order while their identities still match. Hard-stop
recovery relies on the monotonic ordering above and deterministic rerun rather
than Git, a persistent journal, or a profile backup side effect.

### Authority

`--yes` may accept ordinary confirmation for a completely resolved safe plan.
It cannot:

- Choose an ambiguous shell or PowerShell edition.
- Choose or invent a custom profile.
- Repair or remove divergent owned content.
- Resolve malformed markers.
- Follow an identity that the runtime cannot verify.
- Bypass metadata preservation or another blocked capability.

`--workspace` and `--skip-git-check` do not apply to user-level Completion
state. Supplying either is invalid rather than silently changing or ignoring
its meaning. Completion targets are protected by their exact external
preflight and recovery contract, not by workspace Git.

### Install Handoff

Interactive Framework installation ends with a visible reminder after the
Framework operation has completed:

```text
Shell completion was not changed.

Install completion for your detected shells:
  open-forge completion install
```

The reminder may offer a copyable or clickable next command. It does not call a
Completion handler inside `install`, merge profile effects into the
Framework plan, or emit multiple operation results from one invocation.

Framework removal, if later accepted, follows the same rule: it may remind the
user about `completion remove`, but it does not silently change external shell
configuration.

## Boundaries

Completion owns only explicit per-user shell integration. It never becomes a
Framework installation side effect or uses workspace Git as external-profile
recovery. It does not invent an unavailable shell target or mutate state whose
identity and owned boundary cannot be proven. Script generation remains the
safe manual fallback when an external mutation cannot satisfy this contract.

## Verification

Implementation evidence must prove:

- Script generation for every supported shell without a workspace.
- Default and custom target resolution without loading user profiles.
- Multi-select, explicit shell, and `--all` request equivalence.
- Creation of absent exact profiles only after visible selection and plan review.
- Byte and metadata preservation outside marker-owned regions.
- Current, absent, outdated, incomplete, divergent, malformed, and unsafe states.
- Symlink preservation and identity revalidation on every supported runtime and filesystem.
- Install and remove interruption after every persistent effect.
- Deterministic rerun from every incomplete hard-stop state.
- No Completion side effect from interactive, structured, or non-interactive Framework installation.

## Related Current Sources

- [CLI interface](../interface.md)
- [Completion protocol](completion-protocol.md)
- [Request construction](request-construction.md)
- [Operation prerequisites](operation-prerequisites.md)
- [Mutation execution](mutation-execution.md)
- [Filesystem effects](filesystem-effects.md)
