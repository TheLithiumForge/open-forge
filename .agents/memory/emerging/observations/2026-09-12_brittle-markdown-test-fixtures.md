---
open-forge:
  description: Markdown wording changes broke an Update fixture because its scenario depended on replacing incidental live prose
  tags: [Memory, Observation, AgentLearning, Contextual, Candidate, CLI, Testing, Snapshot, Markdown, Dogfood]
---

# Observation: Test Scenarios Should Not Depend On Incidental Markdown Wording

## Expected Behavior

A Framework wording improvement should not break a test of unrelated CLI
behavior. Tests should construct their intended scenario explicitly and assert
the behavior they claim to verify.

## Observed Failure

The Update Integration fixture `SeedCoalescedAuthoredAndGeneratedChange` read
the current embedded Memory entrypoint and replaced a specific explanatory
sentence to simulate older authored content. It separately removed one generated
entry. When Framework prose changed, the first replacement silently did nothing.
The fixture's single combined check still passed because the generated entry
had changed. The test then expected both a File and GeneratedRegion change but
observed only GeneratedRegion.

The failure was recorded during scoped-continuity work and reproduced during
the later full fixture review. The product's coalescing behavior was not the
cause. The test had stopped constructing its stated scenario.

## Correction And Evidence

The first correction changed the authored heading and checked both edits
independently; the focused selection changed from 86 passing and one failing
to 87 passing. The broader repair removes that remaining wording dependency:
it adds deliberate authored fixture content and uses the existing Markdown
parser to replace the generated Entries region with an explicit empty region.
The fixture requires a populated original region so both changes are real.
A second older-source fixture now also appends deliberate content instead of
replacing a sentence. All behavioral assertions remain unchanged.

The final full Integration run passed 1,722 of 1,724 cases, including every
Update case. The two remaining failures expose stale embedded Extension data;
they are not wording-dependent fixtures. The test still proves coalescing,
exact final bytes, recovery, verification and
repeat no-op behavior. Its dependency on machine-readable region structure is
intentional; headings and explanatory sentences no longer define its scenario.
The [follow-up plan](../../working/cli-development/tasks/cli-dogfood-follow-up-plan.md)
tracks the wider audit and qualification.

## User Direction And Future Guidance

The user explicitly asked to avoid brittle tests that fail merely because a
Markdown sentence changes, and prefers snapshots where they are the best fit.

- Build ordinary behavioral scenarios from small explicit fixtures that expose
  the meaningful input. Avoid silent search-and-replace over incidental prose.
- Use reviewed snapshots when stable Markdown, rendering, or complete output
  wording is the intended subject. Update them deliberately when that wording
  changes. Ordinary test runs must not rewrite the expected result.
- Keep safety, state, effects, identity and ordering as direct assertions.
  A broad snapshot should not obscure those guarantees.
- Keep source-to-distribution parity checks strict. A stale embedded catalogue
  is a product mismatch, not permission to update a test oracle until it agrees.

Snapshots do not make every wording change irrelevant: an intentional output
contract change should produce a focused, reviewable snapshot diff. The useful
distinction is whether prose is the test's subject or an incidental dependency.
No new snapshot library or blanket test rewrite is implied by this Observation.
