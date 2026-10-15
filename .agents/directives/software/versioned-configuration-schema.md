---
open-forge:
  description: Keep visible configuration versioned, tolerant when read and backed by stable published schemas
  tags: [LoadNow, Directive, Software, Configuration, Schema, Versioning, Compatibility, JSON]
---

# Versioned Configuration Schema

## Instructions

- Apply these rules to configuration a person writes or can see, including generated files a person may open. Exclude internal wire formats whose components share one build.
- Declare `schemaVersion`. Person-authored configuration also declares `$schema`, subject to the private-repository publishing exception below. A generated file does not need `$schema` for authoring help it will never receive, though one costs little and makes it readable to the same tools.
- Put schema documents under one repository directory per version. Move the path version and `schemaVersion` together, so an older file keeps resolving its original schema after a newer version ships.
- Read an unrecognised `schemaVersion`, take the understood keys, report the version difference as a fact and continue. Read lower, higher and absent versions without losing the entries they carry. Accept and ignore unknown keys.
- Validate keys the program acts on and report a wrong shape with a cause naming the file and key rather than an internal type. Treat `$schema` as an ordinary key, preserve it on rewrite and never fetch it at runtime. Schema validation belongs to editors and CI, not the program reading its configuration.
- Treat a published `$schema` URL as a stable public identifier. While the repository is private, ship the schemas and either omit `$schema` until it is public or point to a workspace copy. Offer a command that writes the schema into the workspace for offline and private-repository validation. A relative `$schema` requires a guaranteed existing target.
- Once a version ships, its schema document stops changing. A new shape has a new version directory and `schemaVersion`.
- Prove lower, higher and absent version reading, unknown-key acceptance and ignoring, and actionable wrong-shape reporting directly. Validate documentation examples against the schema document.

The [Versioned Configuration Schema Pattern](../../patterns/software/versioned-configuration-schema.md) shows the files and reader outcomes. [Contract Ownership](contract-ownership.md) defines the wider source boundaries.
