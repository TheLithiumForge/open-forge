---
open-forge:
  description: Settled historical council synthesis for the accepted read-only Doctor and exact Repair boundary
  responsibility: Preserve independent repair perspectives, safety boundaries, dissent, and the focused council resolution as context
  tags: [Memory, Archived, CLI, Release, Review, Council, Historical, Doctor, Repair, Safety, Mutation, Contextual]
---

# Diagnosis And Repair Council Synthesis

## Status

This is settled historical and contextual council synthesis. It does not replace
the current [`doctor` contracts](../../../crystallized/documents/cli/contracts/doctor/_doctor.md) or [`repair`
contracts](../../../crystallized/documents/cli/contracts/repair/_repair.md), and it does not create a public
schema or implementation authority. The maintainer accepted the separate
read-only Doctor operation and canonical `repair` operation after this evidence
was reconciled. The focused council recommendation for `repair` was adopted;
the dissent below remains useful context rather than an open decision.

- **Origin:** `.agents/memory/working/cli-release/review/check-fix-council.md`.
- **Archived because:** Queue 18 settled and the reconciled outcome moved into
  the current Doctor and Repair Interface and Behavior contracts.
- **Current authority:** the linked current Doctor and Repair contract sets.

## Verified Current Direction

- `doctor` is the accepted name for complete structural validation, findings,
  and recommendations.
- Checking and inspection are read-only. A flag must not turn `doctor` into a
  mutating operation.
- Automatic repair is allowed only when one exact, meaning-preserving local
  correction is proven. Ambiguous suggestions and authored decisions remain
  guided or manual.
- Every write uses a fresh plan, dry-run parity, preflight, expected-state
  revalidation, verification, and recovery. Saved plans and generic apply are
  rejected.
- Existing targeted operations, including `index` and bounded route operations,
  remain the clearest interface when the user already knows the intended change.
- The first-release journey is accepted as the `repair` wizard, automatic
  safe-exact selection, explicit three-value relinks, and one atomic plan. Its
  first catalogue is same-target path, case, encoding, and unique-fragment
  correction plus user-selected missing-target relinks from bounded candidate
  evidence.
- A complete six-domain Doctor diagnosis is a strict write gate for general
  Repair, including explicit relinks. Generated navigation, route intent,
  recovery cleanup, Framework lifecycle, and Extension lifecycle remain outside
  general Repair.

Archived CLI-v2 diagnosis and repair material supports typed findings, direct
repair proposals, conflict detection, dry-run, fresh rediagnosis, and explicit
domains. It remains historical raw evidence.

## Evidence Basis

The council compared four fresh independent perspectives against:

- The accepted command and safety state in the
  [CLI Release Checkpoint](../../../working/checkpoints/cli-release.md).
- Accepted result, dry-run, diagnosis, and mutation decisions in the
  [Decision Agenda](../../../working/cli-release/decision-agenda.md).
- The targeted [Index contract set](../../../crystallized/documents/cli/contracts/index/_index.md),
  [Route Init contract set](../../../crystallized/documents/cli/contracts/route/init/_init.md),
  [Route Create contract set](../../../crystallized/documents/cli/contracts/route/create/_create.md), and
  [Route Update contract set](../../../crystallized/documents/cli/contracts/route/update/_update.md) contracts.
- The historical
  [Diagnosis And Repair Contract](../../cli-v2/documents/contracts/diagnosis-and-repair.md).

The perspective summaries below preserve their material differences. The
reconciliation used no vote. Its useful safety conclusions are reflected in the
current contracts, while this synthesis remains contextual.

## Independent Perspectives

### Workflow Clarity

This view preferred a separate `repair` surface, rejected `doctor --fix` and
generic `fix`, and required explicit finding selection. It proposed distinct
preview and apply operations, but that shape conflicts with the current shared
preference for one write operation plus `--dry-run` and therefore remains only
raw advice.

### Mutation Safety

This view also preferred `repair`, required fresh planning, explicit scope,
whole-batch conflict checks, bounded confirmation, Git and backup policy, and
fresh rediagnosis. It emphasized that `complete` must be qualified to the
selected repair scope rather than global workspace health.

### Contract Architecture

