---
open-forge:
  description: Explore additional thin package-manager wrappers after canonical native artifacts and the first npm wrapper ship
  tags: [Memory, Idea, Contextual, Candidate, CLI, Distribution, Package, Bundle, Executable, Dogfood]
---

# CLI Distribution Channels After Initial Release

## Current Boundary

The canonical executable uses .NET Native AOT. The accepted
[distribution graph](../../crystallized/documents/cli/distribution.md#accepted-package-graph)
contains one thin npm wrapper and six native platform packages. The root
`package.json` remains an ecosystem-neutral orchestration layer rather than the
distributable package. Native artifact and wrapper placement are settled in
that graph. Additional channels and distro-specific variants remain candidates.

Release receipts and the current release hold are separate from this packaging
baseline. This Idea does not authorize publication.

## Candidates

- Explore additional ecosystem-native packages that install the same CLI or a
  compatible executable, including PyPI, NuGet, and other justified package
  conventions. Define version identity, platform coverage, provenance,
  signatures, update behavior, and support ownership before adding a channel.
- Consider direct executable release assets only with an accepted checksum,
  signature, provenance, and upgrade story. Do not revive a source archive as
  a side effect of adding binary delivery.
- Evaluate publishing each generic Linux RID artifact together with an opt-in
  distro-specific artifact for users who need a narrower platform baseline. Keep
  the generic artifact canonical unless compatibility evidence justifies another
  support contract. Define artifact naming and installer selection so the two
  variants cannot be confused.

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
6. Before accepting distro-specific Linux artifacts, measure their compatibility
   gain against generic artifacts on named distro and glibc support floors. Also
   account for native-runner coverage, duplicated signing and provenance,
   vulnerability rebuild cadence, installer selection, documentation, and the
   long-term support cost of every added variant.
