# CodeGraph draft

CodeGraph draft is the reviewed release and lifecycle data that prepares CodeGraph for Open Forge V1.

`drafts/codegraph-1.6.2.json` pins all six official archives and uses the shared definition types from the updated Tool core. The production catalogue stays empty until registration integration and native qualification are ready. A review wrapper isn't a valid `--source` input.

## Definition fields

The wrapper separates these parts:

| Field                           | Meaning                                                                                      |
| ------------------------------- | -------------------------------------------------------------------------------------------- |
| `definition`                    | Shared manifest shape, third-party origin, blocked admission and empty native qualification  |
| `definition.lifecycle`          | Install, uninstall, index cleanup, removal order and telemetry on and off commands           |
| `definition.registration`       | Four rewrite forms, optional prompt hook, conditional role line and role activation commands |
| `definition.targets.*.launcher` | Bundled Node and fixed script arguments, with no shell                                       |
| `releaseEvidence`               | Asset sizes, hashes, checksum URLs, signature identities and verification commands           |
| `guidance`                      | Required practical Skill and its exact SHA-256                                               |
| `consent`                       | Pinned-source write inventory                                                                |
| `trustNotes`                    | CodeGraph-specific decisions and missing upstream controls                                   |
| `review`                        | Native integration gates and unresolved shared hook selectors                                |

The review schema references the production manifest schema. Five rewrite records cover four forms because both native instruction files need a marked-line rewrite. The hook record describes the pristine Windows fixture. Its platform spelling and variable array position need Registration support before activation.

## Verification

All six CodeGraph archives were downloaded on 2026-11-13. Each length and SHA-256 matched the official release and published checksums. Each GitHub build attestation verified against its exact workflow identity. No upstream program bytes are redistributed here. After editing the Skill, update its guidance hash.

Codebase Memory is unsupported in V1. Its draft and research are preserved on `feature/tool-codebase-memory` at `7c76ca5fb`. This branch keeps CodeGraph only. The [maintenance checklist](../../docs/tool-maintenance.md) defines promotion and native journey gates.