This view separated an immutable per-invocation fact snapshot, explicit diagnosis
domains, typed findings, domain-produced repair proposals, and a thin lifecycle
coordinator. It recommended defining those boundaries now but deferring a public
general repair command until at least two domains demonstrate genuine reuse.

### Radical Simplifier

This view rejected a general repair command for the first release. It kept
repairs in targeted operations and required recurring cross-domain atomic repair
jobs before accepting a general coordinator.

## Shared Ground

- `doctor` remains read-only. No `doctor --fix`.
- Do not expose both `fix` and `repair`. The maintainer accepted `repair` as the
  canonical constrained operation name.
- Finding text, diagnostic strings, codes, plugin registries, and operation order
  never select mutation behavior.
- A repairable finding and its exact typed proposal are produced together from
  current facts.
- A repair invocation reruns diagnosis and planning. It never trusts a rendered
  report or saved plan as current authority.
- Manual, ambiguous, destructive, ownership, and authority choices produce no
  automatic effects.
- Conflicting selected repairs block before the first write. Order does not
  resolve conflicts, and unrelated safe repairs are not silently applied as a
  partial batch.
- Successful application is followed by verification and fresh diagnosis.

## Historical Reconciled Preliminary Direction

The list below preserves the pre-feedback council synthesis. Items 4 and 5 are
superseded by the accepted read-only `doctor` plus the `repair` wizard and
automatic safe-exact selection; the safety boundaries remain relevant.

1. Keep `doctor` read-only and reject `doctor --fix`.
2. Keep known intentional changes in targeted domain operations.
3. Define typed diagnosis and exact repair-proposal boundaries before a general
   mutation coordinator.
4. Defer a public general `repair` operation until cross-domain or repeated
   diagnosis-driven repair demonstrates value beyond targeted commands. The
   maintainer later rejected this deferral by accepting the Queue 18 pack.
5. If accepted later, use one direct `repair` operation with explicit selection
   and `--dry-run`, not separate `plan` and `apply` operations or generic `fix`.

## Historical Material Dissent

Two perspectives would ship `repair` as soon as proof-backed findings and
recovery are available because it gives agents a direct bridge from diagnosis to
safe mutation. The stricter perspectives require demonstrated cross-domain use
first because a general selector, batching, and conflict surface creates broad
mutation authority.

## Focused Command-Name Council

Three later independent naming perspectives evaluated `repair` and `fix` after
the read-only `doctor` and separate Repair journey were accepted in principle:

- The comprehension lens preferred `repair` because it communicates constrained
  restoration of diagnosed conditions rather than a promise to solve every
  finding.
- The CLI-consistency lens preferred the `doctor` to `repair` pair and warned
  that `fix` could become an ambiguous catch-all as the command surface grows.
- The semantic-safety lens also preferred `repair`, while warning that the word
  can still overpromise complete restoration unless help and results state which
  manual or ambiguous findings remain.

The focused council recommends `repair` with this naming contract:

> `repair` applies only diagnosis-backed, conflict-free, exact
> meaning-preserving restorations. It neither authors content nor promises to
> eliminate every finding.

This recommendation supplied naming evidence. The maintainer accepted
`repair`; the current Repair contracts define the exact grammar and boundary.

## Evidence Needs Retained From The Council

- Exercise at least two real diagnosis domains through one typed proposal
  boundary when implementation design and evidence work begin.
- Demonstrate that targeted commands and the general repair coordinator remain
  behaviorally distinct without reopening the accepted first-release journey.
- Fixtures for stale findings, changed expected state, overlapping equivalent
  proposals, conflicting proposals, dirty affected paths, interruption,
  verification failure, rollback, and residual evidence.
- Proof that every diagnosis domain ran or reported explicit incompleteness.
- Proof that post-application diagnosis uses fresh facts and reports repaired,
  remaining, new, manual, and blocked findings without claiming global health
  from a scope-limited repair.

## Review Unit

- [Diagnosis And Repair Boundary](check-fix-boundary.md)
- [Current Doctor Contract Set](../../../crystallized/documents/cli/contracts/doctor/_doctor.md)
- [Current Repair Contract Set](../../../crystallized/documents/cli/contracts/repair/_repair.md)
- [Shared CLI Operation Contract](../../../crystallized/documents/cli/shared-operation-contract.md)
