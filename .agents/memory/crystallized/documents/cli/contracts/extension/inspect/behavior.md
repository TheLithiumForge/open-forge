---
open-forge:
  description: Accepted technology-neutral behavior for read-only Extension identity inspection and source-unavailable comparisons
  responsibility: Define inspect's deterministic ID/source resolution, lifecycle trust, comparison facts, result formation, and non-mutation
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Extension, Inspect, Behavior, ReadOnly, Determinism, Lifecycle, CurrentTruth]
---

# extension inspect Behavior Contract

## Status And Boundary

This is the accepted current Crystallized Behavior Contract for read-only
`open-forge extension inspect`. It defines exact request resolution, installed
and source fact collection, one-source dependency closure, lifecycle trust,
semantic comparison, result formation, and conformance without selecting
implementation technology or mutation authority.

## Typed Flow

```text
validated stable ID and optional source
  -> exact workspace and source boundary
  -> installed lifecycle facts
  -> available manifest and dependency facts
  -> baseline/current/intended comparison when possible
  -> one typed read result
  -> human or JSON rendering
```

The operation never creates a mutation plan. Renderers consume the single typed
result and never rerun reads or comparisons.

## Request Resolution

1. Resolve terminal help/version before workspace or source work.
2. Require exactly one stable-ID operand and reject all other operands and
   mutation flags.
3. Resolve at most one exact `--source`; repeated singleton source input is
   invalid.
4. Select exactly CWD or exact `--workspace` without discovery.
5. Classify an explicit source as one structural package or catalogue and prove
   lexical and physical disjointness from the target workspace.
6. Resolve the requested ID exactly. No path, folder, version, proximity,
   fuzzy, registry, cache, network, or fallback rule supplies identity.

An explicit source remains the only source for that request. If it is unavailable
or malformed, retain independently readable installed facts but do not substitute
the embedded catalogue.

## Installed Lifecycle Facts

Read only the `extensions` section of `.agents/open-forge.lifecycle.json`, schema
v1, without merging it with Framework facts. Validate exact workspace binding,
stable package IDs, dependency edges and order facts, target-relative paths,
owner sets, semantic baseline fingerprints, and coverage/trust. Preserve the
common envelope and unrelated `framework` section as bytes and meaning if a
future mutation is considered; inspect itself never writes them.

An absent document or section is not, by itself, proof of unmanaged state.
Malformed, unsupported, unverifiable, or inconsistent data retains safe partial
facts as `untrusted` or `incomplete` and reports unsafe ambiguity as `blocked`.
Matching paths, bytes, fingerprints, or manifests never adopt an unowned file.

Installed-only facts remain available when source bytes are missing. Source
unavailability prevents an intended comparison or mutation plan but never erases
the installed ID, ownership, or readable baseline facts.

## Available Source And Closure

If source bytes are available, parse the exact requested manifest and validate
its stable ID. Resolve dependencies only within the one selected source universe,
transitively and offline. Reject unknown IDs, duplicate active IDs, duplicate
dependency declarations, invalid manifests, cycles, unsafe package paths, and
incomplete closure. A multi-package catalogue never selects another ID by
folder spelling or version.

Without explicit source, available facts use the embedded catalogue. A source
with one completely validated package may support deterministic manifest-ID
inference only where the caller has not supplied an ID; inspect has an explicit
ID, so it does not infer a different subject.

The CLI distribution embeds Framework and first-party Extension assets with
deterministic inventory and hash proof. That proof identifies distributed source
assets; it is not evidence of a selected workspace's current installation or of
a proven runtime implementation.

## Comparison And Fingerprints

When trusted installed facts and current source bytes both exist, form a
baseline/current/intended comparison for package paths, dependency closure,
shared owners, route effects, and lifecycle facts. Current exact bytes are
captured freshly for observations and bounded comparison; they are not a
persistent exact-byte baseline.

Supported parseable kinds use the `open-forge-markdown-v1` conservative
syntax-aware parser/AST semantic fingerprints that
preserve Unicode, semantic text, headings, tags, links, destinations, marker
meaning, inline and code-block content, and significant whitespace. Only
line endings and parser-proven formatting trivia normalize. Generated `Entries`
interiors are derived navigation, not authored package identity. Unsupported,
binary, or unparseable kinds use exact-byte identity and fail closed.

Formatting-only byte differences with equal current, baseline, and intended
semantic fingerprints are observations, not divergence. No formatter executes
and no formatter state is read or written.

## Result And Read-Only Safety

The typed result retains workspace/source, stable ID, installed and available
facts, trust and coverage, dependency order, baseline/current/intended comparison
when possible, source availability, generated-navigation ownership boundary,
findings, status, and one next action. Use the shared seven statuses and the
ordinary precedence for safety and coverage.

List and inspect can report facts without a healthy Framework, but they never
convert those facts into mutation trust. Inspect writes no lifecycle document,
payload, generated region, backup, temporary artifact, or package source
and invokes no public mutation command.

Primary human complete/attention/incomplete results use stdout. Primary human
invalid/blocked/failed/interrupted results use stderr. JSON emits one complete
typed result on stdout for every status; bounded diagnostics use stderr.

## Behavioral Conformance

Conformance must demonstrate exact ID/source resolution, package/catalogue
classification, source disjointness, installed-only/available-only/three-way
states, trusted/untrusted/absent/malformed coverage, source-unavailable facts,
offline dependency closure, semantic fingerprint and generated-region rules,
deterministic result ordering, all seven statuses and streams, one-result JSON,
no prompts, repeatability, and no mutation. The shared CLI Architecture defines
the exact JSON result schema and exit mapping. Gate 5 must prove source-generated
serialization, fixed Markdig where used, real `System.IO`, Native AOT, isolated
tests, and package journeys.
