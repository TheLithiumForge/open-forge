---
open-forge:
  description: Implement route-list binding, source identity, exact-path selection, ID selection, and strict Loader destination resolution
  tags: [Memory, Working, CLI, Task, Route, List, Selection, Loader, Contextual, Complete]
---

# Implement Route-List Selection And Loader Resolution

## Task State

- State: Complete.
- Implementer: Mastermind.
- Parent: [Route Discovery](../_route-discovery.md).
- Predecessors: Foundation workspace/filesystem facts and route-list contracts.

## Expected Outcome

Typed parser values form one request. Source ID, exact path, and Loader-root
selection resolve deterministically while preserving attempted identity through
invalid, blocked, incomplete, and interrupted outcomes.

## Source Placement And Components

Create `List/Shared/Selection/` with cohesive files:

- `RouteListSelectionFactory`: the only construction path for attempted and
  resolved selection identities. Its private capability token prevents successful
  raw construction outside the factory boundary.
- `RouteListSelectionResolutionFactory`: the only construction path for resolved,
  invalid, blocked, incomplete, and interrupted selection outcomes.
- `RouteListSourceReferenceParser`: validates source-reference grammar without
  filesystem access.
- `RouteListSelectionResolver`: coordinates ID, exact path, and default root
  selection using shared source and workspace facts.
- `RouteListSourceCatalogue`: command-local view over shared source facts until
  route inspect proves promotion.

Create `List/Shared/Loader/`:

- `LoaderDestinationParser`: validates percent triplets, decodes once, and
  revalidates every decoded character and path segment.
- `LoaderEntriesParser`: validates the exact final generated region and canonical
  one-line Loader declarations without becoming a general Markdown parser.
- `LoaderDestinationResolver`: proves canonical `.agents` destination,
  containment, recognized root entrypoint, and inventory identity.
- `LoaderDestinationEntryResolver`: applies physical and catalogue policy to one
  already parsed declaration while the parent resolver retains aggregate state.

## Required Rules

- IDs reject leading, trailing, empty, dot, separator, NUL, and control-containing
  segments without normalization.
- Exact paths preserve the attempted canonical logical path even when missing or
  unsafe.
- Ambiguous IDs retain every deterministic candidate and block unless the
  contract permits interactive selection.
- Loader parsing rejects malformed percent forms, decoded controls,
  query/fragment delimiters, rooted or URI-like values, colon and backslash
  hazards, empty or dot segments, and any target outside canonical Loader scope.
- Decode exactly once. Encoded percent data never receives a second
  interpretation.
- All selection and resolution construction goes through the appropriate factory.
  A production-source scan finds `RouteListSelection` construction only in
  `RouteListSelectionFactory` and `RouteListSelectionResolution` construction only
  in `RouteListSelectionResolutionFactory`.

## Evidence

Table-driven Unit grammar tests cover every segment and encoding class.
Integration uses real workspaces for ID/path parity, missing, ambiguity, exact
detached entrypoint, Loader roots, unsafe decoded destinations, physical aliases,
and attempted-identity retention. Later Acceptance process evidence covers public
syntax and parser precedence after complete command composition.

### Accepted Implementation Evidence

- Pure table-driven Unit evidence covers source-reference classification, every
  invalid ID/path segment class, automatic ID derivation, immutable catalogue and
  factory states, strict percent decoding, canonical tags and labels, exact final
  Loader regions, malformed declarations, and one-decode behavior.
- Real-workspace Integration covers exact ID/path parity, attempted identity,
  unknown and ambiguous sources, detached entrypoints, base/overwrite parity,
  orphan overwrites, empty and encoded Loader roots, partial safe roots, mixed
  findings, malformed and unsafe destinations, route ambiguity, contained aliases,
  external aliases, first-external-transition blocking, and unchanged snapshots.
- The completed selection increment passed with 152 Unit cases and 59 Integration
  cases; it added 106 Unit and 39 Integration cases beyond the accepted Foundation
  and route-list contract baselines.
- `dotnet format` verification and the complete Release solution build pass with
  zero warnings and errors. The production-source construction scan also passes.
- Public process and Native AOT proof remain deliberately deferred to route-list
  Presentation and Acceptance, where the complete operation, renderers, help, and
  explicit root composition can be registered together. The command-free root is
  not exposed through an incomplete selection-only operation.

### Review Record

- A bounded correctness review found mixed-status aggregation, forged source IDs,
  arbitrary overwrite aliases, lost partial roots, lax Loader labels/tags, and
  mismatched factory identity. Each issue received focused regression evidence and
  a bounded correction.
- An adversarial path review additionally found forgeable entrypoint kinds and a
  nested-label declaration form. Source-shape invariants and strict compact-link
  validation now fail closed for both.
- A local improvement review recommended central status rules and cohesive splits
  for Loader grammar, Loader entry resolution, and selection outcome construction.
  The resulting same-folder components preserve behavior without promotion or a
  general parser. Final correctness and local-improvement reviews pass.
- The strongest alternative was to trust a future inventory as the sole source of
  valid catalogue facts and leave larger state machines intact. Constructor-level
  invariants, direct callable evidence, and narrower files cost modest local code
  but prevent false complete Loader roots and policy drift before that consumer
  exists. Reconsider only if a later second consumer proves identical semantics.

## Protected Boundaries

- No general URI parser, Markdown link parser, route topology enumeration,
  renderer, status policy, or mutation.
- No source ID normalization, slugification, lowercase conversion, or guessed
  identity.

## Stop Conditions

Stop if selection requires a second global catalogue, parser raw-token rescan, or
shared identity promotion without a second consumer.
