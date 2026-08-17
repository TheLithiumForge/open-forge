---
open-forge:
  description: Explore additional thin package-manager wrappers after canonical native artifacts and the first npm wrapper ship
  tags: [Memory, Idea, Contextual, Candidate, CLI, Distribution, Package, Bundle, Executable, Dogfood]
---

# CLI Distribution Channels After Initial Release

## Current Boundary

.NET Native AOT is the direction for the canonical executable. The first thin
wrapper will use npm. The root `package.json` remains an ecosystem-neutral
orchestration layer rather than the long-term distributable package. Exact
native artifacts, wrapper placement, and later channels remain unsettled.

## Candidates

- Explore additional ecosystem-native packages that install the same CLI or a
  compatible executable, including PyPI, NuGet, and other justified package
  conventions. Define version identity, platform coverage, provenance,
  signatures, update behavior, and support ownership before adding a channel.
- Consider direct executable release assets only with an accepted checksum,
  signature, provenance, and upgrade story. Do not revive a source archive as
  a side effect of adding binary delivery.

## Evidence Before Promotion

1. Complete and dogfood the canonical Native AOT executable and npm wrapper first.
2. Measure an observed installation, startup, offline-use, or distribution
   limitation rather than optimizing an assumed one.
3. Specify one public command and result contract across every retained
   channel, including failure and capability behavior.
4. Prove packaging contents, runtime payload independence, provenance,
   reproducibility, and upgrades on the supported platform matrix.
5. Accept each additional channel as its own distribution decision and release
   responsibility.
