---
open-forge:
  description: Streamline hosted build and release delivery while preserving required platform coverage and making stalled suites diagnosable
  tags: [Memory, Working, Task, CLI, Pipeline, Build, Release, Contextual, Active]
---

# Task 71: Streamline build and release delivery

## Outcome

The maintainer requested this follow-up during beta5 publication on 2026-10-02.
Review the Windows-only and other-platform steps, explain which differences
are necessary, reduce avoidable repeated work, and make unusually slow or
stalled jobs visible and recoverable. Recording this task does not begin its
implementation.

## Evidence

Build `36995867259` on commit `634ab07051e670e3f9bcc1b6603ac39a91f4febc`
completed five platform jobs successfully. Its original Linux ARM64 job
`110803462713` exceeded the configured one-hour limit and was cancelled by
GitHub. Native compilation took about 13 minutes. The test step did not
complete after more than 50 minutes. No failed assertion was reported, and
the completed-job log download returned `BlobNotFound`. The exact cause is
unconfirmed. Only this host was retried, with diagnostic logging enabled.
The retry passed all suites and package qualification without source changes.

The prior four successful Linux ARM64 jobs took 16.1 to 18.0 minutes overall
and 4.1 to 4.4 minutes for their test steps. Comparison runs are
`36816172673`, `36813142591`, `36809625271`, and `36656845585`.
The other beta5 hosts completed in 18.9 to 39.6 minutes.

The source currently explains the Windows split: Git Bash can enable backup
and restore privileges, bypassing ACL denials used by filesystem tests.
Windows test and package steps therefore use PowerShell and preserve the
native exit code. This reason must be evaluated before consolidating shells.

`scripts/delivery/test-suites.ts` disables progress and redirects each suite's
output to an execution log. `scripts/delivery/process.ts` waits synchronously
without a per-suite timeout. These choices limit live diagnosis. They do not
establish the cause of the ARM64 stall.

Detailed comparison and recovery receipts are preserved in worktree
`a_6ecaa48917f3`, under `artifacts/root-beta5-qualification/`:
`hosted-duration-comparison.json`, `linux-arm64-slow-job.json`, and
`arm64-recovery.json`.

## Plan

1. Map each required check to the behavior and platform it proves, including
   managed, native, package, site and public upgrade evidence.
2. Compare stage and suite timings across recent successful and failed runs.
   Investigate the ARM64 stall if usable diagnostics become available.
3. Propose simpler shared orchestration with only necessary platform-specific
   behavior. Preserve exit codes, ACL evidence and exact artifact provenance.
4. Evaluate suite deadlines, periodic progress, process diagnostics and
   diagnostic retention before job cancellation.
5. Avoid redundant main/develop builds and reuse successful exact-commit
   artifacts for release. Retry failed hosts without repeating green hosts.
6. Investigate release cancellation. During Release `37003970027`, normal and
   force cancellation marked the run cancelled, yet npm and GitHub publication
   completed. Require clear cancellation boundaries between external effects
   and a documented way to restore public channels after partial publication.
7. Agree the bounded design, implement it, and validate the changed delivery
   behavior proportionately. Do not reduce required coverage by assumption.

## Current state

Open follow-up requested during the beta5 publication investigation; no pipeline redesign has been implemented. The corrected combined beta6 release subsequently passed all six platforms and published under renewed authorization. [Task 70](../../../archived/cli-development/tasks/task70-existing-workspace-adoption-during-installation.md#beta-6-release-complete-2026-10-04) retains that receipt and the historical cancellation race. This successful release does not close Task 71's timing, diagnostics or cancellation improvements. Tasks 34 and 39 remain paused. Task 48 remains required before 1.0.
