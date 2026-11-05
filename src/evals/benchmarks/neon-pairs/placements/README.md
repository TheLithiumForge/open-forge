# Placements

A placement says where a large document pack goes when a project uses Open Forge properly: which category and topic scope holds each document, on each install rung, and which sentences move into rule files. `materialize/placement.ts` reads it, and restoring a placed project must give back the frozen pack byte for byte.

It sits outside the case folder because it's a setup choice and not part of the case. Plain docs and Organized docs never read it.

`l4-f2-tournament.json` was made in two steps:

- **Rule sentences** come from a blind reading. Sol read the whole pack without `rules.json` and listed every sentence that binds new work. It found all 30 graded rules among 341 sentences and listed none of the superseded statements, so the rule files don't depend on which rules are graded.
- **Routes and routing text** come from each kind of document's Open Forge category, and from the descriptions in `docs-scopes.json`.

Cross-cutting rules load for matching work through `#LoadNow` and `applyTo`. Each feature area's rules stay on demand, for a task to select by description.
