---
open-forge:
  description: Keep canonical root instructions and harness bridges safely replaceable inside one workspace-owned file
  tags: [Pattern, Framework, Entry, Bridge, Installation, Safety]
---

# Managed Root Entry

## Shape

Canonical source, with fixed heading and footer and illustrative body:

```md
# Open Forge

managed instructions or imports

**End of Open Forge managed section.**
```

| Part                             | Canonical source                    | Workspace target                                                                                         |
| -------------------------------- | ----------------------------------- | -------------------------------------------------------------------------------------------------------- |
| Authored prefix                  | Absent                              | Optional, preserved byte-for-byte                                                                        |
| Managed span                     | Required, exactly one ordered block | Zero candidates before append, or exactly one complete canonical or exact legacy pair before replacement |
| Authored suffix                  | Absent                              | Optional, preserved byte-for-byte                                                                        |
| Reinstallation result            | Same canonical bytes                | Same result, no change                                                                                   |
| Ambiguous or incomplete boundary | Invalid                             | Invalid, no partial output                                                                               |

```text
canonical Framework entry               required policy source
  <- harness-native reference           bridge content only
     harness bridge
```

## Applicability And Sources

For canonical root instructions and harness bridges in a workspace-owned file. Delimiters are root-level parsed Markdown blocks. Nested examples are content. Names and order in the canonical block are fixed. Prefix, suffix and body content vary by file.

- [Deliberate Framework Change](../../directives/open-forge/framework/deliberate-framework-change.md#managed-root-entries) binds validation, preservation, entry roles and reinstallation.
- [Payload Maintenance](../../memory/crystallized/documents/maintenance/payload/_payload.md) maps the file-specific source, implementation, external syntax and verification contracts.
