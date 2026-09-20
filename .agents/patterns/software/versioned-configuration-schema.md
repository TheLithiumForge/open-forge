---
open-forge:
  description: Give every user-facing configuration file a declared schema version and a published schema document, and never let either refuse a file
  tags: [Pattern, Software, Configuration, Schema, Versioning, Compatibility, JSON]
---

# Versioned Configuration Schema

## Shape

A configuration file a person writes, or a generated file a person may open,
declares two things about its own shape:

```json
{
  "$schema": "https://raw.githubusercontent.com/<owner>/<repo>/main/schemas/v1/<file>.schema.json",
  "schemaVersion": 1,
  "…": "the actual settings"
}
```

`$schema` is for the editor. It buys completion, hover documentation, and inline
validation in every editor that already understands JSON Schema, which is the
cheapest documentation a configuration file can carry.

`schemaVersion` is for the reader. It is what lets a program say "this file was
written for a shape I do not know" instead of guessing.

Schema documents live in the repository under one directory per version:

```text
schemas/
  v1/
    <file>.schema.json
    <file>.lock.schema.json
```

The path version and `schemaVersion` move together, so a file that declares
version 1 keeps resolving the schema that described version 1 after version 2
ships. A single unversioned schema would describe the newest shape to every
file, and editors would then report correct older files as wrong.

## Reading Rules

These are what keep the version from becoming a gate:

- **An unrecognised `schemaVersion` is read, not refused.** Take the keys this
  release understands, report the version difference as a fact, and continue. A
  version number that can stop a command is worse than no version number at all.
- **An unknown key is accepted and ignored.** A key the reader has not heard of
  means a newer release wrote it. Refusing it makes a newer file unusable by an
  older program and buys no safety.
- **A key the program does act on is validated, and a wrong shape is reported.**
  Silently skipping a misspelled value is the one unhelpful outcome, because it
  is a mistake the author wants told.
- **`$schema` is an ordinary key.** It is preserved on a rewrite and never
  fetched at runtime. Validation against it belongs to the editor and to CI, not
  to the program reading its own configuration.

## Publishing

The `$schema` value is written into other people's files and outlives the
release that wrote it, so treat it as a stable public identifier.

- While the repository is private, a published URL resolves for nobody. Ship the
  schema documents, and either omit `$schema` until the repository is public or
  point it at a copy inside the workspace.
- Offer a command that writes the schema into the workspace, so the file
  validates offline and inside a private repository. A relative `$schema` is
  only honest once something guarantees the target exists.
- Once a version ships, its schema document stops changing. A new shape is a new
  version directory and a new `schemaVersion`.

## Evidence

Prove the reading rules directly, because they are the ones that decide whether
a future release can still open today's file:

- A file whose `schemaVersion` is lower, higher, and absent is read, and the
  entries it does carry survive.
- A file carrying an unknown key is read and the key is ignored.
- A key the program acts on, given the wrong shape, is reported with a cause
  that names the file and the key rather than an internal type.
- The schema document validates the examples in the documentation, so the
  schema and the prose cannot drift apart.

## Boundaries

This pattern covers configuration a person can see. It does not apply to an
internal wire format between two components of one program, where a shared build
already pins both sides and a version field would record nothing a reader can
act on.

A generated file still declares its version, because a person opens it while
diagnosing and a later release must recognise what wrote it. It does not need
`$schema` for authoring help it will never receive, though one costs little and
makes the file readable to the same tools.

Related: [Contract Ownership](contract-ownership.md) owns the wider question of
which document is the source of truth when a schema, a generated reference, and
prose all describe one contract.
