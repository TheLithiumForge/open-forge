# Tool drafts

Tool drafts are reviewed release facts and proposed setup data that prepare external tools for Open Forge admission.

For example, `drafts/codegraph-1.6.2.json` records verified official assets while its Install arguments stay pending. The production catalogue is still empty. A draft wrapper isn't a valid `--source` input.

## Definition fields

The wrapper keeps these parts separate:

| Field                        | Meaning                                                                              |
| ---------------------------- | ------------------------------------------------------------------------------------ |
| `definition`                 | Production manifest field shape, blocked admission and no executable lifecycle       |
| `lifecycleDraft`             | Source-reviewed forms and null arguments for pending decisions                       |
| `releaseEvidence`            | Asset lengths, hashes, checksum URLs, signature identities and verification commands |
| `guidance`                   | Required Skill source and its exact SHA-256                                          |
| `consent`                    | Pinned-source write inventory and unresolved scope                                   |
| `passiveObservationProposal` | Native MCP names and proposed passive destinations                                   |

The draft schema references the existing manifest schema without changing it. All target qualifications are empty. Pending commands and execution timeouts stay null. The desired workspace host paths in `definition` aren't a claim that an account-wide installer supports them. The consent inventory records actual upstream locations.

## Release verification

Every selected archive was downloaded and checked on 2026-11-13. CodeGraph uses GitHub build attestations. Codebase Memory uses individual Sigstore bundles and a signed checksum file. The checksum fixtures are metadata from the upstream releases. No upstream program bytes are shipped here.

After changing a Skill, update its draft's guidance hash. Keep archive verification separate from native install qualification and language accuracy. Full admission steps and both worked examples are in [Tool maintenance](../../docs/tool-maintenance.md).
