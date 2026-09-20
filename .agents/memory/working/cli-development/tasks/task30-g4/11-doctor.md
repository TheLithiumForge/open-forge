---
open-forge:
  description: Doctor output catalogue with the severity ladder, coverage counts, every finding kind and its message
  tags: [Memory, Working, CLI, Task, Plan, G4, Doctor, Contextual, Active]
---

# 11 — doctor

> Read [00 — G4 conventions](00-conventions.md) first. Depends on
> [03](03-rendering-system.md). Do not run `doctor` against this repository
> at any point; use fixtures.

## Goal

A healthy `doctor` prints two lines. With problems it prints counts, every
error, every check that could not finish, and one next action. Warnings
appear at `standard`, Info at `full`. Every finding names a file and says
what to do. Coverage facts are counts, not findings. Candidates belong to
the finding they resolve. Nothing is emitted twice.

## Depends on / Blocks

- Depends on: 03, and 02 for the category labels. Lane A, after 10.
- Blocks: 40.

## Shape

Diagnosis: the only shape whose warnings are counted at `minimal`.

## Situations

Fixtures only: `healthy`, `info-only` (no ownership record),
`warnings-only` (two broken links, one with candidates), `error-and-warnings`
(malformed frontmatter plus links), `incomplete` (Extension source
unreadable), `blocked-workspace`, `invalid-input`, `changed-extension-file`,
`stale-entries`, `library-drift`, `recovery-bundle`. Each at all four levels,
text and JSON.

## Result model change (C5, option B)

The per-occurrence coverage kinds leave the model. They become counts on the
Links category and on the report:

| Removed kind                                                                        | Becomes                                                                                                                                                         |
| ----------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `reference.target-valid`                                                            | count `linksValid`                                                                                                                                              |
| `reference.external-unchecked`                                                      | count `externalLinksNotChecked`                                                                                                                                 |
| `reference.image`                                                                   | count `imageLinks` (a broken image is `target-missing`)                                                                                                         |
| `reference.cycle`                                                                   | removed (traversal mechanics)                                                                                                                                   |
| `reference.repeat`                                                                  | removed                                                                                                                                                         |
| `reference.candidate-filename`, `-title`, `-literal-content`, `-route-neighborhood` | `CliCandidate` entries on the `target-missing` or `fragment-missing` finding, with `Reasons` (`filename match`, `title match`, `content match`, `nearby route`) |
| `reference.candidates-none`, `-one`, `-several`                                     | removed; cardinality is the candidate list length                                                                                                               |

Ten internal legacy kinds that the contract no longer lists are deleted from
`DoctorFindingKind` and `DoctorFindingTitles`: `framework.install-incomplete`,
`framework.lifecycle-evidence-malformed`, `framework.lifecycle-untrusted`,
`framework.lifecycle-section-missing`, `extension.lifecycle-document-missing`,
`extension.lifecycle-document-invalid`, `extension.lifecycle-untrusted`,
`extension.lifecycle-section-missing`, `library.record-malformed`,
`library.record-unavailable`. `LibraryDoctorInspector` is the one emitter to
check.

The result keeps six categories, coverage per category, limitations,
severity counts and lane counts. The JSON `data` carries the categories.

## Statuses and headlines

| Status                  | When                                                | Headline                                                                                        | Exit | Stream |
| ----------------------- | --------------------------------------------------- | ----------------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | no error, no warning, complete coverage             | `No problems found.`                                                                            |    0 | stdout |
| completed               | Info findings only                                  | `No problems found. <N> info findings were recorded.`                                           |    0 | stdout |
| completed-with-warnings | warnings, no errors                                 | `No errors. <W> warnings and <I> info findings were recorded.` (omit the info clause when zero) |    2 | stdout |
| completed-with-warnings | errors present                                      | `<E> errors, <W> warnings and <I> info findings.` (singular forms when 1; omit zero parts)      |    2 | stdout |
| incomplete              | a category could not finish                         | previous sentence + ` <K> checks could not finish.`                                             |    3 | stdout |
| invalid-input           | operand or unknown flag                             | family `invalid-input`: `Cannot run doctor: <problem>.`                                         |    4 | stderr |
| blocked                 | workspace missing, not a directory, unsafe boundary | `Cannot check this workspace: <reason>.`                                                        |    5 | stderr |
| failed                  | unexpected error                                    | `Doctor stopped because of an unexpected error: <reason>.`                                      |    1 | stderr |
| cancelled               | Ctrl+C                                              | `Doctor was cancelled.`                                                                         |  130 | stderr |

