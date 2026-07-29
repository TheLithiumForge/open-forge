---
open-forge:
  description: Current maintenance contract for the installable Templates Core category entrypoint
  responsibility: Preserve the Templates category contract, source and dogfood boundary, distribution policy, and verification
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Core, Template]
---

# Templates Category Maintenance Contract

## Source

[`src/open-forge/.agents/templates/_templates.md`](../../../../../../../src/open-forge/.agents/templates/_templates.md) is the canonical installed Templates category entrypoint. The repository [Templates entrypoint](../../../../../../templates/_templates.md) dogfoods the same authored contract and adds repository-local generated entries.

The [current Templates contract](../../../framework/primitives/templates.md) defines the primitive's role and relationship with other Core primitives. The [routed Markdown contract](../../../framework/markdown/routes.md) defines the canonical representation shared by the entrypoint and routed Template files. The [Templates as a Core Primitive decision](../../../../decisions/template-primitive.md) preserves why Templates are an independent Core primitive.

## Contract

- Templates are copy-ready source artifacts intended to be instantiated into independently owned workspace content.
- The entrypoint tells agents when to select a template, how ownership transfers on instantiation, how to distinguish continuing requirements from starting content, and how customization and restoration behave.
- Every template makes its selection contract visible before instantiation: its route description and removable source instructions state the need it satisfies and the primary question or result it answers.
- Templates are on-demand. The entrypoint uses #Core and #Template without a load-policy tag so its description remains visible while its Axioms load only when the route is selected.
- A template result receives destination-specific metadata, scope, state, authority, and relationships. The source template does not remain an authority over that result and later template changes do not propagate into it.
- Continuing guidance or requirements belong to the matching authoritative #Core route. A template may link to that source without duplicating its continuing contract.
- Generic templates act as fallbacks. A specialization exists only when it provides materially different copy-ready content.
- Templates remain human-readable and usable through ordinary file operations. Deterministic tools may make selection and instantiation cheaper without becoming authoritative for template meaning or instance state.
- Users may edit, replace, scope, or remove templates. Normal installation or upgrade does not silently restore a removed template.

### Distribution And Dogfood

- The standard Framework ships only the Templates category contract.
- The optional [`development-toolkit` payload](../../../../../../../src/extensions/development-toolkit/payload/.agents/templates/) is canonical for its packaged document and Memory Templates.
- Repository-local copies of packaged leaf Templates remain aligned through automated parity verification. Repository-only Templates remain dogfood candidates rather than installable defaults.
- Promoting another dogfood Template into the package requires its own review of generic value, authority boundary, portability, and source verification.

### Routing And Installation

- Generated `Entries` expose only direct template files and child routes through the ordinary route contract.
- Scoped Templates entrypoints reuse the canonical contract while retaining their local generated entries.
- The CLI recognizes Templates as a Core route type for scoped installation, category creation, validation boundaries, and extension content classification.

## Verification

- The Core installation closure test verifies that the category installs, remains on-demand, is discoverable through #Template, and produces a valid route tree.
- The category-creation closure test verifies that scoped descendants inherit #Template classification.
- The primitive-validation closure test verifies that topical #Workflow or #Directive tags inside a template do not activate those primitive schemas.
- First-party Extension integration verifies isolated package links, indexing, installation, removal, and normalized package-to-dogfood leaf parity.
