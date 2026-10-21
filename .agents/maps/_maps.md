---
open-forge:
  description: Where to find project knowledge, checked before relying on it
  tags: [LoadNow, Core, Map]
---

# Maps

This folder is optional and nothing else depends on it. It holds curated pointers to local and external sources. Each destination keeps its own detail and authority.

## Axioms

- Check these Entries when the work needs an important local or external source.
- Each Map links to one or more destinations and says what they contain and when to use them. Local links are relative to the Map. External sources use their normal URL.
- Keep Maps broad. Prefer modules, projects, repositories, systems or scopes over members and functions unless finer routing clearly helps.
- Choose Map filenames, grouping and nesting depth to suit the workspace.
- Maps do not replace Memory or the destination's current information.

## Entries

- [Where the build, test, package and release system is documented and implemented, for work on delivery scripts, workflows or npm scripts](build-and-delivery.md) - #LoadNow #Map #Build #Delivery #Pipeline #Release #CurrentTruth #Evergreen - applies to `.github/workflows/**`, `package.json`, `scripts/delivery/**`
- [Current map of the repository's important authoritative sources and representations](sources-of-truth.md) - #Map #Repository #CurrentTruth #Evergreen
