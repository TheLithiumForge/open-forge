---
open-forge:
  description: Historical frozen MVP Extension package, ownership, lifecycle, safety, recorded verification, and limitations
  tags: [Memory, Archived, Contextual, Historical, Extension, MVP, Architecture, Lifecycle]
---

# Frozen MVP Extension Architecture

## Origin And Status

This record preserves implementation-specific material extracted from `.agents/memory/crystallized/documents/extensions/architecture.md` during the Task 28 document split. That source combined frozen MVP mechanics with later current material. The [current Extensions Architecture](../crystallized/documents/extensions/architecture.md) replaces it as the coherent source for Extension concepts and composition.

The extracted source had SHA-256 `5e1c30a1d5b7f71e84d9a914830241c78e8241300a26f4a5791c47020e5bcd86`. This record condenses its historical account. Later native-layout annotations and catalogue additions were excluded.

All behavior and limitations below belong to the frozen MVP account. They do not define current Framework or CLI behavior. The verification summaries are recorded historical evidence, not checks rerun for this extraction. The [CLI MVP Architecture](../crystallized/documents/cli/mvp-architecture.md) links to the historical implementation and its test sources.

## Package Sources And Manifest

A normal managed MVP package used this layout:

```text
{package-folder}/
  extension.json
  README.md
  payload/
    ... target-relative complete files ...
```

A dependency-only pack could omit `payload/`. Local sources could use the complete package shape, a plain `payload/` directory, or a direct overlay whose contents mapped directly into the target workspace.

The manifest could declare:

| Field | Recorded meaning |
| --- | --- |
| `id` | Stable lowercase managed identity |
| `name` | Display name |
| `description` | Catalogue selection text |
| `version` | Descriptive package version |
| `dependencies` | Required bundled Extension identities |

Every bundled first-party package declared an identity. A local source could declare one to opt into managed lifecycle; a source without an identity remained unmanaged. Unknown manifest fields were rejected.

Identity was independent of source location and content type. Versions were descriptive and did not provide compatibility solving, version ranges, or migration semantics.

## Dependencies And Assembly

The resolver used stable Extension identities and resolved bundled dependencies offline. It traversed dependencies transitively, installed dependencies before their dependents, rejected unknown identities and cycles, and deduplicated the resolved set.

Local packages could depend on bundled identities. The resolver did not support arbitrary local-to-local graphs, remote packages, registries, or capability providers. Dependency-only packs selected a set of packages without installing placeholder runtime files.

Payloads contained complete files at their intended workspace-relative paths. The CLI constructed a plan, checked source and target safety, assembled workspace routes, maintained generated navigation, and recorded managed ownership when applicable.

## Ownership Record

Installations with a stable identity used `open-forge.extensions.json` at the workspace root.

The receipt recorded:

- Explicitly requested packages.
- Installed dependencies.
- Descriptive versions.
- Owned payload paths.
- Content digests.
- Shared owner sets.

This let the CLI distinguish reconcilable files, files modified after installation, paths shared by managed packages, existing unowned files, and dependencies still required by a retained package.

Generated `Entries` bodies were excluded from authored ownership identity because the CLI could rebuild them around an Extension-owned `entrypoint`.

Unmanaged overlays and externally installed Skills remained outside the receipt. Routing them did not transfer ownership. External installation used distinct paths; two managers could not claim the same installed path. An externally installed Skill did not automatically satisfy an Extension dependency identity.

## Update And Removal

Reinstalling a stable identity acted as managed reconciliation. The CLI verified recorded bytes, planned new payload effects, updated owned files, and removed dropped files only when their recorded contents still matched and no owner remained.

Removal:

- Acted only on explicitly requested identities.
- Refused to break retained dependents.
- Preserved modified owned files by blocking.
- Preserved shared files while another owner remained.
- Refused to remove an `entrypoint` that would strand retained routed descendants.
- Did not automatically prune orphaned dependencies.

The source also recorded that receipts supported explicit preview and removal of retired package identities without retaining their old source packages. Installed files remained ordinary workspace content, and retired identities were not aliases.

## Safety And Application

The recorded preflight considered:

- Portable cross-platform path identity.
- Source and target topology.
- Lexical and physical containment.
- Symlinks, junctions, and hard links.
- File and parent-directory collisions.
- Existing unowned paths.
- Shared managed ownership.
- Git visibility.
- Generated-index side effects.
- Route reachability after removal.

Payloads could not claim `.git/`, `.gitignore`, the ownership receipt, or workspace-owned `.overwrite.md` files.

Different bytes targeting one portable path were a conflict. Identical managed bytes could share owners only through explicit compatible plans. Byte equality alone did not permit adoption of an existing file.

The CLI provided Git review checkpoints and rollback after handled failures. Its recovery limits are recorded below.

## Recorded Verification

The former document reported these kinds of verification:

| Evidence | Recorded coverage |
| --- | --- |
| Pure tests | Dependency selection, manifest validation, path identity, ownership receipts, and collision rules |
| Lifecycle scenarios | Real subprocesses, filesystems, and Git repositories for installation, update, removal, rollback, and review checkpoints |
| MVP catalogue tests | The then-single first-party package |
| Packaged-layout tests | Discovery from built and npm-style package layouts |
| Framework validation | Assembled routes and links after installation |

These summaries describe the recorded checks and their scope. They are not fresh results for the current catalogue or CLI. Deterministic packaging and lifecycle checks also did not prove that every package improved agent outcomes.

## Recorded Limitations

| Area | Frozen MVP limitation |
| --- | --- |
| Implementation separation | Manifest parsing, catalogue discovery, dependencies, ownership, and lifecycle behavior lived in the CLI monolith without an independently expressed Extension domain implementation. |
| Sources and distribution | Bundled packages and local sources had no source-provider boundary, registry protocol, remote trust model, provenance verification, or reproducible third-party fetch contract. |
| Compatibility | Descriptive versions did not negotiate compatibility among packages, the Framework, the CLI, or agent runtimes. |
| Migration | Packages could not declare migrations, compatibility transitions, or required user decisions. The account identified this as insufficient for long-lived third-party packages. |
| Ownership across workspaces | One root receipt assumed one Open Forge lifecycle authority. Multiple repositories, nested scopes, submodules, and several managers lacked a complete ownership model. |
| Dependency expressiveness | Exact bundled identities did not express compatible alternatives, provided capabilities, optional dependencies, conflicts, or external satisfaction. |
| Catalogue governance | There was no accepted stability, deprecation, support, or quality policy. Continued distribution still needed evidence of reusable value. |
| Scoped installation | Payloads could target deep routes, but scope selection, missing scope `entrypoints`, and ownership across repositories or submodules lacked a complete user-facing design. |
| Recovery | Handled failures rolled back. Abrupt process or machine failure relied on Git; there was no persistent recovery journal or workspace mutation lock. |

These were limitations of the recorded implementation, not claims about current behavior or requirements to build a larger package manager. The [Extensions Evolution candidate](../emerging/ideas/extensions-overhaul.md) keeps possible future responses open for evaluation.

## Related Historical Context

- [Historical extension skill-sharing exploration](ideas/extension-skill-sharing.md).
- [Archived CLI-v2 designs](cli-v2/_cli-v2.md), retained as historical input.
