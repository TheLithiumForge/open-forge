---
open-forge:
  description: "Historical record: Let each workspace choose root or scoped Open Forge frontmatter, released in beta9"
  tags: [Memory, Task, CLI, Framework, Frontmatter, Metadata, Install, Contextual, Complete, Archived, Historical]
---

# Task 75: Per-workspace frontmatter form

## Outcome

Every workspace reads Open Forge metadata at the YAML frontmatter root and under `open-forge:`. Each workspace chooses which form Open Forge writes. Install asks for the form on first setup, `--frontmatter root|scoped` selects it without asking, and `install --configure` changes it and converts the owned delivered files in the same operation.

The maintainer selected this direction on 2026-10-08 after the [frontmatter research](../../../emerging/analysis/frontmatter-root-keys-and-okf.md#choose-the-form-per-workspace) and asked the root Overseer to plan and orchestrate the implementation.

Status: Task 75 “Per-workspace frontmatter form” (phase 2/2): milestone 4/4 — complete and released in beta9 on 2026-10-09.

## Accepted decisions

The maintainer accepted D1 to D8. The Overseer resolved C1 to C12 from five independent architecture packets. Reversible defaults are marked.

- **D1 Setting.** `.agents/open-forge.json` gains `"frontmatter": "root" | "scoped"`. A missing key means scoped. `schemaVersion` stays 1.
- **D2 Reading.** Every workspace reads both forms. An `open-forge` mapping is the complete Open Forge `description`, `responsibility`, and `tags`, and root keys beside it belong to another tool. Without that mapping, root keys are Open Forge metadata for an admitted routed source. `applyTo` keeps its accepted dual-location rule. Root keys never make a file a source. Native `SKILL.md` keeps its own parser.
- **D3 Validation.** Both forms use one value grammar and one set of diagnostics. Unknown root keys are preserved and ignored.
- **D4 Output.** The setting selects what Install, Update, and Extension lifecycle deliver, what route creation, route initialization, and adoption write, and therefore how installed Templates look. Repository payload sources stay scoped.
- **D5 Editing.** `route update` edits metadata where it was authored. A routed file without Open Forge metadata gets the workspace's form.
- **D6 Identity.** Managed comparisons use the payload rendered in the workspace's form.
- **D7 Choosing.** First interactive Install asks after the preset, with root preselected. `--frontmatter` skips the question. A fresh unattended Install without the flag uses root (reversible default). `install --configure` changes the form and converts owned delivered files in that operation. Repeated Install and Update never ask.
- **D8 Prose.** Contracts, loader, CLI Skill references, Templates guidance, and public docs describe both forms.
- **C1 Explicit scope key.** Any explicit `open-forge` key reserves the scoped location, whatever its value. A null, sequence, or alias value keeps today's Missing or Malformed result and never falls back to root.
- **C2 One form value.** `OpenForge.Cli.Core.Framework.Documents.Metadata.Models.FrontmatterForm { Scoped, Root }` is the only form type. Settings, writers, renderers, and commands use it.
- **C3 Explicit writes.** Emitters and writers take the form as a required argument. No default parameter hides a forgotten workspace form.
- **C4 One transform direction.** Delivery renders canonical scoped payload to root by moving only the leading `open-forge` mapping. Scoped rendering returns the canonical bytes unchanged. A collision with foreign root `description`, `responsibility`, or `tags`, or a conflicting root `applyTo`, is invalid. An equivalent root `applyTo` is kept and the moved copy is dropped. Eligible documents are `.md` files under `.agents/`, except `SKILL.md`, whose leading frontmatter has an `open-forge` mapping.
- **C5 Configure conversion.** For each owned eligible delivered target, Configure compares the current authored identity with the payload rendered in the selected form and in the other form. A match with the selected form needs no effect. A match with the other form becomes one whole-file replacement in the selected form with projected `Entries`. Any other target, including edited files and Extension targets whose source is unavailable, is kept unchanged and reported. Excluded, user-authored, Library, and overwrite files are never touched. The settings write and conversions form one reviewed plan with ordinary recovery.
- **C6 Form-only configuration.** `install --configure --frontmatter <form>` is valid without a preset and leaves route choices unchanged.
- **C7 Persistence.** Every fresh Install writes the key. A fresh Install keeps an explicit preference already authored in the settings file. An explicit `--frontmatter` on an installed workspace requires `--configure`, matching `--preset`.
- **C8 Route Update creation.** Creating metadata needs a complete intended description and tags. Route Update reads settings only for creation, and plan revalidation compares that settings observation.
- **C9 Health reports.** Status and Doctor compare rendered identity. An unedited owned file in the other form reports the existing changed-target findings with Update advice.
- **C10 Result shape.** Install JSON exposes `data.frontmatter` with `form` and, when it changed, `previousForm`.
- **C11 Wording.** Question: `How should Open Forge write file metadata?` Choices: `Root keys` with `description: and tags: at the top of the frontmatter`, and `Scoped under open-forge:` with `open-forge: holds description: and tags:`. Plan line: `Frontmatter: root`, or `Frontmatter: scoped -> root` when it changes.
- **C13 Help and kept files.** Option help: `Choose where Open Forge writes file metadata: root or scoped. A fresh unattended Install uses root. Change an installed workspace with --configure.` JSON `data.frontmatter` always has `form`, has `previousForm` only when the form changed, and has `kept` (path and reason `edited` or `source-unavailable`) only when files were kept. Text shows `Frontmatter: <form>` or `Frontmatter: <previous> -> <form>`, then `Kept <n> files in their previous form.` when files were kept, with the paths and reasons listed at `standard` detail and above. Kept files add no finding code.
- **C14 Resolution.** Install resolves one `InstallFrontmatterSelection` (form, previous form, persist, convert) as follows. The frozen model lives in `Commands/Install/Models/Configuration/InstallFrontmatterSelection.cs`, and the result model in `Commands/Install/Models/Result/InstallFrontmatter.cs`.

  | Workspace | Request | Form | Persist | Convert |
  |---|---|---|---|---|
  | Fresh | `--frontmatter X` | X | yes | no |
  | Fresh | settings already declare X | X | yes | no |
  | Fresh | interactive, no flag or declaration | asked, root first | yes | no |
  | Fresh | unattended, no flag or declaration | root | yes | no |
  | Installed | no `--configure`, no flag | effective form | no | no |
  | Installed | no `--configure`, flag | invalid input: requires `--configure` | | |
  | Installed | `--configure --frontmatter X` | X | when the declared value differs from X | yes |
  | Installed | interactive `--configure` | asked, current first | when the declared value differs | yes |
  | Installed | noninteractive `--configure --preset`, no flag | effective form | no | yes |
  | Installed | noninteractive `--configure`, no preset or flag | existing invalid input | | |

  A missing key never equals an explicit value, so persisting writes it even when the result is `scoped`. The previous form is the effective form of an installed workspace and absent for a fresh Install.
- **C12 Non-goals.** No OKF `type`, no per-file version marker, no conversion of user-authored files, no change to the defined tags or their loading meaning, no new dependency, and no exceptional machinery.

## Architecture

The [CLI Architecture](../../../crystallized/documents/cli/architecture.md) layers stay unchanged.

| Capability | Owner | Location |
|---|---|---|
| Dual reading, authored-location facts, form-aware emission | Framework Documents | `Framework/Documents/Metadata/` |
| Scoped-to-root document transform | Framework Documents | `Framework/Documents/Metadata/Shared/Transformation/` |
| Delivery rendering seam for Framework and Extension payload | Framework Distribution | `Framework/Distribution/Shared/Content/WorkspacePayloadRenderer.cs` |
| Setting, schema, settings edits | Framework Settings | `Framework/Settings/`, `schemas/v1/open-forge.schema.json` |
| Option, question, plan, conversion, result | Install command | `Commands/Install/` with Rendering and OutputText |
| Rendered delivery and comparison | Update, Extension, Status, Doctor owners | Their existing planners and operational readers |
| New metadata in the workspace form | Route Create, Route Init, adoption | Their existing writers |
| Authored-location editing and creation | Route Update | `Commands/Route/Update/Shared/Planning/Metadata/` |

## Execution capsule

- **Profile.** Assured: a new public option and setting, a shared metadata contract, and changed lifecycle comparisons in a public beta. Contracts freeze before dependent slices. One independent review of the integrated branch, then one grouped correction pass.
- **Applicability check.** Local developer tool. Effects reach workspace Markdown and settings through Install, Update, Extension lifecycle, and Route commands. Recovery comes from the existing recovery bundles and Git. Threat boundary: cooperating Open Forge processes, malformed input, and interruption. YamlDotNet node spans and the existing span editors are sufficient, so no new parser or dependency is needed. Shared foundations reused: metadata parser, emitter, settings codec, mutation, recovery. Exceptional machinery: none.
- **Evidence.** Unit for parsing, transforming, settings, and binding. Integration for lifecycle planning, application, identity, and snapshots. EndToEnd for published Install, Configure, and Route Update journeys. The shared parser, serializer, and composition changes trigger the complete managed and Native AOT gate at the Wave 2 integration and at closeout.
- **Workers.** Worker Watch with `gpt-6.1-sol` at `xhigh`, as the maintainer directed. Each implementation slice runs in its own worktree from the committed integration tip and returns an uncommitted diff. The root Overseer integrates, runs gates that need per-user stores, and commits each accepted wave locally.
- **Stop conditions.** A slice stops and reports when it needs a new public behavior, dependency, shared contract, or a change to a protected path, or when an accepted decision above cannot be implemented as written.

## Waves

| Wave | Slice | Outcome | Milestone |
|---|---|---|---|
| 1 | F1 Metadata core | Dual reading, location facts, form-aware emitter and writer, scoped-only adapters for native and Loader inspection | M3 |
| 1 | F2 Transform and renderer | Scoped-to-root document transform, delivery renderer, shipped-payload rendering parity | M4 |
| 1 | F3 Settings | `frontmatter` key, schema, strict decoding, settings edits | M5 |
| 1 | P1 Contracts | Framework and CLI contract amendments and the Decision record | M2 |
| 2 | Install, Update and Extension delivery, health identity, route writers, Route Update | Consumers apply the form | M6 to M10 |
| 3 | Payload prose, public docs, snapshot review, gates, review, vibe check | Closeout | M11 to M13 |

Wave 2 packets are written after Wave 1 integrates, against the actual integrated API.


## Review and corrections

Wave 2 integration needed a fixture wave. Every fresh Install now writes the settings file, and an unattended fresh Install delivers root form, so about 115 existing Integration tests assumed the old behavior. Four fixture slices kept each test's subject: tests not about the form install explicitly scoped, and settings, cleanup, effect, and snapshot expectations include the settings file. The fixture wave exposed two product defects, F-1 and P-1.

Two independent reviews ran on `cdf06bd84` against `70c5b68dd`: RA for code correctness and safety, RB for contract, documentation, and prose consistency. Findings and dispositions:

| ID | Severity | Finding | Disposition |
|---|---|---|---|
| F-1 | major | Install adoption omits the references catalogue `entries` ownership receipt | Accepted, correction CX1 |
| P-1 | major | Route Create breaks a cross-command presentation invariant | Accepted, correction CX2 |
| RA-1 | major | A kept child whose description cannot be projected into `Entries` blocks Configure | Rejected for this Task: the same description already blocks ordinary `index`, and Configure reports it through the existing bounded finding. Recorded as a pre-existing navigation edge |
| RA-2 | major | A composition test rejected the new JSON member | Fixed by the fixture wave |
| RA-3, RB-1 | minor | Default-detail JSON omitted `kept` | Accepted, correction CX1. The Install contract and `docs/cli.md` were corrected |
| RA-4 | minor | Route Init changed an existing settings-failure boundary | Accepted, correction CX2 |
| RA-5 | minor | Adoption duplicated metadata emission | Accepted, correction CX2 |
| RB-2 | minor | `docs/cli.md` claimed preset-only Configure writes the key | Accepted, corrected by the Overseer |
| RB-3 | minor | Install help claimed only the embedded payload is used | Accepted, correction CX1 |
| V1 | minor | Dry-run previews omitted the frontmatter result | Found in the manual check, fixed by the Overseer |
| V2 | minor | The kept line read `Kept 1 files` | Found in the manual check, fixed by the Overseer |


## Closeout

All milestones are complete on `feature/task75-workspace-frontmatter-form`. Nothing has been pushed or integrated into `develop`.

Final local gate, Windows x64, `npm run build:native -- --offline` followed by `npm run test:built` on the final tree: Native AOT build passed with no trimming or AOT warnings. Unit 4,375 passed. Integration 2,946 passed on the managed target and 2,946 on the Native AOT target, each with 17 declared platform exclusions. EndToEnd 306 passed in each of its three modes: managed, the Native AOT test executable, and managed tests against the native CLI. There were no failures or unexpected skips. `npm run check:docs` passed 13 of 13. Generated `Entries` are current.

The Overseer checked the delivered behavior by hand with the development and native executables: fresh unattended Install delivers root and writes the key, `--frontmatter scoped` delivers scoped, Configure converts unedited owned files in both directions and keeps an edited file with reason `edited`, a repeat is a no-op, Status is current after each conversion, `--frontmatter` without `--configure` is rejected, and Route Create and Route Update write and edit root metadata in place.

Not verified locally: the Docusaurus site build. With borrowed dependencies, `develop` itself fails the same 36 page renders, so that local build is not evidence either way. Unix-only permission cases remain declared platform exclusions on Windows and need the hosted Unix runners.

The [fixture ripple Observation](../../../emerging/observations/2026-10-08_install-default-fixture-ripple.md) records why a fresh-install change needed two extra fixture waves. Remaining follow-up candidates: a shared test capability for the fresh-install file set, and the RA-1 navigation edge where a multiline description cannot be projected into `Entries`.


## Follow-up: user flows and root-form journey coverage

On 2026-10-08 the maintainer asked for user scenarios and flows that cover the frontmatter form, end-to-end tests for them, and proof that the journeys still work when a workspace chooses root. This reopens Task 75 with a new horizon: phase 1/2 authors the scenarios and flows, phase 2/2 implements and gates the tests. Milestones: M1 scenarios and flows, M2 F27 and F28 journey tests, M3 root-form coverage of the existing journeys, M4 integrated six-mode gate.

Before this follow-up, unattended installs already made most journeys run in root form, but F01, F11, F12, F13, and F15 install explicitly scoped because they compare delivered bytes with the scoped canonical payload. No flow chose or switched the form.

### New command scenarios

| ID | Situation |
|---|---|
| C03-13 | Fresh unattended Install defaults to root and records `"frontmatter": "root"` |
| C03-14 | Fresh Install with `--frontmatter scoped` delivers canonical scoped bytes and records the choice |
| C03-15 | First interactive Install asks for the form after the preset, with root first |
| C03-16 | `--frontmatter` on an installed workspace without `--configure` is invalid input with no writes |
| C03-17 | Configure preview shows the form change, the replacements, and kept edited files, with no writes |
| C03-18 | Configure converts unedited owned files, keeps edited and user files, writes the key, and repeats as a no-op |
| C03-19 | A preference already declared in the settings file is honored by the first Install |
| C04-12 | Update delivers owned files in the workspace form, including a file left in the previous form |
| C01-13 | Status reports an owned file left in the previous form as changed, with Update advice |
| C02-12 | Doctor on a root workspace finds no managed changes |
| C05-12 | Index projects Entries from root metadata, and from an `open-forge` block beside another tool's root keys |
| C08-15 | Context reads a root-form source |
| C09-13 | Find by tag matches root tags and ignores another tool's root tags beside an `open-forge` block |
| C13-12 | Route Init in a root workspace scaffolds root metadata |
| C14-15 | Route Create in a root workspace writes root metadata |
| C15-14 | Route Update edits root metadata in place and preserves another tool's root keys |
| C15-15 | Route Update creates metadata in the workspace form for a routed file without metadata |
| C21-19 | Extension Install in a root workspace delivers root-form files and repeats as a no-op |
| C22-16 | Extension Update in a root workspace compares rendered identity and repeats as a no-op |
| X31 | Every delivered Markdown file equals the canonical payload rendered in the workspace form, checked independently |

### F27: Start a workspace in root form and keep working in it

Starting point W0: an authored README and no installation.

| Step | Action | Scenarios |
|---:|---|---|
| 1 | `open-forge install --dry-run` | C03-02, C03-13 |
| 2 | `open-forge install --automatic` | C03-13 |
| 3 | Check every delivered file against the canonical payload rendered to root | X31 |
| 4 | `open-forge status` | C01-02 |
| 5 | `open-forge route create .agents/guidance/review-checklist.md --description "Review checklist for pull requests" --tag Guidance --tag Review` | C14-15 |
| 6 | Add another tool's root key, `sidebar_position: 3`, to that file by hand | C15-14 |
| 7 | `open-forge find --tag Review` | C09-13 |
| 8 | `open-forge context guidance/review-checklist` | C08-15 |
| 9 | `open-forge route update guidance/review-checklist --responsibility "Define the pull request review checklist" --tag Checklist` | C15-14 |
| 10 | `open-forge extension install planning --automatic` | C21-19 |
| 11 | `open-forge doctor` | C02-12 |
| 12 | `open-forge install --automatic`, then `open-forge index` | C03-03, C05-12 |

Alternatives: the interactive first install (C03-15), an explicit scoped install (C03-14), a preference declared before the first install (C03-19), and a file shared with another tool that keeps its own root keys beside an `open-forge` block (C05-12, C09-13).

### F28: Switch an existing workspace's metadata form without losing edits

Starting point: a workspace installed in scoped form by an earlier release. Simulate it with `install --frontmatter scoped --automatic` followed by removing the `frontmatter` property. The Collaboration Extension is installed, the user has created a scoped note with `route create`, and has appended a paragraph to the owned `.agents/patterns/_patterns.md`.

| Step | Action | Scenarios |
|---:|---|---|
| 1 | `open-forge status` | C01-13 |
| 2 | `open-forge install --frontmatter root --automatic` | C03-16 |
| 3 | `open-forge install --configure --frontmatter root --dry-run` | C03-17 |
| 4 | `open-forge install --configure --frontmatter root --automatic` | C03-18 |
| 5 | Check converted Framework and Extension files against the payload rendered to root, and that the edited file and the user note are byte-identical | X31, C03-18 |
| 6 | `open-forge index` | C05-12 |
| 7 | `open-forge status` | C01-13 |
| 8 | `open-forge update --automatic` | C04-12 |
| 9 | `open-forge install --configure --frontmatter scoped --automatic`, then check canonical scoped bytes are restored exactly | C03-18, X31 |
| 10 | Repeat step 9 | C03-18 |

Alternative: the setting is edited by hand, and `update` converts the owned files (C04-12).

### Test plan

- The independent oracle `JourneyFrontmatter.RenderCanonical` in the EndToEnd shared journey support renders canonical scoped payload to root. `JourneyFrontmatter.InstallArguments` adds `--frontmatter`.
- F27 and F28 are new published-process journey classes with `Journey` traits and scenario IDs in their display names.
- F01, F11, F12, F13, and F15 run in both forms. Their byte expectations use the oracle in root form.
- Every other journey that installs passes `--frontmatter root` explicitly, so a future default change cannot silently move them. F27 alone relies on the default, to prove it.

### Follow-up result

- **Scenarios and flows.** C01-13, C02-12, C03-13 to C03-19, C04-12, C05-12, C08-15, C09-13, C13-12, C14-15, C15-14, C15-15, C21-19, C22-16, and X31 were added to their collections. The [F27](../../../crystallized/documents/cli/experience/flows/f27-start-a-workspace-in-root-form-and-keep-working-in-it.md) and [F28](../../../crystallized/documents/cli/experience/flows/f28-switch-an-existing-workspace-metadata-form-without-losing-edits.md) flows were added, and the flow review and assessment now count 28 flows.
- **New journeys.** `F27RootFormJourneyTests` and `F28SwitchFrontmatterFormJourneyTests` carry real state through every step. `JourneyPayloadAssertions` compares every delivered file with the repository payload rendered by the independent oracle. Repeat installs on an installed workspace use no `--frontmatter`, because C14 requires `--configure` there.
- **Existing journeys.** F01, F11, F12, F13, and F15 now run every installing test in both forms, with root expectations from the oracle. Every other journey installs with an explicit `--frontmatter root`. Journey cases grew from 84 to 105. No journey needed a changed expectation in root form, so no product difference between the forms was found.
- **Gate.** `npm run build:native -- --offline` then `npm run test:built`, Windows x64: Unit 4,375 passed. Integration 2,946 passed on each target with 17 declared platform exclusions. EndToEnd 329 passed in each of its three modes. `npm run check:docs` passed 13 of 13, and generated `Entries` are current. One unchanged terminal viewport test timed out once during a worker's first full run and passed on rerun and in every later run.


## Beta 9 release, 2026-10-09

The maintainer authorized the squash merge, a beta release, and a documentation release on 2026-10-09.

- The feature branch was squashed into `develop` as `dffa067d0`, followed by the version change to `0.9.0-beta.9` in `a541a0cc8`.
- The first hosted [Build 37853783984](https://github.com/TheLithiumForge/open-forge/actions/runs/37853783984) passed both Windows targets and failed the test step on all four Unix runners. A local Linux reproduction from a clone of the commit found two test-portability defects and no product defect. `InterruptedConversionRetainsCompleteRecovery` forced a replacement failure with Windows file sharing and now skips elsewhere with the declared reason (`248f4a067`). `PublishedInstallFrontmatterProcessTests` used an undeclared Windows-only skip reason, now declared with delivery tests (`34c818fa5`). After the fixes, the Linux Integration tier passed 2,933 with 30 declared skips.
- [Build 37859981299](https://github.com/TheLithiumForge/open-forge/actions/runs/37859981299) qualified `34c818fa5` on all six platforms.
- Tag `v0.9.0-beta.9` at `34c818fa5` started [Release 37864096247](https://github.com/TheLithiumForge/open-forge/actions/runs/37864096247), which published all seven npm packages, with `latest` and `beta` pointing at `0.9.0-beta.9`, and the GitHub prerelease with six portable archives and `SHA256SUMS`.
- A public smoke test installed the exact npm version in a fresh directory: `--version` reported `0.9.0-beta.9`, an unattended install delivered root form and recorded it, `install --configure --frontmatter scoped --automatic` reported `previousForm` root and converted the files, Status was current, and Install help listed `--frontmatter`.
- The documentation site deploys from `main`, which is fast-forwarded to `develop` after this record.

## Current state

Complete. Released in beta9. Follow-up candidates: a shared test capability for the fresh-install file set, and the RA-1 navigation edge where a multiline description cannot be projected into `Entries`.
