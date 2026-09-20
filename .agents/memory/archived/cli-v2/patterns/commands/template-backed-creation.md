---
open-forge:
  description: "Historical CLI-v2 source: Instantiate an explicitly selected Template body into independently owned Markdown without transferring source metadata or authority"
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# Template Instantiation Boundary

## Boundary

The accepted [CLI Interface](../../../../memory/crystallized/documents/cli/interface.md)
owns consuming command syntax, Template-reference grammar, metadata inputs,
topology rules, fallback values, and results. This Pattern owns only the
reusable source, destination, instantiation, and ownership-transfer
arrangement.

## Shape

```text
resolved Template source -----+
                              |
resolved destination ---------+--> instantiation plan
                              |          |
destination metadata ---------+          v
                                     application
                                          |
                                     verification
                                          |
                              independently owned result
```

The complete request keeps source selection, destination identity, and
destination-owned metadata distinct. The consuming contract decides how each
value is selected and which topology must exist.

Instantiation reads the selected Template body without transferring source
frontmatter or lifecycle authority. It applies destination-owned metadata,
keeps removable source guidance visible, and preserves any protected boundary
owned by the consuming operation. The source remains unchanged.

The consuming operation includes creation and every required derived change in
one complete plan through the [Planned Mutation](../filesystem/planned-mutation.md) Pattern.
It verifies its own destination and semantic result. The Pattern does not
restate command-specific parent, fallback, marker, routing, or result rules.

After successful creation, the result is independently owned. Later Template
changes never mutate it. Provenance may be reported without recording a
managed lifecycle relationship.

## Review Checks

- Source, destination, and destination metadata remain distinct facts.
- Source frontmatter and authority do not transfer.
- Source guidance remains visible and removable.
- The source remains unchanged.
- Creation and required derived effects share one consuming plan.
- The destination becomes independently owned.
- Exact selection, topology, fallback, and result semantics remain in the consuming contract.
