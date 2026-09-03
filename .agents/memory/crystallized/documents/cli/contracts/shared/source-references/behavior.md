---
open-forge:
  description: Accepted current technology-neutral resolution and conformance for shared CLI source references
  responsibility: Define how a conforming implementation classifies, resolves, disambiguates, and reports source references without selecting technology
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Source, Reference, Behavior, CurrentTruth]
---

# CLI Source References Behavior Contract

## Status And Boundary

This file is the accepted current Crystallized authority for technology-neutral
classification, resolution, invariants, safety, result formation, and
conformance behind the [Interface Contract](interface.md). The contract does not
ship yet; implementation and executable proof remain pending Gate 5. The
Interface Contract remains the complete public authority for source-reference
grammar, defaults and interpretation, observable identities and results, errors,
examples, and non-goals. This file links to those definitions rather than
maintaining a second detailed public contract.

The [Shared Result Coordinates](../result-coordinates/interface.md) define the
shared result envelope and process-status mapping. The [CLI
Architecture](../../../architecture.md) defines the BCL-first filesystem and
physical-identity, runtime, and evidence boundaries. This behavior remains
technology-neutral within those accepted choices.

## Reference Invariants

- A source-reference request is classified by the public prefix rule in
  [Accepted Forms](interface.md#accepted-forms). The resolver never guesses
  whether a value is an ID or an exact path.
- ID identity is derived for the current invocation from the selected workspace.
  It is not authority, routing, indexing, management, or a stored registry.
- Exact paths and IDs remain bounded by the public source kind, workspace, and
  route-meaning conditions in [Path Resolution](interface.md#path-resolution),
  [Entrypoints](interface.md#entrypoints), and [Errors](interface.md#errors).
- Resolution alone does not grant write, overwrite, delete, force, or ownership
  authority. The interactive collision boundary in
  [Interactive Use](interface.md#interactive-use) is part of this invariant.

## Reference Classification

At request resolution, a value beginning with `.agents/` or `./.agents/` is an
exact workspace-path request. Every other value is an ID request, including a
value that looks like a relative filesystem path. The resolver does not add a
second path grammar. A command-specific external-source operand remains outside
this shared classification and follows that command's own grammar, as stated in
[Accepted Forms](interface.md#accepted-forms) and [Command Use](interface.md#command-use).

## Input Normalization

Shell parsing removes supported surrounding quotes before the CLI receives the
operand. The resolver preserves the resulting ID or path value, including spaces
and Unicode, and does not implement another quote language after argument
parsing. Documented path input uses `/` on every operating system.

Backslash input, case compatibility, filesystem aliases, and exact physical
identity are realized within the CLI Architecture's BCL-first filesystem
boundary. The resolver does not add compatibility or alias semantics beyond the
public identity rules. See [Quoting Spaces And Special Characters](interface.md#quoting-spaces-and-special-characters)
and [Path Resolution](interface.md#path-resolution).

## ID Derivation

For each invocation, derivation enumerates supported files under the selected
workspace's `.agents` boundary and starts each identity from its canonical
workspace-relative path. It uses `/` between segments, removes `.md` from
Markdown files, applies recognized-entrypoint and `SKILL.md` folder identity,
keeps meaningful non-Markdown extensions, preserves exact case, spaces, and
Unicode, and rejects `.` and `..` segments.

These are the mechanics behind the complete public examples and derivation rules
in [Automatic Source IDs](interface.md#automatic-source-ids). The resolver does
not persist the result in frontmatter or a registry.

## Entrypoint Identity

Entrypoint identity is computed from the containing folder rather than the
recognized entrypoint filename. Compatibility entrypoint names receive the same
folder-based treatment when the Framework recognizes them. If a folder contains
more than one recognized entrypoint, exact path inspection may still identify a
file, but a command requiring valid route meaning blocks on the ambiguous route
relationship. It does not silently choose an entrypoint.

The identity and route boundary follows [Entrypoints](interface.md#entrypoints).

## Exact Path Resolution

For a path-form request, the resolver starts at the selected workspace, accepts
the two public `.agents` prefixes, reports the canonical `.agents/...` form, and
uses documented `/` input. It proves that the path remains lexically and
physically inside the selected workspace and that it identifies a source kind
supported by the selected command. It does not reinterpret the path as another
workspace or external source.

The resolver keeps the backslash, case, alias, and exact-physical-identity
realization within the CLI Architecture's filesystem boundary. It does not
silently add compatibility behavior for them. The public boundary is in [Path
Resolution](interface.md#path-resolution).

## ID Resolution

For an ID-form request, the resolver derives identities from the current selected
workspace and compares the complete input value exactly. It performs no case
correction, fuzzy or synonym matching, percent decoding, or semantic search.

It forms one of the public cardinality outcomes: one source continues, no source
forms invalid input with a useful next action, and several sources enter the
collision flow. Generated `Entries` may validate and order routed sources, but
they cannot become the only identity source or hide a supported file present in
the workspace graph. These mechanics conform to [ID Resolution](interface.md#id-resolution).

## Collision Handling

Collision handling retains every source candidate that derives the requested ID.
The resolver does not choose by file kind, generated order, modification time,
directory depth, or likely intent.

### Interactive Use

An interactive flow may present every candidate and collect one source-reference
choice. That choice completes only the reference-resolution step. It does not
acquire write, overwrite, delete, force, or ownership authority. The caller-visible
choice and example remain in [Interactive Use](interface.md#interactive-use).

### Non-Interactive And JSON Use

For non-interactive and JSON requests, an unresolved collision forms `blocked`
with every candidate path. The resolver neither prompts nor selects a default.
An exact path resolves the intended candidate through the normal path flow. No
new qualifier syntax is introduced while exact paths remain sufficient. See
[Non-Interactive And JSON Use](interface.md#non-interactive-and-json-use).

## Overwrite Resolution

When a base and adjacent overwrite companion form a valid pair, the resolver
uses one logical source identity and reads or inspects the base before the
overwrite. A selection by ID, base path, or overwrite path reaches that complete
logical source. The overwrite is not emitted as an independent source.

An orphan overwrite has no valid inherited route. The resolver may retain its
exact path as broken evidence, but it does not treat the orphan as a valid
standalone source. These invariants implement [Overwrite Companions](interface.md#overwrite-companions).

## Operand Resolution

Existing `.agents` content uses the shared ID-or-exact-path resolution unless a
command contract narrows the requirement. Creation may use either form only
when the selected command defines one unambiguous target shape; otherwise an ID
that could select several target shapes blocks and requests an exact path.
External Extension catalogues, formatter executables, workspace roots, and other
filesystem values remain purpose-specific operands rather than source references.

The shared resolver supplies identity and disambiguation only. It does not grant
the mutation or ownership authority of the selected command. See [Command Use](interface.md#command-use).

## Result Construction

Result formation emits both the automatic ID and canonical workspace-relative
path for a resolved source. Structured output exposes them as separate fields.
An ID collision remains visible as ambiguous while each source retains its exact
path. Content reached through an explicit contained local link has no automatic
ID, so human output uses `ID: none`, structured output uses a null ID, and the
canonical workspace-relative path remains required.

These facts form one typed result consumed by the command's presentation layer;
they do not create a second identity or rerun resolution. The public shapes are
in [Result Display](interface.md#result-display).

## Error Conformance

Error formation preserves the public distinction among invalid, blocked,
unsupported, ambiguous, escaped, and structurally ambiguous references. Each
error includes the original reference, its interpreted form, the cause, and a
useful next action. The resolver does not replace an unavailable identity with a
guess or turn an unresolved collision into a default.

The exact conditions remain those in [Errors](interface.md#errors). This Behavior
Contract adds no error kind or alternate recovery syntax.

## Verification Requirements

Gate 5 executable proof must cover:

- ID derivation for the Loader, entrypoints, ordinary Markdown, Skills, nested
  resources, spaces, Unicode, and non-Markdown resources.
- ID and path parsing without guessing.
- `.agents/...` and `./.agents/...` normalization.
- Exact-case ID matching.
- Missing, unsupported, unsafe, and escaped paths.
- ID collisions in interactive, non-interactive, and JSON use.
- Exact-path disambiguation.
- Base and overwrite selection from the ID, base path, and overwrite path.
- Orphan overwrite behavior.
- Results that show both ID and path.
- Repeat invocations producing the same identity result for unchanged input.

## Related Sources

- [CLI Source References Interface Contract](interface.md)
- [Shared CLI Contract Set](../_shared.md)
- [CLI Architecture](../../../architecture.md)
- [Global CLI Flags Behavior Contract](../global-flags/behavior.md)
- [Context Behavior Contract](../../context/behavior.md)
- [Historical CLI Decision Agenda](../../../../../../archived/cli-release/decision-agenda-2026-08-21.md)
- [Framework path rules](../../../../framework/routing/paths.md)
- [Framework overwrite rules](../../../../framework/routing/overwrites.md)
- [Shared CLI Operation Contract](../../../shared-operation-contract.md)
