---
open-forge:
  description: Historical CLI-v2 source: Shared inventory, parsing, containment, traversal, fragment, diagnosis, and repair semantics for local Markdown references
  responsibility: Define how read-only CLI consumers inspect local references once without changing Framework loading meaning
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Local Reference Contract

## Scope

This contract owns the replacement CLI's interpretation of ordinary local
Markdown references. The Framework path and routed Markdown contracts remain
authoritative for authored syntax and semantic route entries. Production
TypeScript will own exact declarations and diagnostic codes.

## Guarantees

### One Invocation Inventory

An invocation builds one in-memory reference inventory and shares it between
its consumers. The inventory records:

- Canonical workspace-relative source and target paths.
- Source position, visible label, reference kind, destination, and fragment.
- Parsed headings and their canonical fragments.
- Resolution and containment evidence.
- A named status and sufficient evidence for a useful message.

The initial implementation has no persistent cache. A later cache remains
derived and replaceable and must prove invalidation before it can affect
results.

The inventory reads sources and resolves unique targets with named bounded
concurrency. Repeated edges share target evidence. It never performs one
serial physical-resolution chain per reference when the same target can be
resolved once.

### Document Inspection Boundary

Scan each source once into a typed document-fact result containing supported
frontmatter, headings, references, generated regions, source positions, and
parse findings. Consumers depend on those facts rather than scanner tokens or
a particular parser representation.

The initial focused scanner:

- Recognizes canonical inline links and images without using a regular
  expression over the complete document.
- Ignores frontmatter, inline and fenced code, and HTML comments.
- Preserves source line and column.
- Reads visible ATX heading text while preserving Unicode.
- Assigns GitHub-compatible fragments in document order, including duplicate
  suffixes, through one focused replaceable fragment boundary.
- Treats percent encoding as destination syntax, not a different target
  identity.
- Keeps exact generated-region markers available to their dedicated consumer.

Setext headings and reference-style links remain noncanonical Markdown outside
the initial semantic boundary. They do not become semantic sections, route entries, or silently
validated fragment targets. When an unsupported construct affects a requested
check, the result is explicitly incomplete or unverified rather than guessed.

Unicode heading text is supported. A complex inline heading construct becomes
unverified only when the scanner cannot establish its visible text safely.

HTML is never parsed, rendered, or executed. Exact Open Forge HTML comments
are structural delimiters; other comment contents are opaque ignored text.

The scanner may later be replaced by a syntax-tree implementation without
changing the document-fact or consumer contracts.

### Destination Classes

| Destination                                                     | Result                                             |
| --------------------------------------------------------------- | -------------------------------------------------- |
| Contained local file or directory                               | Resolve relative to the containing source          |
| Local Markdown fragment                                         | Validate against the target's canonical headings   |
| Fragment on another local resource                              | Report as unverified, not falsely valid            |
| HTTP or HTTPS                                                   | `external-unchecked`; never fetch or declare valid |
| Other URI scheme                                                | External and unchecked                             |
| Absolute path, query, invalid encoding, or malformed local form | Invalid                                            |
| Local target outside the workspace, including a physical escape | Outside workspace                                  |

Ordinary local links may leave `.agents` while remaining inside the selected
workspace. Semantic route entries remain stricter: they identify contained
files and omit query strings and fragments.

### Traversal Identity

Canonical physical real paths own read-only traversal identity. A document is
parsed once per canonical physical path, so repeated edges and ordinary cycles
terminate. A cycle is not a finding by itself.

Exact device and file identifiers may enrich alias diagnosis only after the
runtime and filesystem pass an identity-precision capability probe. An
unproven or colliding identifier never merges documents. Hard-linked paths may
therefore be visited separately when exact alias identity is unavailable; the
finite discovered path set still bounds traversal.

### Consumer Boundaries

