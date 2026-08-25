---
open-forge:
  description: Implement route-list human, JSON, diagnostic, and binding-derived help projections
  tags: [Memory, Working, CLI, Task, Route, List, Rendering, Help, Diagnostics, Contextual, Complete]
---

# Implement Route-List Presentation And Help

## Task State

- State: Complete.
- Implementer: Mastermind.
- Parent: [Route Discovery](../_route-discovery.md).

## Expected Outcome

One immutable route-list result renders through compact human, expanded human,
JSON, optional verbose diagnostics, and standard symbol-derived help without
rerunning selection, inventory, topology, or status logic.

## Source Placement

Create `List/Shared/Rendering/`:

- `RouteListHumanRenderer` coordinates sections only.
- `RouteListCompactRenderer` and `RouteListExpandedRenderer` own their stable view
  projections when their independent responsibilities justify separate files.
- `RouteListJsonRenderer` uses a concrete generated context.
- `RouteListDiagnosticRenderer` emits bounded escaped facts with no source body or
  secret content.
- `RouteListHelpSections` supplies examples, delimiter wording, related commands,
  notes, and availability through the binding.
- `RouteListTextEscaping` is local projection support and does not own diagnostics
  policy outside this command.

## Required Human Parity

Render workspace path and selectedBy; selected roots or explicit source IDs and
paths; requested and effective depth with explanation; coverage boundary and
evidence; row ID, path, provenance, parent ID/path, applicable child count; finding
code, message, and path; status; and required next action.

Compact remains dense but preserves selected-root identity and finding location.
Expanded remains complete. Ordinary failures are understandable without verbose.

## JSON, Diagnostics, And Help

- JSON is one schema-version-1 concrete document and ignores human view without
  omitting data.
- Verbose diagnostics go only to stderr, are bounded, and change no primary output
  or process facts.
- Standard usage, arguments, options, defaults, and child commands come from the
  exact `System.CommandLine` graph.
- Product sections are ordered and local. Unimplemented related commands are
  labelled unavailable rather than added as executable symbols.
- Do not post-process standard help through string replacement or duplicate option
  spellings in generic Shell code.

## Evidence

Unit fixed-result tests cover every result state, row shape, view, JSON envelope,
escaping, redaction, bounds, and help section. EndToEnd proves root/group/leaf
help, examples, delimiter wording, human snapshots focused on stable contract
text, JSON-plus-verbose stream isolation, and fixed exits.

## Stop Conditions

Stop if presentation needs domain recomputation, if help requires a second command
catalogue, or if diagnostic rendering can fail in a way that changes primary
completion.

## Completion Evidence

- Presentation, operation, and composition implementation is complete and
  verified, but uncommitted. It adds route-list request binding, typed invalid
  depth/workspace results, one inventory/selection/topology/result operation
  coordinator, compact and expanded human output, a dedicated source-generated
  JSON DTO projection, bounded verbose diagnostics, route/group/list help, root
  composition, and managed plus published-process evidence.
- JSON follows the contract with a top-level envelope and result fields
  `selection`, `requestedDepth`, `effectiveDepth`, `coverage`, `findings`, and
  `rows` in the fixed version-1 shape.
  Finite depths are numbers; `all` is a string.
- Verification is clean: warning-free Release solution build; 259 Unit; 84
  Integration; 7 EndToEnd; managed and published `win-x64` root; native
  `win-x64` Integration 84; native `win-x64` EndToEnd 7; format verify clean;
  and `git diff --check` clean.
- Integration and EndToEnd workspace hashes prove that the exercised route-list
  scenarios do not mutate workspace bytes.
- The later repository-routing compatibility correction temporarily renamed the
  Working Index and References Tasks to `index-command.md` and
  `references-command.md`. Their command contracts remain staged under
  `index-candidate/` and `references-candidate/` until the replacement `index`
  command validates the final compatibility-name paths. This later correction
  does not change route-list behavior or evidence.
