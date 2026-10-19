---
open-forge:
  description: Open Task 82 to make every CLI operation finish in seconds on large workspaces, starting with Repair and Doctor diagnosis on this repository
  tags: [Memory, Working, Task, CLI, Performance, Repair, Doctor, Index, Contextual, Active]
---

# Task 82: CLI operations finish in seconds

## Outcome

Every CLI operation finishes in seconds on a large workspace such as this repository, about 9,500 tracked files, 180 entrypoints and 12,000 local references. Results stay exactly as the contracts define them.

The maintainer agreed on 2026-10-15: “our cli operations should take seconds not tens of seconds.”

Status: Task 82 “CLI operations finish in seconds” (phase 4/4): complete. With the archive's outward links ignored through [Task 83](task83-link-scope-and-folder-links.md), Repair takes about three seconds here and Doctor about four, and it ships with Task 83 in beta15.

## Starting point

Task 79's rename evidence made `repair --automatic` time out on this repository. The fix on the same stack brings rename evidence to 6 seconds, but a full `repair --automatic --dry-run` on a complete checkout with one renamed file still takes 63 seconds: whole-workspace diagnosis 31 seconds, including 21.6 seconds scanning 12,093 references, then 15 seconds of proposal and target catalogue work. `index --dry-run` takes about 8 seconds here. Doctor shares Repair's diagnosis and has not been timed on this repository, because Doctor never runs in this checkout. Measure it in a scratch clone.

The archived Memory records hold hundreds of broken links that the repository Markdown gate does not check, so Repair and Doctor have much more to report here than in a typical workspace.

## Approach

1. Measure every command on a complete scratch clone with one manual rename: Repair, Doctor, Index, Status, Context, Find, References and Route list, each with a stage profile.
2. Fix the largest costs first without changing results, starting with the reference inventory and catalogue work in Repair and Doctor.
3. Add a repeatable performance check that counts work rather than timing it, so the gain cannot silently regress.
4. Decide with the maintainer whether the archived broken links should be repaired, excluded from diagnosis or left as they are.

## Progress, 2026-10-15

Two workers measured and sped up the CLI in parallel, one on Repair and Doctor, the other on Index and the read commands. A merge combined their caching into one per-operation read session for path components and link targets, one parsed-document cache and one catalogue reuse path, keeping fresh reads for leaves, links, failures, preflight and revalidation. All 120 output comparisons stayed byte-identical, and work-count tests guard the gains.

Quiet medians on a complete clone of this repository with one manual rename, in seconds:

| Command                  | Before | After merge |
| ------------------------ | -----: | ----------: |
| Status                   |   3.46 |        2.71 |
| Context                  |   3.16 |        2.33 |
| Find, tag CLI            |   4.03 |        3.04 |
| References               |  11.06 |        2.68 |
| Route list               |   3.14 |        2.45 |
| Route inspect            |   3.00 |        2.35 |
| Index dry-run            |   5.35 |        2.75 |
| Index apply              |  13.38 |        5.52 |
| Repair automatic dry-run |  36.76 |        9.99 |
| Doctor                   |  21.72 |        7.10 |

The merge passed 4,987 Unit, 3,312 Integration and 376 EndToEnd tests. A second round targets Repair, Doctor and Index apply under five seconds, and an independent review checks the caches for stale facts.

An independent review of the merged caching found three correctness problems: link resolutions, including missing paths and symlink mappings, were cached, so a symlink retargeted outside the workspace stayed contained; cached reads hid changed headings and file locks and used case-sensitive keys on Windows; and Repair reused the diagnosis catalogue after an interactive prompt. `82548b2d5` fixes all three with failing-first tests inside one operation. The read session now caches only ordinary ancestor directories, file bytes and failures are acquired freshly through one owner, parsing stays reusable per identical content, and Repair reacquires catalogue facts after prompts. YAML reuse, bounded concurrency, skipping Git evidence when nothing is missing and reusing diagnosis observations recover most of the cost.

After the fixes, under variable host load: Status 3.0, Context 3.2, Find 4.0, References 3.1 to 6.5, Route list 4.4, Route inspect 3.2, Index dry-run 3.5, Index apply 8.3, Doctor 6.4 and Repair 12.0 seconds. Repair stays the slowest here because this repository's archived records give it about 776 problems to evaluate. Unit 4,991, Integration 3,319 and EndToEnd 376 pass, and all 120 output comparisons match. A verification review found no remaining stale facts.

Round 3 bounds fresh acquisitions, keeps rename evidence in immutable indices, derives route parents once, spells relative destinations once per folder and target, reuses pure parsing, allocates less, runs four Git processes instead of six and reuses ordinary ancestors only within one fresh validation pass. An independent review found no material problems and confirmed the earlier freshness guarantees and 648 destination spellings against the old formatter.

Another project loaded the host during measurement, so both builds ran interleaved on the same clones, in seconds:

| Command                  | Round 2 | Round 3 |
| ------------------------ | ------: | ------: |
| Status                   |    2.09 |    2.01 |
| Context                  |    4.05 |    3.76 |
| References               |    6.74 |    6.87 |
| Index apply              |    5.59 |    5.88 |
| Repair automatic dry-run |   13.97 |    8.20 |
| Doctor                   |    8.35 |    5.88 |

An earlier pass with less load reached Repair 5.1, Index apply 4.6 and Doctor 3.4 seconds. Unit 4,998, Integration 3,326 and EndToEnd 376 pass, and all 120 output comparisons match.

## Archived links, 2026-10-15

Doctor in a clone of this repository reports 891 link findings, 864 of them in archived records. Across the workspace they are 737 links to missing files, 108 longer paths to the right file, which Repair rewrites automatically, 22 folder links it cannot check, 12 missing headings, 8 renamed targets and 4 absolute links. The folder links are recorded in the [beta follow-up backlog](beta-follow-up-backlog.md). On 2026-10-16 the maintainer chose to stop checking links written inside the archive while still checking links into it. [Task 83](task83-link-scope-and-folder-links.md) adds that setting.

## Final measurement, 2026-10-16

With `.agents/memory/archived` in this repository's `ignoreLinksFrom`, Doctor ignores 6,406 archived links and checks 5,755 valid ones. On the Task 83 build, `repair --automatic --dry-run` took 2.8 to 3.0 seconds here, `index --dry-run` 3.4 to 5.3 and Doctor in a complete clone 3.1 to 7.2, typically about 4, while another project loaded the host. Two safe same-target repairs outside the archive were applied, and the three remaining findings point to Extension files this source repository does not install.
