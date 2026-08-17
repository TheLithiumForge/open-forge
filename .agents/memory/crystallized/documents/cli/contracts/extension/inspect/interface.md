---
open-forge:
  description: Accepted read-only Interface for inspecting one Extension ID across installed, available, and three-way facts
  responsibility: Define inspect's stable-ID syntax, exact source handling, comparison result, statuses, errors, and conformance
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Extension, Inspect, Interface, ReadOnly, Source, Lifecycle, CurrentTruth]
---

# extension inspect Interface Contract

## Status And Authority

This is the accepted current Crystallized Interface Contract for
`open-forge extension inspect`. It owns the exact syntax, stable-ID subject,
source selection, installed and available projections, dependency facts,
source-unavailable behavior, output, statuses, errors, examples, non-goals, and
public conformance. The command does not ship yet.

The sibling [Behavior Contract](behavior.md) defines technology-neutral
resolution and result formation. The [Extension group entrypoint](../_extension.md)
defines group help only. Global Flags defines shared flags.

## Purpose And Boundary

`inspect` gives package-specific read-only detail before an install, update, or
remove decision. It can report installed-only, available-only, or a
baseline/current/intended comparison when both trusted installed facts and
current source bytes are available.

It does not install, update, remove, create, repair, adopt, index, or turn a
recommendation into authority. It may report installed facts without a healthy
current Framework, but that report does not grant mutation trust.

## Syntax

```text
open-forge extension inspect <stable-id> [--source <package-or-catalogue-path>] [global flags]
```

Exactly one stable-ID operand is required. `--source` is one exact external
package or catalogue read location. There is no `--all`, `--automatic`,
`--dry-run`, `--force`, `--prune`, wizard, or mutation flag.

Shared flags are `--workspace <path>`, `--json`, `--view=compact|expanded`,
`--verbose`, `--help`, and `--version`. Their grammar, defaults, repetition,
terminal behavior, and no-op rules are owned by [Global CLI Flags](../../shared/global-flags/interface.md).

## Subject And Source

The stable ID is an exact package identity. It is not a folder name, path
spelling, version selector, fuzzy query, or semantic recommendation. A missing,
malformed, duplicated, or conflicting manifest ID never becomes an inferred ID.

Without `--source`, inspect reads installed facts from the exact workspace and
the embedded available package facts. With `--source`, it reads only that exact
package or catalogue and proves the source is lexically and physically disjoint
from the target workspace. It does not use network, registry, cache, ambient
search, glob, resemblance, or embedded fallback when explicit source input is
unavailable.

The selected source may contain the requested package or a catalogue containing
it. Dependency closure is resolved only within that one source universe. A
multi-package source does not select a different package by proximity.

## Installed And Available Projections

The result may be one of these explicit views:

- **Installed-only:** the ID has readable managed facts, but current
  package source bytes are unavailable.
- **Available-only:** the source has the ID, but no managed installation exists.
- **Baseline/current/intended:** trusted installed facts and current source bytes
  both exist, so the result can compare declared paths, dependencies, owners,
  semantic fingerprints, generated effects, and divergence.
- **Unavailable or ambiguous:** safe partial facts remain visible, but the
  required source, lifecycle, identity, or route facts are incomplete or blocked.

Installed facts survive source unavailability. Inspect does not claim an update,
restore, or exact managed no-op without intended current source bytes.

## Lifecycle Trust And Fingerprints

Installed lifecycle facts retain `trusted`, `untrusted`, `incomplete`, `blocked`,
or `absent` state. The only lifecycle input is the `extensions` section of
`.agents/open-forge.lifecycle.json`, schema v1, in its common envelope. A path,
equal bytes, equal fingerprint, or package ID does not establish ownership. An
absent document or section is not, by itself, proof of unmanaged state; complete
inspection is required before reporting safe absence.

For supported parseable kinds, comparison uses the `open-forge-markdown-v1`
conservative parser/AST-derived syntax-aware
semantic fingerprints that preserve Unicode, semantic text, headings, tags,
links, destinations, code blocks, marker meaning, and significant whitespace.
Only line endings and parser-proven formatting trivia may normalize. Generated
`Entries` interiors are derived navigation, not authored package identity.
Unsupported, binary, and unparseable kinds use exact-byte identity and fail
closed. Exact current bytes are operation-time facts; no persistent exact-byte
baseline digest is implied.

## Output

Expanded output reports exact workspace and source, ID, installed and available
state, trust, source availability, package and dependency closure, declared and
current path facts, baseline/current/intended comparison when possible, generated
navigation as derived, status, and at most one next action. Compact output keeps
ID, source, key states and counts, comparison status, and safety facts. JSON
emits one complete structured result from the same typed result for every status.

Illustrative output:

```text
Open Forge extension inspect development-toolkit
Source: embedded catalogue; available
Installed: yes; trust: trusted
Packages: 1 root; 0 dependencies
Footprint: 21 declared paths; 9 current matches; 11 changed; 1 missing
Comparison: baseline / current / intended
Generated: derived navigation, not package-owned authored bytes
Status: requires attention
Next: open-forge extension update development-toolkit
```

The values are illustrative and are not current inventory or implementation
evidence.

Primary human complete/attention/incomplete results go to stdout. Primary human
invalid/blocked/failed/interrupted results go to stderr. Bounded diagnostics use
stderr; JSON uses one result on stdout for every status.

## Semantic Results And Errors

| Result        | Meaning for `inspect`                                                                                                                                           |
| ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | Requested ID and all applicable installed/source facts are completely readable, including a valid empty comparison or no-op observation.                        |
| `attention`   | Complete facts expose finite divergence, source-unavailable comparison, or another non-blocking lifecycle observation.                                          |
| `incomplete`  | Safe installed or source facts remain, but required current source, lifecycle, dependency, parser, or coverage facts are unavailable.                           |
| `invalid`     | The ID, source input, operands, flag, repetition, or terminal-mode request is invalid.                                                                          |
| `blocked`     | Ambiguous identity, source overlap, unsafe containment, ownership collision, malformed lifecycle evidence, or another unsafe boundary prevents safe inspection. |
| `failed`      | An unexpected read or result-formation failure occurs.                                                                                                          |
| `interrupted` | The caller interrupts before the read-only result completes.                                                                                                    |

Every error names `extension inspect`, the ID or source when known, the cause,
and at most one useful next action. A missing source supplied explicitly is not
silently replaced by embedded data.

## Examples

Inspect the embedded package and any installed facts:

```text
open-forge extension inspect development-toolkit
```

Inspect one exact local catalogue package:

```text
open-forge extension inspect development-toolkit --source D:/packages/open-forge
```

Inspect installed facts without a source and preserve source-unavailable state:

```text
open-forge extension inspect development-toolkit --json
```

## Non-Goals And Public Conformance

Inspect does not mutate lifecycle state, package sources, workspace files,
generated navigation, or ownership. It does not choose a package from multiple
IDs, resolve a semver update, infer trust, reconstruct state from files outside
the exact lifecycle document, or invoke another public command.

Conformance must cover exact ID and source semantics, installed-only,
available-only, and three-way projections, trusted/untrusted/absent states,
source-unavailable installed facts, dependency closure, semantic fingerprints,
generated-navigation boundary, deterministic output, seven statuses, streams,
JSON parity, no prompts, and no writes. The shared CLI Architecture defines the
exact JSON result schema and exit mapping. Gate 5 must prove source-generated
serialization, fixed Markdig where used, real `System.IO`, Native AOT, isolated
tests, and package journeys.
