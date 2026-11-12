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

JSON and YAML decode to the same strict type. Reject duplicate keys, unknown fields, YAML aliases, tags and object construction. Every field below is required. Arrays are ordered and distinct. `map<T>` has string keys. A union with null permits null.

Proposed:

```text
ToolManifestV1 {
  schema: 1, id: ToolId, name: string, version: ExactVersion, license: string,
  source: {repository: HttpsUrl, tag: string, commit: GitSha | null},
  adapter: {id: AdapterId, revision: positiveInteger},
  admission: "candidate" | "enabled" | "blocked", admissionReason: string | null,
  capabilities: CapabilityId[], downloadBase: HttpsUrl,
  artifacts: map<Artifact>, targets: map<Target>,
  runtime: {kind: "native" | "bundled" | "locked-package", version: string | null,
    notes: string, extraDownloads: ArtifactId[], installScripts: "forbidden"},
  configuration: {template: TemplateId, codeRoot: "workspace-relative" | "none",
    documents: "none" | "open-forge-source-inventory", settings: map<Scalar>,
    files: ConfigFile[], actions: ActionId[]},
  processes: {telemetry: "off" | "absent" | "unverified",
    updateChecks: "off" | "absent" | "unverified",
    daemon: "off" | "session-required" | "unverified", watcher: "off",
    autoIndex: "off", hooks: "blocked", consent: "none" | "session-daemon",
    coordination: "worktree" | "account-build-cache", environment: map<string>},
  index: {location: "project-directory" | "native-account-cache" | "external",
    path: TemplateString, perWorktree: true,
    sharedContentCache: "none" | "native-content-keyed",
    freshness: "manual-sync" | "full-rebuild-after-delete" | "manual-update",
    rebuildOnVersionChange: boolean},
  mcp: {transport: "stdio", command: "target-entry", args: TemplateString[],
    cwd: "worktree", environment: map<string>, hosts: HostTemplates},
  skill: {template: SkillId, optional: boolean, defaultEnabled: boolean} | null,
  checks: CheckId[],
  qualification: {attestation: "release-evidence-required" | "required-at-install" | "unavailable",
    workflow: string | null, targets: TargetId[],
    accuracy: "three-languages-2026-11-12" | "pending"}
}
Artifact {
  file: RelativeUrlPath, bytes: positiveInteger | null,
  integrity: "sha256:" + 64LowerHex | "sha512-" + Base64,
  archive: "zip" | "tar.gz" | "tgz" | "file", stripComponents: nonnegativeInteger
}
Target {
  state: "available" | "unsupported" | "blocked", artifacts: ArtifactId[],
  entry: RelativeInstallPath | null,
  lock: {artifacts: ArtifactId[], closureComplete: true} | null, reason: string | null
}
ConfigFile {
  path: TemplateString,
  format: "json-members" | "native-settings" | "native-collections" | "adapter-helper",
  members: string[]
}
HostTemplates {"claude-code": HostTemplate, "codex": HostTemplate}
HostTemplate {path: RelativeWorkspacePath, selector: string}
```

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

Unknown tokens invalidate a definition. Reserved tokens can describe deferred definitions but can't activate in V1. A manifest can't widen template destinations or enable arbitrary helpers. `render-helper` writes only a reviewed compiled helper template.

## Values

Join `downloadBase` and `file` for the exact HTTPS URL. Reject absolute file names, traversal, credentials and query overrides. Artifact references resolve inside the same manifest. Relative paths use forward slashes without dot or parent segments. Only blocked definitions can omit byte sizes or their source commit. Graph archives use SHA-256. npm tarballs can use SHA-512 SRI. Locked-package targets require a complete runtime and dependency artifact closure. Package resolution and install scripts don't run during installation.

Scalars are strings, Booleans, finite numbers or null. Template variables are `${install}`, `${entry}`, `${worktree}`, `${codeRoot}`, `${state}`, `${cache}`, `${hookSink}`, `${projectId}` and `${indexDirName}` only. Render typed values without a shell. Install and entry paths become absolute. Each argument stays separate. Index directory names contain one validated segment.

