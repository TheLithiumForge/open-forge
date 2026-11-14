---
open-forge:
  description: Review and accept tool recipes, official downloads, pinned runtimes and lifecycle commands
  tags: [Memory, Document, CurrentTruth, Maintenance, Tool, Supply]
---

# Tool admission

Tool admission is a maintenance process that reviews an exact download recipe before an Open Forge release can install it.

For example, changing a release asset's hash creates a new recipe to review. A local manifest with that hash can't bypass release admission. Open Forge distributes recipes and never ships, mirrors or rehosts tool bytes.

Status: V1 supply model decided.

## Source

The [Tool manifest contract](../cli/contracts/shared/tool-manifest/interface.md#supply-recipes) defines the accepted supply rules. The [external-tools design](../cli/technical-designs/external-tools.md) defines stores, process capabilities and the split between workspace and account records. This checklist defines how maintainers review those recipes before release admission.

## Contract

### Review a recipe

Complete these checks before adding a tool or changing a pin:

1. Confirm the upstream project, source commit, release tag, license and notices. Record any use or distribution conditions that affect the recipe. Downloading from upstream doesn't remove its license conditions.
2. Select the exact official source for every target. Native tools use the project's GitHub release asset. Node uses `https://nodejs.org/dist/v<version>/`. Packages and their complete dependency closure use the npm registry. Reject mirrors, floating tags and a fallback to an unrelated source.
3. Record each exact version, URL and byte size. Native and Node archives pin SHA-256. npm packages keep their lockfile SRI values. Record the source commit and the attestation identity where upstream publishes build attestations.
4. For a native asset with an attestation, verify the asset digest, expected repository, source commit and build identity. Record the verification method and evidence. A published attestation that can't be verified blocks admission or installation according to the recipe's declared check.
5. For Node, verify the signature on `SHASUMS256.txt` against the upstream release keys, then pin the portable archive's listed hash. Record the checksum document, signature and signer evidence. Mark a target unsupported when no official archive exists. This includes musl/Alpine.
6. For a Node package, review the pinned `package.json`, complete lockfile, resolved registry URLs, integrity values and npm options. Run the private runtime's bundled npm through that absolute Node path. Use `npm ci` without dependency resolution, lockfile rewriting or package install scripts.
7. Review every install, uninstall and index-cleanup argument vector, timeout, environment override, output path and process scope. Check that the tool's installer registers its MCP server. Review the required short Open Forge usage Skill separately. Review the exact original and replacement for every declared command rewrite. Keep selectors, entry names, markers and unrelated values unchanged. Check decline-only removal for optional components and drift reporting.
8. Verify disabled self-update checks and default-off telemetry. Test the visible telemetry opt-in and opt-out. Confirm that watching, automatic indexing and hooks follow the declared process policy.
9. Test each admitted target natively. Cover first install, same-pin reinstall, configuration, update, downgrade, cancellation, command failure, removal and another workspace sharing the same store. Use the declared process consent and final-client shutdown rules.
10. Check safe extraction, exact hashes, account-store containment, reference release, removal exclusions and the managed ignore section. Confirm that preview makes the same pre-effect decisions and starts no network requests or processes.
11. Add the exact manifest hash and adapter revision to release support only after review passes. Keep every previously reviewed pin in the catalogue so an exact older pin can be selected. Keep a blocked pin's reason when it can't be installed.

The [Node verification instructions](https://github.com/nodejs/node#verifying-binaries) define upstream checksum verification. The [npm ci reference](https://docs.npmjs.com/cli/v11/commands/npm-ci/) defines frozen lockfile installation. Those upstream procedures provide evidence for a recipe and don't grant Open Forge release admission.

### Reject a recipe

These conditions block installation:

- An unreviewed manifest tuple or unavailable adapter revision.
- A missing official target, source commit, required hash, size or complete dependency lock.
- A URL outside the recipe's approved source and redirect hosts.
- A size, digest, signature or required attestation mismatch.
- An archive with escaping paths, links, special members or duplicate destinations.
- Missing artifact consent, process consent or destination grants.
- A required output covered by a removal exclusion or another package's receipt.
- Unavailable store identity, references, process scope or required native tests.

Readable blocked definitions stay available to Inspect. Never replace failed verification with the user's Node, a PATH lookup, another asset, a mirror or a self-update.

### Keep evidence

Store the reviewed recipe and test evidence with the tool definition. The workspace settings record one pin per tool and its active roles. The workspace lock records the exact recipe, hashes, files, store references and command results. Account-local state records physical bindings, references and process consent.

## Verification

Use synthetic test-only definitions and local hashed artifacts for core tests. Test strict source selection, hashes, extraction, process arguments and environment, record publication, references, consent, preview parity and exclusions. Production recipes also need the matching native journeys and retained attestation or signature evidence from the checklist. CI supplies the full managed and Native AOT gates. Recipe review never redistributes the tested artifact.

## Not decided yet

- General private Node runtime preparation waits for the maintainer.
- Unix Node archive preparation needs a decision on exact unused launcher links. The current extraction rule rejects links.

## V1 scope

CodeGraph is the only V1 tool. Its recipe pins the official archive and bundled Node. Review local installation with `--yes --no-permissions`, both target hosts, telemetry off by default and explicit prompt-hook choice. Review command-only rewrites to `open-forge tool run codegraph`, with an absolute Open Forge executable when it is not on PATH. Keep the literal substring `codegraph prompt-hook` so upstream uninstall recognises its hook. Removal runs the declared local uninstall with `--keep-cli`, then forced root uninitialisation, before store release and ignore cleanup.

Codebase Memory research stays available for upstream work. Its current installer is account-wide, writes `~/.codex/AGENTS.md`, profiles and hooks, and has no hook opt-out. Missing controls are project-scoped install, project-scoped uninstall and a hook opt-out. Ship no installable definition, usage Skill or public tool page. The [scope decision](https://github.com/TheLithiumForge/open-forge/issues/23#issuecomment-6853898150) replaces earlier graph-installer decisions. Earlier [trial evidence](https://github.com/TheLithiumForge/open-forge/issues/23#issuecomment-6821695053) stays research rather than V1 admission.

Record CodeGraph upstream requests for a launch-command or absolute-launcher option and `--no-prompt-hook` for non-interactive installation. Do not contact upstream without a separate request. Review `first-party` or `third-party` origin and the third-party consent disclosure. The [dated decisions](../cli/technical-designs/external-tools.md#decisions-2026-11-13) cover Run limits, team restore, missing-tool reports and agent guidance. The final help-pointer experiment is withdrawn.

## Retained research

The earlier research below describes a blocked future profile. It gives no V1 installation permission.

Codebase Memory's future profile accepts only code root `.` and physical workspace CWD. Verify native project identity and index binding for both hosts. Use one exact build and resolved account cache for all active clients. Session consent covers the build, cache, process scope and final-client shutdown. Native settings turn automatic indexing, watching and UI off while keeping unrelated settings. Preflight ancestor permissions. A rejected dedicated cache requires a separate default-cache plan, never a silent switch or permission change.

After changed CBM sources, print an exact-project full-rebuild recipe. It deletes the selected native project, verifies that project's database absent, then indexes the exact workspace. It never deletes the account cache. CodeGraph uses external manual sync. Neither profile treats database presence as fresh completion evidence.

## CodeGraph recipe

The next branch reviews and qualifies these declared commands and entries:

- Install - `install --location local --target claude,codex --yes --no-permissions`, from the verified archive's bundled-runtime launcher. `--yes` skips upstream PATH setup.
- Claude MCP - rewrite only `command` and `args` in server `codegraph` from `codegraph serve --mcp` to `open-forge tool run codegraph serve --mcp`.
- Codex MCP - replace only `command` and `args` lines inside `[mcp_servers.codegraph]` as exact text. Stop on any unexpected shape.
- Prompt hook - rewrite the Claude `UserPromptSubmit` command to `open-forge tool run codegraph prompt-hook`. Keep the literal substring `codegraph prompt-hook` for upstream removal. Explain that structural prompts get context from the local index, nothing leaves the machine and `CODEGRAPH_NO_PROMPT_HOOK=1` disables it. Ask whether to keep it, default yes. Non-interactive new choices require `--prompt-hook yes|no`. On no, remove just this hook entry.
- Tool blocks - keep `<!-- CODEGRAPH_START -->` and `<!-- CODEGRAPH_END -->` in `CLAUDE.md` and `AGENTS.md`. Rewrite only their `codegraph explore` shell line through Tool Run. Subagents read these files, so MCP instructions alone don't replace this guidance.
- Open Forge launch - use its command name when available on PATH. Otherwise use the running executable's absolute path. Keep arguments separate in MCP records and quote command strings for their declared host syntax.
- Telemetry - run the tool's `telemetry off` unless the visible saved setting opts in. Disclose the command and choice in consent and Inspect.
- Removal - run `uninstall --location local --target claude,codex --yes --keep-cli`, then `uninit <root> --force`, through the same verified Tool Run path. Release final store references and the `.codegraph/` managed ignore entry afterwards.
- Skill - keep it about the size of the upstream block. Explain `codegraph_explore`, terminal exploration through Tool Run and the verified real index and refresh commands. Confirm call sites in source. C# recall was 41% in the trial. Keep installation internals and process policy here.

These are review requirements, not a production definition on the core branch. The [final graph decision](https://github.com/TheLithiumForge/open-forge/issues/23#issuecomment-6853898150) replaces independent host registration. The [Run decision](https://github.com/TheLithiumForge/open-forge/issues/23#issuecomment-6854341850) replaces the earlier dispatch name.
