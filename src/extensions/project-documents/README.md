# Project Documents

Maintain understandable current project knowledge without duplicating it. Keep documentation where the project already owns it, or use the supplied Documents convention.

## What You Get

| Source | Use it to |
| --- | --- |
| [Documents](content/.agents/memory/crystallized/documents/_documents.md) | Keep one coherent current explanation of a subject. |
| [Vision method](content/.agents/skills/use-workflow/references/project-documents/vision.md) | Clarify purpose, audience, first useful value, and success. |
| [Architecture method](content/.agents/skills/use-workflow/references/project-documents/architecture.md) | Explain responsibilities, boundaries, relationships, and adoption. |
| [Five Templates](content/.agents/templates/documents/_documents.md) | Document, Vision, Architecture, Principles, and Maintenance Contract. |

**Direct dependencies:** [Workflow Support](../workflows/README.md)

## Start Using It

> Use the architecture workflow to review this design. Keep proposed changes distinct from the current architecture and update the existing source when accepted.

## Start With The Question

Use a general Document for a current explanation. Add specialized documents only when their questions need separate maintained answers. This is not a required document set.

Documents are kept top-down. Top-level Documents give an overview of the whole and summarize the narrower Documents they link to. Each narrower Document explains its own part in more detail and links back up. When a Decision changes a narrower Document, the summaries above it are updated too.

Drafts remain candidates until accepted. Existing documentation need not move into Memory. The optional Documents category loads on demand, and the package works without Planning or its Decision convention. Link to rationale wherever the workspace already keeps it.

## Install And Customize

Use the complete source catalogue so dependencies resolve together. See the [installation guide](../../../docs/extensions.md) for managed and manual setup.

Keep or remove the methods, categories, and starter files that fit the workspace. Review dependent references and user-created descendants before removal. Template copies remain independent; deleting a starter never means deleting the work created from it.

