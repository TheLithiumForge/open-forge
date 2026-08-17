---
open-forge:
  description: Historical CLI-v2 source: Static and bounded dynamic shell-completion candidates, sources, workspace resolution, safety, cost limits, and fallback behavior
  responsibility: Define frequent hidden completion requests without turning suggestions into authority, mutation, unbounded discovery, or another public command
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Completion Protocol Contract

## Scope

Generated shell scripts call one hidden completion protocol endpoint. The
endpoint returns suggestions for the incomplete invocation. It is not a public
command, domain operation, or independently rendered CLI result. It remains
absent from help and from its own suggestions.

This contract owns candidate kinds, provider boundaries, workspace resolution,
cost limits, safety, ordering, and failure behavior. The [Completion lifecycle
contract](completion-lifecycle.md) owns script generation, installation,
removal, profile mutation, and recovery.

A suggestion helps complete input. It never selects a subject, confirms an
operation, authorizes a write, or weakens normal validation and preflight.

## Guarantees

### Candidate Kinds

Static completion comes from Commander metadata and readonly domain values. It
covers public commands and groups, canonical flags and aliases, and finite
choice values such as supported shells and lifecycle states.

Dynamic completion is accepted only for these finite local identities:

| Candidate                     | Invocation positions                                                                          | Source                                                                |
| ----------------------------- | --------------------------------------------------------------------------------------------- | --------------------------------------------------------------------- |
| Route identity                | `context`, `find`, `route list`, and `route inspect` route arguments                          | Bounded authored route inventory                                      |
| Template identity             | `create --template` and `route init --template`                                               | Bounded routed Template inventory                                     |
| Persisted Framework exclusion | `install --restore`                                                                           | Exact identities in the bounded `framework.excluded` workspace record |
| Extension id                  | `extension inspect`, `extension add`, `extension update`, and `extension remove` id arguments | Selected bounded catalogue and installed lifecycle inventory          |

Extension providers respect operation eligibility. Add suggests available
uninstalled ids, update suggests installed ids available from the selected
catalogue, and remove suggests installed ids. An external `--path` provider
reads only its bounded package or catalogue metadata. It never scans payload
bodies merely to produce suggestions.

The Framework-exclusion provider suggests exact stored identities only. It
does not suggest unstored descendants covered by an excluded entrypoint, scan
the complete Framework payload, or infer exclusions from missing files.

General path completion remains shell-owned. This includes Open Forge content
paths, custom Template paths beginning with `./`, external Extension `--path`
values, and custom Completion profiles. Open Forge does not duplicate a shell's
filesystem completion or reinterpret a completed path as another input kind.

Tags, arbitrary frontmatter values, full-text findings, and other unbounded
workspace facts are not completion candidates.

### Workspace Resolution

The hidden endpoint resolves the exact `--workspace` value already present in
the incomplete invocation. When it is absent, it uses the exact current working
directory. It performs no upward discovery, nested-root guessing, or fallback
to another workspace.

Public Completion lifecycle commands remain workspace-independent and reject
`--workspace`. The hidden protocol is a parser integration used by every shell
invocation, so it may inspect the selected workspace only to complete a
workspace-aware argument.

### Provider Boundary

The adapter selects at most the one provider relevant to the active argument
or flag. Providers are direct typed functions, not names resolved through a
string registry. Each provider receives normalized parser context and returns
ordinary typed candidates without writing streams.

Protocol-significant candidate kinds, provider outcomes, limit outcomes, and
failure states use named enums or readonly `as const` objects in production
source. Candidate and provider relationships remain visible through direct
imports and exhaustive typed branches.

Every dynamic provider is:

- Read-only and free of prompts.
- Deterministic for the same invocation and inspected bytes.
- Restricted to one relevant bounded inventory or catalogue.
- Free of Git, doctor, repair, formatter, package-script, Extension-code,
  download, and network execution.

External metadata is untrusted. Providers accept only validated exact ids,
sanitize every protocol field for the target shell, and omit invalid
candidates. Initial external Extension suggestions expose identity only rather
than including unreviewed package descriptions.

### Cost And Ordering

Completion runs frequently and must stay cheaper than the operation it helps
invoke. Each dynamic provider therefore has named work, time, and candidate
limits. Implementation evidence establishes their exact values before they
become production constants. Documentation does not invent unmeasured numeric
limits.

The initial implementation uses no persistent completion cache. It does not
run a general Markdown reference scan, full-text `find`, diagnosis, source
review, or broad filesystem traversal. A future cache must preserve exact
workspace selection, bounded invalidation, and the same candidate semantics.

Candidates use the shared deterministic domain ordering for their identity
kind. Completion applies ordinary prefix matching only. It does not use fuzzy
ranking, silently correct a token, or reorder candidates from command history.

### Failure Behavior

A missing workspace inventory, malformed candidate source, exceeded limit,
runtime capability failure, or provider exception produces no dynamic
candidates for that position. Static completion remains available. The hidden
endpoint emits no warning, diagnostic prose, log line, or partial protocol
record that could corrupt shell parsing.

The eventual command invocation performs complete parsing, validation, source
review, planning, and preflight. Completion failure therefore reduces
convenience without changing command availability or safety.

## Boundaries

The hidden endpoint provides bounded suggestions only. It is not a public
operation, result renderer, authority source, filesystem completion engine, or
general workspace discovery mechanism. Failure removes dynamic convenience
without changing command availability, validation, or safety.

## Verification

Implementation evidence must prove:

- Static commands, flags, aliases, and finite choices for every supported shell.
- Every accepted dynamic provider and its exact command-position mapping.
- Exact current-directory and `--workspace` resolution without discovery.
- Native path-completion handoff without an Open Forge filesystem scan.
- Deterministic prefix filtering and ordering.
- Bounded behavior for large, malformed, and adversarial candidate sources.
- Terminal-safe external ids and absence of unreviewed external descriptions.
- Empty dynamic fallback without protocol contamination after every provider
  error and limit outcome.
- Absence of writes, prompts, Git, network, formatter, package, and Extension
  execution during completion.
- Equivalent protocol behavior from the one built artifact under every
  supported runtime.

## Related Current Sources

- [CLI interface](../interface.md)
- [Completion lifecycle](completion-lifecycle.md)
- [Route inventory](route-inventory.md)
- [Managed lifecycle](managed-lifecycle.md)
- [Runtime compatibility](runtime-compatibility.md)
