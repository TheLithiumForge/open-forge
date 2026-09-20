---
open-forge:
  description: Explore a read-only browser WebAssembly CLI playground backed by seeded Open Forge workspaces in documentation
  tags: [Memory, Idea, Contextual, Candidate, CLI, WebAssembly, Browser, Documentation, Docusaurus, Testing]
---

# Browser WebAssembly CLI Playground

## Opportunity

A future Docusaurus documentation site could make selected Open Forge commands
interactive. A browser-loaded .NET WebAssembly module would run against a small
seeded workspace in the browser filesystem and render the same command result the
native CLI would form. This could turn documentation examples into executable,
inspectable journeys without granting access to a visitor's host filesystem.

The existing Native AOT and reflection-free work is encouraging because it has
already forced explicit serialization graphs and linker-aware code. It does not,
by itself, prove browser or WASI compatibility. Browser execution has different
filesystem, process, console, threading, locking, Git, package-size, startup, and
JavaScript-interoperability boundaries.

## Candidate Boundary

Start with a deliberately read-only showcase rather than attempting to compile
the complete native host unchanged:

- select commands whose useful behavior can operate over seeded content without
  Git, child processes, OS locks, recovery, or host filesystem access;
- retain the same command-local operation and result semantics where the platform
  supports them;
- expose one small browser entry API rather than emulating a terminal process;
- use the browser runtime's owned virtual filesystem as a real platform boundary,
  not as a fake filesystem for ordinary CLI tests;
- seed authored workspaces, package catalogues, malformed cases, and expected
  results from focused reusable test-data capabilities where their meaning is
  genuinely identical;
- keep the canonical native executable and its delivery boundary unchanged.

Possible initial demonstrations include help, route discovery, source listing,
context inspection, references, or Extension discovery over a closed seed. The
actual candidate set must be chosen from accepted command dependencies rather
than from surface appeal.

## Investigation

1. Inventory command dependencies on `System.IO`, `System.CommandLine`, console
   streams, environment state, cancellation, processes, Git, locking, and native
   runtime behavior.
2. Compare the current supported .NET browser-WASM and WASI publication models
   from official sources at investigation time. Record interpreter/AOT, trimming,
   threading, filesystem, and JavaScript interop constraints separately.
3. Spike one pure operation and one real browser-filesystem read using a tiny
   seeded workspace. Prove source-generated JSON and YAML paths without reflection.
4. Define a narrow JavaScript-facing request/result boundary with deterministic
   initialization, cancellation, diagnostics, and error translation.
5. Measure download size, cold start, repeat execution, memory, and Docusaurus
   integration cost before expanding the command set.
6. Threat-model untrusted edited seed content, denial-of-service inputs, network
   access, cross-origin assets, and accidental claims of native filesystem or
   mutation safety.
7. Decide whether the result is a documentation-only adapter, a reusable embedded
   CLI core, a WASI artifact, or an idea that should remain deferred.

## Promotion Signals

Promote this idea only if a small spike reuses accepted command semantics without
forking them, remains responsive at a reasonable documentation bundle cost, and
keeps native-only effects visibly unavailable. Reusable seeds may be shared with
tests only when both consumers require identical data meaning; the playground
must not distort the test architecture merely to claim reuse.

Reject or narrow the idea if browser support requires a second product model,
weakens physical-filesystem safety, duplicates command policy, depends on broad
reflection or dynamic loading, or produces an impractical documentation payload.

## Related Sources

- [CLI Architecture](../../crystallized/documents/cli/architecture.md)
- [Test Evidence Integrity](../../../directives/open-forge/testing/evidence-integrity.md)
- [Evidence Tiers](../../../patterns/testing/evidence-tiers.md)
- [Audit CLI Constants And Test Architecture](../../archived/cli-development/tasks/test-architecture-and-constants.md)
