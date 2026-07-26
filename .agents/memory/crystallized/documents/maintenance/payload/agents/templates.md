---
open-forge:
  description: Current maintenance contract for the installable Templates Core category entrypoint
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Core, Template]
---

# Templates Category Maintenance Contract

## Source

[`src/open-forge/.agents/templates/_templates.md`](../../../../../../../src/open-forge/.agents/templates/_templates.md) is the canonical installed Templates category entrypoint. The repository [Templates entrypoint](../../../../../../templates/_templates.md) dogfoods the same authored contract and adds repository-local generated entries.

The [Framework Architecture](../../../framework/architecture.md#templates) owns the primitive's current role and relationship with other Core primitives. The [Templates as a Core Primitive decision](../../../../decisions/template-primitive.md) preserves why Templates have independent ownership.

## Contract

- Templates are copy-ready source artifacts intended to be instantiated into independently owned workspace content.
- The entrypoint tells agents when to select a template, how ownership transfers on instantiation, how to distinguish continuing requirements from starting content, and how customization and restoration behave.
- Every template makes its selection contract visible before instantiation: its route description and removable source instructions state the need it satisfies and the primary question or result it answers.
- Templates are on-demand. The entrypoint uses #Core and #Template without a load-policy tag so its description remains visible while its Axioms load only when the route is selected.
- A template result receives destination-specific metadata, scope, state, authority, and relationships. The source template does not remain an authority over that result and later template changes do not propagate into it.
- Continuing guidance or requirements belong to the matching #Core owner. A template may link to that owner without duplicating its continuing contract.
- Generic templates act as fallbacks. A specialization exists only when it provides materially different copy-ready content.
- Templates remain human-readable and usable through ordinary file operations. Deterministic tools may make selection and instantiation cheaper without owning template meaning or instance state.
- Users may edit, replace, scope, or remove templates. Normal installation or upgrade does not silently restore a removed template.

### Distribution And Dogfood

- The installable source currently ships only the Templates category contract.
- Repository-local templates under [`.agents/templates/`](../../../../../../templates/) are dogfood candidates, not installable defaults.
- Promoting a dogfood template into the source payload requires its own review of generic value, baseline cost, ownership boundary, and source verification.

### Routing And Installation

- Generated `Entries` expose only direct template files and child routes through the ordinary route contract.
- Scoped Templates entrypoints reuse the canonical contract while retaining their local generated entries.
- The CLI recognizes Templates as a Core route type for scoped installation, category creation, validation boundaries, and extension content classification.

## Verification

- `open-forge doctor` must report no route errors for both the repository and `src/open-forge/`.
- The Core installation closure test verifies that the category installs, remains on-demand, is discoverable through #Template, and produces a valid route tree.
- The category-creation closure test verifies that scoped descendants inherit #Template classification.
- The primitive-validation closure test verifies that topical #Workflow or #Directive tags inside a template do not activate those primitive schemas.
- Review source and dogfood entrypoints with generated `Entries` excluded; their authored contracts must remain identical.
