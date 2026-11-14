---
open-forge:
  description: Define strict tool manifests, exact catalogue selection and separate settings and lock records
  tags: [Memory, Document, CLI, Contract, Shared, Tool, Manifest, Interface, CurrentTruth]
---

# Tool manifest interface
A tool manifest is a versioned data definition that names exact artifacts and selects reviewed compiled adapter behavior.

A catalogue selects supported definitions and their defaults. Workspace settings record installed pins and active roles. The lock records what Open Forge wrote and verified. These three records answer different questions.

Status: V1 core contract decided.

## Manifest

JSON and YAML decode to the same strict type. Reject duplicate keys, unknown fields, YAML aliases, tags and object construction. Arrays keep their declared order and contain distinct values. The schema states which fields are required or nullable.

The [manifest schema](../../../../../../../../schemas/tools/v1/manifest.schema.json) defines the exact fields. The typed models in Framework Tools implement that authoring contract. Every field is required unless the schema allows omission.

Tool, role and artifact IDs use the group ID grammar. `none` can't be a tool ID. Versions are full SemVer 2.0 without a `v` prefix. Commits contain 40 lowercase hex characters. Every manifest lists exactly `win-x64`, `win-arm64`, `osx-x64`, `osx-arm64`, `linux-x64` and `linux-arm64`. Unsupported and blocked targets need reasons. No cross-architecture fallback occurs.

## Tokens

Adapter and template tokens have separate finite vocabularies:

| Type | Values |
| --- | --- |
| AdapterId | `codegraph-1.6`, `cbm-0.12`, reserved `qmd-2.8` |
| TemplateId | `codegraph-manual-v1`, `cbm-manual-v1`, reserved `qmd-inventory-v1` |
| SkillId | `code-graph-usage-v1`, reserved `docs-search-usage-v1` |
| CapabilityId | `code-graph`, reserved `docs-search`, reserved `code-to-docs-links` |
| RoleId | `code-navigation`, reserved `docs-search`, reserved `code-to-docs-links` |
| ActionId | `render-members`, `native-settings`, `render-helper` |
| CheckId | `artifact-hash`, `installed-files`, `native-settings`, `host-members`, `index-state`, `hook-policy`, `process-policy`, `cache-permissions`, `dependency-lock`, `source-membership`, `usage-skill` |

Unknown tokens invalidate a definition. Reserved tokens can describe deferred definitions but can't activate in V1. A manifest can't widen its reviewed command or output scope. Tool installers handle their helpers and agent files.

## Values

Each artifact can declare its exact HTTPS `url`. When omitted, join `downloadBase` and `file`. The per-artifact URL lets one recipe name both official Node and npm registry artifacts. `file` stays a contained relative name in either form. Reject absolute file names, traversal, credentials and query overrides. Artifact references resolve inside the same manifest. Relative paths use forward slashes without dot or parent segments. Only blocked definitions can omit byte sizes or their source commit. Native archives use SHA-256. npm tarballs keep their lockfile SRI values. The verified tool entry can run its declared lifecycle commands after preparation.

## Supply recipes

Open Forge distributes reviewed recipes and never ships, mirrors or rehosts tool bytes. Native assets come from the project's own GitHub release. Pin version, official URL, size, SHA-256, source commit and the attestation identity where published. Check published attestation evidence against that exact digest and source identity before admitting the recipe. A recipe requiring install-time verification blocks if that check is unavailable or fails.

Node tools use a private pinned official portable archive from `https://nodejs.org/dist/v<version>/`. Its pinned hash comes from the signed `SHASUMS256.txt`. Keep checksum, signature and signer evidence with the reviewed recipe. No official musl/Alpine build exists, so that target is unsupported. Node and its bundled npm stay in the versioned user store. Commands use absolute paths and never select the user's Node.

Locked-package targets require a complete pinned runtime and dependency closure. The definition pins the package manifest, lockfile, exact npm registry URLs and integrity values. The bundled npm runs `npm ci` with the reviewed options and package scripts disabled. It resolves no new dependency and changes no lockfile. Required artifacts come from the official Node source or npm registry. Another host, incomplete closure, hash mismatch or required signature or attestation failure blocks execution.

The [admission checklist](../../../../maintenance/tools.md) defines recipe review and license checks. The upstream license and notices still apply to direct downloads.

## Launch values

Scalars are strings, Booleans, finite numbers or null. Template variables are `${install}`, `${entry}`, `${worktree}`, `${codeRoot}`, `${state}`, `${cache}`, `${hookSink}`, `${projectId}`, `${indexDirName}`, `${host}` and `${target}` only. Render typed values without a shell. Install and entry paths become absolute. Each argument stays separate. Index directory names contain one validated segment.

Merge process and MCP environment maps. Equal duplicate values are allowed. Unequal duplicates block. Declared values override inheritance, including empty strings. The command is the verified absolute target entry. CWD is the selected physical workspace. Both hosts need evidence for the resulting launch environment.

Candidate admission means native testing is pending. Enabled admission needs a source commit, an available target with an entry, required verification and the target in `qualification.targets`.

