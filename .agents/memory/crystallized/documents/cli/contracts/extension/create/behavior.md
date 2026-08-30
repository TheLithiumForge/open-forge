---
open-forge:
  description: Accepted technology-neutral behavior for Extension catalogue scaffold planning, application, and verification
  responsibility: Define create's exact destination resolution, one scaffold plan, workspace no-op, safety, and typed result
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Extension, Create, Behavior, Catalogue, Mutation, Safety, CurrentTruth]
---

# extension create Behavior Contract

## Status And Boundary

This is the accepted current Crystallized Behavior Contract for
`open-forge extension create`. It defines deterministic request resolution,
catalogue-parent and package-destination facts, one scaffold plan, dry-run and
application, create-only safety policy, verification, result formation, and
technology-neutral conformance. It does not define package schema, parser,
storage, or workspace lifecycle authority.

## Typed Flow

```text
validated ID, catalogue parent, and manifest options
  -> deterministic manifest
  -> exact catalogue and destination facts
  -> scaffold intended state
  -> one complete plan and preflight
  -> dry-run or application
  -> verification
  -> one typed result
```

The operation never forms a workspace lifecycle plan. Its `--workspace` value
may be parsed and reported as an accepted global no-op, but it does not affect
the catalogue destination. Create is the accepted no-workspace mutation
exception: the catalogue destination is the sole operation subject, so create
does not acquire the external workspace mutation lock or mutate workspace state.

## Request Resolution

1. Resolve terminal help/version before any catalogue work.
2. Accept zero or one stable-ID operand and zero or one `--path` value. Require
   both after wizard/direct resolution; missing non-interactive semantic input is
   `invalid`.
3. Accept zero or one nonblank `--name`, `--description`, and
   `--package-version` value plus repeated `--dependency` values. Reject repeated
   singleton metadata, preserve accepted override text exactly, and keep package
   version descriptive rather than imposing SemVer or compatibility semantics.
   Reject invalid dependency IDs, the package ID as its own dependency, and
   duplicate dependency IDs. Sort accepted dependencies by ordinal stable ID for
   serialization; do not resolve their availability.
4. Reject source, package-selection, force, prune, and other mutation flags.
5. Collapse repeated `--automatic` and `--dry-run` presence idempotently.
   Repeated stable ID or `--path` is invalid. Unknown options are invalid.
6. Accept `--workspace` as the shared no-op defined by the global contract.

A prompt-capable human request enters the command-local wizard only for required
facts not supplied explicitly. The argumentless form asks for stable ID and
catalogue parent, a partial explicit request asks only for the missing fact, and
a complete explicit request asks none. Blank or invalid input may be explained
and asked again while input remains available. There is no arbitrary attempt
limit or shared retry abstraction. End of input leaves the request `invalid` and
writes nothing; caller cancellation is `interrupted` and writes nothing.
Automatic mode suppresses interaction only when ID and path are already explicit.
No recommendation, current folder, workspace, or source resemblance fills a
missing value.

After the two required inputs resolve, form the manifest deterministically. The
ID is exact. The default name splits the ID at hyphens, uppercases the first
ASCII letter of each segment, and joins with one space. The default description
is `Open Forge Extension package <stable-id>.`; the default version is `0.1.0`;
and the default dependency set is empty. Explicit nonblank metadata overrides
replace only their fields. Optional metadata adds no wizard question; the
resolved defaults and overrides appear in the plan.

## Destination Facts And Plan

Resolve the exact catalogue parent from `--path` and prove its lexical and
physical identity. Any existing safely resolved ordinary directory is eligible,
including an empty directory. Do not require a marker or create the parent.
Ignore and preserve unrelated sibling files and package directories; inspect only
the exact `<catalogue>/<id>/` package destination and retain its exact physical
identity when it exists. Prove that it is contained by the catalogue parent and
that it is either absent or contains the exact intended scaffold. An exact
matching scaffold is a verified no-op. Any divergent, partial, additional,
unknown, or colliding occupant blocks the complete plan. Existing package bytes
are never adopted or overwritten.