Merge process and MCP environment maps. Equal duplicate values are allowed. Unequal duplicates block. Declared values override inheritance, including empty strings. The command is the verified absolute target entry. CWD is the selected physical workspace. Both hosts need evidence for the resulting launch environment.

Candidate admission means native testing is pending. Enabled admission needs a source commit, an available target with an entry, required verification and the target in `qualification.targets`.

V1 graph definitions require a non-null Skill with `optional:false` and `defaultEnabled:true`. Pending native testing doesn't prevent inspecting a release-supported candidate. Unreviewed tuples and unavailable adapter revisions keep the shared blocked outcome.

## Catalogue

Omitted `--source` selects the embedded catalogue only. A supplied source selects one exact manifest or catalogue file. Don't scan a directory, merge sources or fall back after failure. A single manifest gives its sole version and default pin.

Proposed:

```text
ToolCatalogueV1 {
  schema: 1,
  tools: [{id: ToolId, defaultVersion: ExactVersion,
    versions: [{version: ExactVersion, path: RelativeManifestPath, sha256: 64LowerHex}]}]
}
```

Require distinct tool IDs, distinct versions within each tool and distinct manifest paths. Version lists are nonempty. The default names exactly one listed version. Resolve paths from the catalogue's parent without links, absolute paths or escapes. Verify exact manifest bytes against SHA-256 before decoding. Manifest ID and version match the catalogue entry. Reject duplicate keys and unknown fields at every level.

Select `--tool-version` by exact string when present. Otherwise select the source's declared default. Sort displayed IDs and versions ordinally. Array order never selects a pin.

The release allow-list contains exact `(id, version, manifest SHA-256, adapter revision)` tuples. A local source can choose a supported subset and default. Another pin or program needs an Open Forge release. This core ships no concrete definition or production adapter. Synthetic fixture definitions and local hashed artifacts remain test-only and can't widen production admission.

## Workspace settings

The fixed `.agents/open-forge.json` adds a direct `tools` map. An omitted map means no recorded tools. Each tool requires `version` and `roles`. Optional hosts default to both supported hosts. Optional code root defaults to `.`.

Proposed:

```text
tools?: map<WorkspaceToolSetting>
WorkspaceToolSetting {
  version: ExactVersion, roles: RoleId[],
  hosts?: ("claude-code" | "codex")[], codeRoot?: RelativeWorkspacePath
}
```

An installed inactive tool has `roles:[]`. At most one tool lists a given role. `code-navigation` requires `code-graph`. Reserved roles can't activate in V1. Reject unknown fields inside tool entries, duplicate keys, duplicate array values and incompatible role assignments. Keep unrelated settings keys under the shared tolerant settings contract. CBM permits only `.` in V1.

Hand edits don't install bytes, approve downloads or update host entries. Install adds a tool after verification. Update changes its pin. Configure changes recorded hosts and root. Choose moves or clears a role. Remove deletes the entry after verified removal.

## Lock records

The fixed `.agents/open-forge.lock.json` keeps generated tool receipts separately from settings. Receipts record artifact and installed-file hashes, exact manifest digest and snapshot, adapter revision, generated file paths, host member selectors and last generated semantic values, runtime-directory coverage, native project mappings and recovery records. Existing receipt kinds keep their meanings.

Recovery records identify bundle identity, location, digest and state. Exact prior-byte payloads stay in recovery v2 bundles. Account-local records keep physical store bindings, shared-install references and local process consent outside Git. They supplement the workspace lock and don't replace settings entries.

Settings and lock changes share operation recovery coverage. Missing or mismatched detailed records mean incomplete configuration. They never establish a verified install or permit a silent role switch. Removal uses stored definitions and receipts without requiring the original catalogue. These records add no document fingerprints or workspace integrity gate.

## Outcomes

Source outcomes are:

| Condition | Status and exit |
| --- | --- |
| Unknown ID, absent exact version, invalid shape or default, duplicate identity, digest or manifest identity mismatch | `invalid-input`, 4 |
| Well-formed unreviewed tuple, unavailable adapter revision or unqualified activation target | `blocked`, 5 |
| Unreadable required source bytes | `incomplete`, 3 |

Read commands keep independently readable installed facts. A readable definition's activation requirements remain reportable even when activation is blocked.

The [behavior](behavior.md) defines validation order and conformance.
