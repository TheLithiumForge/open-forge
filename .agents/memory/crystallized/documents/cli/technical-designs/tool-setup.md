---
open-forge:
  description: Define round-2 native tool setup and additive lock records
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool setup

Tool setup is a CLI capability that installs published tools and adds configuration.
For example, bare Install restores the exact CodeGraph version in the lock, then runs its native setup.

Status: Phase 1 accepted on 14 November 2026. The maintainer's C1 through C7 answers below govern implementation.

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
The [RD1 channel rule](../contracts/tool/behavior.md#channel-choice) keeps normal channel choice after that version selection.
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

Remove runs a chosen native uninstall before Open Forge cleanup.
A native command's PATH and shared-binary effects are part of its declared plan.
Codebase Memory uninstall removes its account-wide binary and all agent entries.
Offer it only without `--yes` and with a live terminal, naming those effects in the confirmation.
With `--yes`, no terminal or a declined confirmation, skip it and print the command.
Then remove Open Forge's project additions.
Keep account-wide native settings on every project Remove and print their manual change commands.
Other projects can rely on telemetry off, so there is no automatic restoration or cross-project setting registry.
The [lane decisions](tool-setup-lanes.md#recorded-choices) assign these rules.
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

### Shared planning

A channel request includes the Open Forge home.
Every Tool service `HomePath`, including record locations and recipe context, names the absolute Open Forge home itself.
Root appends `.open-forge` to the OS-user home once.
An absent Open Forge home is a missing read fact. Reading doesn't create it.
A command resolver returns an absolute command path and its real executable invocation.
Detection carries verified recorded-install facts and the responsible manager into channel planning.
A successful resolution includes its executable invocation, command path, runtime, native install directory, ordered process commands, download and extraction destinations and launchers.
Install takes that resolution and executes the checked plan.
Each npm channel declares its entry path relative to the installed package root.
An exact selector uses that declared path and needs no metadata network call.
For latest resolution without a runnable client, Root can read bounded npm metadata through `IToolDownloadHost.ReadNpmMetadataAsync`.
The result gives versions, their package-relative entries, optional Node ranges and the registry used, or a failure.
Honor registry and proxy trust settings. Block unsupported authenticated configuration without reading or returning credentials.
This read adds no package-manager installation or configuration change.
A returns verified private-runtime records with its tool state.
Shared-file removal receives C's locations and expected records, then checks the current launcher inventory before removing an unused runtime.
A handles package-manager paths and supply ordering.
D uses those facts for reporting and native setup.
PATH add and removal results also return pending file-backup handles.
D keeps those handles until their machine record and the final operation verify.
For reused CBM, the native install directory contains its verified existing command.
Setup `--dir` points there without creating another installation.

Tool planning checks files and backup feasibility without creating backups.
Explicit `--allow-path` remains an existing authored settings edit.
C exposes a separate settings-only grant request so it doesn't create a tool lock or machine records before tool effects.
D publishes and reports that edit before tool planning, using the existing grant grammar and Tool backups.
It writes nothing in dry-run and stays saved if later tool work can't proceed.
A missing future executable isn't invoked.
Whole-batch preflight checks every available requirement before effects.
Later native discovery or execution can still fail.
Report that effect failure and completed receipts without claiming that a first-install description predicts every native outcome.

### Recipe values

The [recipe definitions](../../../../../../src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Models/Recipes/ToolRecipeContext.cs) define the shared context and token spellings.
Context paths are resolved absolute host paths.
`MainRoot` is a workspace-relative slash path without a trailing slash.
E.g. `context` produces `context/memory/archived/**`.
Client root keys match `[a-z][a-z0-9-]*` and name resolved roots provided by Root or native output.

Expand tokens once in setup, preview and inventory arguments, environment values, effect and output paths, JSON string values, block content and strict UTF-8 managed-file content.
Keep JSON member names, JSON Pointers, markers, resource IDs and environment keys literal.
`${client:<id>}` selects one declared client root.
`${previous}` is valid only in a manual native-setting command and expands to the recorded setting.
`ToolRecipeDefinitions.DollarEscapeToken` defines `$$` as one literal dollar.
A single dollar outside a token stays literal.
E.g. with `MainRoot` set to `context`, `$$${mainRoot}` produces `$context`, while `$cash` stays `$cash`.
Unknown tokens, missing roots, malformed tokens and invalid paths block planning.
Inserted values aren't expanded again.
Expansion adds no config-edit kind.
A null environment value removes that key.
The process host starts from Root's injected base environment, then applies the overlay.
Keep the first native previous-value observation when replanning file changes after native setup.
A changed stored setting blocks revalidation instead of becoming an inferred previous value.

A managed-file resource ID is its exact manifest logical name in the Framework assembly.
B hashes its original bytes for the profile digest and expands its content for apply.
V1's entries need no new managed-file resource or packaging rule.

### Native outputs

B implements read-only `IToolSetupService.PlanAsync`.
A setup spec declares possible outputs and can declare one bounded native inventory command.
With an installed invocation, use its inventory command, otherwise its declared dry run.
Without an installed invocation, describe the command and possible destinations.
Dry-run output is descriptive and isn't parsed for paths.

C implements `IToolNativePlanReader` for CBM's `agent.install.plan.v1` format.
Require exit zero, bounded complete stdout, the declared type, typed arrays, `writes_started=false` and `network_after_install=false`.
Keep stderr separate and never execute `next_safe_command`.
Collect config, instruction, Skill, agent and prompt file arrays and hook paths.
Add declared profile, legacy-cleanup and malformed-JSON backup destinations omitted by native inventory.
A supplement's optional client ID matches the complete native detected-client set.
Without native inventory, show it as conditional potential output and keep the detected-client set unavailable.
Install and uninstall have separate declarations. An install plan isn't an uninstall inventory.
Keep native discovery and the full client set.
Declarations cover possible writes without selecting clients or reproducing discovery.

An exact backup output names a possible file, including an absent file.
An existing-pattern output matches existing wildcard components using `*` and `?` within slash-separated segments.
Keep an exact final filename even when it is absent inside a matched existing directory.
E.g. `profiles/*/mcp.json` includes a missing `mcp.json` in an existing profile.
It covers profile folders and legacy names without recursive `**`.
Check resolved native paths and client roots through physical containment.
Deduplicate concrete backup targets with host path comparison.
Keep SHA-256 expectations, using null for absence.
Check occupied sibling backups during planning.
Revalidate and take backups immediately before native writes.
Report-only outputs also describe native empty-directory cleanup without backing up arbitrary trees.
They describe telemetry and native cache effects without reading identity or queues.
If inventory names a destination outside the reviewed possible set, stop before native setup and report it.

A marked block can declare an effective-ignore condition.
B checks Git's effective data coverage during preflight, after native output and immediately before apply.
Existing root rules or an internal ignore can satisfy it.
Add the block only when coverage is absent.
When its recorded block already supplies coverage, keep that receipt and return no change.
Don't create an index to obtain coverage.

### Runtime targets

Host wrapper Node requirements stay in the channel runtime requirement.
The V1 runtime grammar accepts an exact stable version or `>=major[.minor[.patch]]`, with an optional `<major[.minor[.patch]]` ceiling.
Missing numeric components mean zero.
This catalogue grammar is separate from CLI tool selectors.
Choose the newest approved private Node inside those bounds.
Different bounds can select different private runtime versions.
Lane A provides the pinned official runtime catalogue and its signed-checksum review evidence.
Target requirements also cover bundled runtimes.
CodeGraph's bundled Node needs Linux kernel 4.18, glibc 2.28 and libstdc++ 6.0.25.
CBM's unspecified numeric floors stay unspecified.
Assets declare bundled runtime version, executable, application entry and runtime arguments relative to the extraction root.
A resolves these declarations without filename heuristics.
Keep upstream release launchers for normal direct execution.

### Root hosts

Root can receive complete Tool homes, client roots, base process environment and process, download and PATH hosts through composition inputs.
Use supplied inputs throughout detection, planning and effects.
Each profile declares its native home policy.
For CodeGraph's `Node` policy, Windows uses `USERPROFILE` and other systems use `HOME`.
When that variable is absent, resolve the OS-user home with the same native rule.
CBM's `HomeThenUserProfile` policy uses the first nonempty `HOME`, then `USERPROFILE`.
If the native home can't be resolved, block preflight.
Resolve these policies from the injected base environment without changing normal child environment.
Root passes only the native allow-list's declared environment variables in `ToolRecipeContext.NativeEnvironment`.
Null means unavailable. An empty map means checked with none present.
B evaluates native precedence and reports `ToolConfigObservation.NativeEnvironmentOverrides` separately from the stored setting.
Its null value means unchecked or unavailable. An empty map means no effective override.
For CodeGraph, a nonempty result has the deciding variable as its key and the effective `on` or `off` value.
Raw environment values stay out of reports and records.
An injected `NativeHomePath` overrides policy resolution for owned journeys.
The journey provides child environment that makes the native tool use that same home.
Root doesn't rewrite normal child environment to force a resolved home.
Resolve production known folders once in Root.
Changing a child environment doesn't relocate .NET Windows known folders.
`TestDataHome` covers legacy stores and isn't a Tool home resolver.

Test live CBM account-uninstall consent and native-first ordering with an injected process host.
The real Windows journey uses `--yes` project-only removal.
Upstream confirmed uninstall writes persistent user PATH, so it stays outside that real journey.
These inputs add no command flag or production test mode.

### Native config reader

CBM reads OpenClaw workspace paths from JSON-like configuration that includes JSON5 syntax.
Root needs the same read behavior to check native output paths before effects.
The BCL JSON reader rejects supported input.
The two checked package readers lose duplicate members or accept input that CBM rejects.

We use a bounded C# port of CBM `v0.12.1`'s read-only validation and path lookup.
The source is [CBM's JSON-like reader](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/config_json_like.c).
Keep its licence and source notice with the port.
Validate the complete UTF-8 object, then read `agents.defaults.workspace` and root `$include` with the native ambiguity rules.
Validate and skip unrelated values without converting their numbers.
Treat a zero-length file as missing.
Keep native workspace, include, environment and profile precedence.
The reader adds no writer, include expansion, runtime, package or public parser API.
It stays private to Root's native client-path resolution.

This is the smallest complete format adapter after the ordinary readers failed.
Focused tests cover native syntax families, invalid complete documents, ambiguous path members, empty input and fallback precedence.
CI checks Native AOT through the normal Root build.
Recheck the port with each CBM profile review.
Replace it with an ordinary reader when one supports the required behavior.

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
| R2-07 | 2026-11-14 | A chosen native uninstall runs before cleanup. C2 governs Codebase Memory's account-wide confirmation and project-only cleanup                                                  |
| R2-08 | 2026-11-14 | Skip Node archive links and invoke npm through Node and its CLI script. Keep extractor link rejection                                                                           |
| R2-09 | 2026-11-14 | Accept both native installers as they are, including Codebase Memory account setup. Add only Open Forge configuration                                                           |
| R2-10 | 2026-11-14 | Add no Open Forge tool Skills or managed-block lines in V1. Reconsider through evals                                                                                            |
| R2-11 | 2026-11-14 | Use the existing command result conventions, with local shape chosen by the mastermind                                                                                          |
| R2-12 | 2026-11-14 | Start at current releases, review every few months and pin CodeGraph SHA256SUMS. The attestation mismatch is a note                                                             |
| R2-13 | 2026-11-14 | CodeGraph version probes set `CODEGRAPH_NO_DOWNLOAD=1`                                                                                                                          |
| R2-14 | 2026-11-14 | Bare writers preflight all tools, perform no effects if blocked and stop at the first failure after effects start                                                               |
| R2-15 | 2026-11-14 | Detect npm, pnpm, Yarn Classic and bun, prefer project lockfiles, ask about multiple clients and check global-bin PATH                                                          |
| R2-16 | 2026-11-14 | Keep the round-1 home, absolute launchers, private Node and chosen marked PATH setup                                                                                            |

The maintainer accepted Phase 1 and answered C1 through C7 in the task conversation on 14 November 2026.
These answers override the earlier R2-07 interpretation and per-project native-setting restoration:

| ID  | Date       | Decided rule                                                                                                                                                                                                    |
| --- | ---------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| C1  | 2026-11-14 | CLI selectors are major.minor or exact stable SemVer. A bare ID selects latest support. No dist-tags or full npm range syntax                                                                                   |
| C2  | 2026-11-14 | Codebase Memory uninstall is account-wide. Only live interactive confirmation naming that effect permits it. With `--yes` or no terminal, remove only Open Forge project additions and print the native command |
| C3  | 2026-11-14 | Keep optional profileDigest and its fixed JSON projection                                                                                                                                                       |
| C4  | 2026-11-14 | Keep the machine-state filenames and workspace physical-path SHA-256 keys                                                                                                                                       |
| C5  | 2026-11-14 | Keep `--channel` on named Install and Update and schema-3 results with `data.tools`                                                                                                                             |
| C6  | 2026-11-14 | Start at CodeGraph 1.6.2 and Codebase Memory 0.12.1. CodeGraph's wrapper needs Node 20 or newer. Review every three months                                                                                      |
| C7  | 2026-11-14 | Project Remove keeps account-wide native settings, reports them and prints native change commands. No per-project restoration or cross-project registry                                                         |

Old #26 and #30 branches are copy sources, not the current design.

## Implementation choices

The mastermind makes these reversible choices under the maintainer's instruction to continue and record questions:

| ID  | Date       | Choice                                                                                                                                                                                                   |
| --- | ---------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| I1  | 2026-11-14 | Port only CBM's read-only JSON-like validation and path lookup for Root's OpenClaw paths after the BCL and two package readers fail native compatibility. Keep the adapter bounded and add no dependency |

## Not decided yet

- Should we keep the bounded native-reader port as the V1 format adapter?
- Should the catalogue's minimum Open Forge version be `0.9.0-beta.19`?