The intended scaffold has exactly these effects:

```text
<catalogue>/<id>/extension.json
<catalogue>/<id>/payload/.agents/
```

The plan does not include README, payload source files, dependency closure,
workspace files, generated navigation, or lifecycle-document effects.
It has no hidden source or target workspace.

The manifest contains exactly `id`, `name`, `description`, `version`, and
ordinally sorted `dependencies` in that property order. All are present. Create
validates declaration syntax only and performs no dependency source or closure
lookup.

If the exact scaffold already exists and matches the intended state, return a
verified no-op. Do not create timestamps or synthetic changes.

## Create-Only Safety And Verification

Preflight validates ID, destination, exact catalogue and destination physical
identity, parent containment, existing-state collision, expected state, and
verification. The standalone create path has no Replace or Delete effect, no
workspace lease, and no recovery bundle. One unsafe, ambiguous, or unavailable
selected fact blocks or incompletes the whole plan.

Dry-run and application share the same request, facts, plan, and preflight.
Dry-run writes no directory, scaffold file, recovery bundle, temporary artifact,
or lifecycle state and cannot claim application verification. It forms the same
pre-effect planning status as application but never produces an apply-time
`failed` or `interrupted` result because it performs no effects. A planning or
read failure and caller cancellation before effects retain their own event
meaning.

Application revalidates the exact parent and destination physical identities,
containment, expected state, and complete destination condition immediately
before effects. It creates the complete scaffold through the separate create-only
exact-destination path, performs no Replace or Delete, and verifies both
scaffold results and the complete destination condition. It never overwrites or
adopts existing package bytes and never acquires the workspace lease.

On failure, stop new effects and preserve any concurrent or partial state; do not
restore, reverse, or compensate for an earlier create effect. A create or
verification failure is `failed`; caller cancellation without a stronger
failure is `interrupted`. A later invocation forms a fresh plan. The persistent
workspace lock is deliberately not involved, and no recovery bundle is created.

## Results And Conformance

Form one typed result containing exact catalogue/path, ID, resolved manifest,
mode, intended scaffold, effects or no-op, workspace-lifecycle unchanged fact,
verification, status, and at most one next action. Human and JSON renderers
consume it once.

The source-generated command-local JSON result emits `catalogue`, `destination`,
`id`, `manifest`, `mode`, `intendedEffects`, `appliedEffects`, `verification`,
and `workspaceLifecycleChanged` in that order. The manifest emits `name`,
`description`, `version`, and `dependencies` in that order. The final Boolean is
always `false`. Do not duplicate the shared envelope's command, status,
workspace, or next-action members.
Use the shared seven statuses and streams; `attention` is currently unreachable
for create.

Create never mutates a workspace, package source, lifecycle document, generated
navigation, or Framework file. It does not acquire the external workspace lock.
Conformance must cover zero, one, and all currently missing required human facts, local
blank/invalid correction without an attempt limit, invalid end of input,
interrupted cancellation, direct requests, automatic omission states,
deterministic manifest defaults, singleton metadata
overrides and repetition rejection, every accepted native option-value form,
dependency ordering and duplicate/self/invalid rejection, exact manifest
property order, exact command-local JSON property order and no envelope
duplication, and no dependency availability resolution,
absent destinations, exact-scaffold no-op, divergent, partial, additional,
unknown, and colliding occupants, empty/populated marker-free parents, unrelated
sibling preservation, exact-destination-only inspection, missing-parent refusal,
exact catalogue and destination physical
identity, workspace no-op, dry-run no-effects, the separate create-only path
with no Replace/Delete, no workspace lease, no recovery bundle,
expected-state revalidation immediately before effects, verification, retained
partial state without restoration, repeated no-op, result parity, all statuses,
and no implementation or shipping claim. The shared CLI Architecture defines the exact JSON result schema and exit
mapping. Gate 5 must prove source-generated serialization, fixed Markdig where
used, real `System.IO`, Native AOT, isolated tests, and package journeys.
