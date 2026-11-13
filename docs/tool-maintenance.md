# Tool maintenance

Tool maintenance is the review procedure that accepts a tool and an exact pin into Open Forge's catalogue.

V1 supports CodeGraph only. Release verification checks the downloaded bytes. Source review checks installer scope and commands. A native journey checks their effects. Complete each before enabling a target.

## Pin checklist

Complete these steps for every pin:

1. Read the definition trust notes, this worked example and the contracts. Declare `first-party` or `third-party` origin. Record repository and licence.
2. Resolve the stable tag to its full source commit. Keep any distinct attested workflow commit separate.
3. Select the exact official asset for all six targets. Record URL, byte size and published SHA-256. Block missing assets with a reason.
4. Download and verify every selected archive against release metadata and published checksums. Verify published attestations with exact signer identity and issuer.
5. Read pinned source, launcher, `--help` and `--print-config`. Record every file, hook, instruction block, Skill, profile, account effect and removal effect. Use local fixtures for tests.
6. Declare the native installer arguments, expected original entries and bounded post-install rewrites. A changed source shape stops the rewrite. Keep entry names and removal markers.
7. Disclose third-party origin, official download, licence and upstream maintenance. Ask about the optional prompt hook, default yes. Non-interactive installs take `--prompt-hook yes|no`.
8. Declare telemetry off through the tool's own command, a visible setting, update checks off and no watchers or automatic indexing during Open Forge writes.
9. Ship a short required usage Skill. Keep tool-specific installation internals in its trust notes and this worked example. Add one conditional usage line per active role to Open Forge's managed agent block.
10. Test schema, hashes, targets, origin, commands, rewrite originals, guidance and blocked promotion with offline fixtures. Check docs, help examples and both CLI Skill copies.
11. After core integration, run the Windows x64 journey in a temporary Git repository under D: with project-only Claude Code and Codex. Inventory all writes, build the index through CodeGraph, check Status, remove and verify the rewritten hook is removed. Record agreed leftovers.
12. Require the other five native journeys in CI. Enable only qualified targets. Keep native installation evidence separate from language accuracy.
13. Accept the exact manifest digest and adapter revision, then set the catalogue default. Run the required .NET whitespace and style checks before every commit.

The [manifest contract](../.agents/memory/crystallized/documents/cli/contracts/shared/tool-manifest/interface.md) defines production support. The [review schema](../schemas/tools/drafts/v1/definition.schema.json) keeps admission blocked while validating the full shared definition shape. A wrapper cannot be passed to production `--source`.

## CodeGraph example

The [definition draft](../src/tools/drafts/codegraph-1.6.2.json) and this worked example define the CodeGraph-specific V1 decisions. The [public tools page](tools.md) explains their effects for you. Issue comments record discussion and aren't the specification.

The pin is `v1.6.2` at `6560052a6f856855d3f71eee838fd66ccfa4285d`, under MIT with third-party origin. All six archives pass length, published SHA-256 and build-attestation verification. The identity is `https://github.com/colbymchenry/codegraph/.github/workflows/release.yml@refs/heads/main`, with issuer `https://token.actions.githubusercontent.com`. The attested workflow source is `0f0af7bf22a5083a47e34fcb1e1bf67355174ef8`. The release workflow makes commits before publishing, so we keep both commits without asserting equivalence.

### Install

Prepare the verified official archive in the per-user versioned store. Its `bin/codegraph(.cmd)` launcher runs bundled Node 24.16.0. `ToolTarget.Launcher` expresses the same launch without a shell. The contained executable is `node.exe` on Windows and `node` elsewhere. Fixed arguments are `--liftoff-only`, `--disable-warning=ExperimentalWarning` and `${install}/lib/dist/bin/codegraph.js`. Both runtime and script stay in the verified store. Run `install --location local --target claude,codex --yes --no-permissions` through this launch binding with the physical workspace as CWD. `--yes` skips global npm installation and PATH setup. System Node, npm, PATH and registry changes aren't needed. Install doesn't choose a role or build an index.

Bare `open-forge tool install` installs every tool declared by the workspace. A teammate can run it after cloning. When CodeGraph is declared but isn't installed locally, `open-forge tool run codegraph <args...>` stops and tells the teammate to run that bare install command. Interactive Run offers normal Install consent. Non-interactive Run never installs silently. Exact artifact consent, verification, destination grants and hook choice still apply.

### Entries and rewrites

After every Install, Configure and Update, apply these declared command rewrites to the entries CodeGraph wrote:

