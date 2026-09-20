---
open-forge:
  description: Define shared path admission, explicit approval and recovery-preserving settings publication
  tags: [Memory, Crystallized, CLI, Contract, Shared, Permission, Behavior, CurrentTruth]
---

# Workspace Permissions Behavior

The [Interface](interface.md) owns syntax and result coordinates. Authorization
uses the same authored settings reader as the rest of `.agents/open-forge.json`.
There is no independent grant parser, persisted subject graph or legacy reader.

## Admission

Determine actual destination paths from the consuming command's checked plan.
Implicit `.agents/` paths need no external grant. Apply existing portable path,
reserved-control, source, ownership and physical-containment checks independently.
An allow-list entry can never authorize overwriting the settings or ownership
controls, deleting a region's host file, or writing into a Library source.

Read settings once for the permission observation. Evaluate every distinct
external destination against the shared allow list. A grant from any previous
Extension or Library operation is reusable by every other owner. Missing,
malformed or unavailable settings withhold external admission. Commands that need
no external destination proceed independently of those settings.

The observed raw settings bytes and file expectation accompany the typed settings.
Before content effects, reread settings under the normal workspace lease and
compare that observation. A grant revoked or changed after planning prevents
application. Revalidation never substitutes a snapshot of the retired grant file.
Library recovery uses current settings and never restores or widens admission.

## Explicit Authored Edits

`--allow-path` uses the existing explicit settings authoring path. It is not an
implicit lifecycle rewrite and is not granted by `--automatic`. Author it before
the command relies on destination admission; never write it during dry-run.
Refuse malformed, unavailable or nonordinary settings and preserve their bytes.
Use the settings reader's ordinary-parent/no-follow boundary and existing JSON
object-model editing. Preserve unrelated keys and order. An atomic same-directory
replacement uses a newly created temporary file; it never overwrites or deletes
an unrelated fixed-name temporary file.

These explicit authored edits remain distinct from the interactive mutation plan.
They persist even if later content work cannot proceed. Their current settings
observation then participates in normal permission revalidation. This does not
add a second generated state record or a separate grant schema.

## Interactive Approval

Prompt only for a human apply request when the existing interactive session
permits it and some destinations lack admission. Show those destinations and
effects, plus Library file/directory scope where relevant. State that remembered
grants apply to all Extensions and Libraries. Offer always, once and cancel;
empty or unrecognised input cancels. Cancellation from the caller remains the
existing interrupted outcome.

Always plans an authored settings create/replace preserving unrelated keys.
Invalid or unsafe settings cannot be repaired implicitly by that approval.
Once admits only the already checked operation, with no settings change. Cancel
and an unattended missing grant produce the existing permission finding and no
content effects. A dry-run never prompts or writes. Approval never overrides a
separate ownership, source, reserved-path, ancestry or collision check.

## Interactive Publication And Recovery

Acquire the normal workspace lease, revalidate the settings observation and all
other concrete command preconditions, and prepare the existing recovery bundle
before publishing an interactive grant or changing content. Recovery contains
exact prior settings bytes or proven absence. Apply and verify the settings
create/replace, then run the consuming command's content effects and its single
best-effort ownership publication in their existing order.

Only a verified file receipt establishes a saved interactive grant. Later
content failure does not erase that receipt or roll back settings. Preserve the
bundle and its exact prior content. Reuse ordinary reversible file recovery;
there is no permission-specific recovery schema or automatic restoration that
could widen current grants. Existing recovery source/ownership checks remain.

A known unavailable grant file is never replaced by an empty synthetic grant
record. The only persisted authority is the authored shared list.