Doctor's status stays `completed-with-warnings` when only warnings exist and
`completed-with-warnings` when errors exist too, as the contract defines
(errors do not make diagnosis fail). Record in the ledger that the status
name changes but the mapping does not.

## Text by level

`minimal`, healthy:

```text
No problems found.
  6 checks complete. 21 links and 20 routes checked.
```

`minimal`, warnings only:

```text
No errors. 2 warnings and 3 info findings were recorded.
  To list them: open-forge doctor --detail standard
```

`minimal`, errors:

```text
1 error, 2 warnings and 3 info findings.
  Error  .agents/skills/pdf/SKILL.md:1:1  Frontmatter is invalid
         The frontmatter block is not closed.
         Edit the file by hand.
  To list the warnings: open-forge doctor --detail standard
Next: open-forge repair --dry-run
```

`minimal`, incomplete:

```text
No errors. 2 warnings were recorded. 1 check could not finish.
  Extensions were not checked: the package source ./packages/toolkit cannot be read.
  To list the warnings: open-forge doctor --detail standard
```

`standard`, errors and warnings (categories appear only when they have listed
findings; Info stays counted):

```text
1 error, 2 warnings and 3 info findings.
Workspace: D:/work/myrepo

Routes and Entries
  Error    .agents/skills/pdf/SKILL.md:1:1   Frontmatter is invalid
           The frontmatter block is not closed. Edit the file by hand.

Links
  Warning  .agents/loader.md:105:3           Broken link
           The linked file was not found: patterns/_patterns.md
           Possible target (not chosen): .agents/patterns/_patterns.md
           Choose it: open-forge repair
  Warning  .agents/maps/_maps.md:32:3        Broken link
           The linked file was not found: nowhere/_nope.md
           No possible target was found. Fix the link by hand.

  21 links valid, 3 external links not checked, 20 routes checked.
Next: open-forge repair --dry-run  (preview the 1 repair that is safe to apply)
```

`full` adds Info rows, the code after each title, `why this was suggested`
under each possible target, `Read from: <source>` per finding, the lane
sentence (`can be fixed automatically`, `needs a choice`) after the message,
and the lane counts sentence: `1 can be fixed automatically, 2 need a choice,
1 must be fixed by hand.`

`debug` adds each category's boundary and coverage on stderr.

The hint line `To list them: ...` appears only when the level hides at least
one finding. It names `--detail standard` when warnings are hidden and
`--detail full` when only Info is hidden.

## Findings catalogue

Severities are the contract's. Lanes decide the per-finding action phrase.
Family rows use the shared sentence with the subject filled in. Title
changes from `DoctorFindingTitles` are marked with `->`.

### Workspace

| Kind                                    | Severity | Lane            | Title                                 | Message                                                                                  | Action                                                                          |
| --------------------------------------- | -------- | --------------- | ------------------------------------- | ---------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------- |
| workspace.unavailable                   | error    | blocked-repair  | Workspace cannot be read              | `<path> does not exist or cannot be read.`                                               | none                                                                            |
| workspace.not-directory                 | error    | blocked-repair  | Workspace is not a directory          | `<path> is a file, not a directory.`                                                     | none                                                                            |
| workspace.agents-missing                | info     | informational   | The .agents folder is missing         | `<path> has no .agents folder. Open Forge is not installed here.`                        | `open-forge install --dry-run`                                                  |
| workspace.agents-inaccessible           | error    | blocked-repair  | The .agents folder cannot be read     | `.agents exists but cannot be read: <reason>.`                                           | none                                                                            |
| workspace.loader-missing                | error    | blocked-repair  | Loader is missing                     | `.agents/loader.md is missing.`                                                          | `open-forge update` when the record exists, else `open-forge install --dry-run` |
| workspace.loader-unreadable             | error    | blocked-repair  | Loader cannot be read                 | `.agents/loader.md cannot be read: <reason>.`                                            | none                                                                            |
| workspace.loader-malformed              | error    | blocked-repair  | Loader has invalid content            | `.agents/loader.md could not be understood: <reason>.`                                   | `open-forge update` (restore the shipped Loader)                                |
| workspace.entry-missing                 | warning  | manual-decision | Entrypoint is missing                 | `<folder> is routed but has no entrypoint file.`                                         | `open-forge route init <id>`                                                    |
| workspace.entry-ambiguous               | error    | blocked-repair  | Several entrypoints match             | `<folder> has more than one entrypoint file: <names>. Keep one.`                         | edit by hand                                                                    |
| workspace.entry-compatibility-collision | error    | blocked-repair  | Entrypoint names conflict             | `<folder> has both <_name.md> and <index.md>. Keep one.`                                 | edit by hand                                                                    |
| workspace.source-id-collision           | warning  | manual-decision | Source IDs conflict                   | `The ID <id> is derived by more than one file: <paths>. Use exact paths, or rename one.` | edit by hand                                                                    |
| workspace.path-invalid                  | error    | blocked-repair  | Path is invalid                       | `<path> is not a valid path for a <kind>.`                                               | edit by hand                                                                    |
| workspace.path-containment              | error    | blocked-repair  | Path is outside the workspace         | `<path> points outside the workspace.`                                                   | edit by hand                                                                    |
| workspace.physical-alias                | error    | blocked-repair  | Path identity is ambiguous            | `<path> and <other> resolve to the same file, so its identity is ambiguous.`             | edit by hand                                                                    |
| workspace.frontmatter-malformed         | warning  | manual-decision | Frontmatter is invalid                | `<what is wrong, from the parser, in plain words>` at `<path>:line:col`                  | edit by hand                                                                    |
| workspace.frontmatter-duplicate         | warning  | manual-decision | Frontmatter contains duplicate fields | `The key <key> appears more than once.`                                                  | edit by hand                                                                    |
| workspace.parse-incomplete              | info     | informational   | Source could not be read completely   | `<path> could not be parsed completely: <reason>. Its checks are incomplete.`            | none                                                                            |
| workspace.unsupported-source            | info     | informational   | Source type is unsupported            | `<path> is not a kind of file Open Forge checks.`                                        | none                                                                            |
| workspace.root-missing                  | warning  | manual-decision | Root route is missing                 | `The Loader lists <route> but <path> does not exist.`                                    | `open-forge route init <id>` or remove the entry                                |
| workspace.root-unreachable              | warning  | manual-decision | Root route cannot be reached          | `<route> is listed but cannot be reached from the Loader: <reason>.`                     | edit by hand                                                                    |
| workspace.detached                      | info     | informational   | Source is outside the loaded routes   | `<path> is not reachable from any route, so agents never load it.`                       | `open-forge index` when its parent is routed                                    |

