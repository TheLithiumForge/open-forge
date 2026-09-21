---
open-forge:
  description: Open Task 46 to let a native Skill's resources be used as routes and to read route metadata from the native SKILL.md frontmatter instead of demanding an Open Forge block
  tags: [Memory, Working, CLI, Task, Skills, Routing, Metadata, Beta, Contextual, Active]
---

# Task 46 — Routed Skill resources

## Task state

- State: **Rechecked; fresh-install report superseded, broader routing choice remains open.** Raised by the maintainer on 2026-09-18.
- Owner: Root.

The 2026-09-21 [navigation recheck](beta-follow-ups/task46-47-navigation.md) found zero Doctor findings after installing Core and all bundled Extensions. The thirteen-warning report below is historical and no longer reproduces. The packet separates the surviving catalogue-advice defect from the broader resource-routing decision.

- Caused by [Task 43](task43-workflows-as-skill.md) landing: the Workflow
  Support Extension puts real routed content inside a native Skill directory,
  and the routing model has no answer for it.

## The problem

A Skill is native. Its resources are not supposed to be Open Forge routes — and
yet the workflow recipes beneath `.agents/skills/use-workflow/references/` are
authored exactly like routes: `open-forge:` frontmatter, descriptions, tags,
`Entries` sections. They behave like routes because they are routes. The only
thing standing between them and the loader is `SKILL.md`, which is not an
entrypoint.

The result is that installing any first-party Extension makes `doctor` fail.

Measured on 2026-09-18 in a scratch workspace, `install` then
`extension install --all` then `doctor`: **exit 2, thirteen warnings, every one
of them a file the Extensions just installed.**

```text
.agents/skills/use-workflow/SKILL.md                    Required route metadata is missing
.agents/skills/use-workflow/references/_references.md   Source is outside the loaded routes
.agents/skills/use-workflow/references/*/_*.md          Route cannot be reached   (4 scopes)
.agents/skills/use-workflow/references/*/*.md           Route cannot be reached   (7 recipes)
```

Installing `development` alone is enough to produce six of them. Two end-to-end
tests already encode the promise this breaks:
`PublishedDoctorGeneratedNavigationProcessTests.FreshExtensionNavigationIsNotFrameworkDrift`
expects `doctor` to exit 0 after an Extension install, for `development` and for
`development-toolkit`. Both fail. They are correct to fail.

## Two decisions, not one

**Should a Skill's resources be routable at all?** Today the answer is implicitly
no, and the content says yes. Either the route scanner stops at a Skill boundary
and treats everything below `SKILL.md` as the Skill's own payload, or the route
chain is allowed to pass through a Skill and the resources become ordinary
routes with an ordinary host. Both are defensible; what does not work is the
current state, where the content claims one and the tooling assumes the other.

**How is a Skill's own metadata read?** `SKILL.md` carries native `name` and
`description` fields, by design — the package deliberately uses only the two
fields the harness defines, and adds no provider-specific keys. Demanding an
`open-forge:` block on top of that would pollute a file another tool owns.
Read the native frontmatter instead: `name` and `description` are already the
two facts a route entry needs, and tags can fall back to absent rather than
missing.

`.agents/skills/_skills.md` already lists the Skill and prints its native
description as the entry text, so half of this is done. The classifier just does
not accept that file as a host.

## What it should reach

Entries currently regenerate for the catalogue only when it is named explicitly:

```sh
open-forge index .agents/skills/use-workflow/references/_references.md
```

From the loader roots, the whole subtree is invisible. Whatever this task
decides, that asymmetry is what [Task 47](task47-entrypoint-reachability.md)
has to resolve; the two share a root cause and should be settled together.

## Boundaries

This is a routing and classification change, not a licence to move the recipes.
The layout came from Task 43 and is the accepted direction. Do not solve this by
putting the methods back under a routed root, and do not solve it by adding an
`open-forge:` block to a native `SKILL.md`.