| Consumer        | Seeds                                                          | Reference behavior                                                                |
| --------------- | -------------------------------------------------------------- | --------------------------------------------------------------------------------- |
| `context`       | Baseline, continuity, explicit selected chains, and overwrites | Diagnose outgoing references from emitted sources; do not load their bodies       |
| `route inspect` | Explicit selected route sources                                | Return direct outgoing reference facts and findings                               |
| `doctor`        | Complete Framework, routed, lifecycle, and recovery inventory  | Follow contained local Markdown references through the complete reachable closure |
| `repair`        | The complete doctor inventory                                  | Plan only uniquely safe local-reference corrections                               |

`find` queries routed metadata. Ordinary links never expand its search boundary.

Doctor's complete reachable closure begins only from its exact known Open
Forge seeds. Reference traversal may reach a contained workspace file outside
`.agents`, but the command never enumerates unrelated workspace files to find
additional starting points.

An explicit `context` route adds its selected route, applicable parent chain,
and overwrite companion. An ordinary link expresses a visible relationship;
it does not recreate Required Routes or another hidden preload graph.

### Status Semantics

Named statuses distinguish at least:

- Valid.
- Missing target.
- Missing fragment.
- Fragment unverified.
- Aliased target.
- Outside workspace.
- Invalid destination.
- External unchecked.

Human output states the source, line, destination, condition, and next action.
Structured output uses the same facts and stable diagnostic code. External
unchecked references are inventory facts rather than errors.

### Repair Boundary

Bare `doctor` remains read-only. `repair` may change a reference only when the
existing authored meaning is proven:

- Canonical path spelling, encoding, or case resolves to the same existing
  physical target.
- A fragment has one exact canonical spelling for the same target heading.

A missing destination, fuzzy basename, multiple candidate, moved authored
concept, external URL, or uncertain fragment remains manual even when a
suggestion is useful. Repair uses the ordinary mutation contract and
revalidates the source bytes and target evidence before application.

### Bounded Completion

Traversal keeps named production limits for source count, reference count,
source bytes, individual document bytes, and elapsed work. Exceeding a limit
returns an explicit incomplete or blocked result with the reached boundary.
It never silently truncates diagnosis or context.

Limits are implementation-owned named constants informed by packaged-artifact
measurements. This semantic contract does not duplicate their numeric values.

### Example

```text
.agents/workflows/release.md:18
  ../patterns/release.md#verification
  valid -> .agents/patterns/release.md#verification

.agents/workflows/release.md:24
  ../patterns/missing.md
  missing target
  Next: restore the intended file or update the authored link

.agents/workflows/release.md:31
  https://example.com/release
  external, not checked
```

## Boundaries

The inventory interprets supported local Markdown references only. It never
fetches external destinations, renders or executes HTML, promotes ordinary
links into loading behavior, scans unrelated workspace files for new seeds,
guesses unsupported syntax, or turns an ambiguous suggestion into a repair.
Incomplete evidence remains explicit.

## Verification

Direct scanner tests prove the supported frontmatter, inline-link, image,
code, comment, ATX-heading, Unicode, duplicate-fragment, generated-marker, and
source-position behavior. Unsupported Setext headings, reference-style links,
and unsafe inline constructs must produce the documented incomplete or
unverified facts rather than guessed results.

Real-workspace integration tests prove containing-file-relative resolution,
workspace and physical containment, repeated-edge reuse, cycle termination,
alias handling under available identity evidence, external classification,
and every named invalid or incomplete state. Bounded-work tests prove that
reaching any resource limit remains explicit.

Consumer tests prove that `context`, `route inspect`, `doctor`, and `repair`
use the same inventory while retaining their distinct seed, traversal, and
mutation boundaries. Repair tests prove that only exact meaning-preserving
corrections become planned effects. Built-process tests cover representative
human and structured findings without making presentation snapshots the source
of semantic coverage.

## Related Current Sources

- [Framework path contract](../../framework/routing/paths.md)
- [Routed Markdown contract](../../framework/markdown/routes.md)
- [Route inventory](route-inventory.md)
- [Diagnosis and repair](diagnosis-and-repair.md)
- [Mutation execution](mutation-execution.md)