V1 graph definitions require a non-null Skill with `optional:false` and `defaultEnabled:true`. Pending native testing doesn't prevent inspecting a release-supported candidate. Unreviewed tuples and unavailable adapter revisions keep the shared blocked outcome.

## Lifecycle commands

A definition declares `install`, `uninstall` and `indexCleanup` commands. Each has an argument array, a `workspace` or `host` scope and a bounded timeout. The executable is always the verified target entry. Repeated argument values are allowed because each position has its own meaning. Output paths and managed ignore paths are declared data and stay inside the workspace. A leading `${codeRoot}/` resolves from the selected contained code root. Other path templates are rejected. Tool-owned outputs are used for permission and exclusion checks, not as permission for Open Forge to edit them.

The [shared Tool rules](../../../shared-operation-contract.md#tool-commands) define when each command runs and how its result is recorded. A missing lifecycle declaration permits inspection but blocks writes. Production support still requires the exact reviewed manifest hash and adapter revision.

## Catalogue

Omitted `--source` selects the embedded catalogue only. A supplied source selects one exact manifest or catalogue file. Don't scan a directory, merge sources or fall back after failure. A single manifest gives its sole version and default pin.

The [catalogue schema](../../../../../../../../schemas/tools/v1/catalogue.schema.json) defines exact catalogue fields and version entries.

Require distinct tool IDs, distinct versions within each tool and distinct manifest paths. Version lists are nonempty. The default names exactly one listed version. Resolve paths from the catalogue's parent without links, absolute paths or escapes. Verify exact manifest bytes against SHA-256 before decoding. Manifest ID and version match the catalogue entry. Reject duplicate keys and unknown fields at every level.

Select `--tool-version` by exact string when present. Otherwise select the source's declared default. Sort displayed IDs and versions ordinally. Array order never selects a pin.

The release allow-list contains exact `(id, version, manifest SHA-256, adapter revision)` tuples. A local source can choose a supported subset and default. Another pin or program needs an Open Forge release. The catalogue keeps every previously reviewed pin. Update can select an exact older reviewed pin for a downgrade, subject to its current install checks. This core ships no concrete definition or production adapter. Synthetic fixture definitions and local hashed artifacts remain test-only and can't widen production admission.

## Workspace settings

The fixed `.agents/open-forge.json` adds a direct `tools` map. An omitted map means no recorded tools. Each tool requires `version` and `roles`. Each workspace pins one version per tool. The latest lifecycle action and exit summary records what ran. Detailed invocations belong in the lock. A separate `toolRuns` map keeps the latest action summary for failed first installs and completed removals without claiming an installed tool. Optional hosts default to both supported hosts. Optional code root defaults to `.`. The visible per-tool telemetry setting defaults off and lets you opt in or out. Inspect reports the effective choice. Declared launches disable tool self-update checks.

The [settings v6 schema](../../../../../../../../schemas/v6/open-forge.schema.json) defines the exact tool-ID map and entry fields.

An installed inactive tool has `roles:[]`. At most one tool lists a given role. `code-navigation` requires `code-graph`. Reserved roles can't activate in V1. Reject unknown fields inside tool entries, duplicate keys, duplicate array values and incompatible role assignments. Keep unrelated settings keys under the shared tolerant settings contract. CBM permits only `.` in V1.

Hand edits don't install bytes or approve downloads. Tool-owned commands handle host entries. Install adds a tool after verification. Update changes its pin. Configure changes recorded hosts and root. Choose moves or clears a role. Remove deletes the entry after verified removal.

## Lock records

The fixed `.agents/open-forge.lock.json` keeps generated tool receipts separately from settings. The [lock v2 schema](../../../../../../../../schemas/v2/open-forge.lock.schema.json) defines its exact authoring shape. Receipts record artifact and installed-file hashes, exact manifest digest and snapshot, adapter revision, generated file paths, declared lifecycle invocations and their results, managed ignore paths, runtime-directory coverage and native project mappings. Tool-owned host files have no generated-member receipts. A separate `toolRuns` map keeps detailed execution evidence independently of installed registrations. Existing receipt kinds keep their meanings.

The lock records no backup data. Without Git, exact prior bytes stay in sibling `.bak` files until new content validates. Git workspaces create no new backups. Account-local records keep physical store bindings, shared-install references and local process consent outside Git. They supplement the workspace lock and don't replace settings entries.

Without Git, settings and lock changes keep their sibling backups until both records validate. Missing or mismatched detailed records mean incomplete configuration. They never establish a verified install or permit a silent role switch. Removal uses stored definitions and receipts without requiring the original catalogue. These records add no document fingerprints or workspace integrity gate.

## Outcomes

Source outcomes are:

| Condition | Status and exit |
| --- | --- |
| Unknown ID, absent exact version, invalid shape or default, duplicate identity, digest or manifest identity mismatch | `invalid-input`, 4 |
| Well-formed unreviewed tuple, unavailable adapter revision or unqualified activation target | `blocked`, 5 |
| Unreadable required source bytes | `incomplete`, 3 |

Read commands keep independently readable installed facts. A readable definition's activation requirements remain reportable even when activation is blocked.

The [behavior](behavior.md) defines validation order and conformance.
