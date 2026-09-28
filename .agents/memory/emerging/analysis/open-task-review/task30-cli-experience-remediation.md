---
open-forge:
  description: Review of Task 30 CLI Experience Remediation, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation
  tags: [Memory, Analysis, TaskReview, Contextual, Candidate]
---

# Task 30 CLI Experience Remediation Review

## Question

Is Task 30 still worth doing, what exactly remains, and when should it run?

**Task record:** [Task 30](../../../archived/cli-development/tasks/task30-cli-experience-remediation.md)

## Current Conclusion

**Recommendation:** Close as done.

**Size:** Small, because only record keeping and moving four small residues to the Tasks that already own them remain.

Every slice the record lists as "Specified, not started" is implemented. Slices 50 to 54, 70 to 72, and 73 all carry changes ledgers, and their tests are in the tree. The umbrella now holds only residues that other Tasks own better. Keeping it open makes the ledger say "Phase 5-D is next" when that work shipped before the beta.

## What It Implies

Users see no change. The CLI already has the diagnosis ownership handoff, the Repair truthfulness rule, the consequence clauses, and the workspace-shape journeys. For maintenance, closing removes a stale next boundary from [project control](../../../working/cli-development/project-control.md) and from [the plan](../../../working/cli-development/plan.md), which both still send the next agent to Phase 5-D. It also lets the two Emerging CLI analysis scopes be sealed, as the analysis overwrite allows once their conclusions have an active owner.

## State Today

Verified against the tree at `815324f9`:

- **Phase 5-D is done.** `DiagnosisOwnershipIntegrationTests` holds the shared handoff and `RepairDoesNotReportCompletionOverBlockingDoctorFinding`. `CliReportInvariantsTests` has `AssertBlockingFindingHasConcreteNextAction` and `CommandOutputSnapshotDirectoriesHaveLiveOwners`. Repair validates through `PortableWorkspacePath`. Slice 52 found all four input postures already correct.
- **Phase 7-S is done.** `WorkspaceShapeJourneyIntegrationTests` covers the dirty and relocated shapes. Slice 72 recorded four capture-normalization findings (F-01 to F-04) and changed nothing, by design.
- **Slice 73 is done.** Seven messages gained a consequence clause.
- **Phase 6 is not satisfied.** Its test was "close it if nothing drifts". The [Task 61](../../../working/cli-development/tasks/task61-documentation-accuracy-and-voice.md) review found seven contract contradictions, which is exactly that drift. The shared presentation rules (the `Workspace:` echo in decision C12 and the `Next:` rule) also exist only in the archived G4 conventions and in `CliReportTrimmer`, not in a current contract.
- **Structural item 2** is settled on the Framework side. Since `a887e730` the loader says missing or empty Axioms sections add no rules. The Doctor contract still reports `route.axioms-invalid` for a missing section, so one reconciliation remains.
- **Structural item 4 is moot.** `scripts/agent-tooling` no longer has a Markdown or Entries parser. Its only regular expressions read Codex TOML.
- **Structural item 5** and the managed-host heading direction are owned by [Task 60](../../../archived/cli-development/tasks/task60-cli-skill.md) and [Task 36](../../../working/cli-development/tasks/task36-extension-merge-and-guards.md).

Stale in the record: phase map rows 4 and 6, "Current next step", the evidence baseline of 18 failures in 3,365 (Task 45 closed with 3,271 passing), and slice paths under `src/cli/core`, which is now split into `framework`, `operations`, `output-text`, `rendering`, `root`, and `shell`.

## Dependencies

- **Blocked by:** nothing.
- **Blocks:** nothing directly. Its stale boundary misleads task selection.
- **Overlaps with:** Task 36, Task 60, [Task 39](../../../working/cli-development/tasks/task39-output-audit.md) (one-code splits), [Task 40](../../../working/cli-development/tasks/task40-capture-coverage.md) (normalization findings), [Task 37](../../../working/cli-development/tasks/task37-wording-review-against-proposals.md) (the four G4 proposals live in this Task's folder), and the proposed reconciliation Task for the Task 61 findings.

## Remaining Work

1. Mark 5-D, 7-S, and slice 73 complete in the phase map, linking each ledger.
2. Move Phase 6 into a new Task for the Task 61 CLI findings. Seed it with the contract contradictions, the Doctor Axioms reconciliation, and a current home for the shared presentation rules.
3. Move F-01 to F-04 to Task 40. Move the `context.closure-unavailable` and `references.invalid-encoding` one-code questions to Task 39.
4. Record item 4 as moot, and point items 5 and the heading direction at Tasks 60 and 36.
5. Update project control and the plan, then archive the record and the finished slices. Leave the G4 proposals where Task 37 links them until Task 37 is decided.
6. Seal the two Emerging CLI analysis scopes.

## Pros And Cons

| Pros                                                            | Cons                                                                   |
| --------------------------------------------------------------- | ---------------------------------------------------------------------- |
| Removes a stale "next" that sends agents to finished work       | The residues scatter across four Tasks, so each owner must accept them |
| Each residue lands with the Task that already owns its question | Closing loses one place that explains the whole remediation arc        |
| Sealing the analyses ends the second-plan risk they carry       | Archiving must keep every link from Tasks 37, 39, and 40 working       |

## Risks And Open Questions

- Moving the G4 proposals before Task 37 is decided would break its inputs. Leave them in place until then.
- The maintainer must rule on slice 72's F-02 (a regex that replaces any 64-hex token) and F-04 (four debug captures leaking a temporary path), because both change captured values.
- Whether contract reconciliation deserves its own Task or joins the small reconciliation step proposed in the Task 33 review is a choice for the maintainer.

## Next Check

**Action:** Update the phase map to match the ledgers, then ask the maintainer to approve the residue moves and the closure.

**Would change the conclusion:** A slice whose ledger describes work that is absent from the tree, or a maintainer wish to keep Phase 6 inside Task 30.

**Acceptance needed:** The maintainer.