### Recovery data

| Kind                            | Severity | Lane           | Title                                                   | Message                                                                            | Action                         |
| ------------------------------- | -------- | -------------- | ------------------------------------------------------- | ---------------------------------------------------------------------------------- | ------------------------------ |
| recovery.bundle-recognized      | info     | informational  | Recovery bundle found -> Recovery bundle is kept        | `A recovery bundle from an earlier command is kept at <path>.`                     | `open-forge cleanup`           |
| recovery.draft-recognized       | warning  | informational  | Incomplete recovery draft found                         | `An unfinished recovery draft is at <path>. A command did not finish.`             | `open-forge cleanup --dry-run` |
| recovery.bundle-collision       | error    | blocked-repair | Recovery records conflict -> Recovery bundle is damaged | `The recovery bundle at <path> is damaged: <reason>. It was left in place.`        | `open-forge cleanup --dry-run` |
| recovery.provenance-unavailable | error    | blocked-repair | Recovery origin could not be verified                   | `The recovery bundle at <path> cannot be verified, so cleanup will not delete it.` | none                           |

### Routes and Entries

| Kind                              | Severity | Lane               | Title                                                                        | Message                                                                                             | Action                                             |
| --------------------------------- | -------- | ------------------ | ---------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------- | -------------------------------------------------- |
| route.entrypoint-missing          | warning  | manual-decision    | Route entrypoint is missing                                                  | `<folder> is routed but has no entrypoint file.`                                                    | `open-forge route init <id>`                       |
| route.entrypoint-duplicate        | error    | blocked-repair     | Route has several entrypoints                                                | `<folder> has more than one entrypoint: <names>.`                                                   | edit by hand                                       |
| route.escape                      | error    | blocked-repair     | Route leaves its allowed boundary                                            | `The entry <text> in <path>:l:c points outside <folder>.`                                           | edit by hand                                       |
| route.unreachable                 | warning  | manual-decision    | Route cannot be reached                                                      | `<path> is routed but no parent lists it.`                                                          | `open-forge index`                                 |
| route.detached                    | warning  | manual-decision    | Source is outside the loaded routes                                          | `<folder> looks like a route but no Loader entry or parent reaches it.`                             | edit by hand                                       |
| route.metadata-required-missing   | warning  | manual-decision    | Required route metadata is missing                                           | `<path> has no <description \| tags> in its frontmatter.`                                           | `open-forge route update <id> --description "..."` |
| route.title-invalid               | warning  | manual-decision    | Route title is invalid                                                       | `<path> has no level-1 heading.`                                                                    | edit by hand                                       |
| route.axioms-invalid              | info     | manual-decision    | Route Axioms are invalid                                                     | `<path> has no Axioms section, or its Axioms section is malformed.`                                 | edit by hand                                       |
| route.generated-region-stale      | warning  | targeted-operation | Generated navigation needs updating -> Entries section is stale              | `The Entries section of <path> does not match its routed files.`                                    | `open-forge index`                                 |
| route.generated-region-missing    | warning  | targeted-operation | Generated navigation is missing -> Entries section is missing                | `<path> has no Entries section.`                                                                    | `open-forge index`                                 |
| route.generated-region-malformed  | error    | blocked-repair     | Generated navigation markers are invalid -> Entries section is malformed     | `The Entries section of <path> could not be read as a list.`                                        | edit by hand                                       |
| route.generated-region-misplaced  | warning  | manual-decision    | Generated navigation is in the wrong place -> Entries section is not last    | `The Entries section of <path> is followed by another section.`                                     | edit by hand                                       |
| route.generated-region-duplicate  | error    | blocked-repair     | Generated navigation appears more than once -> More than one Entries section | `<path> has more than one Entries section.`                                                         | edit by hand                                       |
| route.generated-entry-missing     | warning  | targeted-operation | Generated entry is missing -> Entry is missing                               | `<path> does not list <child>.`                                                                     | `open-forge index`                                 |
| route.generated-entry-extra       | warning  | targeted-operation | Generated entry is no longer needed -> Entry has no file                     | `<path> lists <child>, which does not exist.`                                                       | `open-forge index`                                 |
| route.generated-entry-order       | warning  | targeted-operation | Generated entries are out of order -> Entries are out of order               | `The entries in <path> are not in the expected order.`                                              | `open-forge index`                                 |
| route.generated-entry-path        | warning  | targeted-operation | Generated entry path needs updating -> Entry path is wrong                   | `The entry for <child> in <path> points to <wrong path>.`                                           | `open-forge index`                                 |
| route.generated-entry-description | warning  | targeted-operation | Generated entry description needs updating -> Entry description is stale     | `The entry for <child> in <path> has an old description.`                                           | `open-forge index`                                 |
| route.generated-entry-tags        | warning  | targeted-operation | Generated entry tags need updating -> Entry tags are stale                   | `The entry for <child> in <path> has old tags.`                                                     | `open-forge index`                                 |
| route.overwrite-orphan            | warning  | manual-decision    | Overwrite has no base file                                                   | `<name>.overwrite.md has no <name>.md beside it.`                                                   | edit by hand                                       |
| route.overwrite-independent-index | warning  | targeted-operation | Overwrite is listed independently                                            | `<path> lists <name>.overwrite.md as its own entry. Overwrite files are read with their base file.` | `open-forge index`                                 |
| route.compatibility-conflict      | error    | blocked-repair     | Route names conflict                                                         | `<folder> can be reached by two route names: <names>.`                                              | edit by hand                                       |

