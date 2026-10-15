---
open-forge:
  description: Give every user-facing configuration file a declared schema version and a published schema document, and never let either refuse a file
  tags: [Pattern, Software, Configuration, Schema, Versioning, Compatibility, JSON]
---

# Versioned Configuration Schema

## Shape

Illustrative person-authored JSON. The URL and setting are placeholders in a schematic file. Its actual settings are omitted. The key names are fixed.

```json
{
  "$schema": "https://raw.githubusercontent.com/example/project/main/schemas/v1/settings.schema.json",
  "schemaVersion": 1,
  "enabled": true
}
```

```text
schemas/                           versioned repository location, name illustrative
  v1/                              repeatable version directory
    settings.schema.json           schema for visible configuration
    settings.lock.schema.json      schema for a corresponding lock file, if present
```

| Part                                    | Status                                                                                                            |
| --------------------------------------- | ----------------------------------------------------------------------------------------------------------------- |
| `schemaVersion`                         | Required even for generated visible files                                                                         |
| `$schema`                               | Required for person-authored configuration, optional for generated files, subject to private-repository exception |
| Path version matching `schemaVersion`   | Required, previous version remains fixed                                                                          |
| File basenames, setting and version `1` | Illustrative                                                                                                      |

| Reader input                    | Expected result                                                        |
| ------------------------------- | ---------------------------------------------------------------------- |
| Unknown version                 | Understood keys read, version difference reported, operation continues |
| Lower, higher or absent version | Entries carried by the file survive                                    |
| Unknown key                     | Accepted and ignored                                                   |
| Acted-on key with wrong shape   | Cause names file and key                                               |
| `$schema` on rewrite            | Preserved, no runtime fetch                                            |

## Applicability And Sources

For configuration a person can see. Internal wire formats pinned by one shared build are outside this shape. `$schema` supports editor completion, hover text and validation. `schemaVersion` identifies the file's shape to its reader.

- [Versioned Configuration Schema Directive](../../directives/software/versioned-configuration-schema.md) binds reading, publication, offline availability and evidence.
- [Contract Ownership](contract-ownership.md) shows which source defines each contract fact.