| File and entry                                                                       | Expected original                                                          | Declared rewrite                                                                                    |
| ------------------------------------------------------------------------------------ | -------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------- |
| `.mcp.json`, `mcpServers.codegraph`                                                  | `command:codegraph`, `args:[serve,--mcp]`, `type:stdio`, `alwaysLoad:true` | Command from one resolver and args `[tool,run,codegraph,serve,--mcp]`. Keep `type` and `alwaysLoad` |
| `.codex/config.toml`, `[mcp_servers.codegraph]`                                      | `command = "codegraph"`, `args = ["serve", "--mcp"]`                       | Only those two lines, to the resolved command and args `[tool,run,codegraph,serve,--mcp]`           |
| `.claude/settings.json`, `UserPromptSubmit` hook                                     | `codegraph.cmd prompt-hook` on Windows, `codegraph prompt-hook` elsewhere  | Resolved command followed by `tool run codegraph prompt-hook`                                       |
| `.claude/CLAUDE.md` and root `AGENTS.md`, `CODEGRAPH_START` to `CODEGRAPH_END` block | `codegraph explore "<symbol names or question>"`                           | Resolved command followed by `tool run codegraph explore "<symbol names or question>"`              |

The shared `ToolRegistration` holds five `ToolEntryRewrite` records across four forms. `json-command` and `toml-command` describe MCP. `hook-command` describes the prompt hook. Two `marked-line` records describe the native instruction files. The full expected marked line is:

```text
- **Shell** (always works): `codegraph explore "<symbol names or question>"` prints the same output.
```

The replacement changes only the command inside that line to `"${openForge}" tool run codegraph explore "<symbol names or question>"`. The two MCP forms use `${openForge}` as command and an argument array. The hook uses `"${openForge}" tool run codegraph prompt-hook`.

One resolver uses `open-forge` on PATH or the absolute path of the running Open Forge executable otherwise. Quote the result for each destination. JSON uses the existing serializer. Codex replaces only exact command and args lines as text, with no TOML parser. An unexpected or ambiguous shape stops and reports the file. An exact recorded matching rewritten hook stays unchanged on rerun because native Install can keep that hook.

Keep the native server name `codegraph`, block markers and literal substring `codegraph prompt-hook` so native Uninstall recognizes them. Keep unrelated members, hooks and instruction lines. `--no-permissions` prevents Claude's `mcp__codegraph__*` auto-allow entry. The installer can remove recognized legacy `.claude.json` MCP members and `mark-dirty` and `sync-if-dirty` hooks. The [consent inventory](tools/codegraph-consent.md) records every native file, temporary and cleanup effect.

### Hook choice

`ToolOptionalComponent` declares ID `prompt-hook`, finite flag `prompt-hook`, default enabled and the complete expected JSON entry for decline. This maps to `--prompt-hook yes|no`. Ask whether to keep the Claude prompt hook. Defaults to yes, as CodeGraph does. On structural prompts, it adds context from the local index. Nothing leaves the machine. `CODEGRAPH_NO_PROMPT_HOOK=1` disables it at runtime. Non-interactive installs take `--prompt-hook yes|no`. On no, remove only the complete matching CodeGraph hook entry. Keep its containing group, sibling commands and other hooks, as the shared component contract requires.

### Telemetry

`ToolLifecycle.TelemetryOff` declares `[telemetry,off]`. `TelemetryOn` declares `[telemetry,on]`. Both run once in workspace scope while changing the disclosed account preference. Run CodeGraph's `telemetry off` unless the visible `tools.codegraph.telemetry` setting enables it. Disclose the account preference at `~/.codegraph/telemetry.json`, its temporary file and deletion of buffered telemetry files. Suppress telemetry before all other invocations. Keep update checks off even when telemetry is enabled. Consent and Inspect list all files, rewrites, hook choice and telemetry.

### Removal

`ToolLifecycle.RemovalOrder` is `[uninstall,index-cleanup]`. Run `open-forge tool run codegraph uninstall --location local --target claude,codex --yes --keep-cli`, then `open-forge tool run codegraph uninit <root> --force`. Native Uninstall removes recognized project entries, blocks and hooks, including the rewritten prompt hook. Uninit removes only this project's `.codegraph` index and checks Git hooks through the private sink.

Then remove Open Forge's required Skill, conditional active-role line and managed `.codegraph/` ignore entry. Release the store version only when no workspace uses it. Report agreed leftovers. Keep unrelated content. Git is the backup in Git workspaces. Without Git, prepare a sibling `.bak` before changing an existing file.

### Agent guidance

Keep CodeGraph's own instruction block with its corrected shell line. Upstream measurements found subagents read `CLAUDE.md` and `AGENTS.md` but miss MCP initialization instructions. The required `open-forge-codegraph` Skill stays short and practical. It gives MCP versus shell use, first Init, rebuild, sync, teammate setup and the C# recall warning.

While CodeGraph fills code-navigation, Open Forge's own managed `AGENTS.md` and `CLAUDE.md` blocks contain one conditional line:

```text
Code navigation: CodeGraph is active. Use `codegraph_explore` or `open-forge tool run codegraph explore "<question>"` before grep, see the open-forge-codegraph Skill.
```

Remove that line when the role is cleared, moved or removed. Install leaves roles unchanged. Keep the native CodeGraph block independent of that line. `ToolRoleGuidance` holds the role and line. `ToolRegistration.Activate` repeats local Install. `Deactivate` repeats local Uninstall with `--keep-cli`, so switching keeps the stored version and index. Index cleanup belongs only to Remove.

