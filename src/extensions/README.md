# Open Forge Extensions

First-party Open Forge Extension sources live here.

An Extension is one optional installation and ownership unit. Its payload may contain any deliberate combination of routed files. Installed files retain the meaning of their destination routes and remain usable without the manifest, source package, receipt, or CLI.

## Current Catalogue

The pre-release catalogue currently advertises one deliberately small package:

- `development-toolkit` adds six lean Workflows, one native Experience Design Skill, and nine copy-ready Templates

Install only when that complete unit earns its context and maintenance cost:

```sh
open-forge extend --list
open-forge extend development-toolkit --dry-run
open-forge extend development-toolkit
open-forge extend --remove development-toolkit --dry-run
```

The current CLI does not select individual features inside a package. Users own installed files and may remove routes that provide no local value after reviewing the resulting route tree.

## Source Shape

The current package uses:

```text
development-toolkit/
  extension.json
  README.md
  payload/
    .agents/
      skills/
      templates/
      workflows/
```

A normal managed package contains an `extension.json`, an optional `README.md`, and complete target-relative files beneath `payload/`. A dependency-only package may omit `payload/` when it declares at least one dependency. A local source may also be a plain payload directory or direct overlay.

Every bundled package declares a stable lowercase id independently of its folder. A local source with a manifest id opts into receipt-managed lifecycle. An idless local payload remains unmanaged.

The current manifest accepts:

- `id`
- `name`
- `description`
- `version`
- A duplicate-free `dependencies` array of bundled ids

Unknown fields are rejected. Dependencies resolve transitively and offline before their dependents. Installed files still express every runtime relationship through ordinary links and route meaning because package metadata is not agent context.

## Payload Contract

Payload files are authored for their final workspace-relative locations. Open Forge-authored files normally use #Extension with their useful type and topic tags. Runtime-native files such as `SKILL.md` keep their native metadata. Loading tags appear only when baseline or continuity loading is deliberate.

Shared behavior has one canonical package source. Identical-file deduplication is a safety boundary, not permission to maintain competing first-party copies.

The `development-toolkit` package is canonical for its nine shipped Template leaves. Because this repository installs the Extension for dogfooding, its corresponding Template leaves remain exactly aligned through an automated parity check.

## Planning, Ownership, And Removal

The CLI validates every selected manifest, source path, target path, existing receipt, dependency, collision, and route-integrity boundary before writing. `--dry-run` reports the complete plan without mutation.

Normal writes require recognizable tracked Core anchors and a clean target-scoped Git checkpoint. `--pro` bypasses only the Git and Core checkpoint lifecycle. It never bypasses manifest, containment, collision, ownership, or route-integrity checks.

Managed packages are recorded in the transparent root `open-forge.extensions.json` receipt. It records dependency edges, requested roots, owned paths, owner sets, and payload digests. Reinstalling an id reconciles its owned files. Removal protects user changes, retained dependents, shared ownership, and reachable routed descendants.

Payload writes, generated `Entries`, and receipt changes roll back together on handled in-process failures. The MVP has no registry, network resolution, compatibility solver, automatic orphan pruning, migration hooks, persistent recovery journal, or crash recovery.

## Catalogue Reset

Earlier first-party package ids were removed before a stable release and are not aliases. Existing installed files remain ordinary workspace content. Their receipts remain sufficient for explicit preview and removal without retaining the old source catalogue.
