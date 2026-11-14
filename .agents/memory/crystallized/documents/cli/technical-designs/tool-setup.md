---
open-forge:
  description: Define round-2 native tool setup and additive lock records
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool setup

Tool setup is a CLI capability that installs published tools and adds configuration.
For example, bare Install restores the exact CodeGraph version in the lock, then runs its native setup.

Status: Round-2 design decided on 14 November 2026. This phase updates contracts and compile-only types before implementation.

## Catalogue

Framework Tools has one typed C# dictionary indexed by ID.
Entries declare ID, name, origin, upstream, licence, published channels and channel requirements.
Requirements cover runtime range, manager minimum, OS, CPU and Linux libc.
Profiles use non-overlapping major.minor ranges, tested versions, a minimum reviewed tool patch and a minimum Open Forge version.
A profile gives setup and uninstall argument vectors with declared effects and an optional native dry run.
Detection has a command, bounded version probe, parser and environment overlay.
V1 has no Open Forge tool Skill or managed-block line.
Native MCP, instruction blocks and Skills stay under upstream control.
Open Forge guidance can return if evals show a need.

## Versions

The [Tool Interface](../contracts/tool/interface.md) defines `id@version`.
Named Install and Update with a bare ID select latest support.
Bare Install uses exact lock versions and uses latest support only for a declaration without a lock.
Initial profiles are CodeGraph `1.6.x` from `1.6.2` and Codebase Memory `0.12.x` from `0.12.1`.
Their compatibility ranges keep the major.minor meaning and their minimum reviewed patches limit selectable releases.
Keep older supported profiles and exact pins as the catalogue grows.
The lock records the actual exact version, not a tested-baseline substitute.
Only known newer catalogue support justifies "update Open Forge first".

## Channels

Use an existing supported install, then a usable published package-manager channel, then private Node for an npm tool without suitable Node, then a verified GitHub archive.
An advertised `--channel` can override the choice.
Don't create a second install.
Before replacement, disclose account scope.

Detect npm, pnpm, Yarn Classic and bun by bounded CLI calls.
Yarn Modern is detected by major version and isn't a global-install candidate.
Prefer a single usable manager named by a project lockfile.
Ask when multiple clients or lockfile preferences apply.
With `--yes`, choose npm if usable, then the first usable client in npm, pnpm, Yarn Classic, bun order.
Check runtime compatibility and the selected client's global-bin PATH.
An entry can require one client.
Manager minimums are keyed by npm client so npm, pnpm, Yarn Classic and bun use their own minimum versions.
Package-manager verification is weaker than a reviewed dependency closure.

GitHub assets need pinned SHA-256 or verified attestation.
CodeGraph uses per-asset SHA-256 from its published `SHA256SUMS`.
Its 1.6.2 attestation/tag mismatch is a recorded note and doesn't block the SHA-256 route.
A digest mismatch still blocks.
Open Forge distributes recipes, not upstream tool bytes.

## Home and launchers

Use `~/.open-forge/`, or `%USERPROFILE%\.open-forge\` on Windows:

| Folder                     | Content                                                  |
| -------------------------- | -------------------------------------------------------- |
| `bin/`                     | Generated launchers and native binaries                  |
| `runtimes/node/<version>/` | Private Node                                             |
| `tools/<id>/<version>/`    | Open Forge-placed tool files                             |
| `state/`                   | Machine facts, native previous settings and PATH history |

Launchers are small `.cmd` files and executable `sh` scripts with absolute paths baked in.
They start the tool entry on its pinned runtime or forward to the upstream release launcher.
A native `.exe` can be placed directly.
Never put Node itself in bin.
Update rewrites launchers.
Open Forge takes no part when the tool runs.

Some native Windows MCP hosts need `cmd /c <tool>` for `.cmd` launchers.
Keep this as a native-test requirement.
Don't rewrite upstream MCP entries to impose this form.
Report an upstream host incompatibility if its own installer doesn't supply a working entry.

## Private Node

Use official portable archives from `nodejs.org/dist`.
Pin the archive SHA-256 from signed SHASUMS256.
Share the runtime by version and keep several versions when tools need them.
The extraction input omits reviewed link entries before calling the existing link-rejecting extractor.
The extractor itself still rejects links, escapes and special members.
Invoke npm through the real Node executable and npm CLI script.
On Unix the script is `<runtime>/lib/node_modules/npm/bin/npm-cli.js`.
Use the archive's `node_modules/npm/bin/npm-cli.js` on Windows.
Install the tool with that Node's own npm into `tools/<id>/<version>/`.
Native addon prebuilds then match the pinned runtime.
The user's Node stays untouched and there is no Node proxy.

Illustrative future Rune flow:

```text
published npm Rune + no suitable Node
  -> verified private Node
  -> that Node's npm installs Rune
  -> absolute-path launcher
  -> direct rune command
