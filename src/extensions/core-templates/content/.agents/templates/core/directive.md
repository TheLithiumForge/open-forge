---
open-forge:
  description: Write required behavior for one clearly selected scope
  tags: [Extension, Template, Directive]
---

# {Required Behavior}

{
Use when accepted direction must govern agent behavior. For advice, choose Guidance.
Place the result under the relevant Directives scope. Give it its own description and tags, including #Directive and #LoadNow. Remove #Template and #Extension.
Update the containing entrypoint's Entries. A child scope must be selected before its Directives load.
Use optional `applyTo` only when matching workspace-relative paths should narrow where the Directive applies. It is checked before loading and may appear at the frontmatter root or under `open-forge:`. Keep `#LoadNow` required.
Replace prompts and remove this source guidance. Keep the instructions under a non-empty Instructions heading.
}

## Instructions

- {State the action required, the condition that triggers it, and the scope where it applies.}
- {When needed, state a meaningful exception or the response to a conflict or unavailable requirement. Refer to the defining source when another instruction already supplies the detail.}

{Use additional rules only for distinct obligations. Do not copy inherited rules or add approval requirements that the accepted direction does not establish.}
