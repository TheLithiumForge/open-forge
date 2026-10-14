---
open-forge:
  description: "Historical record: shipped Patterns Axioms that show a visual expected result and link both ways without naming categories, blind-reviewed and released in beta13"
  tags: [Memory, Task, Framework, Pattern, Core, Linking, Documentation, Contextual, Complete, Archived, Historical]
---

# Task 80: Patterns show the shape

## Outcome

The shipped Patterns entrypoint `src/open-forge/.agents/patterns/_patterns.md` is correct. Its Axioms make every Pattern a visual representation of how something should be. A reviewer can be pointed at a piece of code or a file together with the matching Pattern and understand the expected shape without further explanation. The Axioms encourage linking and referencing in both directions between Patterns and the other Core categories.

The maintainer asked on 2026-10-13:

> "Make sure the patterns actually represent what I want - a visual representation easy to understand for people and agents of how something should be, basically throw a reviewer on some piece of code and show the pattern and it should be understandable. We already did a pass for that, just wanted to make sure of it. Also make sure you encourage linking and referencing to and from patterns to other core categories (we do not name them though)."

The maintainer then narrowed the scope on 2026-10-13: the shipped `patterns.md` file is what matters most and must be correct. Maintenance of this repository's own Patterns moves to [Task 81](../../../working/cli-development/tasks/task81-own-patterns-maintenance.md), after the next beta deployment and release.

Status: Task 80 “Patterns show the shape” (phase 3/3): released in beta13 with Tasks 78 and 79.

## Scope

- The shipped Patterns entrypoint `src/open-forge/.agents/patterns/_patterns.md`, its Axioms and anything that copies or quotes them.
- The Pattern guidance in the Framework documentation and site pages, kept in line with the entrypoint.

Out of scope: this repository's own Patterns and bundled Extension Patterns, which belong to Task 81.

## Approach

1. Find the earlier Pattern clarity pass in the Archived records and Git history and build on it.
2. Check the Axioms against the outcome: a Pattern's main content is a concrete example, tree, table, diagram or outline that a reviewer can apply to real code or files.
3. Run a reviewer test: give a reviewer the entrypoint, one example Pattern written by its rules, and one real artifact, and record where the shape was unclear.
4. Strengthen the linking guidance so Patterns link to the rules, advice, maps and procedures that relate to them, and those sources link back.
5. Update the entrypoint, the documentation that describes it, generated Entries and the affected tests and snapshots, then check links.

## Reading of the maintainer's wording

The reading of “we do not name them though” follows the other shipped entrypoints, which describe related sources by what they hold: required rules, advice, maps, procedures, and starting files. Patterns encourage links to those sources and back from sources that apply the shape, without naming their categories or changing tags, routes, or loading behavior.

## Author receipt, 2026-10-14

Built on [Task 28's Pattern wording and exception review](../../../archived/cli-development/tasks/source-framework-review.md) and the visual-main-content revision in `300442fd6`, found through Git pickaxe. The shipped entrypoint and its maintenance, Framework, README and site explanations now identify required, optional and repeatable parts, nesting, fixed and illustrative details, binding sources, and justified departures reported separately from conformance.

The kit contains an API-body Pattern, two complete JSON artifacts, a linked requirement source, and a starting body with a backlink. Two independent blind reviewers, each given one artifact without an answer key, found it clear. Their verdicts were that A conforms and B deviates because `fields` is an object instead of an array and `requestId` is misplaced. The Overseer accepted their matching tightenings in the shipped Axioms.

Managed verification passed: Release build, 4,962 Unit tests, 16 Markdown/payload/parity tests, 2 Skill journey tests, and 4 Status tests, with no failures or skips. Prettier, whitespace, five-file Skill byte parity, and `git diff --check` passed. Every Status difference was read before updating snapshots. The shipped startup estimate is now 6,568 tokens, displayed as 6.6k in the updated documentation. Repository and Extension Patterns remain unchanged for Task 81. No commit or native publication occurred.

## Final gates, 2026-10-14

The stack with Tasks 78, 79 and 80 and the permission prompt default passed at `0b6a259d0`. Windows x64 `build:native` and `test:built`: Unit 4,962, Integration 3,297 with 17 platform exclusions, public 376, native Integration 3,297, native public 376 and public against the native CLI 376. WSL Ubuntu: Unit 4,962, Integration 3,278 with 36 declared skips, EndToEnd 341 with 35 declared skips. The site typecheck and build with local search, Prettier on all Markdown and repository Index also pass.
