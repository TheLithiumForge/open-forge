---
open-forge:
  description: "Historical CLI-v2 source: One-artifact Node.js, Bun, and Deno runtime floors, invocation, behavioral parity, capability gating, and release evidence"
  responsibility: Define what cross-runtime support means without introducing runtime-specific commands, builds, hidden fallbacks, or a polyglot shebang
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# Runtime Compatibility Contract

## Scope

Open Forge distributes one ESM artifact with one public CLI and result contract.
Node.js, Bun, and Deno execute the same bytes. Runtime selection may change
available platform evidence, but never command meaning, request shape, result
shape, or authority.

## Guarantees

### Accepted Floors

| Runtime |   Minimum | Support promise                                                                                                                             |
| ------- | --------: | ------------------------------------------------------------------------------------------------------------------------------------------- |
| Node.js | `22.12.0` | Full support                                                                                                                                |
| Bun     |   `1.3.0` | Full support                                                                                                                                |
| Deno    |   `2.8.0` | The same CLI and application contract, with explicit capability blocking where the runtime and filesystem cannot prove a required guarantee |

Node.js `22.12.0` is the natural floor imposed by Commander 15 and remains on
a supported Node.js release line. Bun 1.3 is the accepted development and
runtime baseline. Deno 2.8 is the accepted baseline for the Node compatibility
surface used by the artifact. A release should recommend current supported
patches even when an older patch remains inside the accepted range.

These ranges describe the replacement CLI. `package.json` is the current
machine-readable source for the runtime floors it can express: Node.js
`>=22.12.0` and Bun `>=1.3.0`. npm package metadata cannot express the Deno
floor, so this contract and the exact-floor executable end-to-end
configuration retain Deno `>=2.8.0`. Build and test configuration consume or
verify these sources instead of repeating unrelated version literals.

### One Artifact

The release contains one artifact:

```text
dist/cli.mjs
```

Build-time generated modules embed the exact Framework payload, first-party
Extension catalogue, and their fingerprints in this artifact. Standalone
source trees, archives, manifests, and adjacent payload copies are not release
artifacts. The authored Framework and Extension sources remain inspectable in
the source repository without duplicating them beside the executable.

It is built once as ESM with Bun's Node target and contains the ordinary Node
shebang:

```text
#!/usr/bin/env node
```

The shebang makes Node.js the default host for a conventional package-manager
installation. Bun may explicitly override it, and Deno may execute or install
the same npm entrypoint through its own launcher. Open Forge does not ship
runtime-specific bundles, native executables, or a polyglot shebang.

The installed public command is always:

```text
open-forge
```

Representative one-shot launchers are:

```text
npx open-forge
bunx --bun open-forge
deno x npm:open-forge
```

A runtime-specific global installer may create its ordinary wrapper so the
resulting installed command is still `open-forge`. Launcher installation and
package resolution are explicit user actions, not network behavior performed
by Open Forge.

### Behavioral Parity

Every supported runtime exposes the same:

- Command paths, arguments, flags, help, and completion protocol.
- Request and result types, JSON schema, messages, statuses, and exit mapping.
- Workspace and content-path semantics.
- Planning, preflight, authority, mutation, verification, and recovery stages.
- Deterministic ordering and formatted presentation rules.

Production source uses standard JavaScript and the deliberately supported
`node:` compatibility surface. It does not import `bun:` modules, depend on the
`Bun` global, require Deno globals, or select separate domain implementations
by runtime name.

### Capability Gating

A supported runtime does not imply that every runtime and filesystem pair can
prove every operation's prerequisite. Open Forge probes the required
capability before application and returns the ordinary typed blocked result
when evidence is insufficient.

For example, Deno on Windows NTFS may not preserve enough precision for exact
file identity. An identity-sensitive mutation blocks with the same diagnostic
and next action that any insufficient platform capability receives. It does
not continue with weaker comparison, silently omit verification, or expose a
different Deno command.

Runtime detection may select a focused capability adapter or diagnostic. It
must not reinterpret input, weaken containment or authority, or become a
string-resolved behavior registry.

Deno permissions remain explicit launcher authority. Open Forge does not ask
for network access. Read, write, environment, and subprocess permissions are
granted only as required by the selected invocation and remain separate from
Open Forge confirmation flags.

### Implementation Ownership

Runtime identifiers, accepted floors, support states, and capability results
become named readonly production values. Package metadata is read or embedded
at the build boundary. Application modules receive typed runtime facts and do
not repeat raw version or runtime strings.

## Boundaries

Runtime support promises one artifact, interface, and application contract.
It does not promise that every runtime and filesystem pair can prove every
operation prerequisite, introduce runtime-specific commands or builds, weaken
a failed capability check, request network access, or provide a polyglot
shebang.

## Compatibility And Evolution

The accepted runtime floors describe the current replacement candidate.
`package.json` is authoritative for the Node.js and Bun ranges, while this
contract and the exact-floor runtime evidence retain the Deno range. Changing
a floor, support state, artifact shape, invocation path, or capability promise
requires the document, applicable package metadata, build boundary, and
runtime matrix to advance together. A runtime-specific capability block
preserves compatibility. A runtime-specific command, result, or weaker
fallback does not.

## Verification

One Bun-hosted runtime-matrix suite executes the exact candidate artifact under:

- Every accepted minimum version.
- The current supported Node.js LTS lines.
- The latest stable Bun release.
- The active Deno LTS and latest stable release when distinct.

The same artifact hash must pass representative help, parsing, structured
output, read-only workspace, mutation, recovery, and completion-protocol
journeys. Minimum-version lanes use exact pinned releases rather than mutable
tags. Runtime-specific capability tests assert the documented blocked result
where a guarantee is unavailable.

Operating-system and filesystem coverage is a separate evidence matrix. A
runtime passing on one host does not prove symlink, metadata, identity, or
atomicity behavior on another.

## Related Current Sources

- [CLI interface](../interface.md)
- [CLI development toolchain](../development-toolchain.md)
- [Operation prerequisites](operation-prerequisites.md)
- [Filesystem effects](filesystem-effects.md)
- [Completion protocol](completion-protocol.md)
