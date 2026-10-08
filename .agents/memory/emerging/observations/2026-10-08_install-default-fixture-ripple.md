---
open-forge:
  description: Changing what a fresh Install writes broke about 200 unrelated tests because fixtures install the Framework directly and enumerate its files
  tags: [Memory, Observation, AgentLearning, Contextual, Candidate, CLI, Testing, Orchestration, Dogfood]
---

# Observation: A Fresh Install Change Ripples Through Fixtures

## Expected Behavior

A Task that changes what a fresh Install writes should update the tests about Install, plus a small number of tests that check the installed file set.

## Observed Failure

Task 75 made every fresh Install write `.agents/open-forge.json` and made an unattended fresh Install deliver root-form metadata. Both behaviors were accepted. After the consumer wave was integrated, about 115 Integration tests and 93 EndToEnd tests failed. Almost none of them were about Install or frontmatter.

The failures fell into four groups:

- Fixtures created `.agents/open-forge.json` after Install and now hit an existing file.
- Temporary-workspace and published-journey cleanup only knew a fixed list of product-written files, so the new settings file made disposal fail. In the EndToEnd tier this masked the real assertions behind cleanup errors.
- Tests compared delivered files with the canonical scoped payload bytes after an unattended Install.
- Tests enumerated fresh-install effects, files, or receipts exactly.

Fixtures install the Framework through many local helpers, about 21 in the Integration project alone, and keep their own lists of product-written files. No shared install fixture let one change cover them.

## Correction And Evidence

Five fixture slices updated the tests without changing product code. Tests whose subject is not the delivered form now install explicitly scoped. Cleanup lists and effect expectations include the settings file. Settings writes after Install replace or merge the file. The work also exposed two real product defects that the cleanup errors had hidden: a missing ownership receipt and a presentation gap. The managed Integration tier went from 115 failures to green, and the EndToEnd tier from 93 failures to 306 passing.

## Promotion Signal

Consider one shared test capability that knows every file a fresh Install writes, used by every fixture that cleans up or enumerates an installed workspace. Promote it when another Task changes the fresh-install file set. Until then, plan a fixture wave whenever a Task changes what a fresh Install writes or delivers by default, and run the EndToEnd tier before calling a consumer wave green.