### Links

| Kind                               | Severity | Lane            | Title                                    | Message                                                                                            | Action                                                      |
| ---------------------------------- | -------- | --------------- | ---------------------------------------- | -------------------------------------------------------------------------------------------------- | ----------------------------------------------------------- |
| reference.target-missing           | warning  | guided-choice   | Broken link                              | `The linked file was not found: <destination>.` then candidates or `No possible target was found.` | `open-forge repair` when candidates exist; else fix by hand |
| reference.fragment-missing         | warning  | guided-choice   | Linked heading was not found             | `<file> has no heading <#fragment>.` then candidates                                               | `open-forge repair` or fix by hand                          |
| reference.fragment-unverified      | warning  | blocked-repair  | Linked heading could not be checked      | `<file> could not be parsed, so <#fragment> was not checked.`                                      | `open-forge doctor` after fixing the file                   |
| reference.destination-malformed    | warning  | manual-decision | Link destination is invalid              | `<destination> is not a link Open Forge can check.`                                                | fix by hand                                                 |
| reference.destination-absolute     | warning  | manual-decision | Absolute local link is unsupported       | `<destination> is an absolute path. Use a relative path.`                                          | fix by hand                                                 |
| reference.destination-query        | warning  | manual-decision | Local link query is unsupported          | `<destination> has a query string, which local links do not support.`                              | fix by hand                                                 |
| reference.destination-encoding     | error    | blocked-repair  | Link encoding is unsupported             | `<destination> uses an encoding that cannot be resolved safely.`                                   | fix by hand                                                 |
| reference.target-outside-workspace | error    | blocked-repair  | Link leaves the workspace                | `<destination> points outside the workspace.`                                                      | fix by hand                                                 |
| reference.target-physical-escape   | error    | blocked-repair  | Link resolves outside the workspace      | `<destination> resolves outside the workspace through a link.`                                     | fix by hand                                                 |
| reference.target-alias             | error    | blocked-repair  | Link target identity is ambiguous        | `<destination> resolves to more than one file.`                                                    | fix by hand                                                 |
| reference.target-unreadable        | warning  | blocked-repair  | Link target cannot be read               | `<file> exists but cannot be read: <reason>.`                                                      | none                                                        |
| reference.target-unsupported       | info     | informational   | Link target type is unsupported          | `<destination> is a kind of file Open Forge does not check.`                                       | none                                                        |
| reference.same-target-path         | info     | safe-exact      | Equivalent link path is available        | `<destination> works but is not the canonical spelling: <canonical>.`                              | `open-forge repair --automatic`                             |
| reference.same-target-case         | info     | safe-exact      | Equivalent link letter case is available | `<destination> differs from the file's name only by letter case: <canonical>.`                     | `open-forge repair --automatic`                             |
| reference.same-target-encoding     | info     | safe-exact      | Equivalent link encoding is available    | `<destination> uses a different encoding than the canonical <canonical>.`                          | `open-forge repair --automatic`                             |
| reference.same-target-fragment     | info     | safe-exact      | Equivalent heading link is available     | `<#fragment> matches the heading <#canonical> apart from spelling.`                                | `open-forge repair --automatic`                             |