```

Rune isn't published and isn't a V1 entry.

## PATH

Status and Inspect always show bin and PATH membership.
Also show the selected manager's global-bin readiness.
If bin is missing, offer "Add it for me" or "I'll do it myself".
On Windows, use `Environment.SetEnvironmentVariable(..., EnvironmentVariableTarget.User)`.
Never use `setx` or system PATH.
On macOS and Linux, offer detected `.zshrc`, `.bashrc`, `.profile`, fish config and a custom file.
Write a self-identifying marked block only in files the person chooses.
Print the exact POSIX or fish line for manual setup.
Keep history per machine, and use the marks to find entries if history is lost.
Offer confirmed PATH removal after the last tool.
Say that terminals and agent apps need a restart.
`--yes` doesn't supply live confirmation for PATH edits or shared deletion.
Without a TTY, print instructions and leave those actions unselected.

## Native setup

Run the upstream installer unchanged through its documented argument vector.
CodeGraph uses `install --yes --location local --target claude,codex`.
Keep its hook, permissions, MCP names and instruction blocks.
Codebase Memory uses its account-wide installer with native client discovery.
Use its documented configuration-only binary mode after the verified binary is placed.
Disclose its global MCP entries, Skills, hooks, instructions and watcher defaults.
Open Forge writes no MCP entry or Codex TOML section for either tool.

Remove runs the native uninstall before Open Forge cleanup.
A native command's PATH and shared-binary effects are part of its declared plan.
If they require live confirmation, ask separately when a TTY is available.
Without live confirmation, print the native command and leave removal pending.
The [lane choices](tool-setup-lanes.md#recorded-choices) record the Codebase Memory case.
Don't request all-project index deletion.

## Additions and records

The [Tool record contracts](../contracts/shared/tool-records/_tool-records.md) define additive JSON union, absent-only set, marked block, absent-only file and native command changes.
Settings declare supported ranges.
The committed lock records exact version, channel and added items, keys, markers and file hashes.
An optional profile digest helps detect recipe changes.
Native previous account settings live in per-user state, keyed per workspace.
There are no repository config preimages and no workspace Tool edit state.
Tool commands don't run concurrently by design.
They add no lease or recovery dependency under `.open-forge-state/`.

Git covers tracked unchanged bytes.
Other existing bytes get sibling `.bak` files.
Git can't recover uncommitted bytes.
Use expected-state checks and normal atomic file replacement.
Keep incomplete effects and guarded manual recovery instructions.

## Operation flow

Resolve every selected tool, preflight all of them and show the plan.
If one is blocked, perform no effects.
Confirmation or `--yes` accepts downloads and the shown native setup.
Dry-run uses upstream dry run where available and otherwise describes the work.
It contacts the network only when latest must be resolved and says so.
Once effects start, stop at the first failure and report completed and unstarted work.
Reapply keeps additive receipts and reports drift.

Root hosts process, network and platform PATH capabilities.
Framework supplies catalogue, channel, config and record services.
Operations supplies command requests, selection, consent, plans and results.
Rendering and OutputText supply presentation.
The frozen interfaces are in `Framework/Tools/Models/Services/ToolServiceContracts.cs`.
No command or service behavior is implemented in this phase.

## Decisions

The maintainer's 14 November 2026 `of-tool-setup-decisions-2.md` is the detailed source.
The [round-2 issue comment](https://github.com/TheLithiumForge/open-forge/issues/23#issuecomment-6868316792) records the summary.
These decisions override the earlier [redesign](https://github.com/TheLithiumForge/open-forge/issues/23#issuecomment-6867293832) where they differ:

| ID    | Date       | Decided rule                                                                                                                                                                    |
| ----- | ---------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| R2-01 | 2026-11-14 | Repository facts and additive receipts go in the committed lock. Machine facts and native previous values go in user state. No Tool workspace state or extra cross-process lock |
| R2-02 | 2026-11-14 | Bare Install uses the exact lock version. Fresh installation uses latest support. Keep older supported config profiles                                                          |
| R2-03 | 2026-11-14 | Keep exact version, channel and applied changes in the lock. A profile digest is optional                                                                                       |
| R2-04 | 2026-11-14 | Use only `--yes` for unattended consent, including downloads. It never accepts PATH edits or shared deletion                                                                    |
| R2-05 | 2026-11-14 | Install and Update use `id@version`, with a bare name selecting latest support                                                                                                  |
| R2-06 | 2026-11-14 | Dry-run describes work, uses native dry run where available and discloses any latest-resolution network access                                                                  |
| R2-07 | 2026-11-14 | Native uninstall runs before Open Forge cleanup. Shared deletion and final PATH removal require confirmation                                                                    |
| R2-08 | 2026-11-14 | Skip Node archive links and invoke npm through Node and its CLI script. Keep extractor link rejection                                                                           |
| R2-09 | 2026-11-14 | Accept both native installers as they are, including Codebase Memory account setup. Add only Open Forge configuration                                                           |
| R2-10 | 2026-11-14 | Add no Open Forge tool Skills or managed-block lines in V1. Reconsider through evals                                                                                            |
| R2-11 | 2026-11-14 | Use the existing command result conventions, with local shape chosen by the mastermind                                                                                          |
| R2-12 | 2026-11-14 | Start at current releases, review every few months and pin CodeGraph SHA256SUMS. The attestation mismatch is a note                                                             |
| R2-13 | 2026-11-14 | CodeGraph version probes set `CODEGRAPH_NO_DOWNLOAD=1`                                                                                                                          |
| R2-14 | 2026-11-14 | Bare writers preflight all tools, perform no effects if blocked and stop at the first failure after effects start                                                               |
| R2-15 | 2026-11-14 | Detect npm, pnpm, Yarn Classic and bun, prefer project lockfiles, ask about multiple clients and check global-bin PATH                                                          |
| R2-16 | 2026-11-14 | Keep the round-1 home, absolute launchers, private Node and chosen marked PATH setup                                                                                            |

The lane plan records the remaining reversible choices.
Old #26 and #30 branches are copy sources, not the current design.
