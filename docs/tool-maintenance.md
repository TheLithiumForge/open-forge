# Tool maintenance

Tool maintenance is the review procedure that accepts a tool and an exact pin into Open Forge's catalogue.

V1 supports CodeGraph only. Release verification checks the downloaded bytes. Source review checks installer scope and commands. A native journey checks their effects. Complete each before enabling a target.

## Pin checklist

Complete these steps for every pin:

1. Read the latest maintainer decisions and contracts. Declare `first-party` or `third-party` origin. Record repository and licence.
2. Resolve the stable tag to its full source commit. Keep any distinct attested workflow commit separate.
3. Select the exact official asset for all six targets. Record URL, byte size and published SHA-256. Block missing assets with a reason.
4. Download and verify every selected archive against release metadata and published checksums. Verify published attestations with exact signer identity and issuer.
5. Read pinned source, launcher, `--help` and `--print-config`. Record every file, hook, instruction block, Skill, profile, account effect and removal effect. Use local fixtures for tests.
6. Declare the native installer arguments, expected original entries and bounded post-install rewrites. A changed source shape stops the rewrite. Keep entry names and removal markers.
7. Disclose third-party origin, official download, licence and upstream maintenance. Ask about the optional prompt hook, default yes. Non-interactive installs take `--prompt-hook yes|no`.
8. Declare telemetry off through the tool's own command, a visible setting, update checks off and no watchers or automatic indexing during Open Forge writes.
9. Ship a short required usage Skill. Keep installation internals here and in contracts. Add one conditional usage line per active role to Open Forge's managed agent block.
10. Test schema, hashes, targets, origin, commands, rewrite originals, guidance and blocked promotion with offline fixtures. Check docs, help examples and both CLI Skill copies.
11. After core integration, run the Windows x64 journey in a temporary Git repository under D: with project-only Claude Code and Codex. Inventory all writes, build the index through CodeGraph, check Status, remove and verify the rewritten hook is removed. Record agreed leftovers.
12. Require the other five native journeys in CI. Enable only qualified targets. Keep native installation evidence separate from language accuracy.
13. Accept the exact manifest digest and adapter revision, then set the catalogue default. Run the required .NET whitespace and style checks before every commit.

The [manifest contract](../.agents/memory/crystallized/documents/cli/contracts/shared/tool-manifest/interface.md) defines production support. The [review schema](../schemas/tools/drafts/v1/definition.schema.json) keeps unqualified commands outside that support. A wrapper cannot be passed to production `--source`.

## CodeGraph pin

The [definition draft](../src/tools/drafts/codegraph-1.6.2.json) pins `v1.6.2` at `6560052a6f856855d3f71eee838fd66ccfa4285d`, under MIT with third-party origin. All six archives pass length, published SHA-256 and build-attestation verification. The identity is `https://github.com/colbymchenry/codegraph/.github/workflows/release.yml@refs/heads/main`, with issuer `https://token.actions.githubusercontent.com`. The attested workflow source is `0f0af7bf22a5083a47e34fcb1e1bf67355174ef8`. The release workflow makes commits before publishing, so we keep both commits without asserting equivalence.

The official archive's `bin/codegraph(.cmd)` launches bundled Node 24.16.0. Install uses `install --location local --target claude,codex --yes --no-permissions`. `--yes` skips npm and PATH setup. No system Node or npm is needed. Uninstall uses the same local targets with `--keep-cli`, then `uninit <root> --force` removes the project index.

The [consent inventory](tools/codegraph-consent.md) defines every output and exact command rewrite. Keep its native server name `codegraph`. MCP rewrites keep Claude's `type` and `alwaysLoad`. TOML rewrites change only the expected command and args lines. The prompt-hook rewrite keeps `codegraph prompt-hook` for native uninstall. Keep the CodeGraph instruction block because upstream measurements found subagents read instruction files but miss MCP initialization instructions.

## Process policy

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

Check Git's resolved hook path under that same child environment. Don't edit real Git hooks, system PATH or the registry. Prompt-hook `no` removes only recognized matching commands, then prunes empty containers as upstream does. `CODEGRAPH_NO_PROMPT_HOOK=1` also disables runtime hook behavior.

For a user-approved first index, use `open-forge tool exec codegraph init --yes` from the workspace root. This initializes and indexes. Use `index --quiet` for a full rebuild of an initialized index and `sync` for changed files. The private hook sink and process opt-outs apply to these external commands too. Install itself never passes `--init` or builds an index.

Our C# trial measured 31 of 75 sites, 41.3% recall and 88.6% precision. TypeScript recall was 56.4%. Python recall was 82.3%. JavaScript was unmeasured. CodeGraph doesn't supply the PowerShell graph used in this trial. The required Skill tells agents to confirm call sites in source.

## Source verification

On 2026-11-13, the verified Windows launcher returned help for `install`, `uninstall`, `init`, `index`, `sync`, `uninit`, `telemetry` and `explore`. `install --location local --print-config claude` printed `type:stdio`, `command:codegraph`, `args:[serve,--mcp]` and `alwaysLoad:true` at `.mcp.json`. The Codex form printed `[mcp_servers.codegraph]` and exactly the command and args lines recorded in the draft. `--print-config` takes one target ID. No installer was applied.

Pinned `claude.ts`, `codex.ts` and `shared.ts` confirm `.claude/CLAUDE.md` and root `AGENTS.md`, prompt-hook matching and native cleanup. `instructions-template.ts` supplies the exact shell placeholder. Runtime help confirms `index` requires initialization in source and `init --yes` builds the first index. Native effects still need the journey.

## Passive checks

Status and Inspect use the declared native name `codegraph` at `.mcp.json` and `.codex/config.toml`. They don't rename entries or run CodeGraph. JSON checks read the exact member. TOML checks read only supported literal table and line forms without a parser. Presence alone is `present-unverified`. Ambiguous, unreadable or unsupported text keeps an unavailable cause. Inspect must show origin, upstream, licence, outputs, rewrites, hook choice, telemetry and trust notes even at normal detail.

## Unsupported tool

Codebase Memory `v0.12.1` was verified across all six selected archives, including published SHA-256 and individual Sigstore bundles. Its checksum bundle also verified. The [research record](tools/codebase-memory-research.md) keeps exact source, release evidence and the full account write inventory. It has no packaged definition, Skill or public tools page.

An upstream change needs project-scoped install and uninstall and a hook opt-out. The native installer writes account Codex `AGENTS.md`, Skills, profiles and hooks. `--skip-config` and `--skip-binary` don't supply project scope. Uninstall also sweeps account clients and can edit PATH. Future support needs a new pin and a new native review. Don't contact upstream.

## Upstream requests

The CodeGraph definition records these missing controls:

- Set the launch command, or write an absolute launcher path when CodeGraph isn't on PATH.
- Add `--no-prompt-hook` for non-interactive installs.

These would remove the corresponding Open Forge rewrites and hook cleanup. The maintainer can use this record for later upstream pull requests.

## Not decided yet

- Native qualification and bounded lifecycle timeouts wait for the core writers and real journeys.
- Verify every rewritten entry and agreed leftover on Windows, then the other five platforms in CI.
