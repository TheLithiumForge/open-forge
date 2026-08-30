---
open-forge:
  description: Embed and read the canonical Framework payload through ordinary BCL resources
  tags: [Memory, Working, CLI, Task, Foundation, Framework, Distribution, EmbeddedResource, NativeAOT, Contextual]
---

# Embed The Canonical Framework Payload

## Task State

- State: Ready after contract-freeze integration.
- Parent: [Next-Wave Shared Foundations](_shared-foundations.md).
- Consumers: root Install, root Update, and Framework-aware Route Init.

## Outcome

Core embeds the complete canonical `src/open-forge/` tree through ordinary
`EmbeddedResource` items under one fixed logical-name prefix and exposes one
immutable payload and deterministic inventory through `Framework/Distribution/`.
Runtime never reads the checkout.

## Architecture And Ownership

- Production: `Framework/Distribution/**` and
  `OpenForge.Cli.Core.csproj` resource wiring.
- Models: payload, asset, and inventory data under the nearest matching
  `Models/` scope; the reader owns exact-prefix BCL access.
- Reader rules: canonical `/` paths; reject empty, unsafe, duplicate, or
  noncanonical identities; exact bytes; ordinal order; per-asset SHA-256; one
  deterministic inventory fingerprint.
- Protected: commands, lifecycle models, root composition, source checkout at
  runtime, dependencies, and the existing embedded Extension catalogue design.

## Evidence

Compute source/payload set and byte parity from `src/open-forge/`; do not maintain
a second literal hash inventory. Cover path validation, order, hashes, aggregate
fingerprint, and no checkout access. Publish `linux-x64`, move the binary to an
isolated directory, and prove every resource remains readable under Native AOT.

## Stop Conditions

Stop before base64/GZip source generation, archive packaging, runtime repository
reads, reflective assembly/type scanning, reflective behavior discovery, a new
package, or any feature addition/removal not accepted by the maintainer. Bounded
manifest-resource-name enumeration under the exact prefix remains ordinary BCL
payload access.