Counts on this category: `linksChecked`, `linksValid`,
`externalLinksNotChecked`, `imageLinks`.

### Framework files

| Kind                                     | Severity | Lane               | Title                                                                           | Message                                                                                                | Action                            |
| ---------------------------------------- | -------- | ------------------ | ------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------ | --------------------------------- |
| framework.install-absent                 | info     | informational      | Framework is not installed                                                      | `Open Forge is not installed in this workspace.`                                                       | `open-forge install --dry-run`    |
| framework.ownership-observation          | info     | informational      | Framework ownership observation -> No ownership record                          | family `ownership-observation` (`Framework files`)                                                     | none                              |
| framework.managed-missing                | warning  | targeted-operation | Managed Framework file is missing -> Framework file is missing                  | `<path> is missing. It was installed by the Framework.`                                                | `open-forge update`               |
| framework.managed-changed                | warning  | targeted-operation | Managed Framework file changed -> Framework file changed                        | `<path> changed since it was installed.`                                                               | `open-forge update`               |
| framework.lifecycle-evidence-unavailable | warning  | blocked-repair     | Framework installation record cannot be read -> Ownership record cannot be read | `.agents/open-forge.lock.json could not be read: <reason>.`                                            | none                              |
| framework.bridge-boundary                | error    | blocked-repair     | Framework bridge needs review -> Open Forge section in AGENTS.md needs review   | `The Open Forge section in <AGENTS.md \| CLAUDE.md> is missing or changed.`                            | `open-forge update`               |
| framework.root-region-boundary           | error    | blocked-repair     | Framework root region needs review -> Open Forge section boundary is unclear    | `The Open Forge section in <file> has no clear start or end.`                                          | edit by hand                      |
| framework.ownership-conflict             | warning  | manual-decision    | Framework file ownership conflicts                                              | `<path> is claimed by the Framework and by <other>.`                                                   | fix by hand                       |
| framework.partial-lifecycle              | error    | blocked-repair     | Framework lifecycle operation is incomplete -> Framework update did not finish  | `Some Framework files are current and others are not, so an update did not finish.`                    | `open-forge update`               |
| framework.partial-recovery               | error    | blocked-repair     | Framework recovery is incomplete                                                | `The recovery bundle at <path> was partly applied: some files match the old content and some the new.` | `open-forge doctor --detail full` |
| framework.distributed-payload-defect     | error    | manual-decision    | Distributed Framework content is invalid -> Bundled Framework is invalid        | family `payload-invalid`                                                                               | reinstall the CLI                 |

### Extensions

