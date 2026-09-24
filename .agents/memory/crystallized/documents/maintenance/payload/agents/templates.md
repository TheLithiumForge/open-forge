---
open-forge:
  description: Current maintenance contract for the installable Templates Core category entrypoint
  responsibility: Preserve the Templates category contract, source and dogfood boundary, distribution policy, and verification
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Core, Template]
---

# Templates Category Maintenance Contract

## Source

[`src/open-forge/.agents/templates/_templates.md`](../../../../../../../src/open-forge/.agents/templates/_templates.md) is the canonical installed Templates category entrypoint. The repository [Templates entrypoint](../../../../../../templates/_templates.md) dogfoods the same authored contract and adds repository-local generated entries.

The [current Templates contract](../../../framework/primitives/templates.md) defines the primitive's role and relationship with other Core primitives. The [routed Markdown contract](../../../framework/markdown/routes.md) defines the canonical representation shared by the entrypoint and routed Template files. The [Templates as a Core Primitive decision](../../../../decisions/framework/template-primitive.md) preserves why Templates are an independent Core primitive.

## Contract

- Templates are copy-ready starting files. After copying and adapting one, the destination is maintained independently.
- The entrypoint explains when to choose a Template, how the result becomes independent, how continuing requirements differ from starting content, and how customization works. Restoration follows the inherited Loader rule: removed defaults stay removed unless the user asks to restore them.
- Every Template makes its selection information visible before use: its route description and removable `{...}` instructions state the need and primary question or result.
- Template prose is never hidden in HTML comments. Exact Open Forge control-marker comments contain no instructions and remain the only first-party exception.
- Templates are on-demand. The entrypoint uses #Core and #Template without a load-policy tag so its description remains visible while its Axioms load only when the route is selected.
- A Template result receives destination-specific metadata, scope, state, authority, and relationships. The source Template does not control the result, and later Template changes do not update it.
- Continuing guidance or requirements belong to the matching #Core route. A Template may link to that source without copying its complete rules.
- Generic Templates are fallbacks. A specialized Template exists only when it provides meaningfully different starting content.
- Templates remain readable and usable through ordinary file operations. Tools may make selection and copying cheaper without defining Template meaning or result state.
- Users may edit, replace, scope, or remove templates. Normal installation or upgrade does not silently restore a removed template.

### Distribution And Dogfood

- The standard Framework ships only the Templates category contract.
- The optional [Project Documents](../../../../../../../src/extensions/project-documents/content/.agents/templates/documents/), [Collaboration](../../../../../../../src/extensions/collaboration/content/.agents/templates/collaboration/), [Observations and Handoffs](../../../../../../../src/extensions/observations-and-handoffs/content/.agents/templates/observations-and-handoffs/), [Workflow Support](../../../../../../../src/extensions/workflows/content/.agents/templates/workflows/), and [Planning](../../../../../../../src/extensions/planning/content/.agents/templates/planning/) payloads are canonical for their respective packaged Templates. The optional [Core Templates](../../../../../../../src/extensions/core-templates/content/.agents/templates/core/) and [Flows and Scenarios](../../../../../../../src/extensions/scenarios/content/.agents/templates/scenarios/) packages provide their own starter subtrees. Development Toolkit includes Project Documents, Planning, and Flows and Scenarios through dependencies; the other Template packages remain separately selectable.
- Repository-local copies of packaged leaf Templates remain aligned through automated parity verification. Repository-only Templates remain dogfood candidates rather than installable defaults.
- Promoting another dogfood Template into the package requires its own review of generic value, authority boundary, portability, and source verification.

### Core Templates

The optional [Core Templates package](../../../../../../../src/extensions/core-templates/README.md) owns `templates/core/` and has no extension dependencies. It supplies seven on-demand starters, not new root routes or a required authoring workflow.

| Source | Maintenance boundary |
| --- | --- |
| [Catalogue](../../../../../../../src/extensions/core-templates/content/.agents/templates/core/_core.md) | Explain each role, destination-specific metadata, and the native Skill exception. |
| [Directive](../../../../../../../src/extensions/core-templates/content/.agents/templates/core/directive.md) | Retain one substantive Instructions section and explain destination LoadNow classification. |
| [Guidance](../../../../../../../src/extensions/core-templates/content/.agents/templates/core/guidance.md) | Preserve the situation, recommended approach, reasons, and tradeoffs. |
| [Pattern](../../../../../../../src/extensions/core-templates/content/.agents/templates/core/pattern.md) | Define an inspectable shape, a valid or explicitly schematic example, and meaningful variations. |
| [Skill](../../../../../../../src/extensions/core-templates/content/.agents/templates/core/skill.md) | Keep a routed Template wrapper around a complete native SKILL.md starter. Only the fenced file is instantiated; native discovery remains the active runtime's responsibility. |
| [Template](../../../../../../../src/extensions/core-templates/content/.agents/templates/core/template.md) | Distinguish instructions for the Template author from removable prompts retained for its future reader. A result that is itself a Template keeps that classification. |
| [Map](../../../../../../../src/extensions/core-templates/content/.agents/templates/core/map.md) | Keep destinations, their purpose, and selection conditions explicit without copying their facts. |
| [Memory](../../../../../../../src/extensions/core-templates/content/.agents/templates/core/memory.md) | Select state by meaning, preserve source and uncertainty, and avoid requiring an optional record category. |

Package source files are canonical. Their counterparts under `.agents/templates/core/` retain the same authored content. Verify ordinary body copying, destination metadata, native Skill extraction, package links, and independent copies surviving package update/removal. A structural pass does not establish improved agent behavior or native runtime activation.

### Routing And Installation

- Generated `Entries` expose only direct template files and child routes through the ordinary route contract.
- Scoped Templates entrypoints reuse the canonical contract while retaining their local generated entries.
- The CLI recognizes Templates as a Core route type for scoped installation, category creation, validation boundaries, and extension content classification.

## Verification

- The Core installation closure test verifies that the category installs, remains on-demand, is discoverable through #Template, and produces a valid route tree.
- The category-creation closure test verifies that scoped descendants inherit #Template classification.
- The primitive-validation closure test verifies that topical #Workflow or #Directive tags inside a template do not activate those primitive schemas.
- First-party Extension integration verifies isolated package links, indexing, installation, removal, and normalized package-to-dogfood leaf parity.
- First-party source verification rejects hidden Template instructions and any
  HTML comment other than an exact position-valid Open Forge control marker.
