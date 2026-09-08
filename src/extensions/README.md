# Open Forge Extensions

First-party Extension sources for the replacement Open Forge CLI live here.
The replacement CLI is not released; the frozen legacy executable retains its
historical package format and does not consume this catalogue layout.

An Extension is one optional package that is installed and managed as a unit. Its payload may contain any deliberate combination of routed files. Installed files keep the meaning of their destination routes and remain usable without the manifest, source package, receipt, or CLI.

## Current Catalogue

The pre-release catalogue contains one small package:

- `development-toolkit` adds six lean Workflows, one native Experience Design Skill, and nine copy-ready Templates

Install it only when the complete package is worth its context and maintenance cost:

```sh
open-forge extension list --available
open-forge extension install development-toolkit --dry-run
open-forge extension install development-toolkit
open-forge extension remove development-toolkit --dry-run
```

The current CLI does not install individual features from a package. Users own installed files and may remove routes that provide no local value after reviewing the result.

## Source Shape

The current package uses:

```text
development-toolkit/
  extension.json
  README.md
  content/
    .agents/
      skills/
      templates/
      workflows/
```

A normal managed package contains `extension.json`, an optional `README.md`, and complete target-relative files under `content/`. A package that provides only dependencies may omit `content/`. The replacement CLI accepts an exact package or catalogue, not an unmanifested overlay.

Every bundled package declares a stable lowercase id that does not depend on its folder name. A local package requires a valid manifest ID and uses separate lifecycle ownership. Consumer permission for external targets never supplies ownership.

The current manifest accepts:

- `id`
- `name`
- `description`
- `version`
- A duplicate-free `dependencies` array of bundled ids

Unknown fields are rejected. Dependencies resolve transitively and offline before their dependents. Installed files still express runtime relationships through ordinary links and route meaning because package metadata is not agent context.

## Payload Contract

Write payload files for their final workspace-relative locations. Open Forge files normally use #Extension with useful type and topic tags. Runtime-native files such as `SKILL.md` keep their native metadata. Add loading tags only when baseline or continuity loading is deliberate.

Shared behavior has one exact package source. Identical-file deduplication prevents collisions; it does not justify competing first-party copies.

The `development-toolkit` package defines its nine shipped Template files. This repository also keeps dogfood copies, which an automated parity check keeps identical.

## Planning, Ownership, And Removal

Before writing, the CLI validates selected manifests, source and target paths, receipts, dependencies, collisions, and route integrity. `--dry-run` prints the complete plan without changing files.

Normal writes require recognizable tracked Core files and a clean Git checkpoint for the target. `--pro` bypasses only the Git and Core checkpoint. It never bypasses manifest, containment, collision, ownership, or route-integrity checks.

The root `open-forge.extensions.json` receipt records managed packages, dependencies, requested packages, owned paths, shared owners, and payload digests. Reinstalling an id reconciles its files. Removal protects user changes, packages that are still needed, shared files, and reachable routed descendants.

Payload writes, generated `Entries`, and receipt changes roll back together after handled failures. The MVP has no registry, network resolution, compatibility solver, automatic orphan pruning, migration hooks, persistent recovery journal, or crash recovery.

## Catalogue Reset

Earlier first-party package ids were removed before the stable release and are not aliases. Existing installed files remain ordinary workspace content. Their receipts are enough to preview and remove them without keeping the old source catalogue.
