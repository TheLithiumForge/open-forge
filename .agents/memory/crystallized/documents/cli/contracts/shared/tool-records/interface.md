---
open-forge:
  description: Define tool declarations, tested baselines and local state
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool record interface

Tool records are the four data sets that separate project intent from local installation facts.
A committed `codegraph` range can be shared across machines without recording a user's home directory.

Status: V1 design decided. Implementation waits for maintainer review of step 0.

## Settings

Settings stay at `.agents/open-forge.json`, including with a custom context root.
Settings v6 derives freshly from released v5.
All v5 fields and unknown fields keep their meanings.
The optional `tools` map has one entry per tool ID.
Each entry requires `version`, a supported major.minor range.
An omitted or empty map declares no tools.

Proposed:

```json
{
  "$schema": "https://raw.githubusercontent.com/TheLithiumForge/open-forge/main/schemas/v6/open-forge.schema.json",
  "schemaVersion": 6,
  "tools": {
    "codegraph": { "version": "1.6.x" },
    "codebase-memory": { "version": "0.12.x" }
  }
}
```

This declares compatibility. It doesn't install bytes.
Settings contain neither role assignments nor machine paths.

## Lock

The committed lock stays at `.agents/open-forge.lock.json`.
Lock v2 derives freshly from released v1.
Existing ownership and Library records keep their meanings.
Each optional `tools.<id>` entry records `version`, the exact tested version, and `profileDigest`, the SHA-256 of the applied config profile.
The digest covers the setup vector and effects, config changes, templates and guidance resource content in deterministic order.
It excludes machine paths, current file values and timestamps.
The digest encoding is a review proposal.

Proposed:

```text
tools.codegraph = {
  version: "1.6.2",
  profileDigest: "<64 lowercase SHA-256 hex characters>"
}
```

The exact version is a tested baseline, not a separate executable pin for every project.
A detected compatible patch can be reused and reported as distinct from the baseline.
For a missing installation, the proposed team-install rule selects the baseline when it is still available and reviewed.
Update resolves a new baseline and supported range.
A missing or inconsistent baseline requires a reviewed plan. It never becomes a silent latest-version selection.
Whether a newly resolved package patch becomes the tested baseline, or the lock keeps a catalogue-tested patch, is part of Q1.
The machine's installed version and the lock's tested baseline stay separate until that choice is made.

## Workspace state

Workspace edit history lives at `.open-forge-state/tools/<id>.json`.
This uses the existing fixed workspace state directory, including with a custom context root.
Its local `.gitignore` excludes the state from Git.
Keep schema 1 edit records with ID, profile digest, target scope, file and selector, prior presence and value, applied presence and value and verification state.
Keep the typed removal recipe for an Open Forge change so later catalogue changes don't erase its restoration rules.
JSON null is a present value. Absence is separate.
Text records keep the exact block or managed-file bytes, not a diff.
Native-command records keep the read-back prior and applied values and allow-listed restoration vector.
A value already desired before the first apply is borrowed. It gains no removal permission.
Recorded setup outputs distinguish upstream changes from Open Forge's added changes.

If local history is lost, committed settings and the lock can't reconstruct prior values.
Report the missing history and keep unrelated configuration.
Self-identifying managed blocks can be found without machine state, but discovery alone doesn't permit deleting changed content.

## Machine state

Machine facts live under `~/.open-forge/state/`, never in a repository file.
The proposed files are `tools/<id>.json`, `runtimes/node/<version>.json` and `path.json`.
They record channel, exact observed version, tool and runtime paths, generated launchers, integrity evidence and what PATH entries or shell blocks Open Forge added.
Runtime use comes from the launchers, not a cross-project reference registry.
PATH records include the exact chosen files and the prior and applied entry or block.
State schema 1 keeps prior absence separate from an empty value.
The machine filenames are a review proposal.

Before any shared deletion, check current launcher uses and ask for confirmation.
Lost or unreadable state cannot be treated as zero uses.
The [record Behavior](behavior.md) defines reapply, removal and publication.