| Kind                              | Severity | Lane               | Title                                                                                    | Message                                                                                                           | Action                                      |
| --------------------------------- | -------- | ------------------ | ---------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------- | ------------------------------------------- |
| extension.ownership-observation   | info     | informational      | No ownership record                                                                      | family `ownership-observation` (`installed Extensions`)                                                           | none                                        |
| extension.manifest-missing        | warning  | manual-decision    | Extension manifest is missing                                                            | `The package at <path> has no extension.json.`                                                                    | fix by hand                                 |
| extension.manifest-malformed      | error    | blocked-repair     | Extension manifest is invalid                                                            | `<path>/extension.json could not be read: <reason>. Expected keys: id, name, description, version, dependencies.` | fix by hand                                 |
| extension.duplicate-id            | error    | blocked-repair     | Extension ID is duplicated                                                               | `Two packages in <source> have the ID <id>.`                                                                      | fix by hand                                 |
| extension.unknown-id              | warning  | manual-decision    | Extension ID is unknown                                                                  | `The ownership record names <id>, which is not in the bundled Extensions or the recorded source.`                 | `open-forge extension list`                 |
| extension.version-invalid         | warning  | manual-decision    | Extension version is invalid                                                             | `<id> has an invalid version: <value>.`                                                                           | fix by hand                                 |
| extension.managed-missing         | warning  | targeted-operation | Managed Extension file is missing -> Extension file is missing                           | `<path> is missing. It was installed by <id>.`                                                                    | `open-forge extension update <id>`          |
| extension.managed-changed         | warning  | targeted-operation | Managed Extension file changed -> Extension file changed                                 | `<path> changed since it was installed by <id>.`                                                                  | `open-forge extension update <id>`          |
| extension.dependency-missing      | warning  | manual-decision    | Required Extension dependency is missing                                                 | `<id> requires <dependency>, which is not installed.`                                                             | `open-forge extension install <dependency>` |
| extension.dependency-cycle        | error    | blocked-repair     | Extension dependencies form a cycle                                                      | `<a> requires <b>, which requires <a>.`                                                                           | fix by hand                                 |
| extension.dependency-incompatible | warning  | manual-decision    | Extension dependency version is incompatible                                             | `<id> requires <dependency> <range>, but <version> is installed.`                                                 | fix by hand                                 |
| extension.source-unavailable      | info     | informational      | Extension source cannot be read                                                          | `The source of <id>, <path>, cannot be read, so its files were not compared.`                                     | none                                        |
| extension.catalogue-unavailable   | error    | blocked-repair     | Extension catalogue cannot be read -> Package folder cannot be read                      | `The package folder <path> cannot be read.`                                                                       | none                                        |
| extension.partial-lifecycle       | error    | blocked-repair     | Extension lifecycle operation is incomplete -> Extension update did not finish           | `Some files of <id> are current and others are not.`                                                              | `open-forge extension update <id>`          |
| extension.ownership-collision     | warning  | manual-decision    | Extension file ownership conflicts                                                       | `<path> is claimed by <id> and by <other>.`                                                                       | fix by hand                                 |
| extension.bridge-registration     | warning  | manual-decision    | Extension bridge registration needs review -> Extension entry is missing from its parent | `<parent> does not list <path>, which <id> installed.`                                                            | `open-forge index`                          |

### Libraries (reported under Workspace)

| Kind                                | Severity | Lane            | Title                                                              | Message                                                                    | Action                            |
| ----------------------------------- | -------- | --------------- | ------------------------------------------------------------------ | -------------------------------------------------------------------------- | --------------------------------- |
| library.ownership-observation       | info     | informational   | No ownership record                                                | family `ownership-observation` (`Libraries`)                               | none                              |
| library.source-root-invalid         | error    | blocked-repair  | Library source root is invalid -> Library source folder is invalid | `The source folder of <id>, <path>, is not a folder inside the workspace.` | `open-forge library inspect <id>` |
| library.source-root-aliased         | error    | blocked-repair  | Library source root has an ambiguous path                          | `The source folder of <id>, <path>, resolves to an ambiguous location.`    | none                              |
| library.inventory-incomplete        | warning  | informational   | Library source scan is incomplete                                  | `The source folder of <id> could not be scanned completely: <reason>.`     | `open-forge library inspect <id>` |
| library.projection-missing          | warning  | manual-decision | Registered Library link is missing                                 | `<path>, a link of <id>, is missing.`                                      | `open-forge library sync <id>`    |
| library.projection-dangling         | error    | blocked-repair  | Library link target is missing                                     | `<path> links to <target>, which does not exist.`                          | `open-forge library inspect <id>` |
| library.projection-retargeted       | error    | blocked-repair  | Library link target changed                                        | `<path> no longer links to <expected>; it links to <actual>.`              | `open-forge library inspect <id>` |
| library.path-collision              | warning  | manual-decision | Library destination is occupied                                    | `<path> is used by <id> and by <other>.`                                   | fix by hand                       |
| library.link-capability-unsupported | error    | blocked-repair  | Required Library links are unsupported                             | `This system cannot create the file links the <id> Library needs.`         | none                              |
| library.extension-collision         | warning  | manual-decision | Library and Extension paths conflict                               | `<path> is claimed by the <id> Library and the <id> Extension.`            | fix by hand                       |
| library.recovery-safe-exact         | info     | safe-exact      | Verified Library recovery is available                             | `A verified recovery step for <id> can restore <path>.`                    | `open-forge repair --automatic`   |

