---
open-forge:
  description: README and documentation site overhaul in the maintainer's voice guide, with the site phase waiting for approval
  tags: [Memory, Working, Contextual, Active, Documentation, Writing]
---

# Docs overhaul

The maintainer asked for a README and documentation site overhaul that follows [Project Voice](../crystallized/documents/maintenance/project-voice.md), keeps every current fact and presents Open Forge as maintainer-friendly as well as spec-driven and safe.

## Where it stands

- The README rewrite, the integration-levels diagram and the Project Voice update are on branch `claude/readme-docs-overhaul-423xd0`, for review in a pull request.
- The site phase waits for the maintainer to approve the README.

## Next step

When the README is approved, update the site under `src/docusaurus/`:

1. Rework the landing page in `src/components/home/` around routed context, spec-driven safety and maintainer-owned docs, and add the integration-levels diagram.
2. Add one guide page for your own docs and outside routes, and replace the repeated outside-route detail in the concept pages with one line and a link.
3. State the default `.agents` context-folder caveat once, in Installation and the intro, and remove it from the other pages.
4. Rewrite `intro.md` overview first, and add outside routes and the context folder to `highlights.md`.
5. Regroup the sidebar into Start here, Guides, Concepts, CLI, Extensions and Reference.
6. Keep `docs/cli.md` as the reference and only fix links.
