---
open-forge:
  description: Extensions add optional routed files into the existing tree; the extend command is a dogfooding MVP; installed files remain runtime truth
  tags: [Memory, Decision, CurrentTruth, Extension, CLI]
---

# Extensions And CLI

Accepted decisions extracted from the design sessions and idea notes on 2026-07-06.

- #Extension is optional installable material added on top of #Core and #Memory.
- Extensions should add files into the existing routed structure instead of creating another framework root.
- Extension payload routes use #Extension plus route type and scope tags by default when the file format is Open Forge-authored. Runtime-native files such as `SKILL.md` keep native metadata. Extension content does not use #OpenForge because it must stay relevance-routed unless a load-policy tag is deliberately chosen.
- The current `extend` command installs local overlays, local extension packages with `payload/`, and bundled first-party extensions as a dogfooding MVP.
- Bundled first-party extensions live under `src/extensions/{id}/payload` in the CLI package.
- First-party extension metadata outside `payload/` may help CLI list/select behavior, but installed files remain runtime truth.
- Final extension design still needs route templates, scaffold content, preview, trust/provenance, dependency handling, install/update/remove behavior, and authoring helpers.
- Installed files remain runtime truth. Manifests may help install and migrate, but agents should not need hidden manifests to route at runtime.