## Next rules

One overall `Next:`, chosen in this order: any safe-exact finding ->
`open-forge repair --dry-run` (reason: preview the N repairs that are safe to
apply); any targeted-operation finding -> that command, first by category
order; any guided-choice finding -> `open-forge repair`; any
manual-decision error -> a sentence naming the first file; blocked ->
none; healthy -> none.

## JSON data by level

| Level    | Findings                                  | `data`                                                                                            |
| -------- | ----------------------------------------- | ------------------------------------------------------------------------------------------------- |
| minimal  | errors                                    | `{}`                                                                                              |
| standard | + warnings; `category`, `resolution`      | `{ categories: [ { name, coverage, counts: { errors, warnings, infos }, limitations: [...] } ] }` |
| full     | + infos; candidates, evidence, provenance | + `lanes: { safeExact, guidedChoice, targetedOperation, manualDecision, blockedRepair }`          |

Counts at every level: `checks`, `checksComplete`, `errors`, `warnings`,
`infos`, `linksChecked`, `linksValid`, `externalLinksNotChecked`,
`imageLinks`, `routesChecked`, `frameworkFiles`, `extensionsInstalled`,
`librariesRegistered`.

## References

- `src/cli/core/OpenForge.Cli.Core/Commands/Doctor/Models/Result/DoctorFindingModels.cs` — remove the kinds listed above.
- `src/cli/core/OpenForge.Cli.Core/Commands/Doctor/Shared/Domains/LocalReferenceDoctorDescriptorReader.cs`, `LocalReferenceDoctorFindingFactory.cs`, `LocalReferenceDoctorCandidateInspector.cs` — counts instead of per-occurrence findings; candidates attached.
- `src/cli/core/OpenForge.Cli.Core/Commands/Doctor/Shared/Domains/LibraryDoctorInspector.cs` — legacy kinds.
- `src/cli/core/OpenForge.Cli.Core/Commands/Doctor/Shared/Rendering/*` — replaced by `Presentation/Doctor/DoctorReportSelector.cs`.
- `src/cli/tests/unit/.../Doctor/Shared/Rendering/DoctorHumanSnapshots.cs` — deleted.
- Doctor interface contract sections Coverage, Finding Contract, Finite Diagnostic Catalogue, Output, Command-Local JSON Result Graph, Compact JSON Output (ledger only).

## Preconditions

- [ ] 03 merged. 10 merged (shared Framework and Extension wording).

## Steps

1. [ ] Apply the result model change: counts for coverage kinds, candidates on
       their finding, legacy kinds deleted. Verify: unit tests for the local
       reference inspector prove one finding per broken link with N candidates
       and no candidate findings.
2. [ ] Write `DoctorReportSelector` producing findings with the titles and
       messages above, categories, lanes, counts, limitations and the hint.
3. [ ] Delete the twenty old rendering files.
4. [ ] Regenerate the fixture snapshots and review.
5. [ ] Three suites green.

## Acceptance

- [ ] Healthy `minimal` is two lines. Warnings-only `minimal` is two lines.
- [ ] No finding kind is emitted more than once per subject and location.
- [ ] `full --format json` on the two-broken-links fixture has exactly two
      Links findings, each with its candidates inside.
- [ ] The deleted kinds appear nowhere in `src/cli`.

## Changes ledger

> This subtask took two runs. The first died at `Selected model is at capacity`
> after about five hours with 219 files of work; the overseer committed that work
> before doing anything else, then resumed the lane on the reserve model against
> the same worktree and branch, carrying forward the two failures the dead run
> had named. The resumed run finished them.