### Upstream requests

The definition's trust notes record these missing controls:

- An installer option to set the launch command, or an absolute launcher path when CodeGraph isn't on PATH. This removes the Open Forge rewrites.
- `--no-prompt-hook` for non-interactive installs. This removes the separate Open Forge opt-out cleanup.

Don't contact upstream unasked.

### Process policy

The accepted launcher profile applies these settings:

| Setting                     | Value or rule                                                                    |
| --------------------------- | -------------------------------------------------------------------------------- |
| `DO_NOT_TRACK`              | `1` when off, `0` when enabled                                                   |
| `CODEGRAPH_TELEMETRY`       | `0` when off, `1` when enabled, so inherited values cannot contradict the choice |
| Native telemetry            | Run `telemetry off` unless the saved user setting enables it                     |
| `CODEGRAPH_NO_UPDATE_CHECK` | `1`, including when telemetry is enabled                                         |
| `CODEGRAPH_NO_DAEMON`       | `1`                                                                              |
| `CODEGRAPH_NO_WATCH`        | `1`                                                                              |
| `CODEGRAPH_DIR`             | Selected single-segment project index directory, default `.codegraph`            |
| `GIT_CONFIG_PARAMETERS`     | Empty                                                                            |
| `GIT_CONFIG_COUNT`          | `1`                                                                              |
| `GIT_CONFIG_KEY_0`          | `core.hooksPath`                                                                 |
| `GIT_CONFIG_VALUE_0`        | Verified private hook sink                                                       |

Check Git's resolved hook path under that same child environment. Don't edit real Git hooks, system PATH or the registry. Prompt-hook `no` removes only the complete matching entry and keeps its containing group. `CODEGRAPH_NO_PROMPT_HOOK=1` also disables runtime hook behavior.

For a user-approved first index, use `open-forge tool run codegraph init --yes` from the workspace root. This initializes and indexes. Use `index --quiet` for a full rebuild of an initialized index and `sync` for changed files. The private hook sink and process opt-outs apply to these external commands too. Install itself never passes `--init` or builds an index.

Our C# trial measured 31 of 75 sites, 41.3% recall and 88.6% precision. TypeScript recall was 56.4%. Python recall was 82.3%. JavaScript was unmeasured. CodeGraph doesn't supply the PowerShell graph used in this trial. The required Skill tells agents to confirm call sites in source.

## Source verification

On 2026-11-13, the verified Windows launcher returned help for `install`, `uninstall`, `init`, `index`, `sync`, `uninit`, `telemetry` and `explore`. `install --location local --print-config claude` printed `type:stdio`, `command:codegraph`, `args:[serve,--mcp]` and `alwaysLoad:true` at `.mcp.json`. The Codex form printed `[mcp_servers.codegraph]` and exactly the command and args lines recorded in the draft. `--print-config` takes one target ID. No installer was applied. Reading the launcher members in all six verified archives confirms each declared bundled executable, fixed flags and script path.

Pinned `claude.ts`, `codex.ts` and `shared.ts` confirm `.claude/CLAUDE.md` and root `AGENTS.md`, prompt-hook matching and native cleanup. `instructions-template.ts` supplies the exact shell placeholder. Runtime help confirms `index` requires initialization in source and `init --yes` builds the first index. Native effects still need the journey.

## Passive checks

Status and Inspect use the declared native name `codegraph` at `.mcp.json` and `.codex/config.toml`. They don't rename entries or run CodeGraph. JSON checks read the exact member. TOML checks read only supported literal table and line forms without a parser. Presence alone is `present-unverified`. Ambiguous, unreadable or unsupported text keeps an unavailable cause. Inspect must show origin, upstream, licence, outputs, rewrites, hook choice, telemetry and trust notes even at normal detail.

## Separate draft

Codebase Memory's draft, verified research and missing upstream controls are preserved on `feature/tool-codebase-memory` at `7c76ca5fb`. This branch carries CodeGraph only.

## Integration gates

The typed lifecycle uses policy caps of 120 seconds for installation, removal, cleanup and role commands and 30 seconds for telemetry. These aren't measured native timings. Recheck them during qualification.

The shared hook form carries one exact expected string and JSON Pointer. CodeGraph writes `codegraph.cmd prompt-hook` on Windows and `codegraph prompt-hook` elsewhere. It appends its hook group after existing `UserPromptSubmit` groups. The current typed hook and decline pointers describe only the pristine Windows fixture at array index zero. `review.registrationGaps` records both limits. Keep admission blocked until Registration can bind the correct platform spelling and appended entry without assuming index zero.

## Not decided yet

- Resolve the shared representation for hook platform spelling and variable array position with the Registration lane.
- Verify every rewritten entry and agreed leftover on Windows after the core lanes arrive, then the other five platforms in CI.