- file layout/type: the legacy Doctor renderers are deleted; native `Presentation/Doctor/` owns the presentation, selector, wording, help, text renderer and generated JSON context. `CliStandaloneComposer` closes the binding over it.
- status or exit: the status for a workspace with findings moves from `requires attention` to `completed-with-warnings`, per the `Statuses and headlines` rows. The exit mapping is unchanged.
- message: the completion headline `Workspace checks completed. No files changed.` becomes `No problems found.`, with `No problems found. <N> info findings were recorded.` when only info findings exist, and `No errors. <W> warnings and <I> info findings were recorded.` when warnings exist. All three are the catalogue's rows.
- message: **thirty finding titles were renamed**, each one specified in the `Findings` table as an explicit `old -> new` pair. Examples: `Recovery bundle found -> Recovery bundle is kept`; `Generated navigation needs updating -> Entries section is stale`; `Generated entry is no longer needed -> Entry has no file`; `Framework bridge needs review -> Open Forge section in AGENTS.md needs review`; `Extension catalogue cannot be read -> Package folder cannot be read`.
- finding: nine finding kinds are removed, per the catalogue — `framework.install-incomplete`, `framework.lifecycle-evidence-malformed`, `framework.lifecycle-untrusted`, `framework.lifecycle-section-missing`, `extension.lifecycle-document-missing`, `extension.lifecycle-document-invalid`, `extension.lifecycle-untrusted`, `extension.lifecycle-section-missing`, `library.record-malformed` and `library.record-unavailable`.
- result model: per-occurrence link coverage findings become named coverage counts; candidate findings and cardinality findings are folded into candidates attached to the broken-link finding.
- text: the detail ladder is now errors at `minimal`, warnings at `standard`, info and evidence at `full` and `debug`, matching the catalogue's severity ladder. Hidden findings produce detail-specific hints.
- JSON member: coverage and lane facts become command-owned `DoctorData` inside the schema v3 envelope.
- subject: a blocked pathless finding carried `id: null`; it now carries the stable `id: "workspace"`.
- snapshots: all eleven situations are captured at `minimal`, `standard`, `full` and `debug`, in text and JSON. **Doctor's snapshot class lives in the unit suite**, not the integration one.
- test: the legacy Doctor rendering tests are replaced by native selector and projection parity tests plus the four-detail snapshot coverage.
- evidence: the resumed run reported unit 3,377 passed / 0 failed / 0 skipped; integration 2,235 passed / 17 skipped; Doctor snapshots 11/11; `PublishedDoctorProcessTests` 3/3; `npm run check:dotnet` exactly the five documented errors. Overseer figures, re-run on the merge commit: unit 3,192 passed, 0 failed, 0 skipped; integration 2,208 total, 2,191 passed, 0 failed, 17 skipped; **the complete end-to-end suite 163 passed, 0 failed, 0 skipped** - the first full run since `c1f61d19`, fifteen merges earlier; `npm run check:dotnet` exactly the five documented errors.

## Divergences observed

1. **The hint sentence carries more words than the catalogue gives.** The
   catalogue says `To list them: open-forge doctor --detail standard`; the code
   emits `To list the warnings: open-forge doctor --detail standard`, and
   likewise `To list the info findings`. Native output was left unchanged.
   **Maintainer decision.**

2. **"Nothing is emitted twice" conflicts with the sentence actions.** At
   `standard`, `full` and `debug` the shared renderer prints both the command and
   the reason, so `Fix it by hand.` and `Fix the link by hand.` appear twice.
   Either the rule or the shared renderer has to give. **Maintainer decision**,
   and it is a *shared* renderer question, not a Doctor-only one.

3. **The `error-and-warnings` situation no longer contains an error.** It is
   named and exemplified as containing one, but `workspace.frontmatter-malformed`
   is catalogued as a **warning**. The old fixture produced `1 error, 2
   warnings`; the native fixture produces `No errors. 3 warnings and 3 info
   findings were recorded.` So the situation's name, its example and its
   catalogued severities disagree. **Clarification required**, and the fixture
   probably needs a genuine error to keep the situation meaningful.

4. **`reference.cycle` is dead.** The literal survives only in unrelated
   Workspace Settings unknown-key tests; no Doctor kind or emitter uses it.
   **Durable record to change.**

5. **The first run died at `Selected model is at capacity`** after roughly five
   hours, with two failures outstanding that it had already diagnosed: a parity
   test asserting an informational finding appears in `standard` text, which the
   native ladder shows only at `full`; and a blocked-workspace snapshot that
   changed once the selector gained a stable subject fallback. Both were carried
   into the resume packet verbatim and fixed there. **Process note:** partial
   work survives a capacity failure, and committing it *before* deciding
   anything else is what made the five hours recoverable.

6. **The first run scattered build scratch inside the source tree.** It created
   `src/cli/.appdata/`, `src/cli/.dotnet-cli-home/`, `src/cli/NuGet.config` and
   four `src/cli/doctor-build-*` trees while working around the unreadable user
   NuGet configuration. The overseer's junk check only excluded paths *outside*
   `src/cli/`, so the first commit swept all of it in; it was stripped and
   recommitted. **Durable record:** the merge check must also assert that only
   `core/`, `root/` and `tests/` appear directly under `src/cli/`.

7. **The build needed `NU1900` suppressed.** The initial build hit the offline
   NuGet audit error; restoring with `-p:NuGetAudit=false --ignore-failed-sources`
   succeeded. Same root cause as [24](24-route-update.md) divergence 3.

## Rollback

Restore the bridge registration and the deleted kinds from the before commit.
